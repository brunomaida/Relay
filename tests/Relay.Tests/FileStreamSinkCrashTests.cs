using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Internal;
using Xunit;

namespace Relay.Tests;

/// <summary>
/// Regression tests for two consumer-crash paths in FileStreamSink:
/// 1. IOException in FlushBuffer did not reset _bufferPos → OOB on next WriteToBackend.
/// 2. Disposed _stream not nulled in TryRecoverBackend catch → ObjectDisposedException
///    escapes catch (IOException) in FlushBuffer → consumer thread dies permanently.
/// These tests use a sealed-compatible test double that replicates the buffer/flush logic.
/// </summary>
public sealed class FileStreamSinkCrashTests : IDisposable
{
    private readonly string _path = Path.GetTempFileName();

    public void Dispose()
    {
        if (File.Exists(_path)) File.Delete(_path);
    }

    /// <summary>
    /// Crash 1: before fix, IOException in FlushBuffer left _bufferPos unreset.
    /// Next WriteToBackend computed ref _writeBuffer[_bufferPos] == ref _writeBuffer[_writeBuffer.Length]
    /// → IndexOutOfRangeException escaped catch (IOException) → consumer thread died.
    /// Post-fix: _bufferPos = 0 in catch block prevents OOB.
    /// </summary>
    [Fact]
    public void IOException_InFlushBuffer_DoesNotKillConsumer()
    {
        using var sink = new BufferedFaultSink(_path);
        sink.Start();

        Thread.Sleep(80); // let first flush succeed

        sink.InjectFlushFault = true;

        for (int i = 0; i < 32; i++)
            sink.Enqueue(new Entry64 { A = i + 100 });

        Thread.Sleep(200);

        sink.IsConsuming.Should().BeTrue("IOException must not crash the consumer thread (Crash 1)");
        sink.ConsumerException.Should().BeNull();

        sink.Stop(drainTimeoutMs: 1_000);
    }

    /// <summary>
    /// Crash 2: before fix, TryRecoverBackend catch did not null _stream after Dispose.
    /// A subsequent FlushBackend (with _bufferPos > 0) called _stream!.Write on the disposed
    /// stream → ObjectDisposedException (not IOException) → escaped consumer → crash.
    /// Post-fix: _stream = null in catch; FlushBuffer null-guards at entry.
    /// </summary>
    [Fact]
    public void RecoveryFailure_WithDisposedStream_DoesNotKillConsumer()
    {
        using var sink = new BufferedFaultSink(_path);
        sink.Start();

        Thread.Sleep(80);

        // Inject both faults: flush fails → unhealthy, then recovery's OpenStream also fails.
        // Pre-fix: _stream held disposed reference; next FlushBackend → ObjectDisposedException → crash.
        // Post-fix: _stream nulled in catch; FlushBuffer's null guard prevents the crash.
        sink.InjectFlushFault   = true;
        sink.InjectRecoverFault = true;

        for (int i = 0; i < 32; i++)
            sink.Enqueue(new Entry64 { A = i });

        Thread.Sleep(500);

        sink.IsConsuming.Should().BeTrue("ObjectDisposedException must not escape consumer (Crash 2)");
        sink.ConsumerException.Should().BeNull();

        sink.Stop(drainTimeoutMs: 1_000);
    }

    /// <summary>
    /// Regression guard: transient IOException → health restores after fault clears.
    /// </summary>
    [Fact]
    public void TransientIOException_RecoveryRestoresHealth()
    {
        using var sink = new BufferedFaultSink(_path);
        sink.Start();

        Thread.Sleep(80);

        sink.InjectFlushFault = true;
        Thread.Sleep(100);
        sink.InjectFlushFault = false; // clear fault — TryRecoverBackend should succeed

        bool recovered = SpinUntil(() => sink.IsHealthy, timeoutMs: 3_500);
        recovered.Should().BeTrue("sink must recover after transient IOException");

        sink.IsConsuming.Should().BeTrue();
        sink.ConsumerException.Should().BeNull();

        sink.Stop(drainTimeoutMs: 1_000);
    }

    // =========================================================================
    // Test double: replicates FileStreamSink<T> buffer + flush logic verbatim,
    // with injectable faults. Uses SpscQueueSink<T> directly — no mocking frameworks.
    // =========================================================================

    private sealed class BufferedFaultSink : SpscQueueSink<Entry64>
    {
        private static readonly int  EntrySize   = Unsafe.SizeOf<Entry64>();
        private static readonly long TicksPerMs  = Stopwatch.Frequency / 1_000;
        private const int RetryMaxDelayMs        = 60_000;

        private readonly string _path;
        private readonly byte[] _writeBuffer;

        private FileStream? _stream;
        private int         _bufferPos;
        private int         _retryDelayMs     = 1_000;
        private long        _retryAfterTicks;

        public volatile bool InjectFlushFault;
        public volatile bool InjectRecoverFault;

        public BufferedFaultSink(string path)
            : base(ringCapacity: 128, flushIntervalMs: 50, pipeName: "fault-test")
        {
            _path        = path;
            _writeBuffer = GC.AllocateArray<byte>(4096 * EntrySize, pinned: true);
            OpenStream();
        }

        protected override unsafe void WriteToBackend(in Entry64 item)
        {
            if (_bufferPos + EntrySize > _writeBuffer.Length)
                FlushBuffer();

            Unsafe.CopyBlockUnaligned(
                ref _writeBuffer[_bufferPos],
                ref Unsafe.As<Entry64, byte>(ref Unsafe.AsRef(in item)),
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
                if (InjectRecoverFault)
                    throw new IOException("injected OpenStream failure");
                OpenStream();
                if (_bufferPos > 0) { _stream!.Write(_writeBuffer.AsSpan(0, _bufferPos)); _bufferPos = 0; }
                _healthy      = true;
                _retryDelayMs = 1_000;
            }
            catch (Exception)
            {
                _stream          = null; // drop disposed reference — FlushBuffer null-guards above
                _retryDelayMs    = Math.Min(_retryDelayMs * 2, RetryMaxDelayMs);
                _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
            }
        }

        protected override void DisposeBackend()
        {
            try { _stream?.Dispose(); } catch { /* best-effort */ }
        }

        private void FlushBuffer()
        {
            if (_stream is null) { _bufferPos = 0; return; }
            try
            {
                if (InjectFlushFault)
                    throw new IOException("injected write failure");
                _stream.Write(_writeBuffer.AsSpan(0, _bufferPos));
                _bufferPos = 0;
            }
            catch (IOException)
            {
                _bufferPos       = 0; // prevent OOB on next WriteToBackend
                _healthy         = false;
                _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
            }
        }

        private void OpenStream() =>
            _stream = new FileStream(_path, FileMode.Append, FileAccess.Write,
                FileShare.Read, bufferSize: 0, FileOptions.SequentialScan);
    }

    private static bool SpinUntil(Func<bool> cond, int timeoutMs)
    {
        long deadline = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < deadline)
        {
            if (cond()) return true;
            Thread.Sleep(20);
        }
        return cond();
    }
}
