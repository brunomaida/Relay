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
/// Two benchmarks:
/// <list type="bullet">
///   <item><description>
///     <c>ShouldRotate_Predicate</c> — isolates the predicate via the
///     <c>BenchInvokeShouldRotate</c> accessor. This is the regression gate for the
///     <c>DateTime.UtcNow.Date</c> removal.
///   </description></item>
///   <item><description>
///     <c>ShouldRotate_HotPath</c> — drives full <c>WriteToBackend</c> (predicate + buffer
///     copy + bookkeeping). Realistic per-record consumer cost; predicate savings are
///     diluted by the ~30-50 ns memcpy.
///   </description></item>
/// </list>
/// Reflection is unusable for either accessor because <c>ReadOnlySpan&lt;byte&gt;</c>
/// parameters cannot be marshalled through <c>MethodInfo.Invoke</c>; both accessors are
/// <c>internal</c> forwarders visible via <c>InternalsVisibleTo Relay.Benchmarks</c>.
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
        // Do NOT call Start() — drive accessors directly to isolate predicate / WriteToBackend
        // from ring/consumer-loop overhead.
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try { _sink?.Dispose(); }              catch { }
        try { Directory.Delete(_dir, true); }  catch { }
    }

    /// <summary>
    /// Isolated <c>ShouldRotate</c> predicate — the rotation gate without surrounding
    /// buffer copy. Before fix: dominated by <c>DateTime.UtcNow.Date</c> (~7-11 ns).
    /// After fix: bounds check + cached <c>HfClock</c>-tick compare (~1-2 ns).
    /// </summary>
    [Benchmark]
    public bool ShouldRotate_Predicate()
    {
        return _sink.BenchInvokeShouldRotate(64);
    }

    /// <summary>
    /// Full <c>WriteToBackend</c> — predicate + buffer copy + bookkeeping. Realistic
    /// per-record consumer cost; the predicate savings are diluted by the ~30-50 ns memcpy.
    /// </summary>
    [Benchmark]
    public void ShouldRotate_HotPath()
    {
        _sink.BenchInvokeWriteToBackend(_payload);
    }
}
