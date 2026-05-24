using System;
using System.Runtime.InteropServices;
using System.Threading;
using FluentAssertions;
using Relay.Internal;
using Xunit;

namespace Relay.Tests.Internal;

/// <summary>ThreadAffinity P/Invoke wrapper: pin success, invalid-cpu fail-soft, and CPU-stay verification.</summary>
public sealed class ThreadAffinityTests
{
    /// <summary>
    /// Pin to CPU 0, then verify all iterations of work run on CPU 0.
    /// Skipped on unsupported platforms.
    /// </summary>
    [Fact]
    public void Pin_ToValidCpu_ReturnsTrue_AndSubsequentWorkRunsOnSameCpu()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
        {
            // Platform not supported — best-effort only.
            return;
        }

        bool pinResult = false;
        int[] cpusObserved = new int[100];

        var t = new Thread(() =>
        {
            pinResult = ThreadAffinity.Pin(0);
            if (!pinResult) return;

            for (int i = 0; i < 100; i++)
            {
                cpusObserved[i] = GetCurrentProcessorNumber();
                Thread.SpinWait(100);
            }
        });
        t.Start();
        t.Join(TimeSpan.FromSeconds(5));

        if (!pinResult)
        {
            // Pin failed (e.g. permission denied in CI). Skip rather than fail.
            return;
        }

        // Every iteration must land on CPU 0 — no migration should occur after pinning.
        foreach (int cpu in cpusObserved)
            cpu.Should().Be(0, "pinned thread must not migrate away from CPU 0");
    }

    [Fact]
    public void Pin_ToInvalidCpu_ReturnsFalse_NoThrow()
    {
        // CPU -1 is rejected at the guard before any P/Invoke.
        ThreadAffinity.Pin(-1).Should().BeFalse();

        // CPU 64 is outside the supported range (we model affinity as a 64-bit mask).
        ThreadAffinity.Pin(64).Should().BeFalse();
    }

    // ── Platform helpers ──────────────────────────────────────────────────────────

    private static int GetCurrentProcessorNumber()
    {
        if (OperatingSystem.IsWindows()) return GetCurrentProcessorNumberWindows();
        if (OperatingSystem.IsLinux())  return sched_getcpu();
        return 0;
    }

    [DllImport("kernel32", EntryPoint = "GetCurrentProcessorNumber")]
    private static extern int GetCurrentProcessorNumberWindows();

    // glibc sched_getcpu() wrapper. Returns the logical CPU the calling thread is running on.
    [DllImport("libc", EntryPoint = "sched_getcpu")]
    private static extern int sched_getcpu();
}
