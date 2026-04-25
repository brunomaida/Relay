using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Relay.Tests;

public sealed class SpscQueueSinkFlushTests
{
    [Fact]
    public void Flush_FromExternalThread_FlushBackendRunsOnConsumerThread_NotCaller()
    {
        // Invariant under test: regardless of timing, FlushBackend must never execute on the
        // producer thread. A null-before-stop assertion would be racy — the consumer may have
        // already picked up the flag and run FlushBackend by the time we check. Instead we
        // assert the thread-id inequality: if FlushBackend ran, it ran on a different thread.
        using var sink = new TrackingSpscSink(ringCapacity: 4_096, flushIntervalMs: 60_000);
        sink.Start();

        int callerThread = Environment.CurrentManagedThreadId;
        sink.Flush();
        sink.Stop(drainTimeoutMs: 500);

        sink.FlushBackendThreadId.Should().NotBeNull("consumer eventually ran FlushBackend");
        sink.FlushBackendThreadId.Should().NotBe(callerThread,
            "FlushBackend() must run on the consumer thread, never the caller's thread");
    }

    [Fact]
    public void Flush_BeforeStop_ConsumerObservesSignal_WithoutDrainingViaStopFinally()
    {
        // This test must distinguish "consumer ran FlushBackend because Flush() signalled it"
        // from "consumer ran FlushBackend in the Stop() finally block". If Flush() were a
        // no-op, the finally-path would still set FlushBackendThreadId during Stop(), hiding
        // the bug. We poll for the signal BEFORE calling Stop() to prove the signalling path.
        using var sink = new TrackingSpscSink(ringCapacity: 4_096, flushIntervalMs: 60_000);
        sink.Start();

        byte[] payload = [1, 2, 3];
        sink.Enqueue(payload);
        sink.Flush();

        int callerThread = Environment.CurrentManagedThreadId;
        WaitUntil(() => sink.FlushBackendThreadId is not null, timeoutMs: 1_000)
            .Should().BeTrue("Flush() must cause the consumer to run FlushBackend before Stop()");

        sink.FlushBackendThreadId.Should().NotBe(callerThread);
        sink.Stop(drainTimeoutMs: 1_000);
        sink.WriteCount.Should().Be(1);
    }

    private static bool WaitUntil(Func<bool> predicate, int timeoutMs)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (predicate()) return true;
            Thread.Sleep(5);
        }
        return predicate();
    }

    // Records which thread calls each consumer method. Used to verify Flush() signalling path.
    private sealed class TrackingSpscSink : SpscQueueSink
    {
        public int? FlushBackendThreadId { get; private set; }
        public int  WriteCount           { get; private set; }

        public TrackingSpscSink(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(ReadOnlySpan<byte> payload) => WriteCount++;
        protected override void FlushBackend()    => FlushBackendThreadId = Environment.CurrentManagedThreadId;
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }
}
