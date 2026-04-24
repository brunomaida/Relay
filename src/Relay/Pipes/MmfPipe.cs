using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Relay.Pipes;

/// <summary>
/// Pipe that writes items to a pre-allocated <see cref="MemoryMappedFile"/>.
/// Writes via <see cref="MemoryMappedViewAccessor"/> never throw IOException — failure mode
/// is capacity exhaustion: once the file is full, <see cref="IsHealthy"/> returns false.
/// </summary>
public sealed class MmfPipe<T> : SpscQueuePipe<T> where T : unmanaged
{
    private const int DefaultRingCapacity  = 65_536;
    private const int DefaultFlushInterval = 250;

    private static readonly int EntrySize = Unsafe.SizeOf<T>();

    private readonly MemoryMappedFile           _mmf;
    private readonly MemoryMappedViewAccessor   _view;
    private readonly long                        _maxBytes;

    // Written by consumer thread via Volatile.Write; read by producer thread via Volatile.Read in IsHealthy.
    private long _position;

    /// <summary>True while the file has remaining capacity.</summary>
    public override bool IsHealthy => _healthy && Volatile.Read(ref _position) + EntrySize <= _maxBytes;

    /// <inheritdoc/>
    protected override bool PropagateAfterAccept => false;

    public MmfPipe(
        string path,
        long   maxBytes,
        int    ringCapacity  = DefaultRingCapacity,
        int    flushInterval = DefaultFlushInterval)
        : base(ringCapacity, flushInterval, "mmf")
    {
        if (maxBytes < EntrySize)
            throw new ArgumentException($"maxBytes must be >= {EntrySize}.", nameof(maxBytes));

        _maxBytes = maxBytes;
        _mmf      = MemoryMappedFile.CreateFromFile(path, FileMode.OpenOrCreate, null, maxBytes);
        _view     = _mmf.CreateViewAccessor(0, maxBytes, MemoryMappedFileAccess.ReadWrite);
    }

    protected override void WriteToBackend(in T item)
    {
        _view.Write(_position, ref Unsafe.AsRef(in item));
        Volatile.Write(ref _position, _position + EntrySize);
    }

    protected override void FlushBackend()   => _view.Flush();
    protected override void TryRecoverBackend() { /* capacity only — no recovery */ }

    protected override void DisposeBackend()
    {
        try { _view.Flush(); } catch { /* best-effort */ }
        _view.Dispose();
        _mmf.Dispose();
    }
}
