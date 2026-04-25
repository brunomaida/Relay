using System;
using System.Buffers.Binary;
using System.Diagnostics;
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
public sealed class UnixSocketByteSink : SpscQueueSink
{
    private const int MinBackoffMs = 1_000;
    private const int MaxBackoffMs = 30_000;

    private readonly string _path;
    private readonly byte[] _sendBuffer;

    private Socket? _socket;
    private int     _filled;
    private int     _backoffMs      = MinBackoffMs;
    private long    _nextRetryTicks;

    public UnixSocketByteSink(
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
