using System;
using System.Buffers.Binary;
using System.IO.Pipes;
using System.Runtime.CompilerServices;

namespace Relay.Receivers;

/// <summary>
/// Management-plane named-pipe receiver. Compatible with <see cref="Relay.Sinks.NamedPipeSink"/>.
/// Wire format: <c>[4B BE payload_length][payload bytes]</c>.
/// <para>
/// Call <see cref="WaitForConnection"/> once from the management thread before the
/// <see cref="Poll"/> loop. <see cref="Poll"/> blocks until a complete frame is available
/// (management-plane only — blocking on a dedicated management thread is acceptable).
/// </para>
/// </summary>
/// <remarks>// [management-plane] — not for coordination hot-path use.</remarks>
/// <typeparam name="TState">Caller state threaded into callback — avoids closure allocation.</typeparam>
public sealed class NamedPipeReceiver<TState> : PacketReceiver
{
    private readonly NamedPipeServerStream   _pipe;
    private readonly TState                  _state;
    private readonly PacketCallback<TState>  _callback;
    private readonly byte[]                  _header;  // 4-byte length prefix; POH-pinned
    private readonly byte[]                  _buffer;  // payload buffer; POH-pinned, pre-touched

    /// <param name="pipeName">Named pipe identifier — must match the <see cref="Relay.Sinks.NamedPipeSink"/> producer.</param>
    /// <param name="state">Caller state passed to <paramref name="callback"/> on each frame.</param>
    /// <param name="callback">Invoked synchronously for each received frame. Must not store the span.</param>
    /// <param name="next">Optional: dispatched frame also forwarded to this sink after the callback.</param>
    /// <param name="bufferSize">Payload buffer size in bytes (default 64 KiB).</param>
    public NamedPipeReceiver(
        string                 pipeName,
        TState                 state,
        PacketCallback<TState> callback,
        PacketSink?            next       = null,
        int                    bufferSize = 65_536)
    {
        _state    = state;
        _callback = callback;
        Next      = next;
        _header   = GC.AllocateUninitializedArray<byte>(4, pinned: true);
        _buffer   = GC.AllocateUninitializedArray<byte>(bufferSize, pinned: true);
        for (int i = 0; i < bufferSize; i += 4096) _buffer[i] = 0;

        _pipe = new NamedPipeServerStream(
            pipeName,
            PipeDirection.In,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte);
    }

    /// <summary>
    /// Blocking wait for the <see cref="Relay.Sinks.NamedPipeSink"/> client to connect.
    /// Call once from the management thread before entering the <see cref="Poll"/> loop.
    /// </summary>
    /// <remarks>// [management-plane]</remarks>
    public void WaitForConnection() => _pipe.WaitForConnection();

    /// <summary>
    /// Reads one length-prefixed frame synchronously (management-plane — blocking acceptable).
    /// Returns <c>false</c> when the pipe is not connected or the client disconnects mid-frame.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Poll()
    {
        if (!_pipe.IsConnected) return false;

        // Header: [4B BE payload_length]
        if (!ReadExact(_header.AsSpan())) return false;

        int frameLen = BinaryPrimitives.ReadInt32BigEndian(_header);
        if (frameLen <= 0 || frameLen > _buffer.Length) return false;

        // Payload
        var payload = _buffer.AsSpan(0, frameLen);
        if (!ReadExact(payload)) return false;

        _callback(_state, payload);
        Next?.Enqueue(payload);
        return true;
    }

    private bool ReadExact(Span<byte> target)
    {
        int offset = 0;
        while (offset < target.Length)
        {
            int read = _pipe.Read(target[offset..]);
            if (read == 0) return false;
            offset += read;
        }
        return true;
    }

    /// <inheritdoc/>
    public override void Dispose() => _pipe.Dispose();
}
