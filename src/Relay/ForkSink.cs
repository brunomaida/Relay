using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Forwards every accepted item to a primary pipe AND always propagates to
/// <see cref="DispatchSink{T}.Next"/>. The primary pipe is driven synchronously on the caller
/// thread; <see cref="DispatchSink{T}.Next"/> always runs regardless of the primary's outcome.
/// </summary>
/// <remarks>
/// Intended use: audit / backup sink in parallel with the main delivery chain.
/// <para>
/// Lifecycle is delegated: <see cref="Flush"/> and <see cref="Dispose"/> forward to the primary.
/// If the chain has a distinct owner for Next, the primary-only delegation keeps responsibility
/// local to the fork.
/// </para>
/// <para>
/// JIT note: <see cref="PropagateAfterAccept"/> is a sealed constant true, so the base
/// <see cref="DispatchSink{T}.Enqueue"/> constant-folds the propagate branch — the call to
/// <c>Next?.Enqueue</c> is issued unconditionally after local acceptance.
/// </para>
/// <para>Thread safety: inherits from the primary sink. If primary is a
/// <see cref="SpscQueueSink{T}"/>, only one thread may call <c>Enqueue</c> at a time; if
/// primary is a <see cref="MpscQueueSink{T}"/>, concurrent producers are safe. Do NOT wrap
/// <c>Enqueue</c> in an external lock — the fork itself is stateless; adding a monitor costs
/// ~1000 cycles per call with no benefit.</para>
/// </remarks>
public sealed class ForkSink<T> : DispatchSink<T> where T : unmanaged
{
    private readonly DispatchSink<T> _primary;

    /// <param name="primary">The pipe to forward every item to. Must not be null.</param>
    public ForkSink(DispatchSink<T> primary) : base(propagateAfterAccept: true) =>
        _primary = primary ?? throw new ArgumentNullException(nameof(primary));

    /// <inheritdoc/>
    public override bool IsHealthy => _primary.IsHealthy;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in T item)
    {
        _primary.Enqueue(in item);
        return true;
    }

    /// <inheritdoc/>
    public override void Flush() => _primary.Flush();

    /// <inheritdoc/>
    public override void Dispose() => _primary.Dispose();
}
