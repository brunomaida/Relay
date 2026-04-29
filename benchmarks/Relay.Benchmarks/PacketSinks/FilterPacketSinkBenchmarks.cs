using System;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.PacketSinks;

/// <summary>
/// Measures <see cref="FilterSink"/> (packet) <see cref="PacketPredicate"/> cost on pass and
/// reject paths. Both paths return true from <c>Accept</c> — rejected payloads do not propagate
/// to <c>Next</c>.
/// </summary>
/// <remarks>Mirror of <see cref="FilterSinkBenchmarks"/> for the packet hierarchy.</remarks>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class FilterPacketSinkBenchmarks
{
    private FilterSink _filterPass   = null!;
    private FilterSink _filterReject = null!;
    private byte[]     _payload      = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _payload    = new byte[64];
        _payload[0] = 5;
        _payload[1] = 10;

        // Predicate always passes — ByteCounterPipe downstream so JIT cannot eliminate the call.
        _filterPass = new FilterSink(_ => true, new ByteCounterPipe());

        // Predicate always rejects — downstream never called.
        _filterReject = new FilterSink(_ => false, new ByteCounterPipe());
    }

    [Benchmark]
    public void Filter_Packet_Pass() => _filterPass.Enqueue(_payload);

    [Benchmark(Baseline = true)]
    public void Filter_Packet_Reject() => _filterReject.Enqueue(_payload);
}
