using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Relay.Receivers;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay whose first code hop is <see cref="TcpReceiver{TState}.Poll"/> (SWAN S5.5d):
/// each builds the chain its route's hops name, calls <c>Poll</c> in its body, drives the chain to the exit and asserts the
/// exit's effect in its body.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ReceiverTcpRouteTests
{
    private const int TimeoutMs         = 5_000;
    private const int RecoveryTimeoutMs = 10_000; // >= 3x NamedPipeSink.MinBackoffMs; the first retry waits 2 s

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.pipes.namedpipeclientstream.connect.cc2fb93e</c>:
    /// <c>TcpReceiver.Poll &gt; PacketSink.Enqueue &gt; SpscQueueSink.Accept &gt; SpscByteRingBuffer publish/consume &gt;
    /// ConsumeLoop &gt; NamedPipeSink.TryRecoverBackend &gt; ConnectClient</c>. The constructor's connect fails (no server yet),
    /// the server starts, and the consumer's deadline <c>TryRecoverBackend &gt; ConnectClient</c> connects; the server's accept
    /// and the frame it reads happen only over that connection.
    /// </summary>
    [Fact]
    public async Task Poll_NamedPipeSinkServerRestored_RecoveryConnectDeliversFrame()
    {
        byte[] payload = [0x5A, 0x17, 0xC3, 0x01, 0x8E, 0x42, 0x00, 0xFF];
        var pipeName = "relay-swan-pipe-" + Guid.NewGuid().ToString("N");
        using var sink = new NamedPipeSink(pipeName, ringCapacity: 4_096, flushIntervalMs: 50);
        sink.IsHealthy.Should().BeFalse("the constructor's connect fails while no pipe server exists");
        using var server   = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None);
        var connect = Task.Run(server.WaitForConnection);
        using var receiver = new TcpReceiver<int>(new IPEndPoint(IPAddress.Loopback, 0), state: 0, static (_, _) => { }, next: sink);
        using var client   = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(receiver.LocalEndPoint);
        var accept = Task.Run(receiver.Accept);
        SpinWait.SpinUntil(() => accept.IsCompleted, TimeoutMs).Should().BeTrue("the receiver accepts the queued client");
        await accept;
        sink.Start();

        SpinWait.SpinUntil(() => connect.IsCompleted, RecoveryTimeoutMs).Should().BeTrue("TryRecoverBackend must connect after the backoff");
        await connect;
        SpinWait.SpinUntil(() => sink.IsHealthy, TimeoutMs).Should().BeTrue("ConnectClient marks the sink healthy once connected");
        var read = Task.Run(() => ReadExactly(server, 4 + payload.Length));
        client.Send(Frame(payload));
        SpinWait.SpinUntil(() => receiver.Poll(), TimeoutMs).Should().BeTrue("Poll returns once the frame arrives");
        sink.Stop(drainTimeoutMs: 2_000);

        SpinWait.SpinUntil(() => read.IsCompleted, TimeoutMs).Should().BeTrue("the frame travels over the recovered pipe connection");
        (await read).Should().Equal(Frame(payload));
    }

    private static byte[] Frame(byte[] payload)
    {
        var frame = new byte[4 + payload.Length];
        BinaryPrimitives.WriteUInt32BigEndian(frame, (uint)payload.Length);
        payload.CopyTo(frame, 4);
        return frame;
    }

    // Blocks until count bytes arrive; callers run it on Task.Run under a bounded wait.
    private static byte[] ReadExactly(Stream server, int count)
    {
        var bytes = new byte[count];
        server.ReadExactly(bytes);
        return bytes;
    }
}
