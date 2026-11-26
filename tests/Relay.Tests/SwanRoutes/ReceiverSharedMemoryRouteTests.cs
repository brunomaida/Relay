using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Relay.Receivers;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Windows-only flow tests for the SWAN hot routes of Relay that enter at a receiver's <c>Poll</c> and cross a named
/// shared-memory ring (SWAN S5.5d). Split from <see cref="ReceiverRouteTests"/> so the class-level
/// <c>[SupportedOSPlatform("windows")]</c> (CA1416) does not cover the cross-platform receiver routes.
/// </summary>
[SupportedOSPlatform("windows")]
[Trait("Category", "Integration")]
public sealed class ReceiverSharedMemoryRouteTests
{
    private const int TimeoutMs = 5_000;

    /// <summary>
    /// Route <c>scan.in.app.out.relay.packetcallback.invoke.8006079c</c>:
    /// <c>NamedPipeReceiver.Poll &gt; PacketSink.Enqueue &gt; SharedMemorySpscSink.Accept &gt; SharedMemorySpscReceiver.Poll &gt; PacketCallback</c>.
    /// </summary>
    [WindowsFact]
    public async Task Poll_NamedPipeIntoSharedMemorySink_SharedMemoryReceiverCallbackGetsFrame()
    {
        byte[] payload  = [0x5A, 0x17, 0xC3, 0x01, 0x8E];
        var pipeName    = "relay-swan-pipe-" + Guid.NewGuid().ToString("N");
        var mmfName     = "relay-swan-shm-" + Guid.NewGuid().ToString("N");
        var received    = new FrameBox();
        using var shmSink      = new SharedMemorySpscSink(mmfName, totalCapacity: 4_096);
        using var shmReceiver  = new SharedMemorySpscReceiver<FrameBox>(mmfName, received,
                                     static (box, frame) => box.Frame = frame.ToArray());
        using var pipeReceiver = new NamedPipeReceiver<int>(pipeName, state: 0, static (_, _) => { }, next: shmSink);
        using var client       = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
        var connect = Task.Run(pipeReceiver.WaitForConnection);
        client.Connect(TimeoutMs);
        SpinWait.SpinUntil(() => connect.IsCompleted, TimeoutMs).Should().BeTrue("the pipe server must accept the client");
        await connect;
        var write = Task.Run(() => WriteFrame(client, payload));

        var poll = Task.Run(() => pipeReceiver.Poll());
        SpinWait.SpinUntil(() => poll.IsCompleted, TimeoutMs).Should().BeTrue("Poll returns once the client's frame is read");
        (await poll).Should().BeTrue();
        SpinWait.SpinUntil(() => write.IsCompleted, TimeoutMs).Should().BeTrue("the client's write completes once Poll read it");
        await write;
        shmReceiver.Poll().Should().BeTrue();

        received.Frame.Should().Equal(payload);
    }

    // Blocks until Poll reads: NamedPipeReceiver's server pipe has no in-buffer on Windows; no Flush (FlushFileBuffers waits too).
    private static void WriteFrame(Stream client, byte[] payload)
    {
        Span<byte> header = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);
        client.Write(header);
        client.Write(payload);
    }

    private sealed class FrameBox
    {
        public byte[]? Frame;
    }
}

/// <summary>A <see cref="FactAttribute"/> skipped off Windows (named MMF).</summary>
file sealed class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows()) Skip = "Windows only: named memory-mapped files.";
    }
}
