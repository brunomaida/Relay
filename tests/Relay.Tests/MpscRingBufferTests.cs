using System;
using System.Runtime.InteropServices;
using System.Threading;
using FluentAssertions;
using Relay.Buffers;
using Xunit;

namespace Relay.Tests;

/// <summary>Unit tests for the internal <see cref="MpscRingBuffer{T}"/>.</summary>
public sealed class MpscRingBufferTests
{
    // ── constructor validation ────────────────────────────────────────────────

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(100)]
    public void Constructor_NonPowerOfTwo_Throws(int capacity)
    {
        var act = () => new MpscRingBuffer<TestEntry64>(capacity);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(4)]
    [InlineData(16)]
    public void Constructor_PowerOfTwo_Succeeds(int capacity)
    {
        var act = () => new MpscRingBuffer<TestEntry64>(capacity);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ZeroOrNegative_Throws(int capacity)
    {
        var act = () => new MpscRingBuffer<TestEntry64>(capacity);
        act.Should().Throw<ArgumentException>();
    }

    // ── basic round-trip ──────────────────────────────────────────────────────

    [Fact]
    public void TryPublish_SingleItem_RoundTripSucceeds()
    {
        var ring = new MpscRingBuffer<TestEntry64>(4);
        var item = new TestEntry64 { A = unchecked((long)0xDEADBEEF_CAFEBABE), B = unchecked((long)0x1234_5678_9ABC_DEF0) };

        ring.TryPublish(in item).Should().BeTrue();
        ring.TryConsume(out var result).Should().BeTrue();
        result.A.Should().Be(item.A);
        result.B.Should().Be(item.B);
    }

    // ── capacity boundary ────────────────────────────────────────────────────

    [Fact]
    public void TryPublish_FillToCapacity_ReturnsFalseWhenFull()
    {
        const int capacity = 16;
        var ring = new MpscRingBuffer<TestEntry64>(capacity);
        var item = new TestEntry64 { A = 1 };

        for (int i = 0; i < capacity; i++)
            ring.TryPublish(in item).Should().BeTrue($"slot {i} should be available");

        ring.TryPublish(in item).Should().BeFalse("ring is full");

        // Consume one slot — should now be able to publish again.
        ring.TryConsume(out _).Should().BeTrue();
        ring.TryPublish(in item).Should().BeTrue("one slot freed after consume");
    }

    // ── empty guard ──────────────────────────────────────────────────────────

    [Fact]
    public void TryConsume_Empty_ReturnsFalse()
    {
        var ring = new MpscRingBuffer<TestEntry64>(4);
        ring.TryConsume(out _).Should().BeFalse();
    }

    // ── IsEmpty tracking ─────────────────────────────────────────────────────

    [Fact]
    public void IsEmpty_TracksPublished()
    {
        var ring = new MpscRingBuffer<TestEntry64>(4);
        var item = new TestEntry64 { A = 7 };

        ring.IsEmpty.Should().BeTrue("fresh ring is empty");

        ring.TryPublish(in item);
        ring.IsEmpty.Should().BeFalse("item published");

        ring.TryConsume(out _);
        ring.IsEmpty.Should().BeTrue("item consumed");
    }

    // ── Reset ────────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_ClearsState()
    {
        var ring = new MpscRingBuffer<TestEntry64>(4);
        var item = new TestEntry64 { A = 5 };

        ring.TryPublish(in item);
        ring.TryPublish(in item);

        ring.Reset();

        ring.IsEmpty.Should().BeTrue("Reset clears published flag");
        ring.Count.Should().Be(0);

        // Ring should be usable again after reset.
        ring.TryPublish(in item).Should().BeTrue();
        ring.TryConsume(out var result).Should().BeTrue();
        result.A.Should().Be(5);
    }

    // ── single-thread ordered delivery ───────────────────────────────────────

    [Fact]
    public void SingleProducerSingleConsumer_1KItems_InOrderDelivery()
    {
        const int count = 1024;
        var ring = new MpscRingBuffer<TestEntry64>(count);

        for (int i = 0; i < count; i++)
        {
            var item = new TestEntry64 { A = i };
            ring.TryPublish(in item).Should().BeTrue();
        }

        for (int i = 0; i < count; i++)
        {
            ring.TryConsume(out var result).Should().BeTrue();
            result.A.Should().Be(i, $"item {i} out of order");
        }

        ring.IsEmpty.Should().BeTrue();
    }

    // ── multi-producer correctness ────────────────────────────────────────────

    [Fact]
    public void MultiProducer_4Threads_ConsumerReceivesAllWithoutLoss()
    {
        const int producers    = 4;
        const int itemsPerProd = 25_000;
        const int total        = producers * itemsPerProd;

        // Ring large enough that TryPublish rarely returns false; producers do NOT retry.
        // We use a larger ring and confirm count at the end rather than mid-run.
        var ring    = new MpscRingBuffer<TestEntry64>(65536);
        var barrier = new ManualResetEventSlim(false);
        var threads = new Thread[producers];

        // Per-producer monotonic sequence tracking (A = producerId * 10^7 + sequence).
        for (int p = 0; p < producers; p++)
        {
            int pid = p;
            threads[p] = new Thread(() =>
            {
                barrier.Wait();
                for (int seq = 0; seq < itemsPerProd; seq++)
                {
                    var item = new TestEntry64 { A = pid, B = seq };
                    while (!ring.TryPublish(in item))
                        Thread.SpinWait(10);
                }
            })
            { IsBackground = true };
            threads[p].Start();
        }

        // Consumer thread: drain until total received.
        int received = 0;
        // Per-producer last sequence (for monotonicity check).
        var lastSeq = new long[producers];
        for (int i = 0; i < producers; i++) lastSeq[i] = -1;
        bool monotonic = true;

        var consumer = new Thread(() =>
        {
            while (received < total)
            {
                if (ring.TryConsume(out var item))
                {
                    received++;
                    int  id  = (int)item.A;
                    long seq = item.B;
                    if (seq <= lastSeq[id])
                        monotonic = false;
                    lastSeq[id] = seq;
                }
                else
                {
                    Thread.SpinWait(10);
                }
            }
        })
        { IsBackground = true };
        consumer.Start();

        barrier.Set();

        foreach (var t in threads)
            t.Join(10_000).Should().BeTrue("producer thread timed out");
        consumer.Join(10_000).Should().BeTrue("consumer thread timed out");

        received.Should().Be(total);
        monotonic.Should().BeTrue("per-producer sequence must be monotonically increasing");
    }

    [Fact]
    public void MultiProducer_HighContention_NoCorruption_Stress()
    {
        const int producers    = 8;
        const int itemsPerProd = 50_000;
        const int total        = producers * itemsPerProd;

        // Small ring forces frequent full-ring fallback; producers spin-retry.
        var ring    = new MpscRingBuffer<TestEntry64>(1024);
        var barrier = new ManualResetEventSlim(false);
        var threads = new Thread[producers];

        // Track per-producer sum so we can verify no loss and no corruption.
        var producerSums = new long[producers];

        for (int p = 0; p < producers; p++)
        {
            int pid = p;
            threads[p] = new Thread(() =>
            {
                barrier.Wait();
                long localSum = 0;
                for (int seq = 1; seq <= itemsPerProd; seq++)
                {
                    // Encode producer id in A, sequence in B.
                    var item = new TestEntry64 { A = pid * 1_000_000L + seq, B = seq };
                    while (!ring.TryPublish(in item))
                        Thread.SpinWait(5);
                    localSum += item.A;
                }
                Volatile.Write(ref producerSums[pid], localSum);
            })
            { IsBackground = true };
            threads[p].Start();
        }

        int  received    = 0;
        long consumerSum = 0;

        var consumer = new Thread(() =>
        {
            while (received < total)
            {
                if (ring.TryConsume(out var item))
                {
                    received++;
                    consumerSum += item.A;
                }
                else
                {
                    Thread.SpinWait(5);
                }
            }
        })
        { IsBackground = true };
        consumer.Start();

        barrier.Set();

        foreach (var t in threads)
            t.Join(25_000).Should().BeTrue("producer thread timed out");
        consumer.Join(25_000).Should().BeTrue("consumer thread timed out");

        long expectedSum = 0;
        for (int p = 0; p < producers; p++)
            expectedSum += Volatile.Read(ref producerSums[p]);

        received.Should().Be(total, "no items should be lost");
        consumerSum.Should().Be(expectedSum, "sum of A values must match — no corruption");
    }

    // ── private helpers ───────────────────────────────────────────────────────

    /// <summary>64-byte cache-line-sized payload for ring buffer tests.</summary>
    [StructLayout(LayoutKind.Sequential, Size = 64)]
    private struct TestEntry64
    {
        public long A;
        public long B;
    }
}
