using System;

namespace Relay.Builder;

/// <summary>
/// Type-safe fluent builder state. Maintains the head type so callers get the concrete
/// type back from <see cref="Build"/> without casting.
/// </summary>
public sealed class PipeChain<T, THead>
    where T    : unmanaged
    where THead : DispatchPipe<T>
{
    private readonly THead      _head;
    private DispatchPipe<T>     _tail;

    internal PipeChain(THead head)
    {
        _head = head ?? throw new ArgumentNullException(nameof(head));
        _tail = head;
    }

    /// <summary>
    /// Appends <paramref name="next"/> as the fallback for the current tail. If
    /// <paramref name="next"/> is a <see cref="SpscQueuePipe{T}"/> or <see cref="MpscQueuePipe{T}"/>,
    /// wires its <c>Prev</c> pointer so recovery can drain back to the predecessor.
    /// </summary>
    public PipeChain<T, THead> To(DispatchPipe<T> next)
    {
        if (next is null) throw new ArgumentNullException(nameof(next));
        WirePrev(next);
        _tail.Next = next;
        _tail      = next;
        return this;
    }

    /// <summary>
    /// Inserts a <see cref="ForkPipe{T}"/> as the fallback of the current tail. Every item
    /// reaching the fork is forwarded to <paramref name="primary"/> and then propagated to the
    /// next pipe added via <see cref="To"/>.
    /// </summary>
    public PipeChain<T, THead> Fork(DispatchPipe<T> primary)
    {
        if (primary is null) throw new ArgumentNullException(nameof(primary));
        var fork   = new ForkPipe<T>(primary);
        _tail.Next = fork;
        _tail      = fork;
        return this;
    }

    /// <summary>
    /// Opens a conditional gate. Must be closed by <see cref="FilterBinding{T,THead}.To"/>.
    /// Items failing <paramref name="predicate"/> are silently consumed — they do NOT propagate
    /// to <see cref="DispatchPipe{T}.Next"/>.
    /// </summary>
    public FilterBinding<T, THead> When(Predicate<T> predicate)
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));
        return new FilterBinding<T, THead>(this, predicate);
    }

    /// <summary>
    /// Adds a broadcast step: every item reaching this tail is delivered to all branches
    /// registered via <paramref name="configure"/>. Fallback to <see cref="DispatchPipe{T}.Next"/>
    /// only fires when all branches are unhealthy.
    /// </summary>
    public PipeChain<T, THead> Multi(Action<MultiBuilder<T>> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        var builder = new MultiBuilder<T>();
        configure(builder);
        var multi  = builder.Build();
        _tail.Next = multi;
        _tail      = multi;
        return this;
    }

    /// <summary>
    /// Fixed-arity 2-branch broadcast using the CRTP <see cref="Multi2Pipe{T,TC1,TC2}"/> variant.
    /// When <typeparamref name="TC1"/> and <typeparamref name="TC2"/> are sealed, the JIT
    /// devirtualizes both <c>Enqueue</c> calls — saves ~6c versus the array-based overload.
    /// </summary>
    public PipeChain<T, THead> Multi<TC1, TC2>(TC1 c1, TC2 c2)
        where TC1 : DispatchPipe<T>
        where TC2 : DispatchPipe<T>
    {
        var multi  = new Multi2Pipe<T, TC1, TC2>(c1, c2);
        _tail.Next = multi;
        _tail      = multi;
        return this;
    }

    /// <summary>Returns the head of the assembled chain.</summary>
    public THead Build() => _head;

    internal void AppendFilter(FilterPipe<T> filter, DispatchPipe<T> downstream)
    {
        _tail.Next = filter;
        // Downstream owns any further .To extensions — the filter never invokes Next on miss.
        _tail      = downstream;
    }

    private void WirePrev(DispatchPipe<T> next)
    {
        switch (next)
        {
            case SpscQueuePipe<T> sq: sq.Prev = _tail; break;
            case MpscQueuePipe<T> mq: mq.Prev = _tail; break;
        }
    }
}
