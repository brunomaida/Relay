using System;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class PacketSinkChainTests
{
    private static readonly byte[] Payload = [1, 2, 3, 4];

    [Fact]
    public void Enqueue_HealthySink_AcceptsPayload()
    {
        var sink = new CollectingSink();
        sink.Enqueue(Payload);
        sink.Received.Should().HaveCount(1);
        sink.Received[0].Should().Equal(Payload);
    }

    [Fact]
    public void Enqueue_UnhealthySink_FallsThrough_ToNext()
    {
        var primary = new CollectingSink();
        var fallback = new CollectingSink();
        primary.Next = fallback;
        primary.SetHealthy(false);

        primary.Enqueue(Payload);

        primary.Received.Should().BeEmpty();
        fallback.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Enqueue_NullNext_UnhealthySink_DropsPayload()
    {
        var sink = new CollectingSink();
        sink.SetHealthy(false);

        var act = () => sink.Enqueue(Payload);
        act.Should().NotThrow();
    }

    [Fact]
    public void Enqueue_PropagateAfterAccept_True_PayloadReachesNext()
    {
        var fork = new PropagatingCollectingSink();
        var next = new CollectingSink();
        fork.Next = next;

        fork.Enqueue(Payload);

        fork.Received.Should().HaveCount(1);
        next.Received.Should().HaveCount(1);
        next.Received[0].Should().Equal(Payload);
    }

    [Fact]
    public void Enqueue_PropagateAfterAccept_False_NextNotCalled()
    {
        var sink = new CollectingSink(); // PropagateAfterAccept = false (default)
        var next = new CollectingSink();
        sink.Next = next;

        sink.Enqueue(Payload);

        sink.Received.Should().HaveCount(1);
        next.Received.Should().BeEmpty();
    }

    [Fact]
    public void Enqueue_ThreeNodeChain_FallsToTerminal()
    {
        var a = new CollectingSink();
        var b = new CollectingSink();
        var c = new CollectingSink();
        a.Next = b;
        b.Next = c;
        a.SetHealthy(false);
        b.SetHealthy(false);

        a.Enqueue(Payload);

        a.Received.Should().BeEmpty();
        b.Received.Should().BeEmpty();
        c.Received.Should().HaveCount(1);
    }

    // Sealed subclass with PropagateAfterAccept = true for testing the propagate path.
    private sealed class PropagatingCollectingSink : CollectingSink
    {
        public PropagatingCollectingSink() : base(propagateAfterAccept: true) { }
    }
}
