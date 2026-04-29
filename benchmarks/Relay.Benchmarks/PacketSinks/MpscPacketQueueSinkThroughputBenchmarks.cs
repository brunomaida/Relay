using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.PacketSinks;

/// <summary>
/// End-to-end throughput: producer pushes N byte payloads into an <see cref="MpscQueueSink"/>
/// (packet base), consumer thread drains them via a trivial <c>WriteToBackend</c>. Measures
/// cumulative cost of MPSC byte-ring publish (CAS + header) + length-prefixed peek/advance
/// + consumer-loop cost. Single-producer only here; multi-producer contention is Phase 7.
/// </summary>
/// <remarks>Mirror of <see cref="QueueSinkPacketThroughputBenchmarks"/> for the MPSC byte ring.</remarks>
[MemoryDiagnoser]
public class MpscPacketQueueSinkThroughputBenchmarks
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
        using var sink = new TestMpscPacketSink(RingCapacity, backendSpinCycles: 0);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
        return sink.Sum;
    }

    [Benchmark]
    public long Push_Single_SlowBackend()
    {
        using var sink = new TestMpscPacketSink(RingCapacity, backendSpinCycles: 50);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
        return sink.Sum;
    }
}

/// <summary>
/// Trivial MPSC packet queue sink: increments <see cref="Sum"/> on every consumed payload.
/// No backend I/O — exercises pure MPSC byte-ring + consumer-loop cost. Single-producer
/// use here; multi-producer contention is Phase 7.
/// </summary>
internal sealed class TestMpscPacketSink : MpscQueueSink
{
    private readonly int _backendSpinCycles;
    public long Sum;

    public TestMpscPacketSink(int ringCapacity, int backendSpinCycles = 0)
        : base(ringCapacity, flushIntervalMs: 100, pipeName: "bench-mpsc-packet")
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
