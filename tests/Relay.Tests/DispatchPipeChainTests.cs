using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Relay;
using Relay.Builder;
using Xunit;

namespace Relay.Tests;

[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct Entry64 { public long A; public long B; }

/// <summary>Chain routing and fallback behaviour.</summary>
public sealed class DispatchPipeChainTests
{
    [Fact]
    public void NullPipe_AlwaysAccepts()
    {
        var pipe = NullPipe<Entry64>.Instance;
        pipe.IsHealthy.Should().BeTrue();
        pipe.Enqueue(new Entry64 { A = 1 }); // no throw, no fallback
    }

    [Fact]
    public void Serial_WhenHeadHealthy_TailNeverReceivesItem()
    {
        var head = new CountingPipe();
        var tail = new CountingPipe();
        RelayBuilder.Start<Entry64, CountingPipe>(head).To(tail).Build();

        head.Enqueue(new Entry64 { A = 42 });

        head.Accepted.Should().Be(1);
        tail.Accepted.Should().Be(0);
    }

    [Fact]
    public void Serial_WhenHeadUnhealthy_TailReceivesItem()
    {
        var head = new CountingPipe(healthy: false);
        var tail = new CountingPipe();
        RelayBuilder.Start<Entry64, CountingPipe>(head).To(tail).Build();

        head.Enqueue(new Entry64 { A = 42 });

        head.Accepted.Should().Be(0);
        tail.Accepted.Should().Be(1);
    }

    [Fact]
    public void Serial_WhenAllUnhealthy_ItemDropped()
    {
        var p1 = new CountingPipe(healthy: false);
        var p2 = new CountingPipe(healthy: false);
        RelayBuilder.Start<Entry64, CountingPipe>(p1).To(p2).Build();

        p1.Enqueue(new Entry64 { A = 1 });

        p1.Accepted.Should().Be(0);
        p2.Accepted.Should().Be(0);
    }

    [Fact]
    public void Serial_Depth3_FallsThrough()
    {
        var p1 = new CountingPipe(healthy: false);
        var p2 = new CountingPipe(healthy: false);
        var p3 = new CountingPipe();
        RelayBuilder.Start<Entry64, CountingPipe>(p1).To(p2).To(p3).Build();

        p1.Enqueue(new Entry64 { A = 1 });

        p1.Accepted.Should().Be(0);
        p2.Accepted.Should().Be(0);
        p3.Accepted.Should().Be(1);
    }

    [Fact]
    public void Builder_WiresNextPointers()
    {
        var p1 = new CountingPipe();
        var p2 = new CountingPipe();
        RelayBuilder.Start<Entry64, CountingPipe>(p1).To(p2).Build();

        p1.Next.Should().BeSameAs(p2);
        p2.Next.Should().BeNull();
    }

    // Minimal testing pipe — synchronous, in-memory.
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
