using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay whose first code hop is <see cref="PacketSink.Enqueue"/> (SWAN S5.5d): each builds the chain its route's hops name,
/// calls <c>Enqueue</c> in its body, drains the chain and asserts the exit's effect in its body.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PacketSinkEnqueueRouteTests : IDisposable
{
    private const int TimeoutMs         = 5_000;
    private const int RecoveryTimeoutMs = 10_000; // >= 3x RotatingFileSink.MinBackoffMs; the first retry waits 2 s
    private const int NoDeadlineMs      = 60_000; // no deadline flush runs within a test

    private static readonly DateTime Day = new(2026, 11, 25, 0, 0, 0, DateTimeKind.Utc);

    private readonly string _dir = Directory.CreateTempSubdirectory("relay-swan-").FullName;

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.filestream.write.c4f42784</c>: the first open fails on a missing
    /// directory, the directory is restored, and <c>TryRecoverBackend &gt; TryOpenStream</c> reopens the file and writes its header.
    /// </summary>
    [Fact]
    public void Enqueue_RotatingFileSinkBackendRestored_RecoveryReopensFileWithHeader()
    {
        byte[] header  = [0x52, 0x4C, 0x59, 0x01];
        byte[] payload = [0x5A, 0x17, 0xC3, 0x01];
        var dir = Path.Combine(_dir, "rotating");
        using var sink = new RotatingFileSink(dir, "route", ringCapacity: 4_096, flushIntervalMs: 50,
                                              header: header, utcNow: static () => Day);
        sink.Start();

        sink.Enqueue(payload);
        SpinWait.SpinUntil(() => !sink.IsHealthy, TimeoutMs).Should().BeTrue("the first open fails while the directory is missing");
        Directory.CreateDirectory(dir);
        SpinWait.SpinUntil(() => sink.IsHealthy, RecoveryTimeoutMs).Should().BeTrue("TryRecoverBackend must reopen the file after the backoff");
        sink.Stop(drainTimeoutMs: 1_000);

        File.ReadAllBytes(Path.Combine(dir, "route-20261125.0000.log")).Should().Equal(header);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.system.io.filestream.write.1f4d3790</c>: the second payload does not fit the write buffer, so
    /// <c>FileSink.WriteToBackend &gt; FlushToStream</c> writes the first one; no deadline or <c>Stop</c> flush runs before the check.
    /// </summary>
    [Fact]
    public void Enqueue_FileSinkPayloadOverflowsWriteBuffer_WriteToBackendFlushesFirstPayload()
    {
        byte[] first  = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
        byte[] second = [0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18];
        var path = Path.Combine(_dir, "file.bin");
        using var sink = new FileSink(path, writeBufferCapacity: 8, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        sink.Start();

        sink.Enqueue(first);
        sink.Enqueue(second);
        SpinWait.SpinUntil(() => ReadShared(path).Length == first.Length, TimeoutMs).Should().BeTrue("WriteToBackend flushes the first payload when the second does not fit");
        ReadShared(path).Should().Equal(first);
        sink.Stop(drainTimeoutMs: 1_000);

        byte[] expected = [.. first, .. second];
        File.ReadAllBytes(path).Should().Equal(expected);
    }

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
