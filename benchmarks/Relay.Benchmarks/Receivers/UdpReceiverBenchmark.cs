using System.Net;
using System.Net.Sockets;
using BenchmarkDotNet.Attributes;
using Relay.Receivers;

namespace Relay.Benchmarks.Receivers;

/// <summary>
/// Measures <see cref="UdpReceiver{TState}.Poll"/> hot path:
/// <c>Socket.Poll(0, SelectRead)</c> + non-blocking <c>Receive</c> + callback dispatch.
/// </summary>
/// <remarks>
/// Two states benchmarked:
/// <list type="bullet">
///   <item><c>Poll_Empty</c> — no datagram queued; measures pure overhead of the early-exit gate.</item>
///   <item><c>Roundtrip_PerFrame</c> — sender sends, receiver polls; includes one loopback send.</item>
/// </list>
/// </remarks>
[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(5)]
public class UdpReceiverBenchmark
{
    private static readonly byte[] _payload = new byte[128];

    private UdpClient?                            _sender;
    private UdpReceiver<UdpReceiverBenchmark>?    _receiver;
    private IPEndPoint?                           _localEp;
    private int                                   _received;

    [GlobalSetup]
    public void Setup()
    {
        _receiver = new UdpReceiver<UdpReceiverBenchmark>(
            new IPEndPoint(IPAddress.Loopback, 0),
            this,
            static (s, _) => s._received++,
            kernelBufferSize: 1 << 22);
        _localEp = _receiver.LocalEndPoint;

        _sender = new UdpClient(AddressFamily.InterNetwork);
        _sender.Client.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        _sender.Client.SendBufferSize = 1 << 22;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _receiver?.Dispose();
        _sender?.Dispose();
    }

    [Benchmark]
    public bool Poll_Empty() => _receiver!.Poll();

    [Benchmark]
    public bool Roundtrip_PerFrame()
    {
        _sender!.Send(_payload, _payload.Length, _localEp);
        bool got;
        while (!(got = _receiver!.Poll())) { /* spin until kernel hands the datagram up */ }
        return got;
    }
}
