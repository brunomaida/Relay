using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Versioning;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary>
/// SpscQueueSink that delivers payloads to a Unix domain socket with 4-byte BE length prefix.
/// Acts as client; expects an existing server (e.g., Input2Log UnixSocketInput).
/// </summary>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class UnixSocketSink : SpscQueueSink
{
    private const int MinBackoffMs = 1_000;
    private const int MaxBackoffMs = 30_000;

    private readonly string _path;
    private readonly byte[] _sendBuffer;

    private Socket? _socket;
    private int     _filled;
    private int     _backoffMs      = MinBackoffMs;
    private long    _nextRetryTicks;

    public UnixSocketSink(
        string path,
        int    sendBufferCapacity = 65_536,
        int    ringCapacity       = 65_536,
        int    flushIntervalMs    = 100)
        : base(ringCapacity, flushIntervalMs, $"unix-{System.IO.Path.GetFileName(path)}")
    {
        _path       = path;
        _sendBuffer = GC.AllocateArray<byte>(sendBufferCapacity, pinned: true);
        Connect();
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        int needed = 4 + payload.Length;

        // Framed payload larger than the send buffer — bypass batching and send directly.
        if (needed > _sendBuffer.Length)
        {
            if (_filled > 0) FlushBackend();
            if (_socket is null || !_healthy) return;
            Span<byte> hdr = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(hdr, (uint)payload.Length);
            // Header and payload are sent as a logical unit: if either send fails or only
            // partially succeeds mid-way, we set _healthy=false so recovery reconnects
            // (re-syncing the peer's frame reader at the next connection boundary).
            try
            {
                SendAll(hdr);
                SendAll(payload);
                _backoffMs = MinBackoffMs;
            }
            catch
            {
                _filled  = 0;
                _healthy = false;
            }
            return;
        }

        if (_filled + needed > _sendBuffer.Length) FlushBackend();

        BinaryPrimitives.WriteUInt32BigEndian(_sendBuffer.AsSpan(_filled), (uint)payload.Length);
        _filled += 4;
        payload.CopyTo(_sendBuffer.AsSpan(_filled));
        _filled += payload.Length;
    }

    protected override void FlushBackend()
    {
        if (_filled == 0 || _socket is null) return;
        try
        {
            SendAll(_sendBuffer.AsSpan(0, _filled));
            _filled    = 0;
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _filled  = 0;
            _healthy = false;
        }
    }

    /// <summary>Sends all bytes in <paramref name="buffer"/>, looping on partial sends.</summary>
    /// <exception cref="IOException">Thrown when the remote end closes the connection.</exception>
    private void SendAll(ReadOnlySpan<byte> buffer)
    {
        int total = 0;
        while (total < buffer.Length)
        {
            int sent = _socket!.Send(buffer.Slice(total));
            if (sent <= 0) throw new IOException("Send returned 0; peer closed");
            total += sent;
        }
    }

    protected override void TryRecoverBackend()
    {
        if (_healthy) return;
        if (HfClock.NowTicks < _nextRetryTicks) return;
        Connect();
    }

    protected override void DisposeBackend()
    {
        try { _socket?.Shutdown(SocketShutdown.Both); } catch { }
        _socket?.Dispose();
        _socket = null;
    }

    private void Connect()
    {
        try
        {
            _socket?.Dispose();
            _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            _socket.Connect(new UnixDomainSocketEndPoint(_path));
            _healthy   = true;
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _socket?.Dispose();
            _socket         = null;
            _healthy        = false;
            _backoffMs      = Math.Min(_backoffMs * 2, MaxBackoffMs);
            _nextRetryTicks = HfClock.NowTicks + (long)_backoffMs * (Stopwatch.Frequency / 1_000);
        }
    }
}
