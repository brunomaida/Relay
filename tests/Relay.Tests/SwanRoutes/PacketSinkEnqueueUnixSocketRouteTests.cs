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
    private const int TimeoutMs = 5_000;

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
        peer.Receive(frame).Should().Be(frame.Length);
        BinaryPrimitives.ReadUInt32BigEndian(frame).Should().Be((uint)payload.Length);
        frame.AsSpan(4).ToArray().Should().Equal(payload);
    }
}
