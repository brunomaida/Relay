using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Relay.Buffers;
using Relay.Internal;

namespace Relay;

/// <summary>
/// Abstract base for a <see cref="PacketSink"/> that buffers byte payloads in a lock-free MPSC
/// ring and delivers them via a dedicated consumer thread. Subclasses implement the backend.
/// </summary>
/// <remarks>
/// Any number of producer threads may call <see cref="PacketSink.Enqueue"/> concurrently —
/// zero allocation, one CAS per write (via <see cref="MpscByteRingBuffer"/> HeadCache reservation).
/// Consumer thread runs <see cref="WriteToBackend"/>, <see cref="FlushBackend"/>,
/// <see cref="TryRecoverBackend"/>, and <see cref="TryDrainToPrev"/> on a flush-interval cadence.
/// <para>
/// Recovery drain: on flush interval, if <see cref="PacketSink.Next"/> (set via builder as
/// <see cref="Prev"/>) has recovered, byte payloads buffered during failure are drained back upstream.
/// </para>
/// </remarks>
public abstract class MpscQueueSink : PacketSink
{
    private const int SpinIter  = 10;
    private const int YieldIter = 5;
    private const int SleepMs   = 1;
    private const int BatchSize = 256;

    private readonly MpscByteRingBuffer _ring;
    private readonly long               _flushIntervalTicks;
    private readonly string             _pipeName;

    private Thread?       _thread;
    private volatile bool _running;
    private int           _flushRequested;     // Volatile-signalled by Flush(); read by consumer.
    private Exception?    _consumerException;
    private long          _drainDeadlineTicks;

    /// <summary>
    /// Backend health flag. Set to false by the consumer thread on IOException.
    /// Set back to true by <see cref="TryRecoverBackend"/> when recovery succeeds.
    /// Never written by the producer.
    /// </summary>
    protected volatile bool _healthy = true;

    /// <summary>Predecessor in the chain. Wired by the sink-chain builder.</summary>
    internal PacketSink? Prev { get; set; }

    /// <summary>False if the consumer thread terminated with an unhandled exception.</summary>
    public bool IsConsuming => _running && _consumerException is null;

    /// <summary>Non-null when the consumer thread crashed. Read on cold path only.</summary>
    public Exception? ConsumerException => _consumerException;

    /// <summary>
    /// True when the backend is healthy. Ring capacity is NOT checked here — a full ring causes
    /// <see cref="Accept"/> to return false, which triggers the same fallback path at lower cost
    /// (avoids a redundant <see cref="System.Threading.Volatile.Read"/> of the consumer-owned head).
    /// </summary>
    public override bool IsHealthy => _healthy;

    /// <param name="ringCapacity">MPSC ring capacity in bytes. Must be a positive power of two.</param>
    /// <param name="flushIntervalMs">Max time between forced flushes in milliseconds.</param>
    /// <param name="pipeName">Optional name used as thread suffix for debugger/profiler visibility.</param>
    protected MpscQueueSink(int ringCapacity, int flushIntervalMs, string pipeName = "")
    {
        _ring               = new MpscByteRingBuffer(ringCapacity);
        _flushIntervalTicks = (long)flushIntervalMs * (Stopwatch.Frequency / 1_000);
        _pipeName           = pipeName;
    }

    /// <summary>Pre-faults the ring buffer and starts the consumer thread.</summary>
    public void Start()
    {
        if (_running) return;
        _running = true;
        _ring.PreFaultAndLock();
        _thread = new Thread(ConsumeLoop)
        {
            Name         = string.IsNullOrEmpty(_pipeName) ? "relay-packet-mpsc" : $"relay-packet-mpsc-{_pipeName}",
            IsBackground = true,
            Priority     = ThreadPriority.BelowNormal
        };
        _thread.Start();
    }

    /// <summary>Signals stop and waits up to <paramref name="drainTimeoutMs"/> for the ring to drain.</summary>
    public void Stop(int drainTimeoutMs = 5_000)
    {
        if (!_running) return;
        long tpm = Stopwatch.Frequency / 1_000;
        Volatile.Write(ref _drainDeadlineTicks, HfClock.NowTicks + (long)drainTimeoutMs * tpm);
        _running = false;
        _thread?.Join(TimeSpan.FromMilliseconds(drainTimeoutMs));
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload) => _ring.TryPublish(payload);

    /// <summary>Writes a single byte payload to the backend. Called exclusively on the consumer thread.</summary>
    protected abstract void WriteToBackend(ReadOnlySpan<byte> payload);

    /// <summary>Flushes any pending writes to the backend. Called on the consumer thread.</summary>
    protected abstract void FlushBackend();

    /// <summary>
    /// Attempts recovery after a backend failure. Sets <see cref="_healthy"/> to true on success.
    /// Called on the flush interval when <c>!_healthy</c>.
    /// </summary>
    protected abstract void TryRecoverBackend();

    /// <summary>Closes the backend and releases its resources. Called in the consumer finally block.</summary>
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
                        // QPC (~25c + LFENCE stall) throttled to every 8 spin iterations;
                        // yield/sleep paths are already expensive enough that QPC is noise.
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
                    // Clear BEFORE calling FlushBackend — avoids missing a concurrent Flush()
                    // signal that arrives between the clear and the actual flush operation.
                    if (flushNow) Volatile.Write(ref _flushRequested, 0);
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
    // Gated on !_running: drain during _running=true would let this consumer thread call
    // Prev.Enqueue concurrently with active producers — SPSC-violation on Prev's ring.
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
