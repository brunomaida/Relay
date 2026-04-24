using System;
using FluentAssertions;
using Relay;
using Relay.Builder;
using Xunit;

namespace Relay.Tests;

/// <summary>Multi-broadcast delivery, partial failure, and CRTP variant.</summary>
public sealed class MultiPipeTests
{
    [Fact]
    public void Multi_DeliversToAllChildren()
    {
        var c1 = new CountingPipe();
        var c2 = new CountingPipe();
        var multi = new MultiPipe<Entry64>(c1, c2);

        multi.Enqueue(new Entry64 { A = 1 });
        multi.Enqueue(new Entry64 { A = 2 });

        c1.Accepted.Should().Be(2);
        c2.Accepted.Should().Be(2);
    }

    [Fact]
    public void Multi_PartialFailure_HealthyChildStillReceives()
    {
        var c1 = new CountingPipe(healthy: false);
        var c2 = new CountingPipe();
        var multi = new MultiPipe<Entry64>(c1, c2);

        multi.IsHealthy.Should().BeTrue(); // c2 is healthy
        multi.Enqueue(new Entry64 { A = 1 });

        c1.Accepted.Should().Be(0);
        c2.Accepted.Should().Be(1);
    }

    [Fact]
    public void Multi_AllFail_FallsToNext()
    {
        var c1   = new CountingPipe(healthy: false);
        var c2   = new CountingPipe(healthy: false);
        var next = new CountingPipe();
        var multi = RelayBuilder
            .Start<Entry64, MultiPipe<Entry64>>(new MultiPipe<Entry64>(c1, c2))
            .To(next)
            .Build();

        multi.Enqueue(new Entry64 { A = 1 });

        c1.Accepted.Should().Be(0);
        c2.Accepted.Should().Be(0);
        next.Accepted.Should().Be(1);
    }

    [Fact]
    public void Multi_RequiresAtLeastOneChild()
    {
        Action act = () => new MultiPipe<Entry64>();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Multi2_CrTP_DeliversToBothChildren()
    {
        var c1  = new CountingPipe();
        var c2  = new CountingPipe();
        var multi = new Multi2Pipe<Entry64, CountingPipe, CountingPipe>(c1, c2);

        multi.Enqueue(new Entry64 { A = 10 });

        c1.Accepted.Should().Be(1);
        c2.Accepted.Should().Be(1);
    }

    [Fact]
    public void Multi2_IsHealthy_TrueWhenEitherHealthy()
    {
        var c1  = new CountingPipe(healthy: false);
        var c2  = new CountingPipe();
        var multi = new Multi2Pipe<Entry64, CountingPipe, CountingPipe>(c1, c2);

        multi.IsHealthy.Should().BeTrue();
    }

    private sealed class CountingPipe : DispatchPipe<Entry64>
    {
        private readonly bool _healthy;
        public int Accepted { get; private set; }

        public CountingPipe(bool healthy = true) => _healthy = healthy;

        public override bool IsHealthy => _healthy;
        protected override bool Accept(in Entry64 item) { Accepted++; return true; }
        public override void Flush()   { }
        public override void Dispose() { }
    }
}
