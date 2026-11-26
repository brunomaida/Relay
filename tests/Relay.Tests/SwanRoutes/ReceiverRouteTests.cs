using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Relay.Builder;
using Relay.Receivers;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay whose first code hop is a receiver's <c>Poll</c> (SWAN S5.5d): each builds the
/// chain its route's hops name, calls <c>Poll</c> in its body, drives the chain to the exit and asserts the exit's effect in its body.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ReceiverRouteTests : IDisposable
{
    private const int TimeoutMs         = 5_000;
    private const int RecoveryTimeoutMs = 10_000; // >= 3x FileSink.MinBackoffMs; the first retry waits 2 s
    private const int BacklogFrames     = 1_500;  // 8 B records (12 000 B) fit the 16 384 B ring; Stop must land before the consumer empties it

    private readonly string _dir = Directory.CreateTempSubdirectory("relay-swan-").FullName;

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.filestream.write.ddd367eb</c>: frames fall through the unhealthy primary into the
    /// fallback's ring; after the primary recovers, the fallback's shutdown drains its backlog to <c>Prev</c>, and the primary's
    /// <c>DisposeBackend &gt; FlushToStream</c> lands it in the primary's file.
    /// </summary>
    [Fact]
    public async Task Poll_FileSinkFallbackBacklogAfterPrimaryRecovers_DrainsToPrimaryFile()
    {
        var pipeName     = "relay-swan-pipe-" + Guid.NewGuid().ToString("N");
        var primaryDir   = Path.Combine(_dir, "primary");
        var primaryPath  = Path.Combine(primaryDir, "primary.bin");
        var fallbackPath = Path.Combine(_dir, "fallback.bin");
        using var primary  = new FileSink(primaryPath, ringCapacity: 16_384, flushIntervalMs: 1_000);
        using var fallback = new FileSink(fallbackPath, writeBufferCapacity: 1, ringCapacity: 16_384, flushIntervalMs: 0);
        var chain = RelayBuilder.StartPacket(primary).To(fallback);
        using var receiver = new NamedPipeReceiver<int>(pipeName, state: 0, static (_, _) => { }, next: chain.Head);
        using var client   = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
        var connect = Task.Run(receiver.WaitForConnection);
        client.Connect(TimeoutMs);
        SpinWait.SpinUntil(() => connect.IsCompleted, TimeoutMs).Should().BeTrue("the pipe server must accept the client");
        await connect;
        var write = Task.Run(() => WriteFrames(client, BacklogFrames + 1));
        primary.Start();

        var probe = Task.Run(() => receiver.Poll());
        SpinWait.SpinUntil(() => probe.IsCompleted, TimeoutMs).Should().BeTrue("Poll returns once the client's first frame is read");
        (await probe).Should().BeTrue();
        SpinWait.SpinUntil(() => !primary.IsHealthy, TimeoutMs).Should().BeTrue("the primary's first open fails while its directory is missing");
        var backlog = Task.Run(() => { int n = 0; while (n < BacklogFrames && receiver.Poll()) n++; return n; });
        SpinWait.SpinUntil(() => backlog.IsCompleted, TimeoutMs).Should().BeTrue("Poll returns once each remaining frame is read");
        (await backlog).Should().Be(BacklogFrames);
        SpinWait.SpinUntil(() => write.IsCompleted, TimeoutMs).Should().BeTrue("the client's writes complete once Poll read them");
        await write;
        Directory.CreateDirectory(primaryDir);
        SpinWait.SpinUntil(() => primary.IsHealthy, RecoveryTimeoutMs).Should().BeTrue("the primary must recover after the backoff");
        fallback.Start();
        fallback.Stop(drainTimeoutMs: 2_000);
        primary.Stop(drainTimeoutMs: 2_000);

        byte[] drained = File.ReadAllBytes(primaryPath);
        drained.Should().NotBeEmpty("the fallback's backlog beyond one batch must drain to Prev");
        byte[] kept    = File.ReadAllBytes(fallbackPath);
        var delivered = new byte[kept.Length + drained.Length];
        kept.CopyTo(delivered, 0);
        drained.CopyTo(delivered, kept.Length);
        delivered.Should().Equal(Frames(first: 1, count: BacklogFrames));
    }

    // Blocks until Poll reads: NamedPipeReceiver's server pipe has no in-buffer on Windows; no Flush (FlushFileBuffers waits too).
    private static void WriteFrames(Stream client, int count)
    {
        Span<byte> header = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(header, sizeof(int));
        byte[] frames = Frames(first: 0, count);
        for (int i = 0; i < count; i++)
        {
            client.Write(header);
            client.Write(frames, i * sizeof(int), sizeof(int));
        }
    }

    private static byte[] Frames(int first, int count)
    {
        var bytes = new byte[count * sizeof(int)];
        for (int i = 0; i < count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * sizeof(int)), first + i);
        return bytes;
    }
}
