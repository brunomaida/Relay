// Backward-compatibility tests for the [Obsolete] RamSink shims.
// Verifies that RamSink<T> and RamSink delegate correctly to MemorySink<T> and MemorySink.
#pragma warning disable CS0618 // Type or member is obsolete — intentional, testing shim

using System;
using FluentAssertions;
using Relay;
using Relay.Sinks;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

/// <summary>
/// Exercises the [Obsolete] <c>RamSink&lt;T&gt;</c> and <c>RamSink</c> shims.
/// All behaviour must be identical to <see cref="MemorySink{T}"/> and <see cref="MemorySink"/>.
/// </summary>
public sealed class RamSinkObsoleteTests
{
    // ---- RamSink (packet, non-generic) ----

    [Fact]
    public void RamSink_Packet_BuffersAndDrainsInOrder()
    {
        using var sink = new RamSink(capacity: 4_096);
        var target = new CollectingSink();
        byte[] a = [10, 20], b = [30, 40, 50];

        sink.Enqueue(a);
        sink.Enqueue(b);
        sink.DrainTo(target);

        target.Received.Should().HaveCount(2);
        target.Received[0].Should().Equal(a);
        target.Received[1].Should().Equal(b);
    }

    [Fact]
    public void RamSink_Packet_IsHealthy_FalseWhenFull()
    {
        using var sink = new RamSink(capacity: 64);
        byte[] payload = new byte[28];

        sink.IsHealthy.Should().BeTrue();
        sink.Enqueue(payload);
        sink.Enqueue(payload);
        sink.IsHealthy.Should().BeFalse("two 32B records fill a 64B buffer");
    }

    [Fact]
    public void RamSink_Packet_Dispose_IsIdempotent()
    {
        var sink = new RamSink(capacity: 4_096);
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void RamSink_Packet_IsInstanceOf_MemorySink()
    {
        using var sink = new RamSink(capacity: 4_096);
        sink.Should().BeAssignableTo<MemorySink>("shim must inherit from MemorySink");
    }

    // ---- RamSink<T> (generic, unmanaged) ----

    [Fact]
    public void RamSinkT_BuffersAndDrainsAllItems()
    {
        using var sink = new RamSink<Entry64>(capacity: 64);
        var dst = new CountingSink();

        sink.Enqueue(new Entry64 { A = 1 });
        sink.Enqueue(new Entry64 { A = 2 });
        sink.DrainTo(dst);

        dst.Accepted.Should().Be(2);
    }

    [Fact]
    public void RamSinkT_IsHealthy_FalseWhenFull()
    {
        using var sink = new RamSink<Entry64>(capacity: 4);
        for (int i = 0; i < 4; i++)
            sink.Enqueue(new Entry64 { A = i });

        sink.IsHealthy.Should().BeFalse("ring is full after capacity items");
    }

    [Fact]
    public void RamSinkT_Dispose_IsIdempotent()
    {
        var sink = new RamSink<Entry64>(capacity: 64);
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void RamSinkT_IsInstanceOf_MemorySinkT()
    {
        using var sink = new RamSink<Entry64>(capacity: 64);
        sink.Should().BeAssignableTo<MemorySink<Entry64>>("shim must inherit from MemorySink<T>");
    }

    private sealed class CountingSink : DispatchSink<Entry64>
    {
        private int _count;
        public int Accepted => _count;
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) { _count++; return true; }
        public override void Flush()   { }
        public override void Dispose() { }
    }
}
