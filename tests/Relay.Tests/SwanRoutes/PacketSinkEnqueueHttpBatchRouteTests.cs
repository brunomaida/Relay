using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Relay.Sinks.Observability.Seq;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay that enter at <see cref="PacketSink.Enqueue"/> and exit through
/// <see cref="SeqSink"/> (an <c>HttpBatchSink</c>) to an in-memory <c>TestServer</c> (SWAN S5.5d): each builds the chain its
/// route's hops name, calls <c>Enqueue</c> in its body, drives the chain to the exit and asserts the POST bodies in its body.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PacketSinkEnqueueHttpBatchRouteTests
{
    private const int    TimeoutMs    = 5_000;
    private const int    NoDeadlineMs = 60_000; // no deadline flush runs within a test
    private const string SeqPath      = "/api/events/raw";

    /// <summary>
    /// Route <c>rp29</c>: <c>Enqueue &gt; SpscByteRingBuffer publish/consume &gt; ConsumeLoop &gt; BatchSink.WriteToBackend &gt;
    /// HttpBatchSink.OnFlush &gt; out.seq</c>. The second event does not fit the batch, so <c>WriteToBackend</c> posts the first;
    /// no deadline or <c>Stop</c> flush runs before the check.
    /// </summary>
    [Fact]
    public void Enqueue_SeqSinkEventOverflowsBatch_WriteToBackendPostsFirstBatch()
    {
        byte[] first  = "{\"@m\":\"first\"}\n"u8.ToArray();
        byte[] second = "{\"@m\":\"second\"}\n"u8.ToArray();
        var received = new ConcurrentQueue<(string Path, byte[] Body)>();
        using var host = StartServer(received);
        using var http = host.GetTestServer().CreateClient();
        using var sink = new SeqSink(http, "http://localhost", ringCapacity: 4_096, batchCapacity: 24, flushIntervalMs: NoDeadlineMs);
        sink.Start();

        sink.Enqueue(first);
        sink.Enqueue(second);
        SpinWait.SpinUntil(() => !received.IsEmpty, TimeoutMs).Should().BeTrue("WriteToBackend posts the first batch when the second event does not fit");
        received.TryPeek(out var firstPost).Should().BeTrue();
        firstPost.Path.Should().Be(SeqPath);
        firstPost.Body.Should().Equal(first);
        sink.Stop(drainTimeoutMs: 2_000);

        var posts = received.ToArray();
        posts.Should().HaveCount(2, "the consumer's final flush posts the second batch");
        posts[1].Body.Should().Equal(second);
    }

    /// <summary>
    /// Route <c>scan.in.app.out.seq.eb202b39</c>: the event fits the batch, so only the consumer loop's
    /// <c>BatchSink.FlushBackend &gt; FlushScratch &gt; HttpBatchSink.OnFlush</c> posts it.
    /// </summary>
    [Fact]
    public void Enqueue_SeqSinkEventFitsBatch_ConsumerFlushPostsBatch()
    {
        byte[] payload = "{\"@m\":\"event\"}\n"u8.ToArray();
        var received = new ConcurrentQueue<(string Path, byte[] Body)>();
        using var host = StartServer(received);
        using var http = host.GetTestServer().CreateClient();
        using var sink = new SeqSink(http, "http://localhost", ringCapacity: 4_096, flushIntervalMs: 50);
        sink.Start();

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 2_000);

        var posts = received.ToArray();
        posts.Should().ContainSingle("the consumer loop flushes the batch once");
        posts[0].Path.Should().Be(SeqPath);
        posts[0].Body.Should().Equal(payload);
    }

    private static IHost StartServer(ConcurrentQueue<(string Path, byte[] Body)> received) =>
        new HostBuilder().ConfigureWebHost(web =>
        {
            web.UseTestServer();
            web.Configure(app => app.Run(async ctx =>
            {
                using var body = new MemoryStream();
                await ctx.Request.Body.CopyToAsync(body);
                received.Enqueue((ctx.Request.Path.Value ?? string.Empty, body.ToArray()));
            }));
        }).Start();
}
