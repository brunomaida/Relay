using System;

namespace Relay.Builder;

/// <summary>
/// Intermediate state returned by <see cref="SinkChain{T,THead}.When"/>. Forces the caller to
/// supply a downstream via <see cref="To"/> before the chain can continue — prevents dangling
/// predicates with no target.
/// </summary>
public readonly struct FilterBinding<T, THead>
    where T    : unmanaged
    where THead : DispatchSink<T>
{
    private readonly SinkChain<T, THead> _chain;
    private readonly Predicate<T>        _predicate;

    internal FilterBinding(SinkChain<T, THead> chain, Predicate<T> predicate)
    {
        _chain     = chain;
        _predicate = predicate;
    }

    /// <summary>
    /// Wraps <paramref name="downstream"/> in a <see cref="FilterSink{T}"/> using the predicate
    /// captured by <see cref="SinkChain{T,THead}.When"/>, then appends it as the fallback of the
    /// current tail. Items failing the predicate are silently consumed (not propagated to Next).
    /// Subsequent <c>.To</c> calls extend <paramref name="downstream"/>'s fallback chain.
    /// </summary>
    public SinkChain<T, THead> To(DispatchSink<T> downstream)
    {
        if (downstream is null) throw new ArgumentNullException(nameof(downstream));
        var filter = new FilterSink<T>(_predicate, downstream);
        _chain.AppendFilter(filter, downstream);
        return _chain;
    }
}
