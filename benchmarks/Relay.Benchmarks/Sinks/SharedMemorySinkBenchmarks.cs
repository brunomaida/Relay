using System;
using System.Runtime.Versioning;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="SharedMemorySpscSink"/> Accept hot path —
/// SPSC Volatile.Read on WriteIndex + Thread.MemoryBarrier + Volatile.Write.
/// Windows-only (named MMF).
/// </summary>
/// <remarks>
/// Pre-fix (CAS-before-write) vs post-fix (write-barrier-publish) expected delta:
/// ~10–30 cycles per frame for the explicit <see cref="System.Threading.Thread.MemoryBarrier"/>
/// (Codex estimate). On x86/x64, Volatile.Write already implies a store-barrier; the explicit
/// Thread.MemoryBarrier adds belt-and-suspenders portability overhead — ARM64 will see a larger
/// delta (explicit dmb ish vs implicit stlr).
/// </remarks>
[SupportedOSPlatform("windows")]
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class SharedMemorySinkBenchmarks
{
    private SharedMemorySpscSink _sink    = null!;
    private byte[]               _payload = null!;
    private string               _name    = string.Empty;

    /// <summary>Payload size in bytes (64B and 256B variants).</summary>
    [Params(64, 256)]
    public int PayloadSize;

    [GlobalSetup]
    public void Setup()
    {
        _name    = "Local\\relay-bench-" + Guid.NewGuid().ToString("N");
        _sink    = new SharedMemorySpscSink(_name, totalCapacity: 4 * 1024 * 1024);
        _payload = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++) _payload[i] = (byte)i;
    }

    [GlobalCleanup]
    public void Cleanup() => _sink.Dispose();

    /// <summary>Single payload through Accept — SPSC Volatile.Read WriteIndex + payload write + barrier + Volatile.Write.</summary>
    [Benchmark]
    public void Accept_Single() => _sink.Enqueue(_payload);
}
