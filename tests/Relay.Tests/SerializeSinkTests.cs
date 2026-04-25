using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class SerializeSinkTests
{
    [StructLayout(LayoutKind.Sequential, Size = 64)]
    private struct Event64 { public long Value; }

    private sealed class NoAllocCollectingSink : PacketSink
    {
        public int AcceptCount;
        public override bool IsHealthy => true;
        protected override bool Accept(ReadOnlySpan<byte> payload) { AcceptCount++; return true; }
        public override void Flush() { }
        public override void Dispose() { }
    }

    [Fact]
    public void Accept_ConvertsStructToBytes_CorrectLength()
    {
        var downstream  = new CollectingSink();
        var serialize   = new SerializeSink<Event64>(downstream);
        var item        = new Event64 { Value = unchecked((long)0xDEAD_BEEF_CAFE_1234UL) };

        serialize.Enqueue(in item);

        downstream.Received.Should().HaveCount(1);
        downstream.Received[0].Should().HaveCount(64);
    }

    [Fact]
    public void Accept_ConvertsStructToBytes_CorrectContent()
    {
        var downstream = new CollectingSink();
        var serialize  = new SerializeSink<Event64>(downstream);
        var item       = new Event64 { Value = 42L };

        serialize.Enqueue(in item);

        byte[] expected = new byte[64];
        MemoryMarshal.Write(expected.AsSpan(), (long)42L);
        downstream.Received[0].Should().Equal(expected);
    }

    [Fact]
    public void IsHealthy_MirrorsTarget()
    {
        var downstream = new CollectingSink();
        var serialize  = new SerializeSink<Event64>(downstream);

        serialize.IsHealthy.Should().BeTrue();
        downstream.SetHealthy(false);
        serialize.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public void Accept_ZeroAllocation_VerifiedViaGcMetrics()
    {
        var downstream = new NoAllocCollectingSink();
        var serialize  = new SerializeSink<Event64>(downstream);
        var item       = new Event64 { Value = 1L };

        serialize.Enqueue(in item); // warm JIT

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1_000; i++)
            serialize.Enqueue(in item);
        long after = GC.GetAllocatedBytesForCurrentThread();

        (after - before).Should().Be(0, "SerializeSink.Accept must not allocate");
        downstream.AcceptCount.Should().Be(1_001);
    }
}
