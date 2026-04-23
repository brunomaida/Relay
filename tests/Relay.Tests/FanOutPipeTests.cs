using System;
using FluentAssertions;
using Relay;
using Relay.Builder;
using Xunit;

namespace Relay.Tests;

/// <summary>FanOut delivery, partial failure, and CRTP variant.</summary>
public sealed class FanOutPipeTests
{
    [Fact]
    public void FanOut_DeliversToAllChildren()
    {
        var c1 = new CountingPipe();
        var c2 = new CountingPipe();
        var fan = new FanOutPipe<Entry64>(c1, c2);

        fan.Enqueue(new Entry64 { A = 1 });
        fan.Enqueue(new Entry64 { A = 2 });

        c1.Accepted.Should().Be(2);
        c2.Accepted.Should().Be(2);
    }

    [Fact]
    public void FanOut_PartialFailure_HealthyChildStillReceives()
    {
        var c1 = new CountingPipe(healthy: false);
        var c2 = new CountingPipe();
        var fan = new FanOutPipe<Entry64>(c1, c2);

        fan.IsHealthy.Should().BeTrue(); // c2 is healthy
        fan.Enqueue(new Entry64 { A = 1 });

        c1.Accepted.Should().Be(0);
        c2.Accepted.Should().Be(1);
    }

    [Fact]
    public void FanOut_AllFail_FallsToNext()
    {
        var c1   = new CountingPipe(healthy: false);
        var c2   = new CountingPipe(healthy: false);
        var next = new CountingPipe();
        var fan  = RelayBuilder
            .Start<Entry64, FanOutPipe<Entry64>>(new FanOutPipe<Entry64>(c1, c2))
            .To(next)
            .Build();

        fan.Enqueue(new Entry64 { A = 1 });

        c1.Accepted.Should().Be(0);
        c2.Accepted.Should().Be(0);
        next.Accepted.Should().Be(1);
    }

    [Fact]
    public void FanOut_RequiresAtLeastOneChild()
    {
        Action act = () => new FanOutPipe<Entry64>();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FanOut2_CrTP_DeliversToBothChildren()
    {
        var c1  = new CountingPipe();
        var c2  = new CountingPipe();
        var fan = new FanOut2Pipe<Entry64, CountingPipe, CountingPipe>(c1, c2);

        fan.Enqueue(new Entry64 { A = 10 });

        c1.Accepted.Should().Be(1);
        c2.Accepted.Should().Be(1);
    }

    [Fact]
    public void FanOut2_IsHealthy_TrueWhenEitherHealthy()
    {
        var c1  = new CountingPipe(healthy: false);
        var c2  = new CountingPipe();
        var fan = new FanOut2Pipe<Entry64, CountingPipe, CountingPipe>(c1, c2);

        fan.IsHealthy.Should().BeTrue();
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
