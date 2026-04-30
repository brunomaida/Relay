using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="MmfSink{T}"/> end-to-end Push throughput. Validates the cost-map
/// claim that this is the "fastest durable backend".
/// </summary>
[MemoryDiagnoser]
public class MmfSinkBenchmarks
{
    private string  _path = string.Empty;
    private Entry64 _item;

    [Params(100_000, 1_000_000)]
    public int ItemCount;

    [GlobalSetup]
    public void Setup()
    {
        _item = new Entry64 { A = 1, B = 2 };
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Fresh file per iteration — MmfSink uses a fixed capacity and never wraps.
        _path = Path.Combine(Path.GetTempPath(), $"relay-bench-{Guid.NewGuid():N}.mmf");
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        try { File.Delete(_path); } catch { /* best-effort */ }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try { File.Delete(_path); } catch { /* best-effort */ }
    }

    [Benchmark]
    public void Push_Single()
    {
        // Reserve enough capacity for the run + a margin.
        long capacity = (long)ItemCount * 64 + 1024;
        using var sink = new MmfSink<Entry64>(_path, maxBytes: capacity, ringCapacity: 65_536, flushInterval: 250);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(in _item);
        sink.Stop(30_000);
    }
}
