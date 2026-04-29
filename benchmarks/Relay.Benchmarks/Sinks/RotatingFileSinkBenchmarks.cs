using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="RotatingFileSink"/> consumer hot path — specifically the cost of
/// the per-record rotation predicate (<c>ShouldRotate</c>) which historically called
/// <c>DateTime.UtcNow.Date</c>. Used as a regression gate for the fix that caches the
/// next-day boundary in <c>HfClock</c> ticks.
/// </summary>
/// <remarks>
/// Driven via the <c>internal</c> <c>BenchInvokeWriteToBackend</c> accessor (visible through
/// <c>InternalsVisibleTo Relay.Benchmarks</c>) to isolate the consumer-thread cost from ring
/// publish + consumer-loop cost. Reflection is unusable here because <c>ReadOnlySpan&lt;byte&gt;</c>
/// parameters cannot be marshalled through <c>MethodInfo.Invoke</c>. The benchmark runs on a
/// single thread; no Start/Stop. Each invocation writes 64 bytes into the POH write buffer
/// and exercises the <c>ShouldRotate</c> predicate exactly once.
/// <para>
/// The accessor is a one-line forwarder so the JIT inlines straight into the
/// <c>WriteToBackend</c> body under measurement. The absolute number is not the gate — the
/// <b>ratio</b> (before-fix / after-fix mean ns) is what acceptance checks. Expected ratio
/// after the fix lands: ≥ 10x.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class RotatingFileSinkBenchmarks
{
    private RotatingFileSink _sink    = null!;
    private byte[]           _payload = new byte[64];
    private string           _dir     = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _dir = Path.Combine(Path.GetTempPath(), $"relay-bench-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_dir);
        _sink = new RotatingFileSink(_dir, "log",
            maxBytes:        1_000_000_000, // large — size-based rotation never triggers
            ringCapacity:    4096,
            flushIntervalMs: 50);
        // Do NOT call Start() — drive WriteToBackend directly via the internal accessor to
        // isolate ShouldRotate cost from ring/consumer-loop overhead.
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try { _sink?.Dispose(); }              catch { }
        try { Directory.Delete(_dir, true); }  catch { }
    }

    /// <summary>
    /// Single payload through <c>WriteToBackend</c> — exercises <c>ShouldRotate</c> exactly once.
    /// Before fix: dominated by <c>DateTime.UtcNow.Date</c> (~11 ns @ 4.5 GHz).
    /// After fix: single <c>HfClock.NowTicks</c> compare (~0.7 ns @ 4.5 GHz).
    /// </summary>
    [Benchmark]
    public void ShouldRotate_HotPath()
    {
        _sink.BenchInvokeWriteToBackend(_payload);
    }
}
