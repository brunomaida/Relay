using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;
using FluentAssertions;
using Relay.Receivers;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.Receivers;

/// <summary>
/// Regression tests for hot-path-audit findings F0 (TCP/NamedPipe wire desync on bogus frameLen)
/// and F0b (SharedMemorySpscReceiver infinite stall on bogus frameLen / wrong magic).
/// </summary>
public sealed class ReceiverDesyncStallTests
{
    // ── F0 · TcpReceiver ──────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void TcpReceiver_Poll_ThrowsAndTearsDown_OnNegativeFrameLen()
    {
        using var recv = new TcpReceiver<int>(
            new IPEndPoint(IPAddress.Loopback, 0), state: 0,
            callback: static (_, _) => { });

        int port = recv.LocalEndPoint.Port;

        var senderThread = new Thread(() =>
        {
            using var client = new TcpClient();
            client.Connect(IPAddress.Loopback, port);
            using var stream = client.GetStream();
            // BE-encoded -1 = 0xFFFFFFFF
            Span<byte> header = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(header, -1);
            stream.Write(header);
            stream.Flush();
            Thread.Sleep(200);
        }) { IsBackground = true };
        senderThread.Start();

        recv.Accept();
        Thread.Sleep(50);

        Action act = () => recv.Poll();
        act.Should().Throw<InvalidDataException>("bogus length must surface, not silently desync wire");

        // After teardown, subsequent Poll returns false (stream nulled) — does NOT block or throw
        recv.Poll().Should().BeFalse("stream is torn down after bogus-length detection");
        senderThread.Join(2_000);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void TcpReceiver_Poll_ThrowsAndTearsDown_OnOversizedFrameLen()
    {
        using var recv = new TcpReceiver<int>(
            new IPEndPoint(IPAddress.Loopback, 0), state: 0,
            callback: static (_, _) => { },
            bufferSize: 256);

        int port = recv.LocalEndPoint.Port;

        var senderThread = new Thread(() =>
        {
            using var client = new TcpClient();
            client.Connect(IPAddress.Loopback, port);
            using var stream = client.GetStream();
            // length way larger than buffer
            Span<byte> header = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(header, 1_000_000);
            stream.Write(header);
            stream.Flush();
            Thread.Sleep(200);
        }) { IsBackground = true };
        senderThread.Start();

        recv.Accept();
        Thread.Sleep(50);

        Action act = () => recv.Poll();
        act.Should().Throw<InvalidDataException>();
        recv.Poll().Should().BeFalse();
        senderThread.Join(2_000);
    }

    // ── F0 · NamedPipeReceiver ───────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void NamedPipeReceiver_Poll_ThrowsAndTearsDown_OnNegativeFrameLen()
    {
        string pipeName = "relay-pipe-recv-f0-" + Guid.NewGuid().ToString("N");
        using var recv = new NamedPipeReceiver<int>(pipeName, state: 0,
            callback: static (_, _) => { });

        var clientThread = new Thread(() =>
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            client.Connect(2_000);
            Span<byte> header = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(header, -42);
            client.Write(header);
            client.Flush();
            Thread.Sleep(200);
        }) { IsBackground = true };
        clientThread.Start();

        recv.WaitForConnection();
        clientThread.Join(5_000);

        Action act = () => recv.Poll();
        act.Should().Throw<InvalidDataException>();
        recv.Poll().Should().BeFalse("pipe is torn down after bogus-length detection");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void NamedPipeReceiver_Poll_ThrowsAndTearsDown_OnOversizedFrameLen()
    {
        string pipeName = "relay-pipe-recv-f0over-" + Guid.NewGuid().ToString("N");
        using var recv = new NamedPipeReceiver<int>(
            pipeName, state: 0,
            callback: static (_, _) => { },
            bufferSize: 256);

        var clientThread = new Thread(() =>
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            client.Connect(2_000);
            Span<byte> header = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(header, 1_000_000);
            client.Write(header);
            client.Flush();
            Thread.Sleep(200);
        }) { IsBackground = true };
        clientThread.Start();

        recv.WaitForConnection();
        clientThread.Join(5_000);

        Action act = () => recv.Poll();
        act.Should().Throw<InvalidDataException>();
        recv.Poll().Should().BeFalse();
    }

    // ── F0b · SharedMemorySpscReceiver ───────────────────────────────────────

    [Fact]
    [SupportedOSPlatform("windows")]
    public void SharedMemorySpscReceiver_Ctor_ThrowsOnWrongMagic()
    {
        if (!OperatingSystem.IsWindows()) return; // Named MMF: Windows only

        string name = "Local\\relay-recv-f0b-magic-" + Guid.NewGuid().ToString("N");
        // Create an MMF without the SharedMemorySpscSink magic header
        using var mmf  = MemoryMappedFile.CreateNew(name, 4096);
        using var view = mmf.CreateViewAccessor(0, 4096);
        view.Write(0, 0xDEADBEEFu); // wrong magic (should be 0x4C473200)

        Action act = () => new SharedMemorySpscReceiver<int>(name, state: 0,
            callback: static (_, _) => { });
        act.Should().Throw<InvalidDataException>("ctor must reject MMF without the SharedMemorySpsc magic");
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void SharedMemorySpscReceiver_Poll_ThrowsOnBogusFrameLen()
    {
        if (!OperatingSystem.IsWindows()) return;

        string name = "Local\\relay-recv-f0b-len-" + Guid.NewGuid().ToString("N");
        const int Total = 4096;
        using var sink = new SharedMemorySpscSink(name, totalCapacity: Total);
        using var recv = new SharedMemorySpscReceiver<int>(name, state: 0,
            callback: static (_, _) => { });

        // Bypass the sink and stamp a bogus BE-encoded length directly into the ring,
        // then advance WriteIdx so the receiver sees a "frame" available.
        using var mmf  = MemoryMappedFile.OpenExisting(name);
        using var view = mmf.CreateViewAccessor(0, 0);

        const int HeaderSize   = 128;
        const int WriteIdxOff  = 8;
        // Write BE -1 at data area offset 0 (read by receiver as frameLen)
        byte[] hdr = { 0xFF, 0xFF, 0xFF, 0xFF };
        view.WriteArray(HeaderSize, hdr, 0, 4);
        // Advance WriteIdx by 4 (header only, payload omitted on purpose)
        view.Write(WriteIdxOff, 4);

        Action act = () => recv.Poll();
        act.Should().Throw<InvalidDataException>("receiver must fail-fast on bogus length instead of stalling");
    }
}
