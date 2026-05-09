using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Relay.Sinks;

/// <summary>
/// Last-resort <see cref="PacketSink"/> fallback backed by a fixed-size native memory buffer.
/// No consumer thread — <see cref="Accept"/> writes synchronously on the producer thread.
/// Call <see cref="DrainTo"/> from the recovery path once the primary sink recovers.
/// </summary>
/// <remarks>
/// <para><b>Thread contract:</b> SPSC non-concurrent. <see cref="Accept"/> runs on the producer
/// thread; <see cref="DrainTo"/> on the recovery thread. NEVER simultaneously. The caller
/// guarantees producer quiescence before invoking <see cref="DrainTo"/>. No CAS —
/// <c>Volatile.Write</c>/<c>Volatile.Read</c> on <c>_head</c>/<c>_tail</c> suffice.</para>
/// <para><b>Layout:</b> Fill-once non-circular. Records fill <c>_buffer[0.._capacity]</c>
/// linearly. Record = <c>[uint32 length (host order)][payload][padding to 4-byte multiple]</c>.
/// <c>recordSize = 4 + ((payloadLen + 3) &amp; ~3)</c>. Length is host-order because the buffer
/// is process-local (never crosses wire).</para>
/// <para><b>Capacity:</b> Partial drain does NOT free capacity. Only full drain
/// (<c>_head &gt;= _tail</c>) resets the pointers to zero, reopening the buffer.</para>
/// </remarks>
// unsealed to allow [Obsolete] RamSink compat shim in _Compat/
public unsafe class MemorySink : PacketSink
{
    private readonly byte* _buffer;
    private readonly int   _capacity;

    private int _head;  // consumer-owned; advanced by DrainTo
    private int _tail;  // producer-owned; advanced by Accept

    private bool _disposed;

    /// <param name="capacity">Buffer size in bytes. Must be a positive power of two.</param>
    public MemorySink(int capacity = 4 * 1024 * 1024)
    {
        if (capacity <= 0 || (capacity & (capacity - 1)) != 0)
            throw new ArgumentException("Capacity must be a positive power of two.", nameof(capacity));
        _capacity = capacity;
        _buffer   = (byte*)NativeMemory.AlignedAlloc((nuint)capacity, 64);
    }

    /// <summary>
    /// True when <c>_tail &lt; _capacity</c> — conservative approximation. <see cref="Accept"/>
    /// is authoritative for fit: a large payload may still overflow when <see cref="IsHealthy"/>
    /// is true, in which case <see cref="Accept"/> returns false and the payload falls through to
    /// <see cref="PacketSink.Next"/>.
    /// </summary>
    public override bool IsHealthy => _tail < _capacity;

    /// <summary>
    /// Writes length prefix + payload at absolute position <c>_tail</c>. Returns false when the
    /// record would exceed the buffer. Fill-once: partial drain does NOT free capacity — only
    /// a full <see cref="DrainTo"/> (which resets both pointers to zero) reopens the buffer.
    /// Safe to call from the producer thread only.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override unsafe bool Accept(ReadOnlySpan<byte> payload)
    {
        int alignedLen = (payload.Length + 3) & ~3;
        int recordSize = 4 + alignedLen;

        // Absolute-position check (not _tail - _head): after a partial drain, _head may have
        // advanced but _tail stays put. Writing at _buffer + _tail with _tail ~= _capacity
        // would overflow the buffer. Only a full drain reset frees _tail.
        if (_tail + recordSize > _capacity) return false;

        *(uint*)(_buffer + _tail) = (uint)payload.Length;
        fixed (byte* src = payload)
            Unsafe.CopyBlockUnaligned(_buffer + _tail + 4, src, (uint)payload.Length);

        Volatile.Write(ref _tail, _tail + recordSize);
        return true;
    }

    /// <summary>
    /// Replays all buffered payloads to <paramref name="target"/> in order.
    /// Stops if <paramref name="target"/> becomes unhealthy. Resets head and tail when fully drained.
    /// Call from the recovery consumer thread only — never concurrently with <see cref="Accept"/>.
    /// </summary>
    public unsafe void DrainTo(PacketSink target)
    {
        while (_head < Volatile.Read(ref _tail))
        {
            if (!target.IsHealthy) return;

            int len        = (int)*(uint*)(_buffer + _head);
            int alignedLen = (len + 3) & ~3;

            target.Enqueue(new ReadOnlySpan<byte>(_buffer + _head + 4, len));
            Volatile.Write(ref _head, _head + 4 + alignedLen);
        }

        // Full drain complete — reset pointers so the buffer can be reused.
        if (_head >= _tail)
        {
            _head = 0;
            Volatile.Write(ref _tail, 0);
        }
    }

    /// <inheritdoc/>
    public override void Flush() { }   // No consumer thread; nothing to flush.

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMemory.AlignedFree(_buffer);
    }
}
