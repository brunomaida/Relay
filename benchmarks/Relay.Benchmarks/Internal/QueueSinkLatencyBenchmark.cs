using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Relay;
using Relay.Internal;

namespace Relay.Benchmarks.Internal;

/// <summary>
/// Per-message end-to-end latency benchmark for SpscQueueSink.
/// Measures p50/p99/p999 under three affinity/priority scenarios.
/// </summary>
/// <remarks>
/// BDN's Mean column measures wall time of one benchmark iteration (not p999).
/// The real metrics — p50, p99, p999 — are printed to console at each iteration.
/// Return value (p999 ns) prevents dead-code elimination.
/// </remarks>
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 5)]
[MemoryDiagnoser]
public class QueueSinkLatencyBenchmark
{
    private const int ItemCount    = 5_000_000;
    private const int RingCapacity = 131_072;   // 128K — larger than producer burst, stays resident in L2

    [Params(ScenarioId.Default, ScenarioId.NormalPriority, ScenarioId.NormalPinned)]
    public ScenarioId Scenario;

    public enum ScenarioId { Default, NormalPriority, NormalPinned }

    // Histogram buckets: log2 of nanoseconds (bucket i covers [2^i, 2^(i+1)) ns).
    // Index 0 = sub-1ns (rounds to here), index 63 = ≥ 4 seconds.
    private readonly long[] _histogram = new long[64];

    private long _p999Ns;  // last iteration p999, returned from [Benchmark]

    [IterationCleanup]
    public void IterationCleanup()
    {
        (long p50, long p99, long p999) = ComputePercentiles(_histogram);
        long total = 0;
        for (int i = 0; i < _histogram.Length; i++) total += _histogram[i];
        Console.WriteLine($"  [{Scenario}]  samples={total}  p50={p50}ns  p99={p99}ns  p999={p999}ns");
        _p999Ns = p999;
        Array.Clear(_histogram, 0, _histogram.Length);
    }

    /// <summary>
    /// Runs the full producer+consumer workload for one iteration.
    /// Returns p999 ns to prevent dead-code elimination.
    /// </summary>
    [Benchmark]
    public long Measure_P999_Latency_Ns()
    {
        (ThreadPriority priority, int cpu) = Scenario switch
        {
            ScenarioId.Default       => (ThreadPriority.BelowNormal, -1),
            ScenarioId.NormalPriority => (ThreadPriority.Normal, -1),
            ScenarioId.NormalPinned   => (ThreadPriority.Normal, 2),  // logical CPU 2 — P-core on i9-12700
            _                         => (ThreadPriority.Normal, -1)
        };

        using var sink = new LatencyConsumerSink(RingCapacity, _histogram, priority, cpu);
        sink.Start();

        var item = new TimestampedItem();
        for (int i = 0; i < ItemCount; i++)
        {
            item.Timestamp = HfClock.NowTicks;
            sink.Enqueue(in item);
        }

        sink.Stop(drainTimeoutMs: 30_000);

        return _p999Ns;
    }

    // ── Histogram helpers ─────────────────────────────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void RecordNs(long[] histogram, long deltaNs)
    {
        if (deltaNs <= 0) deltaNs = 1;
        int bucket = Math.Min(63, BitOperations.Log2((ulong)deltaNs));
        histogram[bucket]++;
    }

    private static (long p50, long p99, long p999) ComputePercentiles(long[] histogram)
    {
        // Count total recorded samples.
        long total = 0;
        for (int i = 0; i < histogram.Length; i++) total += histogram[i];
        if (total == 0) return (0, 0, 0);

        long p50  = 0;
        long p99  = 0;
        long p999 = 0;

        long t50  = total * 50L / 100;
        long t99  = total * 99L / 100;
        long t999 = total * 999L / 1000;

        long cum = 0;
        for (int i = 0; i < histogram.Length; i++)
        {
            cum += histogram[i];
            long bucketNs = 1L << i;  // lower bound of bucket i
            if (p50  == 0 && cum >= t50)  p50  = bucketNs;
            if (p99  == 0 && cum >= t99)  p99  = bucketNs;
            if (p999 == 0 && cum >= t999) p999 = bucketNs;
            if (p999 != 0) break;
        }
        return (p50, p99, p999);
    }
}

// ── Item struct ───────────────────────────────────────────────────────────────────

/// <summary>64-byte timestamped item used for latency measurement.</summary>
[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct TimestampedItem
{
    /// <summary>Stopwatch timestamp captured by the producer before TryPublish.</summary>
    public long Timestamp;
    // 56 bytes of implicit padding (Size=64 via StructLayout) keep cache-line alignment.
}

// ── Concrete consumer sink ────────────────────────────────────────────────────────

/// <summary>
/// SPSC consumer sink that records per-message latency into a shared histogram.
/// Sealed so the JIT can devirtualize WriteToBackend.
/// </summary>
internal sealed class LatencyConsumerSink : SpscQueueSink<TimestampedItem>
{
    private readonly long[] _histogram;

    public LatencyConsumerSink(int ringCapacity, long[] histogram, ThreadPriority priority, int affinityCpu)
        : base(ringCapacity, flushIntervalMs: 100, pipeName: "lat-bench", threadPriority: priority, affinityCpu: affinityCpu)
    {
        _histogram = histogram;
    }

    protected override void WriteToBackend(in TimestampedItem item)
    {
        long nowTicks  = HfClock.NowTicks;
        long deltaTicks = nowTicks - item.Timestamp;
        // deltaTicks * 1e9 / Stopwatch.Frequency: on Windows Freq = 10^7 → 100ns resolution.
        long deltaNs   = deltaTicks * 1_000_000_000L / Stopwatch.Frequency;
        QueueSinkLatencyBenchmark.RecordNs(_histogram, deltaNs);
    }

    protected override void FlushBackend()      { }
    protected override void TryRecoverBackend() { }
    protected override void DisposeBackend()    { }
}
