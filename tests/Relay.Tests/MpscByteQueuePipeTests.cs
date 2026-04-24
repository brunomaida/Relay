using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests;

/// <summary>Lifecycle, multi-producer delivery, fallback, and crash semantics for
/// <see cref="MpscByteQueuePipe"/>.</summary>
public sealed class MpscByteQueuePipeTests
{
    // ── lifecycle ─────────────────────────────────────────────────────────────

    [Fact]
    public void Start_Stop_LifeCycle()
    {
        using var pipe = new InMemoryMpscBytePipe();
        pipe.Start();
        pipe.IsConsuming.Should().BeTrue();

        pipe.Stop(drainTimeoutMs: 2_000);
        pipe.IsConsuming.Should().BeFalse();
    }

    // ── single producer ───────────────────────────────────────────────────────

    [Fact]
    public void SingleProducer_Enqueue_ConsumerReceives()
    {
        using var pipe    = new InMemoryMpscBytePipe();
        var       payload = new byte[] { 10, 20, 30 };

        pipe.Start();
        pipe.Enqueue(payload.AsSpan());

        // Wait up to 1 s for the consumer to deliver the record.
        var deadline = DateTime.UtcNow.AddSeconds(1);
        while (pipe.ReceivedCount < 1 && DateTime.UtcNow < deadline)
            Thread.Sleep(5);

        pipe.Stop(drainTimeoutMs: 2_000);

        pipe.Received.Count.Should().Be(1);
        pipe.Received[0].Should().Equal(payload);
    }

    // ── multi-producer ────────────────────────────────────────────────────────

    [Fact]
    public void MultiProducer_4Threads_10KItemsEach_AllDelivered()
    {
        const int Producers    = 4;
        const int PerProducer  = 10_000;
        const int Total        = Producers * PerProducer;

        // Ring sized so TryPublish never returns false during the burst.
        // 40K records × 12B each ≈ 480KB; use 1MB to guarantee no drops.
        using var pipe    = new InMemoryMpscBytePipe(ringCapacity: 1 << 20);
        var       barrier = new ManualResetEventSlim(false);
        var       threads = new Thread[Producers];

        for (int p = 0; p < Producers; p++)
        {
            int pid = p;
            threads[p] = new Thread(() =>
            {
                barrier.Wait();
                var buf = new byte[8];
                for (int seq = 0; seq < PerProducer; seq++)
                {
                    // Encode producerId (int32 LE) + seq (int32 LE) into 8 bytes.
                    buf[0] = (byte) pid;
                    buf[1] = (byte)(pid >> 8);
                    buf[2] = (byte)(pid >> 16);
                    buf[3] = (byte)(pid >> 24);
                    buf[4] = (byte) seq;
                    buf[5] = (byte)(seq >> 8);
                    buf[6] = (byte)(seq >> 16);
                    buf[7] = (byte)(seq >> 24);
                    pipe.Enqueue(buf.AsSpan());
                }
            })
            { IsBackground = true };
            threads[p].Start();
        }

        pipe.Start();
        barrier.Set();

        foreach (var t in threads)
            t.Join(10_000).Should().BeTrue("producer thread timed out");

        pipe.Stop(drainTimeoutMs: 10_000);

        pipe.ReceivedCount.Should().Be(Total);

        // Verify per-producer monotonic sequence — decode each received 8-byte record.
        var lastSeq = new int[Producers];
        for (int p = 0; p < Producers; p++) lastSeq[p] = -1;

        bool monotonic = true;
        foreach (var record in pipe.Received)
        {
            if (record.Length < 8) { monotonic = false; break; }
            int prodId = record[0] | (record[1] << 8) | (record[2] << 16) | (record[3] << 24);
            int seq    = record[4] | (record[5] << 8) | (record[6] << 16) | (record[7] << 24);

            if ((uint)prodId >= (uint)Producers) { monotonic = false; break; }
            if (seq <= lastSeq[prodId])           { monotonic = false; break; }
            lastSeq[prodId] = seq;
        }

        monotonic.Should().BeTrue("per-producer sequences must be monotonically increasing");
    }

    // ── crash semantics ───────────────────────────────────────────────────────

    [Fact]
    public void ConsumerException_ExposedAfterCrash()
    {
        using var pipe = new CrashingMpscBytePipe();
        pipe.Start();
        pipe.Enqueue(new byte[] { 1 }.AsSpan());

        Thread.Sleep(200); // let consumer crash

        pipe.IsConsuming.Should().BeFalse();
        pipe.ConsumerException.Should().NotBeNull();
        pipe.ConsumerException!.Message.Should().Contain("crash");
    }

    // ── ring-full fallback ────────────────────────────────────────────────────

    [Fact]
    public void RingFull_FallsBackToNext_WhenConsumerNotStarted()
    {
        // capacity=32: three 4B records (8B each) = 24B claimed. Fourth hits wrapPoint=0=head → full.
        // Any subsequent Enqueue falls back to Next.
        var primary  = new InMemoryMpscBytePipe(ringCapacity: 32);
        var fallback = new MpscCountingBytePipe();
        primary.Next = fallback; // wire manually (InternalsVisibleTo)

        // Do NOT call Start() — consumer never drains.
        primary.Enqueue(new byte[4].AsSpan()); // tail → 8
        primary.Enqueue(new byte[4].AsSpan()); // tail → 16
        primary.Enqueue(new byte[4].AsSpan()); // tail → 24
        primary.Enqueue(new byte[4].AsSpan()); // ring full (wrapPoint=0=head) → Next

        fallback.Accepted.Should().BeGreaterThanOrEqualTo(1);

        primary.Dispose();
    }

    // ── unhealthy primary routes to fallback ──────────────────────────────────

    [Fact]
    public void Serial_ItemRoutedToFallback_WhenPrimaryUnhealthy()
    {
        using var primary  = new UnhealthyMpscBytePipe(healthy: false);
        using var fallback = new InMemoryMpscBytePipe();

        primary.Next = fallback; // wire chain manually

        primary.Start();
        fallback.Start();

        primary.Enqueue(new byte[] { 42 }.AsSpan());

        Thread.Sleep(200);

        fallback.Stop(drainTimeoutMs: 500);
        fallback.ReceivedCount.Should().Be(1);

        primary.Stop(drainTimeoutMs: 500);
    }

    // ── private test pipes ────────────────────────────────────────────────────

    private sealed class InMemoryMpscBytePipe : MpscByteQueuePipe
    {
        private readonly List<byte[]> _received = new();
        private readonly object       _lock     = new();

        public InMemoryMpscBytePipe(int ringCapacity = 4096, int flushIntervalMs = 50, string name = "test")
            : base(ringCapacity, flushIntervalMs, name) { }

        public int ReceivedCount { get { lock (_lock) return _received.Count; } }
        public IReadOnlyList<byte[]> Received { get { lock (_lock) return _received.ToArray(); } }

        protected override void WriteToBackend(ReadOnlySpan<byte> payload)
        {
            lock (_lock) _received.Add(payload.ToArray());
        }

        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class CrashingMpscBytePipe : MpscByteQueuePipe
    {
        public CrashingMpscBytePipe() : base(64, 50, "crash") { }

        protected override void WriteToBackend(ReadOnlySpan<byte> payload) =>
            throw new InvalidOperationException("crash");

        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class UnhealthyMpscBytePipe : MpscByteQueuePipe
    {
        private readonly bool _healthyFlag;
        public int Consumed { get; private set; }

        public UnhealthyMpscBytePipe(bool healthy) : base(64, 50, "unhealthy") => _healthyFlag = healthy;

        public override bool IsHealthy => _healthyFlag && base.IsHealthy;

        protected override void WriteToBackend(ReadOnlySpan<byte> payload) => Consumed++;
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    /// <summary>Minimal <see cref="BytePipe"/> that counts accepted payloads without consuming.</summary>
    private sealed class MpscCountingBytePipe : BytePipe
    {
        private int _count;
        public int Accepted => _count;
        public override bool IsHealthy => true;

        protected override bool Accept(ReadOnlySpan<byte> payload)
        {
            _count++;
            return true;
        }

        public override void Flush()   { }
        public override void Dispose() { }
    }
}
