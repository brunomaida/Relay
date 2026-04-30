using System;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="RamSink"/> (packet) <c>Accept</c> hot path — synchronous in-memory fill.
/// No consumer thread; the sink is fill-once until <c>DrainTo</c> resets pointers.
/// </summary>
/// <remarks>
/// The buffer fills monotonically during the benchmark. Once exhausted, <see cref="RamSink.Accept"/>
/// returns false and <see cref="PacketSink.Enqueue"/> falls through to <c>_dropCount</c>
/// increment — both paths are sub-10 ns. The reported mean is a stable mix once steady state
/// is reached. <see cref="GlobalSetup"/> sizes capacity for the BDN window without recycling.
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
        // 256MB ring — large enough that the Accept fast path dominates BDN's invocation window.
        _sink    = new RamSink(capacity: 256 * 1024 * 1024);
        _payload = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++) _payload[i] = (byte)i;
    }

    [GlobalCleanup]
    public void Cleanup() => _sink.Dispose();

    /// <summary>Single payload through Accept — bounds check + uint header + CopyBlock + Volatile.Write.</summary>
    [Benchmark]
    public void Accept_Single() => _sink.Enqueue(_payload);
}
