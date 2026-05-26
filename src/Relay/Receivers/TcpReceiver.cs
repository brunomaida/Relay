using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Relay.Receivers;

/// <summary>
/// Management-plane TCP receiver. Accepts exactly one connection per session via blocking
/// <see cref="Accept"/>; subsequent <see cref="Poll"/> calls are non-blocking.
/// Wire format: <c>[frameLen:4 BE][frame:N]</c> — matches Relay's <c>TcpSink</c> / <c>NamedPipeSink</c>.
/// </summary>
/// <remarks>// [management-plane] — blocking Accept; non-blocking Poll after connection.</remarks>
/// <typeparam name="TState">
/// Caller state threaded into <paramref name="callback"/> on each frame — avoids closure allocation.
/// </typeparam>
public sealed class TcpReceiver<TState> : PacketReceiver
{
    private readonly TcpListener            _listener;
    private readonly TState                 _state;
    private readonly PacketCallback<TState> _callback;
    private readonly byte[]                 _buffer;          // POH-pinned; pre-touched in ctor
    private readonly int                    _kernelBufferSize;
    private TcpClient?     _client;
    private NetworkStream? _stream;

    /// <param name="local">Local bind endpoint; port 0 = OS-assigned ephemeral port.</param>
    /// <param name="state">Caller state passed to <paramref name="callback"/> on each frame.</param>
    /// <param name="callback">Invoked synchronously for each received frame. Must not store the span.</param>
    /// <param name="next">Optional: dispatched frame also forwarded to this sink.</param>
    /// <param name="bufferSize">Frame buffer size (default 64 KiB — max coordination frame).</param>
    /// <param name="kernelBufferSize">Kernel RX buffer size in bytes (default 1 MB).</param>
    public TcpReceiver(
        IPEndPoint             local,
        TState                 state,
        PacketCallback<TState> callback,
        PacketSink?            next             = null,
        int                    bufferSize       = 65_536,
        int                    kernelBufferSize = 1 << 20)
    {
        _state            = state;
        _callback         = callback;
        _kernelBufferSize = kernelBufferSize;
        Next              = next;
        _buffer           = GC.AllocateUninitializedArray<byte>(bufferSize, pinned: true);
        // Pre-touch pages per fTL TcpGateway pattern — eliminates soft page faults on first receive
        for (int i = 0; i < bufferSize; i += 4096) _buffer[i] = 0;

        _listener = new TcpListener(local);
        _listener.Start(backlog: 1);
    }

    /// <summary>Actual local endpoint after OS bind — advertise to senders.</summary>
    public IPEndPoint LocalEndPoint => (IPEndPoint)_listener.LocalEndpoint;

    /// <summary>
    /// Blocking accept — waits for the sender to connect. Call once from the management thread
    /// before entering the <see cref="Poll"/> loop.
    /// </summary>
    /// <remarks>// [management-plane]</remarks>
    public void Accept()
    {
        _client = _listener.AcceptTcpClient();
        _client.NoDelay           = true;
        _client.ReceiveBufferSize = _kernelBufferSize;
        _stream = _client.GetStream();
    }

    /// <summary>
    /// Non-blocking poll. Reads at most one length-prefixed frame per call.
    /// Returns <c>false</c> when no frame is available or <see cref="Accept"/> has not been called.
    /// </summary>
    /// <remarks>
    /// <see cref="NetworkStream.DataAvailable"/> reports kernel buffer state (not full-frame
    /// availability); <see cref="ReadExact"/> handles TCP segmentation correctly.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Poll()
    {
        if (_stream is null || !_stream.DataAvailable) return false;

        Span<byte> header = stackalloc byte[4];
        if (!ReadExact(header)) return false;

        int frameLen = BinaryPrimitives.ReadInt32BigEndian(header);
        if (frameLen <= 0 || frameLen > _buffer.Length)
        {
            // Header consumed but payload was never delivered — the wire is now mid-frame.
            // Tear down so the next Poll returns false; caller must restart the session.
            TearDown();
            throw new InvalidDataException(
                $"TcpReceiver: invalid frame length {frameLen} (buffer={_buffer.Length}). Connection torn down.");
        }

        var payload = _buffer.AsSpan(0, frameLen);
        if (!ReadExact(payload)) return false;

        _callback(_state, payload);
        Next?.Enqueue(payload);
        return true;
    }

    private void TearDown()
    {
        _stream?.Dispose();
        _stream = null;
        _client?.Dispose();
        _client = null;
    }

    private bool ReadExact(Span<byte> target)
    {
        int offset = 0;
        while (offset < target.Length)
        {
            int read = _stream!.Read(target[offset..]);
            if (read == 0) return false;
            offset += read;
        }
        return true;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();
        _listener.Stop();
    }
}
