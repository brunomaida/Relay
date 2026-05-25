using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary>
/// Sink that writes blittable items as raw binary to a <see cref="FileStream"/>.
/// Each item occupies exactly <c>sizeof(T)</c> bytes on disk.
/// Uses a POH-pinned write buffer; flushes on the flush-interval cadence.
/// IOException sets <c>_healthy = false</c>; <see cref="TryRecoverBackend"/> reopens the stream
/// with exponential backoff 1s → <see cref="RetryMaxDelayMs"/>.
/// </summary>
/// <remarks>
/// <para>Thread safety: <c>single-producer</c> — inherits <see cref="SpscQueueSink{T}"/> topology.
/// Only one thread may call <c>Enqueue</c> at a time. File I/O runs on the internally-owned
/// consumer thread via <c>WriteToBackend</c> and <c>FlushBackend</c>; do not call those methods
/// directly. Do NOT wrap <c>Enqueue</c> in an external lock — this sink uses volatile/Interlocked
/// primitives; adding a monitor costs ~1000 cycles per call with no benefit.</para>
/// </remarks>
public sealed class FileStreamSink<T> : SpscQueueSink<T> where T : unmanaged
{
    private const int DefaultRingCapacity  = 524_288;
    private const int DefaultFlushInterval = 250;

    public const int RetryMaxDelayMs = 60_000;

    private static readonly long TicksPerMs = Stopwatch.Frequency / 1_000;
    private static readonly int  EntrySize  = Unsafe.SizeOf<T>();

    private readonly string _path;
    private readonly byte[] _writeBuffer;

    private FileStream? _stream;
    private int         _bufferPos;
    private int         _retryDelayMs = 1_000;
    private long        _retryAfterTicks;

    public FileStreamSink(
        string path,
        int    ringCapacity  = DefaultRingCapacity,
        int    flushInterval = DefaultFlushInterval)
        : base(ringCapacity, flushInterval, "file")
    {
        _path        = path ?? throw new ArgumentNullException(nameof(path));
        _writeBuffer = GC.AllocateArray<byte>(4096 * EntrySize, pinned: true);
        OpenStream();
    }

    protected override unsafe void WriteToBackend(in T item)
    {
        if (_bufferPos + EntrySize > _writeBuffer.Length)
            FlushBuffer();

        Unsafe.CopyBlockUnaligned(
            ref _writeBuffer[_bufferPos],
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
            OpenStream();
            if (_bufferPos > 0) { _stream!.Write(_writeBuffer.AsSpan(0, _bufferPos)); _bufferPos = 0; }
            _healthy      = true;
            _retryDelayMs = 1_000;
        }
        catch (Exception)
        {
            _retryDelayMs = Math.Min(_retryDelayMs * 2, RetryMaxDelayMs);
            _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
        }
    }

    protected override void DisposeBackend()
    {
        try { _stream?.Dispose(); } catch { /* best-effort */ }
    }

    private void FlushBuffer()
    {
        try
        {
            _stream!.Write(_writeBuffer.AsSpan(0, _bufferPos));
            _bufferPos = 0;
        }
        catch (IOException)
        {
            _healthy         = false;
            _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
        }
    }

    private void OpenStream() =>
        _stream = new FileStream(_path, FileMode.Append, FileAccess.Write,
            FileShare.Read, bufferSize: 0, FileOptions.SequentialScan);
}
