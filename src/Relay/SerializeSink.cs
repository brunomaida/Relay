using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Relay;

/// <summary>
/// Bridge from the typed <see cref="DispatchSink{T}"/> hierarchy to the <see cref="PacketSink"/>
/// hierarchy. Converts <typeparamref name="T"/> to a <c>ReadOnlySpan&lt;byte&gt;</c> via
/// <see cref="MemoryMarshal.AsBytes{T}"/> — zero copy, zero allocation.
/// </summary>
/// <remarks>
/// <see cref="Accept"/> always returns <c>true</c>: the packet chain assumes
/// responsibility for delivery. The typed chain receives no per-record delivery signal.
/// Health is mirrored from the target packet sink for typed-chain short-circuiting.
/// </remarks>
/// <typeparam name="T">Unmanaged struct. Must be a multiple of 64 bytes (cache-line aligned).</typeparam>
public sealed class SerializeSink<T> : DispatchSink<T> where T : unmanaged
{
    private readonly PacketSink _target;

    /// <param name="target">Packet sink that receives the serialized bytes.</param>
    public SerializeSink(PacketSink target) => _target = target;

    /// <inheritdoc/>
    public override bool IsHealthy => _target.IsHealthy;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in T item)
    {
        _target.Enqueue(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in item, 1)));
        return true;
    }

    /// <inheritdoc/>
    public override void Flush()   => _target.Flush();

    /// <inheritdoc/>
    public override void Dispose() => _target.Dispose();
}
