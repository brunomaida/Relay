using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Relay.Memory;

namespace Relay.Buffers;

/// <summary>128-byte padded long to prevent false sharing between producer/consumer cache lines.</summary>
[StructLayout(LayoutKind.Explicit, Size = 128)]
internal struct PaddedLong
{
    [FieldOffset(64)] public long Value;
}

/// <summary>
/// Lock-free single-producer/single-consumer ring buffer for unmanaged types.
/// Head and tail use 128-byte padded structs to avoid false sharing across cache lines.
/// Backing memory is allocated via <see cref="NativeMemory.AlignedAlloc(nuint, nuint)"/> with
/// 64-byte alignment — guarantees every slot starts on a cache-line boundary when
/// <c>sizeof(T)</c> is a multiple of 64 (Relay invariant). The pinned-POH array path allocates
/// with only 8-byte alignment on .NET (object header precedes elements), which would cause
/// slots to straddle cache lines for all T ≥ 64B.
/// </summary>
/// <typeparam name="T">Unmanaged element type.</typeparam>
internal sealed unsafe class SpscRingBuffer<T> : IDisposable where T : unmanaged
{
    private readonly T*  _basePtr;
    private readonly int _mask;
    private readonly int _bytesAllocated;

    private PaddedLong _head;        // consumer-owned
    private PaddedLong _cachedTail;  // consumer-only snapshot of _tail; eliminates cross-core Volatile.Read on the fast path
    private PaddedLong _tail;        // producer-owned
    private PaddedLong _cachedHead;  // producer-only snapshot of _head; eliminates cross-core Volatile.Read on the fast path

    private bool _disposed;

    /// <summary>Number of slots in the ring buffer.</summary>
    public int Capacity { get; }

    /// <summary>Current number of items (approximate, non-atomic).</summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (int)(Volatile.Read(ref _tail.Value) - Volatile.Read(ref _head.Value));
    }

    /// <summary>True when the ring contains no available slots.</summary>
    public bool IsFull
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _tail.Value - Volatile.Read(ref _head.Value) >= Capacity;
    }

    /// <summary>True when no items are available. Call from consumer thread only — head read is non-volatile.</summary>
    internal bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _head.Value >= Volatile.Read(ref _tail.Value);
    }

    /// <param name="capacity">Must be a positive power of two.</param>
    public SpscRingBuffer(int capacity)
    {
        if (capacity <= 0 || (capacity & (capacity - 1)) != 0)
            throw new ArgumentException("Capacity must be a positive power of two.", nameof(capacity));

        Capacity        = capacity;
        _mask           = capacity - 1;
        _bytesAllocated = capacity * sizeof(T);
        _basePtr        = (T*)NativeMemory.AlignedAlloc((nuint)_bytesAllocated, 64);
        NativeMemory.Clear(_basePtr, (nuint)_bytesAllocated);
    }

    /// <summary>Pre-faults every page of the backing ring and attempts <c>VirtualLock</c> on Windows.</summary>
    internal void PreFaultAndLock() => RelayMemory.PreFaultAndLock((byte*)_basePtr, (nuint)_bytesAllocated);

    /// <summary>
    /// Attempts to publish an item. Returns false if the buffer is full.
    /// Must be called from a single producer thread only.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPublish(in T item)
    {
        long tail = _tail.Value;
        long wrap = tail - Capacity;
        if (_cachedHead.Value <= wrap)
        {
            _cachedHead.Value = Volatile.Read(ref _head.Value);
            if (_cachedHead.Value <= wrap) return false;
        }
        _basePtr[tail & _mask] = item;
        Volatile.Write(ref _tail.Value, tail + 1);
        return true;
    }

    /// <summary>
    /// Attempts to consume an item. Returns false if the buffer is empty.
    /// Must be called from a single consumer thread only.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConsume(out T item)
    {
        long head = _head.Value;
        if (head >= _cachedTail.Value)
        {
            _cachedTail.Value = Volatile.Read(ref _tail.Value);
            if (head >= _cachedTail.Value) { Unsafe.SkipInit(out item); return false; }
        }
        item = _basePtr[head & _mask];
        Volatile.Write(ref _head.Value, head + 1);
        return true;
    }

    /// <summary>
    /// Attempts to consume up to <paramref name="dest"/>.Length items into <paramref name="dest"/>
    /// with a single <see cref="Volatile.Read(ref long)"/> of the producer tail and a single
    /// <see cref="Volatile.Write(ref long, long)"/> of the consumer head. Returns the item count written.
    /// Single consumer thread only.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int TryConsumeBatch(Span<T> dest)
    {
        long head = _head.Value;
        if (head >= _cachedTail.Value)
        {
            _cachedTail.Value = Volatile.Read(ref _tail.Value);
            if (head >= _cachedTail.Value) return 0;
        }

        long available = _cachedTail.Value - head;
        int  n         = (int)Math.Min(available, dest.Length);

        for (int i = 0; i < n; i++)
            dest[i] = _basePtr[(head + i) & _mask];

        Volatile.Write(ref _head.Value, head + n);
        return n;
    }

    // --- Batched-write API for Multi2Pipe and producer batch ---

    /// <summary>Reserves a tail slot without writing. Returns false when full.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryReserveTail(out long tail)
    {
        tail = _tail.Value;
        long wrap = tail - Capacity;
        if (_cachedHead.Value <= wrap)
        {
            _cachedHead.Value = Volatile.Read(ref _head.Value);
            if (_cachedHead.Value <= wrap) return false;
        }
        return true;
    }

    /// <summary>Writes item into the reserved slot. No memory fence — caller issues fence.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteSlot(long tail, in T item)
    {
        _basePtr[tail & _mask] = item;
    }

    /// <summary>Advances tail after all slots are written and the caller has issued a fence.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void CommitTail(long tail) =>
        Volatile.Write(ref _tail.Value, tail + 1);

    /// <summary>
    /// Attempts to publish <paramref name="batch"/> in one shot: reserves range, writes all slots,
    /// issues one <see cref="Thread.MemoryBarrier"/> plus one <see cref="Volatile.Write(ref long, long)"/>
    /// on tail. Single producer thread only. Returns count actually published (0 when full).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int TryPublishBatch(ReadOnlySpan<T> batch)
    {
        if (batch.Length == 0) return 0;

        long tail = _tail.Value;
        long wrap = tail - Capacity;
        if (_cachedHead.Value <= wrap + batch.Length - 1)
        {
            _cachedHead.Value = Volatile.Read(ref _head.Value);
            if (_cachedHead.Value <= wrap) return 0;
        }

        long free = _cachedHead.Value - wrap;
        int  n    = (int)Math.Min(free, batch.Length);

        for (int i = 0; i < n; i++)
            _basePtr[(tail + i) & _mask] = batch[i];

        Thread.MemoryBarrier();
        Volatile.Write(ref _tail.Value, tail + n);
        return n;
    }

    /// <summary>Resets head and tail to zero. Not thread-safe — call only when idle.</summary>
    public void Reset() { _head.Value = 0; _tail.Value = 0; _cachedHead.Value = 0; _cachedTail.Value = 0; }

    /// <summary>Frees the native ring. Idempotent.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMemory.AlignedFree(_basePtr);
    }
}
