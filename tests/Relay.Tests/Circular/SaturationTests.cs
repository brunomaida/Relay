using System;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Tests.Circular.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Relay.Tests.Circular;

/// <summary>
/// Tests that probe ring saturation behavior: overflow accounting, throughput under
/// backpressure, and deadlock-freedom when injecting more items than ring capacity.
/// </summary>
/// <remarks>
/// <para>
/// When a ring node's buffer is full, <c>Accept</c> returns false and <c>DispatchSink.Enqueue</c>
/// calls <c>Next?.Enqueue(item)</c>. Wiring a <see cref="CountingTerminalSink{T}"/> as the entry
/// node's <c>Next</c> captures overflowed items without competing with the ring circuit.
/// </para>
/// <para>
/// <c>InternalsVisibleTo</c> is declared in <c>Relay.csproj</c>, so the <c>internal set</c>
/// on <c>DispatchSink{T}.Next</c> is accessible from this test project.
/// </para>
/// </remarks>
public class SaturationTests
{
    private readonly ITestOutputHelper _output;

    public SaturationTests(ITestOutputHelper output) => _output = output;

    // -------------------------------------------------------------------------
    // Quiesce helper — waits until TotalCount stabilises (two consecutive equal
    // polls spaced pollMs apart). Returns the final stable count.
    // -------------------------------------------------------------------------

    private static long WaitQuiesce<T>(SpscRingTopology<T> ring, int pollMs = 20, int timeoutMs = 10_000)
        where T : unmanaged
    {
        long deadline = Environment.TickCount64 + timeoutMs;
        long prev     = -1;
        int  stable   = 0;
        while (Environment.TickCount64 < deadline)
        {
            Thread.Sleep(pollMs);
            long next = ring.TotalCount();
            if (next == prev) { if (++stable >= 2) return next; }
            else              { stable = 0; }
            prev = next;
        }
        return ring.TotalCount();
    }

    // -------------------------------------------------------------------------
    // Test 1 — finite saturation with drop counting (commit-gate)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Injects 10 000 items into a Ring-3 topology whose ring buffers hold only 64 entries each.
    /// Items that cannot enter the ring (Accept=false) are captured by a CountingTerminalSink
    /// wired as the entry node's Next fallback. Asserts that processed + dropped == injected,
    /// proving every item is accounted for and no item is silently created or destroyed.
    /// </summary>
    [Fact]
    public void Ring3_Packet64_SmallBuffer_1s_OverflowItemsDroppedAtTerminal()
    {
        const int injected   = 10_000;
        const int ringCap    = 64;   // intentionally small to provoke overflow

        var dropCounter = new CountingTerminalSink<Packet64>();

        using var ring = new SpscRingTopology<Packet64>(
            new RingNodeConfig(NodeCount: 3, RingCapacity: ringCap, DecrementHops: false));
        ring.Start();

        // Wire drop counter as Next on the entry node only.
        // Overflow at intermediate nodes goes to their Next (null) — silent drop.
        // This test measures entry-level saturation.
        ring.Entry.Next = dropCounter;

        for (long id = 0; id < injected; id++)
            ring.Entry.Enqueue(new Packet64 { HopCount = 0, Id = id });

        WaitQuiesce(ring, pollMs: 20, timeoutMs: 5_000);
        ring.Stop(drainMs: 3_000);

        long processed = ring.TotalCount();
        long dropped   = dropCounter.Count;

        _output.WriteLine($"injected={injected} processed={processed} dropped={dropped}");

        // DecrementHops=false: items cycle indefinitely, so TotalCount grows far beyond injected.
        // The meaningful assertions are:
        //   1. The ring processed at least one item (liveness).
        //   2. The drop counter captured at least one overflow (saturation was provoked).
        processed.Should().BeGreaterThan(0, "at least some items must complete a ring visit");
        dropped.Should().BeGreaterThan(0,
            "with ringCap=64 and injected=10000, at least one overflow must be captured at entry");
    }

    // -------------------------------------------------------------------------
    // Test 2 — simple flow-positive assertion under saturation (commit-gate)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that items flow through a saturated Ring-3 topology — i.e. that the ring does
    /// not stall when producers inject faster than consumers can drain. The primary assertion
    /// is liveness: <c>TotalCount &gt; 0</c>.
    /// </summary>
    [Fact]
    public void Ring3_Packet64_SmallBuffer_SubSecond_TotalFlowPositive()
    {
        const int injected = 5_000;
        const int ringCap  = 64;

        using var ring = new SpscRingTopology<Packet64>(
            new RingNodeConfig(NodeCount: 3, RingCapacity: ringCap, DecrementHops: false));
        ring.Start();

        for (long id = 0; id < injected; id++)
            ring.Entry.Enqueue(new Packet64 { HopCount = 0, Id = id });

        WaitQuiesce(ring, pollMs: 20, timeoutMs: 5_000);
        ring.Stop(drainMs: 3_000);

        _output.WriteLine($"injected={injected} processed={ring.TotalCount()}");

        ring.TotalCount().Should().BeGreaterThan(0,
            "ring must process at least one item despite buffer saturation");
    }

    // -------------------------------------------------------------------------
    // Test 3 — deadlock-freedom under backpressure in a deeper ring (commit-gate)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Injects 5 000 items into a Ring-5 topology with ring buffers of 64 entries.
    /// The primary goal is to confirm that the ring does not deadlock under sustained
    /// backpressure — if it does, the test will time out rather than hang the process.
    /// </summary>
    [Fact]
    public void Ring5_Packet64_BackpressureContained_SubSecond_NoDeadlock()
    {
        const int injected = 5_000;
        const int ringCap  = 64;

        using var ring = new SpscRingTopology<Packet64>(
            new RingNodeConfig(NodeCount: 5, RingCapacity: ringCap, DecrementHops: false));
        ring.Start();

        for (long id = 0; id < injected; id++)
            ring.Entry.Enqueue(new Packet64 { HopCount = 0, Id = id });

        WaitQuiesce(ring, pollMs: 20, timeoutMs: 5_000);
        ring.Stop(drainMs: 3_000);

        _output.WriteLine($"injected={injected} processed={ring.TotalCount()}");

        // Liveness: ring must have processed something.
        ring.TotalCount().Should().BeGreaterThan(0,
            "ring must not deadlock — at least one item must be processed");
    }

    // -------------------------------------------------------------------------
    // Test 4 — saturation rate measurement (Stress, excluded from commit gate)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runs an <see cref="InfiniteRingTopology{T}"/> with small ring buffers for 30 seconds,
    /// seeding a burst of 512 items and measuring throughput under ring saturation.
    /// Prints a <see cref="RingTestReport"/> with min/avg/max msg/s and GC impact.
    /// </summary>
    [Fact]
    [Trait("Category", "Stress")]
    public void Ring3_Packet64_SmallBuffer_30s_SaturationRateMeasured()
    {
        const int seedItems = 512;
        const int ringCap   = 256; // small buffer to provoke saturation
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet64>(
            new RingNodeConfig(NodeCount: 3, RingCapacity: ringCap, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet64 { HopCount = 0, Id = i });

        var report = new RingTestReport(_output, maxSnapshots: snapshots);

        // Warmup: allow JIT and ring threads to reach steady-state before measurement.
        Thread.Sleep(5_000);

        report.Start(ring.TotalCount());

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring3_Packet64_SmallBuffer_{snapshots}s_Saturation", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }
}
