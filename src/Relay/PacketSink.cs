using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Relay;

/// <summary>
/// Abstract base for a composable fallback dispatch pipeline over variable-length byte payloads.
/// Parallel hierarchy to <see cref="DispatchSink{T}"/>; share no types by design.
/// </summary>
/// <remarks>
/// Payload semantics: the <c>ReadOnlySpan&lt;byte&gt;</c> passed to <see cref="Accept"/> is valid
/// for the duration of the call only. Implementations that buffer must copy before returning.
/// </remarks>
public abstract class PacketSink : IDisposable
{
    /// <summary>Next sink in the fallback chain. Set by builder or test wiring.</summary>
    public PacketSink? Next { get; internal set; }

    /// <summary>
    /// True when this sink can accept payloads. Written exclusively by the consumer thread
    /// (on backend failure or recovery); never written by the producer.
    /// </summary>
    public abstract bool IsHealthy { get; }

    /// <summary>
    /// When true, <see cref="Enqueue"/> continues to <see cref="Next"/> after a successful
    /// local <see cref="Accept"/> — enabling fork/audit patterns. Set via base ctor
    /// (<see cref="ForkSink"/> passes <c>true</c>). Field, not virtual property — eliminates
    /// one vtable slot from the hot Enqueue path and removes a PGO-dependent indirect call.
    /// </summary>
    public readonly bool PropagateAfterAccept;

    /// <param name="propagateAfterAccept">When true, Enqueue propagates to Next after a successful Accept.</param>
    protected PacketSink(bool propagateAfterAccept = false) => PropagateAfterAccept = propagateAfterAccept;

    private long _dropCount;

    /// <summary>
    /// Cumulative count of payloads dropped at this terminal sink — observed when Next is null
    /// and either IsHealthy is false or Accept returned false. Read on cold path only;
    /// Volatile.Read for atomic 8-byte read on x64/arm64.
    /// </summary>
    public long DropCount => Volatile.Read(ref _dropCount);

    /// <summary>
    /// Routes <paramref name="payload"/>: delivers locally when healthy, then propagates to
    /// <see cref="Next"/> if <see cref="PropagateAfterAccept"/> is true. Falls through to
    /// <see cref="Next"/> on failure, or counts as a drop if <c>Next == null</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(ReadOnlySpan<byte> payload)
    {
        if (IsHealthy && Accept(payload))
        {
            if (PropagateAfterAccept) Next?.Enqueue(payload);
            return;
        }

        if (Next is { } next)
        {
            next.Enqueue(payload);
            return;
        }

        // Terminal drop — Next is null and either IsHealthy is false or Accept returned false.
        Interlocked.Increment(ref _dropCount);
    }

    /// <summary>
    /// Attempts to deliver <paramref name="payload"/> to this sink's local buffer or backend.
    /// Returns true on success, false to trigger fallback to <see cref="Next"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract bool Accept(ReadOnlySpan<byte> payload);

    /// <summary>Flushes any buffered payloads to the backend. Consumer-thread-only semantics
    /// for subclasses with a consumer thread; safe to call from any thread otherwise.</summary>
    public abstract void Flush();

    /// <inheritdoc/>
    public abstract void Dispose();
}
