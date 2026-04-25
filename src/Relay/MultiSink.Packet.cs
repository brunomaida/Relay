using System;

namespace Relay;

/// <summary>
/// Broadcasts every payload to all child <see cref="PacketSink"/> instances.
/// <see cref="PacketSink.Next"/> is used only when all children are unhealthy.
/// </summary>
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
