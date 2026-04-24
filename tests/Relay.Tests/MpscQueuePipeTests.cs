using System;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Builder;
using Xunit;

namespace Relay.Tests;

/// <summary>MpscQueuePipe lifecycle, multi-producer delivery, ring-full fallback, and crash semantics.</summary>
public sealed class MpscQueuePipeTests
{
    // ── lifecycle ─────────────────────────────────────────────────────────────

    [Fact]
    public void Start_Stop_LifeCycle()
    {
        using var pipe = new InMemoryMpscPipe();
        pipe.Start();
        pipe.IsConsuming.Should().BeTrue();

        pipe.Stop(500);
        pipe.IsConsuming.Should().BeFalse();
    }

    // ── single producer ───────────────────────────────────────────────────────

    [Fact]
    public void SingleProducer_Enqueue_ConsumerReceives()
    {
        using var pipe = new InMemoryMpscPipe();
        pipe.Start();

        const int count = 32;
        for (int i = 0; i < count; i++)
            pipe.Enqueue(new Entry64 { A = i, B = i * 2 });

        pipe.Stop(drainTimeoutMs: 2_000);

        pipe.ConsumedCount.Should().Be(count);
    }

    // ── multi-producer ────────────────────────────────────────────────────────

    [Fact]
    public void MultiProducer_4Threads_AllItemsDelivered()
    {
        const int producers    = 4;
        const int itemsPerProd = 10_000;
        const int total        = producers * itemsPerProd;

        // Ring sized generously so TryPublish never returns false during the burst.
        // Enqueue drops silently when the ring is full (Next == null), so we must ensure
        // the ring never saturates: capacity > producers × itemsPerProd with headroom.
        using var pipe    = new InMemoryMpscPipe(ringCapacity: 65536);
        var       barrier = new ManualResetEventSlim(false);
        var       threads = new Thread[producers];
        long      expectedSum = 0;

        // Compute expected sum before releasing producers.
        for (int p = 0; p < producers; p++)
            for (int seq = 0; seq < itemsPerProd; seq++)
                expectedSum += p * 100_000L + seq + 1;

        for (int p = 0; p < producers; p++)
        {
            int pid = p;
            threads[p] = new Thread(() =>
            {
                barrier.Wait();
                for (int seq = 0; seq < itemsPerProd; seq++)
                {
                    var item = new Entry64 { A = pid * 100_000L + seq + 1 };
                    pipe.Enqueue(in item);
                }
            })
            { IsBackground = true };
            threads[p].Start();
        }

        pipe.Start();
        barrier.Set();

        foreach (var t in threads)
            t.Join(10_000).Should().BeTrue("producer thread timed out");

        pipe.Stop(drainTimeoutMs: 5_000);

        pipe.ConsumedCount.Should().Be(total);
        pipe.SumOfA.Should().Be(expectedSum);
    }

    // ── ring-full fallback ────────────────────────────────────────────────────

    [Fact]
    public void RingFull_FallsBackToNext_WhenConsumerNotStarted()
    {
        // No consumer thread — ring fills and stays full; items overflow to Next.
        const int cap      = 16;
        const int overflow = 10;

        var fallback = new CountingMpscPipe();
        using var pipe = new InMemoryMpscPipe(ringCapacity: cap, consumeItems: false);
        RelayBuilder.Start<Entry64, InMemoryMpscPipe>(pipe).To(fallback).Build();
        // Do NOT call Start() — ring never drains.

        for (int i = 0; i < cap; i++)
            pipe.Enqueue(new Entry64 { A = i });     // fills ring

        for (int i = 0; i < overflow; i++)
            pipe.Enqueue(new Entry64 { A = 99 });    // ring full → Next

        fallback.Accepted.Should().Be(overflow);
    }

    // ── crash semantics ───────────────────────────────────────────────────────

    [Fact]
    public void ConsumerException_ExposedAfterCrash()
    {
        using var pipe = new CrashingMpscPipe();
        pipe.Start();
        pipe.Enqueue(new Entry64 { A = 1 });

        Thread.Sleep(200); // let consumer crash

        pipe.IsConsuming.Should().BeFalse();
        pipe.ConsumerException.Should().NotBeNull();
    }

    // ── unhealthy primary routes to fallback ──────────────────────────────────

    [Fact]
    public void Serial_ItemRoutedToFallback_WhenPrimaryUnhealthy()
    {
        using var primary  = new UnhealthyMpscPipe();
        using var fallback = new InMemoryMpscPipe();

        primary.Next = fallback;

        primary.Start();
        fallback.Start();

        primary.Enqueue(new Entry64 { A = 99 });

        Thread.Sleep(200);

        fallback.Stop(500);
        fallback.ConsumedCount.Should().Be(1);

        primary.Stop(500);
    }

    // ── private test pipes ────────────────────────────────────────────────────

    private sealed class InMemoryMpscPipe : MpscQueuePipe<Entry64>
    {
        private readonly bool _consume;
        private int           _count;
        private long          _sumOfA;

        public InMemoryMpscPipe(int ringCapacity = 1024, bool consumeItems = true)
            : base(ringCapacity, 50, "test") => _consume = consumeItems;

        public int  ConsumedCount => Volatile.Read(ref _count);
        public long SumOfA        => Volatile.Read(ref _sumOfA);

        protected override void WriteToBackend(in Entry64 item)
        {
            if (!_consume) return;
            Volatile.Write(ref _count,  _count  + 1);
            Volatile.Write(ref _sumOfA, _sumOfA + item.A);
        }

        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class CrashingMpscPipe : MpscQueuePipe<Entry64>
    {
        public CrashingMpscPipe() : base(16, 50, "crash") { }

        protected override void WriteToBackend(in Entry64 item) =>
            throw new InvalidOperationException("crash");

        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    /// <summary>Always-unhealthy MPSC pipe — routes every item to Next without consuming.</summary>
    private sealed class UnhealthyMpscPipe : MpscQueuePipe<Entry64>
    {
        public UnhealthyMpscPipe() : base(64, 50, "unhealthy") { }

        public override bool IsHealthy => false;

        protected override void WriteToBackend(in Entry64 item) { }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class CountingMpscPipe : DispatchPipe<Entry64>
    {
        public int Accepted { get; private set; }
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) { Accepted++; return true; }
        public override void Flush()   { }
        public override void Dispose() { }
    }
}
