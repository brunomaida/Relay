using System;
using System.Runtime.CompilerServices;

namespace Relay.Internal;

internal static class SinkConstraints
{
    /// <summary>
    /// Asserts that <c>sizeof(T)</c> is a positive multiple of 64 bytes so that adjacent ring
    /// slots never share a 64B cache line. Throws <see cref="InvalidOperationException"/> when
    /// violated. Runs in both Debug and Release — this is a constructor-only cold path and the
    /// cost is nil; silent misalignment causes ~50–200 c/item of false-sharing overhead.
    /// </summary>
    /// <remarks>
    /// Inter-slot false sharing is avoided iff <c>sizeof(T) % 64 == 0</c>, independent of the
    /// absolute alignment of the backing array. Sizes 32, 96, 160, etc. allow two adjacent slots
    /// to land in one cache line and are rejected.
    /// </remarks>
    public static void AssertCacheLineAligned<T>() where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (size == 0 || size % 64 != 0)
            throw new InvalidOperationException(
                $"T={typeof(T).Name} has size {size}B; Relay requires sizeof(T) to be a positive multiple of 64B to prevent inter-slot false sharing.");
    }
}
