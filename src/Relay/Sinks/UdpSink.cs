using System;
using System.Diagnostics;
using System.Net.Sockets;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary><see cref="SpscQueueSink"/> that delivers byte payloads as UDP datagrams.</summary>
public sealed class UdpSink : SpscQueueSink
{
    private const int MinBackoffMs = 1_000;
    private const int MaxBackoffMs = 10_000;

    private readonly string _host;
    private readonly int    _port;
    private readonly int    _maxPayload;

    private Socket? _socket;
    private int     _backoffMs      = MinBackoffMs;
    private long    _nextRetryTicks;

    /// <param name="host">UDP destination hostname or IP.</param>
    /// <param name="port">UDP destination port.</param>
    /// <param name="maxPayload">Max payload bytes; payloads exceeding this mark the sink unhealthy.</param>
    /// <param name="ringCapacity">SPSC ring size in bytes. Must be power of two.</param>
    /// <param name="flushIntervalMs">Max ms between recovery checks.</param>
    public UdpSink(
        string host,
        int    port,
        int    maxPayload      = 65_507,
        int    ringCapacity    = 65_536,
        int    flushIntervalMs = 100)
        : base(ringCapacity, flushIntervalMs, $"udp-{host}:{port}")
    {
        _host       = host;
        _port       = port;
        _maxPayload = maxPayload;
        CreateSocket();
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        if (_socket is null || payload.Length > _maxPayload)
        {
            MarkUnhealthy();
            return;
        }
        try
        {
            _socket.Send(payload);
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            MarkUnhealthy();
        }
    }

    private void MarkUnhealthy()
    {
        _healthy        = false;
        _nextRetryTicks = HfClock.NowTicks + (long)_backoffMs * (Stopwatch.Frequency / 1_000);
    }

    // UDP is fire-and-forget per datagram; no buffer to flush.
    protected override void FlushBackend() { }

    protected override void TryRecoverBackend()
    {
        if (_healthy) return;
        if (HfClock.NowTicks < _nextRetryTicks) return;
        try
        {
            _socket?.Dispose();
            CreateSocket();
            _healthy   = true;
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _socket?.Dispose();
            _socket         = null;
            _backoffMs      = Math.Min(_backoffMs * 2, MaxBackoffMs);
            _nextRetryTicks = HfClock.NowTicks + (long)_backoffMs * (Stopwatch.Frequency / 1_000);
        }
    }

    protected override void DisposeBackend()
    {
        _socket?.Dispose();
        _socket = null;
    }

    private void CreateSocket()
    {
        _socket = new Socket(SocketType.Dgram, ProtocolType.Udp) { DontFragment = true };
        _socket.Connect(_host, _port);
    }
}
