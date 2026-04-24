using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

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
/// Authoritative copy — shared with projects via Relay lib reference.
/// </summary>
/// <typeparam name="T">Unmanaged element type.</typeparam>
internal sealed class SpscRingBuffer<T> where T : unmanaged
{
    private readonly T[]  _buffer;
    private readonly int  _mask;

    private PaddedLong _head;        // consumer-owned
    private PaddedLong _cachedTail;  // consumer-only snapshot of _tail; eliminates cross-core Volatile.Read on the fast path
    private PaddedLong _tail;        // producer-owned
    private PaddedLong _cachedHead;  // producer-only snapshot of _head; eliminates cross-core Volatile.Read on the fast path

    /// <summary>Number of slots in the ring buffer.</summary>
    public int Capacity { get; }

    /// <summary>Backing array (pinned on POH). Exposed for <see cref="Relay.Memory.RelayMemory"/>.</summary>
    internal T[] Buffer => _buffer;

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

        Capacity = capacity;
        _mask    = capacity - 1;
        _buffer  = GC.AllocateArray<T>(capacity, pinned: true);
    }

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
        Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), (nint)(tail & _mask)) = item;
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
            if (head >= _cachedTail.Value) { item = default; return false; }
        }
        item = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), (nint)(head & _mask));
        Volatile.Write(ref _head.Value, head + 1);
        return true;
    }

    // --- Batched-write API for FanOut2Pipe ---

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
        Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), (nint)(tail & _mask)) = item;
    }

    /// <summary>Advances tail after all slots are written and the caller has issued a fence.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void CommitTail(long tail) =>
        Volatile.Write(ref _tail.Value, tail + 1);

    /// <summary>Resets head and tail to zero. Not thread-safe — call only when idle.</summary>
    public void Reset() { _head.Value = 0; _tail.Value = 0; }
}
