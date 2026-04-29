using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks;

/// <summary>
/// End-to-end throughput: producer pushes N items into an <see cref="SpscQueueSink{T}"/>,
/// consumer thread drains them via a trivial <see cref="WriteToBackend"/>. Measures the
/// cumulative cost of ring ops + consumer loop + drain on Stop.
/// </summary>
/// <remarks>
/// <para>Compares:</para>
/// <list type="bullet">
///   <item><c>Push_Single</c> — <c>Enqueue(in T)</c> per item: 1 mfence per publish, 1 per consume.</item>
///   <item><c>Push_Batch32</c> — <c>EnqueueBatch(span32)</c>: 1 mfence per 32 publishes. Consumer
///         loop uses <c>TryConsumeBatch</c>: 1 mfence per 32 head advances.</item>
/// </list>
/// <para>Validates Items #1 (batch API) and #2 (64-byte aligned native ring) together —
/// steady-state producer+consumer dynamics, not just isolated ring ops.</para>
/// </remarks>
[MemoryDiagnoser]
public class QueuePipeThroughputBenchmarks
{
    private Entry64[] _batch = null!;
    private Entry64   _item;

    private const int BatchSize    = 32;
    private const int RingCapacity = 65_536;

    [Params(100_000, 1_000_000)]
    public int ItemCount;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _item  = new Entry64 { A = 1, B = 2 };
        _batch = new Entry64[BatchSize];
        for (int i = 0; i < BatchSize; i++) _batch[i] = _item;
    }

    [Benchmark(Baseline = true)]
    public long Push_Single()
    {
        using var pipe = new TestSpscPipe(RingCapacity, backendSpinCycles: 0);
        pipe.Start();
        for (int i = 0; i < ItemCount; i++)
            pipe.Enqueue(in _item);
        pipe.Stop(30_000);
        return pipe.Sum;
    }

    [Benchmark]
    public long Push_Batch32()
    {
        using var pipe = new TestSpscPipe(RingCapacity, backendSpinCycles: 0);
        pipe.Start();
        ReadOnlySpan<Entry64> span = _batch;
        int total = ItemCount - (ItemCount % BatchSize);
        for (int i = 0; i < total; i += BatchSize)
            pipe.EnqueueBatch(span);
        pipe.Stop(30_000);
        return pipe.Sum;
    }

    [Benchmark]
    public long Push_Single_SlowBackend()
    {
        // Simulated ~50-cycle backend work per item — representative of a tiny file/mem write.
        // Consumer becomes the bottleneck; mfence amortization on head advance is measurable.
        using var pipe = new TestSpscPipe(RingCapacity, backendSpinCycles: 50);
        pipe.Start();
        for (int i = 0; i < ItemCount; i++)
            pipe.Enqueue(in _item);
        pipe.Stop(30_000);
        return pipe.Sum;
    }

    [Benchmark]
    public long Push_Batch32_SlowBackend()
    {
        using var pipe = new TestSpscPipe(RingCapacity, backendSpinCycles: 50);
        pipe.Start();
        ReadOnlySpan<Entry64> span = _batch;
        int total = ItemCount - (ItemCount % BatchSize);
        for (int i = 0; i < total; i += BatchSize)
            pipe.EnqueueBatch(span);
        pipe.Stop(30_000);
        return pipe.Sum;
    }

    [Benchmark]
    public long MpscPush_Single()
    {
        using var pipe = new TestMpscPipe(RingCapacity, backendSpinCycles: 0);
        pipe.Start();
        for (int i = 0; i < ItemCount; i++)
            pipe.Enqueue(in _item);
        pipe.Stop(30_000);
        return pipe.Sum;
    }

    [Benchmark]
    public long MpscPush_Single_SlowBackend()
    {
        // Simulated ~50-cycle backend work per item — representative of a tiny file/mem write.
        using var pipe = new TestMpscPipe(RingCapacity, backendSpinCycles: 50);
        pipe.Start();
        for (int i = 0; i < ItemCount; i++)
            pipe.Enqueue(in _item);
        pipe.Stop(30_000);
        return pipe.Sum;
    }
}

/// <summary>
/// Trivial SPSC queue pipe: increments <see cref="Sum"/> on every consumed item.
/// No backend I/O — exercises pure ring + consumer-loop cost.
/// </summary>
internal sealed class TestSpscPipe : SpscQueueSink<Entry64>
{
    private readonly int _backendSpinCycles;
    public long Sum;

    public TestSpscPipe(int ringCapacity, int backendSpinCycles = 0)
        : base(ringCapacity, flushIntervalMs: 100, pipeName: "bench")
    {
        _backendSpinCycles = backendSpinCycles;
    }

    protected override void WriteToBackend(in Entry64 item)
    {
        Sum += item.A;
        if (_backendSpinCycles > 0) Thread.SpinWait(_backendSpinCycles);
    }

    protected override void FlushBackend() { }
    protected override void TryRecoverBackend() { }
    protected override void DisposeBackend() { }
}

/// <summary>
/// Trivial MPSC queue pipe: increments <see cref="Sum"/> on every consumed item.
/// No backend I/O — exercises pure MPSC ring + consumer-loop cost.
/// Single-producer use only here (BDN runs one-thread); multi-producer in Phase 7.
/// </summary>
internal sealed class TestMpscPipe : MpscQueueSink<Entry64>
{
    private readonly int _backendSpinCycles;
    public long Sum;

    public TestMpscPipe(int ringCapacity, int backendSpinCycles = 0)
        : base(ringCapacity, flushIntervalMs: 100, pipeName: "bench-mpsc")
    {
        _backendSpinCycles = backendSpinCycles;
    }

    protected override void WriteToBackend(in Entry64 item)
    {
        Sum += item.A;
        if (_backendSpinCycles > 0) Thread.SpinWait(_backendSpinCycles);
    }

    protected override void FlushBackend() { }
    protected override void TryRecoverBackend() { }
    protected override void DisposeBackend() { }
}
