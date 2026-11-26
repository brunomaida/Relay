using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow test for the SWAN hot route of Relay that enters at <c>RotatingFileSink.Cleanup</c>'s <c>Directory.GetFiles</c> port and
/// exits through its <c>File.Delete</c> (SWAN S5.5d). <c>Cleanup</c> is private (<c>RotatingFileSink.cs:200</c>), so the test drives
/// it through <see cref="PacketSink.Enqueue"/> and a size rotation (owner-accepted deviation, spec L3).
/// </summary>
[Trait("Category", "Integration")]
public sealed class DrainCleanupRouteTests : IDisposable
{
    private const int TimeoutMs    = 5_000;
    private const int NoDeadlineMs = 60_000; // no deadline flush runs within a test

    private static readonly DateTime Day = new(2026, 11, 25, 0, 0, 0, DateTimeKind.Utc);

    private readonly string _dir = Directory.CreateTempSubdirectory("relay-swan-").FullName;

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    /// <summary>
    /// Route <c>scan.in.system.io.directory.getfiles.out.system.io.file.delete.0068d74a</c>: two older files the sink never wrote
    /// sit in its directory; the second payload crosses <c>maxBytes</c>, and <c>Cleanup</c> lists the directory and deletes the
    /// listed files beyond <c>maxFiles: 2</c>, which are exactly those two. The sink's own files survive, so nothing here
    /// overlaps route <c>74b74c96</c>, which deletes the sink's own rotated-out file.
    /// </summary>
    [Fact]
    public void Enqueue_RotatingFileSinkSizeRotationWithOlderFilesListed_CleanupDeletesListedOlderFiles()
    {
        byte[] first  = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
        byte[] second = [0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18];
        string older  = Path.Combine(_dir, "route-20261120.0000.log");
        string oldest = Path.Combine(_dir, "route-20261119.0000.log");
        File.WriteAllBytes(older, [0xA1]);
        File.WriteAllBytes(oldest, [0xA2]);
        using var sink = new RotatingFileSink(_dir, "route", maxBytes: 8, maxFiles: 2, ringCapacity: 4_096,
                                              flushIntervalMs: NoDeadlineMs, utcNow: static () => Day);
        sink.Start();

        sink.Enqueue(first);
        sink.Enqueue(second);
        sink.Stop(drainTimeoutMs: 1_000);

        SpinWait.SpinUntil(() => !File.Exists(older) && !File.Exists(oldest), TimeoutMs).Should().BeTrue("Cleanup deletes the listed files beyond maxFiles");
        File.Exists(older).Should().BeFalse("Cleanup deletes the older listed file");
        File.Exists(oldest).Should().BeFalse("Cleanup deletes the oldest listed file");
        File.Exists(FilePath(0)).Should().BeTrue("the sink's rotated-out file is within maxFiles");
        File.Exists(FilePath(1)).Should().BeTrue("the sink's current file is within maxFiles");
    }

    private string FilePath(int seq) => Path.Combine(_dir, $"route-20261125.{seq:D4}.log");
}
