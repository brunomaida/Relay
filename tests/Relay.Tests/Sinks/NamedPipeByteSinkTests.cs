using System;
using System.Buffers.Binary;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.Sinks;

public sealed class NamedPipeByteSinkTests
{
    [Fact]
    public async Task Enqueue_PayloadDeliveredWith4ByteBELengthPrefix()
    {
        string name = "relay-test-" + Guid.NewGuid().ToString("N");
        var ready   = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

        var serverTask = Task.Run(() =>
        {
            using var server = new NamedPipeServerStream(name, PipeDirection.In, 1,
                                                         PipeTransmissionMode.Byte, PipeOptions.None);
            server.WaitForConnection();
            byte[] lenBuf = new byte[4];
            int read = server.Read(lenBuf, 0, 4);
            if (read != 4) { ready.SetException(new Exception("short header")); return; }
            int len = (int)BinaryPrimitives.ReadUInt32BigEndian(lenBuf);
            byte[] payload = new byte[len];
            int got = 0;
            while (got < len) got += server.Read(payload, got, len - got);
            ready.SetResult(payload);
        });

        await Task.Delay(100);

        using var sink = new NamedPipeByteSink(name, flushIntervalMs: 50);
        sink.Start();
        sink.Enqueue([10, 20, 30]);
        sink.Stop(drainTimeoutMs: 2_000);

        var received = await ready.Task.WaitAsync(TimeSpan.FromSeconds(3));
        received.Should().Equal(10, 20, 30);
        await serverTask;
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var sink = new NamedPipeByteSink("relay-disp-" + Guid.NewGuid().ToString("N"));
        sink.Start();
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }
}
