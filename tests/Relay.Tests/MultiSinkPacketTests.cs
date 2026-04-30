using System;
using FluentAssertions;
using Relay.Builder;
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

    [Fact]
    public void Multi2_Enqueue_DeliversToBothChildren()
    {
        var a = new CollectingSink();
        var b = new CollectingSink();
        var multi2 = new Multi2PacketSink<CollectingSink, CollectingSink>(a, b);

        multi2.Enqueue(Payload);

        a.Received.Should().HaveCount(1);
        b.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Multi2_IsHealthy_True_WhenAtLeastOneChildHealthy()
    {
        var healthy   = new CollectingSink();
        var unhealthy = new CollectingSink();
        unhealthy.SetHealthy(false);
        var multi2 = new Multi2PacketSink<CollectingSink, CollectingSink>(healthy, unhealthy);

        multi2.IsHealthy.Should().BeTrue();
    }

    [Fact]
    public void Multi2_IsHealthy_False_WhenBothChildrenUnhealthy()
    {
        var a = new CollectingSink();
        var b = new CollectingSink();
        a.SetHealthy(false);
        b.SetHealthy(false);
        var multi2 = new Multi2PacketSink<CollectingSink, CollectingSink>(a, b);

        multi2.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public void Multi2_Flush_ForwardsToBoth()
    {
        var a = new CollectingSink();
        var b = new CollectingSink();
        var multi2 = new Multi2PacketSink<CollectingSink, CollectingSink>(a, b);

        multi2.Flush();

        a.Flushes.Should().Be(1);
        b.Flushes.Should().Be(1);
    }

    [Fact]
    public void Multi2_Dispose_ForwardsToBoth()
    {
        var a = new CollectingSink();
        var b = new CollectingSink();
        var multi2 = new Multi2PacketSink<CollectingSink, CollectingSink>(a, b);

        multi2.Dispose();

        a.Disposed.Should().BeTrue();
        b.Disposed.Should().BeTrue();
    }

    [Fact]
    public void Multi2_Ctor_Throws_OnNullChild()
    {
        var ok = new CollectingSink();
        Action a1 = () => new Multi2PacketSink<CollectingSink, CollectingSink>(null!, ok);
        Action a2 = () => new Multi2PacketSink<CollectingSink, CollectingSink>(ok, null!);
        a1.Should().Throw<ArgumentNullException>();
        a2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Builder_Multi_TC1_TC2_WiresChain()
    {
        // The .Multi<TC1,TC2>(c1,c2) overload installs Multi2PacketSink as Next of NullSink.
        // NullSink.Accept always returns true, so payloads never reach the multi under default
        // PropagateAfterAccept=false — this test asserts the builder did not throw and returned a
        // chain whose head is the original NullSink.Instance.
        var a    = new CollectingSink();
        var b    = new CollectingSink();
        var head = RelayBuilder
            .StartPacket(NullSink.Instance)
            .Multi(a, b)
            .Head;

        head.Should().BeSameAs(NullSink.Instance);
        head.Next.Should().BeOfType<Multi2PacketSink<CollectingSink, CollectingSink>>();
    }
}
