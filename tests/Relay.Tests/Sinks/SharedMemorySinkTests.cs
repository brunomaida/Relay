using System;
using System.Buffers.Binary;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.Sinks;

/// <summary>
/// Tests for <see cref="SharedMemorySink"/>.
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

        using var sink = new SharedMemorySink(name, cap);
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

        using var sink = new SharedMemorySink(name, total);

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

        using var sink = new SharedMemorySink(name, total);

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
        var sink = new SharedMemorySink(name, 1024);
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }
}
