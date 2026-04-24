using System;
using System.Runtime.CompilerServices;
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
/// </list>
/// <para>Contract: N producer threads, 1 consumer thread. Multi-consumer is undefined behaviour.</para>
/// </remarks>
internal sealed class MpscRingBuffer<T> where T : unmanaged
{
    /// <summary>
    /// Per-slot storage: <c>Published</c> is producer-commit / consumer-recycle flag;
    /// <c>Value</c> carries the payload. Kept as a plain (unpadded) struct — T is guaranteed
    /// to be a positive multiple of 64 bytes (Relay invariant), so adjacent slot
    /// <c>Published</c> fields land on different cache lines and don't false-share.
    /// </summary>
    private struct Slot
    {
        public int Published;
        public T   Value;
    }

    private readonly Slot[] _slots;
    private readonly int    _mask;

    private PaddedLong _claimedTail;
    private PaddedLong _headCache;
    private PaddedLong _head;

    /// <summary>Slot count (power of two).</summary>
    public int Capacity { get; }

    /// <summary>Pre-faults every page of the backing slot array and attempts a best-effort
    /// <c>VirtualLock</c> on Windows. Called once from the consumer pipe's <c>Start()</c>.</summary>
    internal void PreFaultAndLock() => RelayMemory.PreFaultAndLock(_slots);

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

        Capacity = capacity;
        _mask    = capacity - 1;
        _slots   = GC.AllocateArray<Slot>(capacity, pinned: true);
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
            item = default;
            return false;
        }

        item = slot.Value;
        Volatile.Write(ref slot.Published, 0);
        Volatile.Write(ref _head.Value, pos + 1);
        return true;
    }

    /// <summary>Resets counters and slot flags. Not thread-safe — call only when idle.</summary>
    public void Reset()
    {
        _claimedTail.Value = 0;
        _headCache.Value   = 0;
        _head.Value        = 0;
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].Published = 0;
            _slots[i].Value     = default;
        }
    }
}
