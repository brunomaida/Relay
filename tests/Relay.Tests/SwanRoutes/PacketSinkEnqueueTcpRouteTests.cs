using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay that enter at <see cref="PacketSink.Enqueue"/> and exit through
/// <see cref="TcpSink"/> to a loopback peer (SWAN S5.5d): each builds the chain its route's hops name, calls <c>Enqueue</c>
/// in its body, drives the chain to the exit and asserts the bytes the peer received in its body.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PacketSinkEnqueueTcpRouteTests
{
    private const int TimeoutMs         = 5_000;
    private const int RecoveryTimeoutMs = 10_000; // >= 3x TcpSink.MinBackoffMs; the first retry waits 2 s
    private const int NoDeadlineMs      = 60_000; // no deadline flush runs within a test

    /// <summary>
    /// Route <c>rl1</c>: <c>Enqueue &gt; SpscByteRingBuffer publish/consume &gt; ConsumeLoop &gt; TcpSink.WriteToBackend &gt; out.tcp</c>.
    /// The second frame does not fit the send buffer, so <c>WriteToBackend</c> sends the first; no deadline or <c>Stop</c> flush runs before the check.
    /// </summary>
    [Fact]
    public async Task Enqueue_TcpSinkFrameOverflowsSendBuffer_WriteToBackendSendsFirstFrame()
    {
        byte[] first  = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
        byte[] second = [0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18];
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        listener.Listen(1);
        using var sink = new TcpSink("127.0.0.1", Port(listener), sendBufferCapacity: 16, ringCapacity: 4_096,
                                     flushIntervalMs: NoDeadlineMs);
        var accept = Task.Run(listener.Accept);
        SpinWait.SpinUntil(() => accept.IsCompleted, TimeoutMs).Should().BeTrue("the sink connects in its constructor");
        using var peer = await accept;
        sink.Start();

        sink.Enqueue(first);
        sink.Enqueue(second);
        SpinWait.SpinUntil(() => peer.Available >= 4 + first.Length, TimeoutMs).Should().BeTrue("WriteToBackend sends the first frame when the second does not fit");
        Receive(peer, 4 + first.Length).Should().Equal(Frame(first));
        sink.Stop(drainTimeoutMs: 2_000);

        SpinWait.SpinUntil(() => peer.Available >= 4 + second.Length, TimeoutMs).Should().BeTrue("the consumer's final flush sends the second frame");
        Receive(peer, 4 + second.Length).Should().Equal(Frame(second));
    }

    /// <summary>
    /// Route <c>scan.in.app.out.tcp.f088d710</c>: the frame fits the send buffer, so only the consumer loop's
    /// <c>TcpSink.FlushBackend &gt; SendAll</c> sends it.
    /// </summary>
    [Fact]
    public async Task Enqueue_TcpSinkFrameFitsSendBuffer_ConsumerFlushSendsFrame()
    {
        byte[] payload = [0x5A, 0x17, 0xC3, 0x01, 0x8E, 0x42, 0x00, 0xFF];
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        listener.Listen(1);
        using var sink = new TcpSink("127.0.0.1", Port(listener), ringCapacity: 4_096, flushIntervalMs: 50);
        var accept = Task.Run(listener.Accept);
        SpinWait.SpinUntil(() => accept.IsCompleted, TimeoutMs).Should().BeTrue("the sink connects in its constructor");
        using var peer = await accept;
        sink.Start();

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 2_000);

        SpinWait.SpinUntil(() => peer.Available >= 4 + payload.Length, TimeoutMs).Should().BeTrue("the consumer loop flushes the frame");
        Receive(peer, 4 + payload.Length).Should().Equal(Frame(payload));
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.net.sockets.socket.connect.f74d0a9a</c>: the constructor's connect is refused (port bound,
    /// not listening), the listener starts, and <c>TryRecoverBackend &gt; ConnectSocket</c> reconnects; the frame reaches the peer
    /// only over that connection.
    /// </summary>
    [Fact]
    public async Task Enqueue_TcpSinkListenerRestored_RecoveryConnectDeliversFrame()
    {
        byte[] payload = [0x5A, 0x17, 0xC3, 0x01, 0x8E, 0x42, 0x00, 0xFF];
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        using var sink = new TcpSink("127.0.0.1", Port(listener), ringCapacity: 4_096, flushIntervalMs: 50);
        sink.IsHealthy.Should().BeFalse("the constructor's connect is refused while the port does not listen");
        sink.Start();
        listener.Listen(1);

        var accept = Task.Run(listener.Accept);
        SpinWait.SpinUntil(() => accept.IsCompleted, RecoveryTimeoutMs).Should().BeTrue("TryRecoverBackend must reconnect after the backoff");
        using var peer = await accept;
        SpinWait.SpinUntil(() => sink.IsHealthy, TimeoutMs).Should().BeTrue("ConnectSocket marks the sink healthy once connected");
        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 2_000);

        SpinWait.SpinUntil(() => peer.Available >= 4 + payload.Length, TimeoutMs).Should().BeTrue("the frame travels over the recovered connection");
        Receive(peer, 4 + payload.Length).Should().Equal(Frame(payload));
    }

    private static int Port(Socket listener) => ((IPEndPoint)listener.LocalEndPoint!).Port;

    private static byte[] Frame(byte[] payload)
    {
        var frame = new byte[4 + payload.Length];
        BinaryPrimitives.WriteUInt32BigEndian(frame, (uint)payload.Length);
        payload.CopyTo(frame, 4);
        return frame;
    }

    // Called once Available covers count, so Receive never blocks.
    private static byte[] Receive(Socket peer, int count)
    {
        var bytes = new byte[count];
        int got = 0, read;
        while (got < count && (read = peer.Receive(bytes.AsSpan(got))) > 0) got += read;
        return bytes;
    }
}
