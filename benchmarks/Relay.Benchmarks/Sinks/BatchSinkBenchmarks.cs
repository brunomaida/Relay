using System;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="BatchSink"/> hot path. Drives the producer-side ring publish via
/// <see cref="PacketSink.Enqueue(System.ReadOnlySpan{byte})"/> on a no-op subclass — the
/// scratch-buffer accumulation logic in <c>WriteToBackend</c> is sealed, so the consumer-side
/// cost is covered indirectly via this BDN combined with <c>SpscQueueSink</c> packet
/// throughput (Phase 2). M5 closed under "covered indirectly" by design — driving
/// <c>WriteToBackend</c> from a subclass would require a production accessor change which
/// the plan flags as undesirable.
/// </summary>
[MemoryDiagnoser]
public class BatchSinkBenchmarks
{
    private TestBatchSink _sink    = null!;
    private byte[]        _payload = null!;

    [GlobalSetup]
    public void Setup()
    {
        _payload = new byte[64];
        for (int i = 0; i < 64; i++) _payload[i] = (byte)i;
        _sink = new TestBatchSink(ringCapacity: 65_536, batchCapacity: 65_536, flushIntervalMs: 100);
        // Start the consumer so the ring drains continuously — measuring steady-state
        // producer-side ring publish, not back-pressure / drop path. Consumer-side
        // WriteToBackend cost is not directly measurable without modifying production
        // source; documented in M5 note.
        _sink.Start();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _sink.Stop(2_000);
        _sink.Dispose();
    }

    /// <summary>Ring publish via Enqueue — IsHealthy + Accept (TryPublish on the SPSC byte ring).</summary>
    [Benchmark]
    public void Enqueue_RingPublish() => _sink.Enqueue(_payload);
}

/// <summary>
/// Test BatchSink with a no-op <c>OnFlush</c>. Concrete instance that drives the inherited
/// ring publish; consumer thread runs to keep the ring drained at steady state.
/// </summary>
internal sealed class TestBatchSink : BatchSink
{
    public TestBatchSink(int ringCapacity, int batchCapacity, int flushIntervalMs)
        : base(ringCapacity, batchCapacity, flushIntervalMs, "bench-batch") { }

    protected override void OnFlush(ReadOnlySpan<byte> batch) { /* no-op */ }
}
