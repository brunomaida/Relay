using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Relay.Sinks.Http;
using Xunit;

namespace Relay.Tests.Sinks.Http;

/// <remarks>
/// Tests use Microsoft.AspNetCore.TestHost (in-memory pipeline). Real-world breaker tuning
/// (cbFailures, cbOpenDurationMs, flushIntervalMs) should be validated against a real socket
/// fixture in a future endurance test phase.
/// </remarks>
public sealed class HttpBatchSinkTests
{
    private sealed class TestHttpBatchSink : HttpBatchSink
    {
        public TestHttpBatchSink(HttpClient client, Uri endpoint)
            : base(client, endpoint,
                   ringCapacity: 4096, batchCapacity: 256, flushIntervalMs: 50,
                   cbFailures: 3, cbOpenDurationMs: 100, sinkName: "test") { }

        protected override string ContentType => "application/octet-stream";
    }

    private static (TestServer server, List<byte[]> received) BuildServer(HttpStatusCode status = HttpStatusCode.OK)
    {
        var received = new List<byte[]>();
        var hostBuilder = new HostBuilder().ConfigureWebHost(web =>
        {
            web.UseTestServer();
            web.Configure(app => app.Run(async ctx =>
            {
                using var ms = new MemoryStream();
                await ctx.Request.Body.CopyToAsync(ms);
                lock (received) received.Add(ms.ToArray());
                ctx.Response.StatusCode = (int)status;
            }));
        });
        var host = hostBuilder.Start();
        return (host.GetTestServer(), received);
    }

    private static void WaitFor(Func<bool> predicate, int timeoutMs = 1_000, int pollMs = 10)
    {
        var deadline = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < deadline)
        {
            if (predicate()) return;
            Thread.Sleep(pollMs);
        }
        if (!predicate()) throw new TimeoutException($"Predicate did not become true within {timeoutMs}ms");
    }

    [Fact]
    public void Posts_accumulated_batch_to_endpoint()
    {
        var (server, received) = BuildServer();
        var client = server.CreateClient();
        using var sink = new TestHttpBatchSink(client, new Uri("http://localhost/ingest"));
        sink.Start();

        sink.Enqueue(new byte[] { 0x01, 0x02 });
        sink.Enqueue(new byte[] { 0x03 });
        sink.Flush();
        WaitFor(() => received.Count >= 1, timeoutMs: 1_000);

        received.Should().ContainSingle();
        received[0].Should().Equal(new byte[] { 0x01, 0x02, 0x03 });
    }

    [Fact]
    public void Opens_breaker_after_consecutive_5xx_failures()
    {
        var (server, received) = BuildServer(HttpStatusCode.InternalServerError);
        var client = server.CreateClient();
        using var sink = new TestHttpBatchSink(client, new Uri("http://localhost/ingest"));
        sink.Start();

        // Drive failures one at a time, waiting for each to be processed before the next.
        for (int i = 0; i < 3; i++)
        {
            long before = sink.HttpFailureCount;
            sink.Enqueue(new byte[] { (byte)i });
            sink.Flush();
            WaitFor(() => sink.HttpFailureCount > before, timeoutMs: 1_000);
        }

        var requestsBefore = received.Count;
        var droppedBefore = sink.DroppedBatchCount;

        // Breaker open — next enqueue + flush should NOT hit the server.
        sink.Enqueue(new byte[] { 0xFF });
        sink.Flush();
        WaitFor(() => sink.DroppedBatchCount > droppedBefore, timeoutMs: 1_000);

        received.Count.Should().Be(requestsBefore, "breaker open suppresses the request");
        sink.BreakerOpenCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Closes_breaker_after_open_duration_when_probe_succeeds()
    {
        int callCount = 0;
        HttpStatusCode CurrentStatus() => Interlocked.Increment(ref callCount) <= 3
            ? HttpStatusCode.InternalServerError
            : HttpStatusCode.OK;

        var hostBuilder = new HostBuilder().ConfigureWebHost(web =>
        {
            web.UseTestServer();
            web.Configure(app => app.Run(ctx =>
            {
                ctx.Response.StatusCode = (int)CurrentStatus();
                return Task.CompletedTask;
            }));
        });
        using var host = hostBuilder.Start();
        var server = host.GetTestServer();

        using var sink = new TestHttpBatchSink(server.CreateClient(), new Uri("http://localhost/ingest"));
        sink.Start();

        // Drive 3 failures to open breaker, one at a time.
        for (int i = 0; i < 3; i++)
        {
            long before = sink.HttpFailureCount;
            sink.Enqueue(new byte[] { 1 });
            sink.Flush();
            WaitFor(() => sink.HttpFailureCount > before, timeoutMs: 1_000);
        }
        sink.BreakerOpenCount.Should().Be(1);

        // Sleep past breaker open duration (100ms).
        Thread.Sleep(150);

        // First flush after window = half-open probe; succeeds (200), closes breaker.
        sink.Enqueue(new byte[] { 2 }); sink.Flush();
        WaitFor(() => callCount >= 4, timeoutMs: 1_000);
        int afterProbe = callCount;

        // Subsequent flush should pass through (breaker closed).
        sink.Enqueue(new byte[] { 3 }); sink.Flush();
        WaitFor(() => callCount > afterProbe, timeoutMs: 1_000);

        callCount.Should().BeGreaterThan(afterProbe, "breaker closed on probe success; subsequent requests flow");
        sink.BreakerOpenCount.Should().Be(1, "breaker only opened once across the test");
    }
}
