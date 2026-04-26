using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Relay.Internal;

namespace Relay.Sinks.Http;

/// <summary>
/// Abstract <see cref="BatchSink"/> that POSTs the accumulated batch to an HTTP endpoint with
/// a circuit breaker. Subclasses supply the endpoint, content-type, and per-request headers.
/// </summary>
/// <remarks>
/// Failure model: HTTP 5xx, network errors, and 4xx all increment a consecutive failure counter.
/// After <c>cbFailures</c> consecutive failures, the breaker opens for <c>cbOpenDurationMs</c> —
/// flushes during open are dropped without an outgoing request. First flush after the duration
/// passes is the half-open probe; success closes the breaker.
/// </remarks>
public abstract class HttpBatchSink : BatchSink
{
    private readonly HttpClient _http;
    private readonly Uri        _endpoint;
    private readonly int        _cbFailures;
    private readonly long       _cbOpenDurationTicks;

    private int  _failures;
    private long _breakerOpenUntilTicks;

    /// <param name="http">Shared HttpClient. Caller owns disposal.</param>
    /// <param name="endpoint">Absolute target URI for POST.</param>
    /// <param name="ringCapacity">SPSC ring capacity (power of two, bytes).</param>
    /// <param name="batchCapacity">Scratch buffer capacity (bytes, POH-pinned).</param>
    /// <param name="flushIntervalMs">Max ms between forced flushes.</param>
    /// <param name="cbFailures">Consecutive flush failures before breaker opens.</param>
    /// <param name="cbOpenDurationMs">Breaker open duration before half-open probe.</param>
    /// <param name="sinkName">Diagnostic thread suffix.</param>
    protected HttpBatchSink(
        HttpClient http,
        Uri        endpoint,
        int        ringCapacity,
        int        batchCapacity,
        int        flushIntervalMs,
        int        cbFailures,
        int        cbOpenDurationMs,
        string     sinkName)
        : base(ringCapacity, batchCapacity, flushIntervalMs, sinkName)
    {
        _http                = http ?? throw new ArgumentNullException(nameof(http));
        _endpoint            = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _cbFailures          = cbFailures;
        _cbOpenDurationTicks = (long)cbOpenDurationMs * (Stopwatch.Frequency / 1_000);
    }

    /// <summary>HTTP Content-Type header value for the POST body.</summary>
    protected abstract string ContentType { get; }

    /// <summary>Optional hook to add per-request headers (auth, vendor-specific).</summary>
    protected virtual void ConfigureRequest(HttpRequestMessage request) { }

    /// <inheritdoc/>
    protected sealed override void OnFlush(ReadOnlySpan<byte> batch)
    {
        if (HfClock.NowTicks < Volatile.Read(ref _breakerOpenUntilTicks))
            return;  // breaker open — drop counted at base PacketSink layer if Next is null.

        // ByteArrayContent requires a heap byte[]; HttpClient sync API doesn't accept Span.
        // 1 alloc per flush; acceptable in low-frequency batched IO. Zero-alloc HTTP path is Phase 4.
        var buffer  = batch.ToArray();
        using var content = new ByteArrayContent(buffer);
        content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
        using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint) { Content = content };
        ConfigureRequest(request);

        try
        {
            // SendAsync(...).GetAwaiter().GetResult() rather than Send(): TestServer's ClientHandler
            // throws NotSupportedException on Send, and SocketsHttpHandler's Send is itself a
            // sync-over-async wrapper. No await in the dispatch path — this is a sync call.
            using var response = _http.SendAsync(request).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                Interlocked.Exchange(ref _failures, 0);
                return;
            }
            IncrementFailureAndMaybeOpenBreaker();
        }
        catch
        {
            IncrementFailureAndMaybeOpenBreaker();
        }
    }

    private void IncrementFailureAndMaybeOpenBreaker()
    {
        if (Interlocked.Increment(ref _failures) >= _cbFailures)
            Volatile.Write(ref _breakerOpenUntilTicks, HfClock.NowTicks + _cbOpenDurationTicks);
    }
}
