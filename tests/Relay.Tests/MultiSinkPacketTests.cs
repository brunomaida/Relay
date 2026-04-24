using System;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class MultiSinkPacketTests
{
    private static readonly byte[] Payload = [5, 6, 7];

    [Fact]
    public void Enqueue_AllChildrenReceivePayload()
    {
        var a    = new CollectingSink();
        var b    = new CollectingSink();
        var c    = new CollectingSink();
        var multi = new MultiSink(a, b, c);

        multi.Enqueue(Payload);

        a.Received.Should().HaveCount(1);
        b.Received.Should().HaveCount(1);
        c.Received.Should().HaveCount(1);
    }

    [Fact]
    public void IsHealthy_True_WhenAtLeastOneChildHealthy()
    {
        var healthy   = new CollectingSink();
        var unhealthy = new CollectingSink();
        unhealthy.SetHealthy(false);
        var multi = new MultiSink(healthy, unhealthy);

        multi.IsHealthy.Should().BeTrue();
    }

    [Fact]
    public void IsHealthy_False_WhenAllChildrenUnhealthy()
    {
        var a = new CollectingSink();
        var b = new CollectingSink();
        a.SetHealthy(false);
        b.SetHealthy(false);
        var multi = new MultiSink(a, b);

        multi.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public void Enqueue_AllChildrenUnhealthy_FallsToNext()
    {
        var a    = new CollectingSink();
        var b    = new CollectingSink();
        var next = new CollectingSink();
        a.SetHealthy(false);
        b.SetHealthy(false);
        var multi = new MultiSink(a, b);
        multi.Next = next;

        multi.Enqueue(Payload);

        next.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Enqueue_UnhealthyChild_StillReceivesPayload_ViaEnqueue()
    {
        var healthy   = new CollectingSink();
        var unhealthy = new CollectingSink();
        unhealthy.SetHealthy(false);
        var multi = new MultiSink(healthy, unhealthy);

        multi.Enqueue(Payload);

        healthy.Received.Should().HaveCount(1);
        unhealthy.AcceptCallCount.Should().Be(0);
    }
}
