using System;

namespace Relay.Builder;

/// <summary>Entry point for assembling a composable fallback dispatch chain.</summary>
/// <example>
/// <code>
/// // Serial fallback: file → RAM
/// var head = RelayBuilder
///     .Start&lt;Entry, FileStreamPipe&lt;Entry&gt;&gt;(new FileStreamPipe&lt;Entry&gt;("/data/rec.bin"))
///     .To(new RamPipe&lt;Entry&gt;())
///     .Build();
///
/// head.Start();
/// head.Enqueue(in entry);
/// </code>
/// </example>
public static class RelayBuilder
{
    /// <summary>Begins a new pipe chain with <paramref name="head"/> as the first pipe.</summary>
    public static PipeChain<T, THead> Start<T, THead>(THead head)
        where T    : unmanaged
        where THead : DispatchPipe<T>
        => new(head);
}
