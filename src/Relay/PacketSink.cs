using System;
using System.Runtime.CompilerServices;

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
    /// local <see cref="Accept"/> — enabling fork/audit patterns.
    /// JIT eliminates the branch in sealed subclasses returning a compile-time constant.
    /// </summary>
    public virtual bool PropagateAfterAccept => false;

    /// <summary>
    /// Routes <paramref name="payload"/>: delivers locally when healthy, then propagates to
    /// <see cref="Next"/> if <see cref="PropagateAfterAccept"/> is true. Falls through to
    /// <see cref="Next"/> on failure, or drops silently when <c>Next == null</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(ReadOnlySpan<byte> payload)
    {
        if (IsHealthy && Accept(payload))
        {
            if (PropagateAfterAccept) Next?.Enqueue(payload);
            return;
        }
        Next?.Enqueue(payload);
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
