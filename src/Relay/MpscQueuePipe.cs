using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Relay.Buffers;
using Relay.Internal;

namespace Relay;

/// <summary>
/// Abstract base for a pipe that buffers items in a lock-free MPSC ring and delivers them
/// via a dedicated consumer thread. Subclasses implement the backend (file, TCP, MMF, RAM).
/// </summary>
/// <remarks>
/// Any number of producer threads may call <see cref="DispatchPipe{T}.Enqueue"/> concurrently —
/// zero allocation, one CAS per write. Consumer thread runs <see cref="WriteToBackend"/>,
/// <see cref="FlushBackend"/>, <see cref="TryRecoverBackend"/>, and <see cref="TryDrainToPrev"/>
/// on a flush-interval cadence.
/// <para>
/// Recovery drain: on flush interval, if <see cref="DispatchPipe{T}.Next"/> (set via builder as
/// <see cref="Prev"/>) has recovered, items buffered during failure are drained back upstream.
/// Drain-to-Prev is SPSC w.r.t. Prev — the consumer thread is the sole caller of Prev.Enqueue
/// during drain, matching the single-consumer contract on this ring.
/// </para>
/// </remarks>
public abstract class MpscQueuePipe<T> : DispatchPipe<T> where T : unmanaged
{
    private const int SpinIter  = 10;
    private const int YieldIter = 5;
    private const int SleepMs   = 1;
    private const int BatchSize = 256;

    private readonly MpscRingBuffer<T> _ring;
    private readonly long              _flushIntervalTicks;
    private readonly string            _pipeName;

    private Thread?       _thread;
    private volatile bool _running;
    private Exception?    _consumerException;
    private long          _drainDeadlineTicks;

    /// <summary>
    /// Backend health flag. Set to false by the consumer thread on IOException.
    /// Set back to true by <see cref="TryRecoverBackend"/> when recovery succeeds.
    /// Never written by the producer.
    /// </summary>
    protected volatile bool _healthy = true;

    /// <summary>Set by <see cref="Builder.PipeChain{T,THead}.To"/> — predecessor in the chain.</summary>
    internal DispatchPipe<T>? Prev { get; set; }

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

    /// <param name="ringCapacity">MPSC ring capacity in entries. Must be a positive power of two.</param>
    /// <param name="flushIntervalMs">Max time between forced flushes in milliseconds.</param>
    /// <param name="pipeName">Optional name used as thread suffix for debugger/profiler visibility.</param>
    protected MpscQueuePipe(int ringCapacity, int flushIntervalMs, string pipeName = "")
    {
        _ring               = new MpscRingBuffer<T>(ringCapacity);
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
            Name         = string.IsNullOrEmpty(_pipeName) ? "relay-mpsc" : $"relay-mpsc-{_pipeName}",
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
    protected override bool Accept(in T item) => _ring.TryPublish(in item);

    /// <summary>Writes a single item to the backend. Called exclusively on the consumer thread.</summary>
    protected abstract void WriteToBackend(in T item);

    /// <summary>Flushes any pending writes to the backend. Called on the flush interval.</summary>
    protected abstract void FlushBackend();

    /// <summary>
    /// Attempts recovery after a backend failure. Sets <see cref="_healthy"/> to true on success.
    /// Called on the flush interval when <c>!_healthy</c>.
    /// </summary>
    protected abstract void TryRecoverBackend();

    /// <summary>Closes the backend and releases its resources. Called in the consumer finally block.</summary>
    protected abstract void DisposeBackend();

    /// <inheritdoc/>
    public override void Flush()   => FlushBackend();

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

                if (_ring.TryConsume(out var item))
                {
                    WriteToBackend(in item);
                    idleSpin = 0;

                    int batch = 1;
                    while (batch < BatchSize && _ring.TryConsume(out item))
                    {
                        WriteToBackend(in item);
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

                if (checkDeadline && HfClock.NowTicks >= flushDeadline)
                {
                    FlushBackend();
                    TryRecoverBackend();
                    TryDrainToPrev();
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

    // On recovery, drain accumulated items back to the predecessor (which has recovered).
    // SPSC caution w.r.t. Prev: this consumer thread is the sole caller of Prev.Enqueue during drain.
    // If the original producers concurrently resume feeding Prev, multiple threads enter Prev's Accept
    // simultaneously — a race window proportional to cache-coherency latency. Callers must ensure
    // producers quiesce before this drain runs, or accept the narrow window for violation in pathological cases.
    private void TryDrainToPrev()
    {
        if (Prev is not { IsHealthy: true }) return;
        while (_ring.TryConsume(out var item))
        {
            if (!Prev.IsHealthy) break;
            Prev.Enqueue(in item);
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
