using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary>
/// Sink that sends blittable items over a TCP socket (<c>sizeof(T)</c> bytes per item).
/// Uses a POH-pinned send buffer and a non-blocking <see cref="Socket"/>. Brief backpressure
/// on the send buffer is absorbed by a bounded spin; persistent backpressure marks the pipe
/// unhealthy so new items fall through to <c>Next</c>. <see cref="TryRecoverBackend"/>
/// reconnects with exponential backoff 1s → 30s.
/// </summary>
public sealed class TcpSink<T> : SpscQueueSink<T> where T : unmanaged
{
    private const int DefaultRingCapacity  = 16_384;
    private const int DefaultFlushInterval = 250;
    private const int RetryMaxDelayMs      = 30_000;

    // Bounded spin on WouldBlock: ~64 iterations × Thread.SpinWait(32) ≈ 6μs.
    // Short enough not to starve the ring, long enough to absorb brief TCP backpressure.
    private const int MaxWouldBlockSpins = 64;
    private const int SpinIterations     = 32;

    private static readonly long TicksPerMs = Stopwatch.Frequency / 1_000;
    private static readonly int  EntrySize  = Unsafe.SizeOf<T>();

    private readonly string _host;
    private readonly int    _port;
    private readonly byte[] _sendBuffer;

    private Socket? _socket;
    private int     _bufferPos;
    private int     _retryDelayMs = 1_000;
    private long    _retryAfterTicks;

    public TcpSink(
        string host,
        int    port,
        int    ringCapacity  = DefaultRingCapacity,
        int    flushInterval = DefaultFlushInterval)
        : base(ringCapacity, flushInterval, "tcp")
    {
        _host       = host ?? throw new ArgumentNullException(nameof(host));
        _port       = port;
        _sendBuffer = GC.AllocateArray<byte>(4096 * EntrySize, pinned: true);

        // Startup resilience: if the primary is unreachable at construction time, mark unhealthy
        // and let TryRecoverBackend reconnect with backoff. Chains remain usable from the start.
        try
        {
            TryConnect();
        }
        catch (Exception)
        {
            _healthy         = false;
            _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
        }
    }

    /// <inheritdoc/>
    protected override bool PropagateAfterAccept => false;

    protected override unsafe void WriteToBackend(in T item)
    {
        if (_bufferPos + EntrySize > _sendBuffer.Length)
            FlushBuffer();

        // If send buffer is still full (socket backpressured), drop this item.
        // The ring stops feeding once _healthy=false; future items take the fallback path.
        if (_bufferPos + EntrySize > _sendBuffer.Length)
            return;

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
            _socket?.Dispose();
            TryConnect();
            _healthy      = true;
            _retryDelayMs = 1_000;
            if (_bufferPos > 0) FlushBuffer();
        }
        catch (Exception)
        {
            _retryDelayMs    = Math.Min(_retryDelayMs * 2, RetryMaxDelayMs);
            _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
        }
    }

    protected override void DisposeBackend()
    {
        try { _socket?.Dispose(); } catch { /* best-effort */ }
    }

    private void FlushBuffer()
    {
        if (_socket is null) { MarkUnhealthy(); return; }

        int offset          = 0;
        int stalledAttempts = 0;

        while (offset < _bufferPos)
        {
            int sent;
            SocketError err;
            try
            {
                sent = _socket.Send(
                    _sendBuffer.AsSpan(offset, _bufferPos - offset),
                    SocketFlags.None,
                    out err);
            }
            catch (Exception)
            {
                MarkUnhealthy();
                return;
            }

            if (err == SocketError.Success && sent > 0)
            {
                offset += sent;
                stalledAttempts = 0;
                continue;
            }

            if (err == SocketError.WouldBlock)
            {
                if (++stalledAttempts >= MaxWouldBlockSpins)
                {
                    // Persistent backpressure — preserve unsent bytes, mark unhealthy,
                    // let the fallback chain take new items until recovery.
                    ShiftUnsent(offset);
                    MarkUnhealthy();
                    return;
                }
                Thread.SpinWait(SpinIterations);
                continue;
            }

            MarkUnhealthy();
            return;
        }

        ShiftUnsent(offset);
    }

    private void ShiftUnsent(int offset)
    {
        int remaining = _bufferPos - offset;
        if (remaining > 0 && offset > 0)
            Buffer.BlockCopy(_sendBuffer, offset, _sendBuffer, 0, remaining);
        _bufferPos = remaining;
    }

    private void MarkUnhealthy()
    {
        _healthy         = false;
        _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
    }

    private void TryConnect()
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
        };
        socket.Connect(_host, _port);
        socket.Blocking = false; // non-blocking sends — WouldBlock surfaced via SocketError
        _socket = socket;
    }
}
