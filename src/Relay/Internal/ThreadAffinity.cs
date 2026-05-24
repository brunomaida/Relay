using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Relay.Internal;

/// <summary>
/// Best-effort CPU affinity pin. Calls OS-specific APIs to bind the calling thread
/// to a single logical processor. Fail-soft: any P/Invoke failure returns false without
/// throwing — callers must not rely on pinning succeeding.
/// </summary>
internal static class ThreadAffinity
{
    /// <summary>
    /// Attempts to pin the calling thread to logical CPU <paramref name="cpu"/>.
    /// Returns <c>true</c> on success, <c>false</c> on any failure (unsupported OS,
    /// invalid CPU index, insufficient privilege). Never throws.
    /// </summary>
    public static bool Pin(int cpu)
    {
        if (cpu < 0 || cpu >= 64) return false;

        try
        {
            if (OperatingSystem.IsWindows()) return PinWindows(cpu);
            if (OperatingSystem.IsLinux())  return PinLinux(cpu);
            return false;
        }
        catch
        {
            return false;
        }
    }

    [SupportedOSPlatform("windows")]
    private static bool PinWindows(int cpu)
    {
        nint thread = GetCurrentThread();
        nuint mask   = (nuint)(1UL << cpu);
        nuint prev   = SetThreadAffinityMask(thread, mask);
        return prev != nuint.Zero;
    }

    [SupportedOSPlatform("linux")]
    private static unsafe bool PinLinux(int cpu)
    {
        // cpu_set_t on Linux is 128 bytes; we model it as two ulongs (supports CPUs 0-127).
        // Only the low 64-bit word is needed for cpu < 64, which is enforced above.
        ulong lo = 1UL << cpu;
        ulong hi = 0UL;
        int rc = sched_setaffinity(0, 16, &lo);
        _ = hi; // suppress unused-variable warning
        return rc == 0;
    }

    // ── Windows P/Invoke ──────────────────────────────────────────────────────────

    [DllImport("kernel32", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    private static extern nint GetCurrentThread();

    [DllImport("kernel32", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    private static extern nuint SetThreadAffinityMask(nint hThread, nuint dwThreadAffinityMask);

    // ── Linux P/Invoke ────────────────────────────────────────────────────────────

    // sched_setaffinity(pid_t pid, size_t cpusetsize, cpu_set_t *mask)
    // Returns 0 on success, -1 on error.
    [DllImport("libc", SetLastError = true)]
    [SupportedOSPlatform("linux")]
    private static extern unsafe int sched_setaffinity(int pid, nuint cpusetsize, ulong* mask);
}
