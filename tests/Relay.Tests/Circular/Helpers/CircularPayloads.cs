using System;
using System.Runtime.InteropServices;

namespace Relay.Tests.Circular.Helpers;

/// <summary>
/// 64-byte cache-line aligned payload. HopCount at offset 0, Id at offset 8, 48 bytes padding.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 64)]
internal struct Packet64
{
    public long HopCount;
    public long Id;
}

/// <summary>
/// 128-byte cache-line aligned payload. HopCount at offset 0, Id at offset 8, Seq at offset 16, 104 bytes padding.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 128)]
internal struct Packet128
{
    public long HopCount;
    public long Id;
    public long Seq;
}

/// <summary>
/// 256-byte cache-line aligned payload. HopCount at offset 0, Id at offset 8, Seq at offset 16, 232 bytes padding.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 256)]
internal struct Packet256
{
    public long HopCount;
    public long Id;
    public long Seq;
}

/// <summary>
/// 320-byte cache-line aligned payload. HopCount at offset 0, Id at offset 8, Seq at offset 16, 296 bytes padding.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 320)]
internal struct Packet320
{
    public long HopCount;
    public long Id;
    public long Seq;
}

/// <summary>
/// Byte-ring header layout and helpers for variable-length packet payloads.
/// Layout: [0..7] long HopCount (LE), [8..15] long Id (LE), [16..] payload bytes.
/// </summary>
internal static class PacketLayout
{
    /// <summary>
    /// Byte offset where payload begins after the fixed 16-byte header.
    /// </summary>
    public const int HeaderBytes = 16;

    /// <summary>
    /// Read HopCount from packet header bytes [0..7].
    /// </summary>
    public static long ReadHop(ReadOnlySpan<byte> p) => BitConverter.ToInt64(p[..8]);

    /// <summary>
    /// Read Id from packet header bytes [8..15].
    /// </summary>
    public static long ReadId(ReadOnlySpan<byte> p) => BitConverter.ToInt64(p[8..16]);

    /// <summary>
    /// Write HopCount to packet header bytes [0..7].
    /// </summary>
    public static void WriteHop(Span<byte> p, long v) => BitConverter.TryWriteBytes(p[..8], v);

    /// <summary>
    /// Write Id to packet header bytes [8..15].
    /// </summary>
    public static void WriteId(Span<byte> p, long v) => BitConverter.TryWriteBytes(p[8..16], v);
}
