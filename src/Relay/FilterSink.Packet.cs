using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Delegate for evaluating whether a packet payload should be forwarded.
/// The span is valid for the duration of the call only — do not store it.
/// </summary>
public delegate bool PacketPredicate(ReadOnlySpan<byte> payload);

/// <summary>
/// Conditional gate: payloads matching the predicate are forwarded to <paramref name="downstream"/>;
/// non-matching payloads are silently consumed and do NOT propagate to <see cref="PacketSink.Next"/>.
/// </summary>
public sealed class FilterSink : PacketSink
{
    private readonly PacketPredicate _predicate;
    private readonly PacketSink      _downstream;

    /// <param name="predicate">True → forward to downstream. False → discard.</param>
    /// <param name="downstream">Receives payloads that pass the predicate.</param>
    public FilterSink(PacketPredicate predicate, PacketSink downstream)
    {
        _predicate  = predicate;
        _downstream = downstream;
    }

    /// <summary>
    /// Always true. If this mirrored downstream health, the base
    /// <see cref="PacketSink.Enqueue"/> would skip <see cref="Accept"/> whenever downstream
    /// failed, routing the payload straight to <see cref="PacketSink.Next"/> — violating the
    /// "filtered items never propagate to Next" invariant. Downstream owns its own fallback chain.
    /// </summary>
    public override bool IsHealthy => true;

    /// <summary>
    /// Forwards matching payloads to downstream. Always returns true — filtered items are consumed,
    /// not propagated to <see cref="PacketSink.Next"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        if (_predicate(payload)) _downstream.Enqueue(payload);
        return true;
    }

    /// <inheritdoc/>
    public override void Flush()   => _downstream.Flush();

    /// <inheritdoc/>
    public override void Dispose() => _downstream.Dispose();
}
