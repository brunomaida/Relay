using System;
using System.Runtime.Versioning;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Receivers;
using Relay.Sinks;
using Relay.Tests.Circular.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Relay.Tests.Circular;

/// <summary>
/// Tests for Ring-3 topologies where N1 wraps <see cref="SharedMemorySpscSink"/> and a
/// <see cref="SharedMemorySpscReceiver{TState}"/> consumes frames from a polling thread.
/// </summary>
/// <remarks>
/// <para>
/// Topology: N0 (PacketRingNode, decrement) → N1 (BackendPacketRingNode wrapping SharedMemorySpscSink)
/// → N2 (PacketRingNode, decrement) → N0.
/// </para>
/// <para>
/// HopCount=5 (odd): N0 and N2 decrement; N1 passes through unchanged.
/// Each item visits N1 exactly (5+1)/2 = 3 times, so the receiver gets 3 frames per injected item.
/// </para>
/// <para>
/// MMF lifecycle: BackendPacketRingNode.DisposeBackend calls <c>shm.Dispose()</c>.
/// Do NOT use <c>using</c> on the sink — BackendPacketRingNode owns its lifetime.
/// The receiver holds its own independent MMF view handle; it continues to work after the sink is disposed.
/// </para>
/// <para>
/// Windows only: Named MMFs require Windows. Each test returns early on non-Windows so the commit gate
/// passes on Linux without running the MMF code. The <c>[SupportedOSPlatform]</c> attribute suppresses
/// the analyzer warning.
/// </para>
/// </remarks>
public class ReceiverSinkRingTests
{
    private readonly ITestOutputHelper _output;

    public ReceiverSinkRingTests(ITestOutputHelper output) => _output = output;

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
    // Finite test (commit-gate)
    // -------------------------------------------------------------------------

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Ring3_SharedMemorySpscSink_Packet_Finite_2s_ReceiverCountsFrames()
    {
        if (!OperatingSystem.IsWindows()) return; // Named MMF: Windows only

        const string mmfName   = "Local\\relay-ring-rcv-finite";
        const int    itemCount = 50;
        const int    hopCount  = 5; // odd → N1 visited (hopCount+1)/2 = 3 times per item

        // counter[0] accumulates frames received by the polling thread.
        var counter = new long[1];

        // shm: no 'using' — BackendPacketRingNode.DisposeBackend owns disposal via n1.Stop().
        var shm = new SharedMemorySpscSink(mmfName);

        // receiver: holds its own MMF view handle independent of shm's handle.
        using var receiver = new SharedMemorySpscReceiver<long[]>(
            mmfName,
            state: counter,
            callback: static (ctr, _) => Interlocked.Increment(ref ctr[0]));

        // Ring-3 packet topology.
        var n0 = new PacketRingNode(65536, 1, "n0");
        var n1 = new BackendPacketRingNode(shm, 65536, 1, "n1");
        var n2 = new PacketRingNode(65536, 1, "n2");
        n0.RingNext = n1; n1.RingNext = n2; n2.RingNext = n0;
        n0.Start(); n1.Start(); n2.Start();

        // Polling thread — non-blocking Poll() loop.
        int pollerRun = 1;
        var pollerThread = new Thread(() =>
        {
            while (Volatile.Read(ref pollerRun) == 1)
            {
                if (!receiver.Poll()) Thread.SpinWait(20);
            }
            // Drain any frames that arrived between ring stop and poller stop.
            while (receiver.Poll()) { }
        }) { IsBackground = true };
        pollerThread.Start();

        // Inject packets.
        Span<byte> buf = stackalloc byte[64];
        for (int id = 0; id < itemCount; id++)
        {
            buf.Clear();
            PacketLayout.WriteHop(buf, hopCount);
            PacketLayout.WriteId(buf, id);
            n0.Enqueue(buf);
        }

        // Wait for all hops to complete (ring quiesces when counts stop rising).
        WaitQuiesce(() => n0.Count + n1.Count + n2.Count);

        long n1Count = n1.Count; // capture before Stop disposes shm

        // Stop ring: n1.Stop() → DisposeBackend() → shm.Dispose().
        // The receiver's own MMF handle keeps the mapping alive until receiver.Dispose().
        n2.Stop(); n1.Stop(); n0.Stop();

        // Allow poller to drain any remaining frames, then signal shutdown.
        Thread.Sleep(200);
        Volatile.Write(ref pollerRun, 0);
        pollerThread.Join(1_000);

        long totalReceived = Volatile.Read(ref counter[0]);
        _output.WriteLine($"n1.Count={n1Count} received={totalReceived}");

        totalReceived.Should().BeGreaterThan(0, "receiver must get at least one frame");
        totalReceived.Should().Be(n1Count, "receiver must see every frame N1 wrote to shared memory");
    }

    // -------------------------------------------------------------------------
    // Stress test (excluded from commit gate)
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Stress")]
    [SupportedOSPlatform("windows")]
    public void Ring3_SharedMemorySpscSink_Packet_Infinite_30s_Throughput()
    {
        if (!OperatingSystem.IsWindows()) return; // Named MMF: Windows only

        const string mmfName   = "Local\\relay-ring-rcv-stress";
        const int    seedItems = 128;
        const int    snapshots = 30;

        var counter = new long[1];

        // shm: no 'using' — BackendPacketRingNode owns disposal.
        var shm = new SharedMemorySpscSink(mmfName, totalCapacity: 4 * 1024 * 1024);

        using var receiver = new SharedMemorySpscReceiver<long[]>(
            mmfName,
            state: counter,
            callback: static (ctr, _) => Interlocked.Increment(ref ctr[0]));

        // Infinite ring: N0=PacketMpscRingNode (re-injection from N2's consumer thread).
        var n0 = new PacketMpscRingNode(65536, 1, "n0");
        var n1 = new BackendPacketRingNode(shm, 65536, 1, "n1");
        var n2 = new PacketRingNode(65536, 1, "n2");
        n0.RingNext = n1; n1.RingNext = n2; n2.RingNext = n0;
        n0.Start(); n1.Start(); n2.Start();

        // Polling thread.
        int pollerRun = 1;
        var pollerThread = new Thread(() =>
        {
            while (Volatile.Read(ref pollerRun) == 1)
            {
                if (!receiver.Poll()) Thread.SpinWait(20);
            }
            while (receiver.Poll()) { }
        }) { IsBackground = true };
        pollerThread.Start();

        // Seed with a large hopCount so items circulate for the full test window.
        Span<byte> buf = stackalloc byte[64];
        for (int i = 0; i < seedItems; i++)
        {
            buf.Clear();
            PacketLayout.WriteHop(buf, 10_000_000L); // far more hops than possible in 5 s
            PacketLayout.WriteId(buf, i);
            n0.Enqueue(buf);
        }

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
        report.Print("Ring3_SharedMemorySpscSink_Infinite_30s", n0.Count + n1.Count + n2.Count);

        // Give poller time to drain remaining frames from the shared ring.
        Thread.Sleep(200);
        Volatile.Write(ref pollerRun, 0);
        pollerThread.Join(1_000);

        long totalReceived = Volatile.Read(ref counter[0]);
        _output.WriteLine($"n1.Count={n1.Count} received={totalReceived}");

        totalReceived.Should().BeGreaterThan(0, "receiver must consume frames during the 30s window");
    }
}
