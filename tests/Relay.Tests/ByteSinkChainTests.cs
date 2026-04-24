using System;
using System.Collections.Generic;
using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests;

/// <summary>Chain routing and fallback semantics for <see cref="PacketSink"/>.</summary>
public sealed class ByteSinkChainTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // 1. Healthy head consumes locally; Next never sees the payload.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Enqueue_HealthyPipe_ConsumesLocally_NextNotCalled()
    {
        var head = new CountingByteSink();
        var next = new CountingByteSink();
        head.Next = next;

        head.Enqueue(new byte[] { 1, 2, 3 }.AsSpan());

        head.Accepted.Should().Be(1);
        next.Accepted.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. Unhealthy head (IsHealthy == false) bypasses Accept and forwards.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Enqueue_UnhealthyPipe_ForwardsToNext()
    {
        var head = new DeadByteSink();
        var next = new CountingByteSink();
        head.Next = next;

        head.Enqueue(new byte[] { 7 }.AsSpan());

        next.Accepted.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. Accept returns false (ring full / reject) → forwards to Next.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Enqueue_AcceptReturnsFalse_ForwardsToNext()
    {
        var head = new RejectByteSink();
        var next = new CountingByteSink();
        head.Next = next;

        head.Enqueue(new byte[] { 5, 6 }.AsSpan());

        next.Accepted.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. No Next → silent drop, no exception.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Enqueue_NextNull_SilentlyDropped()
    {
        var head = new DeadByteSink();
        // Next is null by default — not wired.

        var act = () => head.Enqueue(new byte[] { 99 }.AsSpan());
        act.Should().NotThrow();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5. Fallback preserves payload bytes exactly.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Enqueue_PayloadFlowsIntact_AcrossFallback()
    {
        var head    = new DeadByteSink();
        var capture = new CaptureByteSink();
        head.Next = capture;

        var expected = new byte[] { 1, 2, 3, 4, 5 };
        head.Enqueue(expected.AsSpan());

        capture.LastReceived.Should().Equal(expected);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. Dispose is idempotent.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_Idempotent()
    {
        var pipe = new CountingByteSink();
        pipe.Dispose();
        var act = () => pipe.Dispose();
        act.Should().NotThrow();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private test pipes
    // ─────────────────────────────────────────────────────────────────────────

    private sealed class CountingByteSink : PacketSink
    {
        private readonly int _maxAccept;
        private int          _count;

        public CountingByteSink(int maxAccept = int.MaxValue) => _maxAccept = maxAccept;
        public int Accepted => _count;

        public override bool IsHealthy => _count < _maxAccept;

        protected override bool Accept(ReadOnlySpan<byte> payload)
        {
            if (_count >= _maxAccept) return false;
            _count++;
            return true;
        }

        public override void Flush()   { }
        public override void Dispose() { }
    }

    private sealed class DeadByteSink : PacketSink
    {
        public override bool IsHealthy                          => false;
        protected override bool Accept(ReadOnlySpan<byte> payload) => true;
        public override void Flush()   { }
        public override void Dispose() { }
    }

    private sealed class RejectByteSink : PacketSink
    {
        public override bool IsHealthy                          => true;
        protected override bool Accept(ReadOnlySpan<byte> payload) => false;
        public override void Flush()   { }
        public override void Dispose() { }
    }

    /// <summary>Captures the last received payload for inspection.</summary>
    private sealed class CaptureByteSink : PacketSink
    {
        private readonly List<byte[]> _received = new();

        public byte[]? LastReceived => _received.Count > 0 ? _received[_received.Count - 1] : null;

        public override bool IsHealthy => true;

        protected override bool Accept(ReadOnlySpan<byte> payload)
        {
            _received.Add(payload.ToArray());
            return true;
        }

        public override void Flush()   { }
        public override void Dispose() { }
    }
}
