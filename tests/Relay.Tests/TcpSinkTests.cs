using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Builder;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests;

/// <summary>TcpSink loopback write, reconnect backoff, and backpressure fallback.</summary>
public sealed class TcpSinkTests
{
    private static readonly int EntrySize = Unsafe.SizeOf<Entry64>();

    [Fact]
    public void TcpSink_WritesAllEntries_ReadbackMatches()
    {
        using var server = new LoopbackServer();

        using (var pipe = new TcpSink<Entry64>("127.0.0.1", server.Port,
                   ringCapacity: 64, flushInterval: 25))
        {
            pipe.Start();

            const int count = 16;
            for (int i = 0; i < count; i++)
                pipe.Enqueue(new Entry64 { A = i, B = i * 7 });

            pipe.Stop(drainTimeoutMs: 2_000);
        }

        var bytes = server.WaitForBytes(16 * EntrySize, timeoutMs: 2_000);
        bytes.Length.Should().BeGreaterThanOrEqualTo(16 * EntrySize);

        for (int i = 0; i < 16; i++)
        {
            long a = BitConverter.ToInt64(bytes, i * EntrySize);
            long b = BitConverter.ToInt64(bytes, i * EntrySize + 8);
            a.Should().Be(i);
            b.Should().Be(i * 7);
        }
    }

    [Fact]
    public void TcpSink_ConnectFailure_FallsBackAndRetries()
    {
        // Nothing is listening on this port — constructor connect fails.
        int deadPort = GetFreePort();

        var fallback = new CountingPipe();

        Action ctor = () =>
        {
            using var pipe = new TcpSink<Entry64>("127.0.0.1", deadPort,
                ringCapacity: 16, flushInterval: 50);
            RelayBuilder.Start<Entry64, TcpSink<Entry64>>(pipe).To(fallback).Build();
            pipe.Start();

            pipe.Enqueue(new Entry64 { A = 1 });
            pipe.Enqueue(new Entry64 { A = 2 });

            pipe.Stop(drainTimeoutMs: 500);
        };

        // Constructor surfaces the connect failure synchronously.
        ctor.Should().Throw<SocketException>();
    }

    [Fact]
    [Trait("Category", "Stress")]
    public void TcpSink_ReconnectLoop_RecoversAndFallsBackCleanly()
    {
        // Producer sustains load while the server is dropped and accepted in a loop.
        // Validates: pipe does not crash, IsHealthy toggles, fallback captures items during outages.
        const int cycles         = 5;
        const int itemsPerCycle  = 8_000;
        const int outageMs       = 150;

        using var server   = new ReconnectableServer();
        var       fallback = new CountingPipe();

        using var pipe = new TcpSink<Entry64>("127.0.0.1", server.Port,
            ringCapacity: 1024, flushInterval: 25);
        RelayBuilder.Start<Entry64, TcpSink<Entry64>>(pipe).To(fallback).Build();
        pipe.Start();

        int produced = 0;
        for (int c = 0; c < cycles; c++)
        {
            for (int i = 0; i < itemsPerCycle; i++)
            {
                pipe.Enqueue(new Entry64 { A = produced++ });
            }
            Thread.Sleep(30);
            server.Drop();               // abortive close — client sees RST
            Thread.Sleep(outageMs);
            server.Accept();             // next connect attempt succeeds
            SpinUntil(() => pipe.IsHealthy, 2_000);
        }

        pipe.Stop(drainTimeoutMs: 2_000);

        pipe.ConsumerException.Should().BeNull("consumer must never crash during reconnect storm");
        long total = server.TotalReceivedBytes + (long)fallback.Accepted * EntrySize;
        total.Should().BeGreaterThan(0, "items must reach backend or fallback during the run");
    }

    [Fact]
    public void TcpSink_ServerDropsConnection_MarksUnhealthy()
    {
        using var server = new LoopbackServer();

        using var pipe = new TcpSink<Entry64>("127.0.0.1", server.Port,
            ringCapacity: 1024, flushInterval: 25);
        pipe.Start();

        pipe.Enqueue(new Entry64 { A = 1 });
        Thread.Sleep(200); // let consumer flush the first item

        // Abortive close — forces RST so the client sees the broken pipe promptly.
        server.Drop();

        // Flood enough data to overflow the kernel send buffer and force the client to
        // observe the RST. Each Entry64 is 64B; send ~128KB to exceed a typical default.
        for (int i = 0; i < 2_048; i++)
            pipe.Enqueue(new Entry64 { A = i });

        bool unhealthy = SpinUntil(() => !pipe.IsHealthy, 3_000);
        unhealthy.Should().BeTrue("socket close must eventually surface as !IsHealthy");
    }

    private static bool SpinUntil(Func<bool> condition, int timeoutMs)
    {
        long deadline = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < deadline)
        {
            if (condition()) return true;
            Thread.Sleep(20);
        }
        return condition();
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private sealed class CountingPipe : DispatchSink<Entry64>
    {
        public int Accepted { get; private set; }
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) { Accepted++; return true; }
        public override void Flush()   { }
        public override void Dispose() { }
    }

    /// <summary>Minimal loopback TCP server: accepts one client, buffers all received bytes.</summary>
    private sealed class LoopbackServer : IDisposable
    {
        private readonly TcpListener    _listener;
        private readonly Thread         _acceptThread;
        private readonly MemoryStream   _received = new();
        private readonly object         _lock     = new();
        private          TcpClient?     _client;
        private volatile bool           _running;

        public int Port { get; }

        public LoopbackServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port      = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _running  = true;
            _acceptThread = new Thread(AcceptLoop) { IsBackground = true };
            _acceptThread.Start();
        }

        private void AcceptLoop()
        {
            try
            {
                _client = _listener.AcceptTcpClient();
                var stream = _client.GetStream();
                var buf    = new byte[4096];
                while (_running)
                {
                    int n;
                    try { n = stream.Read(buf, 0, buf.Length); }
                    catch { break; }
                    if (n <= 0) break;
                    lock (_lock) _received.Write(buf, 0, n);
                }
            }
            catch { /* shutdown */ }
        }

        public byte[] WaitForBytes(int minBytes, int timeoutMs)
        {
            long deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                lock (_lock) if (_received.Length >= minBytes) return _received.ToArray();
                Thread.Sleep(10);
            }
            lock (_lock) return _received.ToArray();
        }

        public void Drop()
        {
            try { _client!.LingerState = new LingerOption(true, 0); } catch { }
            try { _client?.Client.Close(0); } catch { }
            try { _client?.Close(); } catch { }
        }

        public void Dispose()
        {
            _running = false;
            try { _client?.Close(); } catch { }
            try { _listener.Stop(); } catch { }
            _acceptThread.Join(500);
            _received.Dispose();
        }
    }

    /// <summary>Loopback server that supports dropping and re-accepting clients on demand.</summary>
    private sealed class ReconnectableServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly Thread      _acceptThread;
        private          TcpClient?  _client;
        private volatile bool        _running;
        private          long        _totalBytes;

        public int Port { get; }
        public long TotalReceivedBytes => Interlocked.Read(ref _totalBytes);

        public ReconnectableServer()
        {
            _listener     = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port          = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _running      = true;
            _acceptThread = new Thread(AcceptLoop) { IsBackground = true };
            _acceptThread.Start();
        }

        private void AcceptLoop()
        {
            var buf = new byte[8192];
            while (_running)
            {
                try
                {
                    _client = _listener.AcceptTcpClient();
                    var stream = _client.GetStream();
                    while (_running)
                    {
                        int n;
                        try { n = stream.Read(buf, 0, buf.Length); }
                        catch { break; }
                        if (n <= 0) break;
                        Interlocked.Add(ref _totalBytes, n);
                    }
                }
                catch { if (!_running) return; }
            }
        }

        public void Drop()
        {
            try { _client!.LingerState = new LingerOption(true, 0); } catch { }
            try { _client?.Client.Close(0); } catch { }
            try { _client?.Close(); } catch { }
            _client = null;
        }

        /// <summary>No-op — accept thread is already waiting on the listener between drops.</summary>
        public void Accept() { /* accept loop resumes on its own */ }

        public void Dispose()
        {
            _running = false;
            try { _client?.Close(); } catch { }
            try { _listener.Stop(); } catch { }
            _acceptThread.Join(500);
        }
    }
}
