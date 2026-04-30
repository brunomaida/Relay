using System;
using System.Diagnostics;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay.Buffers;

namespace Relay.Benchmarks;

/// <summary>
/// Measures <see cref="MpscByteRingBuffer"/> throughput under multi-producer contention.
/// Companion of <see cref="MpscContentionBenchmarks"/> for the byte/packet ring.
/// </summary>
[MemoryDiagnoser]
public class MpscByteContentionBenchmarks
{
    private const int RingCapacity     = 65_536;
    private const int ItemsPerProducer = 1_000_000;
    private const int PayloadSize      = 64;

    [Params(1, 2, 4, 8)]
    public int ProducerCount;

    private MpscByteRingBuffer    _ring        = null!;
    private byte[]                _payload     = null!;
    private Thread[]              _producers   = null!;
    private Thread                _consumer    = null!;
    private ManualResetEventSlim  _startGate   = null!;
    private CountdownEvent        _producerDone = null!;
    private long                  _consumed;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _payload = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++) _payload[i] = (byte)i;
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _ring         = new MpscByteRingBuffer(RingCapacity);
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
                Name         = $"mpsc-byte-producer-{i}"
            };
            _producers[i].Start();
        }

        _consumer = new Thread(ConsumerLoop)
        {
            IsBackground = true,
            Priority     = ThreadPriority.AboveNormal,
            Name         = "mpsc-byte-consumer"
        };
        _consumer.Start();

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
    public long Mpsc_Byte_Throughput_TotalItems()
    {
        long target = (long)ProducerCount * ItemsPerProducer;

        var sw = Stopwatch.StartNew();
        _startGate.Set();

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
            SpinWait sp = default;
            while (!_ring.TryPublish(_payload)) sp.SpinOnce();
        }
        _producerDone.Signal();
    }

    private void ConsumerLoop()
    {
        long target = (long)ProducerCount * ItemsPerProducer;
        SpinWait sp = default;
        while (Volatile.Read(ref _consumed) < target)
        {
            if (_ring.TryPeek(out _, out int adv))
            {
                _ring.Advance(adv);
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
