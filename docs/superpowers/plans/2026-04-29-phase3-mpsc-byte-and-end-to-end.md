# Phase 3 — MPSC Byte Ring + MPSC End-to-End

> **Phase 3 of 8** in master plan `docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md`. **For agentic workers:** REQUIRED SUB-SKILL: superpowers:subagent-driven-development. Steps use checkbox (`- [ ]`).

**Goal:** Add BDN coverage for the new MPSC byte ring (H1/H2) and MPSC end-to-end push throughput in both typed (H3) and packet (H4) variants. Closes coverage gaps **H1**, **H2**, **H3**, **H4** from `benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md`. **No production code changes.**

**Architecture:** three new BDN files. H1+H2 → one new `MpscByteRingBufferBenchmarks` class mirroring the existing `ByteRingBufferBenchmarks`. H3 → extend the existing typed throughput benchmark with Mpsc-side variants. H4 → new packet Mpsc throughput class with a `TestMpscPacketSink` helper.

**Tech Stack:** BenchmarkDotNet 0.13.12, .NET 9.

**Out of scope:** multi-thread MPSC contention BDN — that closes blind subgraph M10 in Phase 7. This phase measures **single-producer** MPSC steady-state only (matches the rest of the BDN suite, which is single-thread).

**Acceptance gate:** all three BDN classes build, run to completion under `--job short`, produce reports in `benchmarks/artifacts/2026-04-29-phase3/`. No production code changed. Tests still pass.

---

## File Structure

| File | Action | Responsibility |
|---|---|---|
| `benchmarks/Relay.Benchmarks/MpscByteRingBufferBenchmarks.cs` | **Create** | H1 + H2 — `MpscByteRingBuffer.TryPublish` / `TryPeek` / `Advance` single-thread, mirrors `ByteRingBufferBenchmarks` |
| `benchmarks/Relay.Benchmarks/QueueSinkThroughputBenchmarks.cs` | Modify | H3 — extend with `MpscPush_Single` / `MpscPush_Single_SlowBackend`. Adds `TestMpscPipe` helper inheriting `MpscQueueSink<Entry64>` |
| `benchmarks/Relay.Benchmarks/PacketSinks/MpscPacketQueueSinkThroughputBenchmarks.cs` | **Create** | H4 — packet Mpsc end-to-end Push throughput. Adds `TestMpscPacketSink` helper inheriting non-generic packet `MpscQueueSink` |

> Note: the existing typed throughput class is named `QueuePipeThroughputBenchmarks` (file: `QueueSinkThroughputBenchmarks.cs`). The class-rename to `QueueSinkThroughputBenchmarks` per master plan is OUT OF SCOPE for this phase — leave the class name alone, just add new methods + helper.

---

## Task 1: Branch & Baseline

- [ ] **Step 1: Create the Phase 3 branch**

```bash
git checkout develop
git pull
git checkout -b chore/260429-bdn-mpsc-byte-and-end-to-end
```

- [ ] **Step 2: Verify baseline tests pass**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failures.

- [ ] **Step 3: No commit yet** (baseline-only).

---

## Task 2: H1 + H2 — `MpscByteRingBufferBenchmarks.cs`

**Files:**
- Create: `benchmarks/Relay.Benchmarks/MpscByteRingBufferBenchmarks.cs`

- [ ] **Step 1: Write the BDN class**

Mirrors `ByteRingBufferBenchmarks.cs`. Single-thread (one producer, one consumer in the same thread for measurement isolation). Multi-thread contention is Phase 7.

```csharp
using BenchmarkDotNet.Attributes;
using Relay.Buffers;

namespace Relay.Benchmarks;

/// <summary>
/// Measures <see cref="MpscByteRingBuffer"/> primitives on a single thread against the
/// SPSC byte-ring baseline. No consumer thread — isolates atomic CAS / header-flag cost
/// from cross-thread coordination. Multi-producer contention is Phase 7.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class MpscByteRingBufferBenchmarks
{
    private MpscByteRingBuffer _ring     = null!;
    private MpscByteRingBuffer _ringFull = null!;
    private SpscByteRingBuffer _spsc     = null!;
    private byte[]             _payload  = null!;

    // 64 → 4 KB (L1), 1024 → 64 KB (L2), 65536 → 4 MB (L3 spill)
    [Params(64, 1024, 65536)]
    public int Capacity;

    // Tiny / cache-line / medium / large
    [Params(8, 64, 256, 1024)]
    public int PayloadSize;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _ring    = new MpscByteRingBuffer(Capacity);
        _spsc    = new SpscByteRingBuffer(Capacity);
        _payload = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++) _payload[i] = (byte)i;

        _ringFull = new MpscByteRingBuffer(Capacity);
        while (_ringFull.TryPublish(_payload)) { }
    }

    /// <summary>SPSC byte-ring round-trip baseline for ratio comparison.</summary>
    [Benchmark(Baseline = true)]
    public bool Spsc_RoundTrip()
    {
        _spsc.TryPublish(_payload);
        bool ok = _spsc.TryPeek(out _, out int adv);
        _spsc.Advance(adv);
        return ok;
    }

    /// <summary>MPSC round-trip uncontended — single-producer single-consumer same thread.</summary>
    [Benchmark]
    public bool Mpsc_RoundTrip_NoContention()
    {
        _ring.TryPublish(_payload);
        bool ok = _ring.TryPeek(out _, out int adv);
        _ring.Advance(adv);
        return ok;
    }

    /// <summary>Failed publish on full MPSC ring — head-cache hit + early exit.</summary>
    [Benchmark]
    public bool Mpsc_TryPublish_Full() => _ringFull.TryPublish(_payload);

    /// <summary>Failed peek on empty MPSC ring — Volatile.Read header + bit-test + early exit.</summary>
    [Benchmark]
    public bool Mpsc_TryPeek_Empty() => _ring.TryPeek(out _, out _);
}
```

- [ ] **Step 2: Build**

Run: `dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo`
Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/Relay.Benchmarks/MpscByteRingBufferBenchmarks.cs
git commit -m "test: add MpscByteRingBufferBenchmarks (H1 + H2 — TryPublish, TryPeek, Advance, Full, Empty) w/Claude"
```

---

## Task 3: H3 — extend typed throughput with `MpscPush_*`

**Files:**
- Modify: `benchmarks/Relay.Benchmarks/QueueSinkThroughputBenchmarks.cs`

The existing class `QueuePipeThroughputBenchmarks` already has `Push_Single`, `Push_Batch32`, `Push_Single_SlowBackend`, `Push_Batch32_SlowBackend` for SPSC. Add Mpsc-side variants and a `TestMpscPipe` helper.

- [ ] **Step 1: Append `TestMpscPipe` helper at the end of the file**

After the `TestSpscPipe` helper (around the bottom of the file), append:

```csharp
/// <summary>
/// Trivial MPSC queue pipe: increments <see cref="Sum"/> on every consumed item.
/// No backend I/O — exercises pure MPSC ring + consumer-loop cost.
/// Single-producer use only here (BDN runs one-thread); multi-producer in Phase 7.
/// </summary>
internal sealed class TestMpscPipe : MpscQueueSink<Entry64>
{
    private readonly int _backendSpinCycles;
    public long Sum;

    public TestMpscPipe(int ringCapacity, int backendSpinCycles = 0)
        : base(ringCapacity, flushIntervalMs: 100, pipeName: "bench-mpsc")
    {
        _backendSpinCycles = backendSpinCycles;
    }

    protected override void WriteToBackend(in Entry64 item)
    {
        Sum += item.A;
        if (_backendSpinCycles > 0) Thread.SpinWait(_backendSpinCycles);
    }

    protected override void FlushBackend() { }
    protected override void TryRecoverBackend() { }
    protected override void DisposeBackend() { }
}
```

- [ ] **Step 2: Add Mpsc benchmark methods to the existing class body**

Inside `QueuePipeThroughputBenchmarks` (after `Push_Batch32_SlowBackend`), append:

```csharp
[Benchmark]
public long MpscPush_Single()
{
    using var pipe = new TestMpscPipe(RingCapacity, backendSpinCycles: 0);
    pipe.Start();
    for (int i = 0; i < ItemCount; i++)
        pipe.Enqueue(in _item);
    pipe.Stop(30_000);
    return pipe.Sum;
}

[Benchmark]
public long MpscPush_Single_SlowBackend()
{
    // Simulated ~50-cycle backend work per item — representative of a tiny file/mem write.
    using var pipe = new TestMpscPipe(RingCapacity, backendSpinCycles: 50);
    pipe.Start();
    for (int i = 0; i < ItemCount; i++)
        pipe.Enqueue(in _item);
    pipe.Stop(30_000);
    return pipe.Sum;
}
```

> Note: `MpscQueueSink<T>` does not expose `EnqueueBatch` — only single-publish variants here. Batch variant would require a typed multi-publish API not yet on the public surface.

- [ ] **Step 3: Build**

Run: `dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo`
Expected: 0 warnings, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add benchmarks/Relay.Benchmarks/QueueSinkThroughputBenchmarks.cs
git commit -m "test: extend QueuePipeThroughputBenchmarks w/ MpscPush_Single + MpscPush_SlowBackend (H3) w/Claude"
```

---

## Task 4: H4 — `MpscPacketQueueSinkThroughputBenchmarks.cs`

**Files:**
- Create: `benchmarks/Relay.Benchmarks/PacketSinks/MpscPacketQueueSinkThroughputBenchmarks.cs`

- [ ] **Step 1: Write the BDN class**

Mirror of `QueueSinkPacketThroughputBenchmarks` (created in Phase 2) but using non-generic packet `MpscQueueSink` base.

```csharp
using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.PacketSinks;

/// <summary>
/// End-to-end throughput: producer pushes N byte payloads into an <see cref="MpscQueueSink"/>
/// (packet base), consumer thread drains them via a trivial <c>WriteToBackend</c>. Measures
/// cumulative cost of MPSC byte-ring publish (CAS + header) + length-prefixed peek/advance
/// + consumer-loop cost. Single-producer only here; multi-producer contention is Phase 7.
/// </summary>
/// <remarks>Mirror of <see cref="QueueSinkPacketThroughputBenchmarks"/> for the MPSC byte ring.</remarks>
[MemoryDiagnoser]
public class MpscPacketQueueSinkThroughputBenchmarks
{
    private byte[] _payload = null!;

    private const int RingCapacity = 65_536;
    private const int PayloadSize  = 64;

    [Params(100_000, 1_000_000)]
    public int ItemCount;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _payload = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++) _payload[i] = (byte)i;
    }

    [Benchmark(Baseline = true)]
    public long Push_Single()
    {
        using var sink = new TestMpscPacketSink(RingCapacity, backendSpinCycles: 0);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
        return sink.Sum;
    }

    [Benchmark]
    public long Push_Single_SlowBackend()
    {
        using var sink = new TestMpscPacketSink(RingCapacity, backendSpinCycles: 50);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
        return sink.Sum;
    }
}

/// <summary>
/// Trivial MPSC packet queue sink: increments <see cref="Sum"/> on every consumed payload.
/// No backend I/O — exercises pure MPSC byte-ring + consumer-loop cost. Single-producer
/// use here; multi-producer contention is Phase 7.
/// </summary>
internal sealed class TestMpscPacketSink : MpscQueueSink
{
    private readonly int _backendSpinCycles;
    public long Sum;

    public TestMpscPacketSink(int ringCapacity, int backendSpinCycles = 0)
        : base(ringCapacity, flushIntervalMs: 100, pipeName: "bench-mpsc-packet")
    {
        _backendSpinCycles = backendSpinCycles;
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        Sum += payload[0];
        if (_backendSpinCycles > 0) Thread.SpinWait(_backendSpinCycles);
    }

    protected override void FlushBackend() { }
    protected override void TryRecoverBackend() { }
    protected override void DisposeBackend() { }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo`
Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/Relay.Benchmarks/PacketSinks/MpscPacketQueueSinkThroughputBenchmarks.cs
git commit -m "test: add MpscPacketQueueSinkThroughputBenchmarks (H4) w/Claude"
```

---

## Task 5: Run Phase 3 BDNs

**Files:**
- Create artifacts under: `benchmarks/artifacts/2026-04-29-phase3/`

- [ ] **Step 1: Run BDNs (short job)**

```bash
dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo
dotnet run --project benchmarks/Relay.Benchmarks -c Release --no-build -- \
  --filter "*MpscByteRingBufferBenchmarks*" "*QueuePipeThroughputBenchmarks*" "*MpscPacketQueueSinkThroughputBenchmarks*" \
  --job short \
  --artifacts benchmarks/artifacts/2026-04-29-phase3 \
  --exporters json markdown
```

> **Note:** `MpscByteRingBufferBenchmarks` has `[Params(Capacity, PayloadSize)]` matrix = 12 cells × 4 methods = 48 cases. With ShortRun (3+3=6 invocations), total ~10 min. End-to-end throughput benchmarks add ~5 min. Total wall time ~15 min for the full Phase 3 run.

Expected: all benchmarks run to completion, exit 0.

- [ ] **Step 2: Sanity check on numbers**

Inspect `benchmarks/artifacts/2026-04-29-phase3/results/*-report-github.md`. Sanity gates:
- `Mpsc_RoundTrip_NoContention` (cap 1024, payload 64): expect 1.5-3x of `Spsc_RoundTrip` baseline (CAS + 2× header writes vs single-volatile-write SPSC). Cost-map predicted ~50c MPSC byte TryPublish vs ~35c SPSC byte → ratio ~1.4x. BDN will surface the actual ratio under one-thread CAS.
- `Mpsc_TryPublish_Full` should be < 5 ns (head-cache hit + early exit).
- `Mpsc_TryPeek_Empty` should be < 3 ns (single Volatile.Read + bit-test).
- `MpscPush_Single` (100k items): roughly 1.2-2x of `Push_Single` SPSC (the existing baseline) — single-producer CAS overhead. If 5x worse, investigate.
- All `MpscPacket.Push_*` complete in < 60 sec wall time per case.

If any gate fires far outside, abort and report.

- [ ] **Step 3: Commit artifacts**

```bash
git add benchmarks/artifacts/2026-04-29-phase3/
git commit -m "test: Phase 3 BDN runs — MPSC byte ring + Mpsc end-to-end numbers w/Claude"
```

---

## Task 6: Update Master Plan + Cross-Check

**Files:**
- Modify: `docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md`
- Modify: `benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md`

- [ ] **Step 1: Mark Phase 3 done in master plan**

In the Phase Map table, replace the Phase 3 status cell with:

```markdown
✅ done — MpscByteRingBufferBenchmarks (H1+H2), MpscPush_* extended in QueuePipeThroughputBenchmarks (H3), MpscPacketQueueSinkThroughputBenchmarks (H4); numbers under `benchmarks/artifacts/2026-04-29-phase3/`
```

- [ ] **Step 2: Update gap report**

In `benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md` §3 high-priority table, mark H1, H2, H3, H4 as resolved per Phase 2 conventions (`~~HX~~ resolved` + Phase 3 reference).

- [ ] **Step 3: Commit**

```bash
git add docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md \
        benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md
git commit -m "docs: mark Phase 3 done — MPSC byte ring + Mpsc throughput BDNs landed w/Claude"
```

---

## Task 7: Final Verification

- [ ] **Step 1: Re-run full test suite**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failed (no production code changed).

- [ ] **Step 2: Verify branch contents**

Run: `git log --oneline develop..HEAD`
Expected: 5 commits — `test:` ×3 (H1+H2, H3, H4) + `test:` artifacts + `docs:` master plan & cross-check.

- [ ] **Step 3: Verify no production source touched**

Run: `git diff develop..HEAD -- src/ tests/`
Expected: empty.

- [ ] **Step 4: Stop here — do NOT push**

User pushes + opens PR + merges manually. Do NOT run `git push`. Do NOT run `gh pr create`.

---

## Self-Review Checklist

- ✅ Spec coverage — H1, H2, H3, H4 closed.
- ✅ No placeholders — every code block is concrete.
- ✅ Type consistency — `TestMpscPipe`, `TestMpscPacketSink` named identically across declaration and benchmark methods.
- ✅ Frequent commits — 5 commits, one per logical concern.
- ✅ Branch name `chore/<yyMMdd>-<slug>`.
- ✅ Conventional Commits + `w/Claude`.
- ✅ Test gate enforced — production unchanged so no-regression check only.
- ✅ No production source changes.
