using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="UnixSocketSink"/> end-to-end Push throughput against a loopback
/// unix-domain socket server. Linux/macOS-only — Windows runs of the BDN suite skip this
/// class because the platform-gate keeps the BDN runner from picking it up.
/// </summary>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[MemoryDiagnoser]
public class UnixSocketSinkBenchmarks
{
    private Socket?                  _listener;
    private CancellationTokenSource? _drainCts;
    private Task?                    _serverTask;
    private string                   _path    = string.Empty;
    private byte[]                   _payload = null!;

    [Params(100_000, 1_000_000)]
    public int ItemCount;

    [GlobalSetup]
    public void Setup()
    {
        _payload = new byte[64];
        for (int i = 0; i < 64; i++) _payload[i] = (byte)i;
        _path    = Path.Combine(Path.GetTempPath(), "relay-bench-" + Guid.NewGuid().ToString("N") + ".sock");

        _listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        _listener.Bind(new UnixDomainSocketEndPoint(_path));
        _listener.Listen(8);

        _drainCts   = new CancellationTokenSource();
        _serverTask = Task.Run(() => ServerLoop(_drainCts.Token));
    }

    private async Task ServerLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                Socket client;
                try { client = await _listener!.AcceptAsync(token).ConfigureAwait(false); }
                catch { break; }
                _ = Task.Run(async () =>
                {
                    byte[] buf = new byte[4096];
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            int n = await client.ReceiveAsync(buf, SocketFlags.None, token).ConfigureAwait(false);
                            if (n == 0) break;
                        }
                    }
                    catch { /* expected on cleanup */ }
                    finally { client.Dispose(); }
                });
            }
        }
        catch { /* expected on cleanup */ }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _drainCts?.Cancel();
        _listener?.Dispose();
        _serverTask?.Wait(2_000);
        _drainCts?.Dispose();
        try { File.Delete(_path); } catch { /* best-effort */ }
    }

    [Benchmark]
    public void Push_Single()
    {
        using var sink = new UnixSocketSink(_path);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
    }
}
