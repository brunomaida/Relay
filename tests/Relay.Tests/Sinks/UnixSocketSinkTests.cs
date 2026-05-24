using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.Sinks;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class UnixSocketSinkTests
{
    [Fact(Skip = "linux/macos only — runs in CI on those platforms")]
    public async Task Accept_PayloadLargerThanBuffer_BypassesBuffer()
    {
        // sendBufferCapacity=64: a 256-byte payload (+ 4B header = 260B) exceeds buffer → bypass path.
        string sockPath = Path.Combine(Path.GetTempPath(), "relay-big-" + Guid.NewGuid().ToString("N") + ".sock");
        byte[] payload  = new byte[256];
        for (int i = 0; i < payload.Length; i++) payload[i] = (byte)(i & 0xFF);

        var ready = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

        var serverTask = Task.Run(() =>
        {
            using var server = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            server.Bind(new UnixDomainSocketEndPoint(sockPath));
            server.Listen(1);
            using var client = server.Accept();
            byte[] lenBuf = new byte[4];
            int read = client.Receive(lenBuf, 4, SocketFlags.None);
            if (read != 4) { ready.SetException(new Exception("short header")); return; }
            int len = (int)BinaryPrimitives.ReadUInt32BigEndian(lenBuf);
            byte[] buf = new byte[len];
            int got = 0;
            while (got < len) got += client.Receive(buf, got, len - got, SocketFlags.None);
            ready.SetResult(buf);
        });

        await Task.Delay(100);

        using var sink = new UnixSocketSink(sockPath, sendBufferCapacity: 64, flushIntervalMs: 50);
        sink.Start();
        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 2_000);

        var received = await ready.Task.WaitAsync(TimeSpan.FromSeconds(3));
        sink.ConsumerException.Should().BeNull("consumer must not crash on oversized payload");
        received.Should().Equal(payload);
        await serverTask;
        try { File.Delete(sockPath); } catch { }
    }

    [Fact(Skip = "linux/macos only — runs in CI on those platforms")]
    public async Task Enqueue_PayloadDeliveredWith4ByteBELengthPrefix()
    {
        string sockPath = Path.Combine(Path.GetTempPath(), "relay-" + Guid.NewGuid().ToString("N") + ".sock");
        var ready       = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

        var serverTask = Task.Run(() =>
        {
            using var server = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            server.Bind(new UnixDomainSocketEndPoint(sockPath));
            server.Listen(1);
            using var client = server.Accept();
            byte[] lenBuf = new byte[4];
            int read = client.Receive(lenBuf, 4, SocketFlags.None);
            if (read != 4) { ready.SetException(new Exception("short header")); return; }
            int len = (int)BinaryPrimitives.ReadUInt32BigEndian(lenBuf);
            byte[] payload = new byte[len];
            int got = 0;
            while (got < len) got += client.Receive(payload, got, len - got, SocketFlags.None);
            ready.SetResult(payload);
        });

        await Task.Delay(100);

        using var sink = new UnixSocketSink(sockPath, flushIntervalMs: 50);
        sink.Start();
        sink.Enqueue([5, 6, 7]);
        sink.Stop(drainTimeoutMs: 2_000);

        var received = await ready.Task.WaitAsync(TimeSpan.FromSeconds(3));
        received.Should().Equal(5, 6, 7);
        await serverTask;
        try { File.Delete(sockPath); } catch { }
    }
}
