using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.PacketSinks;

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(5)]
public class TcpSinkBenchmark
{
    private TcpListener?             _listener;
    private TcpClient?               _server;
    private CancellationTokenSource? _drainCts;
    private Task?                    _drainTask;
    private TcpSink?                 _sink;
    private byte[]                   _payload = new byte[128];

    [GlobalSetup]
    public void Setup()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        int port = ((IPEndPoint)_listener.LocalEndpoint).Port;

        // Kick off accept BEFORE starting the client sink so Connect() sees a listener backlog.
        var acceptTask = _listener.AcceptTcpClientAsync();

        _sink = new TcpSink("127.0.0.1", port);
        _sink.Start();

        // Block until the server side of the connection is established.
        _server    = acceptTask.GetAwaiter().GetResult();
        _drainCts  = new CancellationTokenSource();
        _drainTask = Task.Run(() => DrainLoop(_server, _drainCts.Token));
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
        _sink?.Stop(1_000);
        _server?.Close();
        _listener?.Stop();
        _listener?.Server.Dispose();
        _drainTask?.Wait(500);
        _drainCts?.Dispose();
    }

    private const int BatchSize = 1024;

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void TcpSink_Enqueue_128B()
    {
        for (int i = 0; i < BatchSize; i++)
            _sink!.Enqueue(_payload);
    }
}
