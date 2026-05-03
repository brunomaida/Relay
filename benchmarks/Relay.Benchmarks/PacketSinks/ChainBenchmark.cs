using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Relay;
using Relay.Builder;
using Relay.Sinks;

namespace Relay.Benchmarks.PacketSinks;

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(5)]
public class ChainBenchmark
{
    private TcpSink?    _tcp;
    private MemorySink? _ram;
    private TcpSink?  _tcp2;
    private TcpSink?  _tcp3;
    private TcpListener? _l1, _l2, _l3;
    private TcpClient?   _s1, _s2, _s3;
    private CancellationTokenSource? _drainCts;
    private Task?        _drainTask1, _drainTask2, _drainTask3;
    private byte[]       _payload = new byte[128];

    [GlobalSetup]
    public void Setup()
    {
        _l1 = MakeListener(); _l2 = MakeListener(); _l3 = MakeListener();

        // Kick off all three accepts before starting client sinks.
        var a1 = _l1.AcceptTcpClientAsync();
        var a2 = _l2.AcceptTcpClientAsync();
        var a3 = _l3.AcceptTcpClientAsync();

        _tcp  = new TcpSink("127.0.0.1", Port(_l1));
        _ram  = new MemorySink();
        _tcp2 = new TcpSink("127.0.0.1", Port(_l2));
        _tcp3 = new TcpSink("127.0.0.1", Port(_l3));

        SinkChainBuilder.Start(_tcp).To(_ram);
        _tcp.Start();
        _tcp2.Start();
        _tcp3.Start();

        // Block until all three server-side connections are established.
        _s1 = a1.GetAwaiter().GetResult();
        _s2 = a2.GetAwaiter().GetResult();
        _s3 = a3.GetAwaiter().GetResult();

        _drainCts  = new CancellationTokenSource();
        _drainTask1 = Task.Run(() => DrainLoop(_s1, _drainCts.Token));
        _drainTask2 = Task.Run(() => DrainLoop(_s2, _drainCts.Token));
        _drainTask3 = Task.Run(() => DrainLoop(_s3, _drainCts.Token));
    }

    private static void DrainLoop(TcpClient client, CancellationToken token)
    {
        byte[] buf = new byte[4096];
        var stream = client.GetStream();
        try
        {
            while (!token.IsCancellationRequested)
            {
                int n = stream.Read(buf, 0, buf.Length);
                if (n == 0) break;
            }
        }
        catch { /* socket torn down on cleanup — expected */ }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _drainCts?.Cancel();
        _tcp?.Stop(500); _tcp2?.Stop(500); _tcp3?.Stop(500);
        _s1?.Close(); _s2?.Close(); _s3?.Close();
        _l1?.Stop(); _l1?.Server.Dispose();
        _l2?.Stop(); _l2?.Server.Dispose();
        _l3?.Stop(); _l3?.Server.Dispose();
        _drainTask1?.Wait(500); _drainTask2?.Wait(500); _drainTask3?.Wait(500);
        _drainCts?.Dispose();
    }

    private const int BatchSize = 1024;

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    public void TcpSink_NoPropagation()
    {
        for (int i = 0; i < BatchSize; i++)
            _tcp!.Enqueue(_payload);
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void FanOut_2Sinks()
    {
        for (int i = 0; i < BatchSize; i++)
        {
            _tcp2!.Enqueue(_payload);
            _tcp3!.Enqueue(_payload);
        }
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void SerializeSink_Overhead()
    {
        // MemoryMarshal.AsBytes bridge overhead vs direct packet enqueue
        var bridge = new SerializeSink<long>(_tcp!);
        long val = 42L;
        for (int i = 0; i < BatchSize; i++)
            bridge.Enqueue(in val);
    }

    private static TcpListener MakeListener()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        return l;
    }

    private static int Port(TcpListener l)
        => ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
}
