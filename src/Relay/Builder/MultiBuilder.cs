using System;
using System.Collections.Generic;

namespace Relay.Builder;

/// <summary>
/// Sub-builder used to collect branches for a <see cref="MultiSink{T}"/>. Each branch is a
/// self-contained sub-chain whose head is added as a child. When exactly two sealed branches
/// are supplied via <see cref="Branch{TC}(TC)"/>, the caller should prefer the CRTP overload
/// on <see cref="SinkChain{T,THead}"/> directly for devirtualization.
/// </summary>
public sealed class MultiBuilder<T> where T : unmanaged
{
    private readonly List<DispatchSink<T>> _branches = new();

    internal MultiBuilder() { }

    /// <summary>Adds <paramref name="branch"/> as a child. Delivery is synchronous and broadcast.</summary>
    public MultiBuilder<T> Branch(DispatchSink<T> branch)
    {
        if (branch is null) throw new ArgumentNullException(nameof(branch));
        _branches.Add(branch);
        return this;
    }

    /// <summary>
    /// Builds a self-contained sub-chain via <paramref name="configure"/> and adds its head as
    /// a child. Use this when the branch itself needs a fallback chain (e.g. TCP → RAM).
    /// </summary>
    public MultiBuilder<T> Branch<THead>(THead head, Action<SinkChain<T, THead>> configure)
        where THead : DispatchSink<T>
    {
        if (head is null)      throw new ArgumentNullException(nameof(head));
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var chain = new SinkChain<T, THead>(head);
        configure(chain);
        _branches.Add(chain.Build());
        return this;
    }

    internal MultiSink<T> Build()
    {
        if (_branches.Count == 0)
            throw new InvalidOperationException("MultiBuilder requires at least one branch.");
        return new MultiSink<T>(_branches.ToArray());
    }
}
