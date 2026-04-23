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
public abstract class DispatchPipe<T> : IDisposable where T : unmanaged
{
    /// <summary>Next pipe in the fallback chain. Set by <see cref="Builder.PipeChain{T,THead}"/>.</summary>
    public DispatchPipe<T>? Next { get; internal set; }

    /// <summary>
    /// True when this pipe can accept items. Set and cleared exclusively by the consumer thread
    /// (or by internal capacity checks). Never written by the producer.
    /// </summary>
    public abstract bool IsHealthy { get; }

    /// <summary>
    /// Routes <paramref name="item"/>: delivers locally when healthy, otherwise delegates to
    /// <see cref="Next"/> (or drops if <c>Next == null</c>).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(in T item)
    {
        if (IsHealthy && Accept(in item)) return;
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
