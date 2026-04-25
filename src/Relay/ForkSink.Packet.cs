using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// <see cref="PacketSink"/> that delivers to a primary sink and propagates to <see cref="PacketSink.Next"/>
/// after a successful accept — enabling tee/audit patterns.
/// </summary>
public sealed class ForkSink : PacketSink
{
    private readonly PacketSink _primary;

    /// <param name="primary">Sink that receives a copy of every payload.</param>
    public ForkSink(PacketSink primary) => _primary = primary;

    /// <inheritdoc/>
    public override bool PropagateAfterAccept => true;

    /// <inheritdoc/>
    public override bool IsHealthy => _primary.IsHealthy;

    /// <summary>Enqueues to the primary; base propagates to Next on success.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        _primary.Enqueue(payload);
        return _primary.IsHealthy;
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
