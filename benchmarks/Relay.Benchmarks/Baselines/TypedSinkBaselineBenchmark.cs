using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Baselines;

/// <summary>Baseline: typed sink throughput before PacketSink changes.</summary>
[MemoryDiagnoser]
public class TypedSinkBaselineBenchmark
{
    private TcpListener?             _listener;
    private TcpClient?               _server;
    private CancellationTokenSource? _drainCts;
    private Task?                    _drainTask;
    private TcpSink<Event64>?        _tcpSink;
    private Event64                  _event;

    [GlobalSetup]
    public void Setup()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        int port = ((IPEndPoint)_listener.LocalEndpoint).Port;

        // Kick off accept BEFORE starting the client sink so Connect() sees a listener backlog.
        var acceptTask = _listener.AcceptTcpClientAsync();

        _tcpSink = new TcpSink<Event64>("127.0.0.1", port);
        _tcpSink.Start();

        // Block until the server side of the connection is established.
        _server    = acceptTask.GetAwaiter().GetResult();
        _drainCts  = new CancellationTokenSource();
        _drainTask = Task.Run(() => DrainLoop(_server, _drainCts.Token));

        _event = new Event64 { Value = 42L };
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
        _tcpSink?.Stop(1_000);
        _server?.Close();
        _listener?.Stop();
        _listener?.Server.Dispose();
        _drainTask?.Wait(500);
        _drainCts?.Dispose();
    }

    private const int BatchSize = 1024;

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    public void TcpSinkTyped_Enqueue()
    {
        for (int i = 0; i < BatchSize; i++)
            _tcpSink!.Enqueue(in _event);
    }

    [System.Runtime.InteropServices.StructLayout(
        System.Runtime.InteropServices.LayoutKind.Sequential, Size = 64)]
    private struct Event64 { public long Value; }
}
