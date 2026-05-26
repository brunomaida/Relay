using System;
using Relay;

namespace Relay.Tests.Receivers;

/// <summary>PacketSink that invokes a callback on each received payload — for receiver tests.</summary>
internal sealed class SpyPacketSink : PacketSink
{
    private readonly Action<ReadOnlySpan<byte>> _onPayload;

    public SpyPacketSink(Action<ReadOnlySpan<byte>> onPayload) => _onPayload = onPayload;

    public override bool IsHealthy => true;

    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        _onPayload(payload);
        return true;
    }

    public override void Flush()   { }
    public override void Dispose() { }
}
