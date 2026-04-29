using BenchmarkDotNet.Attributes;
using Relay.Buffers;

namespace Relay.Benchmarks;

/// <summary>
/// Measures <see cref="MpscByteRingBuffer"/> primitives on a single thread against the
/// SPSC byte-ring baseline. No consumer thread — isolates atomic CAS / header-flag cost
/// from cross-thread coordination. Multi-producer contention is Phase 7.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class MpscByteRingBufferBenchmarks
{
    private MpscByteRingBuffer _ring     = null!;
    private MpscByteRingBuffer _ringFull = null!;
    private SpscByteRingBuffer _spsc     = null!;
    private byte[]             _payload  = null!;

    // 64 → 4 KB (L1), 1024 → 64 KB (L2), 65536 → 4 MB (L3 spill)
    [Params(64, 1024, 65536)]
    public int Capacity;

    // Tiny / cache-line / medium / large
    [Params(8, 64, 256, 1024)]
    public int PayloadSize;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _ring    = new MpscByteRingBuffer(Capacity);
        _spsc    = new SpscByteRingBuffer(Capacity);
        _payload = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++) _payload[i] = (byte)i;

        _ringFull = new MpscByteRingBuffer(Capacity);
        while (_ringFull.TryPublish(_payload)) { }
    }

    /// <summary>SPSC byte-ring round-trip baseline for ratio comparison.</summary>
    [Benchmark(Baseline = true)]
    public bool Spsc_RoundTrip()
    {
        _spsc.TryPublish(_payload);
        bool ok = _spsc.TryPeek(out _, out int adv);
        _spsc.Advance(adv);
        return ok;
    }

    /// <summary>MPSC round-trip uncontended — single-producer single-consumer same thread.</summary>
    [Benchmark]
    public bool Mpsc_RoundTrip_NoContention()
    {
        _ring.TryPublish(_payload);
        bool ok = _ring.TryPeek(out _, out int adv);
        _ring.Advance(adv);
        return ok;
    }

    /// <summary>Failed publish on full MPSC ring — head-cache hit + early exit.</summary>
    [Benchmark]
    public bool Mpsc_TryPublish_Full() => _ringFull.TryPublish(_payload);

    /// <summary>Failed peek on empty MPSC ring — Volatile.Read header + bit-test + early exit.</summary>
    [Benchmark]
    public bool Mpsc_TryPeek_Empty() => _ring.TryPeek(out _, out _);
}
