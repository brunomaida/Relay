using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary>
/// SpscQueueSink that rotates files by size and/or date. Optional header written once per
/// new file. Cleanup retention by max-file count.
/// </summary>
public sealed class RotatingFileSink : SpscQueueSink
{
    private const int MinBackoffMs = 1_000;
    private const int MaxBackoffMs = 60_000;

    private readonly string               _dir;
    private readonly string               _prefix;
    private readonly long                 _maxBytes;
    private readonly int                  _maxFiles;
    private readonly byte[]               _writeBuffer;
    private readonly ReadOnlyMemory<byte> _header;

    private FileStream? _stream;
    private int         _filled;
    private long        _currentFileBytes;
    private int         _seq;
    private DateTime    _currentDay;
    private long        _nextDayBoundaryTicks;          // HfClock ticks at the next UTC midnight
    private int         _backoffMs      = MinBackoffMs;
    private long        _nextRetryTicks;

    public RotatingFileSink(
        string                dir,
        string                filenamePrefix,
        long                  maxBytes            = 100 * 1024 * 1024,
        int                   maxFiles            = 10,
        int                   writeBufferCapacity = 65_536,
        int                   ringCapacity        = 65_536,
        int                   flushIntervalMs     = 200,
        ReadOnlyMemory<byte>? header              = null)
        : base(ringCapacity, flushIntervalMs, $"file-{filenamePrefix}")
    {
        _dir         = dir;
        _prefix      = filenamePrefix;
        _maxBytes    = maxBytes;
        _maxFiles    = maxFiles;
        _writeBuffer = GC.AllocateArray<byte>(writeBufferCapacity, pinned: true);
        _header      = header ?? ReadOnlyMemory<byte>.Empty;
        _currentDay           = DateTime.UtcNow.Date;
        _nextDayBoundaryTicks = ComputeNextDayBoundaryTicks();
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        if (_stream is null && !TryOpenStream()) return;

        if (ShouldRotate(payload.Length)) RotateNow();

        if (_filled + payload.Length > _writeBuffer.Length) FlushToStream();

        payload.CopyTo(_writeBuffer.AsSpan(_filled));
        _filled += payload.Length;
        _currentFileBytes += payload.Length;
    }

    protected override void FlushBackend() => FlushToStream();

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

    private bool ShouldRotate(int incomingBytes)
    {
        if (_currentFileBytes + incomingBytes > _maxBytes) return true;
        if (HfClock.NowTicks >= _nextDayBoundaryTicks) return true;
        return false;
    }

    private void RotateNow()
    {
        FlushToStream();
        _stream?.Dispose();
        _stream = null;
        _seq++;
        _currentDay           = DateTime.UtcNow.Date;
        _nextDayBoundaryTicks = ComputeNextDayBoundaryTicks();
        _currentFileBytes     = 0;
        TryOpenStream();
        Cleanup();
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
            _filled  = 0;
            _healthy = false;
            _stream?.Dispose();
            _stream = null;
        }
    }

    private bool TryOpenStream()
    {
        try
        {
            string filename = $"{_prefix}-{_currentDay:yyyyMMdd}.{_seq:D4}.log";
            string path     = Path.Combine(_dir, filename);

            _stream = new FileStream(path, FileMode.Append, FileAccess.Write,
                                     FileShare.Read, bufferSize: 1, useAsync: false);
            if (_header.Length > 0 && _stream.Position == 0)
            {
                _stream.Write(_header.Span);
                _currentFileBytes = _header.Length;
            }
            else
            {
                _currentFileBytes = _stream.Position;
            }
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

    private void Cleanup()
    {
        try
        {
            var files = Directory.GetFiles(_dir, $"{_prefix}-*.log")
                                 .OrderByDescending(f => f)
                                 .Skip(_maxFiles)
                                 .ToArray();
            foreach (var f in files) File.Delete(f);
        }
        catch { /* best-effort */ }
    }

    private static long ComputeNextDayBoundaryTicks()
    {
        var  now          = DateTime.UtcNow;
        var  nextMidnight = now.Date.AddDays(1);
        long msUntil      = (long)(nextMidnight - now).TotalMilliseconds;
        return HfClock.NowTicks + msUntil * (Stopwatch.Frequency / 1_000);
    }

    /// <summary>
    /// Test-only hook: forces the next-day-boundary tick threshold. Visible to
    /// <c>Relay.Tests</c> via <c>InternalsVisibleTo</c>. Production callers must use
    /// <see cref="ComputeNextDayBoundaryTicks"/> (resampled inside <see cref="RotateNow"/>).
    /// </summary>
    internal void SetDayBoundaryForTest(long ticks) => _nextDayBoundaryTicks = ticks;

    /// <summary>
    /// Benchmark-only accessor: invokes <see cref="WriteToBackend"/> directly so BDN can
    /// isolate the per-record consumer-thread cost (the <c>ShouldRotate</c> predicate)
    /// without ring publish + consumer-loop overhead. Visible to <c>Relay.Benchmarks</c>
    /// via <c>InternalsVisibleTo</c>; never call from production code.
    /// </summary>
    internal void BenchInvokeWriteToBackend(ReadOnlySpan<byte> payload) => WriteToBackend(payload);
}
