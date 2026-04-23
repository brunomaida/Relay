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
            byte*  bp    = (byte*)ptr;
            nuint  total = (nuint)(array.Length * sizeof(T));
            nuint  page  = 4096;

            for (nuint i = 0; i < total; i += page)
                Volatile.Read(ref bp[i]);

            Volatile.Read(ref bp[total - 1]);

            if (OperatingSystem.IsWindows())
                TryVirtualLock(bp, total);
        }
    }

    [SupportedOSPlatform("windows")]
    private static void TryVirtualLock(byte* ptr, nuint bytes)
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualLock(void* lpAddress, nuint dwSize);

        VirtualLock(ptr, bytes); // ignore failure — best-effort
    }
}
