using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks;

/// <summary>
/// Compares FanOutPipe.Enqueue (array loop + virtual dispatch) vs FanOut2Pipe.Enqueue (CRTP).
/// Expected: FanOut2Pipe saves ~6c due to JIT devirtualizing Accept within each inlined Enqueue call.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class FanOutEnqueueBenchmarks
{
    private FanOutPipe<Entry64> _fanOut = null!;
    private FanOut2Pipe<Entry64, SinkPipe, SinkPipe> _fanOut2 = null!;
    private Entry64 _item;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _item = new Entry64 { A = 3, B = 7 };
        _fanOut = new FanOutPipe<Entry64>(new SinkPipe(), new SinkPipe());
        _fanOut2 = new FanOut2Pipe<Entry64, SinkPipe, SinkPipe>(new SinkPipe(), new SinkPipe());
    }

    /// <summary>FanOutPipe.Enqueue: array loop + virtual call per child.</summary>
    [Benchmark(Baseline = true)]
    public void FanOut_Enqueue() => _fanOut.Enqueue(in _item);

    /// <summary>FanOut2Pipe.Enqueue: CRTP — JIT devirtualizes Accept within each inlined Enqueue call.</summary>
    [Benchmark]
    public void FanOut2_Enqueue() => _fanOut2.Enqueue(in _item);
}

/// <summary>
/// Compares FanOutPipe.IsHealthy (OR-reduction loop) vs FanOut2Pipe.IsHealthy (short-circuit field access).
/// Consult DisassemblyDiagnoser output to confirm the JIT did not constant-fold IsHealthy to true.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class FanOutIsHealthyBenchmarks
{
    private FanOutPipe<Entry64> _fanOut = null!;
    private FanOut2Pipe<Entry64, SinkPipe, SinkPipe> _fanOut2 = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _fanOut = new FanOutPipe<Entry64>(new SinkPipe(), new SinkPipe());
        _fanOut2 = new FanOut2Pipe<Entry64, SinkPipe, SinkPipe>(new SinkPipe(), new SinkPipe());
    }

    /// <summary>FanOutPipe.IsHealthy: OR-reduction loop over children array.</summary>
    [Benchmark(Baseline = true)]
    public bool FanOut_IsHealthy() => _fanOut.IsHealthy;

    /// <summary>FanOut2Pipe.IsHealthy: short-circuit OR of two concrete sealed fields.</summary>
    [Benchmark]
    public bool FanOut2_IsHealthy() => _fanOut2.IsHealthy;
}
