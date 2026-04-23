using System;
using System.Runtime.CompilerServices;

namespace Relay.Internal;

internal static class PipeConstraints
{
    /// <summary>
    /// DEBUG-only assert: <c>sizeof(T)</c> must be a positive multiple of 64 bytes so that
    /// adjacent ring slots never share a 64B cache line. Throws
    /// <see cref="InvalidOperationException"/> in DEBUG builds when violated. No-op in Release.
    /// </summary>
    /// <remarks>
    /// Inter-slot false sharing is avoided iff <c>sizeof(T) % 64 == 0</c>, independent of the
    /// absolute alignment of the backing array. Sizes 32, 96, 160, etc. allow two adjacent slots
    /// to land in one cache line and are rejected.
    /// </remarks>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void AssertCacheLineAligned<T>() where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (size == 0 || size % 64 != 0)
            throw new InvalidOperationException(
                $"T={typeof(T).Name} has size {size}B; Relay requires sizeof(T) to be a positive multiple of 64B to prevent inter-slot false sharing.");
    }
}
