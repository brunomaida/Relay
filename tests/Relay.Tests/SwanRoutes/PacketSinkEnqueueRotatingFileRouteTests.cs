using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay that enter at <see cref="PacketSink.Enqueue"/> and exit through
/// <see cref="RotatingFileSink"/> (SWAN S5.5d): each builds the chain its route's hops name, calls <c>Enqueue</c> in its body,
/// drives the chain to the exit and asserts the exit's effect in its body.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PacketSinkEnqueueRotatingFileRouteTests : IDisposable
{
    private const int TimeoutMs    = 5_000;
    private const int NoDeadlineMs = 60_000; // no deadline flush runs within a test

    private static readonly DateTime Day = new(2026, 11, 25, 0, 0, 0, DateTimeKind.Utc);

    private static readonly byte[] Header = [0x52, 0x4C, 0x59, 0x01];
    private static readonly byte[] First  = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
    private static readonly byte[] Second = [0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18];

    private readonly string _dir = Directory.CreateTempSubdirectory("relay-swan-").FullName;

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.filestream.write.047f36f6</c>: the first write finds no stream, so
    /// <c>RotatingFileSink.WriteToBackend &gt; TryOpenStream</c> opens the file and writes its header.
    /// </summary>
    [Fact]
    public void Enqueue_RotatingFileSinkFirstWrite_TryOpenStreamWritesHeader()
    {
        using var sink = new RotatingFileSink(_dir, "route", ringCapacity: 4_096, flushIntervalMs: 50,
                                              header: Header, utcNow: static () => Day);
        sink.Start();

        sink.Enqueue(First);
        sink.Stop(drainTimeoutMs: 1_000);

        byte[] expected = [.. Header, .. First];
        File.ReadAllBytes(FilePath(0)).Should().Equal(expected);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.filestream.write.6c4e263f</c>: the second payload does not fit the write buffer, so
    /// <c>RotatingFileSink.WriteToBackend &gt; FlushToStream</c> writes the first one; no deadline or <c>Stop</c> flush runs before the check.
    /// </summary>
    [Fact]
    public void Enqueue_RotatingFileSinkPayloadOverflowsWriteBuffer_WriteToBackendFlushesFirstPayload()
    {
        using var sink = new RotatingFileSink(_dir, "route", writeBufferCapacity: 8, ringCapacity: 4_096,
                                              flushIntervalMs: NoDeadlineMs, utcNow: static () => Day);
        sink.Start();

        sink.Enqueue(First);
        sink.Enqueue(Second);
        SpinWait.SpinUntil(() => ReadShared(FilePath(0)).Length == First.Length, TimeoutMs).Should().BeTrue("WriteToBackend flushes the first payload when the second does not fit");
        ReadShared(FilePath(0)).Should().Equal(First);
        sink.Stop(drainTimeoutMs: 1_000);

        byte[] expected = [.. First, .. Second];
        File.ReadAllBytes(FilePath(0)).Should().Equal(expected);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.filestream.write.8a298010</c>: the second payload crosses <c>maxBytes</c>, so
    /// <c>WriteToBackend &gt; RotateNow &gt; FlushToStream</c> writes the buffered first payload to the old file before it rotates.
    /// </summary>
    [Fact]
    public void Enqueue_RotatingFileSinkSizeRotation_RotateNowFlushesBufferedPayloadToOldFile()
    {
        using var sink = new RotatingFileSink(_dir, "route", maxBytes: 8, ringCapacity: 4_096,
                                              flushIntervalMs: NoDeadlineMs, utcNow: static () => Day);
        sink.Start();

        sink.Enqueue(First);
        sink.Enqueue(Second);
        sink.Stop(drainTimeoutMs: 1_000);

        File.ReadAllBytes(FilePath(0)).Should().Equal(First);
        File.ReadAllBytes(FilePath(1)).Should().Equal(Second);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.filestream.write.e68a1fc2</c>: the second payload crosses <c>maxBytes</c>, so
    /// <c>WriteToBackend &gt; RotateNow &gt; TryOpenStream</c> opens the next file and writes its header.
    /// </summary>
    [Fact]
    public void Enqueue_RotatingFileSinkSizeRotation_RotateNowOpensNextFileWithHeader()
    {
        byte[] first  = [0x01, 0x02, 0x03, 0x04];
        byte[] second = [0x11, 0x12, 0x13, 0x14];
        using var sink = new RotatingFileSink(_dir, "route", maxBytes: 8, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs,
                                              header: Header, utcNow: static () => Day);
        sink.Start();

        sink.Enqueue(first);
        sink.Enqueue(second);
        sink.Stop(drainTimeoutMs: 1_000);

        byte[] expectedOld  = [.. Header, .. first];
        byte[] expectedNext = [.. Header, .. second];
        File.ReadAllBytes(FilePath(0)).Should().Equal(expectedOld);
        File.ReadAllBytes(FilePath(1)).Should().Equal(expectedNext);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.file.delete.74b74c96</c>: the second payload crosses <c>maxBytes</c> with
    /// <c>maxFiles: 1</c>, so <c>WriteToBackend &gt; RotateNow &gt; Cleanup</c> deletes the rotated-out file.
    /// </summary>
    [Fact]
    public void Enqueue_RotatingFileSinkRetentionOfOne_CleanupDeletesRotatedFile()
    {
        using var sink = new RotatingFileSink(_dir, "route", maxBytes: 8, maxFiles: 1, ringCapacity: 4_096,
                                              flushIntervalMs: NoDeadlineMs, utcNow: static () => Day);
        sink.Start();

        sink.Enqueue(First);
        sink.Enqueue(Second);
        sink.Stop(drainTimeoutMs: 1_000);

        File.Exists(FilePath(0)).Should().BeFalse("Cleanup keeps only the newest file");
        File.ReadAllBytes(FilePath(1)).Should().Equal(Second);
    }

    private string FilePath(int seq) => Path.Combine(_dir, $"route-20261125.{seq:D4}.log");

    // The sink holds the file open for write: read with FileShare.ReadWrite.
    private static byte[] ReadShared(string path)
    {
        if (!File.Exists(path)) return [];
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var bytes = new byte[stream.Length];
        stream.ReadExactly(bytes);
        return bytes;
    }
}
