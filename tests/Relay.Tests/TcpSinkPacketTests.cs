using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests;

public sealed class TcpSinkPacketTests : IDisposable
{
    private readonly TcpListener  _listener;
    private readonly int          _port;
    private readonly List<byte[]> _received = new();
    private readonly Thread       _serverThread;
    private volatile bool         _serverRunning = true;

    public TcpSinkPacketTests()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        _port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _serverThread = new Thread(ServerLoop) { IsBackground = true };
        _serverThread.Start();
    }

    public void Dispose()
    {
        _serverRunning = false;
        _listener.Stop();
        _serverThread.Join(500);
    }

    [Fact]
    public void Enqueue_PayloadDeliveredWith4ByteBigEndianLengthPrefix()
    {
        using var sink = new TcpSink("127.0.0.1", _port, flushIntervalMs: 50);
        sink.Start();
        byte[] payload = [10, 20, 30, 40];

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 2_000);

        WaitForReceived(count: 1);
        _received.Should().HaveCount(1);
        _received[0].Should().Equal(payload);
    }

    [Fact]
    public void Enqueue_MultiplePayloads_AllDeliveredInOrder()
    {
        using var sink = new TcpSink("127.0.0.1", _port, flushIntervalMs: 50);
        sink.Start();

        for (int i = 0; i < 5; i++)
            sink.Enqueue([(byte)i]);
        sink.Stop(drainTimeoutMs: 2_000);

        WaitForReceived(count: 5);
        _received.Should().HaveCount(5);
        for (int i = 0; i < 5; i++)
            _received[i][0].Should().Be((byte)i);
    }

    [Fact]
    public void IsHealthy_True_AfterSuccessfulConnect()
    {
        using var sink = new TcpSink("127.0.0.1", _port, flushIntervalMs: 100);
        sink.Start();
        Thread.Sleep(200);

        sink.IsHealthy.Should().BeTrue();
        sink.Stop();
    }

    [Fact]
    public void Accept_PayloadLargerThanBuffer_BypassesBuffer()
    {
        // sendBufferCapacity=64: a 256-byte payload (+ 4B header = 260B) exceeds buffer → bypass path.
        byte[] payload = new byte[256];
        for (int i = 0; i < payload.Length; i++) payload[i] = (byte)(i & 0xFF);

        using var sink = new TcpSink("127.0.0.1", _port, sendBufferCapacity: 64, flushIntervalMs: 50);
        sink.Start();

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 2_000);

        WaitForReceived(count: 1);
        sink.ConsumerException.Should().BeNull("consumer must not crash on oversized payload");
        _received.Should().HaveCount(1);
        _received[0].Should().Equal(payload);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var sink = new TcpSink("127.0.0.1", _port);
        sink.Start();
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }

    private void ServerLoop()
    {
        try
        {
            while (_serverRunning)
            {
                _listener.Server.Poll(50_000, SelectMode.SelectRead);
                if (!_serverRunning) break;
                if (!_listener.Pending()) continue;
                var client = _listener.AcceptTcpClient();
                var stream = client.GetStream();
                var lenBuf = new byte[4];
                try
                {
                    while (true)
                    {
                        int n = stream.Read(lenBuf, 0, 4);
                        if (n == 0) break;
                        int len = (int)BinaryPrimitives.ReadUInt32BigEndian(lenBuf);
                        var payload = new byte[len];
                        int read = 0;
                        while (read < len)
                            read += stream.Read(payload, read, len - read);
                        lock (_received) _received.Add(payload);
                    }
                }
                catch { }
                finally { client.Dispose(); }
            }
        }
        catch { }
    }

    private void WaitForReceived(int count, int timeoutMs = 3_000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            lock (_received)
                if (_received.Count >= count) return;
            Thread.Sleep(10);
        }
    }
}
