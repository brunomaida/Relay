using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.PacketSinks;

/// <summary>
/// Measures <see cref="MultiSink"/> (packet, N=2) Enqueue cost vs the CRTP
/// <see cref="Multi2PacketSink{TC1,TC2}"/> variant landed in Phase 6.
/// </summary>
/// <remarks>Mirror of <see cref="MultiEnqueueBenchmarks"/>.</remarks>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class MultiPacketEnqueueBenchmarks
{
    private MultiSink                                       _multi   = null!;
    private Multi2PacketSink<ByteCounterPipe, ByteCounterPipe> _multi2 = null!;
    private byte[]                                          _payload = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _payload    = new byte[64];
        _payload[0] = 3;
        _payload[1] = 7;
        _multi      = new MultiSink(new ByteCounterPipe(), new ByteCounterPipe());
        _multi2     = new Multi2PacketSink<ByteCounterPipe, ByteCounterPipe>(
            new ByteCounterPipe(), new ByteCounterPipe());
    }

    [Benchmark(Baseline = true)]
    public void Multi_Packet_Enqueue() => _multi.Enqueue(_payload);

    [Benchmark]
    public void Multi2_Packet_Enqueue() => _multi2.Enqueue(_payload);
}

/// <summary>
/// Measures <see cref="MultiSink.IsHealthy"/> short-circuit OR-reduction on packet hierarchy.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class MultiPacketIsHealthyBenchmarks
{
    private MultiSink _multi = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // ByteCounterPipe is always healthy — short-circuit on first child.
        _multi = new MultiSink(new ByteCounterPipe(), new ByteCounterPipe());
    }

    [Benchmark(Baseline = true)]
    public bool Multi_Packet_IsHealthy() => _multi.IsHealthy;
}
