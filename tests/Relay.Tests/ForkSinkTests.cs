using System;
using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests;

/// <summary>
/// Verifies <see cref="ForkSink{T}"/> semantics: every item is forwarded to the primary pipe
/// AND to <see cref="DispatchSink{T}.Next"/> (when set), regardless of the primary's internal
/// accept outcome. Lifecycle calls delegate exclusively to the primary.
/// </summary>
public sealed class ForkSinkTests
{
    [Fact]
    public void Constructor_NullPrimary_Throws()
    {
        var act = () => new ForkSink<Entry64>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Fork_DeliversToPrimaryAndNext_WhenPrimaryHealthy()
    {
        // Fork delivers to primary via Accept, then propagates to Next (PropagateAfterAccept=true).
        var primary   = new CountingPipe();
        var auditNext = new CountingPipe();
        var fork      = new ForkSink<Entry64>(primary);
        fork.Next     = auditNext;

        fork.Enqueue(new Entry64 { A = 1 });

        primary.Accepted.Should().Be(1);
        auditNext.Accepted.Should().Be(1);
    }

    [Fact]
    public void Fork_IsHealthyMirrorsPrimary()
    {
        // IsHealthy delegates to primary. When primary is dead, Fork is unhealthy — Accept is not
        // called. Item falls through to Next via standard fallback.
        var primary   = new DeadPipe();
        var auditNext = new CountingPipe();
        var fork      = new ForkSink<Entry64>(primary);
        fork.Next     = auditNext;

        fork.IsHealthy.Should().BeFalse();
        fork.Enqueue(new Entry64 { A = 1 });

        auditNext.Accepted.Should().Be(1, "fallback path routes to Next when Fork is unhealthy");
    }

    [Fact]
    public void Fork_Flush_DelegatesToPrimary()
    {
        var primary = new FlushTrackingPipe();
        var fork    = new ForkSink<Entry64>(primary);

        fork.Flush();

        primary.Flushes.Should().Be(1);
    }

    [Fact]
    public void Fork_Dispose_DelegatesToPrimary()
    {
        var primary = new DisposeTrackingPipe();
        var fork    = new ForkSink<Entry64>(primary);

        fork.Dispose();

        primary.Disposed.Should().BeTrue();
    }

    [Fact]
    public void Fork_InChain_NextAlwaysReceives_EvenWhenPrimaryInternallyDrops()
    {
        // Primary is healthy (IsHealthy=true) but its Enqueue internally drops every item
        // (Accept=false, no Next). Fork.Accept is still called — it calls _primary.Enqueue and
        // returns true unconditionally. PropagateAfterAccept=true ensures Next always fires.
        var primary   = new InternalRejectPipe();
        var auditNext = new CountingPipe();
        var fork      = new ForkSink<Entry64>(primary);
        fork.Next     = auditNext;

        fork.Enqueue(new Entry64 { A = 1 });

        // Fork accepted (returned true) → auditNext receives the item.
        auditNext.Accepted.Should().Be(1, "Next always called regardless of primary's internal outcome");
    }

    // ── Private test helpers ──────────────────────────────────────────────────

    private sealed class CountingPipe : DispatchSink<Entry64>
    {
        public int Accepted;
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) { Accepted++; return true; }
        public override void Flush() { }
        public override void Dispose() { }
    }

    private sealed class DeadPipe : DispatchSink<Entry64>
    {
        public override bool IsHealthy => false;
        protected override bool Accept(in Entry64 item) => true;
        public override void Flush() { }
        public override void Dispose() { }
    }

    /// <summary>
    /// IsHealthy=true but Accept returns false — simulates a primary that is "up" but
    /// silently rejects (e.g., ring full). Fork.Accept calls _primary.Enqueue, which drops
    /// internally; Fork still returns true so Next is always called.
    /// </summary>
    private sealed class InternalRejectPipe : DispatchSink<Entry64>
    {
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) => false;
        public override void Flush() { }
        public override void Dispose() { }
    }

    private sealed class FlushTrackingPipe : DispatchSink<Entry64>
    {
        public int Flushes;
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) => true;
        public override void Flush() => Flushes++;
        public override void Dispose() { }
    }

    private sealed class DisposeTrackingPipe : DispatchSink<Entry64>
    {
        public bool Disposed;
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) => true;
        public override void Flush() { }
        public override void Dispose() => Disposed = true;
    }
}
