using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Abstract base for a composable fallback dispatch pipeline.
/// T must be a cache-line-aligned unmanaged struct (32, 64, 128, or 256 bytes).
/// Zero counters in base — subclasses opt in via override.
/// </summary>
/// <remarks>
/// Hot path: <see cref="Enqueue"/> checks <see cref="IsHealthy"/> (short-circuit) then
/// <see cref="Accept"/>. On any failure, delegates to <see cref="Next"/> (if set) or drops silently.
/// The chain is decentralized — no orchestrator; each pipe manages its own health and routing.
/// </remarks>
public abstract class DispatchSink<T> : IDisposable where T : unmanaged
{
    /// <summary>Next pipe in the fallback chain. Set by <see cref="Builder.SinkChain{T,THead}"/>.</summary>
    public DispatchSink<T>? Next { get; internal set; }

    /// <summary>
    /// True when this pipe can accept items. Set and cleared exclusively by the consumer thread
    /// (or by internal capacity checks). Never written by the producer.
    /// </summary>
    public abstract bool IsHealthy { get; }

    /// <summary>
    /// When true, <see cref="Enqueue"/> continues to <see cref="Next"/> even after a successful
    /// local <see cref="Accept"/>. Default <c>false</c> = write-and-stop (current semantics).
    /// Override and seal to enable bypass/tee behavior — JIT constant-folds the branch in both
    /// directions when the override returns a compile-time constant.
    /// </summary>
    protected virtual bool PropagateAfterAccept => false;

    /// <summary>
    /// Routes <paramref name="item"/>: delivers locally when healthy, then either stops or
    /// propagates to <see cref="Next"/> based on <see cref="PropagateAfterAccept"/>. On any
    /// local failure (unhealthy or Accept=false), delegates to <see cref="Next"/> (or drops
    /// if <c>Next == null</c>).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(in T item)
    {
        bool accepted = IsHealthy && Accept(in item);
        if (accepted && !PropagateAfterAccept) return;
        Next?.Enqueue(in item);
    }

    /// <summary>
    /// Attempts to deliver <paramref name="item"/> to this pipe's local backend or ring.
    /// Returns true on success, false to trigger fallback.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract bool Accept(in T item);

    /// <summary>Forces any buffered items toward the backend.</summary>
    public abstract void Flush();

    /// <inheritdoc/>
    public abstract void Dispose();
}
