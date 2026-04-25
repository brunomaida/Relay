using System;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class PacketSinkDropCountTests
{
    [Fact]
    public void Enqueue_Healthy_NoNext_NoDropCounted()
    {
        var sink = new CollectingSink();
        sink.Enqueue([1, 2, 3]);
        sink.DropCount.Should().Be(0, "healthy accept does not count as drop");
    }

    [Fact]
    public void Enqueue_Unhealthy_NoNext_DropCounted()
    {
        var sink = new CollectingSink();
        sink.SetHealthy(false);
        sink.Enqueue([1, 2, 3]);
        sink.DropCount.Should().Be(1, "unhealthy + no fallback = drop");
    }

    [Fact]
    public void Enqueue_Unhealthy_WithNext_DropNotCountedHere()
    {
        var primary  = new CollectingSink();
        var fallback = new CollectingSink();
        primary.Next = fallback;
        primary.SetHealthy(false);

        primary.Enqueue([1, 2, 3]);

        primary.DropCount.Should().Be(0, "primary delegated; fallback may have accepted");
        fallback.DropCount.Should().Be(0);
        fallback.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Enqueue_BothUnhealthy_DropCountedAtTerminal()
    {
        var primary  = new CollectingSink();
        var fallback = new CollectingSink();
        primary.Next = fallback;
        primary.SetHealthy(false);
        fallback.SetHealthy(false);

        primary.Enqueue([1, 2, 3]);

        primary.DropCount.Should().Be(0, "primary forwarded to next");
        fallback.DropCount.Should().Be(1, "fallback dropped — terminal");
    }

    [Fact]
    public void Enqueue_AcceptReturnsFalse_DropCountedAtSink()
    {
        var sink = new RejectingSink();
        sink.Enqueue([1, 2, 3]);
        sink.DropCount.Should().Be(1, "Accept=false + no Next = drop");
    }

    private sealed class RejectingSink : PacketSink
    {
        public override bool IsHealthy => true;
        protected override bool Accept(ReadOnlySpan<byte> payload) => false;
        public override void Flush()   { }
        public override void Dispose() { }
    }
}
