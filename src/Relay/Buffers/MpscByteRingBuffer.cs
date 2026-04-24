using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Relay.Memory;

namespace Relay.Buffers;

/// <summary>
/// Lock-free multi-producer / single-consumer ring for variable-length byte payloads.
/// Producers atomically reserve a contiguous byte range via
/// <see cref="Interlocked.CompareExchange(ref long, long, long)"/> on the shared tail, write
/// the payload, then publish via a volatile header store. One consumer thread reads headers
/// in order, skipping wrap-padding markers.
/// </summary>
/// <remarks>
/// <para>Header layout (4 bytes, little-endian):</para>
/// <list type="bullet">
///   <item>High bit clear (<c>0x80000000 == 0</c>): reserved but not yet published — consumer
///         must wait.</item>
///   <item>High bit set, low 31 bits ≤ <see cref="MaxLength"/>: published record; low bits
///         carry the actual payload length.</item>
///   <item>High bit set, low 31 bits == <c>0x7FFFFFFF</c> (sentinel): padding marker from a
///         producer that had to skip to the next wrap boundary.</item>
/// </list>
/// <para>Producer sequence on a successful reservation:</para>
/// <list type="number">
///   <item>Plain copy of the payload bytes (POH store order is preserved on x86/x64 TSO and
///         enforced by the subsequent volatile write on weaker-memory platforms).</item>
///   <item><see cref="Volatile.Write(ref int, int)"/> of the record header — release fence.</item>
///   <item>If the reservation wrapped the buffer, <see cref="Volatile.Write(ref int, int)"/> of
///         the padding marker at the original tail position.</item>
/// </list>
/// <para>Consumer sequence:</para>
/// <list type="number">
///   <item><see cref="Volatile.Read(ref int)"/> of the header at <c>_head</c>.</item>
///   <item>If padding: clear the header with a volatile store of <c>0</c> and advance
///         <c>_head</c> past the wrap; retry.</item>
///   <item>If published: return a zero-copy span to the payload. On <see cref="Advance"/>
///         clear the header (volatile zero) before advancing <c>_head</c> — this recycles the
///         slot for the next producer generation.</item>
/// </list>
/// <para>
/// The header-clear-before-head-advance pattern is the same invariant used in Log2's typed
/// MPSC slot: producers reserve a monotonic range via CAS; consumers release slots by
/// clearing the published flag before advancing head.
/// </para>
/// </remarks>
internal sealed class MpscByteRingBuffer : IDisposable
{
    internal const uint HighBit       = 0x80000000u;
    internal const uint LengthMask    = 0x7FFFFFFFu;
    internal const uint PaddingLowBits = 0x7FFFFFFFu;
    internal const uint PaddingFull   = 0xFFFFFFFFu;
    internal const uint MaxLength     = 0x7FFFFFFEu;
    internal const int  HeaderSize    = 4;

    private const int MinCapacity = 16;

    private readonly byte[] _buffer;
    private readonly int    _mask;

    private PaddedLong _claimedTail;
    private PaddedLong _headCache;
    private PaddedLong _head;

    /// <summary>Ring size in bytes (power of two).</summary>
    public int Capacity { get; }

    /// <summary>Backing byte[] (pinned on POH). Exposed for <see cref="Relay.Memory.RelayMemory"/>.</summary>
    internal byte[] Buffer => _buffer;

    /// <summary>True when no published record is available at <c>_head</c>. Consumer thread only.</summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            long pos = _head.Value;
            uint hdr = ReadHeaderVolatile((int)(pos & _mask));
            return (hdr & HighBit) == 0;
        }
    }

    /// <summary>Approximate in-flight byte count (non-atomic, diagnostic only).</summary>
    public int ByteCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (int)(Volatile.Read(ref _claimedTail.Value) - Volatile.Read(ref _head.Value));
    }

    /// <param name="capacity">Ring size in bytes. Power of two, minimum 16.</param>
    public MpscByteRingBuffer(int capacity)
    {
        if (capacity < MinCapacity || (capacity & (capacity - 1)) != 0)
            throw new ArgumentException(
                $"Capacity must be a power of two ≥ {MinCapacity}.", nameof(capacity));

        Capacity = capacity;
        _mask    = capacity - 1;
        _buffer  = GC.AllocateArray<byte>(capacity, pinned: true);
    }

    /// <summary>
    /// Attempts to publish <paramref name="payload"/>. Returns false when the ring has
    /// insufficient free space OR the record would exceed <see cref="Capacity"/>. Safe from any
    /// producer thread — one <see cref="Interlocked.CompareExchange(ref long, long, long)"/>
    /// per successful reservation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPublish(ReadOnlySpan<byte> payload)
    {
        int len = payload.Length;
        if ((uint)len > MaxLength) return false;
        int paddedLen  = (len + 3) & ~3;
        int recordSize = HeaderSize + paddedLen;
        if (recordSize > Capacity) return false;

        while (true)
        {
            long claimed   = Volatile.Read(ref _claimedTail.Value);
            int  pos       = (int)(claimed & _mask);
            int  contiguous = Capacity - pos;

            int  reserve;
            bool needsWrap;
            if (contiguous >= recordSize)
            {
                reserve   = recordSize;
                needsWrap = false;
            }
            else
            {
                reserve   = contiguous + recordSize;
                needsWrap = true;
            }

            long wrapPoint = claimed + reserve - Capacity;
            long hc        = _headCache.Value;
            if (hc <= wrapPoint)
            {
                hc = Volatile.Read(ref _head.Value);
                _headCache.Value = hc;
                if (hc <= wrapPoint) return false;
            }

            if (Interlocked.CompareExchange(ref _claimedTail.Value, claimed + reserve, claimed) != claimed)
                continue;

            if (needsWrap)
            {
                // Write payload at offset 0 (after-wrap slot), publish its header, then stamp padding marker at original pos.
                payload.CopyTo(_buffer.AsSpan(HeaderSize, len));
                PublishHeader(0, (uint)len | HighBit);
                PublishHeader(pos, PaddingFull);
            }
            else
            {
                payload.CopyTo(_buffer.AsSpan(pos + HeaderSize, len));
                PublishHeader(pos, (uint)len | HighBit);
            }
            return true;
        }
    }

    /// <summary>
    /// Peeks the next published record. Returns a zero-copy <see cref="ReadOnlySpan{Byte}"/>
    /// into the backing buffer; caller must pass <paramref name="advanceBytes"/> to
    /// <see cref="Advance"/> after consuming. Consumer thread only.
    /// </summary>
    /// <remarks>Transparently skips padding markers by self-recursing.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out ReadOnlySpan<byte> payload, out int advanceBytes)
    {
        long pos = _head.Value;
        int  idx = (int)(pos & _mask);
        uint hdr = ReadHeaderVolatile(idx);

        if ((hdr & HighBit) == 0)
        {
            payload      = default;
            advanceBytes = 0;
            return false;
        }

        uint lenField = hdr & LengthMask;
        if (lenField == PaddingLowBits)
        {
            int skip = Capacity - idx;
            PublishHeader(idx, 0);
            Volatile.Write(ref _head.Value, pos + skip);
            return TryPeek(out payload, out advanceBytes);
        }

        int len       = (int)lenField;
        int paddedLen = (len + 3) & ~3;
        payload       = _buffer.AsSpan(idx + HeaderSize, len);
        advanceBytes  = HeaderSize + paddedLen;
        return true;
    }

    /// <summary>
    /// Advances head by <paramref name="bytes"/>. Clears the record header first (volatile
    /// zero) to recycle the slot for future producers, then releases head. Consumer thread only.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int bytes)
    {
        long pos = _head.Value;
        int  idx = (int)(pos & _mask);
        PublishHeader(idx, 0);
        Volatile.Write(ref _head.Value, pos + bytes);
    }

    /// <summary>Resets counters and clears all slot headers. Not thread-safe — call only when idle.</summary>
    public void Reset()
    {
        _claimedTail.Value = 0;
        _headCache.Value   = 0;
        _head.Value        = 0;
        Array.Clear(_buffer);
    }

    /// <summary>Pre-faults every page of the backing byte[] and attempts best-effort VirtualLock on Windows.</summary>
    internal void PreFaultAndLock() => RelayMemory.PreFaultAndLock(_buffer);

    /// <summary>No-op; the POH-pinned array is reclaimed by the GC when the ring is collected.</summary>
    public void Dispose() { }

    // --- header I/O: plain read for setup/reset, volatile read/write for producer-consumer synchronization ---

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PublishHeader(int pos, uint value)
    {
        ref byte bufRef = ref MemoryMarshal.GetArrayDataReference(_buffer);
        ref int  hdrRef = ref Unsafe.As<byte, int>(ref Unsafe.AddByteOffset(ref bufRef, pos));
        Volatile.Write(ref hdrRef, (int)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint ReadHeaderVolatile(int pos)
    {
        ref byte bufRef = ref MemoryMarshal.GetArrayDataReference(_buffer);
        ref int  hdrRef = ref Unsafe.As<byte, int>(ref Unsafe.AddByteOffset(ref bufRef, pos));
        return (uint)Volatile.Read(ref hdrRef);
    }
}
