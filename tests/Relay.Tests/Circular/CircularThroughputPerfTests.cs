using System.Threading;
using FluentAssertions;
using Relay.Tests.Circular.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Relay.Tests.Circular;

/// <summary>
/// Sustained steady-state throughput benchmarks for circular ring topologies.
/// Protocol: 10 s warmup + 30 s measurement (1 s intervals) = 40 s per test.
/// All tests are <c>[Trait("Category", "Perf")]</c> and excluded from the commit gate.
/// </summary>
/// <remarks>
/// <para>
/// Uses <see cref="InfiniteRingTopology{T}"/> (MPSC entry node) so items circulate
/// indefinitely without hop-count decrement.
/// </para>
/// <para>
/// The 10 s warmup ensures JIT compilation and OS thread scheduling have settled
/// before <see cref="RingTestReport.Start"/> captures the baseline delta counter.
/// </para>
/// </remarks>
public class CircularThroughputPerfTests
{
    private readonly ITestOutputHelper _output;

    public CircularThroughputPerfTests(ITestOutputHelper output) => _output = output;

    // -------------------------------------------------------------------------
    // NodeCount scaling — payload fixed at Packet64, RingCapacity=8192, seedItems=512
    // -------------------------------------------------------------------------

    /// <summary>
    /// Measures sustained throughput of a 3-node infinite ring with Packet64 payload
    /// over a 30-second window following a 10-second JIT-warmup phase.
    /// </summary>
    [Fact]
    [Trait("Category", "Perf")]
    public void Ring3_Packet64_Perf_30s_ThroughputSteadyState()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet64>(
            new RingNodeConfig(NodeCount: 3, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet64 { HopCount = 0, Id = i });

        // Warmup: allow JIT compilation and ring threads to reach steady-state.
        Thread.Sleep(10_000);

        var report = new RingTestReport(_output, maxSnapshots: snapshots);
        report.Start(ring.TotalCount());

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring3_Packet64_Perf_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Measures sustained throughput of a 5-node infinite ring with Packet64 payload
    /// over a 30-second window following a 10-second JIT-warmup phase.
    /// </summary>
    [Fact]
    [Trait("Category", "Perf")]
    public void Ring5_Packet64_Perf_30s_ThroughputSteadyState()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet64>(
            new RingNodeConfig(NodeCount: 5, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet64 { HopCount = 0, Id = i });

        // Warmup: allow JIT compilation and ring threads to reach steady-state.
        Thread.Sleep(10_000);

        var report = new RingTestReport(_output, maxSnapshots: snapshots);
        report.Start(ring.TotalCount());

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring5_Packet64_Perf_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Measures sustained throughput of an 8-node infinite ring with Packet64 payload
    /// over a 30-second window following a 10-second JIT-warmup phase.
    /// </summary>
    [Fact]
    [Trait("Category", "Perf")]
    public void Ring8_Packet64_Perf_30s_ThroughputSteadyState()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet64>(
            new RingNodeConfig(NodeCount: 8, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet64 { HopCount = 0, Id = i });

        // Warmup: allow JIT compilation and ring threads to reach steady-state.
        Thread.Sleep(10_000);

        var report = new RingTestReport(_output, maxSnapshots: snapshots);
        report.Start(ring.TotalCount());

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring8_Packet64_Perf_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Measures sustained throughput of a 13-node infinite ring with Packet64 payload
    /// over a 30-second window following a 10-second JIT-warmup phase.
    /// </summary>
    [Fact]
    [Trait("Category", "Perf")]
    public void Ring13_Packet64_Perf_30s_ThroughputSteadyState()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet64>(
            new RingNodeConfig(NodeCount: 13, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet64 { HopCount = 0, Id = i });

        // Warmup: allow JIT compilation and ring threads to reach steady-state.
        Thread.Sleep(10_000);

        var report = new RingTestReport(_output, maxSnapshots: snapshots);
        report.Start(ring.TotalCount());

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring13_Packet64_Perf_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }

    // -------------------------------------------------------------------------
    // PayloadSize scaling — NodeCount fixed at 3, RingCapacity=8192, seedItems=512
    // -------------------------------------------------------------------------

    /// <summary>
    /// Measures sustained throughput of a 3-node infinite ring with Packet128 payload
    /// over a 30-second window following a 10-second JIT-warmup phase.
    /// </summary>
    [Fact]
    [Trait("Category", "Perf")]
    public void Ring3_Packet128_Perf_30s_ThroughputSteadyState()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet128>(
            new RingNodeConfig(NodeCount: 3, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet128 { HopCount = 0, Id = i });

        // Warmup: allow JIT compilation and ring threads to reach steady-state.
        Thread.Sleep(10_000);

        var report = new RingTestReport(_output, maxSnapshots: snapshots);
        report.Start(ring.TotalCount());

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring3_Packet128_Perf_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Measures sustained throughput of a 3-node infinite ring with Packet256 payload
    /// over a 30-second window following a 10-second JIT-warmup phase.
    /// </summary>
    [Fact]
    [Trait("Category", "Perf")]
    public void Ring3_Packet256_Perf_30s_ThroughputSteadyState()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet256>(
            new RingNodeConfig(NodeCount: 3, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet256 { HopCount = 0, Id = i });

        // Warmup: allow JIT compilation and ring threads to reach steady-state.
        Thread.Sleep(10_000);

        var report = new RingTestReport(_output, maxSnapshots: snapshots);
        report.Start(ring.TotalCount());

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring3_Packet256_Perf_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Measures sustained throughput of a 3-node infinite ring with Packet320 payload
    /// over a 30-second window following a 10-second JIT-warmup phase.
    /// </summary>
    [Fact]
    [Trait("Category", "Perf")]
    public void Ring3_Packet320_Perf_30s_ThroughputSteadyState()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet320>(
            new RingNodeConfig(NodeCount: 3, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet320 { HopCount = 0, Id = i });

        // Warmup: allow JIT compilation and ring threads to reach steady-state.
        Thread.Sleep(10_000);

        var report = new RingTestReport(_output, maxSnapshots: snapshots);
        report.Start(ring.TotalCount());

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring3_Packet320_Perf_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }
}
