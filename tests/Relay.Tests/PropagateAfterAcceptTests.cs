using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests;

/// <summary>
/// Verifies the <see cref="DispatchSink{T}.PropagateAfterAccept"/> virtual property semantics:
/// default (false) stops after successful accept; override (true) continues to Next even after
/// a successful accept. Fallback to Next on unhealthy or Accept=false is unchanged.
/// </summary>
public sealed class PropagateAfterAcceptTests
{
    [Fact]
    public void Default_StopsAfterSuccessfulAccept()
    {
        // Default PropagateAfterAccept=false: A accepts, chain stops — B never sees the item.
        var a = new CountingPipe();
        var b = new CountingPipe();
        a.Next = b;

        a.Enqueue(new Entry64 { A = 1 });

        a.Accepted.Should().Be(1);
        b.Accepted.Should().Be(0);
    }

    [Fact]
    public void Propagate_ForwardsToNextAfterSuccessfulAccept()
    {
        // PropagateAfterAccept=true: A accepts and still forwards to B.
        var a = new PropagateCountingPipe();
        var b = new CountingPipe();
        a.Next = b;

        a.Enqueue(new Entry64 { A = 1 });

        a.Accepted.Should().Be(1);
        b.Accepted.Should().Be(1);
    }

    [Fact]
    public void Propagate_IsHealthyFalse_SkipsAcceptAndGoesToNext()
    {
        // IsHealthy=false gates Accept — Accept is never called regardless of PropagateAfterAccept.
        // Item falls through to B via the standard fallback path, not via propagate.
        var a = new PropagateButUnhealthyPipe();
        var b = new CountingPipe();
        a.Next = b;

        a.Enqueue(new Entry64 { A = 1 });

        a.AttemptedAccept.Should().Be(0, "IsHealthy=false short-circuits before Accept");
        b.Accepted.Should().Be(1);
    }

    [Fact]
    public void Propagate_ThreeHopChain_AllReceive_WhenAllAccept()
    {
        // Every pipe has PropagateAfterAccept=true — all three receive the item.
        var a = new PropagateCountingPipe();
        var b = new PropagateCountingPipe();
        var c = new CountingPipe();
        a.Next = b;
        b.Next = c;

        a.Enqueue(new Entry64 { A = 1 });

        a.Accepted.Should().Be(1);
        b.Accepted.Should().Be(1);
        c.Accepted.Should().Be(1);
    }

    [Fact]
    public void Propagate_ThreeHopChain_StopsAtFirstNonPropagate()
    {
        // A propagates to B, but B has default PropagateAfterAccept=false — C never receives.
        var a = new PropagateCountingPipe();
        var b = new CountingPipe();
        var c = new CountingPipe();
        a.Next = b;
        b.Next = c;

        a.Enqueue(new Entry64 { A = 1 });

        a.Accepted.Should().Be(1);
        b.Accepted.Should().Be(1);
        c.Accepted.Should().Be(0, "B's PropagateAfterAccept=false stops the chain");
    }

    [Fact]
    public void AcceptReturnsFalse_Propagate_FallsThroughToNext()
    {
        // Accept is called (IsHealthy=true) but returns false — propagate does NOT engage.
        // The item falls through to Next via the standard fallback path.
        var a = new PropagateRejecterPipe();
        var b = new CountingPipe();
        a.Next = b;

        a.Enqueue(new Entry64 { A = 1 });

        a.AttemptedAccept.Should().Be(1, "Accept was called — IsHealthy gate passed");
        b.Accepted.Should().Be(1, "fallback path still routes to Next after Accept=false");
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

    private sealed class PropagateCountingPipe : DispatchSink<Entry64>
    {
        public PropagateCountingPipe() : base(propagateAfterAccept: true) { }
        public int Accepted;
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) { Accepted++; return true; }
        public override void Flush() { }
        public override void Dispose() { }
    }

    private sealed class PropagateButUnhealthyPipe : DispatchSink<Entry64>
    {
        public PropagateButUnhealthyPipe() : base(propagateAfterAccept: true) { }
        public int AttemptedAccept;
        public override bool IsHealthy => false;
        protected override bool Accept(in Entry64 item) { AttemptedAccept++; return true; }
        public override void Flush() { }
        public override void Dispose() { }
    }

    private sealed class PropagateRejecterPipe : DispatchSink<Entry64>
    {
        public PropagateRejecterPipe() : base(propagateAfterAccept: true) { }
        public int AttemptedAccept;
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) { AttemptedAccept++; return false; }
        public override void Flush() { }
        public override void Dispose() { }
    }
}
