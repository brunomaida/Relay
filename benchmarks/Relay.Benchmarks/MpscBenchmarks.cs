using BenchmarkDotNet.Attributes;
using Relay.Buffers;

namespace Relay.Benchmarks;

/// <summary>
/// Measures MpscRingBuffer primitives on a single thread against the SpscRingBuffer baseline.
/// No consumer thread — isolates atomic CAS / flag-read cost from thread coordination.
/// Multi-thread contention correctness is validated by MpscQueueSinkTests stress tests.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class MpscBenchmarks
{
    private MpscRingBuffer<Entry64>  _ring        = null!;
    private MpscRingBuffer<Entry64>  _ringFull    = null!;
    private SpscRingBuffer<Entry64>  _spscBaseline = null!;
    private Entry64 _item;

    // 64 → 4 KB (L1), 1024 → 64 KB (L2), 65536 → 4 MB (L3 spill)
    [Params(64, 1024, 65536)]
    public int Capacity;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _ring          = new MpscRingBuffer<Entry64>(Capacity);
        _spscBaseline  = new SpscRingBuffer<Entry64>(Capacity);
        _item          = new Entry64 { A = 42, B = 99 };

        _ringFull = new MpscRingBuffer<Entry64>(Capacity);
        for (int i = 0; i < Capacity; i++)
            _ringFull.TryPublish(in _item);
    }

    /// <summary>SPSC round-trip: TryPublish then TryConsume. Baseline for MPSC comparison.</summary>
    [Benchmark(Baseline = true)]
    public bool Spsc_TryPublish_Baseline()
    {
        _spscBaseline.TryPublish(in _item);
        return _spscBaseline.TryConsume(out _);
    }

    /// <summary>MPSC round-trip: single producer, no contention — measures CAS + flag overhead vs SPSC.</summary>
    [Benchmark]
    public bool Mpsc_TryPublish_NoContention()
    {
        _ring.TryPublish(in _item);
        return _ring.TryConsume(out _);
    }

    /// <summary>Failed publish on full MPSC ring — headCache refresh + early exit.</summary>
    [Benchmark]
    public bool Mpsc_TryPublish_Full() => _ringFull.TryPublish(in _item);

    /// <summary>Failed consume on empty MPSC ring — Volatile.Read(Published) + early exit.</summary>
    [Benchmark]
    public bool Mpsc_TryConsume_Empty() => _ring.TryConsume(out _);
}
