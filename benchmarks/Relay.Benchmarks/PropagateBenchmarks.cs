using System.Runtime.CompilerServices;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks;

/// <summary>
/// Measures the overhead of <see cref="DispatchSink{T}.PropagateAfterAccept"/> on the default
/// (false) path and the tee (always-propagate) path.
/// <para>
/// Performance gate: <see cref="Depth1_Healthy_Default"/> must stay within ~5% of
/// <see cref="EnqueueBenchmarks.Depth1_Healthy"/> (reference: 0.226 ns full-job / 0.215 ns
/// short-job on Intel i9-12900K, hot caches). Adding the PropagateAfterAccept virtual property
/// to the base class must not regress the default dispatch path.
/// </para>
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class PropagateBenchmarks
{
    private DispatchSink<Entry64> _depth1Default       = null!;
    private DispatchSink<Entry64> _depth1PropagateOnly = null!;
    private DispatchSink<Entry64> _depth2PropagateFork = null!;
    private DispatchSink<Entry64> _depth2ForkWrapped   = null!;

    private Entry64 _item;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _item = new Entry64 { A = 1, B = 2 };

        // Depth 1: single CounterPipe — baseline, PropagateAfterAccept=false (default path).
        _depth1Default = new CounterPipe();

        // Depth 1: single PropagateCounterPipe, no Next — measures pure propagate-branch cost.
        // Next?.Enqueue is issued but the null-check short-circuits without dispatch.
        _depth1PropagateOnly = new PropagateCounterPipe();

        // Depth 2: PropagateCounterPipe → CounterPipe — both receive the item.
        var sink2a = new CounterPipe();
        var prop2  = new PropagateCounterPipe();
        prop2.Next = sink2a;
        _depth2PropagateFork = prop2;

        // Depth 2: ForkSink(CounterPipe) → CounterPipe — actual ForkSink cost vs. custom propagate.
        var primaryCounter = new CounterPipe();
        var auditCounter   = new CounterPipe();
        var fork           = new ForkSink<Entry64>(primaryCounter);
        fork.Next          = auditCounter;
        _depth2ForkWrapped = fork;
    }

    /// <summary>
    /// Baseline: single CounterPipe (default PropagateAfterAccept=false).
    /// Gate: must stay within ~5% of EnqueueBenchmarks.Depth1_Healthy.
    /// </summary>
    [Benchmark(Baseline = true)]
    public void Depth1_Healthy_Default() => _depth1Default.Enqueue(in _item);

    /// <summary>
    /// Single PropagateCounterPipe with no Next. After Accept, Next?.Enqueue null-checks and
    /// exits. Measures the pure cost of the propagate branch against the default baseline.
    /// </summary>
    [Benchmark]
    public void Depth1_Healthy_Propagate_NoNext() => _depth1PropagateOnly.Enqueue(in _item);

    /// <summary>
    /// PropagateCounterPipe → CounterPipe: both pipes receive the item. Measures the fork
    /// pattern using a hand-rolled propagate override.
    /// </summary>
    [Benchmark]
    public void Depth2_Propagate_Fork() => _depth2PropagateFork.Enqueue(in _item);

    /// <summary>
    /// ForkSink(CounterPipe) → CounterPipe: actual ForkSink wiring. Compare to
    /// Depth2_Propagate_Fork to verify ForkSink adds no measurable overhead over a custom pipe.
    /// </summary>
    [Benchmark]
    public void Depth2_Fork_Wrapped() => _depth2ForkWrapped.Enqueue(in _item);
}

/// <summary>
/// Healthy propagate pipe with an observable side-effect — prevents JIT dead-code elimination.
/// PropagateAfterAccept=true ensures Next is always called after a successful accept.
/// </summary>
internal sealed class PropagateCounterPipe : DispatchSink<Entry64>
{
    public long LastValue;

    public override bool IsHealthy => true;

    protected override bool PropagateAfterAccept => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in Entry64 item)
    {
        Volatile.Write(ref LastValue, item.A);
        return true;
    }

    public override void Flush() { }
    public override void Dispose() { }
}
