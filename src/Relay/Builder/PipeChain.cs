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
    /// Appends <paramref name="next"/> as the fallback for the current tail.
    /// If <paramref name="next"/> is a <see cref="SpscQueuePipe{T}"/>, wires its
    /// <c>Prev</c> pointer so recovery can drain back to the predecessor.
    /// </summary>
    public PipeChain<T, THead> To(DispatchPipe<T> next)
    {
        if (next is null) throw new ArgumentNullException(nameof(next));
        if (next is SpscQueuePipe<T> sq) sq.Prev = _tail;
        _tail.Next = next;
        _tail      = next;
        return this;
    }

    /// <summary>Returns the head of the assembled chain.</summary>
    public THead Build() => _head;
}
