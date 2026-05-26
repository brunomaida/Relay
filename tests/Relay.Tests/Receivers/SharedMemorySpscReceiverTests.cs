using System;
using System.Buffers.Binary;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Threading;
using FluentAssertions;
using Relay.Receivers;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.Receivers;

/// <summary>
/// Tests for <see cref="SharedMemorySpscReceiver{TState}"/>.
/// Verifies round-trip compatibility with <see cref="SharedMemorySpscSink"/>:
/// same ring layout (128-byte header, BE length-prefixed frames, modular index).
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SharedMemorySpscReceiverTests
{
    [Fact(Skip = "Windows only — Named MMF not supported on Linux")]
    public void Poll_ReturnsFalse_WhenRingIsEmpty()
    {
        string name = "Local\\relay-recv-" + Guid.NewGuid().ToString("N");
        using var sink = new SharedMemorySpscSink(name, totalCapacity: 4096);
        using var recv = new SharedMemorySpscReceiver<int>(name, state: 0,
            callback: static (_, _) => { });

        recv.Poll().Should().BeFalse("no frames have been written to the ring");
    }

    [Fact(Skip = "Windows only — Named MMF not supported on Linux")]
    public void Poll_InvokesCallback_AfterSinkEnqueues()
    {
        string name = "Local\\relay-recv-" + Guid.NewGuid().ToString("N");
        byte[]? captured = null;
        byte[] payload = { 0xDE, 0xAD, 0xBE, 0xEF };

        using var sink = new SharedMemorySpscSink(name, totalCapacity: 4096);
        using var recv = new SharedMemorySpscReceiver<Action<byte[]>>(
            name,
            state: b => captured = b,
            callback: static (capture, frame) => capture(frame.ToArray()));

        sink.Enqueue(payload);

        // Spin until the frame is visible (WriteIndex propagated via Volatile.Write in sink)
        bool received = false;
        for (int i = 0; i < 1_000 && !received; i++)
            received = recv.Poll();

        received.Should().BeTrue("frame must be received after sink enqueues it");
        captured.Should().BeEquivalentTo(payload, "payload must survive MMF ring-buffer round-trip");
    }

    [Fact(Skip = "Windows only — Named MMF not supported on Linux")]
    public void ReadIndex_AdvancesInMmf_AfterConsumingFrame()
    {
        string name  = "Local\\relay-recv-" + Guid.NewGuid().ToString("N");
        int    total = 4096;
        const int ReadIdxOffset = 64;
        byte[] payload = new byte[16];

        using var sink = new SharedMemorySpscSink(name, totalCapacity: total);
        using var recv = new SharedMemorySpscReceiver<int>(name, state: 0,
            callback: static (_, _) => { });

        sink.Enqueue(payload);

        for (int i = 0; i < 1_000 && !recv.Poll(); i++) { }

        // After consuming, ReadIndex in MMF must advance by 4 (header) + 16 (payload) = 20
        using var mmf  = MemoryMappedFile.OpenExisting(name);
        using var view = mmf.CreateViewAccessor(0, 0);
        Span<byte> idxBuf = stackalloc byte[4];
        view.ReadArray(ReadIdxOffset, idxBuf.ToArray(), 0, 4);
        int readIdx = BinaryPrimitives.ReadInt32LittleEndian(idxBuf); // MMF stores int as LE (machine-order)

        readIdx.Should().Be(4 + payload.Length, "ReadIndex must advance by one frame length after consuming");
    }

    [Fact(Skip = "Windows only — Named MMF not supported on Linux")]
    public void Poll_HandlesRingWrap_ReadsAllFrames()
    {
        // Ring capacity: 512 data bytes. Frames: 100-byte payload each = 104 bytes per frame.
        // Write 5 frames — total 520 bytes > 512, forcing ring wrap.
        string name = "Local\\relay-recv-" + Guid.NewGuid().ToString("N");
        const int Total    = 512 + 128; // 128 header + 512 data
        const int PayloadN = 100;
        const int FrameN   = 5;

        var received = new System.Collections.Generic.List<byte[]>();
        using var sink = new SharedMemorySpscSink(name, totalCapacity: Total);
        using var recv = new SharedMemorySpscReceiver<System.Collections.Generic.List<byte[]>>(
            name,
            state: received,
            callback: static (list, frame) => list.Add(frame.ToArray()));

        for (int f = 0; f < FrameN; f++)
        {
            byte[] payload = new byte[PayloadN];
            payload[0] = (byte)f; // mark each frame
            sink.Enqueue(payload);

            // Read each frame before writing the next to avoid overflow
            for (int i = 0; i < 1_000 && !recv.Poll(); i++) { }
        }

        received.Should().HaveCount(FrameN, "all frames must survive ring-wrap reads");
        for (int f = 0; f < FrameN; f++)
            received[f][0].Should().Be((byte)f, "frame order must be preserved through ring wrap");
    }
}
