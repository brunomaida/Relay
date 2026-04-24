using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests;

public sealed class FileSinkTests : IDisposable
{
    private readonly string _path = Path.GetTempFileName();

    public void Dispose()
    {
        if (File.Exists(_path)) File.Delete(_path);
    }

    [Fact]
    public void Enqueue_PayloadWrittenToFile()
    {
        byte[] payload = [10, 20, 30];
        using var sink = new FileSink(_path, ringCapacity: 4_096, flushIntervalMs: 50);
        sink.Start();

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 1_000);

        File.ReadAllBytes(_path).Should().Equal(payload);
    }

    [Fact]
    public void Start_WithHeader_HeaderWrittenBeforePayloads()
    {
        byte[] header  = [0xCA, 0xFE];
        byte[] payload = [0x01, 0x02];
        File.Delete(_path);
        using var sink = new FileSink(_path, ringCapacity: 4_096, flushIntervalMs: 50,
                                       header: header);
        sink.Start();

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 1_000);

        byte[] all = File.ReadAllBytes(_path);
        all[..2].Should().Equal(header);
        all[2..].Should().Equal(payload);
    }

    [Fact]
    public void Start_HeaderNotWritten_WhenFileAlreadyHasContent()
    {
        byte[] existing = [0xAA, 0xBB];
        byte[] header   = [0xFF, 0xFF];
        byte[] payload  = [0x01];
        File.WriteAllBytes(_path, existing);

        using var sink = new FileSink(_path, ringCapacity: 4_096, flushIntervalMs: 50,
                                       header: header);
        sink.Start();
        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 1_000);

        byte[] all = File.ReadAllBytes(_path);
        all[..2].Should().Equal(existing, "header must not overwrite existing content");
        all[2..].Should().Equal(payload);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var sink = new FileSink(_path, ringCapacity: 4_096);
        sink.Start();
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }
}
