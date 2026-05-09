using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Relay.Buffers;
using Relay.Internal;
using Relay.Memory;

namespace Relay;

/// <summary>
/// Abstract base for a <see cref="PacketSink"/> that buffers payloads in a lock-free SPSC ring
/// and delivers them via a dedicated consumer thread. Subclasses implement the backend.
/// </summary>
public abstract class SpscQueueSink : PacketSink
{
    private const int SpinIter  = 10;
    private const int YieldIter = 5;
    private const int SleepMs   = 1;
    private const int BatchSize = 256;

    private readonly SpscByteRingBuffer _ring;
    private readonly long               _flushIntervalTicks;
    private readonly string             _pipeName;

    private Thread?       _thread;
    private volatile bool _running;
    private int           _flushRequested;     // Volatile-signalled by Flush(); read by consumer.
    private Exception?    _consumerException;
    private long          _drainDeadlineTicks;

    /// <summary>
    /// Backend health. Written exclusively by the consumer thread on IOException or recovery.
    /// The producer reads it via <see cref="IsHealthy"/> for short-circuit gating.
    /// </summary>
    protected volatile bool _healthy = true;

    /// <summary>Predecessor in the fallback chain. Wired by <see cref="Builder.SinkChain{THead}"/>.</summary>
    internal PacketSink? Prev { get; set; }

    /// <inheritdoc/>
    public override bool IsHealthy => _healthy;

    /// <summary>False only when the consumer thread exited due to an unhandled exception.</summary>
    public bool IsConsuming => _running && _consumerException is null;

    /// <summary>Non-null when the consumer thread crashed. Read on cold path only.</summary>
    public Exception? ConsumerException => _consumerException;

    /// <param name="ringCapacity">SPSC ring capacity in bytes. Must be a positive power of two.</param>
    /// <param name="flushIntervalMs">Max milliseconds between forced flushes.</param>
    /// <param name="pipeName">Optional thread-name suffix for debugger visibility.</param>
    protected SpscQueueSink(int ringCapacity, int flushIntervalMs, string pipeName = "")
    {
        _ring               = new SpscByteRingBuffer(ringCapacity);
        _flushIntervalTicks = (long)flushIntervalMs * (Stopwatch.Frequency / 1_000);
        _pipeName           = pipeName;
    }

    /// <summary>Pre-faults the ring buffer and starts the consumer thread.</summary>
    public void Start()
    {
        if (_running) return;
        _running = true;
        RelayMemory.PreFaultAndLock(_ring.Buffer);
        _thread = new Thread(ConsumeLoop)
        {
            Name         = string.IsNullOrEmpty(_pipeName) ? "relay-packet" : $"relay-packet-{_pipeName}",
            IsBackground = true,
            Priority     = ThreadPriority.BelowNormal
        };
        _thread.Start();
    }

    /// <summary>Signals the consumer to stop and waits up to <paramref name="drainTimeoutMs"/> ms.</summary>
    public void Stop(int drainTimeoutMs = 5_000)
    {
        if (!_running) return;
        Volatile.Write(ref _drainDeadlineTicks,
            HfClock.NowTicks + (long)drainTimeoutMs * (Stopwatch.Frequency / 1_000));
        _running = false;
        _thread?.Join(TimeSpan.FromMilliseconds(drainTimeoutMs));
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload) => _ring.TryPublish(payload);

    /// <summary>Writes a single payload to the backend. Called exclusively on the consumer thread.</summary>
    protected abstract void WriteToBackend(ReadOnlySpan<byte> payload);

    /// <summary>Flushes pending writes to the backend. Called on the consumer thread.</summary>
    protected abstract void FlushBackend();

    /// <summary>Attempts recovery after a backend failure. Called on the consumer thread.</summary>
    protected abstract void TryRecoverBackend();

    /// <summary>Closes the backend and releases resources. Called in the consumer finally block.</summary>
    protected abstract void DisposeBackend();

    /// <summary>
    /// Signals the consumer thread to flush. Never calls <see cref="FlushBackend"/> directly —
    /// that method is consumer-thread-only.
    /// </summary>
    public override void Flush() => Volatile.Write(ref _flushRequested, 1);

    /// <inheritdoc/>
    public override void Dispose() => Stop();

    private void ConsumeLoop()
    {
        try
        {
            long flushDeadline = HfClock.NowTicks + _flushIntervalTicks;
            int  idleSpin      = 0;

            while (ShouldKeepDraining())
            {
                bool checkDeadline;

                if (_ring.TryPeek(out var payload, out int advance))
                {
                    WriteToBackend(payload);
                    _ring.Advance(advance);
                    idleSpin = 0;

                    int batch = 1;
                    while (batch < BatchSize && _ring.TryPeek(out payload, out advance))
                    {
                        WriteToBackend(payload);
                        _ring.Advance(advance);
                        batch++;
                    }
                    checkDeadline = true;
                }
                else if (_running)
                {
                    if (idleSpin < SpinIter)
                    {
                        Thread.SpinWait(20);
                        checkDeadline = (idleSpin & 0x7) == 0;
                    }
                    else if (idleSpin < SpinIter + YieldIter)
                    {
                        Thread.Yield();
                        checkDeadline = true;
                    }
                    else
                    {
                        Thread.Sleep(SleepMs);
                        checkDeadline = true;
                    }
                    idleSpin++;
                }
                else
                {
                    checkDeadline = true;
                }

                bool flushNow    = Volatile.Read(ref _flushRequested) == 1;
                bool deadlineHit = checkDeadline && HfClock.NowTicks >= flushDeadline;
                if (flushNow || deadlineHit)
                {
                    if (flushNow)
                    {
                        // Drain ring before clearing the signal — items published before Flush()
                        // must be included in this batch (signal can arrive before ring item is peeked).
                        while (_ring.TryPeek(out var p, out int a)) { WriteToBackend(p); _ring.Advance(a); }
                        Volatile.Write(ref _flushRequested, 0);
                    }
                    FlushBackend();
                    if (deadlineHit)
                    {
                        TryRecoverBackend();
                        TryDrainToPrev();
                    }
                    flushDeadline = HfClock.NowTicks + _flushIntervalTicks;
                }
            }
        }
        catch (Exception ex)
        {
            _consumerException = ex;
        }
        finally
        {
            FlushBackend();
            DisposeBackend();
        }
    }

    // On shutdown, drains ring payloads back to the predecessor (which has recovered).
    // Gated on !_running: if drain ran during _running=true the consumer thread calling
    // Prev.Enqueue here would race with the original producer — two writers on a SPSC ring.
    private void TryDrainToPrev()
    {
        if (_running) return;
        if (Prev is not { IsHealthy: true }) return;
        while (_ring.TryPeek(out var payload, out int advance))
        {
            if (!Prev.IsHealthy) break;
            Prev.Enqueue(payload);
            _ring.Advance(advance);
        }
    }

    private bool ShouldKeepDraining()
    {
        if (_running) return true;
        if (_ring.IsEmpty) return false;
        long deadline = Volatile.Read(ref _drainDeadlineTicks);
        return deadline == 0 || HfClock.NowTicks < deadline;
    }
}
