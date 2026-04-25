using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.PacketSinks;

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(5)]
public class FileSinkBenchmark
{
    private FileSink? _sink;
    private byte[]    _payload = new byte[128];
    private string    _path    = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _path = Path.Combine(Path.GetTempPath(), $"relay-bench-{Guid.NewGuid():N}.bin");
        _sink = new FileSink(_path, writeBufferCapacity: 65_536, ringCapacity: 65_536, flushIntervalMs: 200);
        _sink.Start();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _sink?.Stop(2_000);
        try { File.Delete(_path); } catch { /* best effort */ }
    }

    private const int BatchSize = 1024;

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void FileSink_Enqueue_128B()
    {
        for (int i = 0; i < BatchSize; i++)
            _sink!.Enqueue(_payload);
    }
}
