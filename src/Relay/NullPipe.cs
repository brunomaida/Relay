using System.Runtime.CompilerServices;
// No additional usings needed — System.Runtime.CompilerServices covers MethodImplOptions.
namespace Relay;

/// <summary>
/// No-op sink: accepts every item and discards it immediately.
/// Useful as a terminal fallback, in tests, or to disable a pipe without restructuring the chain.
/// </summary>
public sealed class NullPipe<T> : DispatchPipe<T> where T : unmanaged
{
    /// <summary>Shared singleton — allocation-free.</summary>
    public static readonly NullPipe<T> Instance = new();

    public override bool IsHealthy => true;

    /// <inheritdoc/>
    protected override bool PropagateAfterAccept => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in T item) => true;

    public override void Flush()   { }
    public override void Dispose() { }
}
