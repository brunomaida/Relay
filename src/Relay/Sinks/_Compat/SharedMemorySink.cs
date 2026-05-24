using System;
using System.Runtime.Versioning;

namespace Relay.Sinks;

/// <summary>
/// Backward-compatibility shim. Use <see cref="SharedMemorySpscSink"/> instead.
/// </summary>
[Obsolete("Renamed to SharedMemorySpscSink. The previous name implied MPSC tolerance which was incorrect; use SharedMemorySpscSink and ensure single-producer-per-process semantics.", error: false)]
[SupportedOSPlatform("windows")]
public sealed class SharedMemorySink : SharedMemorySpscSink
{
    /// <inheritdoc cref="SharedMemorySpscSink(string, int)"/>
    public SharedMemorySink(string name, int totalCapacity = 4 * 1024 * 1024)
        : base(name, totalCapacity) { }
}
