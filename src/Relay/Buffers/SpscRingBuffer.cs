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

    private PaddedLong _head;
    private PaddedLong _tail;

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
        long head = Volatile.Read(ref _head.Value);

        if (tail - head >= Capacity) return false;

        _buffer[tail & _mask] = item;
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
        long tail = Volatile.Read(ref _tail.Value);

        if (head >= tail) { item = default; return false; }

        item = _buffer[head & _mask];
        Volatile.Write(ref _head.Value, head + 1);
        return true;
    }

    // --- Batched-write API for FanOut2Pipe ---

    /// <summary>Reserves a tail slot without writing. Returns false when full.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryReserveTail(out long tail)
    {
        tail = _tail.Value;
        return tail - Volatile.Read(ref _head.Value) < Capacity;
    }

    /// <summary>Writes item into the reserved slot. No memory fence — caller issues fence.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe void WriteSlot(long tail, in T item) =>
        Unsafe.CopyBlockUnaligned(
            ref Unsafe.As<T, byte>(ref _buffer[tail & _mask]),
            ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in item)),
            (uint)sizeof(T));

    /// <summary>Advances tail after all slots are written and the caller has issued a fence.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void CommitTail(long tail) =>
        Volatile.Write(ref _tail.Value, tail + 1);

    /// <summary>Resets head and tail to zero. Not thread-safe — call only when idle.</summary>
    public void Reset() { _head.Value = 0; _tail.Value = 0; }
}
