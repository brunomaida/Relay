using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using FluentAssertions;
using Relay.Buffers;
using Xunit;

namespace Relay.Tests;

/// <summary>Unit tests for the internal <see cref="SpscByteRingBuffer"/>.</summary>
public sealed class SpscByteRingBufferTests
{
    // ── constructor validation ────────────────────────────────────────────────

    [Theory]
    [InlineData(15)]
    [InlineData(17)]
    [InlineData(100)]
    public void Constructor_NonPowerOfTwo_Throws(int capacity)
    {
        var act = () => new SpscByteRingBuffer(capacity);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void Constructor_SmallerThanMinimum_Throws(int capacity)
    {
        var act = () => new SpscByteRingBuffer(capacity);
        act.Should().Throw<ArgumentException>();
    }

    // ── basic round-trip ──────────────────────────────────────────────────────

    [Fact]
    public void TryPublish_SinglePayload_RoundTripSucceeds()
    {
        var ring    = new SpscByteRingBuffer(64);
        var payload = new byte[20];
        for (int i = 0; i < payload.Length; i++) payload[i] = (byte)(i + 1);

        ring.TryPublish(payload.AsSpan()).Should().BeTrue();

        ring.TryPeek(out var read, out int advance).Should().BeTrue();
        read.ToArray().Should().Equal(payload);

        // recordSize = 4 + ((20+3)&~3) = 4 + 20 = 24
        advance.Should().Be(24);
        ring.Advance(24);
        ring.IsEmpty.Should().BeTrue();
    }

    // ── capacity boundary ────────────────────────────────────────────────────

    [Fact]
    public void TryPublish_PayloadLargerThanCapacity_ReturnsFalse()
    {
        // capacity=16: recordSize for 13-byte payload = 4 + ((13+3)&~3) = 4+16 = 20 > 16.
        var ring = new SpscByteRingBuffer(16);
        ring.TryPublish(new byte[13].AsSpan()).Should().BeFalse();
    }

    [Fact]
    public void TryPublish_FillExactCapacity_Succeeds_ThenNextReturnsFalse()
    {
        // capacity=32: smallest record (1-byte payload) = 4+4 = 8 bytes → 4 records fit exactly.
        var ring   = new SpscByteRingBuffer(32);
        var record = new byte[1] { 0xAB };

        ring.TryPublish(record.AsSpan()).Should().BeTrue();
        ring.TryPublish(record.AsSpan()).Should().BeTrue();
        ring.TryPublish(record.AsSpan()).Should().BeTrue();
        ring.TryPublish(record.AsSpan()).Should().BeTrue();

        // Ring is now full — one more must fail.
        ring.TryPublish(record.AsSpan()).Should().BeFalse();
    }

    // ── wrap-around / padding marker ─────────────────────────────────────────

    [Fact]
    public void TryPublish_WrapAround_UsesPaddingMarker_ConsumerSkips()
    {
        // Layout trace (capacity=32, mask=31):
        //   Step 1: publish 8B  → recordSize=12, pos=0,  contiguous=32 ≥ 12, tail→12
        //   Step 2: consume     → head→12, IsEmpty
        //   Step 3: publish 12B → recordSize=16, pos=12, contiguous=20 ≥ 16, tail→28
        //   Step 4: consume     → head→28, IsEmpty
        //   Step 5: publish 8B  → recordSize=12, pos=28, contiguous=4  < 12 → padding at 28
        //                         tail+=4 → 32; record at pos=0, tail_logical=32+12=44
        //   Consumer: head=28 → sees PaddingMarker → skip 4 → head=32 → pos=0 → sees real record.

        var ring = new SpscByteRingBuffer(32);

        var first  = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };           // 8 bytes
        var second = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };   // 12 bytes
        var third  = new byte[] { 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8 }; // 8 bytes

        // Step 1 + consume
        ring.TryPublish(first.AsSpan()).Should().BeTrue();
        ring.TryPeek(out _, out int adv1).Should().BeTrue();
        ring.Advance(adv1); // head→12

        // Step 3 + consume
        ring.TryPublish(second.AsSpan()).Should().BeTrue();
        ring.TryPeek(out _, out int adv2).Should().BeTrue();
        ring.Advance(adv2); // head→28

        // Step 5: forces padding at pos=28, record at pos=0
        ring.TryPublish(third.AsSpan()).Should().BeTrue();

        // Consumer must transparently skip the padding marker and return the third payload.
        ring.TryPeek(out var result, out int adv3).Should().BeTrue();
        result.ToArray().Should().Equal(third);
        adv3.Should().Be(12); // 4 header + 8 payload (already 4-aligned)

        ring.Advance(adv3);
        ring.IsEmpty.Should().BeTrue();
    }

    // ── empty peek ───────────────────────────────────────────────────────────

    [Fact]
    public void TryPeek_Empty_ReturnsFalse()
    {
        var ring = new SpscByteRingBuffer(64);
        ring.TryPeek(out var payload, out int advance).Should().BeFalse();
        advance.Should().Be(0);
        payload.IsEmpty.Should().BeTrue();
    }

    // ── ByteCount ────────────────────────────────────────────────────────────

    [Fact]
    public void ByteCount_TracksOutstanding()
    {
        // record for 12B payload = 4+12 = 16; record for 20B payload = 4+20 = 24 → total 40.
        var ring = new SpscByteRingBuffer(64);
        ring.TryPublish(new byte[12].AsSpan()).Should().BeTrue();
        ring.TryPublish(new byte[20].AsSpan()).Should().BeTrue();
        ring.ByteCount.Should().Be(40);
    }

    // ── Reset ────────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_ClearsHeadAndTail()
    {
        var ring = new SpscByteRingBuffer(32);
        ring.TryPublish(new byte[] { 1, 2, 3 }.AsSpan()).Should().BeTrue();
        ring.IsEmpty.Should().BeFalse();

        ring.Reset();

        ring.IsEmpty.Should().BeTrue();
        ring.ByteCount.Should().Be(0);
    }

    // ── stress ───────────────────────────────────────────────────────────────

    [Trait("Category", "Stress")]
    [Fact]
    public void Stress_SingleProducerSingleConsumer_1MRecords_NoLossNoCorruption()
    {
        // Use a 64 KiB ring so the producer doesn't spin-stall too long.
        const int Capacity    = 65536;
        const int TotalItems  = 1_000_000;
        const int MaxPayload  = 200;

        var ring = new SpscByteRingBuffer(Capacity);
        var rng  = new Random(42);

        var  done          = new ManualResetEventSlim(false);
        long producerHash  = 0;
        long consumerHash  = 0;
        int  producedCount = 0;
        int  consumedCount = 0;
        bool seqMonotonic  = true;

        // Pre-generate payloads deterministically so producer and consumer can agree.
        // Each record: [seq:4 bytes LE][random payload]
        var payloads = new byte[TotalItems][];
        for (int i = 0; i < TotalItems; i++)
        {
            int len  = rng.Next(1, MaxPayload + 1);
            var buf  = new byte[4 + len];
            BinaryPrimitives_WriteInt32LE(buf, 0, i);
            for (int j = 4; j < buf.Length; j++)
                buf[j] = (byte)(i ^ j);
            payloads[i] = buf;
        }

        // Compute expected producer hash (over full record bytes including seq prefix).
        foreach (var p in payloads)
            foreach (var b in p) producerHash = unchecked(producerHash * 31 + b);

        var producerThread = new Thread(() =>
        {
            for (int i = 0; i < TotalItems; i++)
            {
                while (!ring.TryPublish(payloads[i].AsSpan()))
                    Thread.SpinWait(10);
                producedCount++;
            }
        }) { IsBackground = true };

        var consumerThread = new Thread(() =>
        {
            int lastSeq = -1;
            try
            {
                while (consumedCount < TotalItems)
                {
                    if (!ring.TryPeek(out var payload, out int advance))
                    {
                        Thread.SpinWait(10);
                        continue;
                    }

                    // Verify sequence number (first 4 bytes).
                    int seq = BinaryPrimitives_ReadInt32LE(payload);
                    if (seq != lastSeq + 1) seqMonotonic = false;
                    lastSeq = seq;

                    foreach (var b in payload) consumerHash = unchecked(consumerHash * 31 + b);

                    ring.Advance(advance);
                    consumedCount++;
                }
            }
            finally
            {
                done.Set();
            }
        }) { IsBackground = true };

        consumerThread.Start();
        producerThread.Start();

        done.Wait(TimeSpan.FromSeconds(30)).Should().BeTrue("stress test must finish within 30 s");

        producerThread.Join(1_000);
        consumerThread.Join(1_000);

        consumedCount.Should().Be(TotalItems, "no records must be lost");
        consumerHash.Should().Be(producerHash, "no corruption");
        seqMonotonic.Should().BeTrue("sequence numbers must arrive in order");
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static void BinaryPrimitives_WriteInt32LE(byte[] buf, int offset, int value)
    {
        buf[offset]     = (byte) value;
        buf[offset + 1] = (byte)(value >> 8);
        buf[offset + 2] = (byte)(value >> 16);
        buf[offset + 3] = (byte)(value >> 24);
    }

    private static int BinaryPrimitives_ReadInt32LE(ReadOnlySpan<byte> span)
        => span[0] | (span[1] << 8) | (span[2] << 16) | (span[3] << 24);
}
