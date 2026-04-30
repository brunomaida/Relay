using System;

namespace Relay.Builder;

/// <summary>Fluent builder for <see cref="PacketSink"/> fallback chains.</summary>
public sealed class SinkChain<THead> where THead : PacketSink
{
    /// <summary>First sink in the chain. Pass to <see cref="PacketSink.Enqueue"/> callers.</summary>
    public THead Head { get; }

    private PacketSink _tail;

    internal SinkChain(THead head) { Head = head; _tail = head; }

    /// <summary>
    /// Appends <paramref name="sink"/> as fallback. Wires <see cref="SpscQueueSink.Prev"/>
    /// when <paramref name="sink"/> is an <see cref="SpscQueueSink"/> (enables drain-to-prev).
    /// </summary>
    public SinkChain<THead> To(PacketSink sink)
    {
        _tail.Next = sink;
        if (sink is SpscQueueSink spsc) spsc.Prev = _tail;
        _tail = sink;
        return this;
    }

    /// <summary>Inserts a <see cref="ForkSink"/> that delivers to <paramref name="primary"/> and Next.</summary>
    public SinkChain<THead> Fork(PacketSink primary)
    {
        var fork = new ForkSink(primary);
        _tail.Next = fork;
        _tail = fork;
        return this;
    }

    /// <summary>Opens a conditional gate. Close with <see cref="FilterBinding{THead}.To"/>.</summary>
    public FilterBinding<THead> When(PacketPredicate predicate) => new(this, predicate);

    /// <summary>Inserts a <see cref="MultiSink"/> broadcasting to all <paramref name="children"/>.</summary>
    public SinkChain<THead> Multi(params PacketSink[] children)
    {
        var multi = new MultiSink(children);
        _tail.Next = multi;
        _tail = multi;
        return this;
    }

    /// <summary>
    /// Fixed-arity 2-branch broadcast using the CRTP <see cref="Multi2PacketSink{TC1,TC2}"/>
    /// variant. When <typeparamref name="TC1"/> and <typeparamref name="TC2"/> are sealed, the
    /// JIT devirtualizes both <c>Enqueue</c> calls — saves 1-3 ns vs the array-based overload.
    /// </summary>
    public SinkChain<THead> Multi<TC1, TC2>(TC1 c1, TC2 c2)
        where TC1 : PacketSink
        where TC2 : PacketSink
    {
        var multi  = new Multi2PacketSink<TC1, TC2>(c1, c2);
        _tail.Next = multi;
        _tail      = multi;
        return this;
    }

    /// <summary>Returns the head as a bare <see cref="PacketSink"/> reference.</summary>
    public static implicit operator PacketSink(SinkChain<THead> chain) => chain.Head;

    // Called by FilterBinding.To: installs the filter at the current tail and advances
    // the tail to downstream. The filter is terminal for the predicate; downstream
    // extends the chain for any subsequent .To(...).
    internal void AppendFilter(FilterSink filter, PacketSink downstream)
    {
        _tail.Next = filter;
        _tail      = downstream;
    }
}
