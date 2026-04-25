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
