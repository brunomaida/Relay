using System;
using FluentAssertions;
using Relay.Builder;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class SinkChainBuilderTests
{
    private static readonly byte[] Payload = [1, 2, 3];

    [Fact]
    public void To_WiresNextCorrectly()
    {
        var primary  = new CollectingSink();
        var fallback = new CollectingSink();

        SinkChainBuilder.Start(primary).To(fallback);
        primary.SetHealthy(false);
        primary.Enqueue(Payload);

        fallback.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Fork_InsertsForkSinkBetweenHeadAndNext()
    {
        var audit  = new CollectingSink();
        var next   = new CollectingSink();
        var head   = new CollectingSink();

        SinkChainBuilder.Start(head).Fork(audit).To(next);
        head.Enqueue(Payload);

        audit.Received.Should().HaveCount(1);
        next.Received.Should().HaveCount(1);
    }

    [Fact]
    public void When_To_InsertsFilterSink()
    {
        var downstream = new CollectingSink();
        var head       = new CollectingSink();

        SinkChainBuilder.Start(head)
            .When(p => p.Length > 0 && p[0] == 0xFF)
            .To(downstream);

        head.Next!.Enqueue([0xFF, 1]);
        head.Next!.Enqueue([0x00, 1]);

        downstream.Received.Should().HaveCount(1);
        downstream.Received[0][0].Should().Be(0xFF);
    }

    [Fact]
    public void Multi_InsertsBroadcastSink()
    {
        var a    = new CollectingSink();
        var b    = new CollectingSink();
        var head = new CollectingSink();
        head.SetHealthy(false);

        SinkChainBuilder.Start(head).Multi(a, b);
        head.Enqueue(Payload);

        a.Received.Should().HaveCount(1);
        b.Received.Should().HaveCount(1);
    }

    [Fact]
    public void ImplicitOperator_ReturnsHeadAsPacketSink()
    {
        var head  = new CollectingSink();
        PacketSink sink = SinkChainBuilder.Start(head);

        sink.Should().BeSameAs(head);
    }

    [Fact]
    public void When_To_To_ChainContinuesFromDownstream_FilterNotOverwritten()
    {
        // Regression: FilterBinding.To must advance the chain tail to downstream, so a
        // subsequent .To(b) appends b to downstream (not to the attach point, which would
        // overwrite the filter).
        var head       = new CollectingSink();
        var downstream = new CollectingSink();
        var b          = new CollectingSink();

        SinkChainBuilder.Start(head)
            .When(p => p.Length > 0 && p[0] == 0xFF)
            .To(downstream)
            .To(b);

        head.Next.Should().BeOfType<FilterSink>();
        downstream.Next.Should().BeSameAs(b);
    }
}
