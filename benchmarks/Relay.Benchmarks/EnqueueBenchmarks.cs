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
    private DispatchPipe<Entry64> _depth1Healthy = null!;
    private DispatchPipe<Entry64> _depth2AcceptReject = null!;
    private DispatchPipe<Entry64> _depth2HeadUnhealthy = null!;
    private DispatchPipe<Entry64> _depth3AllUnhealthy = null!;

    private Entry64 _item;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _item = new Entry64 { A = 1, B = 2 };

        // Depth 1: single healthy sink — minimal overhead baseline
        _depth1Healthy = new SinkPipe();

        // Depth 2: healthy head rejects item (Accept=false) → fallback to healthy tail
        _depth2AcceptReject = RelayBuilder
            .Start<Entry64, RejectPipe>(new RejectPipe())
            .To(new SinkPipe())
            .Build();

        // Depth 2: unhealthy head → healthy tail (1 fallback hop)
        _depth2HeadUnhealthy = RelayBuilder
            .Start<Entry64, DeadPipe>(new DeadPipe())
            .To(new SinkPipe())
            .Build();

        // Depth 3: all unhealthy → NullPipe terminal (2 fallback hops + silent drop)
        _depth3AllUnhealthy = RelayBuilder
            .Start<Entry64, DeadPipe>(new DeadPipe())
            .To(new DeadPipe())
            .To(new NullPipe<Entry64>())
            .Build();
    }

    /// <summary>Baseline: single healthy pipe, one Enqueue → Accept call.</summary>
    [Benchmark(Baseline = true)]
    public void Depth1_Healthy() => _depth1Healthy.Enqueue(in _item);

    /// <summary>Head healthy but rejects item (Accept=false): fallback hop via Next.</summary>
    [Benchmark]
    public void Depth2_AcceptReject() => _depth2AcceptReject.Enqueue(in _item);

    /// <summary>Head unhealthy: one fallback hop to healthy tail (+4c expected).</summary>
    [Benchmark]
    public void Depth2_HeadUnhealthy() => _depth2HeadUnhealthy.Enqueue(in _item);

    /// <summary>All pipes unhealthy: two fallback hops, silent drop at NullPipe (+8c expected).</summary>
    [Benchmark]
    public void Depth3_AllUnhealthy() => _depth3AllUnhealthy.Enqueue(in _item);
}
