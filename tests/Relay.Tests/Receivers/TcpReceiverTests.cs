using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Receivers;
using Xunit;

namespace Relay.Tests.Receivers;

public sealed class TcpReceiverTests
{
    // ── LocalEndPoint ─────────────────────────────────────────────────────────

    [Fact]
    public void TcpReceiver_LocalEndPoint_ReturnsActualPort()
    {
        using var recv = new TcpReceiver<int>(
            new IPEndPoint(IPAddress.Loopback, 0), state: 0,
            callback: static (_, _) => { });

        recv.LocalEndPoint.Port.Should().BeGreaterThan(0);
    }

    // ── Poll before Accept — no stream yet ────────────────────────────────────

    [Fact]
    public void TcpReceiver_Poll_ReturnsFalse_BeforeAccept()
    {
        using var recv = new TcpReceiver<int>(
            new IPEndPoint(IPAddress.Loopback, 0), state: 0,
            callback: static (_, _) => { });

        recv.Poll().Should().BeFalse("no stream before Accept()");
    }

    // ── Loopback round-trip ───────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void TcpReceiver_AcceptAndPoll_LoopbackRoundTrip()
    {
        byte[]? captured = null;
        using var recv = new TcpReceiver<Action<byte[]>>(
            new IPEndPoint(IPAddress.Loopback, 0),
            state: b => captured = b,
            callback: static (capture, frame) => capture(frame.ToArray()));

        int port = recv.LocalEndPoint.Port;

        // Connect sender on background thread; receiver blocks in Accept()
        var senderThread = new Thread(() =>
        {
            Thread.Sleep(20); // small delay so Accept() is waiting first
            using var client = new TcpClient();
            client.Connect(IPAddress.Loopback, port);
            using var stream = client.GetStream();

            byte[] payload = { 0xAB, 0xCD };
            // Write [4B BE length][payload]
            Span<byte> header = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);
            stream.Write(header);
            stream.Write(payload);
            stream.Flush();
            Thread.Sleep(50); // keep connection alive until Poll
        });
        senderThread.Start();

        recv.Accept(); // blocks until sender connects
        Thread.Sleep(30); // let data arrive
        recv.Poll().Should().BeTrue();
        captured.Should().BeEquivalentTo(new byte[] { 0xAB, 0xCD });

        senderThread.Join(millisecondsTimeout: 2000);
    }
}
