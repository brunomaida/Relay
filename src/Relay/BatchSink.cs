using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Abstract <see cref="SpscQueueSink"/> that accumulates incoming payloads in a POH-pinned scratch
/// buffer on the consumer thread, flushing when the next payload cannot fit or when
/// <see cref="PacketSink.Flush"/> is signalled. Subclasses implement <see cref="OnFlush"/>.
/// </summary>
/// <remarks>
/// Lock-free: only the consumer thread mutates <c>_scratch</c> / <c>_offset</c> from
/// <see cref="WriteToBackend"/> and <see cref="FlushBackend"/>. Producer enqueue path is the
/// inherited SPSC ring publish.
/// </remarks>
public abstract class BatchSink : SpscQueueSink
{
    private readonly byte[] _scratch;
    private          int    _offset;

    /// <param name="ringCapacity">SPSC ring capacity in bytes. Power of two ≥ 16.</param>
    /// <param name="batchCapacity">Scratch buffer capacity in bytes. Pinned (POH).</param>
    /// <param name="flushIntervalMs">Max ms between forced flushes (timer-driven).</param>
    /// <param name="sinkName">Diagnostic name for the consumer thread.</param>
    protected BatchSink(int ringCapacity, int batchCapacity, int flushIntervalMs, string sinkName)
        : base(ringCapacity, flushIntervalMs, sinkName)
    {
        _scratch = GC.AllocateUninitializedArray<byte>(batchCapacity, pinned: true);
    }

    /// <summary>Capacity of the scratch buffer in bytes.</summary>
    protected int BatchCapacity => _scratch.Length;

    /// <inheritdoc/>
    protected sealed override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        if (payload.Length > _scratch.Length)
            return;  // oversized — silently drop (caller responsibility to size scratch).

        if (_offset + payload.Length > _scratch.Length)
            FlushScratch();

        payload.CopyTo(_scratch.AsSpan(_offset));
        _offset += payload.Length;
    }

    /// <inheritdoc/>
    protected sealed override void FlushBackend() => FlushScratch();

    /// <inheritdoc/>
    protected override void TryRecoverBackend() { /* base BatchSink has no backend; subclasses override. */ }

    /// <inheritdoc/>
    protected override void DisposeBackend() { /* base BatchSink owns no resource other than _scratch. */ }

    /// <summary>Called on the consumer thread with the accumulated batch. Span is valid for this call only.</summary>
    protected abstract void OnFlush(ReadOnlySpan<byte> batch);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FlushScratch()
    {
        if (_offset == 0) return;
        try { OnFlush(_scratch.AsSpan(0, _offset)); }
        finally { _offset = 0; }
    }
}
