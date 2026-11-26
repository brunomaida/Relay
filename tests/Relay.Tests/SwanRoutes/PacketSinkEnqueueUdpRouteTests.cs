using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay that enter at <see cref="PacketSink.Enqueue"/> and exit through
/// <see cref="UdpSink"/> (SWAN S5.5d): each builds the chain its route's hops name, calls <c>Enqueue</c> in its body,
/// drives the chain to the exit and asserts the datagram the loopback peer received in its body.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PacketSinkEnqueueUdpRouteTests
{
    private const int TimeoutMs         = 5_000;
    private const int RecoveryTimeoutMs = 10_000; // >= 3x UdpSink.MinBackoffMs; the first retry waits 1 s

    /// <summary>
    /// Route <c>scan.in.app.out.system.net.sockets.socket.connect.7bdfa44c</c>: an oversized payload marks the sink unhealthy,
    /// and <c>TryRecoverBackend &gt; CreateSocket</c> (the only writer of <c>_healthy = true</c> in <see cref="UdpSink"/>)
    /// restores it; the next datagram goes out on the recreated socket.
    /// </summary>
    [Fact]
    public void Enqueue_UdpSinkOversizedPayload_RecoveryCreatesSocketAndDeliversNextDatagram()
    {
        byte[] oversized = new byte[16];
        byte[] payload   = [0x5A, 0x17, 0xC3, 0x01];
        using var peer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        peer.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        using var sink = new UdpSink("127.0.0.1", ((IPEndPoint)peer.LocalEndPoint!).Port, maxPayload: 8,
                                     ringCapacity: 4_096, flushIntervalMs: 50);
        sink.Start();

        sink.Enqueue(oversized);
        SpinWait.SpinUntil(() => !sink.IsHealthy, TimeoutMs).Should().BeTrue("a payload above maxPayload marks the sink unhealthy");
        SpinWait.SpinUntil(() => sink.IsHealthy, RecoveryTimeoutMs).Should().BeTrue("TryRecoverBackend must recreate the socket after the backoff");
        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 1_000);

        SpinWait.SpinUntil(() => peer.Available > 0, TimeoutMs).Should().BeTrue("the datagram travels over the recreated socket");
        var datagram = new byte[64];
        int length = peer.Receive(datagram);
        datagram.AsSpan(0, length).ToArray().Should().Equal(payload, "the oversized payload is never sent");
    }
}
