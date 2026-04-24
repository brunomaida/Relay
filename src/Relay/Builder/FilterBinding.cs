using System;

namespace Relay.Builder;

/// <summary>
/// Intermediate state returned by <see cref="PipeChain{T,THead}.When"/>. Forces the caller to
/// supply a downstream via <see cref="To"/> before the chain can continue — prevents dangling
/// predicates with no target.
/// </summary>
public readonly struct FilterBinding<T, THead>
    where T    : unmanaged
    where THead : DispatchPipe<T>
{
    private readonly PipeChain<T, THead> _chain;
    private readonly Predicate<T>        _predicate;

    internal FilterBinding(PipeChain<T, THead> chain, Predicate<T> predicate)
    {
        _chain     = chain;
        _predicate = predicate;
    }

    /// <summary>
    /// Wraps <paramref name="downstream"/> in a <see cref="FilterPipe{T}"/> using the predicate
    /// captured by <see cref="PipeChain{T,THead}.When"/>, then appends it as the fallback of the
    /// current tail. Items failing the predicate are silently consumed (not propagated to Next).
    /// Subsequent <c>.To</c> calls extend <paramref name="downstream"/>'s fallback chain.
    /// </summary>
    public PipeChain<T, THead> To(DispatchPipe<T> downstream)
    {
        if (downstream is null) throw new ArgumentNullException(nameof(downstream));
        var filter = new FilterPipe<T>(_predicate, downstream);
        _chain.AppendFilter(filter, downstream);
        return _chain;
    }
}
