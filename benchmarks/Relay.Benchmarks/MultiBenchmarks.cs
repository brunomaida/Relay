using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks;

/// <summary>
/// Compares MultiPipe.Enqueue (array loop + virtual dispatch) vs Multi2Pipe.Enqueue (CRTP generic).
/// Children are CounterPipe (Volatile.Write) to prevent JIT dead-code elimination.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class MultiEnqueueBenchmarks
{
    private MultiPipe<Entry64> _multi = null!;
    private Multi2Pipe<Entry64, CounterPipe, CounterPipe> _multi2 = null!;
    private Entry64 _item;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _item = new Entry64 { A = 3, B = 7 };
        _multi  = new MultiPipe<Entry64>(new CounterPipe(), new CounterPipe());
        _multi2 = new Multi2Pipe<Entry64, CounterPipe, CounterPipe>(new CounterPipe(), new CounterPipe());
    }

    /// <summary>MultiPipe.Enqueue: array loop + virtual call per child.</summary>
    [Benchmark(Baseline = true)]
    public void Multi_Enqueue() => _multi.Enqueue(in _item);

    /// <summary>Multi2Pipe.Enqueue: CRTP generic — shared-code JIT, devirtualization not guaranteed.</summary>
    [Benchmark]
    public void Multi2_Enqueue() => _multi2.Enqueue(in _item);
}

/// <summary>
/// Compares MultiPipe.IsHealthy (OR-reduction loop) vs Multi2Pipe.IsHealthy (short-circuit field access).
/// Consult DisassemblyDiagnoser output to confirm the JIT did not constant-fold IsHealthy to true.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class MultiIsHealthyBenchmarks
{
    private MultiPipe<Entry64> _multi = null!;
    private Multi2Pipe<Entry64, SinkPipe, SinkPipe> _multi2 = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _multi  = new MultiPipe<Entry64>(new SinkPipe(), new SinkPipe());
        _multi2 = new Multi2Pipe<Entry64, SinkPipe, SinkPipe>(new SinkPipe(), new SinkPipe());
    }

    /// <summary>MultiPipe.IsHealthy: OR-reduction loop over children array.</summary>
    [Benchmark(Baseline = true)]
    public bool Multi_IsHealthy() => _multi.IsHealthy;

    /// <summary>Multi2Pipe.IsHealthy: short-circuit OR of two concrete sealed fields.</summary>
    [Benchmark]
    public bool Multi2_IsHealthy() => _multi2.IsHealthy;
}
