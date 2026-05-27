using System;
using System.Threading;
using FluentAssertions;
using Relay.Tests.Circular.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Relay.Tests.Circular;

/// <summary>
/// Tests for circular rings of pure sink nodes. No concrete backend sinks — only
/// SpscRingNode / MpscRingNode topologies. Finite tests are commit-gate; stress tests
/// are excluded via <c>[Trait("Category", "Stress")]</c>.
/// </summary>
/// <remarks>
/// <para>
/// Finite tests use a quiesce loop before <c>Stop()</c> because
/// <see cref="SpscRingTopology{T}.Stop"/> halts nodes sequentially (0→N), and the last
/// node forwards back to node[0] whose consumer has already exited. Without the quiesce
/// wait, each item is processed by exactly one full circuit (nodeCount visits) regardless
/// of hopCount. The loop polls <c>TotalCount()</c> until it stabilises for two consecutive
/// 20 ms windows — at that point all hops have completed and the ring is idle.
/// </para>
/// </remarks>
public class PureSinkRingTests
{
    private readonly ITestOutputHelper _output;

    public PureSinkRingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // -------------------------------------------------------------------------
    // Quiesce helper — blocks until TotalCount stops increasing.
    // Max wait ≈ timeoutMs. Returns final stable count.
    // -------------------------------------------------------------------------

    // Two consecutive stable polls required to avoid declaring quiescence while the consumer
    // thread is momentarily idle (Thread.Sleep(1) idle branch) between processing batches.
    private static long WaitQuiesce<T>(SpscRingTopology<T> ring, int pollMs = 20, int timeoutMs = 10_000)
        where T : unmanaged
    {
        long deadline  = Environment.TickCount64 + timeoutMs;
        long prev      = -1;
        int  stable    = 0;
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

    private static long WaitQuiescePacket(PacketRingTopology ring, int pollMs = 20, int timeoutMs = 10_000)
    {
        long deadline  = Environment.TickCount64 + timeoutMs;
        long prev      = -1;
        int  stable    = 0;
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
    // Finite tests (commit-gate)
    // -------------------------------------------------------------------------

    [Fact]
    public void Ring3_Packet64_Finite_SubSecond_TotalCountMatchesHops()
    {
        const int itemCount = 1_000;
        const int hopCount  = 10;

        using var ring = new SpscRingTopology<Packet64>(
            new RingNodeConfig(NodeCount: 3, RingCapacity: 1024, DecrementHops: true));
        ring.Start();

        for (long id = 0; id < itemCount; id++)
            ring.Entry.Enqueue(new Packet64 { HopCount = hopCount, Id = id });

        WaitQuiesce(ring);
        ring.Stop(drainMs: 5_000);

        ring.TotalCount().Should().Be(itemCount * (hopCount + 1));
    }

    [Fact]
    public void Ring5_Packet128_Finite_SubSecond_TotalCountMatchesHops()
    {
        const int itemCount = 1_000;
        const int hopCount  = 10;

        using var ring = new SpscRingTopology<Packet128>(
            new RingNodeConfig(NodeCount: 5, RingCapacity: 1024, DecrementHops: true));
        ring.Start();

        for (long id = 0; id < itemCount; id++)
            ring.Entry.Enqueue(new Packet128 { HopCount = hopCount, Id = id });

        WaitQuiesce(ring);
        ring.Stop(drainMs: 5_000);

        ring.TotalCount().Should().Be(itemCount * (hopCount + 1));
    }

    [Fact]
    public void Ring8_Packet256_Finite_SubSecond_TotalCountMatchesHops()
    {
        const int itemCount = 500;
        const int hopCount  = 10;

        using var ring = new SpscRingTopology<Packet256>(
            new RingNodeConfig(NodeCount: 8, RingCapacity: 1024, DecrementHops: true));
        ring.Start();

        for (long id = 0; id < itemCount; id++)
            ring.Entry.Enqueue(new Packet256 { HopCount = hopCount, Id = id });

        WaitQuiesce(ring);
        ring.Stop(drainMs: 5_000);

        ring.TotalCount().Should().Be(itemCount * (hopCount + 1));
    }

    [Fact]
    public void Ring13_Packet320_Finite_SubSecond_TotalCountMatchesHops()
    {
        const int itemCount = 200;
        const int hopCount  = 10;

        using var ring = new SpscRingTopology<Packet320>(
            new RingNodeConfig(NodeCount: 13, RingCapacity: 1024, DecrementHops: true));
        ring.Start();

        for (long id = 0; id < itemCount; id++)
            ring.Entry.Enqueue(new Packet320 { HopCount = hopCount, Id = id });

        WaitQuiesce(ring);
        ring.Stop(drainMs: 5_000);

        ring.TotalCount().Should().Be(itemCount * (hopCount + 1));
    }

    // -------------------------------------------------------------------------
    // Stress tests (excluded from commit gate)
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Stress")]
    public void Ring3_Packet64_Infinite_30s_Throughput()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet64>(
            new RingNodeConfig(NodeCount: 3, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet64 { HopCount = 0, Id = i });

        // Warmup: allow JIT and ring threads to reach steady-state before measurement.
        Thread.Sleep(5_000);

        var report = new RingTestReport(_output);
        report.Start();

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring3_Packet64_Infinite_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Stress")]
    public void Ring5_Packet128_Infinite_30s_Throughput()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet128>(
            new RingNodeConfig(NodeCount: 5, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet128 { HopCount = 0, Id = i });

        // Warmup: allow JIT and ring threads to reach steady-state before measurement.
        Thread.Sleep(5_000);

        var report = new RingTestReport(_output);
        report.Start();

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring5_Packet128_Infinite_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Stress")]
    public void Ring8_Packet256_Infinite_30s_Throughput()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet256>(
            new RingNodeConfig(NodeCount: 8, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet256 { HopCount = 0, Id = i });

        // Warmup: allow JIT and ring threads to reach steady-state before measurement.
        Thread.Sleep(5_000);

        var report = new RingTestReport(_output);
        report.Start();

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring8_Packet256_Infinite_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Stress")]
    public void Ring13_Packet320_Infinite_30s_Throughput()
    {
        const int seedItems = 512;
        const int snapshots = 30;

        using var ring = new InfiniteRingTopology<Packet320>(
            new RingNodeConfig(NodeCount: 13, RingCapacity: 8192, DecrementHops: false));
        ring.Start();

        for (int i = 0; i < seedItems; i++)
            ring.Entry.Enqueue(new Packet320 { HopCount = 0, Id = i });

        // Warmup: allow JIT and ring threads to reach steady-state before measurement.
        Thread.Sleep(5_000);

        var report = new RingTestReport(_output);
        report.Start();

        for (int s = 0; s < snapshots; s++)
        {
            Thread.Sleep(1_000);
            report.Record(ring.TotalCount());
        }

        ring.Stop(drainMs: 2_000);
        report.Stop();
        report.Print($"Ring13_Packet320_Infinite_{snapshots}s", ring.TotalCount());

        ring.TotalCount().Should().BeGreaterThan(0);
    }

    // -------------------------------------------------------------------------
    // Packet ring tests (finite, commit-gate)
    // -------------------------------------------------------------------------

    [Fact]
    public void Ring3_Packet_64B_Finite_SubSecond_TotalCountMatchesHops()
    {
        const int itemCount = 1_000;
        const int hopCount  = 5;

        // RingCapacity=131072: each 64-byte payload record = 68 bytes (4B header + 64B payload),
        // so capacity = 131072 / 68 = 1927 entries — comfortably above itemCount=1000.
        // 65536 / 68 = 963 < 1000 → ring overflow at peak load with the smaller capacity.
        using var ring = new PacketRingTopology(
            new PacketRingConfig(NodeCount: 3, RingCapacity: 131072, MaxPayloadBytes: 64));
        ring.Start();

        Span<byte> buf = stackalloc byte[64];
        for (long id = 0; id < itemCount; id++)
        {
            buf.Clear();
            PacketLayout.WriteHop(buf, hopCount);
            PacketLayout.WriteId(buf, id);
            ring.Entry.Enqueue(buf);
        }

        WaitQuiescePacket(ring);
        ring.Stop(drainMs: 5_000);

        ring.TotalCount().Should().Be(itemCount * (hopCount + 1));
    }

    [Fact]
    public void Ring5_Packet_64B_Finite_SubSecond_TotalCountMatchesHops()
    {
        const int itemCount = 1_000;
        const int hopCount  = 5;

        // RingCapacity=131072: see Ring3 comment above for capacity calculation.
        using var ring = new PacketRingTopology(
            new PacketRingConfig(NodeCount: 5, RingCapacity: 131072, MaxPayloadBytes: 64));
        ring.Start();

        Span<byte> buf = stackalloc byte[64];
        for (long id = 0; id < itemCount; id++)
        {
            buf.Clear();
            PacketLayout.WriteHop(buf, hopCount);
            PacketLayout.WriteId(buf, id);
            ring.Entry.Enqueue(buf);
        }

        WaitQuiescePacket(ring);
        ring.Stop(drainMs: 5_000);

        ring.TotalCount().Should().Be(itemCount * (hopCount + 1));
    }
}
