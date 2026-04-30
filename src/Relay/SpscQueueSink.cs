using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Relay.Buffers;
using Relay.Internal;

namespace Relay;

/// <summary>
/// Abstract base for a pipe that buffers items in a lock-free SPSC ring and delivers them
/// via a dedicated consumer thread. Subclasses implement the backend (file, TCP, MMF, RAM).
/// </summary>
/// <remarks>
/// Producer (caller) calls <see cref="DispatchSink{T}.Enqueue"/> — zero allocation, zero lock.
/// Consumer thread runs <see cref="WriteToBackend"/>, <see cref="FlushBackend"/>,
/// <see cref="TryRecoverBackend"/>, and <see cref="TryDrainToPrev"/> on a flush-interval cadence.
/// <para>
/// Recovery drain: on flush interval, if <see cref="DispatchSink{T}.Next"/> (set via builder as
/// <see cref="Prev"/>) has recovered, items buffered during failure are drained back upstream.
/// </para>
/// </remarks>
public abstract class SpscQueueSink<T> : DispatchSink<T> where T : unmanaged
{
    private const int SpinIter  = 10;
    private const int YieldIter = 5;
    private const int SleepMs   = 1;
    private const int BatchSize = 256;

    private readonly SpscRingBuffer<T> _ring;
    private readonly long              _flushIntervalTicks;
    private readonly string            _pipeName;
    private readonly T[]               _consumeBuf;

    private Thread?       _thread;
    private volatile bool _running;
    private Exception?    _consumerException;
    private long          _drainDeadlineTicks;
    private int           _flushRequested;

    /// <summary>
    /// Backend health flag. Set to false by the consumer thread on IOException.
    /// Set back to true by <see cref="TryRecoverBackend"/> when recovery succeeds.
    /// Never written by the producer.
    /// </summary>
    protected volatile bool _healthy = true;

    /// <summary>Set by <see cref="Builder.SinkChain{T,THead}.To"/> — predecessor in the chain.</summary>
    internal DispatchSink<T>? Prev { get; set; }

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

    /// <param name="ringCapacity">SPSC ring capacity in entries. Must be a positive power of two.</param>
    /// <param name="flushIntervalMs">Max time between forced flushes in milliseconds.</param>
    /// <param name="pipeName">Optional name used as thread suffix for debugger/profiler visibility.</param>
    protected SpscQueueSink(int ringCapacity, int flushIntervalMs, string pipeName = "")
    {
        SinkConstraints.AssertCacheLineAligned<T>();
        _ring               = new SpscRingBuffer<T>(ringCapacity);
        _flushIntervalTicks = (long)flushIntervalMs * (Stopwatch.Frequency / 1_000);
        _pipeName           = pipeName;
        _consumeBuf         = GC.AllocateArray<T>(BatchSize, pinned: true);
    }

    /// <summary>Pre-faults the ring buffer and starts the consumer thread.</summary>
    public void Start()
    {
        if (_running) return;
        _running = true;
        _ring.PreFaultAndLock();
        _thread = new Thread(ConsumeLoop)
        {
            Name         = string.IsNullOrEmpty(_pipeName) ? "relay" : $"relay-{_pipeName}",
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

    /// <summary>
    /// Publishes up to <paramref name="items"/>.Length entries in a single producer fence.
    /// Items that don't fit locally fall through to <see cref="DispatchSink{T}.Next"/> one by one
    /// (or drop if Next is null). Unhealthy backend routes the whole batch to Next.
    /// Single producer thread only.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnqueueBatch(ReadOnlySpan<T> items)
    {
        if (items.Length == 0) return;

        if (!IsHealthy)
        {
            for (int i = 0; i < items.Length; i++) Next?.Enqueue(in items[i]);
            return;
        }

        int published = _ring.TryPublishBatch(items);
        for (int i = published; i < items.Length; i++)
            Next?.Enqueue(in items[i]);
    }

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

    /// <summary>
    /// Signals the consumer thread to flush the backend on its next loop iteration.
    /// Non-blocking. FlushBackend is never called from the producer thread — eliminates
    /// the race between producer-initiated flush and the consumer's periodic flush.
    /// </summary>
    public override void Flush() => Volatile.Write(ref _flushRequested, 1);

    /// <inheritdoc/>
    public override void Dispose()
    {
        Stop();
        _ring.Dispose();
    }

    private void ConsumeLoop()
    {
        try
        {
            long flushDeadline = HfClock.NowTicks + _flushIntervalTicks;
            int  idleSpin      = 0;

            while (ShouldKeepDraining())
            {
                bool checkDeadline;

                int consumed = _ring.TryConsumeBatch(_consumeBuf);
                if (consumed > 0)
                {
                    // Single Volatile.Read(tail) + single Volatile.Write(head) per batch — saves
                    // (N-1) mfences vs per-item TryConsume. WriteToBackend is still per-item.
                    for (int i = 0; i < consumed; i++)
                        WriteToBackend(in _consumeBuf[i]);
                    idleSpin      = 0;
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
                    // Clear BEFORE FlushBackend so a racing Flush() that flips the flag back
                    // to 1 during backend work is picked up on the next iteration — not
                    // silently overwritten to 0 after the fact.
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

    // On shutdown, drains ring items back to the predecessor (which has recovered).
    // Gated on !_running: if drain ran during _running=true the consumer thread calling
    // Prev.Enqueue here would race with the original producer — two writers on a SPSC ring.
    // Items consumed by WriteToBackend before shutdown are not affected.
    private void TryDrainToPrev()
    {
        if (_running) return;
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
