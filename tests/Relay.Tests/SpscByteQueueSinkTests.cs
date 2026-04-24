using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests;

/// <summary>Lifecycle, consumer delivery, fallback, and backend integration for
/// <see cref="SpscByteQueueSink"/>.</summary>
public sealed class SpscByteQueueSinkTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // 1. Start / Stop lifecycle.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Start_Stop_LifeCycle()
    {
        using var pipe = new InMemoryByteSink();
        pipe.Start();
        pipe.IsConsuming.Should().BeTrue();

        pipe.Stop(drainTimeoutMs: 2_000);
        pipe.IsConsuming.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. Consumer receives exactly the enqueued bytes.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Enqueue_ConsumerReceivesPayloadBytewise()
    {
        using var pipe    = new InMemoryByteSink();
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

    // ─────────────────────────────────────────────────────────────────────────
    // 3. 32 records delivered in order.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Enqueue_MultipleRecords_AllDelivered_InOrder()
    {
        // Use a larger ring so all records fit before the consumer has to drain.
        using var pipe = new InMemoryByteSink(ringCapacity: 4096, flushIntervalMs: 50);
        pipe.Start();

        const int Count = 32;
        for (int i = 0; i < Count; i++)
        {
            var buf = new byte[4];
            buf[0] = (byte) i;
            buf[1] = (byte)(i >> 8);
            buf[2] = (byte)(i >> 16);
            buf[3] = (byte)(i >> 24);
            pipe.Enqueue(buf.AsSpan());
        }

        pipe.Stop(drainTimeoutMs: 2_000);

        pipe.Received.Count.Should().Be(Count);

        for (int i = 0; i < Count; i++)
        {
            int seq = pipe.Received[i][0]
                    | (pipe.Received[i][1] << 8)
                    | (pipe.Received[i][2] << 16)
                    | (pipe.Received[i][3] << 24);
            seq.Should().Be(i);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. Crashing backend exposes ConsumerException.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ConsumerException_ExposedAfterCrash()
    {
        using var pipe = new CrashingByteSink();
        pipe.Start();
        pipe.Enqueue(new byte[] { 1 }.AsSpan());

        Thread.Sleep(200); // let consumer crash

        pipe.IsConsuming.Should().BeFalse();
        pipe.ConsumerException.Should().NotBeNull();
        pipe.ConsumerException!.Message.Should().Contain("crash");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5. Ring full without consumer started → fallback to Next.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void RingFull_FallsBackToNext_WhenConsumerNotStarted()
    {
        // capacity=16: record for a 12-byte payload = 4 header + 12 padded = 16 bytes (fills ring).
        // Second 12-byte enqueue: TryPublish returns false → fallback to Next.
        var primary  = new InMemoryByteSink(ringCapacity: 16);
        var fallback = new CountingByteSink();
        primary.Next = fallback; // wire manually (InternalsVisibleTo)

        // Do NOT call Start() — consumer never drains.
        primary.Enqueue(new byte[12].AsSpan()); // fills ring exactly
        primary.Enqueue(new byte[12].AsSpan()); // ring full → Next

        fallback.Accepted.Should().Be(1);

        primary.Dispose();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. Items routed to fallback when primary is unhealthy.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Serial_ItemRoutedToFallback_WhenPrimaryUnhealthy()
    {
        using var primary  = new UnhealthyByteSink(healthy: false);
        using var fallback = new InMemoryByteSink();

        primary.Next = fallback; // wire chain manually

        primary.Start();
        fallback.Start();

        primary.Enqueue(new byte[] { 42 }.AsSpan());

        Thread.Sleep(200);

        fallback.Stop(drainTimeoutMs: 500);
        fallback.ReceivedCount.Should().Be(1);

        primary.Stop(drainTimeoutMs: 500);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 7. Flush() delegates to FlushBackend().
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Flush_InvokesFlushBackend()
    {
        using var pipe = new FlushTrackingByteSink();
        pipe.Start();
        pipe.Enqueue(new byte[] { 1, 2 }.AsSpan());

        pipe.Flush(); // producer-side call routes directly to FlushBackend()

        pipe.FlushCount.Should().BeGreaterThanOrEqualTo(1);
        pipe.Stop(drainTimeoutMs: 2_000);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private test pipes
    // ─────────────────────────────────────────────────────────────────────────

    private sealed class InMemoryByteSink : SpscByteQueueSink
    {
        private readonly List<byte[]> _received = new();
        private readonly object       _lock     = new();

        public InMemoryByteSink(int ringCapacity = 4096, int flushIntervalMs = 50, string name = "test")
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

    private sealed class CrashingByteSink : SpscByteQueueSink
    {
        public CrashingByteSink() : base(64, 50, "crash") { }

        protected override void WriteToBackend(ReadOnlySpan<byte> payload) =>
            throw new InvalidOperationException("crash");

        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class UnhealthyByteSink : SpscByteQueueSink
    {
        private readonly bool _healthyFlag;
        public int Consumed { get; private set; }

        public UnhealthyByteSink(bool healthy) : base(64, 50, "unhealthy")
        {
            _healthyFlag = healthy;
        }

        public override bool IsHealthy => _healthyFlag && base.IsHealthy;

        protected override void WriteToBackend(ReadOnlySpan<byte> payload) => Consumed++;
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class CountingByteSink : PacketSink
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

    private sealed class FlushTrackingByteSink : SpscByteQueueSink
    {
        private int _flushCount;
        public int FlushCount => _flushCount;

        public FlushTrackingByteSink() : base(256, 50, "flush-track") { }

        protected override void WriteToBackend(ReadOnlySpan<byte> payload) { }

        protected override void FlushBackend() =>
            Interlocked.Increment(ref _flushCount);

        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }
}
