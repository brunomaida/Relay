# Phase 7 — MPSC Multi-Thread Contention BDN

> **Phase 7 of 8** in master plan. **For agentic workers:** REQUIRED SUB-SKILL: superpowers:subagent-driven-development.

**Goal:** Add multi-producer contention BDN for `MpscRingBuffer<Entry64>` and `MpscByteRingBuffer`. Closes coverage gap **M10** (blind subgraph §8 in cost-map — "MPSC CAS-retry distribution"). **No production code changes** (no instrumentation; throughput is the measurable proxy).

**Architecture:** custom BDN harness that spawns N producer threads pinned via `Thread.Priority` (cannot use `ProcessorAffinity` from BCL on .NET — Process.ProcessorAffinity is process-wide). Single consumer thread. Producers coordinate via `ManualResetEventSlim` start gate; benchmark method measures wall-clock from "start gate released" to "all producers done + consumer drained".

**Metric:** **items per second total throughput** at producer counts 1, 2, 4, 8. Cost-map blind subgraph is closed by *measurement* (throughput tells us how much retry+coherence cost there is), not by counting retries. Invasive retry-counter instrumentation is OUT of scope for this phase — propose as a follow-up if numbers are surprising.

**Tech Stack:** BenchmarkDotNet 0.13.12, .NET 9. No production code change.

**Out of scope:** retry-counter instrumentation in `MpscRingBuffer`/`MpscByteRingBuffer` (would require production changes; flag as follow-up).

**Acceptance gate:**
- New BDN class compiles and runs under `--job short`
- Throughput numbers surface for at least 1, 2, 4 producer counts (8 may be skipped on dev box if too noisy)
- Cost-map §8 blind subgraph entry is replaced with "measured: <X items/sec at N producers>"

---

## File Structure

| File | Action | Responsibility |
|---|---|---|
| `benchmarks/Relay.Benchmarks/MpscContentionBenchmarks.cs` | **Create** | Multi-producer typed `MpscRingBuffer<Entry64>` throughput at N=1, 2, 4, 8 |
| `benchmarks/Relay.Benchmarks/MpscByteContentionBenchmarks.cs` | **Create** | Multi-producer `MpscByteRingBuffer` throughput at N=1, 2, 4, 8 |

---

## Task 1: Branch & Baseline

- [ ] **Step 1: Create the Phase 7 branch**

```bash
git checkout develop
git pull
git checkout -b chore/260429-bdn-mpsc-multi-thread-contention
```

- [ ] **Step 2: Verify baseline tests pass**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failures.

- [ ] **Step 3: Commit Phase 7 plan + any session strays**

```bash
git add docs/superpowers/plans/2026-04-29-phase7-mpsc-contention.md
# Include .claude/settings.local.json if modified during the session.
git commit -m "docs: add Phase 7 child plan w/Claude"
```

---

## Task 2: M10 — `MpscContentionBenchmarks.cs` (typed)

**Files:**
- Create: `benchmarks/Relay.Benchmarks/MpscContentionBenchmarks.cs`

- [ ] **Step 1: Write the BDN class**

```csharp
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
```

- [ ] **Step 2: Build**

Run: `dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo`
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/Relay.Benchmarks/MpscContentionBenchmarks.cs
git commit -m "test: add MpscContentionBenchmarks (M10 typed — multi-producer throughput at N=1,2,4,8) w/Claude"
```

---

## Task 3: M10 — `MpscByteContentionBenchmarks.cs` (packet)

**Files:**
- Create: `benchmarks/Relay.Benchmarks/MpscByteContentionBenchmarks.cs`

- [ ] **Step 1: Write the BDN class**

Same structure as Task 2 but for `MpscByteRingBuffer` with byte payload.

```csharp
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
```

- [ ] **Step 2: Build**

Run: `dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo`
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/Relay.Benchmarks/MpscByteContentionBenchmarks.cs
git commit -m "test: add MpscByteContentionBenchmarks (M10 packet — multi-producer throughput) w/Claude"
```

---

## Task 4: Run the BDNs

**Files:**
- Create artifacts under: `benchmarks/artifacts/2026-04-29-phase7/`

- [ ] **Step 1: Run the BDNs**

```bash
dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo
dotnet run --project benchmarks/Relay.Benchmarks -c Release --no-build -- \
  --filter "*MpscContentionBenchmarks*" "*MpscByteContentionBenchmarks*" \
  --job short \
  --artifacts benchmarks/artifacts/2026-04-29-phase7 \
  --exporters json markdown
```

> **Wall time:** each iteration spawns N+1 threads + publishes/consumes 1M×N items. ProducerCount=8 means 8M items × ShortRun (3+3 = 6 invocations) = ~12M items per measurement. Total ~10-15 min wall time across 4 producer counts.

> **System load consideration:** other dotnet/testhost processes can skew contention numbers. Verify no parallel BDN/test runs via `tasklist | grep -i dotnet`. If contended, abort and re-run.

- [ ] **Step 2: Sanity gates**

Inspect `benchmarks/artifacts/2026-04-29-phase7/results/*-report-github.md`:
- ProducerCount=1 throughput should match the single-thread baseline from Phase 3 (`Mpsc_TryPublish_NoContention`) — ~50-150M items/sec.
- ProducerCount=2 should show some scaling (1.3-1.8x of N=1) — uncontended CAS plus ~1 retry/M typical.
- ProducerCount=4 may show **negative scaling** (less throughput than N=2) — indicates CAS contention dominating. This is the expected blind-subgraph reveal.
- ProducerCount=8 likely far worse — head-cache miss + cross-core coherence storm. Acceptable.

If ProducerCount=1 is way off the Phase 3 baseline, abort — likely benchmark harness has a bug.

- [ ] **Step 3: Commit artifacts**

```bash
git add benchmarks/artifacts/2026-04-29-phase7/
git commit -m "test: Phase 7 BDN runs — MPSC multi-producer contention numbers w/Claude"
```

---

## Task 5: Update Master Plan + Cross-Check

- [ ] **Step 1: Mark Phase 7 done in master plan**

Update Phase 7 row status with summary including the throughput curve at N=1, 2, 4, 8.

- [ ] **Step 2: Update cross-check §3 + §8 (Blind Subgraphs)**

In `benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md`:
- §3 medium-priority table: mark M10 as `~~M10~~ resolved` with Phase 7 reference + throughput numbers.
- The **§8 Blind Subgraphs** entry "MPSC CAS-retry distribution" in `docs/reports/2026-04-29-resource-cost-map-relay.md` should be updated in Phase 8 (calibration). For now, just note in cross-check that the blind subgraph is "measured (throughput proxy)" — retry-counter instrumentation is a documented follow-up.

- [ ] **Step 3: Commit**

```bash
git add docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md \
        benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md
git commit -m "docs: mark Phase 7 done — MPSC contention BDNs landed w/Claude"
```

---

## Task 6: Final Verification

- [ ] **Step 1**: `dotnet test tests/Relay.Tests` — 0 failed (no production code changed).
- [ ] **Step 2**: `git log --oneline develop..HEAD` — 5 commits (plan, typed, packet, artifacts, docs).
- [ ] **Step 3**: `git diff develop..HEAD -- src/ tests/` — empty.
- [ ] **Step 4**: STOP — do not push.

---

## Self-Review Checklist

- ✅ M10 closed via measurement; retry-counter instrumentation flagged as follow-up.
- ✅ No placeholders.
- ✅ Type consistency — `MpscContentionBenchmarks` (typed) + `MpscByteContentionBenchmarks` (packet) named identically.
- ✅ Branch `chore/<yyMMdd>-<slug>`.
- ✅ Conventional Commits + `w/Claude`.
- ✅ Test gate enforced.
- ✅ No production source changes.
- ✅ `IterationSetup`/`IterationCleanup` properly reset state per BDN iteration (each iteration creates a fresh ring + threads).
