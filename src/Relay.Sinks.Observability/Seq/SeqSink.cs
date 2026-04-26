using System;
using System.Net.Http;
using Relay.Sinks.Http;

namespace Relay.Sinks.Observability.Seq;

/// <summary>
/// HTTP-batch sink that POSTs CLEF-formatted log lines to a Seq server's raw events endpoint.
/// </summary>
/// <remarks>
/// Wire format is opaque to this sink — payloads must already be CLEF JSON lines (one event
/// per <c>\n</c>-terminated record). Encoding is the producer's responsibility (Log2 side
/// uses <c>ClefEncoder</c>).
/// </remarks>
public sealed class SeqSink : HttpBatchSink
{
    private readonly string? _apiKey;

    /// <param name="http">Shared HttpClient. Caller owns disposal.</param>
    /// <param name="serverUrl">Seq server base URL (e.g., <c>http://seq:5341</c>). Trailing slash optional.</param>
    /// <param name="apiKey">Seq API key (sent as <c>X-Seq-ApiKey</c>) or null for unauthenticated.</param>
    public SeqSink(
        HttpClient http,
        string     serverUrl,
        string?    apiKey            = null,
        int        ringCapacity      = 64 * 1024,
        int        batchCapacity     = 64 * 1024,
        int        flushIntervalMs   = 1_000,
        int        cbFailures        = 3,
        int        cbOpenDurationMs  = 30_000)
        : base(http,
               BuildEndpoint(serverUrl),
               ringCapacity, batchCapacity, flushIntervalMs,
               cbFailures, cbOpenDurationMs,
               sinkName: "seq")
    {
        _apiKey = apiKey;
    }

    /// <inheritdoc/>
    protected override string ContentType => "application/vnd.serilog.clef";

    /// <inheritdoc/>
    protected override void ConfigureRequest(HttpRequestMessage request)
    {
        if (_apiKey is not null)
            request.Headers.TryAddWithoutValidation("X-Seq-ApiKey", _apiKey);
    }

    private static Uri BuildEndpoint(string serverUrl)
    {
        if (string.IsNullOrWhiteSpace(serverUrl))
            throw new ArgumentException("serverUrl is required", nameof(serverUrl));
        var trimmed = serverUrl.TrimEnd('/');
        return new Uri(trimmed + "/api/events/raw");
    }
}
