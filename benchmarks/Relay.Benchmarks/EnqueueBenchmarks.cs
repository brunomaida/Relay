using BenchmarkDotNet.Attributes;
using Relay;
using Relay.Builder;

namespace Relay.Benchmarks;

/// <summary>
/// Measures Enqueue throughput across chain configurations.
/// Healthy = item consumed at first pipe; Unhealthy = fallback hop to Next.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class EnqueueBenchmarks
{
    private DispatchSink<Entry64> _depth1Healthy = null!;
    private DispatchSink<Entry64> _depth2AcceptReject = null!;
    private DispatchSink<Entry64> _depth2HeadUnhealthy = null!;
    private DispatchSink<Entry64> _depth3AllUnhealthy = null!;

    private Entry64 _item;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _item = new Entry64 { A = 1, B = 2 };

        // Depth 1: single CounterPipe — Volatile.Write prevents DCE, real baseline cost
        _depth1Healthy = new CounterPipe();

        // Depth 2: RejectPipe (Accept=false) → CounterPipe — 1 accept-reject hop
        _depth2AcceptReject = RelayBuilder
            .Start<Entry64, RejectPipe>(new RejectPipe())
            .To(new CounterPipe())
            .Build();

        // Depth 2: DeadPipe (IsHealthy=false) → CounterPipe — 1 IsHealthy-miss hop
        _depth2HeadUnhealthy = RelayBuilder
            .Start<Entry64, DeadPipe>(new DeadPipe())
            .To(new CounterPipe())
            .Build();

        // Depth 3: DeadPipe → DeadPipe → CounterPipe — 2 fallback hops, real terminal work
        _depth3AllUnhealthy = RelayBuilder
            .Start<Entry64, DeadPipe>(new DeadPipe())
            .To(new DeadPipe())
            .To(new CounterPipe())
            .Build();
    }

    /// <summary>Baseline: single CounterPipe — IsHealthy + Accept + Volatile.Write.</summary>
    [Benchmark(Baseline = true)]
    public void Depth1_Healthy() => _depth1Healthy.Enqueue(in _item);

    /// <summary>Head healthy but rejects (Accept=false): 1 hop to CounterPipe terminal.</summary>
    [Benchmark]
    public void Depth2_AcceptReject() => _depth2AcceptReject.Enqueue(in _item);

    /// <summary>Head unhealthy (IsHealthy=false): 1 hop to CounterPipe terminal.</summary>
    [Benchmark]
    public void Depth2_HeadUnhealthy() => _depth2HeadUnhealthy.Enqueue(in _item);

    /// <summary>Two unhealthy hops, CounterPipe terminal — measures cumulative fallback cost.</summary>
    [Benchmark]
    public void Depth3_AllUnhealthy() => _depth3AllUnhealthy.Enqueue(in _item);
}
