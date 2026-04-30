using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay.Buffers;

namespace Relay.Benchmarks;

/// <summary>
/// Compares <see cref="MpscRingBuffer{T}"/> (v4 stride layout: Published on its own
/// 64-byte cache line, Value on the next) against the legacy Slot-based layout
/// (Value at offset 4/8, straddles cache lines for all valid T).
/// </summary>
/// <remarks>
/// Gate: stride layout must be ≥ legacy layout throughput at N=1. Improvement at N≥2
/// is expected (reduced inter-slot false sharing) but not gated — document delta in
/// commit message. Producer machinery mirrors <see cref="MpscContentionBenchmarks"/>.
/// </remarks>
[MemoryDiagnoser]
public class MpscSlotLayoutBenchmarks
{
    private const int ItemsPerProducer = 500_000;

    [Params(1, 2, 4, 8)]
    public int ProducerCount;

    [Params(1_024, 65_536)]
    public int Capacity;

    private MpscRingBuffer<Entry64>       _ring       = null!;
    private MpscRingBufferLegacy<Entry64> _legacyRing = null!;
    private Entry64                       _item;
    private Thread[]                      _producers    = null!;
    private ManualResetEventSlim          _startGate    = null!;
    private CountdownEvent                _producerDone = null!;
    private long                          _consumed;

    [GlobalSetup]
    public void GlobalSetup() => _item = new Entry64 { A = 1, B = 2 };

    // ── stride layout (production) ───────────────────────────────────────────

    [IterationSetup(Target = nameof(StrideLayout_Throughput))]
    public void SetupStride()
    {
        _ring = new MpscRingBuffer<Entry64>(Capacity);
        InitShared();

        for (int i = 0; i < ProducerCount; i++)
        {
            var idx = i;
            _producers[i] = new Thread(() =>
            {
                _startGate.Wait();
                for (int j = 0; j < ItemsPerProducer; j++)
                {
                    SpinWait sp = default;
                    while (!_ring.TryPublish(in _item)) sp.SpinOnce();
                }
                _producerDone.Signal();
            })
            {
                IsBackground = true,
                Priority     = ThreadPriority.AboveNormal,
                Name         = $"stride-prod-{idx}"
            };
            _producers[i].Start();
        }
        Thread.Sleep(30);
    }

    [IterationCleanup(Target = nameof(StrideLayout_Throughput))]
    public void CleanupStride() { _ring.Dispose(); TeardownShared(); }

    [Benchmark(Baseline = true)]
    public long StrideLayout_Throughput()
    {
        long target = (long)ProducerCount * ItemsPerProducer;
        _startGate.Set();
        _producerDone.Wait();

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
        return Volatile.Read(ref _consumed);
    }

    // ── legacy slot layout (benchmark-only copy) ─────────────────────────────

    [IterationSetup(Target = nameof(LegacySlotLayout_Throughput))]
    public void SetupLegacy()
    {
        _legacyRing = new MpscRingBufferLegacy<Entry64>(Capacity);
        InitShared();

        for (int i = 0; i < ProducerCount; i++)
        {
            var idx = i;
            _producers[i] = new Thread(() =>
            {
                _startGate.Wait();
                for (int j = 0; j < ItemsPerProducer; j++)
                {
                    SpinWait sp = default;
                    while (!_legacyRing.TryPublish(in _item)) sp.SpinOnce();
                }
                _producerDone.Signal();
            })
            {
                IsBackground = true,
                Priority     = ThreadPriority.AboveNormal,
                Name         = $"legacy-prod-{idx}"
            };
            _producers[i].Start();
        }
        Thread.Sleep(30);
    }

    [IterationCleanup(Target = nameof(LegacySlotLayout_Throughput))]
    public void CleanupLegacy() { _legacyRing.Dispose(); TeardownShared(); }

    [Benchmark]
    public long LegacySlotLayout_Throughput()
    {
        long target = (long)ProducerCount * ItemsPerProducer;
        _startGate.Set();
        _producerDone.Wait();

        SpinWait sp = default;
        while (Volatile.Read(ref _consumed) < target)
        {
            if (_legacyRing.TryConsume(out _))
            {
                Interlocked.Increment(ref _consumed);
                sp.Reset();
            }
            else
            {
                sp.SpinOnce();
            }
        }
        return Volatile.Read(ref _consumed);
    }

    // ── shared setup/teardown ────────────────────────────────────────────────

    private void InitShared()
    {
        _startGate    = new ManualResetEventSlim(false);
        _producerDone = new CountdownEvent(ProducerCount);
        _producers    = new Thread[ProducerCount];
        Interlocked.Exchange(ref _consumed, 0L);
    }

    private void TeardownShared()
    {
        _startGate.Dispose();
        _producerDone.Dispose();
    }
}

/// <summary>
/// Verbatim copy of the pre-v4-audit <c>MpscRingBuffer&lt;T&gt;</c> Slot-based layout.
/// <c>Published</c> at offset 0, <c>T</c> at offset 4 (or 8 with alignment padding) —
/// straddles cache lines for all T with sizeof(T) ≥ 64. Kept for before/after comparison
/// in <see cref="MpscSlotLayoutBenchmarks"/> only; never use in production.
/// </summary>
internal sealed unsafe class MpscRingBufferLegacy<T> : IDisposable where T : unmanaged
{
    private struct Slot
    {
        public int Published;
        public T   Value;
    }

    private readonly Slot* _slots;
    private readonly int   _mask;
    private readonly int   _bytesAllocated;

    private PaddedLong _claimedTail;
    private PaddedLong _headCache;
    private PaddedLong _head;

    private bool _disposed;

    public int Capacity { get; }

    public MpscRingBufferLegacy(int capacity)
    {
        if (capacity <= 0 || (capacity & (capacity - 1)) != 0)
            throw new ArgumentException("Capacity must be a positive power of two.", nameof(capacity));

        Capacity        = capacity;
        _mask           = capacity - 1;
        _bytesAllocated = capacity * sizeof(Slot);
        _slots          = (Slot*)NativeMemory.AlignedAlloc((nuint)_bytesAllocated, 64);
        NativeMemory.Clear(_slots, (nuint)_bytesAllocated);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPublish(in T item)
    {
        while (true)
        {
            long claimed   = Volatile.Read(ref _claimedTail.Value);
            long wrapPoint = claimed - Capacity;
            long hc        = _headCache.Value;

            if (hc <= wrapPoint)
            {
                hc = Volatile.Read(ref _head.Value);
                _headCache.Value = hc;
                if (hc <= wrapPoint) return false;
            }

            if (Interlocked.CompareExchange(ref _claimedTail.Value, claimed + 1, claimed) == claimed)
            {
                ref Slot slot = ref _slots[claimed & _mask];
                slot.Value = item;
                Volatile.Write(ref slot.Published, 1);
                return true;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConsume(out T item)
    {
        long pos = _head.Value;
        ref Slot slot = ref _slots[pos & _mask];

        if (Volatile.Read(ref slot.Published) == 0)
        {
            Unsafe.SkipInit(out item);
            return false;
        }

        item = slot.Value;
        Volatile.Write(ref slot.Published, 0);
        Volatile.Write(ref _head.Value, pos + 1);
        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMemory.AlignedFree(_slots);
    }
}
