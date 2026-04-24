using System;

namespace Relay.Builder;

/// <summary>Entry point for assembling a composable fallback dispatch chain.</summary>
/// <example>
/// <code>
/// // Serial fallback (single-producer): file → RAM
/// var head = RelayBuilder
///     .StartSpsc&lt;Entry, FileStreamPipe&lt;Entry&gt;&gt;(new FileStreamPipe&lt;Entry&gt;("/data/rec.bin"))
///     .To(new RamPipe&lt;Entry&gt;())
///     .Build();
///
/// head.Start();
/// head.Enqueue(in entry);
/// </code>
/// </example>
public static class RelayBuilder
{
    /// <summary>
    /// Begins a new pipe chain with <paramref name="head"/> as the first pipe. Generic entry —
    /// prefer <see cref="StartSpsc{T, THead}"/> / <see cref="StartMpsc{T, THead}"/> when the head
    /// is a queue pipe so the choice between single/multi producer is explicit at the call site.
    /// </summary>
    public static PipeChain<T, THead> Start<T, THead>(THead head)
        where T    : unmanaged
        where THead : DispatchPipe<T>
        => new(head);

    /// <summary>
    /// Begins a new chain with a single-producer <see cref="SpscQueuePipe{T}"/> head. One producer
    /// thread, one consumer thread — violating single-producer is undefined behaviour. Choose this
    /// when the producing code is a single thread (e.g., main game loop, dedicated worker).
    /// </summary>
    public static PipeChain<T, THead> StartSpsc<T, THead>(THead head)
        where T    : unmanaged
        where THead : SpscQueuePipe<T>
        => new(head);

    /// <summary>
    /// Begins a new chain with a multi-producer <see cref="MpscQueuePipe{T}"/> head. N producer
    /// threads, one consumer thread — one CAS per enqueue. Choose this when multiple threads
    /// enqueue concurrently (e.g., request handlers, actors).
    /// </summary>
    public static PipeChain<T, THead> StartMpsc<T, THead>(THead head)
        where T    : unmanaged
        where THead : MpscQueuePipe<T>
        => new(head);
}
