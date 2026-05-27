using System;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Relay.Tests.Circular.Helpers;

/// <summary>
/// Collects telemetry during a circular ring test and prints a summary to <see cref="ITestOutputHelper"/>.
/// </summary>
/// <remarks>
/// <para>Threading: <see cref="Record"/> is called from a single timing thread. No concurrent access to any field.</para>
/// <para>Zero allocations in <see cref="Record"/> — all arrays are pre-allocated in the constructor.</para>
/// </remarks>
internal sealed class RingTestReport
{
    private readonly ITestOutputHelper _out;
    private readonly long[] _snapshots;
    private int _snapshotCount;
    private long _lastCount;
    private long _lastTicks;
    private int _gen0Before;
    private int _gen1Before;
    private int _gen2Before;
    private long _allocBefore;
    private long _peakWorkingSet;

    /// <param name="output">xUnit output sink.</param>
    /// <param name="maxSnapshots">Maximum number of 1-second throughput snapshots to retain.</param>
    public RingTestReport(ITestOutputHelper output, int maxSnapshots = 120)
    {
        _out = output;
        _snapshots = new long[maxSnapshots];
    }

    /// <summary>
    /// Captures baseline GC and allocation counters, and initialises the timing state.
    /// Must be called immediately before the ring starts producing.
    /// </summary>
    public void Start()
    {
        _lastTicks = Stopwatch.GetTimestamp();
        _lastCount = 0;
        _snapshotCount = 0;
        _gen0Before = GC.CollectionCount(0);
        _gen1Before = GC.CollectionCount(1);
        _gen2Before = GC.CollectionCount(2);
        _allocBefore = GC.GetTotalAllocatedBytes(precise: false);
    }

    /// <summary>
    /// Records a throughput snapshot. Zero-allocation. Called approximately once per second from a timing thread.
    /// </summary>
    /// <param name="currentTotalCount">Total number of items delivered since the test started.</param>
    public void Record(long currentTotalCount)
    {
        long now = Stopwatch.GetTimestamp();
        long elapsed = now - _lastTicks;
        long delta = currentTotalCount - _lastCount;
        long freq = Stopwatch.Frequency;

        if (elapsed > 0 && _snapshotCount < _snapshots.Length)
            _snapshots[_snapshotCount++] = delta * freq / elapsed;

        _lastTicks = now;
        _lastCount = currentTotalCount;
    }

    /// <summary>
    /// Captures the process peak working set. Cold path — call once after the ring is stopped.
    /// </summary>
    public void Stop()
    {
        _peakWorkingSet = Process.GetCurrentProcess().PeakWorkingSet64;
    }

    /// <summary>
    /// Formats and writes the telemetry summary to the output helper.
    /// </summary>
    /// <param name="label">Test label printed as the section header.</param>
    /// <param name="totalItems">Total items processed during the test run.</param>
    public void Print(string label, long totalItems)
    {
        int gen0Delta = GC.CollectionCount(0) - _gen0Before;
        int gen1Delta = GC.CollectionCount(1) - _gen1Before;
        int gen2Delta = GC.CollectionCount(2) - _gen2Before;
        long allocDelta = GC.GetTotalAllocatedBytes(precise: false) - _allocBefore;

        _out.WriteLine($"=== {label} ===");
        _out.WriteLine($"Items total  : {totalItems:N0}");

        if (_snapshotCount == 0)
        {
            _out.WriteLine("Throughput   : (no snapshots)");
        }
        else
        {
            long min = _snapshots[0];
            long max = _snapshots[0];
            long sum = 0;

            for (int i = 0; i < _snapshotCount; i++)
            {
                long s = _snapshots[i];
                if (s < min) min = s;
                if (s > max) max = s;
                sum += s;
            }

            double avg = (double)sum / _snapshotCount;
            double variance = 0;
            for (int i = 0; i < _snapshotCount; i++)
            {
                double diff = _snapshots[i] - avg;
                variance += diff * diff;
            }
            long stddev = (long)Math.Sqrt(variance / _snapshotCount);

            _out.WriteLine($"Throughput   : min={min:N0} avg={(long)avg:N0} max={max:N0} stddev={stddev:N0} msg/s");
        }

        _out.WriteLine($"GC delta     : gen0={gen0Delta} gen1={gen1Delta} gen2={gen2Delta}");
        _out.WriteLine($"Alloc delta  : {allocDelta:N0} bytes");
        _out.WriteLine($"Peak WS      : {_peakWorkingSet:N0} bytes");
        _out.WriteLine($"Snapshots    : {_snapshotCount}");
    }
}
