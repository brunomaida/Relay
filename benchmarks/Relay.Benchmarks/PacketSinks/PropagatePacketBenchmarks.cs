using System;
using System.Runtime.CompilerServices;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.PacketSinks;

/// <summary>
/// Measures the overhead of packet-side <see cref="PacketSink.Enqueue"/> propagation
/// — default no-propagate path, single-sink propagate-no-Next, and <see cref="ForkSink"/>
/// (primary + Next).
/// </summary>
/// <remarks>
/// Mirror of <see cref="PropagateBenchmarks"/> for the packet hierarchy. Uses
/// <c>ByteCounterPipe</c> (defined in <see cref="ByteEnqueueBenchmarks"/>) as the trivial sink.
/// </remarks>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class PropagatePacketBenchmarks
{
    private PacketSink _depth1Default       = null!;
    private PacketSink _depth1PropagateOnly = null!;
    private PacketSink _depth2PropagateFork = null!;
    private PacketSink _depth2ForkWrapped   = null!;

    private byte[] _payload = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _payload    = new byte[64];
        _payload[0] = 1;
        _payload[1] = 2;

        // Depth 1: single ByteCounterPipe — baseline, default PropagateAfterAccept=false.
        _depth1Default = new ByteCounterPipe();

        // Depth 1: PropagateByteCounterPipe with no Next — pure propagate-branch cost vs default.
        _depth1PropagateOnly = new PropagateByteCounterPipe();

        // Depth 2: PropagateByteCounterPipe → ByteCounterPipe — both receive the payload.
        var prop2 = new PropagateByteCounterPipe();
        prop2.Next = new ByteCounterPipe();
        _depth2PropagateFork = prop2;

        // Depth 2: ForkSink(ByteCounterPipe) → ByteCounterPipe — actual ForkSink cost vs custom propagate.
        var primaryCounter = new ByteCounterPipe();
        var auditCounter   = new ByteCounterPipe();
        var fork           = new ForkSink(primaryCounter);
        fork.Next          = auditCounter;
        _depth2ForkWrapped = fork;
    }

    [Benchmark(Baseline = true)]
    public void Depth1_Healthy_Default() => _depth1Default.Enqueue(_payload);

    [Benchmark]
    public void Depth1_Healthy_Propagate_NoNext() => _depth1PropagateOnly.Enqueue(_payload);

    [Benchmark]
    public void Depth2_Propagate_Fork() => _depth2PropagateFork.Enqueue(_payload);

    [Benchmark]
    public void Depth2_Fork_Wrapped() => _depth2ForkWrapped.Enqueue(_payload);
}

/// <summary>
/// Healthy propagate sink with an observable side-effect — prevents JIT dead-code elimination.
/// PropagateAfterAccept=true ensures Next is always called after a successful accept.
/// </summary>
internal sealed class PropagateByteCounterPipe : PacketSink
{
    public PropagateByteCounterPipe() : base(propagateAfterAccept: true) { }

    public long LastValue;

    public override bool IsHealthy => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        Volatile.Write(ref LastValue, payload[0]);
        return true;
    }

    public override void Flush()   { }
    public override void Dispose() { }
}
