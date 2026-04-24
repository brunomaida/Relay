using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Relay.Internal;

namespace Relay.Pipes;

/// <summary>
/// Pipe that sends blittable items over a TCP socket (<c>sizeof(T)</c> bytes per item).
/// Uses a POH-pinned send buffer. IOException sets <c>_healthy = false</c>;
/// <see cref="TryRecoverBackend"/> reconnects with exponential backoff 1s → 30s.
/// </summary>
public sealed class TcpPipe<T> : SpscQueuePipe<T> where T : unmanaged
{
    private const int DefaultRingCapacity  = 16_384;
    private const int DefaultFlushInterval = 250;
    private const int RetryMaxDelayMs      = 30_000;

    private static readonly long TicksPerMs = Stopwatch.Frequency / 1_000;
    private static readonly int  EntrySize  = Unsafe.SizeOf<T>();

    private readonly string _host;
    private readonly int    _port;
    private readonly byte[] _sendBuffer;

    private TcpClient?    _client;
    private NetworkStream? _stream;
    private int            _bufferPos;
    private int            _retryDelayMs = 1_000;
    private long           _retryAfterTicks;

    public TcpPipe(
        string host,
        int    port,
        int    ringCapacity  = DefaultRingCapacity,
        int    flushInterval = DefaultFlushInterval)
        : base(ringCapacity, flushInterval, "tcp")
    {
        _host       = host ?? throw new ArgumentNullException(nameof(host));
        _port       = port;
        _sendBuffer = GC.AllocateArray<byte>(4096 * EntrySize, pinned: true);
        TryConnect();
    }

    /// <inheritdoc/>
    protected override bool PropagateAfterAccept => false;

    protected override unsafe void WriteToBackend(in T item)
    {
        if (_bufferPos + EntrySize > _sendBuffer.Length)
            FlushBuffer();

        Unsafe.CopyBlockUnaligned(
            ref _sendBuffer[_bufferPos],
            ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in item)),
            (uint)EntrySize);

        _bufferPos += EntrySize;
    }

    protected override void FlushBackend()
    {
        if (_bufferPos > 0) FlushBuffer();
    }

    protected override void TryRecoverBackend()
    {
        if (_healthy || HfClock.NowTicks < _retryAfterTicks) return;

        try
        {
            _stream?.Dispose();
            _client?.Dispose();
            TryConnect();
            if (_bufferPos > 0) { _stream!.Write(_sendBuffer.AsSpan(0, _bufferPos)); _bufferPos = 0; }
            _healthy      = true;
            _retryDelayMs = 1_000;
        }
        catch (Exception)
        {
            _retryDelayMs    = Math.Min(_retryDelayMs * 2, RetryMaxDelayMs);
            _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
        }
    }

    protected override void DisposeBackend()
    {
        try { _stream?.Dispose(); _client?.Dispose(); } catch { /* best-effort */ }
    }

    private void FlushBuffer()
    {
        try
        {
            _stream!.Write(_sendBuffer.AsSpan(0, _bufferPos));
            _bufferPos = 0;
        }
        catch (Exception)
        {
            _healthy         = false;
            _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
        }
    }

    private void TryConnect()
    {
        _client         = new TcpClient { NoDelay = true };
        _client.Connect(_host, _port);
        _stream         = _client.GetStream();
    }
}
