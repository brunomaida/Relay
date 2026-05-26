using System;
using System.IO.Pipes;
using System.Buffers.Binary;
using System.Threading;
using FluentAssertions;
using Relay.Receivers;
using Xunit;

namespace Relay.Tests.Receivers;

/// <summary>Tests for <see cref="NamedPipeReceiver{TState}"/>.</summary>
public sealed class NamedPipeReceiverTests
{
    [Fact]
    public void Poll_ReturnsFalse_BeforeConnection()
    {
        string pipeName = "relay-pipe-recv-" + Guid.NewGuid().ToString("N");
        using var recv = new NamedPipeReceiver<int>(pipeName, state: 0,
            callback: static (_, _) => { });

        recv.Poll().Should().BeFalse("no client has connected yet");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void WaitForConnectionAndPoll_LoopbackRoundTrip()
    {
        string  pipeName = "relay-pipe-recv-" + Guid.NewGuid().ToString("N");
        byte[]? captured = null;
        byte[]  payload  = { 0x01, 0x02, 0x03, 0x04 };

        using var recv = new NamedPipeReceiver<Action<byte[]>>(
            pipeName,
            state: b => captured = b,
            callback: static (capture, frame) => capture(frame.ToArray()));

        // Send one length-prefixed frame from a background thread (simulates external sender)
        var clientThread = new Thread(() =>
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            client.Connect(2_000);
            Span<byte> header = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);
            client.Write(header);
            client.Write(payload);
            client.Flush();
        }) { IsBackground = true };
        clientThread.Start();

        // WaitForConnection blocks until the background client connects (management-plane pattern)
        recv.WaitForConnection();

        // Client is connected; wait for the write+flush to complete
        clientThread.Join(5_000);

        // Data is in the pipe buffer — Poll reads synchronously and returns true immediately
        bool received = recv.Poll();

        received.Should().BeTrue("frame must be received after client sends it");
        captured.Should().BeEquivalentTo(payload, "payload must survive named-pipe round-trip");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Poll_ForwardsToNextSink_WhenChained()
    {
        string pipeName = "relay-pipe-recv-" + Guid.NewGuid().ToString("N");
        var forwarded = new System.Collections.Generic.List<byte[]>();
        var spy = new SpyPacketSink(b => forwarded.Add(b.ToArray()));
        byte[] payload = { 0xAA, 0xBB };

        using var recv = new NamedPipeReceiver<int>(
            pipeName, state: 0,
            callback: static (_, _) => { },
            next: spy);

        var clientThread = new Thread(() =>
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            client.Connect(2_000);
            Span<byte> header = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);
            client.Write(header);
            client.Write(payload);
            client.Flush();
        }) { IsBackground = true };
        clientThread.Start();

        recv.WaitForConnection();
        clientThread.Join(5_000);

        bool received = recv.Poll();

        received.Should().BeTrue();
        forwarded.Should().HaveCount(1);
        forwarded[0].Should().BeEquivalentTo(payload);
    }
}
