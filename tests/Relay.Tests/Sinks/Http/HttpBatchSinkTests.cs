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

    [Fact]
    public void Posts_accumulated_batch_to_endpoint()
    {
        var (server, received) = BuildServer();
        var client = server.CreateClient();
        using var sink = new TestHttpBatchSink(client, new Uri("http://localhost/ingest"));
        sink.Start();

        sink.Enqueue(new byte[] { 0x01, 0x02 });
        sink.Enqueue(new byte[] { 0x03 });
        Thread.Sleep(150);  // exceed flushIntervalMs

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

        for (int i = 0; i < 3; i++)
        {
            sink.Enqueue(new byte[] { (byte)i });
            sink.Flush();
            Thread.Sleep(80);
        }

        var requestsBefore = received.Count;

        // Breaker is open — next enqueue+flush should NOT hit the server.
        sink.Enqueue(new byte[] { 0xFF });
        sink.Flush();
        Thread.Sleep(80);

        received.Count.Should().Be(requestsBefore, "breaker should be open and suppress the request");
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

        for (int i = 0; i < 3; i++) { sink.Enqueue(new byte[] { 1 }); sink.Flush(); Thread.Sleep(80); }
        Thread.Sleep(150);  // exceed cbOpenDurationMs (100ms)
        sink.Enqueue(new byte[] { 2 }); sink.Flush(); Thread.Sleep(80);
        int afterProbe = callCount;

        sink.Enqueue(new byte[] { 3 }); sink.Flush(); Thread.Sleep(80);
        callCount.Should().BeGreaterThan(afterProbe, "breaker should be closed and forward subsequent requests");
    }
}
