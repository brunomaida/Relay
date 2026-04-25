using System;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class ForkSinkPacketTests
{
    private static readonly byte[] Payload = [10, 20, 30];

    [Fact]
    public void Enqueue_PrimaryHealthy_PayloadReachesBothPrimaryAndNext()
    {
        var primary = new CollectingSink();
        var next    = new CollectingSink();
        var fork    = new ForkSink(primary);
        fork.Next   = next;

        fork.Enqueue(Payload);

        primary.Received.Should().HaveCount(1);
        next.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Enqueue_PrimaryUnhealthy_OnlyNextReceivesPayload()
    {
        var primary = new CollectingSink();
        var next    = new CollectingSink();
        var fork    = new ForkSink(primary);
        fork.Next   = next;
        primary.SetHealthy(false);

        fork.Enqueue(Payload);

        primary.Received.Should().BeEmpty("Accept not called on unhealthy primary via ForkSink");
        next.Received.Should().HaveCount(1);
    }

    [Fact]
    public void IsHealthy_MirrorsPrimary()
    {
        var primary = new CollectingSink();
        var fork    = new ForkSink(primary);

        fork.IsHealthy.Should().BeTrue();
        primary.SetHealthy(false);
        fork.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public void Flush_PropagatesTo_PrimaryAndNext()
    {
        var primary = new FlushTrackingSink();
        var next    = new FlushTrackingSink();
        var fork    = new ForkSink(primary);
        fork.Next   = next;

        fork.Flush();

        primary.Flushed.Should().BeTrue();
        next.Flushed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_PropagatesTo_PrimaryAndNext()
    {
        var primary = new FlushTrackingSink();
        var next    = new FlushTrackingSink();
        var fork    = new ForkSink(primary);
        fork.Next   = next;

        fork.Dispose();

        primary.Disposed.Should().BeTrue();
        next.Disposed.Should().BeTrue();
    }

    private sealed class FlushTrackingSink : PacketSink
    {
        public bool Flushed  { get; private set; }
        public bool Disposed { get; private set; }
        public override bool IsHealthy => true;
        protected override bool Accept(ReadOnlySpan<byte> payload) => true;
        public override void Flush()   => Flushed  = true;
        public override void Dispose() => Disposed = true;
    }
}
