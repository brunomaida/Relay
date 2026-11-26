using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay whose first code hop is <see cref="PacketSink.TryEnqueue"/> and whose exit is
/// <c>FileSink.WriteToBackend</c>'s direct write (SWAN S5.5d): each builds the chain its route's hops name, calls <c>TryEnqueue</c>
/// in its body, drains the chain and asserts the file's bytes in its body. Every <see cref="FileSink"/> has a one-byte write buffer,
/// so each payload takes the oversized branch (<c>FileSink.cs:63</c>) and <c>FlushToStream</c> never holds bytes.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PacketSinkTryEnqueueRouteTests : IDisposable
{
    private const int TimeoutMs    = 5_000;
    private const int NoDeadlineMs = 60_000; // no deadline flush runs within a test

    private readonly string _dir = Directory.CreateTempSubdirectory("relay-swan-").FullName;

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    /// <summary>
    /// Route <c>scan.in.relay.packetsink.tryenqueue.system.readonlyspan-system.byte-.out.system.io.filestream.write.1ef02571</c>:
    /// <c>PacketSink.TryEnqueue &gt; MultiSink.Accept &gt; PacketSink.Enqueue &gt; SpscQueueSink.Accept &gt; ConsumeLoop &gt; FileSink.WriteToBackend</c>.
    /// </summary>
    [Fact]
    public void TryEnqueue_MultiSinkIntoFileSink_WritesPayloadToFile()
    {
        byte[] payload = [0x5A, 0x17, 0xC3, 0x01];
        var path = Path.Combine(_dir, "multi.bin");
        using var file  = new FileSink(path, writeBufferCapacity: 1, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        using var multi = new MultiSink(file);
        file.Start();

        multi.TryEnqueue(payload).Should().BeTrue("the child is healthy, so MultiSink accepts");
        file.Stop(drainTimeoutMs: 1_000);

        SpinWait.SpinUntil(() => ReadShared(path).AsSpan().SequenceEqual(payload), TimeoutMs).Should().BeTrue("the child's WriteToBackend writes the payload");
        ReadShared(path).Should().Equal(payload);
    }

    /// <summary>
    /// Route <c>scan.in.relay.packetsink.tryenqueue.system.readonlyspan-system.byte-.out.system.io.filestream.write.2f7ed61b</c>:
    /// <c>PacketSink.TryEnqueue &gt; ForkSink.Accept &gt; PacketSink.Enqueue &gt; SpscQueueSink.Accept &gt; ConsumeLoop &gt; FileSink.WriteToBackend</c>.
    /// </summary>
    [Fact]
    public void TryEnqueue_ForkSinkIntoFileSink_WritesPayloadToFile()
    {
        byte[] payload = [0x5A, 0x17, 0xC3, 0x02];
        var path = Path.Combine(_dir, "fork.bin");
        using var file = new FileSink(path, writeBufferCapacity: 1, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        using var fork = new ForkSink(file);
        file.Start();

        fork.TryEnqueue(payload).Should().BeTrue("the primary is healthy, so ForkSink accepts");
        file.Stop(drainTimeoutMs: 1_000);

        SpinWait.SpinUntil(() => ReadShared(path).AsSpan().SequenceEqual(payload), TimeoutMs).Should().BeTrue("the primary's WriteToBackend writes the payload");
        ReadShared(path).Should().Equal(payload);
    }

    /// <summary>
    /// Route <c>scan.in.relay.packetsink.tryenqueue.system.readonlyspan-system.byte-.out.system.io.filestream.write.64a355c8</c>:
    /// <c>PacketSink.TryEnqueue &gt; Multi2PacketSink.Accept &gt; PacketSink.Enqueue &gt; SpscQueueSink.Accept &gt; ConsumeLoop &gt; FileSink.WriteToBackend</c>.
    /// </summary>
    [Fact]
    public void TryEnqueue_Multi2PacketSinkIntoFileSinks_WritesPayloadToBothFiles()
    {
        byte[] payload = [0x5A, 0x17, 0xC3, 0x03];
        var firstPath  = Path.Combine(_dir, "multi2-a.bin");
        var secondPath = Path.Combine(_dir, "multi2-b.bin");
        using var first  = new FileSink(firstPath, writeBufferCapacity: 1, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        using var second = new FileSink(secondPath, writeBufferCapacity: 1, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        using var multi  = new Multi2PacketSink<FileSink, FileSink>(first, second);
        first.Start();
        second.Start();

        multi.TryEnqueue(payload).Should().BeTrue("both children are healthy, so Multi2PacketSink accepts");
        first.Stop(drainTimeoutMs: 1_000);
        second.Stop(drainTimeoutMs: 1_000);

        SpinWait.SpinUntil(() => ReadShared(firstPath).AsSpan().SequenceEqual(payload) && ReadShared(secondPath).AsSpan().SequenceEqual(payload), TimeoutMs)
            .Should().BeTrue("each child's WriteToBackend writes the payload");
        ReadShared(firstPath).Should().Equal(payload);
        ReadShared(secondPath).Should().Equal(payload);
    }

    /// <summary>
    /// Route <c>scan.in.relay.packetsink.tryenqueue.system.readonlyspan-system.byte-.out.system.io.filestream.write.d3fcfe15</c>:
    /// <c>PacketSink.TryEnqueue &gt; FilterSink.Accept &gt; PacketSink.Enqueue &gt; SpscQueueSink.Accept &gt; ConsumeLoop &gt; FileSink.WriteToBackend</c>.
    /// </summary>
    [Fact]
    public void TryEnqueue_FilterSinkMatchingPayloadIntoFileSink_WritesPayloadToFile()
    {
        byte[] payload = [0x5A, 0x17, 0xC3, 0x04];
        var path = Path.Combine(_dir, "filter.bin");
        using var file   = new FileSink(path, writeBufferCapacity: 1, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        using var filter = new FilterSink(static p => p[0] == 0x5A, file);
        file.Start();

        filter.TryEnqueue(payload).Should().BeTrue("FilterSink is always healthy and always accepts");
        file.Stop(drainTimeoutMs: 1_000);

        SpinWait.SpinUntil(() => ReadShared(path).AsSpan().SequenceEqual(payload), TimeoutMs).Should().BeTrue("the matching payload reaches the downstream's WriteToBackend");
        ReadShared(path).Should().Equal(payload);
    }

    // The sink may still hold the file open for write after Stop (Join result ignored): read with FileShare.ReadWrite.
    private static byte[] ReadShared(string path)
    {
        if (!File.Exists(path)) return [];
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var bytes = new byte[stream.Length];
        stream.ReadExactly(bytes);
        return bytes;
    }
}
