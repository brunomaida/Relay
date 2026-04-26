using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Relay.Sinks.Observability.Seq;
using Xunit;

namespace Relay.Tests.Sinks.Observability.Seq;

/// <remarks>
/// Tests use Microsoft.AspNetCore.TestHost (in-memory pipeline). Real-world wire validation
/// happens in SeqIntegrationTests (Testcontainers `datalust/seq:latest`).
/// </remarks>
public sealed class SeqSinkTests
{
    private static (TestServer server, List<HttpRequestSnapshot> snapshots) BuildServer()
    {
        var snapshots = new List<HttpRequestSnapshot>();
        var hostBuilder = new HostBuilder().ConfigureWebHost(web =>
        {
            web.UseTestServer();
            web.Configure(app => app.Run(async ctx =>
            {
                var snap = new HttpRequestSnapshot(
                    ctx.Request.Path.Value ?? "",
                    ctx.Request.ContentType ?? "",
                    ctx.Request.Headers.TryGetValue("X-Seq-ApiKey", out var k) ? k.ToString() : null);
                lock (snapshots) snapshots.Add(snap);
                ctx.Response.StatusCode = 200;
                await ctx.Response.WriteAsync("ok");
            }));
        });
        var host = hostBuilder.Start();
        return (host.GetTestServer(), snapshots);
    }

    private sealed record HttpRequestSnapshot(string Path, string ContentType, string? ApiKey);

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
    public void Posts_to_seq_raw_endpoint_with_clef_content_type_and_api_key()
    {
        var (server, snapshots) = BuildServer();
        using var sink = new SeqSink(
            http: server.CreateClient(),
            serverUrl: "http://localhost",
            apiKey: "test-key-123",
            ringCapacity: 4096, batchCapacity: 256, flushIntervalMs: 50,
            cbFailures: 3, cbOpenDurationMs: 100);
        sink.Start();

        sink.Enqueue(new byte[] { (byte)'{', (byte)'}' });
        sink.Flush();
        WaitFor(() => snapshots.Count >= 1);

        snapshots.Should().ContainSingle();
        snapshots[0].Path.Should().Be("/api/events/raw");
        snapshots[0].ContentType.Should().Be("application/vnd.serilog.clef");
        snapshots[0].ApiKey.Should().Be("test-key-123");
    }

    [Fact]
    public void Omits_api_key_header_when_apiKey_is_null()
    {
        var (server, snapshots) = BuildServer();
        using var sink = new SeqSink(server.CreateClient(), "http://localhost", apiKey: null,
            ringCapacity: 4096, batchCapacity: 256, flushIntervalMs: 50,
            cbFailures: 3, cbOpenDurationMs: 100);
        sink.Start();
        sink.Enqueue(new byte[] { (byte)'{' });
        sink.Flush();
        WaitFor(() => snapshots.Count >= 1);

        snapshots.Should().ContainSingle();
        snapshots[0].ApiKey.Should().BeNull();
    }
}
