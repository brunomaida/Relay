using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Relay.Builder;
using Relay.Sinks;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay whose first code hop is <see cref="PacketSink.TryEnqueue"/> on a queue fallback
/// and whose gated hop is <c>TryDrainToPrev</c> (SWAN S5.5d): the backlog is published into the fallback's ring before its
/// <c>Start</c>; its <c>Stop</c> flips <c>_running</c> while the one-record-per-write backend still holds the backlog, so the
/// next flush-deadline hit (<c>flushIntervalMs: 0</c>) drains the rest to <c>Prev</c>. Each test asserts in its body an effect only <c>Prev.Enqueue</c> can produce.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PacketSinkTryEnqueueDrainRouteTests : IDisposable
{
    private const int TimeoutMs     = 5_000;
    private const int BacklogFrames = 1_500;  // 8 B records (12 000 B) fit the 16 384 B ring; Stop must land before the consumer empties it
    private const int NoDeadlineMs  = 60_000; // no deadline flush runs within a test

    private readonly string _dir = Directory.CreateTempSubdirectory("relay-swan-").FullName;

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    /// <summary>
    /// Route <c>scan.in.relay.packetsink.tryenqueue.system.readonlyspan-system.byte-.out.relay.packetpredicate.invoke.1ca71dc5</c>:
    /// <c>PacketSink.TryEnqueue &gt; SpscQueueSink.Accept &gt; ConsumeLoop &gt; TryDrainToPrev &gt; PacketSink.Enqueue &gt; FilterSink.Accept</c>.
    /// The predicate discards every frame, so the route ends at its exit: the filter's decision.
    /// </summary>
    [Fact]
    public void TryEnqueue_SpscFallbackBacklogDrainsToFilterPrev_PredicateDiscardsDrainedFrames()
    {
        var fallbackPath = Path.Combine(_dir, "fallback.bin");
        var seen         = new ConcurrentQueue<byte[]>();
        var downstream   = new CollectingSink();
        var filter       = new FilterSink(p => { seen.Enqueue(p.ToArray()); return false; }, downstream);
        using var fallback = new FileSink(fallbackPath, writeBufferCapacity: 1, ringCapacity: 16_384, flushIntervalMs: 0);
        RelayBuilder.StartPacket(filter).To(fallback);
        byte[] expected = Frames(BacklogFrames);

        int accepted = 0;
        for (int i = 0; i < BacklogFrames; i++)
            if (fallback.TryEnqueue(expected.AsSpan(i * sizeof(int), sizeof(int)))) accepted++;
        accepted.Should().Be(BacklogFrames, "the fallback's ring holds the whole backlog");
        fallback.Start();
        fallback.Stop(drainTimeoutMs: 2_000);

        SpinWait.SpinUntil(() => Delivered(ReadShared(fallbackPath), seen).AsSpan().SequenceEqual(expected), TimeoutMs).Should().BeTrue("every backlog frame lands once, in order, across the fallback file and the predicate");
        seen.Should().NotBeEmpty("the fallback's backlog beyond one batch must drain to Prev, the filter");
        downstream.Received.Should().BeEmpty("the predicate discards every drained frame");
        Delivered(ReadShared(fallbackPath), seen).Should().Equal(expected);
    }

    /// <summary>
    /// Route <c>scan.in.relay.packetsink.tryenqueue.system.readonlyspan-system.byte-.out.system.io.filestream.write.648cbaf0</c>:
    /// <c>PacketSink.TryEnqueue &gt; MpscQueueSink.Accept &gt; TryDrainToPrev &gt; PacketSink.Enqueue &gt; SpscQueueSink.Accept &gt; ConsumeLoop &gt; FileSink.WriteToBackend</c>.
    /// <c>TryDrainToPrev</c> is private and its only caller is <c>MpscQueueSink.ConsumeLoop</c> (<c>MpscQueueSink.Packet.cs:219</c>).
    /// </summary>
    [Fact]
    public void TryEnqueue_MpscFallbackBacklogDrainsToFileSinkPrev_WritesDrainedFramesToPrimaryFile()
    {
        var primaryPath  = Path.Combine(_dir, "primary.bin");
        var fallbackPath = Path.Combine(_dir, "fallback.bin");
        using var primary  = new FileSink(primaryPath, writeBufferCapacity: 1, ringCapacity: 16_384, flushIntervalMs: NoDeadlineMs);
        using var fallback = new FileMpscSink(fallbackPath, ringCapacity: 16_384, flushIntervalMs: 0);
        RelayBuilder.StartPacket(primary).To(fallback);
        byte[] expected = Frames(BacklogFrames);

        int accepted = 0;
        for (int i = 0; i < BacklogFrames; i++)
            if (fallback.TryEnqueue(expected.AsSpan(i * sizeof(int), sizeof(int)))) accepted++;
        accepted.Should().Be(BacklogFrames, "the fallback's ring holds the whole backlog");
        primary.Start();
        fallback.Start();
        fallback.Stop(drainTimeoutMs: 2_000);
        primary.Stop(drainTimeoutMs: 2_000);

        SpinWait.SpinUntil(() => Delivered(ReadShared(fallbackPath), ReadShared(primaryPath)).AsSpan().SequenceEqual(expected), TimeoutMs).Should().BeTrue("every backlog frame lands once, in order, across the fallback and primary files");
        ReadShared(primaryPath).Should().NotBeEmpty("the fallback's backlog beyond one batch must drain to Prev, the primary");
        Delivered(ReadShared(fallbackPath), ReadShared(primaryPath)).Should().Equal(expected);
    }

    /// <summary>
    /// Route <c>scan.in.relay.packetsink.tryenqueue.system.readonlyspan-system.byte-.out.system.io.filestream.write.9dd18763</c>:
    /// <c>PacketSink.TryEnqueue &gt; MpscQueueSink.Accept &gt; ConsumeLoop &gt; TryDrainToPrev &gt; PacketSink.Enqueue &gt; SpscQueueSink.Accept &gt; ConsumeLoop &gt; FileSink.WriteToBackend</c>.
    /// </summary>
    [Fact]
    public void TryEnqueue_MpscFallbackConsumerDrainsBacklogToFileSinkPrev_WritesDrainedFramesToPrimaryFile()
    {
        var primaryPath  = Path.Combine(_dir, "primary.bin");
        var fallbackPath = Path.Combine(_dir, "fallback.bin");
        using var primary  = new FileSink(primaryPath, writeBufferCapacity: 1, ringCapacity: 16_384, flushIntervalMs: NoDeadlineMs);
        using var fallback = new FileMpscSink(fallbackPath, ringCapacity: 16_384, flushIntervalMs: 0);
        RelayBuilder.StartPacket(primary).To(fallback);
        byte[] expected = Frames(BacklogFrames);

        int accepted = 0;
        for (int i = 0; i < BacklogFrames; i++)
            if (fallback.TryEnqueue(expected.AsSpan(i * sizeof(int), sizeof(int)))) accepted++;
        accepted.Should().Be(BacklogFrames, "the fallback's ring holds the whole backlog");
        primary.Start();
        fallback.Start();
        fallback.Stop(drainTimeoutMs: 2_000);
        primary.Stop(drainTimeoutMs: 2_000);

        SpinWait.SpinUntil(() => Delivered(ReadShared(fallbackPath), ReadShared(primaryPath)).AsSpan().SequenceEqual(expected), TimeoutMs).Should().BeTrue("every backlog frame lands once, in order, across the fallback and primary files");
        ReadShared(primaryPath).Should().NotBeEmpty("the fallback's consumer must drain its backlog beyond one batch to Prev, the primary");
        Delivered(ReadShared(fallbackPath), ReadShared(primaryPath)).Should().Equal(expected);
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

    // The fallback's kept bytes followed by the frames it drained to Prev.
    private static byte[] Delivered(byte[] kept, byte[] drained) => [.. kept, .. drained];

    private static byte[] Delivered(byte[] kept, ConcurrentQueue<byte[]> drained) => [.. kept, .. drained.SelectMany(static f => f)];

    private static byte[] Frames(int count)
    {
        var bytes = new byte[count * sizeof(int)];
        for (int i = 0; i < count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * sizeof(int)), i);
        return bytes;
    }

    /// <summary>
    /// Chain sink, not a helper: src/ has no concrete packet <see cref="MpscQueueSink"/> (precedent: <c>MpscQueueSinkPacketTests.InMemoryMpscByteSink</c>).
    /// Like a <c>FileSink</c> with a one-byte write buffer, each record is its own write, which widens the window between
    /// <c>Stop</c> and the consumer emptying its ring.
    /// </summary>
    private sealed class FileMpscSink : MpscQueueSink
    {
        private readonly string _path;
        private FileStream?     _stream;

        public FileMpscSink(string path, int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "swan-file-mpsc") => _path = path;

        protected override void WriteToBackend(ReadOnlySpan<byte> payload)
        {
            _stream ??= new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read, bufferSize: 1, useAsync: false);
            _stream.Write(payload);
            _stream.Flush();
        }

        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }

        protected override void DisposeBackend()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
