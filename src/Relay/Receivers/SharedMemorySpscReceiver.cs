using System;
using System.Buffers.Binary;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Relay.Receivers;

/// <summary>
/// Hot-path SPSC shared-memory receiver. Reads frames from the ring written by
/// <see cref="Relay.Sinks.SharedMemorySpscSink"/>.
/// <para>
/// Ring layout (matches Log2 SharedMemorySink protocol exactly):
/// 128-byte header — CL0 [0..63]: Magic(4) + DataCapacity(4) + WriteIndex(4) + pad(52);
/// CL1 [64..127]: ReadIndex(4) + pad(60).
/// Data area at [128..]: <c>[4B BE payload_length][payload bytes]</c>, ring-wrapped.
/// </para>
/// <para>
/// <c>WriteIndex</c> is read via <see cref="Volatile.Read"/>; <c>ReadIndex</c> is advanced
/// via <see cref="Volatile.Write"/> after each consumed frame so the producer can track backpressure.
/// </para>
/// <para>Platform: Named MMFs require Windows.</para>
/// </summary>
/// <typeparam name="TState">Caller state threaded into callback — avoids closure allocation.</typeparam>
[SupportedOSPlatform("windows")]
public sealed unsafe class SharedMemorySpscReceiver<TState> : PacketReceiver
{
    private const int HEADER_SIZE   = 128;
    private const int DATA_CAP_OFF  = 4;
    private const int WRITE_IDX_OFF = 8;
    private const int READ_IDX_OFF  = 64;

    private readonly MemoryMappedFile         _mmf;
    private readonly MemoryMappedViewAccessor _view;
    private readonly byte*                    _ptr;
    private readonly int                      _dataCapacity;
    private readonly TState                   _state;
    private readonly PacketCallback<TState>   _callback;
    private readonly byte[]                   _frameBuffer; // POH-pinned; max payload size
    private          int                      _readIndex;
    private          bool                     _disposed;

    /// <param name="name">Named MMF identifier — must match the <see cref="Relay.Sinks.SharedMemorySpscSink"/> producer.</param>
    /// <param name="state">Caller state passed to <paramref name="callback"/> on each frame.</param>
    /// <param name="callback">Invoked synchronously for each received frame. Must not store the span.</param>
    /// <param name="next">Optional: dispatched frame also forwarded to this sink after the callback.</param>
    /// <param name="maxFrameSize">Maximum single payload size in bytes (default 64 KiB). Must fit within the ring's data area.</param>
    public SharedMemorySpscReceiver(
        string                 name,
        TState                 state,
        PacketCallback<TState> callback,
        PacketSink?            next         = null,
        int                    maxFrameSize = 65_536)
    {
        _state       = state;
        _callback    = callback;
        Next         = next;
        _frameBuffer = GC.AllocateUninitializedArray<byte>(maxFrameSize, pinned: true);
        // Pre-touch pages per fTL pattern — eliminates soft page faults on first receive
        for (int i = 0; i < maxFrameSize; i += 4096) _frameBuffer[i] = 0;

        _mmf  = MemoryMappedFile.OpenExisting(name);
        _view = _mmf.CreateViewAccessor(0, 0); // 0 = map entire file

        byte* ptr = null;
        _view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        _ptr = ptr;

        // DataCapacity is stored by the sink at header offset 4
        _dataCapacity = *(int*)(_ptr + DATA_CAP_OFF);

        // Sync read index from MMF — allows reconnect without position loss
        _readIndex = Volatile.Read(ref *(int*)(_ptr + READ_IDX_OFF));
    }

    /// <summary>
    /// Non-blocking poll. Reads at most one frame per call.
    /// Returns <c>true</c> when a frame was received and the callback invoked.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Poll()
    {
        int writeIndex = Volatile.Read(ref *(int*)(_ptr + WRITE_IDX_OFF));
        if (_readIndex == writeIndex) return false;

        byte* data = _ptr + HEADER_SIZE;

        // Read 4B BE length prefix (ring-wrap safe)
        Span<byte> lenBuf = stackalloc byte[4];
        ReadRing(data, _dataCapacity, _readIndex, lenBuf);
        int frameLen = BinaryPrimitives.ReadInt32BigEndian(lenBuf);

        if (frameLen <= 0 || frameLen > _frameBuffer.Length) return false;

        // Read payload (ring-wrap safe)
        int payloadStart = (_readIndex + 4) % _dataCapacity;
        var payload      = _frameBuffer.AsSpan(0, frameLen);
        ReadRing(data, _dataCapacity, payloadStart, payload);

        // Advance and publish read index so producer can detect backpressure
        _readIndex = (_readIndex + 4 + frameLen) % _dataCapacity;
        Volatile.Write(ref *(int*)(_ptr + READ_IDX_OFF), _readIndex);

        _callback(_state, payload);
        Next?.Enqueue(payload);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReadRing(byte* data, int capacity, int offset, Span<byte> dst)
    {
        int remaining = dst.Length;
        int dstOff    = 0;
        while (remaining > 0)
        {
            int chunk = Math.Min(remaining, capacity - offset);
            new ReadOnlySpan<byte>(data + offset, chunk).CopyTo(dst.Slice(dstOff, chunk));
            offset    = (offset + chunk) % capacity;
            dstOff   += chunk;
            remaining -= chunk;
        }
    }

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
