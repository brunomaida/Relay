using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Relay.Buffers;
using Relay.Internal;
using Relay.Memory;

namespace Relay;

/// <summary>
/// Abstract base for a pipe that buffers byte payloads in a lock-free SPSC ring and delivers them
/// via a dedicated consumer thread. Subclasses implement the backend (file, TCP, MMF, RAM).
/// </summary>
/// <remarks>
/// Producer (caller) calls <see cref="BytePipe.Enqueue"/> — zero allocation, zero lock.
/// Consumer thread runs <see cref="WriteToBackend"/>, <see cref="FlushBackend"/>,
/// <see cref="TryRecoverBackend"/>, and <see cref="TryDrainToPrev"/> on a flush-interval cadence.
/// <para>
/// Recovery drain: on flush interval, if <see cref="BytePipe.Next"/> (set via builder as
/// <see cref="Prev"/>) has recovered, byte payloads buffered during failure are drained back upstream.
/// </para>
/// </remarks>
public abstract class SpscByteQueuePipe : BytePipe
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
    private Exception?    _consumerException;
    private long          _drainDeadlineTicks;

    /// <summary>
    /// Backend health flag. Set to false by the consumer thread on IOException.
    /// Set back to true by <see cref="TryRecoverBackend"/> when recovery succeeds.
    /// Never written by the producer.
    /// </summary>
    protected volatile bool _healthy = true;

    /// <summary>Predecessor in the chain. Wired by byte-pipe test harness today; a dedicated
    /// builder will set this once multiple consumers require one.</summary>
    internal BytePipe? Prev { get; set; }

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

    /// <param name="ringCapacity">SPSC ring capacity in bytes. Must be a positive power of two.</param>
    /// <param name="flushIntervalMs">Max time between forced flushes in milliseconds.</param>
    /// <param name="pipeName">Optional name used as thread suffix for debugger/profiler visibility.</param>
    protected SpscByteQueuePipe(int ringCapacity, int flushIntervalMs, string pipeName = "")
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
            Name         = string.IsNullOrEmpty(_pipeName) ? "relay-byte" : $"relay-byte-{_pipeName}",
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
                }
                else if (_running)
                {
                    if      (idleSpin < SpinIter)               Thread.SpinWait(20);
                    else if (idleSpin < SpinIter + YieldIter)   Thread.Yield();
                    else                                         Thread.Sleep(SleepMs);
                    idleSpin++;
                }

                if (HfClock.NowTicks >= flushDeadline)
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

    // On recovery, drain accumulated byte payloads back to the predecessor (which has recovered).
    // SPSC caution: Prev.Enqueue is called from this consumer thread. If the original producer
    // concurrently resumes feeding Prev, two threads enter Prev's Accept simultaneously — a race
    // window proportional to cache-coherency latency. Callers must ensure the producer quiesces
    // before this drain runs, or accept the narrow window for SPSC-violation in pathological cases.
    private void TryDrainToPrev()
    {
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
