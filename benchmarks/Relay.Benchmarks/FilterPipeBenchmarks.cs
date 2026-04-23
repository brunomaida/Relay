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
/// Downstream is deliberately SinkPipe (zero overhead) so that the delta Pass − Reject
/// represents exactly one additional downstream Enqueue call, not downstream backend cost.
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

        // Predicate always passes — every item forwarded downstream
        _filterPass = new FilterPipe<Entry64>(
            _ => true,
            new SinkPipe());

        // Predicate always rejects — every item silently consumed, no downstream call
        _filterReject = new FilterPipe<Entry64>(
            _ => false,
            new SinkPipe());
    }

    /// <summary>Predicate returns true: item forwarded to downstream SinkPipe.</summary>
    [Benchmark]
    public void Filter_Pass() => _filterPass.Enqueue(in _item);

    /// <summary>Predicate returns false: item consumed silently, no downstream Enqueue.</summary>
    [Benchmark(Baseline = true)]
    public void Filter_Reject() => _filterReject.Enqueue(in _item);
}
