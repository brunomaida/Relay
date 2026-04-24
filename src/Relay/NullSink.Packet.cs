using System;

namespace Relay;

/// <summary>No-op <see cref="PacketSink"/> terminal. Always healthy; silently discards payloads.</summary>
public sealed class NullSink : PacketSink
{
    /// <summary>Shared singleton. Use instead of allocating a new instance.</summary>
    public static readonly NullSink Instance = new();

    /// <inheritdoc/>
    public override bool IsHealthy => true;

    /// <inheritdoc/>
    protected override bool Accept(System.ReadOnlySpan<byte> payload) => true;

    /// <inheritdoc/>
    public override void Flush() { }

    /// <inheritdoc/>
    public override void Dispose() { }
}
