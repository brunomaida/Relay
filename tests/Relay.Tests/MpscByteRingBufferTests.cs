using System;
using System.Threading;
using FluentAssertions;
using Relay.Buffers;
using Xunit;

namespace Relay.Tests;

/// <summary>Unit and stress tests for <see cref="MpscByteRingBuffer"/>.</summary>
public sealed class MpscByteRingBufferTests
{
    // ── constructor validation ────────────────────────────────────────────────

    [Theory]
    [InlineData(15)]
    [InlineData(17)]
    [InlineData(100)]
    public void Constructor_NonPowerOfTwo_Throws(int capacity)
    {
        var act = () => new MpscByteRingBuffer(capacity);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void Constructor_SmallerThanMinimum_Throws(int capacity)
    {
        var act = () => new MpscByteRingBuffer(capacity);
        act.Should().Throw<ArgumentException>();
    }

    // ── basic round-trip ──────────────────────────────────────────────────────

    [Fact]
    public void TryPublish_SinglePayload_RoundTripSucceeds()
    {
        var ring    = new MpscByteRingBuffer(64);
        var payload = new byte[20];
        for (int i = 0; i < 20; i++) payload[i] = (byte)(i + 1);

        ring.TryPublish(payload.AsSpan()).Should().BeTrue();

        ring.TryPeek(out var span, out int advance).Should().BeTrue();
        span.Length.Should().Be(20);
        span.ToArray().Should().Equal(payload);

        // advance = 4 (header) + 20 padded to 20 (already 4-aligned) = 24
        advance.Should().Be(24);
        ring.Advance(advance);
        ring.IsEmpty.Should().BeTrue();
    }

    // ── capacity boundary ─────────────────────────────────────────────────────

    [Fact]
    public void TryPublish_PayloadLargerThanCapacity_ReturnsFalse()
    {
        // capacity=16. recordSize = 4 header + 16 padded = 20 > 16 → rejected before CAS.
        var ring = new MpscByteRingBuffer(16);
        ring.TryPublish(new byte[13].AsSpan()).Should().BeFalse();
    }

    [Fact]
    public void TryPublish_FillExactCapacity_Succeeds_ThenNextReturnsFalse()
    {
        // capacity=32. Each 4B payload takes a 4+4=8B record.
        // Three records = 24B claimed (out of 32). Fourth would bring claimed to 32 = head+capacity → fails.
        var ring = new MpscByteRingBuffer(32);

        ring.TryPublish(new byte[4].AsSpan()).Should().BeTrue();
        ring.TryPublish(new byte[4].AsSpan()).Should().BeTrue();
        ring.TryPublish(new byte[4].AsSpan()).Should().BeTrue();

        // A fourth record would require claimed+8-32=0 <= head=0 → ring full.
        ring.TryPublish(new byte[4].AsSpan()).Should().BeFalse();
    }

    // ── wrap-around + padding marker ──────────────────────────────────────────

    [Fact]
    public void TryPublish_WrapAround_UsesPaddingMarker_ConsumerSkips()
    {
        // Geometry (capacity=32):
        //   Publish 8B: header(4) + payload(8) = 12B → tail=12
        //   Consume: head=12
        //   Publish 12B: header(4) + payload(12) = 16B → tail=28
        //   Consume: head=28
        //   Publish 8B: needs 4+8=12B; contiguous from pos=28 = 4 < 12 → wrap.
        //     Producer stamps padding at pos=28 (size=4), writes record at pos=0 → tail=28+4+12=44
        //   Consumer at head=28 reads padding marker → skips to head=32 (pos=0 masked) → reads 8B record.

        var ring = new MpscByteRingBuffer(32);

        // Step 1: publish 8B
        var first = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        ring.TryPublish(first.AsSpan()).Should().BeTrue();

        // Consume first record
        ring.TryPeek(out var s1, out int a1).Should().BeTrue();
        s1.Length.Should().Be(8);
        ring.Advance(a1); // advance 4+8=12, head=12

        // Step 2: publish 12B
        ring.TryPublish(new byte[12].AsSpan()).Should().BeTrue();

        // Consume second record
        ring.TryPeek(out _, out int a2).Should().BeTrue();
        ring.Advance(a2); // advance 4+12=16, head=28

        // Step 3: publish 8B — will trigger wrap-around
        var third = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
        ring.TryPublish(third.AsSpan()).Should().BeTrue();

        // Consumer at head=28 encounters padding → skips → returns third record at pos=0
        ring.TryPeek(out var s3, out int a3).Should().BeTrue();
        s3.Length.Should().Be(8);
        s3.ToArray().Should().Equal(third);

        ring.Advance(a3);
        ring.IsEmpty.Should().BeTrue();
    }

    // ── TryPeek empty ─────────────────────────────────────────────────────────

    [Fact]
    public void TryPeek_Empty_ReturnsFalse()
    {
        var ring = new MpscByteRingBuffer(16);

        ring.TryPeek(out var payload, out int advance).Should().BeFalse();
        payload.Length.Should().Be(0);
        advance.Should().Be(0);
    }

    // ── IsEmpty ───────────────────────────────────────────────────────────────

    [Fact]
    public void IsEmpty_TracksPublished()
    {
        var ring = new MpscByteRingBuffer(64);

        ring.IsEmpty.Should().BeTrue();

        ring.TryPublish(new byte[] { 1, 2, 3, 4 }.AsSpan()).Should().BeTrue();
        ring.IsEmpty.Should().BeFalse();

        ring.TryPeek(out _, out int adv).Should().BeTrue();
        ring.Advance(adv);
        ring.IsEmpty.Should().BeTrue();
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_ClearsState()
    {
        var ring = new MpscByteRingBuffer(64);

        ring.TryPublish(new byte[] { 1, 2, 3, 4 }.AsSpan()).Should().BeTrue();
        ring.IsEmpty.Should().BeFalse();

        ring.Reset();

        ring.IsEmpty.Should().BeTrue();
        ring.ByteCount.Should().Be(0);

        // Republish succeeds after reset.
        ring.TryPublish(new byte[] { 9, 8, 7, 6 }.AsSpan()).Should().BeTrue();
        ring.TryPeek(out var span, out int adv).Should().BeTrue();
        span.ToArray().Should().Equal(new byte[] { 9, 8, 7, 6 });
        ring.Advance(adv);
    }

    // ── stress: single producer + single consumer ─────────────────────────────

    [Trait("Category", "Stress")]
    [Fact]
    public void Stress_SingleProducerSingleConsumer_100KRecords_NoLossNoCorruption()
    {
        const int Records   = 100_000;
        const int TimeoutMs = 30_000;

        var ring          = new MpscByteRingBuffer(65536);
        var rng           = new Random(42);
        var consumed      = 0;
        var corrupt       = false;
        var consumerError = (Exception?)null;
        var done          = new ManualResetEventSlim(false);

        // Pre-generate payload sizes so consumer can mirror the generator.
        var sizes = new int[Records];
        for (int i = 0; i < Records; i++)
            sizes[i] = 1 + rng.Next(100); // 1..100

        var consumer = new Thread(() =>
        {
            try
            {
                int seq = 0;
                while (seq < Records)
                {
                    if (!ring.TryPeek(out var payload, out int adv))
                    {
                        Thread.SpinWait(20);
                        continue;
                    }

                    // First 4 bytes = sequence number (LE int32)
                    if (payload.Length < 4)
                    {
                        corrupt = true;
                        ring.Advance(adv);
                        continue;
                    }

                    int gotSeq = payload[0] | (payload[1] << 8) | (payload[2] << 16) | (payload[3] << 24);
                    if (gotSeq != seq)
                        corrupt = true;

                    // Verify remaining bytes = (seq & 0xFF) repeated
                    byte fill = (byte)(seq & 0xFF);
                    for (int i = 4; i < payload.Length; i++)
                    {
                        if (payload[i] != fill) { corrupt = true; break; }
                    }

                    ring.Advance(adv);
                    seq++;
                    consumed++;
                }
            }
            catch (Exception ex)
            {
                consumerError = ex;
            }
            finally
            {
                done.Set();
            }
        })
        { IsBackground = true };

        consumer.Start();

        // Producer runs on the test thread.
        var buf = new byte[104]; // max 4 + 100
        for (int seq = 0; seq < Records; seq++)
        {
            int dataLen = sizes[seq];
            int total   = 4 + dataLen;

            buf[0] = (byte) seq;
            buf[1] = (byte)(seq >> 8);
            buf[2] = (byte)(seq >> 16);
            buf[3] = (byte)(seq >> 24);
            byte fill = (byte)(seq & 0xFF);
            for (int i = 4; i < total; i++) buf[i] = fill;

            // Spin until ring has room.
            while (!ring.TryPublish(buf.AsSpan(0, total)))
                Thread.SpinWait(20);
        }

        done.Wait(TimeoutMs).Should().BeTrue("consumer did not finish within 30 s");
        consumerError.Should().BeNull();
        consumed.Should().Be(Records);
        corrupt.Should().BeFalse();
    }

    // ── stress: multi-producer ────────────────────────────────────────────────

    [Trait("Category", "Stress")]
    [Fact]
    public void Stress_MultiProducer_4Threads_100KTotal_NoLossNoCorruption()
    {
        const int Producers   = 4;
        const int PerProducer = 25_000;
        const int Total       = Producers * PerProducer;
        const int TimeoutMs   = 30_000;
        const int MaxPayload  = 64;

        var ring          = new MpscByteRingBuffer(1 << 20); // 1 MiB
        var barrier       = new ManualResetEventSlim(false);
        var received      = 0;
        var corrupt       = false;
        var consumerError = (Exception?)null;
        var done          = new ManualResetEventSlim(false);

        // Per-producer last-seen sequence (to verify monotonicity within each producer).
        var lastSeq = new int[Producers];
        for (int i = 0; i < Producers; i++) lastSeq[i] = -1;

        var consumer = new Thread(() =>
        {
            try
            {
                int count = 0;
                while (count < Total)
                {
                    if (!ring.TryPeek(out var payload, out int adv))
                    {
                        Thread.SpinWait(20);
                        continue;
                    }

                    if (payload.Length < 8)
                    {
                        corrupt = true;
                        ring.Advance(adv);
                        continue;
                    }

                    int prodId = payload[0] | (payload[1] << 8) | (payload[2] << 16) | (payload[3] << 24);
                    int seq    = payload[4] | (payload[5] << 8) | (payload[6] << 16) | (payload[7] << 24);

                    if ((uint)prodId >= (uint)Producers)
                    {
                        corrupt = true;
                    }
                    else
                    {
                        // Sequence must be strictly greater than the last seen for this producer.
                        if (seq <= lastSeq[prodId])
                            corrupt = true;
                        else
                            lastSeq[prodId] = seq;
                    }

                    ring.Advance(adv);
                    count++;
                    Interlocked.Increment(ref received);
                }
            }
            catch (Exception ex)
            {
                consumerError = ex;
            }
            finally
            {
                done.Set();
            }
        })
        { IsBackground = true };

        consumer.Start();

        var threads = new Thread[Producers];
        for (int p = 0; p < Producers; p++)
        {
            int pid = p;
            threads[p] = new Thread(() =>
            {
                var rng = new Random(pid * 17 + 3);
                var buf = new byte[MaxPayload];
                barrier.Wait();

                for (int seq = 1; seq <= PerProducer; seq++)
                {
                    // tag(8) + body(4..56) capped at MaxPayload
                    int bodyLen = 4 + rng.Next(MaxPayload - 8 + 1); // 4..56 → total 12..64 but cap
                    int total   = Math.Min(8 + bodyLen, MaxPayload);
                    if (total < 8) total = 8;

                    // Write 4B producerId + 4B seq (LE)
                    buf[0] = (byte) pid;
                    buf[1] = (byte)(pid >> 8);
                    buf[2] = (byte)(pid >> 16);
                    buf[3] = (byte)(pid >> 24);
                    buf[4] = (byte) seq;
                    buf[5] = (byte)(seq >> 8);
                    buf[6] = (byte)(seq >> 16);
                    buf[7] = (byte)(seq >> 24);

                    while (!ring.TryPublish(buf.AsSpan(0, total)))
                        Thread.SpinWait(20);
                }
            })
            { IsBackground = true };
            threads[p].Start();
        }

        barrier.Set();

        foreach (var t in threads)
            t.Join(TimeoutMs).Should().BeTrue("producer thread timed out");

        done.Wait(TimeoutMs).Should().BeTrue("consumer did not finish within 30 s");

        consumerError.Should().BeNull();
        received.Should().Be(Total);
        corrupt.Should().BeFalse();
    }
}
