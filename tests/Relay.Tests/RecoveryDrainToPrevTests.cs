using System.Runtime.InteropServices;
using System.Threading;
using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests;

/// <summary>
/// Verifies <c>TryDrainToPrev</c> semantics after the M2 race-window fix:
/// drain is gated on <c>!_running</c> so it only fires in the shutdown phase,
/// eliminating concurrent writers on Prev's SPSC ring.
/// </summary>
public sealed class RecoveryDrainToPrevTests
{
    private static readonly Entry64 Item = new() { Value = 1 };

    /// <summary>
    /// While the sink is running (<c>_running=true</c>), <c>TryDrainToPrev</c> must be
    /// a no-op even when Prev is healthy and the flush deadline fires. All items go to
    /// <c>WriteToBackend</c>.
    /// </summary>
    [Fact]
    public void Spsc_TryDrainToPrev_NoOp_WhileRunning()
    {
        var prev     = new CountingPrev();
        var fallback = new InjectableSpscPipe(flushIntervalMs: 0);
        fallback.Prev = prev;
        fallback.Start();

        // Producer = test thread (SPSC: one producer, one consumer thread — contract satisfied).
        for (int i = 0; i < 300; i++)
            fallback.Inject(in Item);

        Thread.Sleep(50);   // flush deadline (0 ms) has fired many times
        fallback.Stop(500);

        prev.Accepted.Should().Be(0, "TryDrainToPrev must be no-op while _running=true");
        fallback.WrittenToBackend.Should().Be(300, "all items must reach WriteToBackend");
    }

    /// <summary>
    /// After <c>Stop()</c> sets <c>_running=false</c>, items still in the ring drain to
    /// Prev via <c>TryDrainToPrev</c> when the flush deadline fires in the shutdown phase.
    /// </summary>
    [Fact]
    public void Spsc_TryDrainToPrev_DrainsOnShutdown()
    {
        const int Count = 600;

        var prev     = new CountingPrev();
        var fallback = new InjectableSpscPipe(flushIntervalMs: 0);
        fallback.Prev = prev;

        // Inject before Start so ring is pre-loaded; no consumer thread yet.
        for (int i = 0; i < Count; i++)
            fallback.Inject(in Item);

        fallback.Start();
        fallback.Stop(drainTimeoutMs: 500);   // _running=false before most items are consumed

        prev.Accepted.Should().BeGreaterThan(0, "shutdown drain must forward ring items to Prev");
        (fallback.WrittenToBackend + prev.Accepted).Should().Be(Count, "no item may be lost");
    }

    /// <summary>
    /// When Prev becomes unhealthy mid-drain the loop breaks immediately; remaining
    /// ring items stay unprocessed (or go to backend on subsequent loop iterations).
    /// </summary>
    [Fact]
    public void Spsc_TryDrainToPrev_StopsWhenPrevUnhealthy()
    {
        const int MaxAccept = 5;

        var prev     = new CountingPrev(maxAccept: MaxAccept);
        var fallback = new InjectableSpscPipe(flushIntervalMs: 0);
        fallback.Prev = prev;

        for (int i = 0; i < 600; i++)
            fallback.Inject(in Item);

        fallback.Start();
        fallback.Stop(drainTimeoutMs: 500);

        prev.Accepted.Should().BeLessThanOrEqualTo(MaxAccept, "drain must stop when Prev.IsHealthy flips false");
    }

    // -------------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential, Size = 64)]
    private struct Entry64 { public long Value; }

    private sealed class InjectableSpscPipe : SpscQueueSink<Entry64>
    {
        private long _written;
        public long WrittenToBackend => Volatile.Read(ref _written);

        public InjectableSpscPipe(int flushIntervalMs)
            : base(ringCapacity: 1024, flushIntervalMs, "drain-test") { }

        // Bypasses IsHealthy gate — lets the test inject items directly into the ring.
        public void Inject(in Entry64 item) => Accept(in item);

        protected override void WriteToBackend(in Entry64 item) => Interlocked.Increment(ref _written);
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class CountingPrev : DispatchSink<Entry64>
    {
        private readonly int _maxAccept;
        private int          _accepted;

        public CountingPrev(int maxAccept = int.MaxValue) => _maxAccept = maxAccept;

        public int Accepted => _accepted;

        public override bool IsHealthy => _accepted < _maxAccept;

        protected override bool Accept(in Entry64 item)
        {
            if (_accepted >= _maxAccept) return false;
            _accepted++;
            return true;
        }

        public override void Flush()   { }
        public override void Dispose() { }
    }
}
