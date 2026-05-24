using System;
using System.Buffers.Binary;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.Sinks;

/// <summary>
/// Tests for <see cref="SharedMemorySpscSink"/>.
/// Verifies byte-exact compatibility with Log2 SharedMemorySink protocol:
///   Magic = 0x4C473200 ("LG2\0"), int WriteIndex at offset 8, int ReadIndex at offset 64,
///   records = 4-byte BE length prefix + payload, ring-wrapped modular index.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SharedMemorySinkTests
{
    // Protocol constants — must match Log2 SharedMemorySink exactly
    private const uint Magic            = 0x4C473200u; // "LG2\0", NOT "LOG2"
    private const int  HeaderSize       = 128;
    private const int  WriteIndexOffset = 8;
    private const int  ReadIndexOffset  = 64;

    [Fact(Skip = "Windows only — Named MMF not supported on Linux")]
    public void Accept_Payload_WrittenToMmfWithBELengthPrefix()
    {
        string name = "Local\\relay-shm-" + Guid.NewGuid().ToString("N");
        int    cap  = 4 * 1024;

        using var sink = new SharedMemorySpscSink(name, cap);
        byte[] payload = [10, 20, 30];
        sink.Enqueue(payload);

        using var mmf  = MemoryMappedFile.OpenExisting(name);
        using var view = mmf.CreateViewAccessor();

        // Magic at offset 0 (stored as int LE in the process, but value must equal 0x4C473200)
        view.Read<uint>(0, out uint magic);
        magic.Should().Be(Magic);

        // WriteIndex is a modular int; after one record it equals (0 + 4 + 3) % dataCapacity = 7
        view.Read<int>(WriteIndexOffset, out int writeIdx);
        writeIdx.Should().Be(4 + payload.Length);

        // 4-byte BE length prefix at data area start
        byte[] lenBytes = new byte[4];
        view.ReadArray(HeaderSize, lenBytes, 0, 4);
        int len = (int)BinaryPrimitives.ReadUInt32BigEndian(lenBytes);
        len.Should().Be(payload.Length);

        // Payload bytes immediately after the length prefix
        byte[] data = new byte[len];
        view.ReadArray(HeaderSize + 4, data, 0, len);
        data.Should().Equal(payload);
    }

    [Fact(Skip = "Windows only — Named MMF not supported on Linux")]
    public void Accept_BufferFull_DropCounted()
    {
        // Data area = 64 bytes. Each record = 4 + 20 = 24 bytes.
        // Record 1: occupies [0..23] → WriteIndex = 24
        // Record 2: occupies [24..47] → WriteIndex = 48
        // Record 3 needs 24 bytes; (48 + 24) % 64 = 8 → WriteIndex would wrap to 8.
        // Log2 protocol has no back-pressure; it wraps and overwrites.
        // Relay adds DropCount at PacketSink level when Accept returns false.
        // For drop to trigger, we need IsHealthy to return false OR Accept to return false.
        // With a 64-byte ring and 24-byte records, the third Enqueue fits (ring wraps).
        // To force a drop we use a payload larger than _dataCapacity itself.
        string name = "Local\\relay-shm-drop-" + Guid.NewGuid().ToString("N");
        int    dataArea = 64;
        int    total    = HeaderSize + dataArea;

        using var sink = new SharedMemorySpscSink(name, total);

        // A payload that by itself (4 + payload) exceeds dataArea forces Accept to return false.
        byte[] tooBig = new byte[dataArea]; // frameLen = 4 + 64 = 68 > 64 → Accept returns false
        sink.Enqueue(tooBig);
        sink.DropCount.Should().Be(1);
    }

    [Fact(Skip = "Windows only — Named MMF not supported on Linux")]
    public void Accept_RingWrap_WrittenCorrectly()
    {
        // Place WriteIndex near the end of the ring so a record straddles the wrap.
        // Ring = 32 bytes. Write a 10-byte payload first to push WriteIndex to 14.
        // Then write a payload that forces wrap-around.
        string name = "Local\\relay-shm-wrap-" + Guid.NewGuid().ToString("N");
        int    dataArea = 32;
        int    total    = HeaderSize + dataArea;

        using var sink = new SharedMemorySpscSink(name, total);

        // First record: 4 + 10 = 14 bytes → WriteIndex = 14
        sink.Enqueue(new byte[10]);

        // Second record: 4 + 12 = 16 bytes → WriteIndex = (14 + 16) % 32 = 30
        sink.Enqueue(new byte[12]);

        // Third record: 4 + 6 = 10 bytes → starts at 30, wraps at 32 (2 bytes then 8 more)
        byte[] third = [1, 2, 3, 4, 5, 6];
        sink.Enqueue(third);

        using var mmf  = MemoryMappedFile.OpenExisting(name);
        using var view = mmf.CreateViewAccessor();

        view.Read<int>(WriteIndexOffset, out int writeIdx);
        writeIdx.Should().Be((14 + 16 + 10) % dataArea); // = 8

        // Read third record: length prefix starts at offset 30, wraps at 32
        byte[] lenBytes = new byte[4];
        // Bytes [30..31] are first 2 bytes of prefix, [0..1] are last 2 bytes
        byte b0 = view.ReadByte(HeaderSize + 30);
        byte b1 = view.ReadByte(HeaderSize + 31);
        byte b2 = view.ReadByte(HeaderSize + 0);
        byte b3 = view.ReadByte(HeaderSize + 1);
        int len = (int)BinaryPrimitives.ReadUInt32BigEndian([b0, b1, b2, b3]);
        len.Should().Be(third.Length);

        // Payload follows at offset 2 (after wrap), 6 bytes
        byte[] data = new byte[6];
        for (int i = 0; i < 6; i++)
            data[i] = view.ReadByte(HeaderSize + 2 + i);
        data.Should().Equal(third);
    }

    [Fact(Skip = "Windows only — Named MMF not supported on Linux")]
    public void Dispose_IsIdempotent()
    {
        string name = "Local\\relay-shm-disp-" + Guid.NewGuid().ToString("N");
        var sink = new SharedMemorySpscSink(name, 1024);
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Regression test for the publish-ordering race: reader must never observe a partial frame.
    /// Producer writes N frames with a distinctive pattern (high bit set, low 7 bits = seq).
    /// A concurrent reader spins on WriteIndex and validates every payload byte before the
    /// index advances equals the expected pattern — zero bytes signal incomplete writes.
    /// This test fails on the pre-fix CAS-before-write code; passes on the fixed code.
    /// </summary>
    [Fact]
    [SupportedOSPlatform("windows")]
    public unsafe void Reader_NeverObservesPartialFrame()
    {
        const int  Frames      = 20_000;
        const int  PayloadSize = 4_096; // large payload widens the race window (~500+ cycles to copy)
        const int  FrameLen    = 4 + PayloadSize;
        // Ring large enough to never wrap during the run — avoids wrap-path in reader
        int        dataArea    = FrameLen * (Frames + 1);
        int        total       = HeaderSize + dataArea;

        string name = "Local\\relay-shm-race-" + Guid.NewGuid().ToString("N");

        using var sink = new SharedMemorySpscSink(name, total);

        // Open a reader view into the same MMF
        using var readerMmf  = MemoryMappedFile.OpenExisting(name);
        using var readerView = readerMmf.CreateViewAccessor(0, total);

        byte* readerPtr = null;
        readerView.SafeMemoryMappedViewHandle.AcquirePointer(ref readerPtr);
        try
        {
            bool    producerDone = false;
            string? raceError    = null;

            var reader = new Thread(() =>
            {
                int prevIdx = 0;
                byte* data  = readerPtr + HeaderSize;

                while (!Volatile.Read(ref producerDone) || prevIdx < Frames * FrameLen)
                {
                    int writeIdx = Volatile.Read(ref *(int*)(readerPtr + WriteIndexOffset));
                    if (writeIdx == prevIdx)
                    {
                        Thread.SpinWait(10);
                        continue;
                    }

                    // A frame was published. Validate every byte in [prevIdx .. prevIdx+FrameLen).
                    // Length prefix (4 bytes BE): first byte must be 0 (len = PayloadSize < 256)
                    // then remaining prefix bytes, then PayloadSize bytes with high bit set.
                    // Any zero in the payload region = partial write race.
                    int off = prevIdx;

                    // Check length prefix: expect 0x00_00_00_10 (=16, big-endian)
                    byte lenB0 = data[off];
                    byte lenB1 = data[off + 1];
                    byte lenB2 = data[off + 2];
                    byte lenB3 = data[off + 3];
                    int  len   = (lenB0 << 24) | (lenB1 << 16) | (lenB2 << 8) | lenB3;
                    if (len != PayloadSize)
                    {
                        raceError = $"Frame at {off}: length prefix = {len}, expected {PayloadSize}";
                        break;
                    }

                    // Validate payload bytes — each byte has high bit set (0x80..0xFF)
                    for (int i = 0; i < PayloadSize; i++)
                    {
                        byte b = data[off + 4 + i];
                        if ((b & 0x80) == 0)
                        {
                            raceError = $"Partial frame at {off}: payload[{i}] = 0x{b:X2} (high bit not set) — producer had not finished writing";
                            break;
                        }
                    }
                    if (raceError != null) break;

                    prevIdx = writeIdx;
                }
            });

            reader.IsBackground = true;
            reader.Start();

            // Produce N frames. Each payload byte has high bit set so zero = race.
            byte[] payload = new byte[PayloadSize];
            for (int seq = 0; seq < Frames; seq++)
            {
                byte marker = (byte)(0x80 | (seq & 0x7F));
                Array.Fill(payload, marker);
                sink.Enqueue(payload);
            }

            Volatile.Write(ref producerDone, true);
            reader.Join(TimeSpan.FromSeconds(10));

            raceError.Should().BeNull(because: "reader must never see a partial frame");
        }
        finally
        {
            readerView.SafeMemoryMappedViewHandle.ReleasePointer();
        }
    }
}
