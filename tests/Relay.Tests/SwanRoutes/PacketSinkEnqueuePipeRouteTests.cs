using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay that enter at <see cref="PacketSink.Enqueue"/> and exit through
/// <see cref="NamedPipeSink"/> to a local pipe server (SWAN S5.5d): each builds the chain its route's hops name, calls
/// <c>Enqueue</c> in its body, drives the chain to the exit and asserts the bytes the server read in its body.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PacketSinkEnqueuePipeRouteTests
{
    private const int TimeoutMs    = 5_000;
    private const int NoDeadlineMs = 60_000; // no deadline flush runs within a test

    /// <summary>
    /// Route <c>rl5</c>: <c>Enqueue &gt; SpscByteRingBuffer publish/consume &gt; ConsumeLoop &gt; NamedPipeSink.WriteToBackend &gt; out.ipc</c>.
    /// The frame exceeds the send buffer, so <c>WriteToBackend</c> writes it itself; no flush runs before the check.
    /// </summary>
    [Fact]
    public async Task Enqueue_NamedPipeSinkFrameExceedsSendBuffer_WriteToBackendWritesFrame()
    {
        byte[] payload = new byte[256];
        for (int i = 0; i < payload.Length; i++) payload[i] = (byte)(i & 0xFF);
        var pipeName = "relay-swan-pipe-" + Guid.NewGuid().ToString("N");
        using var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None);
        var connect = Task.Run(server.WaitForConnection);
        using var sink = new NamedPipeSink(pipeName, sendBufferCapacity: 64, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        SpinWait.SpinUntil(() => connect.IsCompleted, TimeoutMs).Should().BeTrue("the sink connects in its constructor");
        await connect;
        sink.Start();

        var read = Task.Run(() => ReadExactly(server, 4 + payload.Length));
        sink.Enqueue(payload);
        SpinWait.SpinUntil(() => read.IsCompleted, TimeoutMs).Should().BeTrue("WriteToBackend writes the oversized frame directly");
        (await read).Should().Equal(Frame(payload));
        sink.Stop(drainTimeoutMs: 2_000);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.pipes.pipestream.write.ce87e2b8</c>: the second frame does not fit the send buffer, so
    /// <c>NamedPipeSink.WriteToBackend &gt; FlushBackend</c> writes the first; no deadline or <c>Stop</c> flush runs before the check.
    /// </summary>
    [Fact]
    public async Task Enqueue_NamedPipeSinkFrameOverflowsSendBuffer_WriteToBackendFlushesFirstFrame()
    {
        byte[] first  = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
        byte[] second = [0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18];
        var pipeName = "relay-swan-pipe-" + Guid.NewGuid().ToString("N");
        using var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None);
        var connect = Task.Run(server.WaitForConnection);
        using var sink = new NamedPipeSink(pipeName, sendBufferCapacity: 16, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        SpinWait.SpinUntil(() => connect.IsCompleted, TimeoutMs).Should().BeTrue("the sink connects in its constructor");
        await connect;
        sink.Start();

        var readFirst = Task.Run(() => ReadExactly(server, 4 + first.Length));
        sink.Enqueue(first);
        sink.Enqueue(second);
        SpinWait.SpinUntil(() => readFirst.IsCompleted, TimeoutMs).Should().BeTrue("WriteToBackend flushes the first frame when the second does not fit");
        (await readFirst).Should().Equal(Frame(first));
        var readSecond = Task.Run(() => ReadExactly(server, 4 + second.Length));
        sink.Stop(drainTimeoutMs: 2_000);

        SpinWait.SpinUntil(() => readSecond.IsCompleted, TimeoutMs).Should().BeTrue("the consumer's final flush writes the second frame");
        (await readSecond).Should().Equal(Frame(second));
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.pipes.pipestream.write.3763d85d</c>: the frame fits the send buffer, so only the
    /// consumer loop's <c>NamedPipeSink.FlushBackend</c> writes it.
    /// </summary>
    [Fact]
    public async Task Enqueue_NamedPipeSinkFrameFitsSendBuffer_ConsumerFlushWritesFrame()
    {
        byte[] payload = [0x5A, 0x17, 0xC3, 0x01, 0x8E, 0x42, 0x00, 0xFF];
        var pipeName = "relay-swan-pipe-" + Guid.NewGuid().ToString("N");
        using var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None);
        var connect = Task.Run(server.WaitForConnection);
        using var sink = new NamedPipeSink(pipeName, ringCapacity: 4_096, flushIntervalMs: 50);
        SpinWait.SpinUntil(() => connect.IsCompleted, TimeoutMs).Should().BeTrue("the sink connects in its constructor");
        await connect;
        sink.Start();

        var read = Task.Run(() => ReadExactly(server, 4 + payload.Length));
        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 2_000);

        SpinWait.SpinUntil(() => read.IsCompleted, TimeoutMs).Should().BeTrue("the consumer loop flushes the frame");
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
