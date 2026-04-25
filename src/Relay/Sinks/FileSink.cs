using System;
using System.Diagnostics;
using System.IO;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary>
/// <see cref="SpscQueueSink"/> that accumulates byte payloads in a POH write buffer and flushes
/// to a <see cref="FileStream"/> on the flush interval. Supports an optional header written once
/// when the file is first created (stream position == 0). No rotation.
/// </summary>
public sealed class FileSink : SpscQueueSink
{
    private const int MinBackoffMs = 1_000;
    private const int MaxBackoffMs = 60_000;

    private readonly string               _path;
    private readonly byte[]               _writeBuffer;   // POH pinned
    private readonly ReadOnlyMemory<byte> _header;

    private FileStream? _stream;
    private int         _filled;
    private int         _backoffMs      = MinBackoffMs;
    private long        _nextRetryTicks;

    /// <param name="path">Destination file path.</param>
    /// <param name="writeBufferCapacity">POH write buffer size in bytes. Default 64 KB.</param>
    /// <param name="ringCapacity">SPSC ring size in bytes. Must be power of two. Default 64 KB.</param>
    /// <param name="flushIntervalMs">Max ms between forced flushes. Default 200 ms.</param>
    /// <param name="header">Optional bytes written once when the file is empty. Caller defines content.</param>
    public FileSink(
        string                path,
        int                   writeBufferCapacity = 65_536,
        int                   ringCapacity        = 65_536,
        int                   flushIntervalMs     = 200,
        ReadOnlyMemory<byte>? header              = null)
        : base(ringCapacity, flushIntervalMs, $"file-{Path.GetFileName(path)}")
    {
        _path        = path;
        _writeBuffer = GC.AllocateArray<byte>(writeBufferCapacity, pinned: true);
        _header      = header ?? ReadOnlyMemory<byte>.Empty;
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        if (_stream is null && !TryOpenStream()) return;

        if (_filled + payload.Length > _writeBuffer.Length)
            FlushToStream();

        payload.CopyTo(_writeBuffer.AsSpan(_filled));
        _filled += payload.Length;
    }

    protected override void FlushBackend()
    {
        if (_stream is null) return;
        FlushToStream();
    }

    protected override void TryRecoverBackend()
    {
        if (_healthy) return;
        if (HfClock.NowTicks < _nextRetryTicks) return;
        TryOpenStream();
    }

    protected override void DisposeBackend()
    {
        FlushToStream();
        _stream?.Dispose();
        _stream = null;
    }

    private void FlushToStream()
    {
        if (_stream is null) return;
        try
        {
            if (_filled > 0)
            {
                _stream.Write(_writeBuffer.AsSpan(0, _filled));
                _filled = 0;
            }
            _stream.Flush();
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _filled = 0;
            _healthy = false;
            _stream?.Dispose();
            _stream = null;
        }
    }

    private bool TryOpenStream()
    {
        try
        {
            _stream = new FileStream(_path, FileMode.Append, FileAccess.Write,
                                     FileShare.Read, bufferSize: 1, useAsync: false);
            if (_header.Length > 0 && _stream.Position == 0)
                _stream.Write(_header.Span);

            _healthy   = true;
            _backoffMs = MinBackoffMs;
            return true;
        }
        catch
        {
            _stream?.Dispose();
            _stream         = null;
            _healthy        = false;
            _backoffMs      = Math.Min(_backoffMs * 2, MaxBackoffMs);
            _nextRetryTicks = HfClock.NowTicks + (long)_backoffMs * (Stopwatch.Frequency / 1_000);
            return false;
        }
    }
}
