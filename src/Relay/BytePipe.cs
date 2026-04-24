using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Abstract base for a composable fallback dispatch pipeline over variable-length byte payloads.
/// Parallel hierarchy to <see cref="DispatchPipe{T}"/>; share no types by design — the unmanaged
/// constraint on <see cref="DispatchPipe{T}"/> is incompatible with <see cref="ReadOnlySpan{T}"/>.
/// </summary>
/// <remarks>
/// Hot path: <see cref="Enqueue"/> checks <see cref="IsHealthy"/> (short-circuit) then
/// <see cref="Accept"/>. On any failure, delegates to <see cref="Next"/> (if set) or drops silently.
/// Chain is homogeneous — a <see cref="BytePipe"/> cannot cross into <see cref="DispatchPipe{T}"/>.
/// <para>
/// Payload semantics: the <c>ReadOnlySpan&lt;byte&gt;</c> handed to <see cref="Accept"/> is valid
/// for the duration of the call only. Concrete pipes that buffer must copy the payload before
/// returning from <c>Accept</c> (typically via an SPSC byte ring — see
/// <see cref="SpscByteQueuePipe"/>).
/// </para>
/// </remarks>
public abstract class BytePipe : IDisposable
{
    /// <summary>Next pipe in the fallback chain. Set internally by test wiring or builder.</summary>
    public BytePipe? Next { get; internal set; }

    /// <summary>
    /// True when this pipe can accept payloads. For consumer-backed pipes this is written
    /// exclusively by the consumer thread (on backend IOException or recovery); the producer
    /// never writes it.
    /// </summary>
    public abstract bool IsHealthy { get; }

    /// <summary>
    /// Routes <paramref name="payload"/>: delivers locally when healthy, otherwise delegates to
    /// <see cref="Next"/> (or drops if <c>Next == null</c>).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(ReadOnlySpan<byte> payload)
    {
        if (IsHealthy && Accept(payload)) return;
        Next?.Enqueue(payload);
    }

    /// <summary>
    /// Attempts to deliver <paramref name="payload"/> to this pipe's local backend or ring.
    /// Returns true on success, false to trigger fallback.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract bool Accept(ReadOnlySpan<byte> payload);

    /// <summary>Forces any buffered payloads toward the backend.</summary>
    public abstract void Flush();

    /// <inheritdoc/>
    public abstract void Dispose();
}
