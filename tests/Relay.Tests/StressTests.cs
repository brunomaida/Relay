using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using FluentAssertions;
using Relay;
using Xunit;
using Xunit.Abstractions;

namespace Relay.Tests;

/// <summary>
/// Sustained 5-minute throughput tests with a zero-GC gate.
/// Excluded from the default run — execute explicitly with:
/// <code>dotnet test --filter "Category=Stress"</code>
/// Each test takes ~5 minutes. Total suite: ~10 minutes.
/// </summary>
public sealed class StressTests
{
    private readonly ITestOutputHelper _output;

    public StressTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Single producer, SPSC sink, 5 minutes. Asserts zero gen0/gen1 GC collections —
    /// any allocation on the hot path will trigger at least one collection within 5 min.
    /// </summary>
    [Fact(Timeout = 320_000)]
    [Trait("Category", "Stress")]
    public void Spsc_SustainedThroughput_5Min_ZeroGcPressure()
    {
        var pipe = new StressSpscPipe();
        pipe.Start();

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        int gen0Before = GC.CollectionCount(0);
        int gen1Before = GC.CollectionCount(1);

        var item = new Item64 { Value = 1 };
        var sw   = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < 300)
            pipe.Enqueue(in item);

        pipe.Stop(5_000);

        int  gen0After = GC.CollectionCount(0);
        int  gen1After = GC.CollectionCount(1);
        long consumed  = pipe.Consumed;
        double throughput = consumed / sw.Elapsed.TotalSeconds;

        _output.WriteLine($"Consumed={consumed:N0}  Throughput={throughput:N0}/s  Gen0Δ={gen0After - gen0Before}  Gen1Δ={gen1After - gen1Before}");

        (gen0After - gen0Before).Should().Be(0, "SPSC hot path must not allocate (gen0)");
        (gen1After - gen1Before).Should().Be(0, "SPSC hot path must not allocate (gen1)");
    }

    /// <summary>
    /// Four concurrent producers, MPSC sink, 5 minutes. Zero-GC gate applies to all
    /// producer threads and the consumer thread collectively.
    /// </summary>
    [Fact(Timeout = 320_000)]
    [Trait("Category", "Stress")]
    public void Mpsc_4Producer_SustainedThroughput_5Min_ZeroGcPressure()
    {
        const int ProducerCount = 4;
        const int DurationSec   = 300;

        var pipe = new StressMpscPipe();
        pipe.Start();

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        int gen0Before = GC.CollectionCount(0);

        var gate     = new ManualResetEventSlim(false);
        var enqueued = new long[ProducerCount];
        var threads  = new Thread[ProducerCount];

        for (int i = 0; i < ProducerCount; i++)
        {
            int idx = i;
            threads[idx] = new Thread(() =>
            {
                gate.Wait();
                var item = new Item64 { Value = idx };
                var sw   = Stopwatch.StartNew();
                long n   = 0;
                while (sw.Elapsed.TotalSeconds < DurationSec)
                {
                    pipe.Enqueue(in item);
                    n++;
                }
                Volatile.Write(ref enqueued[idx], n);
            }) { IsBackground = true, Name = $"stress-producer-{i}" };
            threads[idx].Start();
        }

        var wallSw = Stopwatch.StartNew();
        gate.Set();
        for (int i = 0; i < ProducerCount; i++)
            threads[i].Join(TimeSpan.FromSeconds(DurationSec + 30));

        pipe.Stop(5_000);

        int  gen0After = GC.CollectionCount(0);
        long total     = 0;
        for (int i = 0; i < ProducerCount; i++) total += enqueued[i];
        double throughput = total / wallSw.Elapsed.TotalSeconds;

        _output.WriteLine($"Producers={ProducerCount}  Enqueued={total:N0}  Consumed={pipe.Consumed:N0}  Throughput={throughput:N0}/s  Gen0Δ={gen0After - gen0Before}");

        (gen0After - gen0Before).Should().Be(0, "MPSC hot path must not allocate (gen0)");
    }

    // -------------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential, Size = 64)]
    private struct Item64 { public long Value; }

    private sealed class StressSpscPipe : SpscQueueSink<Item64>
    {
        private long _consumed;
        public long Consumed => Volatile.Read(ref _consumed);

        public StressSpscPipe() : base(ringCapacity: 65536, flushIntervalMs: 10, "stress-spsc") { }

        protected override void WriteToBackend(in Item64 item) => Interlocked.Increment(ref _consumed);
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class StressMpscPipe : MpscQueueSink<Item64>
    {
        private long _consumed;
        public long Consumed => Volatile.Read(ref _consumed);

        public StressMpscPipe() : base(ringCapacity: 65536, flushIntervalMs: 10, "stress-mpsc") { }

        protected override void WriteToBackend(in Item64 item) => Interlocked.Increment(ref _consumed);
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }
}
