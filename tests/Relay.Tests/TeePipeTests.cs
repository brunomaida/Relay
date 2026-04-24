using System;
using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests;

/// <summary>
/// Verifies <see cref="TeePipe{T}"/> semantics: every item is forwarded to the primary pipe
/// AND to <see cref="DispatchPipe{T}.Next"/> (when set), regardless of the primary's internal
/// accept outcome. Lifecycle calls delegate exclusively to the primary.
/// </summary>
public sealed class TeePipeTests
{
    [Fact]
    public void Constructor_NullPrimary_Throws()
    {
        var act = () => new TeePipe<Entry64>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Tee_DeliversToPrimaryAndNext_WhenPrimaryHealthy()
    {
        // Tee delivers to primary via Accept, then propagates to Next (PropagateAfterAccept=true).
        var primary   = new CountingPipe();
        var auditNext = new CountingPipe();
        var tee       = new TeePipe<Entry64>(primary);
        tee.Next      = auditNext;

        tee.Enqueue(new Entry64 { A = 1 });

        primary.Accepted.Should().Be(1);
        auditNext.Accepted.Should().Be(1);
    }

    [Fact]
    public void Tee_IsHealthyMirrorsPrimary()
    {
        // IsHealthy delegates to primary. When primary is dead, Tee is unhealthy — Accept is not
        // called. Item falls through to Next via standard fallback.
        var primary   = new DeadPipe();
        var auditNext = new CountingPipe();
        var tee       = new TeePipe<Entry64>(primary);
        tee.Next      = auditNext;

        tee.IsHealthy.Should().BeFalse();
        tee.Enqueue(new Entry64 { A = 1 });

        auditNext.Accepted.Should().Be(1, "fallback path routes to Next when Tee is unhealthy");
    }

    [Fact]
    public void Tee_Flush_DelegatesToPrimary()
    {
        var primary = new FlushTrackingPipe();
        var tee     = new TeePipe<Entry64>(primary);

        tee.Flush();

        primary.Flushes.Should().Be(1);
    }

    [Fact]
    public void Tee_Dispose_DelegatesToPrimary()
    {
        var primary = new DisposeTrackingPipe();
        var tee     = new TeePipe<Entry64>(primary);

        tee.Dispose();

        primary.Disposed.Should().BeTrue();
    }

    [Fact]
    public void Tee_InChain_NextAlwaysReceives_EvenWhenPrimaryInternallyDrops()
    {
        // Primary is healthy (IsHealthy=true) but its Enqueue internally drops every item
        // (Accept=false, no Next). Tee.Accept is still called — it calls _primary.Enqueue and
        // returns true unconditionally. PropagateAfterAccept=true ensures Next always fires.
        var primary   = new InternalRejectPipe();
        var auditNext = new CountingPipe();
        var tee       = new TeePipe<Entry64>(primary);
        tee.Next      = auditNext;

        tee.Enqueue(new Entry64 { A = 1 });

        // Tee accepted (returned true) → auditNext receives the item.
        auditNext.Accepted.Should().Be(1, "Next always called regardless of primary's internal outcome");
    }

    // ── Private test helpers ──────────────────────────────────────────────────

    private sealed class CountingPipe : DispatchPipe<Entry64>
    {
        public int Accepted;
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) { Accepted++; return true; }
        public override void Flush() { }
        public override void Dispose() { }
    }

    private sealed class DeadPipe : DispatchPipe<Entry64>
    {
        public override bool IsHealthy => false;
        protected override bool Accept(in Entry64 item) => true;
        public override void Flush() { }
        public override void Dispose() { }
    }

    /// <summary>
    /// IsHealthy=true but Accept returns false — simulates a primary that is "up" but
    /// silently rejects (e.g., ring full). Tee.Accept calls _primary.Enqueue, which drops
    /// internally; Tee still returns true so Next is always called.
    /// </summary>
    private sealed class InternalRejectPipe : DispatchPipe<Entry64>
    {
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) => false;
        public override void Flush() { }
        public override void Dispose() { }
    }

    private sealed class FlushTrackingPipe : DispatchPipe<Entry64>
    {
        public int Flushes;
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) => true;
        public override void Flush() => Flushes++;
        public override void Dispose() { }
    }

    private sealed class DisposeTrackingPipe : DispatchPipe<Entry64>
    {
        public bool Disposed;
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) => true;
        public override void Flush() { }
        public override void Dispose() => Disposed = true;
    }
}
