# Phase 2 — Packet-Tree Symmetry BDNs

> **Phase 2 of 8** in master plan `docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md`. **For agentic workers:** REQUIRED SUB-SKILL: superpowers:subagent-driven-development. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Add BDN classes for the packet-hierarchy equivalents that already exist in the typed hierarchy. Closes coverage gaps **M1** (ForkSink packet), **M2** (MultiSink packet, N=2), **M4** (FilterSink packet), **M9** (SpscQueueSink packet end-to-end Push) from `benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md`. **No production code changes.**

**Architecture:** four BDN additions, all single-thread producer-side measurements. Reuse existing `ByteCounterPipe` / `ByteRejectPipe` / `ByteDeadPipe` helpers from `ByteEnqueueBenchmarks.cs` (internal in `Relay.Benchmarks` assembly). For M9, mirror `QueuePipeThroughputBenchmarks.TestSpscPipe` with a packet-side `TestSpscPacketSink : SpscQueueSink` (non-generic packet base).

**Tech Stack:** BenchmarkDotNet 0.13.12, .NET 9.

**Out of scope:** `Multi2PacketSink` — type does not exist (M3, scheduled for Phase 6). `MpscByteRingBuffer` BDNs (Phase 3). Backend-specific BDNs (Phase 4).

**Acceptance gate:** all four BDNs build, run to completion under `--job short`, and produce reports in `benchmarks/artifacts/2026-04-29-phase2/`. No production code touched. Tests still pass.

---

## File Structure

| File | Action | Responsibility |
|---|---|---|
| `benchmarks/Relay.Benchmarks/PacketSinks/PropagatePacketBenchmarks.cs` | **Create** | Packet mirror of `PropagateBenchmarks.cs` — measures `ForkSink` (packet) propagate cost |
| `benchmarks/Relay.Benchmarks/PacketSinks/MultiPacketEnqueueBenchmarks.cs` | **Create** | Packet mirror of `MultiEnqueueBenchmarks.cs` — measures `MultiSink` (packet, N=2). No `Multi2`-equivalent yet |
| `benchmarks/Relay.Benchmarks/PacketSinks/FilterPacketSinkBenchmarks.cs` | **Create** | Packet mirror of `FilterSinkBenchmarks.cs` — measures `FilterSink` (packet) Pass / Reject |
| `benchmarks/Relay.Benchmarks/PacketSinks/QueueSinkPacketThroughputBenchmarks.cs` | **Create** | Packet equivalent of `QueuePipeThroughputBenchmarks.cs`. Includes `TestSpscPacketSink` helper inheriting `SpscQueueSink` (non-generic packet base). No batch variant — packet base does not expose `EnqueueBatch` |

---

## Task 1: Branch & Baseline

**Files:** _none modified_

- [ ] **Step 1: Create the Phase 2 branch**

```bash
git checkout develop
git pull
git checkout -b chore/260429-bdn-packet-tree-symmetry
```

- [ ] **Step 2: Verify baseline tests pass**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failures.

- [ ] **Step 3: No commit yet** (baseline-only).

---

## Task 2: M1 — `PropagatePacketBenchmarks.cs`

**Files:**
- Create: `benchmarks/Relay.Benchmarks/PacketSinks/PropagatePacketBenchmarks.cs`

- [ ] **Step 1: Write the BDN class**

Mirrors `PropagateBenchmarks.cs` for the packet hierarchy. `ByteCounterPipe` is already defined in `ByteEnqueueBenchmarks.cs` (internal in same assembly — can reference).

```csharp
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.PacketSinks;

/// <summary>
/// Measures the overhead of packet-side <see cref="PacketSink.Enqueue"/> propagation
/// — default no-propagate path, single-sink propagate-no-Next, and <see cref="ForkSink"/>
/// (primary + Next).
/// </summary>
/// <remarks>
/// Mirror of <see cref="PropagateBenchmarks"/> for the packet hierarchy. Uses
/// <c>ByteCounterPipe</c> (defined in <see cref="ByteEnqueueBenchmarks"/>) as the trivial sink.
/// </remarks>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class PropagatePacketBenchmarks
{
    private PacketSink _depth1Default       = null!;
    private PacketSink _depth1PropagateOnly = null!;
    private PacketSink _depth2PropagateFork = null!;
    private PacketSink _depth2ForkWrapped   = null!;

    private byte[] _payload = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _payload    = new byte[64];
        _payload[0] = 1;
        _payload[1] = 2;

        // Depth 1: single ByteCounterPipe — baseline, default PropagateAfterAccept=false.
        _depth1Default = new ByteCounterPipe();

        // Depth 1: PropagateByteCounterPipe with no Next — pure propagate-branch cost vs default.
        _depth1PropagateOnly = new PropagateByteCounterPipe();

        // Depth 2: PropagateByteCounterPipe → ByteCounterPipe — both receive the payload.
        var prop2 = new PropagateByteCounterPipe();
        prop2.Next = new ByteCounterPipe();
        _depth2PropagateFork = prop2;

        // Depth 2: ForkSink(ByteCounterPipe) → ByteCounterPipe — actual ForkSink cost vs custom propagate.
        var primaryCounter = new ByteCounterPipe();
        var auditCounter   = new ByteCounterPipe();
        var fork           = new ForkSink(primaryCounter);
        fork.Next          = auditCounter;
        _depth2ForkWrapped = fork;
    }

    [Benchmark(Baseline = true)]
    public void Depth1_Healthy_Default() => _depth1Default.Enqueue(_payload);

    [Benchmark]
    public void Depth1_Healthy_Propagate_NoNext() => _depth1PropagateOnly.Enqueue(_payload);

    [Benchmark]
    public void Depth2_Propagate_Fork() => _depth2PropagateFork.Enqueue(_payload);

    [Benchmark]
    public void Depth2_Fork_Wrapped() => _depth2ForkWrapped.Enqueue(_payload);
}

/// <summary>
/// Healthy propagate sink with an observable side-effect — prevents JIT dead-code elimination.
/// PropagateAfterAccept=true ensures Next is always called after a successful accept.
/// </summary>
internal sealed class PropagateByteCounterPipe : PacketSink
{
    public PropagateByteCounterPipe() : base(propagateAfterAccept: true) { }

    public long LastValue;

    public override bool IsHealthy => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        Volatile.Write(ref LastValue, payload[0]);
        return true;
    }

    public override void Flush()   { }
    public override void Dispose() { }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo`
Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/Relay.Benchmarks/PacketSinks/PropagatePacketBenchmarks.cs
git commit -m "test: add PropagatePacketBenchmarks (M1 — packet-side ForkSink + propagate) w/Claude"
```

---

## Task 3: M2 — `MultiPacketEnqueueBenchmarks.cs`

**Files:**
- Create: `benchmarks/Relay.Benchmarks/PacketSinks/MultiPacketEnqueueBenchmarks.cs`

- [ ] **Step 1: Write the BDN class**

```csharp
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.PacketSinks;

/// <summary>
/// Measures <see cref="MultiSink"/> (packet, N=2) Enqueue cost. No <c>Multi2</c>-equivalent
/// exists yet for the packet hierarchy — that is Phase 6 of the master plan.
/// </summary>
/// <remarks>Mirror of <see cref="MultiEnqueueBenchmarks"/>.</remarks>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class MultiPacketEnqueueBenchmarks
{
    private MultiSink _multi   = null!;
    private byte[]    _payload = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _payload    = new byte[64];
        _payload[0] = 3;
        _payload[1] = 7;
        _multi      = new MultiSink(new ByteCounterPipe(), new ByteCounterPipe());
    }

    [Benchmark(Baseline = true)]
    public void Multi_Packet_Enqueue() => _multi.Enqueue(_payload);
}

/// <summary>
/// Measures <see cref="MultiSink.IsHealthy"/> short-circuit OR-reduction on packet hierarchy.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class MultiPacketIsHealthyBenchmarks
{
    private MultiSink _multi = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // ByteCounterPipe is always healthy — short-circuit on first child.
        _multi = new MultiSink(new ByteCounterPipe(), new ByteCounterPipe());
    }

    [Benchmark(Baseline = true)]
    public bool Multi_Packet_IsHealthy() => _multi.IsHealthy;
}
```

- [ ] **Step 2: Build**

Run: `dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo`
Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/Relay.Benchmarks/PacketSinks/MultiPacketEnqueueBenchmarks.cs
git commit -m "test: add MultiPacketEnqueueBenchmarks + IsHealthy (M2) w/Claude"
```

---

## Task 4: M4 — `FilterPacketSinkBenchmarks.cs`

**Files:**
- Create: `benchmarks/Relay.Benchmarks/PacketSinks/FilterPacketSinkBenchmarks.cs`

- [ ] **Step 1: Write the BDN class**

```csharp
using System;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.PacketSinks;

/// <summary>
/// Measures <see cref="FilterSink"/> (packet) <see cref="PacketPredicate"/> cost on pass and
/// reject paths. Both paths return true from <c>Accept</c> — rejected payloads do not propagate
/// to <c>Next</c>.
/// </summary>
/// <remarks>Mirror of <see cref="FilterSinkBenchmarks"/> for the packet hierarchy.</remarks>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3)]
public class FilterPacketSinkBenchmarks
{
    private FilterSink _filterPass   = null!;
    private FilterSink _filterReject = null!;
    private byte[]     _payload      = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _payload    = new byte[64];
        _payload[0] = 5;
        _payload[1] = 10;

        // Predicate always passes — ByteCounterPipe downstream so JIT cannot eliminate the call.
        _filterPass = new FilterSink(_ => true, new ByteCounterPipe());

        // Predicate always rejects — downstream never called.
        _filterReject = new FilterSink(_ => false, new ByteCounterPipe());
    }

    [Benchmark]
    public void Filter_Packet_Pass() => _filterPass.Enqueue(_payload);

    [Benchmark(Baseline = true)]
    public void Filter_Packet_Reject() => _filterReject.Enqueue(_payload);
}
```

- [ ] **Step 2: Build**

Run: `dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo`
Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/Relay.Benchmarks/PacketSinks/FilterPacketSinkBenchmarks.cs
git commit -m "test: add FilterPacketSinkBenchmarks (M4) w/Claude"
```

---

## Task 5: M9 — `QueueSinkPacketThroughputBenchmarks.cs`

**Files:**
- Create: `benchmarks/Relay.Benchmarks/PacketSinks/QueueSinkPacketThroughputBenchmarks.cs`

- [ ] **Step 1: Write the BDN class**

Includes a `TestSpscPacketSink` helper analogous to `TestSpscPipe` in `QueuePipeThroughputBenchmarks.cs`. Note: packet `SpscQueueSink` does not expose `EnqueueBatch` (typed-only); only `Push_Single` and `Push_Single_SlowBackend` variants here.

```csharp
using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Relay;

namespace Relay.Benchmarks.PacketSinks;

/// <summary>
/// End-to-end throughput: producer pushes N byte payloads into an <see cref="SpscQueueSink"/>
/// (packet base), consumer thread drains them via a trivial <c>WriteToBackend</c>. Measures
/// cumulative cost of byte-ring publish + length-prefixed peek/advance + consumer-loop cost.
/// </summary>
/// <remarks>
/// Mirror of <see cref="QueuePipeThroughputBenchmarks"/>. Packet base does not expose a
/// batched <c>EnqueueBatch</c> API (variable-length records make a span-batch awkward), so
/// only single-publish variants are measured.
/// </remarks>
[MemoryDiagnoser]
public class QueueSinkPacketThroughputBenchmarks
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
        using var sink = new TestSpscPacketSink(RingCapacity, backendSpinCycles: 0);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
        return sink.Sum;
    }

    [Benchmark]
    public long Push_Single_SlowBackend()
    {
        // Simulated ~50-cycle backend work per payload — representative of a tiny file/socket write.
        using var sink = new TestSpscPacketSink(RingCapacity, backendSpinCycles: 50);
        sink.Start();
        for (int i = 0; i < ItemCount; i++)
            sink.Enqueue(_payload);
        sink.Stop(30_000);
        return sink.Sum;
    }
}

/// <summary>
/// Trivial SPSC packet queue sink: increments <see cref="Sum"/> on every consumed payload.
/// No backend I/O — exercises pure byte-ring + consumer-loop cost.
/// </summary>
internal sealed class TestSpscPacketSink : SpscQueueSink
{
    private readonly int _backendSpinCycles;
    public long Sum;

    public TestSpscPacketSink(int ringCapacity, int backendSpinCycles = 0)
        : base(ringCapacity, flushIntervalMs: 100, pipeName: "bench-packet")
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
git add benchmarks/Relay.Benchmarks/PacketSinks/QueueSinkPacketThroughputBenchmarks.cs
git commit -m "test: add QueueSinkPacketThroughputBenchmarks (M9 — SpscQueueSink packet end-to-end Push) w/Claude"
```

---

## Task 6: Run all Phase 2 BDNs

**Files:**
- Create artifacts under: `benchmarks/artifacts/2026-04-29-phase2/`

- [ ] **Step 1: Run BDNs (short job for triage)**

```bash
dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo
dotnet run --project benchmarks/Relay.Benchmarks -c Release --no-build -- \
  --filter "*PropagatePacketBenchmarks*" "*MultiPacketEnqueueBenchmarks*" "*MultiPacketIsHealthyBenchmarks*" "*FilterPacketSinkBenchmarks*" "*QueueSinkPacketThroughputBenchmarks*" \
  --job short \
  --artifacts benchmarks/artifacts/2026-04-29-phase2 \
  --exporters json markdown
```

Expected: all benchmarks run to completion, exit 0. Reports under `benchmarks/artifacts/2026-04-29-phase2/results/`.

> **Note:** `QueueSinkPacketThroughputBenchmarks` invokes producer+consumer threads with `Stop(30_000)` drain timeout. With `[Params(100_000, 1_000_000)]` this is the longest BDN in Phase 2 — total wall time ~3-5 minutes for the full set.

- [ ] **Step 2: Quick sanity check on the numbers**

Inspect `benchmarks/artifacts/2026-04-29-phase2/results/*-report-github.md`. Sanity gates:
- `Multi_Packet_Enqueue` should be roughly 1.5x to 2.5x of typed `Multi_Enqueue` (6.89 ns from Phase 1 baseline) — span-pass overhead. If 10x off, something is wrong.
- `Filter_Packet_Reject` should be ≤ 5 ns (delegate-only path).
- `Filter_Packet_Pass` should be roughly 2x typed `Filter_Pass` (6.22 ns).
- `Push_Single` (packet) at 100k items should complete in < 5 seconds wall time.

If any gate fails, investigate before commit. Numbers far outside expected ranges may indicate a JIT issue (DCE, missing Volatile.Write) or accidental allocation on hot path — abort and report.

- [ ] **Step 3: Commit artifacts**

```bash
git add benchmarks/artifacts/2026-04-29-phase2/
git commit -m "test: Phase 2 BDN runs — packet-tree symmetry numbers w/Claude"
```

---

## Task 7: Update Master Plan + Cross-Check

**Files:**
- Modify: `docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md`
- Modify: `benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md`

- [ ] **Step 1: Mark Phase 2 done in master plan**

In the Phase Map table, replace the Phase 2 status cell (`⬜ pending`) with:

```markdown
✅ done — packet-tree symmetry BDNs added (M1 ForkPacket, M2 MultiPacket + IsHealthy, M4 FilterPacket, M9 SpscPacket throughput); numbers under `benchmarks/artifacts/2026-04-29-phase2/`
```

- [ ] **Step 2: Update gap report**

In `benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md` §3, mark M1, M2, M4, M9 as resolved. Replace each row with `~~MX~~ resolved` and append "Phase 2: BDN landed — see `benchmarks/artifacts/2026-04-29-phase2/`".

- [ ] **Step 3: Commit**

```bash
git add docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md \
        benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md
git commit -m "docs: mark Phase 2 done — packet-tree symmetry BDNs landed w/Claude"
```

---

## Task 8: Final Verification & PR Prep

**Files:** _none modified_

- [ ] **Step 1: Re-run full test suite**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failed (no production code changed; should match the develop baseline).

- [ ] **Step 2: Verify the branch contains expected commits**

Run: `git log --oneline develop..HEAD`
Expected: 6 commits — `test:` ×4 (one per BDN class), `test:` Phase 2 artifacts, `docs:` master plan + cross-check update.

- [ ] **Step 3: Verify no production source touched**

Run: `git diff develop..HEAD -- src/ tests/`
Expected: empty diff. No production or test source modified.

- [ ] **Step 4: Stop here — do not push**

The user pushes + opens PR + merges manually. Do NOT run `git push`. Do NOT run `gh pr create`.

---

## Self-Review Checklist (run before handoff)

- ✅ Spec coverage — M1, M2, M4, M9 closed (4 new BDN files + extension; one for each gap). M2 also includes `MultiPacketIsHealthyBenchmarks` (parity with typed `MultiIsHealthyBenchmarks`). M3 (Multi2PacketSink) deferred to Phase 6. Other Mx items deferred per master plan.
- ✅ No placeholders — every `Step` block contains complete code or exact command.
- ✅ Type consistency — `ByteCounterPipe` (existing), `PropagateByteCounterPipe` (new in Task 2), `TestSpscPacketSink` (new in Task 5) — names used identically across declaration and benchmark methods.
- ✅ Frequent commits — 6 commits, one per logical concern (one per BDN class + artifacts + docs).
- ✅ Branch name matches `chore/<yyMMdd>-<slug>` per project CLAUDE.md.
- ✅ Commit messages follow Conventional Commits + `w/Claude` suffix.
- ✅ Test gate (`dotnet test tests/Relay.Tests` 0 failures) enforced — but production code is unchanged so this is a no-regression check, not a fix-validation gate.
- ✅ No production source changes.
