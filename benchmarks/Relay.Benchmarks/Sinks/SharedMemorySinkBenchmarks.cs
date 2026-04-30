using System;
using System.Runtime.Versioning;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="SharedMemorySink"/> Accept hot path — CAS loop on WriteIndex +
/// 2x modular WriteRing. Windows-only (named MMF).
/// </summary>
[SupportedOSPlatform("windows")]
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class SharedMemorySinkBenchmarks
{
    private SharedMemorySink _sink    = null!;
    private byte[]           _payload = null!;
    private string           _name    = string.Empty;

    /// <summary>Payload size in bytes (64B and 256B variants).</summary>
    [Params(64, 256)]
    public int PayloadSize;

    [GlobalSetup]
    public void Setup()
    {
        _name    = "Local\\relay-bench-" + Guid.NewGuid().ToString("N");
        _sink    = new SharedMemorySink(_name, totalCapacity: 4 * 1024 * 1024);
        _payload = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++) _payload[i] = (byte)i;
    }

    [GlobalCleanup]
    public void Cleanup() => _sink.Dispose();

    /// <summary>Single payload through Accept — CAS WriteIndex + modular WriteRing x2.</summary>
    [Benchmark]
    public void Accept_Single() => _sink.Enqueue(_payload);
}
