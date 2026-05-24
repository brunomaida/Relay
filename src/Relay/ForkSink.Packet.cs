using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// <see cref="PacketSink"/> that delivers to a primary sink and propagates to <see cref="PacketSink.Next"/>
/// after a successful accept — enabling tee/audit patterns.
/// </summary>
/// <remarks>
/// <para>Thread safety: inherits from the primary sink. If primary is a
/// <see cref="SpscQueueSink"/>, only one thread may call <c>Enqueue</c> at a time; if primary
/// is a <see cref="MpscQueueSink"/>, concurrent producers are safe. Do NOT wrap <c>Enqueue</c>
/// in an external lock — the fork itself is stateless; adding a monitor costs ~1000 cycles per
/// call with no benefit.</para>
/// </remarks>
public sealed class ForkSink : PacketSink
{
    private readonly PacketSink _primary;

    /// <param name="primary">Sink that receives a copy of every payload.</param>
    public ForkSink(PacketSink primary) : base(propagateAfterAccept: true) => _primary = primary;

    /// <inheritdoc/>
    public override bool IsHealthy => _primary.IsHealthy;

    /// <summary>Enqueues to the primary; base propagates to Next on success.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        _primary.Enqueue(payload);
        return true;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        _primary.Flush();
        Next?.Flush();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        _primary.Dispose();
        Next?.Dispose();
    }
}
