using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.Sinks;

public sealed class RotatingFileSinkTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "relay-rot-" + Guid.NewGuid().ToString("N"));

    public RotatingFileSinkTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { }
    }

    [Fact]
    public void Enqueue_BelowMaxBytes_NoRotation()
    {
        using var sink = new RotatingFileSink(_dir, "log", maxBytes: 1024,
                                              ringCapacity: 4096, flushIntervalMs: 50);
        sink.Start();
        for (int i = 0; i < 4; i++) sink.Enqueue(new byte[100]);
        sink.Stop(drainTimeoutMs: 1_000);

        var files = Directory.GetFiles(_dir, "log-*.log");
        files.Should().HaveCount(1);
        new FileInfo(files[0]).Length.Should().Be(400);
    }

    [Fact]
    public void Enqueue_ExceedsMaxBytes_RotatesToNextFile()
    {
        using var sink = new RotatingFileSink(_dir, "log", maxBytes: 250,
                                              ringCapacity: 4096, flushIntervalMs: 50);
        sink.Start();
        for (int i = 0; i < 4; i++) sink.Enqueue(new byte[100]);
        sink.Stop(drainTimeoutMs: 1_000);

        var files = Directory.GetFiles(_dir, "log-*.log");
        files.Should().HaveCountGreaterThan(1, "rotated when first file exceeded 250 bytes");
    }

    [Fact]
    public void Cleanup_RetainsAtMostMaxFiles()
    {
        using var sink = new RotatingFileSink(_dir, "log", maxBytes: 100,
                                              ringCapacity: 4096, flushIntervalMs: 50,
                                              maxFiles: 2);
        sink.Start();
        for (int i = 0; i < 10; i++) sink.Enqueue(new byte[80]);
        sink.Stop(drainTimeoutMs: 1_000);

        var files = Directory.GetFiles(_dir, "log-*.log");
        files.Length.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public void Header_WrittenOncePerNewFile()
    {
        byte[] header = [0xCA, 0xFE];
        using var sink = new RotatingFileSink(_dir, "log", maxBytes: 50,
                                              ringCapacity: 4096, flushIntervalMs: 50,
                                              header: header);
        sink.Start();
        sink.Enqueue(new byte[40]); // file 1: header(2) + 40 = 42 bytes
        sink.Enqueue(new byte[40]); // exceeds 50 -> rotate to file 2 with header
        sink.Stop(drainTimeoutMs: 1_000);

        var files = Directory.GetFiles(_dir, "log-*.log");
        foreach (var f in files)
        {
            var bytes = File.ReadAllBytes(f);
            bytes[..2].Should().Equal(header, $"each rotated file starts with header: {f}");
        }
    }

    [Fact]
    public void Enqueue_DayBoundaryCrossed_RotatesToNextFile()
    {
        using var sink = new RotatingFileSink(_dir, "log", maxBytes: 1_000_000,
                                              ringCapacity: 4096, flushIntervalMs: 50);
        sink.Start();

        sink.Enqueue(new byte[100]);
        Thread.Sleep(120);                              // let the consumer drain into file 1

        sink.SetDayBoundaryForTest(Relay.Internal.HfClock.NowTicks - 1); // simulate "yesterday ended"

        sink.Enqueue(new byte[100]);
        sink.Stop(drainTimeoutMs: 1_000);

        var files = Directory.GetFiles(_dir, "log-*.log");
        files.Should().HaveCountGreaterThan(1, "day boundary crossed -> rotated");
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var sink = new RotatingFileSink(_dir, "log", maxBytes: 1024);
        sink.Start();
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }
}
