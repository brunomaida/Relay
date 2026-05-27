using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Sinks;
using Relay.Tests.Circular.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Relay.Tests.Circular;

/// <summary>
/// Tests for circular Ring-3 topologies where one node wraps a real backend sink.
/// Topology: N0 (SpscRingNode, decrementHops=true) → N1 (BackendRingNode) → N2 (SpscRingNode, decrementHops=true) → N0.
/// Finite tests are commit-gate; the stress test is excluded via <c>[Trait("Category", "Stress")]</c>.
/// </summary>
/// <remarks>
/// <para>
/// HopCount=5 (odd) distributes evenly across the 3-node mixed ring:
/// each node is visited (5+1)/2 = 3 times per injected item, total 9 per item.
/// BackendRingNode does not decrement HopCount — it passes items through unchanged.
/// </para>
/// <para>
/// BackendRingNode.DisposeBackend disposes the wrapped backend sink. Do not call
/// the backend's Stop() or Dispose() separately — that would double-dispose.
/// </para>
/// </remarks>
public class BackendSinkRingTests
{
    private readonly ITestOutputHelper _output;

    public BackendSinkRingTests(ITestOutputHelper output) => _output = output;

    // -------------------------------------------------------------------------
    // Quiesce helper — polls until the sum of all node counts is stable.
    // -------------------------------------------------------------------------

    private static void WaitQuiesce(Func<long> getTotal, int pollMs = 20, int timeoutMs = 10_000)
    {
        long deadline = Environment.TickCount64 + timeoutMs;
        long prev = -1;
        int stable = 0;
        while (Environment.TickCount64 < deadline)
        {
            Thread.Sleep(pollMs);
            long next = getTotal();
            if (next == prev) { if (++stable >= 2) return; }
            else              { stable = 0; }
            prev = next;
        }
    }

    // -------------------------------------------------------------------------
    // Finite tests (commit-gate)
    // -------------------------------------------------------------------------

    [Fact]
    public void Ring3_FileStreamSink_Packet64_Finite_SubSecond_BackendReceivesAndWritesToFile()
    {
        string path = Path.GetTempFileName();
        try
        {
            const int itemCount = 50;
            const int hopCount  = 5;

            var fileSink = new FileStreamSink<Packet64>(path, ringCapacity: 1024, flushInterval: 1);
            fileSink.Start();

            var n0 = new SpscRingNode<Packet64>(1024, 1, "n0", decrementHops: true);
            var n1 = new BackendRingNode<Packet64>(fileSink, 1024, 1, "n1");
            var n2 = new SpscRingNode<Packet64>(1024, 1, "n2", decrementHops: true);
            n0.RingNext = n1; n1.RingNext = n2; n2.RingNext = n0;
            n0.Start(); n1.Start(); n2.Start();

            for (long id = 0; id < itemCount; id++)
                n0.Enqueue(new Packet64 { HopCount = hopCount, Id = id });

            WaitQuiesce(() => n0.Count + n1.Count + n2.Count);

            // Stop order: n2 then n1 then n0. n1.Stop() → DisposeBackend() → fileSink.Dispose()
            // which flushes and closes the file. No separate fileSink.Stop() needed.
            n2.Stop(); n1.Stop(); n0.Stop();

            // hopCount=5 (odd): each of the 3 nodes is visited 3 times per item.
            n1.Count.Should().Be(itemCount * 3);
            new FileInfo(path).Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Ring3_MmfSink_Packet64_Finite_SubSecond_BackendReceivesItems()
    {
        string path = Path.GetTempFileName();
        try
        {
            const int itemCount = 50;
            const int hopCount  = 5;
            // hopCount=5 odd → each node visited 3 times → n1 receives 50*3=150 items.
            // 150 * sizeof(Packet64)=64 = 9600 bytes. Use 65536 for comfortable headroom.
            const long maxBytes = 65536;

            var mmfSink = new MmfSink<Packet64>(path, maxBytes, ringCapacity: 1024, flushInterval: 1);
            mmfSink.Start();

            var n0 = new SpscRingNode<Packet64>(1024, 1, "n0", decrementHops: true);
            var n1 = new BackendRingNode<Packet64>(mmfSink, 1024, 1, "n1");
            var n2 = new SpscRingNode<Packet64>(1024, 1, "n2", decrementHops: true);
            n0.RingNext = n1; n1.RingNext = n2; n2.RingNext = n0;
            n0.Start(); n1.Start(); n2.Start();

            for (long id = 0; id < itemCount; id++)
                n0.Enqueue(new Packet64 { HopCount = hopCount, Id = id });

            WaitQuiesce(() => n0.Count + n1.Count + n2.Count);

            n2.Stop(); n1.Stop(); n0.Stop();

            // hopCount=5 (odd): each of the 3 nodes is visited 3 times per item.
            n1.Count.Should().Be(itemCount * 3);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Ring3_MemorySink_Packet64_Finite_SubSecond_BackendBuffersItems()
    {
        const int itemCount = 50;
        const int hopCount  = 5;
        // MemorySink capacity must be a power of two >= items n1 will receive.
        // hopCount=5 odd → n1 receives 50*3=150 items. Capacity=1024 > 150.
        // MemorySink is a direct-write DispatchSink<T> — no consumer thread, no Start() needed.
        var mem = new MemorySink<Packet64>(capacity: 1024);

        var n0 = new SpscRingNode<Packet64>(1024, 1, "n0", decrementHops: true);
        var n1 = new BackendRingNode<Packet64>(mem, 1024, 1, "n1");
        var n2 = new SpscRingNode<Packet64>(1024, 1, "n2", decrementHops: true);
        n0.RingNext = n1; n1.RingNext = n2; n2.RingNext = n0;
        n0.Start(); n1.Start(); n2.Start();

        for (long id = 0; id < itemCount; id++)
            n0.Enqueue(new Packet64 { HopCount = hopCount, Id = id });

        WaitQuiesce(() => n0.Count + n1.Count + n2.Count);

        // n1.Stop() → DisposeBackend() → mem.Dispose() → frees native memory.
        // Do not call mem.Dispose() again after this.
        n2.Stop(); n1.Stop(); n0.Stop();

        // hopCount=5 (odd): each of the 3 nodes is visited 3 times per item.
        n1.Count.Should().Be(itemCount * 3);
    }

    // -------------------------------------------------------------------------
    // Stress test (excluded from commit gate)
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Stress")]
    public void Ring3_FileStreamSink_Packet64_Infinite_30s_Throughput()
    {
        string path = Path.GetTempFileName();
        try
        {
            const int seedItems = 256;
            const int snapshots = 30;

            var fileSink = new FileStreamSink<Packet64>(path, ringCapacity: 8192, flushInterval: 1);
            fileSink.Start();

            // Infinite ring: decrementHops=false so items circulate forever.
            // N0 is MpscRingNode because n2's consumer re-injects to n0 (multi-producer scenario).
            var n0 = new MpscRingNode<Packet64>(8192, 1, "n0", decrementHops: false);
            var n1 = new BackendRingNode<Packet64>(fileSink, 8192, 1, "n1");
            var n2 = new SpscRingNode<Packet64>(8192, 1, "n2", decrementHops: false);
            n0.RingNext = n1; n1.RingNext = n2; n2.RingNext = n0;
            n0.Start(); n1.Start(); n2.Start();

            for (int i = 0; i < seedItems; i++)
                n0.Enqueue(new Packet64 { HopCount = 0, Id = i });

            // Warmup: allow JIT and ring threads to reach steady-state before measurement.
            Thread.Sleep(5_000);

            var report = new RingTestReport(_output);
            report.Start(n0.Count + n1.Count + n2.Count);

            for (int s = 0; s < snapshots; s++)
            {
                Thread.Sleep(1_000);
                report.Record(n0.Count + n1.Count + n2.Count);
            }

            n2.Stop(2_000); n1.Stop(2_000); n0.Stop(2_000);
            report.Stop();
            report.Print("Ring3_FileStreamSink_Infinite_30s", n0.Count + n1.Count + n2.Count);

            n1.Count.Should().BeGreaterThan(0);
            new FileInfo(path).Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
