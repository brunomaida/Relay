using System;
using System.Buffers.Binary;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Relay.Sinks;

/// <summary>
/// Synchronous <see cref="PacketSink"/> that writes payloads to a named
/// <see cref="MemoryMappedFile"/> ring buffer using the Log2 SharedMemorySink wire protocol.
/// </summary>
/// <remarks>
/// <para>
/// <b>MMF layout (128-byte header):</b><br/>
/// CL0 [0..63]:  Magic(4) + DataCapacity(4) + WriteIndex(4) + pad(52)<br/>
/// CL1 [64..127]: ReadIndex(4) + pad(60)<br/>
/// [128..] Data area: [4B BE length][N bytes payload] ... (ring-wrapped)
/// </para>
/// <para>
/// <b>WriteIndex semantics:</b> modular int, wraps on every increment
/// (<c>newIdx = (oldIdx + frameLen) % _dataCapacity</c>). Matches Log2 exactly.
/// MPSC-tolerant via <see cref="Interlocked.CompareExchange(ref int,int,int)"/> on WriteIndex.
/// </para>
/// <para>
/// <b>Platform:</b> Named MMFs require Windows. Will throw on other platforms.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed unsafe class SharedMemorySink : PacketSink
{
    // Must match Log2 SharedMemorySink exactly — "LG2\0" = 0x4C473200
    private const uint SHM_MAGIC     = 0x4C473200u;
    private const int  HEADER_SIZE   = 128;
    private const int  WRITE_IDX_OFF = 8;
    private const int  READ_IDX_OFF  = 64;

    private readonly MemoryMappedFile         _mmf;
    private readonly MemoryMappedViewAccessor _view;
    private readonly byte*                    _ptr;
    private readonly int                      _dataCapacity;
    private          bool                     _disposed;

    /// <summary>
    /// Creates or opens a named MMF ring buffer compatible with Log2's SharedMemorySink.
    /// </summary>
    /// <param name="name">Named MMF identifier (e.g. "Local\\my-shm"). Must match the Log2 producer.</param>
    /// <param name="totalCapacity">Total MMF size in bytes, including the 128-byte header.
    /// Must exceed 128. Ignored when opening an existing mapping.</param>
    public SharedMemorySink(string name, int totalCapacity = 4 * 1024 * 1024)
    {
        if (totalCapacity <= HEADER_SIZE)
            throw new ArgumentException("totalCapacity must exceed 128B header.", nameof(totalCapacity));

        _dataCapacity = totalCapacity - HEADER_SIZE;
        _mmf          = MemoryMappedFile.CreateOrOpen(name, totalCapacity);
        _view         = _mmf.CreateViewAccessor(0, totalCapacity);

        byte* ptr = null;
        _view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        _ptr = ptr;

        // Initialize header only if we are the first creator (CAS on magic field).
        // Matches Log2 SharedMemorySink conditional-init pattern for CreateOrOpen safety.
        if (Interlocked.CompareExchange(ref *(int*)_ptr, (int)SHM_MAGIC, 0) == 0)
        {
            *(int*)(_ptr + 4) = _dataCapacity;
            Volatile.Write(ref *(int*)(_ptr + WRITE_IDX_OFF), 0);
            Volatile.Write(ref *(int*)(_ptr + READ_IDX_OFF),  0);
        }
    }

    /// <inheritdoc/>
    public override bool IsHealthy
    {
        get
        {
            int write = Volatile.Read(ref *(int*)(_ptr + WRITE_IDX_OFF));
            int read  = Volatile.Read(ref *(int*)(_ptr + READ_IDX_OFF));
            // Compute unread bytes in the ring (modular distance)
            int unread = (write - read + _dataCapacity) % _dataCapacity;
            return unread < _dataCapacity;
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        int frameLen = 4 + payload.Length;
        if (frameLen > _dataCapacity) return false;

        // CAS to claim write slot — modular index, wraps at _dataCapacity.
        // Matches Log2 SharedMemorySink exactly; no buffer-full guard in Log2.
        int oldIdx, newIdx;
        do
        {
            oldIdx = Volatile.Read(ref *(int*)(_ptr + WRITE_IDX_OFF));
            newIdx = (oldIdx + frameLen) % _dataCapacity;
        }
        while (Interlocked.CompareExchange(
            ref *(int*)(_ptr + WRITE_IDX_OFF), newIdx, oldIdx) != oldIdx);

        byte* data = _ptr + HEADER_SIZE;

        // Write 4-byte big-endian length prefix, then payload — both ring-wrapped.
        Span<byte> lenBuf = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(lenBuf, payload.Length);
        WriteRing(data, _dataCapacity, oldIdx, lenBuf);

        int payloadPos = (oldIdx + 4) % _dataCapacity;
        WriteRing(data, _dataCapacity, payloadPos, payload);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteRing(byte* data, int capacity, int offset, ReadOnlySpan<byte> src)
    {
        int remaining = src.Length;
        int srcOff    = 0;
        while (remaining > 0)
        {
            int chunk = Math.Min(remaining, capacity - offset);
            src.Slice(srcOff, chunk).CopyTo(new Span<byte>(data + offset, chunk));
            offset    = (offset + chunk) % capacity;
            srcOff   += chunk;
            remaining -= chunk;
        }
    }

    /// <inheritdoc/>
    public override void Flush() { }

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _view.SafeMemoryMappedViewHandle.ReleasePointer();
        _view.Dispose();
        _mmf.Dispose();
    }
}
