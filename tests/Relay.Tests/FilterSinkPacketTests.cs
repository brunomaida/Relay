using System;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class FilterSinkPacketTests
{
    private static readonly byte[] MatchPayload    = [0xFF, 2, 3];
    private static readonly byte[] NoMatchPayload  = [0x00, 2, 3];

    private static bool FirstByteIsFF(ReadOnlySpan<byte> p) => p.Length > 0 && p[0] == 0xFF;

    [Fact]
    public void Accept_MatchingPayload_RoutesToDownstream()
    {
        var downstream = new CollectingSink();
        var filter     = new FilterSink(FirstByteIsFF, downstream);

        filter.Enqueue(MatchPayload);

        downstream.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Accept_NonMatchingPayload_IsDiscarded_NotForwardedAnywhere()
    {
        var downstream = new CollectingSink();
        var next       = new CollectingSink();
        var filter     = new FilterSink(FirstByteIsFF, downstream);
        filter.Next    = next;

        filter.Enqueue(NoMatchPayload);

        downstream.Received.Should().BeEmpty();
        next.Received.Should().BeEmpty("filtered items must not reach Next");
    }

    [Fact]
    public void IsHealthy_AlwaysTrue_EvenWhenDownstreamUnhealthy()
    {
        // Regression: FilterSink must NOT mirror downstream health; otherwise PacketSink.Enqueue
        // would skip Accept when downstream is unhealthy, leaking filtered items into Next.
        var downstream = new CollectingSink();
        var filter     = new FilterSink(FirstByteIsFF, downstream);

        filter.IsHealthy.Should().BeTrue();
        downstream.SetHealthy(false);
        filter.IsHealthy.Should().BeTrue("filter is itself never unhealthy; only downstream can fail");
    }

    [Fact]
    public void Enqueue_DownstreamUnhealthy_NonMatchingPayload_NeverReachesNext()
    {
        // Regression: before the fix, IsHealthy mirrored downstream. When downstream became
        // unhealthy, the base Enqueue routed the payload straight to Next without running
        // the predicate — a filtered item would leak into Next. This must never happen.
        var downstream = new CollectingSink();
        var next       = new CollectingSink();
        downstream.SetHealthy(false);
        var filter     = new FilterSink(FirstByteIsFF, downstream);
        filter.Next    = next;

        filter.Enqueue(NoMatchPayload);

        next.Received.Should().BeEmpty("non-matching payload must be consumed by the filter, never by Next");
    }

    [Fact]
    public void Enqueue_AlwaysReturnsTrue_NextNeverTriggeredViaAccept()
    {
        var downstream = new CollectingSink();
        var next       = new CollectingSink();
        var filter     = new FilterSink(FirstByteIsFF, downstream);
        filter.Next    = next;

        filter.Enqueue(MatchPayload);
        filter.Enqueue(NoMatchPayload);

        next.Received.Should().BeEmpty();
    }
}
