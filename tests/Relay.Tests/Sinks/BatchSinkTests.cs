using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Relay.Tests.Sinks;

public sealed class BatchSinkTests
{
    private sealed class CaptureBatchSink : BatchSink
    {
        public readonly List<byte[]> Flushes = new();
        public CaptureBatchSink(int batchCapacity, int flushIntervalMs)
            : base(ringCapacity: 4096, batchCapacity: batchCapacity, flushIntervalMs: flushIntervalMs, sinkName: "test") { }
        protected override void OnFlush(ReadOnlySpan<byte> batch) => Flushes.Add(batch.ToArray());
    }

    [Fact]
    public void Flushes_when_next_payload_would_exceed_capacity()
    {
        using var sink = new CaptureBatchSink(batchCapacity: 16, flushIntervalMs: 10_000);
        sink.Start();

        sink.Enqueue(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });   // 8 bytes
        sink.Enqueue(new byte[] { 9, 10, 11, 12, 13, 14, 15, 16 }); // fills to 16
        sink.Enqueue(new byte[] { 17, 18 });                    // forces flush of the first batch

        Thread.Sleep(50);  // give consumer thread time to drain
        sink.Flush();
        Thread.Sleep(50);

        sink.Flushes.Should().HaveCountGreaterThanOrEqualTo(1);
        sink.Flushes[0].Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
    }

    [Fact]
    public void Flushes_after_flush_interval_even_when_batch_not_full()
    {
        using var sink = new CaptureBatchSink(batchCapacity: 1024, flushIntervalMs: 50);
        sink.Start();

        sink.Enqueue(new byte[] { 0xAA, 0xBB });

        Thread.Sleep(200);  // exceed flushIntervalMs

        sink.Flushes.Should().HaveCountGreaterThanOrEqualTo(1);
        sink.Flushes[0].Should().Equal(new byte[] { 0xAA, 0xBB });
    }

    [Fact]
    public void Flush_signal_drains_pending_payload()
    {
        using var sink = new CaptureBatchSink(batchCapacity: 1024, flushIntervalMs: 10_000);
        sink.Start();

        sink.Enqueue(new byte[] { 0xCC });
        sink.Flush();
        Thread.Sleep(100);

        sink.Flushes.Should().HaveCount(1);
        sink.Flushes[0].Should().Equal(new byte[] { 0xCC });
    }
}
