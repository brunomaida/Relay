using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Builder;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests;

/// <summary>SpscQueueSink lifecycle, consumer drain, and ring-full fallback.</summary>
public sealed class SpscQueueSinkTests
{
    [Fact]
    public void FileStreamSink_WritesAllEntries()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var pipe = new FileStreamSink<Entry64>(path, ringCapacity: 64, flushInterval: 50);
            pipe.Start();

            const int count = 32;
            for (int i = 0; i < count; i++)
                pipe.Enqueue(new Entry64 { A = i, B = i * 2 });

            pipe.Stop(drainTimeoutMs: 2_000);

            var bytes = File.ReadAllBytes(path);
            bytes.Length.Should().Be(count * System.Runtime.CompilerServices.Unsafe.SizeOf<Entry64>());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RingFull_FallsBackToNext_WhenConsumerNotStarted()
    {
        // No consumer thread — ring fills and stays full, items overflow to Next.
        var fallback = new CountingPipe();
        using var pipe = new InMemoryPipe(ringCapacity: 4, consumeItems: false);
        RelayBuilder.Start<Entry64, InMemoryPipe>(pipe).To(fallback).Build();
        // Do NOT call Start() — ring never drains.

        for (int i = 0; i < 4; i++)
            pipe.Enqueue(new Entry64 { A = i });     // fills ring

        pipe.Enqueue(new Entry64 { A = 99 });        // ring full → Next

        fallback.Accepted.Should().Be(1);
    }

    [Fact]
    public void ConsumerException_ExposedAfterCrash()
    {
        using var pipe = new CrashingPipe();
        pipe.Start();
        pipe.Enqueue(new Entry64 { A = 1 });

        Thread.Sleep(200); // let consumer crash

        pipe.IsConsuming.Should().BeFalse();
        pipe.ConsumerException.Should().NotBeNull();
    }

    // In-memory pipe for ring-full testing.
    private sealed class InMemoryPipe : SpscQueueSink<Entry64>
    {
        private readonly bool _consume;
        private int _count;

        public InMemoryPipe(int ringCapacity, bool consumeItems = true)
            : base(ringCapacity, 50, "test") => _consume = consumeItems;

        public int ConsumedCount => _count;

        protected override void WriteToBackend(in Entry64 item) { if (_consume) _count++; }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    // Pipe whose WriteToBackend always throws.
    private sealed class CrashingPipe : SpscQueueSink<Entry64>
    {
        public CrashingPipe() : base(16, 50, "crash") { }

        protected override void WriteToBackend(in Entry64 item) =>
            throw new InvalidOperationException("crash");

        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class CountingPipe : DispatchSink<Entry64>
    {
        public int Accepted { get; private set; }
        public override bool IsHealthy => true;
        protected override bool Accept(in Entry64 item) { Accepted++; return true; }
        public override void Flush()   { }
        public override void Dispose() { }
    }
}
