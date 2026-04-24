using System;
using System.Collections.Generic;
using Relay;

namespace Relay.Tests.TestSinks;

/// <summary>Collects accepted payloads. IsHealthy is configurable.</summary>
internal class CollectingSink : PacketSink
{
    private readonly List<byte[]> _received = new();
    private bool _healthy = true;

    public IReadOnlyList<byte[]> Received => _received;
    public int AcceptCallCount { get; private set; }

    public override bool IsHealthy => _healthy;

    public void SetHealthy(bool value) => _healthy = value;
    public void Clear() => _received.Clear();

    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        AcceptCallCount++;
        _received.Add(payload.ToArray());
        return true;
    }

    public override void Flush()   { }
    public override void Dispose() { }
}
