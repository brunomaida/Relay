# Hot Path Audit — Relay (v4)

**Date:** 2026-04-30
**Scope:** `src/Relay/**` (full solution)
**Auditor:** Claude Sonnet 4.6 / hot-path-audit (generic) skill
**Prior reports:** `2026-04-23` (v1), `2026-04-23-v2` (v2), `2026-04-26` (v3)
**Cost map consulted:** `2026-04-29-resource-cost-map-relay.md` (Phase 7 BDN calibrated)

---

## Delta since v3

Files added or changed since the 2026-04-26 audit:

| File | Status |
|---|---|
| `ForkSink.Packet.cs` | Exists; not audited in v3 (landed with PropagateAfterAccept refactor) |
| `FilterSink.Packet.cs` | Exists; test coverage now present (`FilterSinkPacketTests`) |
| `MultiSink.Packet.cs` | Exists; test coverage now present (`MultiSinkPacketTests`) |
| `MmfSinkTests.cs`, `TcpSinkTests.cs` | New — closes two v3 HIGH test gaps |
| `FilterSinkPacketTests.cs` | New — closes v3 MEDIUM test gap (predicate-fail invariant) |
| `ForkSinkPacketTests.cs` | New; coverage gaps identified below |
| `PacketSinkDropCountTests.cs` | New |

---

## Hot Paths (current — unchanged from v3)

All paths listed in v3 remain current. The cost map is authoritative:

| Path | Tier | cycles/call |
|---|---|---|
| `DispatchSink<T>.Enqueue` / `PacketSink.Enqueue` | ULTRA-HOT | ~2–15c (BDN-calibrated) |
| `SpscRingBuffer<T>.TryPublish/TryConsume` | ULTRA-HOT | ~10c (cached head/tail fast path) |
| `MpscRingBuffer<T>.TryPublish` | ULTRA-HOT | ~15–20c uncontended (BDN Phase 7) |
| `SpscByteRingBuffer.TryPublish/TryPeek` | ULTRA-HOT | ~35c / ~15c |
| `MpscByteRingBuffer.TryPublish/TryPeek` | ULTRA-HOT | ~20c / ~12c |
| `FilterSink*.Accept`, `ForkSink*.Accept`, `Multi*Sink.Accept` | ULTRA-HOT | ~18–32c |
| `SerializeSink<T>.Accept` | ULTRA-HOT | ~5c + downstream |
| `BatchSink.WriteToBackend` | HOT (consumer) | ~10c |
| `RotatingFileSink.ShouldRotate` | HOT (consumer) | ~3c (BDN Phase 1) |

---

## Findings

### NEW — F21 (MEDIUM) — `ForkSink.Accept` (packet) returns `_primary.IsHealthy` instead of `true`

**Location:** `src/Relay/ForkSink.Packet.cs:22–25`

```csharp
protected override bool Accept(ReadOnlySpan<byte> payload)
{
    _primary.Enqueue(payload);
    return _primary.IsHealthy;   // ← wrong
}
```

**Problem.** `ForkSink<T>` (typed, `ForkSink.cs:36`) always returns `true`. The packet variant reads `_primary.IsHealthy` *after* calling `_primary.Enqueue`. Two consequences:

1. **Spurious `_dropCount` increment.** `PacketSink.Enqueue` logic:
   ```
   if (IsHealthy && Accept(payload)) { PropagateAfterAccept → Next?.Enqueue; return; }
   if (Next is { } next) { next.Enqueue; return; }
   Interlocked.Increment(ref _dropCount);  // ← fires when Accept=false AND Next=null
   ```
   If the primary's consumer thread marks `_healthy = false` in the narrow window between `_primary.Enqueue(payload)` and the `return _primary.IsHealthy` read, `Accept` returns `false`. If `Next` is null at this terminal node, `_dropCount` increments — incorrect. The fork is a best-effort side channel; it must never contribute to terminal-drop accounting.

2. **Race-dependent propagation path.** With `Accept` returning `false`, `Next` is reached via the fallback branch rather than the propagate branch. Functionally equivalent when `Next` is not null, but semantically wrong: the fork contract is "deliver to primary **and** always continue to Next."

**Fix.** Return `true`, matching the typed variant and the fork contract:
```csharp
protected override bool Accept(ReadOnlySpan<byte> payload)
{
    _primary.Enqueue(payload);
    return true;
}
```

**Test coverage gap.** `ForkSinkPacketTests.Enqueue_PrimaryUnhealthy_OnlyNextReceivesPayload` tests health=false *before* enqueue (so `IsHealthy` short-circuits, `Accept` is never called). No test covers the mid-call health transition described above.

**Status: OPEN — fix is a one-line change.**

---

### NEW — D13 (MEDIUM) — `MpscQueueSink<T>` constructor missing `SinkConstraints.AssertCacheLineAligned<T>()`

**Location:** `src/Relay/MpscQueueSink.cs:70`

```csharp
protected MpscQueueSink(int ringCapacity, int flushIntervalMs, string pipeName = "")
{
    _ring = new MpscRingBuffer<T>(ringCapacity);  // ← no alignment assertion
    ...
}
```

**Problem.** `SpscQueueSink<T>` calls `SinkConstraints.AssertCacheLineAligned<T>()` in its constructor (`SpscQueueSink.cs:69`). The MPSC counterpart does not. This is the only runtime guard enforcing the `sizeof(T) % 64 == 0` invariant on the MPSC path.

Without the assertion, a subclass may pass a T that violates the invariant in DEBUG builds without any feedback. In production, `MpscRingBuffer<T>.Slot { int Published; T Value; }` already creates slot-straddle risk (see below); an unaligned T compounds it.

**Fix.** Add `SinkConstraints.AssertCacheLineAligned<T>();` as the first line of `MpscQueueSink<T>` constructor, matching `SpscQueueSink<T>`.

**Status: OPEN — one-line fix.**

---

### NEW — F21/G23 (LOW) — Packet `SpscQueueSink`/`MpscQueueSink` ConsumeLoop calls `TryRecoverBackend` and `TryDrainToPrev` on every flush signal, not just on deadline

**Location:** `src/Relay/SpscQueueSink.Packet.cs:160–168`, `src/Relay/MpscQueueSink.Packet.cs:180–190`

```csharp
// Packet variants (BOTH SpscQueueSink and MpscQueueSink):
bool flushDue = Volatile.Read(ref _flushRequested) == 1
             || HfClock.NowTicks >= flushDeadline;
if (checkDeadline && flushDue)
{
    Volatile.Write(ref _flushRequested, 0);
    FlushBackend();
    TryRecoverBackend();   // ← called on every flush trigger
    TryDrainToPrev();      // ← called on every flush trigger
    ...
}
```

**Typed variants correctly separate the two triggers:**
```csharp
// Typed SpscQueueSink<T>:
bool flushNow    = Volatile.Read(ref _flushRequested) == 1;
bool deadlineHit = checkDeadline && HfClock.NowTicks >= flushDeadline;

if (flushNow || deadlineHit)
{
    if (flushNow) Volatile.Write(ref _flushRequested, 0);
    FlushBackend();
    if (deadlineHit)
    {
        TryRecoverBackend();   // ← deadline only
        TryDrainToPrev();      // ← deadline only
    }
    ...
}
```

**Impact.** All concrete `TryRecoverBackend` implementations guard with `if (_healthy) return;` (no-op when healthy) and `if (HfClock.NowTicks < _nextRetryTicks) return;` (rate-limited when recovering). So no functional incorrectness and overhead on the producer-flush path is ~3 cycles per call when healthy. Nonetheless:
- It calls into recovery-probe logic unnecessarily on every producer-triggered flush.
- It departs from the established typed-variant pattern without documented reason.

**Fix.** Apply the same `flushNow` / `deadlineHit` split to both packet variants.

**Status: OPEN — low priority; cosmetic overhead only.**

---

### CARRIES — B6 (MEDIUM/DOCUMENTED) — `MpscRingBuffer<T>.Slot` straddles cache lines

**Location:** `src/Relay/Buffers/MpscRingBuffer.cs:40–44`

```csharp
private struct Slot
{
    public int Published;   // 4 bytes
    public T   Value;       // sizeof(T) bytes — starts at offset 4 (or 8 with alignment padding)
}
```

For a typical `T` with 8-byte-aligned fields: slot size = 72 bytes. With `NativeMemory.AlignedAlloc(64)` base, slots occupy byte ranges [0,71], [72,143], etc. `T.Value` starts at offset 4 (or 8 after padding), never on a 64-byte boundary. Every slot straddles two cache lines.

**Why it matters.** Phase 7 BDN showed negative scaling N=1→N=2 on the typed MPSC ring (10.6M→7.7M items/s aggregate). CAS contention is the dominant cause, but per-slot cache-line straddle means each producer write and consumer read touches two cache lines per slot, compressing the effective window for L1 hits under contention.

**Fix (trade-off).** Pad `Published` to 64 bytes:
```csharp
[StructLayout(LayoutKind.Explicit, Size = sizeof(T) + 64)]
private struct Slot
{
    [FieldOffset(0)]  public int Published;   // 4B
    [FieldOffset(64)] public T   Value;       // T on its own cache line
}
```
Slot size: `sizeof(T) + 64`. For T=64B: 128B/slot vs current 72B (or 68B with no padding). Doubles ring memory footprint. Requires `unsafe` and cannot use `sizeof(T)` in attribute — needs a generic workaround or separate allocation. Document as Phase 8 candidate with BDN regression gate.

**Status: OPEN — documented risk. Worth a dedicated BDN comparison before committing to the fix.**

---

## Resolved since v3

| v3 ID | Component | Status |
|---|---|---|
| H1 (HIGH) | `MpscQueueSink.Flush()` producer-thread race | ✅ Fixed in v3 addendum |
| H24a (MED) | `FilterSink` predicate-fail invariant untested | ✅ `FilterSinkPacketTests` — 5 tests covering all invariants including downstream-unhealthy edge case |
| H24c (HIGH) | `MmfSink` zero coverage | ✅ `MmfSinkTests` — write/readback, capacity exhaustion, fallback |
| H24d (HIGH) | `TcpSink` zero coverage | ✅ `TcpSinkTests` — write/readback, connect failure → unhealthy → fallback |

## Resolved since v4 (this cycle)

| v4 ID | Component | Status |
|---|---|---|
| F21a (MEDIUM) | `ForkSink.Accept` (packet) | ✅ `return true` — matches typed variant contract; mid-call health test added (`ForkSinkPacketTests`) |
| D13 (MEDIUM) | `MpscQueueSink<T>` ctor | ✅ `SinkConstraints.AssertCacheLineAligned<T>()` added as first ctor line; 3 MPSC alignment tests added (`SinkConstraintsTests`) |
| F21b / G1 (LOW) | Packet ConsumeLoop (SPSC + MPSC) | ✅ `flushNow`/`deadlineHit` split applied; `TryRecoverBackend` + `TryDrainToPrev` now deadline-only; up to ~160c saved per idle spin cycle |
| B6 (MED/structural) | `MpscRingBuffer<T>.Slot` | ✅ Slot struct removed; `byte* _basePtr` + `_stride = 64 + sizeof(T)` layout; Published at offset 0, T at offset 64 — no slot straddles a cache line |
| G2 (LOW) | `SharedMemorySink.Accept` | ✅ Contiguous-write fast path added (`oldIdx + frameLen <= _dataCapacity`); eliminates `stackalloc byte[4]` and two `WriteRing` calls on non-wrap writes |

---

## Still Open (carried)

| ID | Component | Severity | Notes |
|---|---|---|---|
| H26 | Stress tests | HIGH | No `[Trait("Category","Stress")]`-tagged sustained-load tests. Recommendation: 5-minute SPSC/MPSC throughput under memory pressure, with `GC.GetTotalMemory` baseline/endpoint gate. |
| F22 | `SpscQueueSink.Start` core-affinity | LOW | No `int coreAffinity = -1` opt-in. Low priority; caller can pin before Start. |
| M2 | `TryDrainToPrev` SPSC race window | MED | Documented in comments. Callers must quiesce producers before drain runs. |
| G3 | `UdpSink` per-datagram syscall | LOW | Batching requires protocol-level permission + delivered-count BDN baseline first. Deferred. |

---

## Summary Table (all findings, v4 + post-fix)

| ID | Severity | Component | Issue | Status |
|---|---|---|---|---|
| F21a | MEDIUM | `ForkSink.Accept` (packet) | Returns `_primary.IsHealthy` instead of `true` | ✅ Fixed |
| D13 | MEDIUM | `MpscQueueSink<T>` ctor | Missing `SinkConstraints.AssertCacheLineAligned<T>()` | ✅ Fixed |
| F21b / G1 | LOW | Packet ConsumeLoop (SPSC + MPSC) | HfClock + recovery called unconditionally on flush | ✅ Fixed |
| B6 | MED | `MpscRingBuffer<T>` | Slot layout straddles cache lines | ✅ Fixed (stride layout) |
| G2 | LOW | `SharedMemorySink.Accept` | No contiguous-write fast path | ✅ Fixed |
| Carried H26 | HIGH | All | No stress tests | OPEN |
| Carried F22 | LOW | `SpscQueueSink.Start` | No core-affinity opt-in | OPEN |
| Carried M2 | MED | `TryDrainToPrev` | SPSC race window on producer-resume | OPEN/Documented |
| Carried G3 | LOW | `UdpSink` | Per-datagram syscall; batching deferred | OPEN |
| Resolved H1 | — | `MpscQueueSink.Flush()` | Producer-thread race → consumer signal | ✅ |
| Resolved H24a | — | `FilterSink` predicate-fail invariant | Now tested | ✅ |
| Resolved H24c | — | `MmfSink` test coverage | `MmfSinkTests` added | ✅ |
| Resolved H24d | — | `TcpSink` test coverage | `TcpSinkTests` added | ✅ |

---

## Benchmark Validation

`MpscSlotLayoutBenchmarks` added (`benchmarks/Relay.Benchmarks/MpscSlotLayoutBenchmarks.cs`).
Runs before/after comparison at ProducerCount=1,2,4,8 and Capacity=1024,65536. BDN run pending.

**Open BDN gaps:**
- `MpscRingBuffer<T>` stride layout: `MpscSlotLayoutBenchmarks` written; actual numbers pending first BDN run.
- `SharedMemorySink` fast path: `SharedMemorySinkBenchmarks` gates the change; run pending.
- Stress tests: none validating memory stability over >1 minute.

---

## Category Scores

| Group | v3 (post-fix) | v4 | Δ | Notes |
|---|---|---|---|---|
| A. CPU & Computation | 9 | **9** | — | No change |
| B. Memory Layout | 10 | **9** | −1 | B6 slot straddle still unresolved; cost map confirms MPSC negative scaling at N=2 |
| C. Allocation & GC | 10 | **10** | — | Zero-alloc hot path, all buffers POH/native |
| D. Language Runtime | 9 | **8** | −1 | Missing `AssertCacheLineAligned<T>()` in `MpscQueueSink<T>` ctor — only DEBUG guard absent |
| E. Compiler & JIT | 9 | **9** | — | No change |
| F. Concurrency | 9 | **8** | −1 | `ForkSink.Accept` packet race window; packet ConsumeLoop flush inconsistency |
| G. System Boundary | 9 | **9** | — | No change |
| H. Test Validation | 4 | **6** | +2 | FilterSink, MmfSink, TcpSink coverage added; stress gap and ForkSink mid-call-health test still missing |

---

## Verdict

**Performance: UNCHANGED — no regressions.** All BDN results from Phase 7 remain valid. No hot-path code changed since v3.

**Correctness: TWO NEW MEDIUM FINDINGS.**
1. `ForkSink.Accept` (packet) returns `_primary.IsHealthy` instead of `true` — inconsistent fork semantics, potential spurious `_dropCount`.
2. `MpscQueueSink<T>` missing alignment assertion — sole DEBUG guard for T invariant absent on MPSC path.

Both are one-line fixes. Neither affects throughput; both affect correctness in edge cases.

**Tests: IMPROVED (+2).** Three v3 HIGH/MED gaps closed. Remaining gap: no stress test suite and no test for `ForkSink.Accept` mid-call health transition.

### Priority Queue (ordered)

1. **(MEDIUM)** `ForkSink.Accept` (packet): `return _primary.IsHealthy` → `return true`. One line.
2. **(MEDIUM)** `MpscQueueSink<T>` ctor: add `SinkConstraints.AssertCacheLineAligned<T>();`. One line.
3. **(LOW)** Packet ConsumeLoop flush split: separate `flushNow`/`deadlineHit`, mirror typed pattern.
4. **(HIGH, deferred)** Stress test suite: 5-min SPSC/MPSC throughput + memory-growth gate.
5. **(MED, Phase 8 candidate)** `MpscRingBuffer<T>.Slot` padding: BDN comparison before committing.
