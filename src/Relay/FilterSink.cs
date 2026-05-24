using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Conditional gate pipe: items passing the predicate are forwarded downstream;
/// items that fail are silently consumed (not forwarded to <see cref="DispatchSink{T}.Next"/>).
/// Always reports healthy.
/// </summary>
/// <remarks>
/// Returning true from <see cref="Accept"/> when the predicate fails prevents the fallback
/// chain from receiving items that were intentionally filtered. If you need a "try next on
/// filter miss" behaviour, compose two serial pipes instead.
/// <para>Thread safety: as thread-safe as the downstream sink. The filter itself is stateless.
/// Do NOT wrap <c>Enqueue</c> in an external lock — the predicate and downstream dispatch are
/// both lock-free when downstream is a queue sink; adding a monitor costs ~1000 cycles per call
/// with no benefit.</para>
/// </remarks>
public sealed class FilterSink<T> : DispatchSink<T> where T : unmanaged
{
    private readonly DispatchSink<T> _downstream;
    private readonly Predicate<T>    _predicate;

    public FilterSink(Predicate<T> predicate, DispatchSink<T> downstream)
    {
        _predicate  = predicate  ?? throw new ArgumentNullException(nameof(predicate));
        _downstream = downstream ?? throw new ArgumentNullException(nameof(downstream));
    }

    /// <summary>Always true — the filter itself never fails; only the downstream can.</summary>
    public override bool IsHealthy => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in T item)
    {
        if (_predicate(item)) _downstream.Enqueue(in item);
        return true;
    }

    public override void Flush()   => _downstream.Flush();
    public override void Dispose() => _downstream.Dispose();
}
