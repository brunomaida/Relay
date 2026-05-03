using System;

namespace Relay.Sinks;

/// <summary>
/// Backward-compatibility shim. Use <see cref="MemorySink{T}"/> instead.
/// </summary>
[Obsolete("Renamed to MemorySink<T>. Will be removed in 2.0.")]
public sealed class RamSink<T> : MemorySink<T> where T : unmanaged
{
    /// <inheritdoc cref="MemorySink{T}(long)"/>
    public RamSink(long capacity = 1 << 23) : base(capacity) { }
}

/// <summary>
/// Backward-compatibility shim. Use <see cref="MemorySink"/> instead.
/// </summary>
[Obsolete("Renamed to MemorySink. Will be removed in 2.0.")]
public sealed class RamSink : MemorySink
{
    /// <inheritdoc cref="MemorySink(int)"/>
    public RamSink(int capacity = 4 * 1024 * 1024) : base(capacity) { }
}
