using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Receivers;
using Xunit;

namespace Relay.Tests.Receivers;

public sealed class UdpReceiverTests
{
    // ── LocalEndPoint ─────────────────────────────────────────────────────────

    [Fact]
    public void UdpReceiver_LocalEndPoint_ReturnsActualPort()
    {
        using var recv = new UdpReceiver<int>(
            new IPEndPoint(IPAddress.Loopback, 0), state: 0,
            callback: static (_, _) => { });

        recv.LocalEndPoint.Port.Should().BeGreaterThan(0);
    }

    // ── Poll — no data ────────────────────────────────────────────────────────

    [Fact]
    public void UdpReceiver_Poll_ReturnsFalse_WhenNoData()
    {
        using var recv = new UdpReceiver<int>(
            new IPEndPoint(IPAddress.Loopback, 0), state: 0,
            callback: static (_, _) => { });

        recv.Poll().Should().BeFalse();
    }

    // ── Loopback round-trip ───────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void UdpReceiver_Poll_InvokesCallback_LoopbackRoundTrip()
    {
        byte[]? captured = null;
        using var recv = new UdpReceiver<Action<byte[]>>(
            new IPEndPoint(IPAddress.Loopback, 0),
            state: b => captured = b,
            callback: static (capture, frame) => capture(frame.ToArray()));

        using var sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        byte[] payload = { 0xDE, 0xAD, 0xBE, 0xEF };
        sender.SendTo(payload, recv.LocalEndPoint);

        Thread.Sleep(10);
        recv.Poll().Should().BeTrue();
        captured.Should().BeEquivalentTo(payload);
    }

    // ── Forward to next sink ──────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void UdpReceiver_Poll_ForwardsToNextSink_WhenChained()
    {
        var captured = new System.Collections.Generic.List<byte[]>();
        var spy = new SpyPacketSink(b => captured.Add(b.ToArray()));

        using var recv = new UdpReceiver<int>(
            new IPEndPoint(IPAddress.Loopback, 0),
            state: 0,
            callback: static (_, _) => { },
            next: spy);

        using var sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        byte[] payload = { 0x01, 0x02 };
        sender.SendTo(payload, recv.LocalEndPoint);

        Thread.Sleep(10);
        recv.Poll();

        captured.Should().HaveCount(1);
        captured[0].Should().BeEquivalentTo(payload);
    }
}
