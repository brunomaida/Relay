using System;
using System.Diagnostics;
using System.Threading;
using Relay.Buffers;
using Xunit;
using Xunit.Abstractions;

namespace Relay.Tests;

/// <summary>
/// Multi-producer throughput harness for <see cref="MpscRingBuffer{T}"/>.
/// BDN does not model cross-thread contention cleanly (each benchmark is a single-thread hot
/// loop). This harness spins N dedicated producer threads + 1 consumer thread, measures total
/// wall time from release-barrier to drain-complete, and emits items/sec scaling figures.
/// </summary>
/// <remarks>
/// <para>Category: <c>Perf</c> — excluded from the default test run. Run explicitly with:</para>
/// <code>dotnet test --filter "Category=Perf"</code>
/// <para>
/// Not a regression gate — results vary with system noise. Use for direction-of-change
/// validation: did a refactor speed up or slow down contention? Expected shape: 2 producers
/// ~1.5× of 1 producer, 4 ~2.5×, 8 ~2.5–3× (scaling falls off as CAS retry dominates).
/// </para>
/// </remarks>
public sealed class MpscThroughputHarness
{
    private readonly ITestOutputHelper _output;

    public MpscThroughputHarness(ITestOutputHelper output) => _output = output;

    [Fact]
    [Trait("Category", "Perf")]
    public void Typed_MpscRingBuffer_Throughput_Sweep_1_2_4_8_Producers()
    {
        _output.WriteLine("=== MpscRingBuffer<Entry64> — items/sec by producer count ===");
        _output.WriteLine("ring capacity = 1_048_576 slots, items per producer = 1_000_000");
        _output.WriteLine("");

        double baseline = 0;
        foreach (int producers in new[] { 1, 2, 4, 8 })
        {
            double rate = RunOneTyped(producers, 1_000_000, capacity: 1 << 20);
            if (baseline == 0) baseline = rate;
            double scaling = rate / baseline;
            _output.WriteLine(
                $"producers={producers,2}  items/sec={rate,15:N0}  scaling vs 1P = {scaling:F2}×");
        }
    }

    [Fact]
    [Trait("Category", "Perf")]
    public void Byte_MpscRingBuffer_Throughput_Sweep_1_2_4_8_Producers()
    {
        _output.WriteLine("=== MpscByteRingBuffer — items/sec by producer count ===");
        _output.WriteLine("ring capacity = 4 MiB bytes, payload = 64 B, items per producer = 500_000");
        _output.WriteLine("");

        double baseline = 0;
        foreach (int producers in new[] { 1, 2, 4, 8 })
        {
            double rate = RunOneByte(producers, 500_000, capacity: 1 << 22, payloadSize: 64);
            if (baseline == 0) baseline = rate;
            double scaling = rate / baseline;
            _output.WriteLine(
                $"producers={producers,2}  items/sec={rate,15:N0}  scaling vs 1P = {scaling:F2}×");
        }
    }

    private static double RunOneTyped(int producers, int perProducer, int capacity)
    {
        var ring    = new MpscRingBuffer<Entry64>(capacity);
        var barrier = new ManualResetEventSlim(false);
        long total  = 0;
        long expected = (long)producers * perProducer;

        var producerThreads = new Thread[producers];
        for (int p = 0; p < producers; p++)
        {
            int pid = p;
            producerThreads[p] = new Thread(() =>
            {
                var item = new Entry64 { A = pid };
                barrier.Wait();
                for (int i = 0; i < perProducer; i++)
                {
                    while (!ring.TryPublish(in item)) Thread.SpinWait(1);
                }
            })
            { IsBackground = true, Priority = ThreadPriority.Highest };
            producerThreads[p].Start();
        }

        long consumerCount = 0;
        var consumer = new Thread(() =>
        {
            while (Volatile.Read(ref consumerCount) < expected)
            {
                if (ring.TryConsume(out _))
                    Volatile.Write(ref consumerCount, consumerCount + 1);
                else
                    Thread.SpinWait(1);
            }
        })
        { IsBackground = true, Priority = ThreadPriority.Highest };
        consumer.Start();

        var sw = Stopwatch.StartNew();
        barrier.Set();
        foreach (var t in producerThreads) t.Join();
        consumer.Join();
        sw.Stop();

        total = consumerCount;
        Assert.Equal(expected, total);
        return expected / sw.Elapsed.TotalSeconds;
    }

    private static double RunOneByte(int producers, int perProducer, int capacity, int payloadSize)
    {
        var ring     = new MpscByteRingBuffer(capacity);
        var barrier  = new ManualResetEventSlim(false);
        long expected = (long)producers * perProducer;

        var producerThreads = new Thread[producers];
        for (int p = 0; p < producers; p++)
        {
            int pid = p;
            producerThreads[p] = new Thread(() =>
            {
                var payload = new byte[payloadSize];
                payload[0] = (byte)pid;
                barrier.Wait();
                for (int i = 0; i < perProducer; i++)
                {
                    while (!ring.TryPublish(payload)) Thread.SpinWait(1);
                }
            })
            { IsBackground = true, Priority = ThreadPriority.Highest };
            producerThreads[p].Start();
        }

        long consumerCount = 0;
        var consumer = new Thread(() =>
        {
            while (Volatile.Read(ref consumerCount) < expected)
            {
                if (ring.TryPeek(out var _, out int adv))
                {
                    ring.Advance(adv);
                    Volatile.Write(ref consumerCount, consumerCount + 1);
                }
                else
                {
                    Thread.SpinWait(1);
                }
            }
        })
        { IsBackground = true, Priority = ThreadPriority.Highest };
        consumer.Start();

        var sw = Stopwatch.StartNew();
        barrier.Set();
        foreach (var t in producerThreads) t.Join();
        consumer.Join();
        sw.Stop();

        Assert.Equal(expected, consumerCount);
        return expected / sw.Elapsed.TotalSeconds;
    }
}
