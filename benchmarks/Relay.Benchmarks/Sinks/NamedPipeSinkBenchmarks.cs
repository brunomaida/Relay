using System;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="NamedPipeSink"/> end-to-end Push throughput against a loopback
/// NamedPipe server. Windows-only.
/// </summary>
/// <remarks>
/// The server runs an accept loop in <see cref="GlobalSetup"/> so each benchmark invocation
/// (which creates a fresh sink/client) gets a new connection — <see cref="NamedPipeSink"/>
/// connects in its constructor, and the consumer thread closes the client on
/// <c>Dispose</c>, requiring a fresh server-side <c>WaitForConnection</c> per cycle.
/// </remarks>
[SupportedOSPlatform("windows")]
[MemoryDiagnoser]
public class NamedPipeSinkBenchmarks
{
    private CancellationTokenSource? _drainCts;
    private Task?                    _serverTask;
    private string                   _pipeName = string.Empty;
    private byte[]                   _payload  = null!;

    [Params(10_000, 100_000)]
    public int ItemCount;

    [GlobalSetup]
    public void Setup()
    {
        _payload = new byte[64];
        for (int i = 0; i < 64; i++) _payload[i] = (byte)i;
        _pipeName = "relay-bench-" + Guid.NewGuid().ToString("N");

        _drainCts   = new CancellationTokenSource();
        _serverTask = Task.Run(() => ServerLoop(_drainCts.Token));
    }

    private async Task ServerLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var server = new NamedPipeServerStream(
                    _pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                try
                {
                    await server.WaitForConnectionAsync(token).ConfigureAwait(false);
                    byte[] buf = new byte[4096];
                    while (!token.IsCancellationRequested)
                    {
                        int n = await server.ReadAsync(buf.AsMemory(), token).ConfigureAwait(false);
                        if (n == 0) break;
                    }
                }
                catch { /* drop client */ }
                finally { server.Dispose(); }
            }
        }
        catch { /* expected on cleanup */ }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _drainCts?.Cancel();
        _serverTask?.Wait(2_000);
        _drainCts?.Dispose();
    }

    [Benchmark]
    public void Push_Single()
    {
        using var sink = new NamedPipeSink(_pipeName);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
    }
}
