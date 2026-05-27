using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Relay;
using Relay.Sinks;

namespace Relay.Tests.Circular.Helpers;

/// <summary>
/// SPSC ring node. Consumer thread increments <see cref="Count"/> and forwards via <see cref="RingNext"/>.
/// When <c>decrementHops</c> is true, HopCount (offset 0 in T) is decremented before forwarding;
/// the item dies when HopCount reaches zero.
/// </summary>
internal sealed class SpscRingNode<T> : SpscQueueSink<T> where T : unmanaged
{
    private long _count;
    private readonly bool _decrementHops;

    internal DispatchSink<T>? RingNext { get; set; }

    public long Count => Volatile.Read(ref _count);

    public SpscRingNode(int ringCapacity = 1024, int flushIntervalMs = 1, string name = "", bool decrementHops = false)
        : base(ringCapacity, flushIntervalMs, name)
    {
        _decrementHops = decrementHops;
    }

    protected override void WriteToBackend(in T item)
    {
        _count++;

        if (_decrementHops)
        {
            T copy = item;
            ref long hop = ref Unsafe.As<T, long>(ref copy);
            if (hop <= 0) return;
            hop--;
            RingNext?.Enqueue(in copy);
        }
        else
        {
            RingNext?.Enqueue(in item);
        }
    }

    protected override void FlushBackend()       { }
    protected override void TryRecoverBackend()  { }
    protected override void DisposeBackend()     { }
}

/// <summary>
/// MPSC ring node. Identical to <see cref="SpscRingNode{T}"/> but extends <see cref="MpscQueueSink{T}"/>.
/// Use as the entry node in infinite rings where the last node's consumer re-injects items (multi-producer).
/// </summary>
internal sealed class MpscRingNode<T> : MpscQueueSink<T> where T : unmanaged
{
    private long _count;
    private readonly bool _decrementHops;

    internal DispatchSink<T>? RingNext { get; set; }

    public long Count => Volatile.Read(ref _count);

    public MpscRingNode(int ringCapacity = 1024, int flushIntervalMs = 1, string name = "", bool decrementHops = false)
        : base(ringCapacity, flushIntervalMs, name)
    {
        _decrementHops = decrementHops;
    }

    protected override void WriteToBackend(in T item)
    {
        _count++;

        if (_decrementHops)
        {
            T copy = item;
            ref long hop = ref Unsafe.As<T, long>(ref copy);
            if (hop <= 0) return;
            hop--;
            RingNext?.Enqueue(in copy);
        }
        else
        {
            RingNext?.Enqueue(in item);
        }
    }

    protected override void FlushBackend()       { }
    protected override void TryRecoverBackend()  { }
    protected override void DisposeBackend()     { }
}

/// <summary>
/// SPSC ring node wrapping a real backend sink. Consumer thread delivers to the backend then
/// forwards the same item to <see cref="RingNext"/>.
/// </summary>
internal sealed class BackendRingNode<T> : SpscQueueSink<T> where T : unmanaged
{
    private readonly DispatchSink<T> _backend;
    private long _count;

    internal DispatchSink<T>? RingNext { get; set; }

    public long Count => Volatile.Read(ref _count);

    public BackendRingNode(DispatchSink<T> backend, int ringCapacity = 1024, int flushIntervalMs = 1, string name = "")
        : base(ringCapacity, flushIntervalMs, name)
    {
        _backend = backend;
    }

    protected override void WriteToBackend(in T item)
    {
        _count++;
        _backend.Enqueue(in item);
        RingNext?.Enqueue(in item);
    }

    protected override void FlushBackend()      => _backend.Flush();
    protected override void TryRecoverBackend() { }
    protected override void DisposeBackend()    => _backend.Dispose();
}

/// <summary>
/// Synchronous ring node — no consumer thread. Forwards immediately in <see cref="Accept"/> on the
/// calling thread. Use at most 2 non-adjacent in large rings to avoid stack depth issues.
/// </summary>
internal sealed class SyncRingNode<T> : DispatchSink<T> where T : unmanaged
{
    private readonly MemorySink<T>? _memory;
    private long _count;
    private long _dropped;

    internal DispatchSink<T>? RingNext { get; set; }

    public override bool IsHealthy => _memory?.IsHealthy ?? true;
    public long Count   => Volatile.Read(ref _count);
    public long Dropped => Volatile.Read(ref _dropped);

    public SyncRingNode(MemorySink<T>? memory = null)
    {
        _memory = memory;
    }

    protected override bool Accept(in T item)
    {
        Interlocked.Increment(ref _count);

        if (_memory is not null)
        {
            if (!_memory.IsHealthy)
            {
                Interlocked.Increment(ref _dropped);
                return false;
            }
            _memory.Enqueue(in item);
        }

        RingNext?.Enqueue(in item);
        return true;
    }

    public override void Flush()   { }
    public override void Dispose() => _memory?.Dispose();
}

/// <summary>
/// SPSC ring node for the packet hierarchy (variable-length <see cref="ReadOnlySpan{T}"/> payloads).
/// Decrements HopCount in a POH-pinned scratch buffer before forwarding to <see cref="RingNext"/>.
/// </summary>
internal sealed class PacketRingNode : Relay.SpscQueueSink
{
    private readonly byte[] _scratch;
    private long _count;

    internal PacketSink? RingNext { get; set; }

    public long Count => Volatile.Read(ref _count);

    public PacketRingNode(int ringCapacity = 65536, int flushIntervalMs = 1, string name = "", int maxPayloadBytes = 512)
        : base(ringCapacity, flushIntervalMs, name)
    {
        _scratch = GC.AllocateArray<byte>(maxPayloadBytes, pinned: true);
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        _count++;

        long hop = PacketLayout.ReadHop(payload);
        if (hop <= 0) return;
        hop--;

        payload.CopyTo(_scratch);
        PacketLayout.WriteHop(_scratch, hop);
        RingNext?.Enqueue(_scratch.AsSpan(0, payload.Length));
    }

    protected override void FlushBackend()       { }
    protected override void TryRecoverBackend()  { }
    protected override void DisposeBackend()     { }
}

/// <summary>
/// SPSC packet ring node wrapping a <see cref="PacketSink"/> backend. Consumer thread delivers to
/// the backend then forwards to <see cref="RingNext"/>.
/// </summary>
internal sealed class BackendPacketRingNode : Relay.SpscQueueSink
{
    private readonly PacketSink _backend;
    private long _count;

    internal PacketSink? RingNext { get; set; }

    public long Count => Volatile.Read(ref _count);

    public BackendPacketRingNode(PacketSink backend, int ringCapacity = 65536, int flushIntervalMs = 1, string name = "")
        : base(ringCapacity, flushIntervalMs, name)
    {
        _backend = backend;
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        _count++;
        _backend.Enqueue(payload);
        RingNext?.Enqueue(payload);
    }

    protected override void FlushBackend()      => _backend.Flush();
    protected override void TryRecoverBackend() { }
    protected override void DisposeBackend()    => _backend.Dispose();
}

/// <summary>
/// MPSC packet ring node. Identical to <see cref="PacketRingNode"/> but extends <see cref="Relay.MpscQueueSink"/>.
/// Use as the entry node in infinite packet rings.
/// </summary>
internal sealed class PacketMpscRingNode : Relay.MpscQueueSink
{
    private readonly byte[] _scratch;
    private long _count;

    internal PacketSink? RingNext { get; set; }

    public long Count => Volatile.Read(ref _count);

    public PacketMpscRingNode(int ringCapacity = 65536, int flushIntervalMs = 1, string name = "", int maxPayloadBytes = 512)
        : base(ringCapacity, flushIntervalMs, name)
    {
        _scratch = GC.AllocateArray<byte>(maxPayloadBytes, pinned: true);
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        _count++;

        long hop = PacketLayout.ReadHop(payload);
        if (hop <= 0) return;
        hop--;

        payload.CopyTo(_scratch);
        PacketLayout.WriteHop(_scratch, hop);
        RingNext?.Enqueue(_scratch.AsSpan(0, payload.Length));
    }

    protected override void FlushBackend()       { }
    protected override void TryRecoverBackend()  { }
    protected override void DisposeBackend()     { }
}

/// <summary>
/// Terminal sink that counts accepted items. Used in saturation tests to measure
/// how many items completed a full ring circuit.
/// </summary>
internal sealed class CountingTerminalSink<T> : DispatchSink<T> where T : unmanaged
{
    private long _count;

    public override bool IsHealthy => true;
    public long Count => Volatile.Read(ref _count);

    protected override bool Accept(in T item)
    {
        Interlocked.Increment(ref _count);
        return true;
    }

    public override void Flush()   { }
    public override void Dispose() { }
}
