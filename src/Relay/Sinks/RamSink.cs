using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Relay.Sinks;

/// <summary>
/// Last-resort native-memory circular ring pipe.
/// Writes to an unmanaged circular buffer (<see cref="NativeMemory.AllocZeroed"/>).
/// Never throws IOException; <see cref="IsHealthy"/> is false only when the ring is full.
/// Drain via <see cref="DrainTo"/> when the primary pipe recovers.
/// </summary>
public sealed unsafe class RamSink<T> : DispatchSink<T> where T : unmanaged
{
    private const int DefaultCapacity = 1 << 23; // 8_388_608 entries — ~512 MB for T=64B

    private readonly T*   _buffer;
    private readonly long _capacity;
    private readonly long _mask;

    private long _head;
    private long _tail;
    private bool _disposed;

    public RamSink(long capacity = DefaultCapacity)
    {
        if (capacity <= 0 || (capacity & (capacity - 1)) != 0)
            throw new ArgumentException("Capacity must be a positive power of two.", nameof(capacity));

        _capacity = capacity;
        _mask     = capacity - 1;
        _buffer   = (T*)NativeMemory.AllocZeroed((nuint)(capacity * sizeof(T)));
    }

    /// <summary>True when the ring has at least one free slot.</summary>
    public override bool IsHealthy => _tail - _head < _capacity;

    /// <inheritdoc/>
    protected override bool PropagateAfterAccept => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in T item)
    {
        if (_tail - _head >= _capacity) return false;
        _buffer[_tail & _mask] = item;
        _tail++;
        return true;
    }

    /// <summary>
    /// Drains all buffered items to <paramref name="target"/> in FIFO order.
    /// Stops early if <paramref name="target"/> becomes unhealthy.
    /// Call from a single thread (typically <paramref name="target"/>'s consumer recovery path).
    /// </summary>
    public void DrainTo(DispatchSink<T> target)
    {
        while (_head < _tail)
        {
            if (!target.IsHealthy) break;
            target.Enqueue(in _buffer[_head & _mask]);
            _head++;
        }
    }

    public override void Flush() { }

    public override void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMemory.Free(_buffer);
    }
}
