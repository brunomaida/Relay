using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Relay.Pipes;

/// <summary>
/// Pipe that writes items to a pre-allocated <see cref="MemoryMappedFile"/>.
/// Writes via a raw pointer acquired once from <see cref="SafeMemoryMappedViewHandle"/> —
/// never throws IOException; failure mode is capacity exhaustion, at which point
/// <see cref="IsHealthy"/> returns false and subsequent items fall through to <c>Next</c>.
/// </summary>
/// <remarks>
/// Bypasses <see cref="MemoryMappedViewAccessor.Write{T}(long, ref T)"/> (which executes
/// a per-call bounds check and marshal) by caching the view's base pointer via
/// <see cref="SafeBuffer.AcquirePointer"/>. The pointer is valid for the lifetime of the
/// pipe; <see cref="SafeBuffer.ReleasePointer"/> is called from <see cref="DisposeBackend"/>.
/// </remarks>
public sealed class MmfPipe<T> : SpscQueuePipe<T> where T : unmanaged
{
    private const int DefaultRingCapacity  = 65_536;
    private const int DefaultFlushInterval = 250;

    private static readonly int EntrySize = Unsafe.SizeOf<T>();

    private readonly MemoryMappedFile           _mmf;
    private readonly MemoryMappedViewAccessor   _view;
    private readonly SafeMemoryMappedViewHandle _handle;
    private readonly long                       _maxBytes;
    private unsafe   byte*                      _basePtr;

    // Written by consumer thread via Volatile.Write; read by producer thread via Volatile.Read in IsHealthy.
    private long _position;

    /// <summary>True while the file has remaining capacity.</summary>
    public override bool IsHealthy => _healthy && Volatile.Read(ref _position) + EntrySize <= _maxBytes;

    /// <inheritdoc/>
    protected override bool PropagateAfterAccept => false;

    public unsafe MmfPipe(
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
        _handle   = _view.SafeMemoryMappedViewHandle;

        byte* p = null;
        _handle.AcquirePointer(ref p);
        _basePtr = p;
    }

    protected override unsafe void WriteToBackend(in T item)
    {
        // Hard guard: producer's IsHealthy check races the ring — items already queued may
        // arrive here after capacity was exhausted. Drop and flip unhealthy so Next drains.
        if (_position + EntrySize > _maxBytes)
        {
            _healthy = false;
            return;
        }
        Unsafe.CopyBlockUnaligned(
            _basePtr + _position,
            (byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in item)),
            (uint)EntrySize);
        Volatile.Write(ref _position, _position + EntrySize);
    }

    protected override void FlushBackend()   => _view.Flush();
    protected override void TryRecoverBackend() { /* capacity only — no recovery */ }

    protected override unsafe void DisposeBackend()
    {
        try { _view.Flush(); } catch { /* best-effort */ }
        if (_basePtr != null)
        {
            try { _handle.ReleasePointer(); } catch { /* best-effort */ }
            _basePtr = null;
        }
        _view.Dispose();
        _mmf.Dispose();
    }
}
