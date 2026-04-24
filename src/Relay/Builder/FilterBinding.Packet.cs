namespace Relay.Builder;

/// <summary>Intermediate state after <see cref="SinkChain{THead}.When"/>; closed by <see cref="To"/>.</summary>
public sealed class FilterBinding<THead> where THead : PacketSink
{
    private readonly SinkChain<THead> _chain;
    private readonly PacketPredicate  _predicate;

    internal FilterBinding(SinkChain<THead> chain, PacketPredicate predicate)
    {
        _chain     = chain;
        _predicate = predicate;
    }

    /// <summary>
    /// Creates a <see cref="FilterSink"/> wrapping <paramref name="downstream"/> and appends it
    /// to the current tail. Advances the chain tail to <paramref name="downstream"/> so subsequent
    /// <c>.To(...)</c> calls extend downstream's fallback chain (not overwrite the filter).
    /// </summary>
    public SinkChain<THead> To(PacketSink downstream)
    {
        var filter = new FilterSink(_predicate, downstream);
        _chain.AppendFilter(filter, downstream);
        return _chain;
    }
}
