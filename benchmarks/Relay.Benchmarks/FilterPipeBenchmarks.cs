using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks;

/// <summary>
/// Measures FilterPipe predicate cost on pass and reject paths.
/// Both paths return true from Accept — rejected items don't propagate to Next.
/// </summary>
/// <remarks>
/// The measured cost is delegate dispatch (Predicate&lt;T&gt; virtual stub), not closure allocation.
/// Zero allocation is expected on the hot path and verified by [MemoryDiagnoser].
/// Downstream is CounterPipe (Volatile.Write) to prevent JIT dead-code elimination on the pass path.
/// Delta Pass − Reject represents predicate-pass overhead + one downstream Enqueue with real work.
/// </remarks>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class FilterPipeBenchmarks
{
    private FilterPipe<Entry64> _filterPass = null!;
    private FilterPipe<Entry64> _filterReject = null!;
    private Entry64 _item;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _item = new Entry64 { A = 5, B = 10 };

        // Predicate always passes — CounterPipe downstream so JIT cannot eliminate the call
        _filterPass = new FilterPipe<Entry64>(
            _ => true,
            new CounterPipe());

        // Predicate always rejects — downstream never called; CounterPipe for symmetric setup
        _filterReject = new FilterPipe<Entry64>(
            _ => false,
            new CounterPipe());
    }

    /// <summary>Predicate returns true: item forwarded to downstream CounterPipe (Volatile.Write).</summary>
    [Benchmark]
    public void Filter_Pass() => _filterPass.Enqueue(in _item);

    /// <summary>Predicate returns false: item consumed silently, no downstream Enqueue.</summary>
    [Benchmark(Baseline = true)]
    public void Filter_Reject() => _filterReject.Enqueue(in _item);
}
