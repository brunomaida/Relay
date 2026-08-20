using System;
using System.Runtime.InteropServices;

namespace Relay.Memory;

/// <summary>
/// Single unambiguous entry point for native allocation in Relay. A sibling project (Wave) shipped
/// a production memory leak by calling the <c>(elementCount, elementSize)</c> overload of
/// <see cref="NativeMemory.AllocZeroed(nuint, nuint)"/> with <c>(bufferBytes, alignment)</c> arguments —
/// this class collapses every call site to a single-argument shape where that swap cannot compile.
/// </summary>
/// <remarks>
/// Deliberately omits relational validation between <c>byteCount</c> and <c>alignment</c> (e.g.
/// <c>byteCount &gt;= alignment</c> or <c>byteCount % alignment == 0</c>) — <c>SpscRingBuffer&lt;Payload1&gt;(capacity: 4)</c>
/// legitimately allocates <c>byteCount=4</c> with <c>alignment=64</c>, which is smaller than the alignment.
/// </remarks>
internal static unsafe class NativeBuffer
{
    /// <summary>Bytes currently outstanding via <see cref="AllocZeroedAligned"/> on this thread.</summary>
    [ThreadStatic] internal static long AlignedBytesOutstanding;

    /// <summary>Bytes currently outstanding via <see cref="AllocZeroed"/> on this thread.</summary>
    [ThreadStatic] internal static long UnalignedBytesOutstanding;

    /// <summary>Allocates and zeroes <paramref name="byteCount"/> bytes aligned to <paramref name="alignment"/>. Free only via <see cref="FreeAligned"/>.</summary>
    internal static void* AllocZeroedAligned(nuint byteCount, int alignment = 64)
    {
        if (byteCount == 0)
            throw new ArgumentOutOfRangeException(nameof(byteCount), "Byte count must be positive.");
        if (alignment <= 0 || (alignment & (alignment - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(alignment), "Alignment must be a positive power of two.");
        if (alignment > 4096)
            throw new ArgumentOutOfRangeException(nameof(alignment), "Alignment must be <= 4096.");

        void* ptr = NativeMemory.AlignedAlloc(byteCount, (nuint)alignment);
        NativeMemory.Clear(ptr, byteCount);
        AlignedBytesOutstanding += (long)byteCount;
        return ptr;
    }

    /// <summary>Frees memory allocated by <see cref="AllocZeroedAligned"/>. Never call on unaligned allocations.</summary>
    internal static void FreeAligned(void* ptr, nuint byteCount)
    {
        NativeMemory.AlignedFree(ptr);
        AlignedBytesOutstanding -= (long)byteCount;
    }

    /// <summary>Allocates and zeroes <paramref name="byteCount"/> bytes, unaligned. Free only via <see cref="Free"/>.</summary>
    internal static void* AllocZeroed(nuint byteCount)
    {
        if (byteCount == 0)
            throw new ArgumentOutOfRangeException(nameof(byteCount), "Byte count must be positive.");

        void* ptr = NativeMemory.AllocZeroed(byteCount);
        UnalignedBytesOutstanding += (long)byteCount;
        return ptr;
    }

    /// <summary>Frees memory allocated by <see cref="AllocZeroed"/>. Never call on aligned allocations.</summary>
    internal static void Free(void* ptr, nuint byteCount)
    {
        NativeMemory.Free(ptr);
        UnalignedBytesOutstanding -= (long)byteCount;
    }

    /// <summary>Zeroes <paramref name="byteCount"/> bytes of already-owned memory. Not an allocation — outstanding-byte counters are unaffected.</summary>
    internal static void Clear(void* ptr, nuint byteCount) => NativeMemory.Clear(ptr, byteCount);
}
