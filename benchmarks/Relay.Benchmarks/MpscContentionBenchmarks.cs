using System;
using System.Diagnostics;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay.Buffers;

namespace Relay.Benchmarks;

/// <summary>
/// Measures <see cref="MpscRingBuffer{T}"/> throughput under multi-producer contention.
/// Closes coverage gap M10 (cost-map §8 "MPSC CAS-retry distribution") by surfacing
/// throughput at producer counts 1, 2, 4, 8 — retry rate is implicit in the throughput
/// curve.
/// </summary>
/// <remarks>
/// <para>
/// Producers run as dedicated <see cref="Thread"/> instances (not Tasks — we want stable
/// scheduling, no thread-pool sharing). A single consumer thread drains the ring while
/// producers are publishing. The benchmark method measures wall-clock from "start gate
/// released" to "consumer drained N×ItemsPerProducer items".
/// </para>
/// <para>
/// <b>Single-thread baseline (ProducerCount=1):</b> directly comparable to
/// <c>MpscBenchmarks.Mpsc_TryPublish_NoContention</c> from Phase 0 — except this class
/// runs the producer on a dedicated <see cref="Thread"/> rather than the BDN measurement
/// thread, so absolute numbers will differ slightly.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class MpscContentionBenchmarks
{
    private const int RingCapacity     = 65_536;
    private const int ItemsPerProducer = 1_000_000;

    [Params(1, 2, 4, 8)]
    public int ProducerCount;

    private MpscRingBuffer<Entry64> _ring     = null!;
    private Entry64                 _item;
    private Thread[]                _producers = null!;
    private Thread                  _consumer = null!;
    private ManualResetEventSlim    _startGate = null!;
    private CountdownEvent          _producerDone = null!;
    private long                    _consumed;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _item = new Entry64 { A = 1, B = 2 };
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _ring         = new MpscRingBuffer<Entry64>(RingCapacity);
        _startGate    = new ManualResetEventSlim(false);
        _producerDone = new CountdownEvent(ProducerCount);
        Interlocked.Exchange(ref _consumed, 0L);

        _producers = new Thread[ProducerCount];
        for (int i = 0; i < ProducerCount; i++)
        {
            _producers[i] = new Thread(ProducerLoop)
            {
                IsBackground = true,
                Priority     = ThreadPriority.AboveNormal,
                Name         = $"mpsc-producer-{i}"
            };
            _producers[i].Start();
        }

        _consumer = new Thread(ConsumerLoop)
        {
            IsBackground = true,
            Priority     = ThreadPriority.AboveNormal,
            Name         = "mpsc-consumer"
        };
        _consumer.Start();

        // Give threads time to reach the start gate.
        Thread.Sleep(50);
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _ring.Dispose();
        _startGate.Dispose();
        _producerDone.Dispose();
    }

    [Benchmark]
    public long Mpsc_Throughput_TotalItems()
    {
        long target = (long)ProducerCount * ItemsPerProducer;

        var sw = Stopwatch.StartNew();
        _startGate.Set();

        // Wait for all producers to finish publishing AND consumer to drain.
        _producerDone.Wait();
        SpinWait sp = default;
        while (Volatile.Read(ref _consumed) < target) sp.SpinOnce();

        sw.Stop();
        return Volatile.Read(ref _consumed);
    }

    private void ProducerLoop()
    {
        _startGate.Wait();
        for (int i = 0; i < ItemsPerProducer; i++)
        {
            // Spin until publish succeeds — captures retry cost in the throughput envelope.
            SpinWait sp = default;
            while (!_ring.TryPublish(in _item)) sp.SpinOnce();
        }
        _producerDone.Signal();
    }

    private void ConsumerLoop()
    {
        long target = (long)ProducerCount * ItemsPerProducer;
        SpinWait sp = default;
        while (Volatile.Read(ref _consumed) < target)
        {
            if (_ring.TryConsume(out _))
            {
                Interlocked.Increment(ref _consumed);
                sp.Reset();
            }
            else
            {
                sp.SpinOnce();
            }
        }
    }
}
