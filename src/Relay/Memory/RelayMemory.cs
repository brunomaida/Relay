using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Relay.Memory;

/// <summary>Pre-faults and VirtualLocks managed arrays to prevent page faults at runtime.</summary>
internal static unsafe class RelayMemory
{
    /// <summary>
    /// Touches every 4KB page of <paramref name="array"/> to bring it into RAM, then attempts
    /// VirtualLock on Windows (best-effort; requires SeLockMemoryPrivilege).
    /// Call once from <c>SpscQueuePipe.Start()</c>.
    /// </summary>
    public static void PreFaultAndLock<T>(T[] array) where T : unmanaged
    {
        if (array.Length == 0) return;

        fixed (T* ptr = array)
        {
            PreFaultAndLock((byte*)ptr, (nuint)(array.Length * sizeof(T)));
        }
    }

    /// <summary>
    /// Pointer-based overload for native-memory ring buffers (aligned allocations).
    /// Touches every 4KB page then attempts VirtualLock on Windows.
    /// </summary>
    public static void PreFaultAndLock(byte* ptr, nuint bytes)
    {
        if (bytes == 0) return;

        const nuint page = 4096;
        for (nuint i = 0; i < bytes; i += page)
            Volatile.Read(ref ptr[i]);

        Volatile.Read(ref ptr[bytes - 1]);

        if (OperatingSystem.IsWindows())
            TryVirtualLock(ptr, bytes);
    }

    [SupportedOSPlatform("windows")]
    private static void TryVirtualLock(byte* ptr, nuint bytes)
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualLock(void* lpAddress, nuint dwSize);

        VirtualLock(ptr, bytes); // ignore failure — best-effort
    }
}
