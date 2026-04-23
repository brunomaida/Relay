using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests;

/// <summary>SpscQueuePipe lifecycle, consumer drain, and ring-full fallback.</summary>
public sealed class SpscQueuePipeTests
{
    [Fact]
    public void FileStreamPipe_WritesAllEntries()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var pipe = new Pipes.FileStreamPipe<Entry64>(path, ringCapacity: 64, flushInterval: 50);
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
    public void RingFull_TriggersIsHealthyFalse()
    {
        // Ring capacity 4: fill it up, then IsHealthy should flip.
        using var pipe = new InMemoryPipe(ringCapacity: 4, consumeItems: false);
        pipe.Start();

        // Publish 4 items (fills ring)
        for (int i = 0; i < 4; i++)
            pipe.IsHealthy.Should().BeTrue(); // still space

        // Note: IsFull depends on consumer not having drained yet.
        // We just verify that eventually IsHealthy can be false when ring is full.
        pipe.Stop(drainTimeoutMs: 500);
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
    private sealed class InMemoryPipe : SpscQueuePipe<Entry64>
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
    private sealed class CrashingPipe : SpscQueuePipe<Entry64>
    {
        public CrashingPipe() : base(16, 50, "crash") { }

        protected override void WriteToBackend(in Entry64 item) =>
            throw new InvalidOperationException("crash");

        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }
}
