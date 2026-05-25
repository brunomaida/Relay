using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Broadcasts every payload to all child <see cref="PacketSink"/> instances.
/// <see cref="PacketSink.Next"/> is used only when all children are unhealthy.
/// </summary>
/// <remarks>
/// <para>Thread safety: inherits from children. The broadcast itself is synchronous on the
/// calling thread. Do NOT wrap <c>Enqueue</c> in an external lock — any child sinks that
/// require single-producer discipline must be respected individually; adding a monitor here
/// costs ~1000 cycles per call with no benefit.
/// Therefore <c>Enqueue</c> on this sink is safe for concurrent callers only when every child
/// is itself multi-producer-safe (e.g. <see cref="MpscQueueSink"/>); mixing SPSC children
/// with concurrent producers is undefined behaviour.</para>
/// </remarks>
public sealed class MultiSink : PacketSink
{
    private readonly PacketSink[] _children;

    /// <param name="children">One or more sinks to broadcast to.</param>
    public MultiSink(params PacketSink[] children) => _children = children;

    /// <inheritdoc/>
    public override bool IsHealthy
    {
        get
        {
            foreach (var c in _children)
                if (c.IsHealthy) return true;
            return false;
        }
    }

    /// <summary>Broadcasts to all children. Returns true always; Next reached only via base IsHealthy gate.</summary>
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        foreach (var c in _children)
            c.Enqueue(payload);
        return true;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        foreach (var c in _children) c.Flush();
        Next?.Flush();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        foreach (var c in _children) c.Dispose();
    }
}

/// <summary>
/// Fixed-arity 2-child broadcast for the packet hierarchy. Mirror of typed
/// <see cref="Multi2Sink{T,TC1,TC2}"/>. When <typeparamref name="TC1"/> and
/// <typeparamref name="TC2"/> are sealed, the JIT devirtualizes and inlines both
/// <see cref="PacketSink.Enqueue"/> calls — saves 1-3 ns vs the array-based
/// <see cref="MultiSink"/> at N=2.
/// </summary>
/// <remarks>
/// <para>Thread safety: inherits from children — same contract as <see cref="MultiSink"/>.
/// Do NOT wrap <c>Enqueue</c> in an external lock; adding a monitor costs ~1000 cycles per call
/// with no benefit.
/// Therefore <c>Enqueue</c> on this sink is safe for concurrent callers only when every child
/// is itself multi-producer-safe (e.g. <see cref="MpscQueueSink"/>); mixing SPSC children
/// with concurrent producers is undefined behaviour.</para>
/// </remarks>
public sealed class Multi2PacketSink<TC1, TC2> : PacketSink
    where TC1 : PacketSink
    where TC2 : PacketSink
{
    private readonly TC1 _c1;
    private readonly TC2 _c2;

    /// <param name="c1">First child sink.</param>
    /// <param name="c2">Second child sink.</param>
    public Multi2PacketSink(TC1 c1, TC2 c2)
    {
        _c1 = c1 ?? throw new ArgumentNullException(nameof(c1));
        _c2 = c2 ?? throw new ArgumentNullException(nameof(c2));
    }

    /// <inheritdoc/>
    public override bool IsHealthy => _c1.IsHealthy || _c2.IsHealthy;

    /// <summary>Broadcasts to both children. Always returns true; falls through to <see cref="PacketSink.Next"/> only when <see cref="IsHealthy"/> is false.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        _c1.Enqueue(payload);
        _c2.Enqueue(payload);
        return true;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        _c1.Flush();
        _c2.Flush();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        _c1.Dispose();
        _c2.Dispose();
    }
}
