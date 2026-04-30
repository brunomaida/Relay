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
/// <para>Layout (ported from Log2's <c>MpscRingBuffer&lt;T&gt;</c> — FIX #18; slot layout
/// revised in v4 audit to eliminate cache-line straddle):</para>
/// <list type="bullet">
///   <item>Three counters (<c>_claimedTail</c>, <c>_headCache</c>, <c>_head</c>) each on its own
///         128-byte <see cref="PaddedLong"/> — zero false sharing between producer CAS, producer
///         head-cache read, and consumer head write.</item>
///   <item>Per-slot <c>Published</c> flag at stride offset 0 (64-byte flag cache line);
///         <c>Value</c> at stride offset 64 (64-byte payload cache line).
///         Stride = 64 + sizeof(T). Every slot is exactly 2 cache lines; no slot straddles
///         a boundary. Backing allocation: <c>NativeMemory.AlignedAlloc(capacity * stride, 64)</c>.</item>
///   <item>Producers consult a local <c>_headCache</c> before issuing a volatile read of
///         <c>_head</c>; the cross-core read happens only when the ring appears full. Under
///         contention this eliminates the dominant cache-line bounce on every <see cref="TryPublish"/>.</item>
/// </list>
/// <para>Contract: N producer threads, 1 consumer thread. Multi-consumer is undefined behaviour.</para>
/// </remarks>
internal sealed unsafe class MpscRingBuffer<T> : IDisposable where T : unmanaged
{
    private readonly byte* _basePtr;
    private readonly int   _stride;          // = 64 + sizeof(T)
    private readonly nuint _bytesAllocated;
    private readonly int   _mask;

    private PaddedLong _claimedTail;
    private PaddedLong _headCache;
    private PaddedLong _head;

    private bool _disposed;

    /// <summary>Slot count (power of two).</summary>
    public int Capacity { get; }

    /// <summary>Pre-faults every page of the backing slot buffer and attempts a best-effort
    /// <c>VirtualLock</c> on Windows. Called once from the consumer pipe's <c>Start()</c>.</summary>
    internal void PreFaultAndLock() => RelayMemory.PreFaultAndLock(_basePtr, _bytesAllocated);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int PublishedAt(long pos) =>
        ref Unsafe.AsRef<int>(_basePtr + (pos & _mask) * _stride);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref T ValueAt(long pos) =>
        ref Unsafe.AsRef<T>(_basePtr + (pos & _mask) * _stride + 64);

    /// <summary>True when no published records are available. Consumer thread only.</summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Volatile.Read(ref PublishedAt(_head.Value)) == 0;
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
        _stride         = 64 + sizeof(T);
        _bytesAllocated = (nuint)(capacity * _stride);
        _basePtr        = (byte*)NativeMemory.AlignedAlloc(_bytesAllocated, 64);
        NativeMemory.Clear(_basePtr, _bytesAllocated);
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
                ValueAt(claimed) = item;
                Volatile.Write(ref PublishedAt(claimed), 1);
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

        if (Volatile.Read(ref PublishedAt(pos)) == 0)
        {
            Unsafe.SkipInit(out item);
            return false;
        }

        item = ValueAt(pos);
        Unsafe.InitBlockUnaligned(ref Unsafe.As<T, byte>(ref ValueAt(pos)), 0, (uint)sizeof(T));
        Volatile.Write(ref PublishedAt(pos), 0);
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
            if (Volatile.Read(ref PublishedAt(pos + n)) == 0) break;
            dest[n] = ValueAt(pos + n);
            Unsafe.InitBlockUnaligned(ref Unsafe.As<T, byte>(ref ValueAt(pos + n)), 0, (uint)sizeof(T));
            Volatile.Write(ref PublishedAt(pos + n), 0);
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
        NativeMemory.Clear(_basePtr, _bytesAllocated);
    }

    /// <summary>Frees the native ring. Idempotent.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMemory.AlignedFree(_basePtr);
    }
}
