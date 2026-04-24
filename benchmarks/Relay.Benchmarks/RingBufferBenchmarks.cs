using BenchmarkDotNet.Attributes;
using Relay.Buffers;

namespace Relay.Benchmarks;

/// <summary>
/// Measures SpscRingBuffer primitives on a single thread.
/// No consumer thread — isolates atomic read/write cost from thread coordination.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class RingBufferBenchmarks
{
    private SpscRingBuffer<Entry64> _ring = null!;
    private SpscRingBuffer<Entry64> _ringFull = null!;
    private Entry64[] _batch = null!;
    private Entry64[] _scratch = null!;
    private Entry64 _item;

    // 64 → 4 KB (L1), 1024 → 64 KB (L2), 65536 → 4 MB (L3 spill)
    [Params(64, 1024, 65536)]
    public int Capacity;

    private const int BatchSize = 32;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _ring = new SpscRingBuffer<Entry64>(Capacity);
        _item = new Entry64 { A = 42, B = 99 };

        _ringFull = new SpscRingBuffer<Entry64>(Capacity);
        for (int i = 0; i < Capacity; i++)
            _ringFull.TryPublish(in _item);

        _batch   = new Entry64[BatchSize];
        _scratch = new Entry64[BatchSize];
        for (int i = 0; i < BatchSize; i++) _batch[i] = _item;
    }

    /// <summary>Failed consume on empty ring — Volatile.Read(tail) + early exit.</summary>
    [Benchmark(Baseline = true)]
    public bool TryConsume_Empty() => _ring.TryConsume(out _);

    /// <summary>Publish then consume on same thread — ring never accumulates.</summary>
    [Benchmark]
    public bool RoundTrip()
    {
        _ring.TryPublish(in _item);
        return _ring.TryConsume(out _);
    }

    /// <summary>Failed publish on full ring — Volatile.Read(head) + early exit.</summary>
    [Benchmark]
    public bool TryPublish_Full() => _ringFull.TryPublish(in _item);

    /// <summary>Publish batch of 32, then consume batch of 32 — 2 fences vs 64 per-item.</summary>
    [Benchmark]
    public int RoundTrip_Batch32()
    {
        _ring.TryPublishBatch(_batch);
        return _ring.TryConsumeBatch(_scratch);
    }
}
