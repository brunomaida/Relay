using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net.Sockets;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary>
/// <see cref="SpscQueueSink"/> that delivers byte payloads over TCP with a 4-byte Big-Endian
/// length prefix. BE framing is wire-compatible with Input2Log TCP, NamedPipe, UnixSocket,
/// and SharedMemory receivers (<c>BinaryPrimitives.ReadInt32BigEndian</c>).
/// </summary>
public sealed class TcpSink : SpscQueueSink
{
    private const int MinBackoffMs = 1_000;
    private const int MaxBackoffMs = 30_000;

    private readonly string _host;
    private readonly int    _port;
    private readonly byte[] _sendBuffer;  // POH pinned

    private Socket? _socket;
    private int     _filled;
    private int     _backoffMs      = MinBackoffMs;
    private long    _nextRetryTicks;

    /// <param name="host">TCP server hostname or IP.</param>
    /// <param name="port">TCP server port.</param>
    /// <param name="sendBufferCapacity">POH send buffer size in bytes. Default 64 KB.</param>
    /// <param name="ringCapacity">SPSC ring size in bytes. Must be power of two. Default 64 KB.</param>
    /// <param name="flushIntervalMs">Max ms between forced flushes. Default 100 ms.</param>
    public TcpSink(
        string host,
        int    port,
        int    sendBufferCapacity = 65_536,
        int    ringCapacity       = 65_536,
        int    flushIntervalMs    = 100)
        : base(ringCapacity, flushIntervalMs, $"tcp-{host}:{port}")
    {
        _host       = host;
        _port       = port;
        _sendBuffer = GC.AllocateArray<byte>(sendBufferCapacity, pinned: true);
        ConnectSocket();
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        // 4B Big-Endian length prefix + payload. BE matches Input2Log TCP/NamedPipe/
        // UnixSocket/SharedMemory receivers (BinaryPrimitives.ReadInt32BigEndian).
        int needed = 4 + payload.Length;
        if (_filled + needed > _sendBuffer.Length)
            FlushBackend();

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
            _socket.Send(_sendBuffer.AsSpan(0, _filled));
            _filled    = 0;
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _filled  = 0;
            _healthy = false;
        }
    }

    protected override void TryRecoverBackend()
    {
        if (_healthy) return;
        if (HfClock.NowTicks < _nextRetryTicks) return;
        ConnectSocket();
    }

    protected override void DisposeBackend()
    {
        try { _socket?.Shutdown(SocketShutdown.Both); } catch { }
        _socket?.Dispose();
        _socket = null;
    }

    private void ConnectSocket()
    {
        try
        {
            _socket?.Dispose();
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            _socket.Connect(_host, _port);
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
