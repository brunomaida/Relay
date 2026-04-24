using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Relay.Memory;

namespace Relay.Buffers;

/// <summary>
/// Lock-free multi-producer / single-consumer ring buffer for unmanaged types.
/// Producers reserve slots via a single <see cref="Interlocked.CompareExchange(ref long, long, long)"/>
/// on the shared tail; consumer reads a per-slot published flag to detect completed writes.
/// </summary>
/// <remarks>
/// <para>Layout (ported from Log2's <c>MpscRingBuffer&lt;T&gt;</c> — FIX #18):</para>
/// <list type="bullet">
///   <item>Three counters (<c>_claimedTail</c>, <c>_headCache</c>, <c>_head</c>) each on its own
///         128-byte <see cref="PaddedLong"/> — zero false sharing between producer CAS, producer
///         head-cache read, and consumer head write.</item>
///   <item>Per-slot <c>Published</c> flag inside an inline <c>Slot</c> struct — producer commits
///         with a single <see cref="Volatile.Write(ref int, int)"/> after writing the value.</item>
///   <item>Producers consult a local <c>_headCache</c> before issuing a volatile read of
///         <c>_head</c>; the cross-core read happens only when the ring appears full. Under
///         contention this eliminates the dominant cache-line bounce on every <see cref="TryPublish"/>.</item>
///   <item>Backing memory: <see cref="NativeMemory.AlignedAlloc(nuint, nuint)"/> with 64-byte
///         alignment — the first slot sits on a cache line boundary, eliminating the straddle
///         caused by the ~16-byte object-header offset on POH-pinned arrays.</item>
/// </list>
/// <para>Contract: N producer threads, 1 consumer thread. Multi-consumer is undefined behaviour.</para>
/// </remarks>
internal sealed unsafe class MpscRingBuffer<T> : IDisposable where T : unmanaged
{
    /// <summary>
    /// Per-slot storage: <c>Published</c> is producer-commit / consumer-recycle flag;
    /// <c>Value</c> carries the payload. Kept as a plain (unpadded) struct — T is guaranteed
    /// to be a positive multiple of 64 bytes (Relay invariant). Slot size is
    /// <c>sizeof(T) + padding</c>; adjacent slots may still straddle cache lines depending on
    /// <c>sizeof(T)</c> — see item #4 in performance audit; revisit under BDN contention data.
    /// </summary>
    private struct Slot
    {
        public int Published;
        public T   Value;
    }

    private readonly Slot* _slots;
    private readonly int   _mask;
    private readonly int   _bytesAllocated;

    private PaddedLong _claimedTail;
    private PaddedLong _headCache;
    private PaddedLong _head;

    private bool _disposed;

    /// <summary>Slot count (power of two).</summary>
    public int Capacity { get; }

    /// <summary>Pre-faults every page of the backing slot buffer and attempts a best-effort
    /// <c>VirtualLock</c> on Windows. Called once from the consumer pipe's <c>Start()</c>.</summary>
    internal void PreFaultAndLock() => RelayMemory.PreFaultAndLock((byte*)_slots, (nuint)_bytesAllocated);

    /// <summary>True when no published records are available. Consumer thread only.</summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            long pos = _head.Value;
            ref Slot slot = ref _slots[pos & _mask];
            return Volatile.Read(ref slot.Published) == 0;
        }
    }

    /// <summary>Approximate in-flight item count (non-atomic, diagnostic only).</summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (int)(Volatile.Read(ref _claimedTail.Value) - Volatile.Read(ref _head.Value));
    }

    /// <param name="capacity">Slot count. Must be a positive power of two.</param>
    public MpscRingBuffer(int capacity)
    {
        if (capacity <= 0 || (capacity & (capacity - 1)) != 0)
            throw new ArgumentException("Capacity must be a positive power of two.", nameof(capacity));

        Capacity        = capacity;
        _mask           = capacity - 1;
        _bytesAllocated = capacity * sizeof(Slot);
        _slots          = (Slot*)NativeMemory.AlignedAlloc((nuint)_bytesAllocated, 64);
        NativeMemory.Clear(_slots, (nuint)_bytesAllocated);
    }

    /// <summary>
    /// Attempts to publish <paramref name="item"/>. Returns false when the ring is full.
    /// Safe to call from any producer thread; one <see cref="Interlocked.CompareExchange(ref long, long, long)"/>
    /// per successful write.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPublish(in T item)
    {
        while (true)
        {
            long claimed   = Volatile.Read(ref _claimedTail.Value);
            long wrapPoint = claimed - Capacity;
            long hc        = _headCache.Value;

            if (hc <= wrapPoint)
            {
                // Ring appears full — refresh from the real head.
                hc = Volatile.Read(ref _head.Value);
                _headCache.Value = hc;
                if (hc <= wrapPoint)
                    return false;
            }

            if (Interlocked.CompareExchange(ref _claimedTail.Value, claimed + 1, claimed) == claimed)
            {
                ref Slot slot = ref _slots[claimed & _mask];
                slot.Value = item;
                Volatile.Write(ref slot.Published, 1);
                return true;
            }
            // CAS lost — another producer claimed this position; retry.
        }
    }

    /// <summary>
    /// Attempts to dequeue an item. Returns false when no published record is available.
    /// Consumer thread only — multi-consumer is undefined behaviour.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConsume(out T item)
    {
        long pos = _head.Value;
        ref Slot slot = ref _slots[pos & _mask];

        if (Volatile.Read(ref slot.Published) == 0)
        {
            Unsafe.SkipInit(out item);
            return false;
        }

        item = slot.Value;
        Volatile.Write(ref slot.Published, 0);
        Volatile.Write(ref _head.Value, pos + 1);
        return true;
    }

    /// <summary>
    /// Attempts to dequeue up to <paramref name="dest"/>.Length items. Each slot carries an
    /// independent <c>Published</c> flag, so the batch path still issues one
    /// <see cref="Volatile.Read(ref int)"/> + one <see cref="Volatile.Write(ref int, int)"/> per slot;
    /// gain is a single <see cref="Volatile.Write(ref long, long)"/> of the head at the end
    /// instead of per-item. Consumer thread only.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int TryConsumeBatch(Span<T> dest)
    {
        long pos = _head.Value;
        int  n   = 0;

        while (n < dest.Length)
        {
            ref Slot slot = ref _slots[(pos + n) & _mask];
            if (Volatile.Read(ref slot.Published) == 0) break;
            dest[n] = slot.Value;
            Volatile.Write(ref slot.Published, 0);
            n++;
        }

        if (n > 0)
            Volatile.Write(ref _head.Value, pos + n);

        return n;
    }

    /// <summary>Resets counters and slot flags. Not thread-safe — call only when idle.</summary>
    public void Reset()
    {
        _claimedTail.Value = 0;
        _headCache.Value   = 0;
        _head.Value        = 0;
        NativeMemory.Clear(_slots, (nuint)_bytesAllocated);
    }

    /// <summary>Frees the native ring. Idempotent.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMemory.AlignedFree(_slots);
    }
}
