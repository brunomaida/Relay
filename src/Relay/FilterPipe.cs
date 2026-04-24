using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Conditional gate pipe: items passing the predicate are forwarded downstream;
/// items that fail are silently consumed (not forwarded to <see cref="DispatchPipe{T}.Next"/>).
/// Always reports healthy.
/// </summary>
/// <remarks>
/// Returning true from <see cref="Accept"/> when the predicate fails prevents the fallback
/// chain from receiving items that were intentionally filtered. If you need a "try next on
/// filter miss" behaviour, compose two serial pipes instead.
/// </remarks>
public sealed class FilterPipe<T> : DispatchPipe<T> where T : unmanaged
{
    private readonly DispatchPipe<T> _downstream;
    private readonly Predicate<T>    _predicate;

    public FilterPipe(Predicate<T> predicate, DispatchPipe<T> downstream)
    {
        _predicate  = predicate  ?? throw new ArgumentNullException(nameof(predicate));
        _downstream = downstream ?? throw new ArgumentNullException(nameof(downstream));
    }

    /// <summary>Always true — the filter itself never fails; only the downstream can.</summary>
    public override bool IsHealthy => true;

    /// <inheritdoc/>
    protected override bool PropagateAfterAccept => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in T item)
    {
        if (_predicate(item)) _downstream.Enqueue(in item);
        return true;
    }

    public override void Flush()   => _downstream.Flush();
    public override void Dispose() => _downstream.Dispose();
}
