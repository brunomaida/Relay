using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// No-op sink: accepts every byte payload and discards it immediately.
/// Useful as a terminal fallback, in tests, or to disable a pipe without restructuring the chain.
/// </summary>
public sealed class NullByteSink : PacketSink
{
    /// <summary>Shared singleton — allocation-free.</summary>
    public static readonly NullByteSink Instance = new();

    public override bool IsHealthy => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload) => true;

    public override void Flush()   { }
    public override void Dispose() { }
}
