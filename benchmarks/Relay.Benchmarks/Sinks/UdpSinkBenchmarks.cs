using System;
using System.Net;
using System.Net.Sockets;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="UdpSink"/> end-to-end Push throughput. Validates the cost-map claim
/// that <c>Socket.Send</c>-per-record caps throughput at ~1.5M payloads/s/core.
/// </summary>
[MemoryDiagnoser]
public class UdpSinkBenchmarks
{
    private Socket? _receiver;
    private int     _port;
    private byte[]  _payload = null!;

    [Params(100_000, 1_000_000)]
    public int ItemCount;

    [GlobalSetup]
    public void Setup()
    {
        _payload = new byte[64];
        for (int i = 0; i < 64; i++) _payload[i] = (byte)i;

        // Bind a loopback UDP receiver so the OS does not RST-port-unreachable.
        _receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _receiver.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        _port = ((IPEndPoint)_receiver.LocalEndPoint!).Port;
        // Increase receive buffer so kernel does not back-pressure under burst.
        _receiver.ReceiveBufferSize = 8 * 1024 * 1024;
    }

    [GlobalCleanup]
    public void Cleanup() => _receiver?.Dispose();

    [Benchmark]
    public void Push_Single()
    {
        using var sink = new UdpSink("127.0.0.1", _port,
                                     maxPayload:      65_507,
                                     ringCapacity:    65_536,
                                     flushIntervalMs: 100);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
    }
}
