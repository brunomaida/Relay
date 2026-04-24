using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Relay.Buffers;

/// <summary>
/// Lock-free single-producer/single-consumer byte ring with length-prefixed records.
/// Each record = <c>[uint32 length LE][payload, 4-byte aligned]</c>. Wrap is handled via a
/// padding marker (sentinel header <c>0xFFFFFFFF</c>) so that the payload is always stored
/// contiguously; consumers never see split payloads. Head and tail are 128-byte padded to
/// avoid false sharing between producer and consumer cache lines.
/// </summary>
/// <remarks>
/// <para>Invariants (producer-side):</para>
/// <list type="bullet">
/// <item><c>Capacity</c> is a power of two ≥ 16.</item>
/// <item><c>_tail</c> is always 4-byte aligned — every record advances tail by a 4-multiple.</item>
/// <item>Header (4 bytes) is always contiguous — guaranteed by the alignment invariant above.</item>
/// <item>Payload is always contiguous — when a record would straddle the wrap point, the
///       producer writes a padding marker at the tail position and restarts the record at offset 0.</item>
/// </list>
/// <para>
/// These invariants eliminate the need for a split-payload scratch buffer on the consumer side
/// (cf. the typed <see cref="SpscRingBuffer{T}"/>, where wrap handling is implicit in the index
/// arithmetic because records are fixed-size and power-of-two).
/// </para>
/// </remarks>
internal sealed class SpscByteRingBuffer : IDisposable
{
    /// <summary>Sentinel header value marking a wrap-padding slot. Reserved length.</summary>
    internal const uint PaddingMarker = 0xFFFFFFFFu;

    /// <summary>Length-prefix size in bytes.</summary>
    internal const int  HeaderSize    = 4;

    private const int MinCapacity = 16;

    private readonly byte[] _buffer;
    private readonly int    _mask;

    private PaddedLong _head;
    private PaddedLong _tail;

    /// <summary>Number of bytes in the underlying storage.</summary>
    public int Capacity { get; }

    /// <summary>Backing byte[] (pinned on POH). Exposed for <see cref="Relay.Memory.RelayMemory"/>.</summary>
    internal byte[] Buffer => _buffer;

    /// <summary>True when no records are available. Call from consumer thread only.</summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _head.Value >= Volatile.Read(ref _tail.Value);
    }

    /// <summary>Bytes currently occupied (approximate, non-atomic).</summary>
    public int ByteCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (int)(Volatile.Read(ref _tail.Value) - Volatile.Read(ref _head.Value));
    }

    /// <param name="capacity">Ring size in bytes. Power of two, minimum 16.</param>
    public SpscByteRingBuffer(int capacity)
    {
        if (capacity < MinCapacity || (capacity & (capacity - 1)) != 0)
            throw new ArgumentException(
                $"Capacity must be a power of two ≥ {MinCapacity}.", nameof(capacity));
        Capacity = capacity;
        _mask    = capacity - 1;
        _buffer  = GC.AllocateArray<byte>(capacity, pinned: true);
    }

    /// <summary>
    /// Attempts to publish <paramref name="payload"/>. Returns false if the ring has insufficient
    /// free space, or if the payload is larger than can ever fit (<c>recordSize &gt; Capacity</c>).
    /// Producer thread only.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPublish(ReadOnlySpan<byte> payload)
    {
        int len = payload.Length;
        if ((uint)len >= PaddingMarker) return false;
        int paddedLen  = (len + 3) & ~3;
        int recordSize = HeaderSize + paddedLen;
        if (recordSize > Capacity) return false;

        long tail = _tail.Value;
        long head = Volatile.Read(ref _head.Value);
        long free = Capacity - (tail - head);

        int pos        = (int)(tail & _mask);
        int contiguous = Capacity - pos;

        if (contiguous >= recordSize)
        {
            if (free < recordSize) return false;
            WriteHeader(pos, (uint)len);
            payload.CopyTo(_buffer.AsSpan(pos + HeaderSize, len));
            Volatile.Write(ref _tail.Value, tail + recordSize);
            return true;
        }

        // Wrap: pad from pos to end, then write record at offset 0.
        long totalNeeded = (long)contiguous + recordSize;
        if (free < totalNeeded) return false;

        WriteHeader(pos, PaddingMarker);
        tail += contiguous;
        WriteHeader(0, (uint)len);
        payload.CopyTo(_buffer.AsSpan(HeaderSize, len));
        Volatile.Write(ref _tail.Value, tail + recordSize);
        return true;
    }

    /// <summary>
    /// Peeks the next record without advancing head. Returns a zero-copy
    /// <see cref="ReadOnlySpan{Byte}"/> over the backing buffer; <paramref name="advanceBytes"/>
    /// is the amount the caller must pass to <see cref="Advance"/> after consuming. Consumer
    /// thread only.
    /// </summary>
    /// <remarks>
    /// Transparently skips padding markers. When the ring is empty, returns false with
    /// <c>payload = default</c>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out ReadOnlySpan<byte> payload, out int advanceBytes)
    {
        long head = _head.Value;
        long tail = Volatile.Read(ref _tail.Value);
        if (head >= tail) { payload = default; advanceBytes = 0; return false; }

        int pos  = (int)(head & _mask);
        uint hdr = ReadHeader(pos);

        if (hdr == PaddingMarker)
        {
            int skip = Capacity - pos;
            head += skip;
            Volatile.Write(ref _head.Value, head);
            if (head >= tail) { payload = default; advanceBytes = 0; return false; }
            pos = (int)(head & _mask);
            hdr = ReadHeader(pos);
        }

        int len        = (int)hdr;
        int paddedLen  = (len + 3) & ~3;
        payload        = _buffer.AsSpan(pos + HeaderSize, len);
        advanceBytes   = HeaderSize + paddedLen;
        return true;
    }

    /// <summary>Advances the consumer head by <paramref name="bytes"/>. Consumer thread only.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int bytes)
    {
        long head = _head.Value;
        Volatile.Write(ref _head.Value, head + bytes);
    }

    /// <summary>Resets head and tail. Not thread-safe — call only when idle.</summary>
    public void Reset() { _head.Value = 0; _tail.Value = 0; }

    /// <summary>No-op; the POH-pinned array is reclaimed by the GC when the ring is collected.</summary>
    public void Dispose() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteHeader(int pos, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.AsSpan(pos, HeaderSize), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint ReadHeader(int pos) =>
        BinaryPrimitives.ReadUInt32LittleEndian(_buffer.AsSpan(pos, HeaderSize));
}
