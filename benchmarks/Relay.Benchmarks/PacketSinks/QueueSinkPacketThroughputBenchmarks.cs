using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.PacketSinks;

/// <summary>
/// End-to-end throughput: producer pushes N byte payloads into an <see cref="SpscQueueSink"/>
/// (packet base), consumer thread drains them via a trivial <c>WriteToBackend</c>. Measures
/// cumulative cost of byte-ring publish + length-prefixed peek/advance + consumer-loop cost.
/// </summary>
/// <remarks>
/// Mirror of <see cref="QueuePipeThroughputBenchmarks"/>. Packet base does not expose a
/// batched <c>EnqueueBatch</c> API (variable-length records make a span-batch awkward), so
/// only single-publish variants are measured.
/// </remarks>
[MemoryDiagnoser]
public class QueueSinkPacketThroughputBenchmarks
{
    private byte[] _payload = null!;

    private const int RingCapacity = 65_536;
    private const int PayloadSize  = 64;

    [Params(100_000, 1_000_000)]
    public int ItemCount;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _payload = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++) _payload[i] = (byte)i;
    }

    [Benchmark(Baseline = true)]
    public long Push_Single()
    {
        using var sink = new TestSpscPacketSink(RingCapacity, backendSpinCycles: 0);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
        return sink.Sum;
    }

    [Benchmark]
    public long Push_Single_SlowBackend()
    {
        // Simulated ~50-cycle backend work per payload — representative of a tiny file/socket write.
        using var sink = new TestSpscPacketSink(RingCapacity, backendSpinCycles: 50);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
        return sink.Sum;
    }
}

/// <summary>
/// Trivial SPSC packet queue sink: increments <see cref="Sum"/> on every consumed payload.
/// No backend I/O — exercises pure byte-ring + consumer-loop cost.
/// </summary>
internal sealed class TestSpscPacketSink : SpscQueueSink
{
    private readonly int _backendSpinCycles;
    public long Sum;

    public TestSpscPacketSink(int ringCapacity, int backendSpinCycles = 0)
        : base(ringCapacity, flushIntervalMs: 100, pipeName: "bench-packet")
    {
        _backendSpinCycles = backendSpinCycles;
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        Sum += payload[0];
        if (_backendSpinCycles > 0) Thread.SpinWait(_backendSpinCycles);
    }

    protected override void FlushBackend() { }
    protected override void TryRecoverBackend() { }
    protected override void DisposeBackend() { }
}
