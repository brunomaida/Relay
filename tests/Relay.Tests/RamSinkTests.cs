using System;
using System.Collections.Generic;
using FluentAssertions;
using Relay;
using Relay.Sinks;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class RamSinkTests
{
    [Fact]
    public void Accept_PayloadsBuffered_DrainToDeliversInOrder()
    {
        using var sink = new RamSink(capacity: 4_096);
        var target = new CollectingSink();
        byte[] a = [1, 2], b = [3, 4, 5], c = [6];

        sink.Enqueue(a);
        sink.Enqueue(b);
        sink.Enqueue(c);

        sink.DrainTo(target);

        target.Received.Should().HaveCount(3);
        target.Received[0].Should().Equal(a);
        target.Received[1].Should().Equal(b);
        target.Received[2].Should().Equal(c);
    }

    [Fact]
    public void Accept_ReturnsFalse_WhenBufferFull()
    {
        using var sink = new RamSink(capacity: 64);
        byte[] payload = new byte[28];

        bool first  = sink.IsHealthy;
        sink.Enqueue(payload);
        bool second = sink.IsHealthy;
        sink.Enqueue(payload);
        bool third  = sink.IsHealthy;

        first.Should().BeTrue();
        second.Should().BeTrue();
        third.Should().BeFalse("buffer is full after two 32-byte records in 64-byte capacity");
    }

    [Fact]
    public void DrainTo_UnhealthyTarget_StopsDrain()
    {
        using var sink = new RamSink(capacity: 4_096);
        var target = new CollectingSink();
        target.SetHealthy(false);

        sink.Enqueue([1]);
        sink.Enqueue([2]);

        sink.DrainTo(target);

        target.Received.Should().BeEmpty("drain stops immediately if target is unhealthy");
    }

    [Fact]
    public void DrainTo_CompletelyDrained_IsHealthyBecomesTrue()
    {
        using var sink = new RamSink(capacity: 64);
        byte[] payload = new byte[28];
        sink.Enqueue(payload);
        sink.Enqueue(payload);
        sink.IsHealthy.Should().BeFalse();

        sink.DrainTo(new CollectingSink());

        sink.IsHealthy.Should().BeTrue("buffer reset after full drain");
    }

    [Fact]
    public void Accept_PartialDrain_DoesNotFreeCapacity()
    {
        // Regression: fill-once contract. If DrainTo stops early (target unhealthy), _head
        // advances but _tail stays at capacity. Accept must still return false — writing at
        // _buffer + _tail with _tail ~= _capacity would overflow the buffer.
        using var sink = new RamSink(capacity: 64);
        byte[] payload = new byte[28];

        sink.Enqueue(payload);
        sink.Enqueue(payload);
        sink.IsHealthy.Should().BeFalse("buffer full after two 32B records in 64B capacity");

        var target = new OneShotCollectingSink();
        sink.DrainTo(target);

        target.Received.Should().HaveCount(1, "target stopped accepting after the first record");

        sink.IsHealthy.Should().BeFalse("partial drain does not free capacity");
    }

    private sealed class OneShotCollectingSink : PacketSink
    {
        private readonly List<byte[]> _received = new();
        private bool _healthy = true;

        public IReadOnlyList<byte[]> Received => _received;
        public override bool IsHealthy => _healthy;

        protected override bool Accept(ReadOnlySpan<byte> payload)
        {
            _received.Add(payload.ToArray());
            _healthy = false;
            return true;
        }

        public override void Flush()   { }
        public override void Dispose() { }
    }

    [Fact]
    public void Dispose_ReleasesNativeMemory_NoException()
    {
        var sink = new RamSink(capacity: 4_096);
        sink.Enqueue([1, 2, 3]);
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledTwice_IsIdempotent()
    {
        var sink = new RamSink(capacity: 4_096);
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }
}
