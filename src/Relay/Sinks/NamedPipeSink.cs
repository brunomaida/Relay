using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary>
/// SpscQueueSink that delivers payloads to a NamedPipe server with 4-byte BE length prefix.
/// Compatible with Input2Log NamedPipe receiver. Acts as the client side; expects an existing
/// server (e.g., Input2Log NamedPipeInput) to be listening.
/// </summary>
public sealed class NamedPipeSink : SpscQueueSink
{
    private const int MinBackoffMs = 1_000;
    private const int MaxBackoffMs = 30_000;

    private readonly string _pipeName;
    private readonly byte[] _sendBuffer;

    private NamedPipeClientStream? _client;
    private int                    _filled;
    private int                    _backoffMs      = MinBackoffMs;
    private long                   _nextRetryTicks;

    public NamedPipeSink(
        string pipeName,
        int    sendBufferCapacity = 65_536,
        int    ringCapacity       = 65_536,
        int    flushIntervalMs    = 100)
        : base(ringCapacity, flushIntervalMs, $"pipe-{pipeName}")
    {
        _pipeName   = pipeName;
        _sendBuffer = GC.AllocateArray<byte>(sendBufferCapacity, pinned: true);
        ConnectClient();
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
        if (_filled == 0 || _client is null || !_client.IsConnected) return;
        try
        {
            _client.Write(_sendBuffer.AsSpan(0, _filled));
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
        ConnectClient();
    }

    protected override void DisposeBackend()
    {
        try { _client?.Dispose(); } catch { }
        _client = null;
    }

    private void ConnectClient()
    {
        try
        {
            _client?.Dispose();
            _client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
            _client.Connect(timeout: 100);
            _healthy   = true;
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _client?.Dispose();
            _client         = null;
            _healthy        = false;
            _backoffMs      = Math.Min(_backoffMs * 2, MaxBackoffMs);
            _nextRetryTicks = HfClock.NowTicks + (long)_backoffMs * (Stopwatch.Frequency / 1_000);
        }
    }
}
