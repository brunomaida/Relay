using System;
using BenchmarkDotNet.Attributes;
using Relay;
using Relay.Sinks;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="RamSink"/> (packet) <c>Accept</c> hot path — synchronous in-memory fill.
/// No consumer thread; the sink is fill-once until <c>DrainTo</c> resets pointers.
/// </summary>
/// <remarks>
/// <see cref="IterationSetup"/> drains the buffer to <see cref="NullSink.Instance"/> before
/// every BDN iteration so capacity is reset and <see cref="RamSink.Accept"/> stays on the
/// happy path (<c>_tail + recordSize &lt;= _capacity</c>) regardless of how many invocations
/// BDN runs per iteration.
/// </remarks>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class RamPacketSinkBenchmarks
{
    private RamSink _sink    = null!;
    private byte[]  _payload = null!;

    /// <summary>Payload size in bytes (64B and 256B variants).</summary>
    [Params(64, 256)]
    public int PayloadSize;

    [GlobalSetup]
    public void Setup()
    {
        // 64MB ring — drained to NullSink between iterations to keep Accept on the fast path.
        _sink    = new RamSink(capacity: 64 * 1024 * 1024);
        _payload = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++) _payload[i] = (byte)i;
    }

    [IterationSetup]
    public void IterationSetup() => _sink.DrainTo(NullSink.Instance);

    [GlobalCleanup]
    public void Cleanup() => _sink.Dispose();

    /// <summary>Single payload through Accept — bounds check + uint header + CopyBlock + Volatile.Write.</summary>
    [Benchmark]
    public void Accept_Single() => _sink.Enqueue(_payload);
}
