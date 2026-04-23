using System;
using System.Runtime.CompilerServices;

namespace Relay.Internal;

internal static class PipeConstraints
{
    /// <summary>
    /// DEBUG-only assert: T must be 32, 64, 128, or 256 bytes to guarantee cache-line alignment.
    /// Throws <see cref="InvalidOperationException"/> in DEBUG builds when violated.
    /// No-op in Release.
    /// </summary>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void AssertCacheLineAligned<T>() where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (size != 32 && size != 64 && size != 128 && size != 256)
            throw new InvalidOperationException(
                $"T={typeof(T).Name} has size {size}B; Relay requires 32/64/128/256B for cache-line alignment.");
    }
}
