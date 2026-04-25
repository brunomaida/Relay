using System;
using System.Runtime.CompilerServices;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks;

/// <summary>
/// Measures <see cref="PacketSink.Enqueue"/> throughput across chain configurations.
/// Healthy = payload consumed at first pipe; Unhealthy/Reject = fallback hop to Next.
/// Fixed 64-byte payload matches <see cref="Entry64"/> in the typed suite for direct A/B comparison.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class ByteEnqueueBenchmarks
{
    private PacketSink _depth1Healthy        = null!;
    private PacketSink _depth2AcceptReject   = null!;
    private PacketSink _depth2HeadUnhealthy  = null!;
    private PacketSink _depth3AllUnhealthy   = null!;

    private byte[] _payload = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _payload    = new byte[64];
        _payload[0] = 1;
        _payload[1] = 2;

        // Depth 1: single ByteCounterPipe — Volatile.Write prevents DCE, real baseline cost.
        _depth1Healthy = new ByteCounterPipe();

        // Depth 2: ByteRejectPipe (Accept=false) ? ByteCounterPipe — 1 accept-reject hop.
        var head1 = new ByteRejectPipe();
        head1.Next = new ByteCounterPipe();
        _depth2AcceptReject = head1;

        // Depth 2: ByteDeadPipe (IsHealthy=false) ? ByteCounterPipe — 1 IsHealthy-miss hop.
        var head2 = new ByteDeadPipe();
        head2.Next = new ByteCounterPipe();
        _depth2HeadUnhealthy = head2;

        // Depth 3: ByteDeadPipe ? ByteDeadPipe ? ByteCounterPipe — 2 fallback hops.
        var head3 = new ByteDeadPipe();
        var mid3  = new ByteDeadPipe();
        head3.Next = mid3;
        mid3.Next  = new ByteCounterPipe();
        _depth3AllUnhealthy = head3;
    }

    /// <summary>Baseline: single ByteCounterPipe — IsHealthy + Accept + Volatile.Write.</summary>
    [Benchmark(Baseline = true)]
    public void Depth1_Byte_Healthy() => _depth1Healthy.Enqueue(_payload);

    /// <summary>Head healthy but rejects (Accept=false): 1 hop to ByteCounterPipe terminal.</summary>
    [Benchmark]
    public void Depth2_Byte_AcceptReject() => _depth2AcceptReject.Enqueue(_payload);

    /// <summary>Head unhealthy (IsHealthy=false): 1 hop to ByteCounterPipe terminal.</summary>
    [Benchmark]
    public void Depth2_Byte_HeadUnhealthy() => _depth2HeadUnhealthy.Enqueue(_payload);

    /// <summary>Two unhealthy hops, ByteCounterPipe terminal — measures cumulative fallback cost.</summary>
    [Benchmark]
    public void Depth3_Byte_AllUnhealthy() => _depth3AllUnhealthy.Enqueue(_payload);
}

#region Helper pipes (byte variants — parallel to BenchmarkTypes.cs typed helpers)

/// <summary>
/// Healthy no-op sink for byte chains. Volatile.Write on first payload byte prevents
/// JIT dead-code elimination while keeping hot-path cost observable.
/// </summary>
internal sealed class ByteCounterPipe : PacketSink
{
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

/// <summary>Always-unhealthy pipe — forces every Enqueue to fall through to Next.</summary>
internal sealed class ByteDeadPipe : PacketSink
{
    public override bool IsHealthy => false;

    // Accept is never called: IsHealthy == false short-circuits Enqueue.
    protected override bool Accept(ReadOnlySpan<byte> payload) => true;

    public override void Flush()   { }
    public override void Dispose() { }
}

/// <summary>Healthy pipe that rejects every payload — forces fallback via Accept returning false.</summary>
internal sealed class ByteRejectPipe : PacketSink
{
    public override bool IsHealthy => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload) => false;

    public override void Flush()   { }
    public override void Dispose() { }
}

#endregion
