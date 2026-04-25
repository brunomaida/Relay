using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests;

public sealed class UdpSinkTests : IDisposable
{
    private readonly UdpClient _receiver;
    private readonly int       _port;

    public UdpSinkTests()
    {
        _receiver = new UdpClient(0);
        _port     = ((IPEndPoint)_receiver.Client.LocalEndPoint!).Port;
    }

    public void Dispose() => _receiver.Dispose();

    [Fact]
    public void Enqueue_PayloadDeliveredAsDatagram_NoLengthPrefix()
    {
        using var sink = new UdpSink("127.0.0.1", _port, ringCapacity: 4_096, flushIntervalMs: 50);
        sink.Start();
        byte[] payload = [1, 2, 3, 4, 5];

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 1_000);

        _receiver.Client.Poll(2_000_000, SelectMode.SelectRead).Should().BeTrue();
        var ep = new IPEndPoint(IPAddress.Any, 0);
        byte[] received = _receiver.Receive(ref ep);
        received.Should().Equal(payload);
    }

    [Fact]
    public void Enqueue_PayloadExceedsMaxPayload_SetUnhealthy()
    {
        using var sink = new UdpSink("127.0.0.1", _port, maxPayload: 4,
                                      ringCapacity: 4_096, flushIntervalMs: 50);
        sink.Start();
        Thread.Sleep(100);

        sink.Enqueue(new byte[5]);
        Thread.Sleep(200);

        sink.IsHealthy.Should().BeFalse();
        sink.Stop();
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var sink = new UdpSink("127.0.0.1", _port);
        sink.Start();
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }
}
