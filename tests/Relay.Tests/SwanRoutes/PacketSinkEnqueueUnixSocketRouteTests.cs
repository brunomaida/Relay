using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay that enter at <see cref="PacketSink.Enqueue"/> and exit through
/// <see cref="UnixSocketSink"/> (SWAN S5.5d). Split from <see cref="PacketSinkEnqueueRouteTests"/> because
/// <see cref="UnixSocketSink"/> carries <c>[SupportedOSPlatform("linux"/"macos")]</c> (CA1416 at the call site).
/// </summary>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[Trait("Category", "Integration")]
public sealed class PacketSinkEnqueueUnixSocketRouteTests : IDisposable
{
    private const int TimeoutMs         = 5_000;
    private const int RecoveryTimeoutMs = 10_000; // >= 3x UnixSocketSink.MinBackoffMs; the first retry waits 2 s
    private const int NoDeadlineMs      = 60_000; // no deadline flush runs within a test

    private readonly string _dir = Directory.CreateTempSubdirectory("relay-swan-").FullName;

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    /// <summary>
    /// Route <c>rp25</c>: <c>Enqueue &gt; SpscByteRingBuffer publish/consume &gt; ConsumeLoop &gt; UnixSocketSink.WriteToBackend &gt; out.ipc</c>.
    /// The payload exceeds the send buffer, so <c>WriteToBackend</c> sends the frame itself.
    /// </summary>
    [Fact]
    public async Task Enqueue_UnixSocketSinkConsumer_PeerReceivesFramedPayload()
    {
        byte[] payload = new byte[256];
        for (int i = 0; i < payload.Length; i++) payload[i] = (byte)(i & 0xFF);
        var sockPath = Path.Combine(_dir, "rp25.sock");
        using var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        listener.Bind(new UnixDomainSocketEndPoint(sockPath));
        listener.Listen(1);
        using var sink = new UnixSocketSink(sockPath, sendBufferCapacity: 64, ringCapacity: 4_096, flushIntervalMs: 50);
        var accept = Task.Run(listener.Accept);
        SpinWait.SpinUntil(() => accept.IsCompleted, TimeoutMs).Should().BeTrue("the sink connects in its constructor");
        using var peer = await accept;
        sink.Start();

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 2_000);
        SpinWait.SpinUntil(() => peer.Available >= 4 + payload.Length, TimeoutMs).Should().BeTrue("the peer must receive the whole frame");

        var frame = new byte[4 + payload.Length];
        int got = 0, read;
        while (got < frame.Length && (read = peer.Receive(frame.AsSpan(got))) > 0) got += read;
        got.Should().Be(frame.Length);
        BinaryPrimitives.ReadUInt32BigEndian(frame).Should().Be((uint)payload.Length);
        frame.AsSpan(4).ToArray().Should().Equal(payload);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.ipc.2fee9b0f</c>: the second frame does not fit the send buffer, so
    /// <c>UnixSocketSink.WriteToBackend &gt; FlushBackend &gt; SendAll</c> sends the first; no deadline or <c>Stop</c> flush runs before the check.
    /// </summary>
    [Fact]
    public async Task Enqueue_UnixSocketSinkFrameOverflowsSendBuffer_WriteToBackendSendsFirstFrame()
    {
        byte[] first  = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
        byte[] second = [0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18];
        var sockPath = Path.Combine(_dir, "2fee9b0f.sock");
        using var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        listener.Bind(new UnixDomainSocketEndPoint(sockPath));
        listener.Listen(1);
        using var sink = new UnixSocketSink(sockPath, sendBufferCapacity: 16, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
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
    /// Route <c>scan.in.app.out.ipc.f978dc96</c>: the frame fits the send buffer, so only the consumer loop's
    /// <c>FlushBackend &gt; SendAll</c> sends it.
    /// </summary>
    [Fact]
    public async Task Enqueue_UnixSocketSinkFrameFitsSendBuffer_ConsumerFlushSendsFrame()
    {
        byte[] payload = [0x5A, 0x17, 0xC3, 0x01, 0x8E, 0x42, 0x00, 0xFF];
        var sockPath = Path.Combine(_dir, "f978dc96.sock");
        using var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        listener.Bind(new UnixDomainSocketEndPoint(sockPath));
        listener.Listen(1);
        using var sink = new UnixSocketSink(sockPath, ringCapacity: 4_096, flushIntervalMs: 50);
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
    /// Route <c>scan.in.app.out.system.net.sockets.socket.connect.42de01c1</c>: the constructor's connect fails (no listener),
    /// the listener is restored, and <c>TryRecoverBackend &gt; Connect</c> reconnects; the frame reaches the peer only over that connection.
    /// </summary>
    [Fact]
    public async Task Enqueue_UnixSocketSinkListenerRestored_RecoveryConnectDeliversFrame()
    {
        byte[] payload = [0x5A, 0x17, 0xC3, 0x01, 0x8E, 0x42, 0x00, 0xFF];
        var sockPath = Path.Combine(_dir, "42de01c1.sock");
        using var sink = new UnixSocketSink(sockPath, ringCapacity: 4_096, flushIntervalMs: 50);
        sink.IsHealthy.Should().BeFalse("the constructor's connect fails while no listener is bound");
        sink.Start();
        using var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        listener.Bind(new UnixDomainSocketEndPoint(sockPath));
        listener.Listen(1);

        var accept = Task.Run(listener.Accept);
        SpinWait.SpinUntil(() => accept.IsCompleted, RecoveryTimeoutMs).Should().BeTrue("TryRecoverBackend must reconnect after the backoff");
        using var peer = await accept;
        SpinWait.SpinUntil(() => sink.IsHealthy, TimeoutMs).Should().BeTrue("Connect marks the sink healthy once connected");
        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 2_000);

        SpinWait.SpinUntil(() => peer.Available >= 4 + payload.Length, TimeoutMs).Should().BeTrue("the frame travels over the recovered connection");
        Receive(peer, 4 + payload.Length).Should().Equal(Frame(payload));
    }

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
