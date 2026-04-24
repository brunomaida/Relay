using BenchmarkDotNet.Attributes;
using Relay.Buffers;

namespace Relay.Benchmarks;

/// <summary>
/// Measures <see cref="SpscByteRingBuffer"/> primitives on a single thread.
/// When <c>PayloadSize + 4</c> (header) exceeds <c>Capacity</c>, the record can never fit;
/// those combos exercise the fast-reject path and still produce valid (low-cost) measurements.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class ByteRingBufferBenchmarks
{
    private SpscByteRingBuffer _ring    = null!;
    private SpscByteRingBuffer _ringFull = null!;
    private byte[]             _payload  = null!;

    // 64 → 4 KB (L1), 1024 → 64 KB (L2), 65536 → 4 MB (L3 spill)
    [Params(64, 1024, 65536)]
    public int Capacity;

    // Varying record sizes: 8 B (tiny), 64 B (cache line), 256 B, 1 KB
    [Params(8, 64, 256, 1024)]
    public int PayloadSize;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _ring = new SpscByteRingBuffer(Capacity);

        _payload = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++)
            _payload[i] = (byte)i;

        // Pre-fill a second ring to saturation for the TryPublish_Full benchmark.
        // Stop as soon as TryPublish returns false (ring full or record too large).
        _ringFull = new SpscByteRingBuffer(Capacity);
        while (_ringFull.TryPublish(_payload)) { }
    }

    /// <summary>Failed peek on empty ring — Volatile.Read(tail) + early exit.</summary>
    [Benchmark(Baseline = true)]
    public bool TryPeek_Empty() => _ring.TryPeek(out _, out _);

    /// <summary>Publish then peek+advance on same thread — ring never accumulates.</summary>
    [Benchmark]
    public bool RoundTrip()
    {
        _ring.TryPublish(_payload);
        bool ok = _ring.TryPeek(out _, out int adv);
        _ring.Advance(adv);
        return ok;
    }

    /// <summary>Failed publish on full (or over-capacity) ring — fast-reject path.</summary>
    [Benchmark]
    public bool TryPublish_Full() => _ringFull.TryPublish(_payload);
}
