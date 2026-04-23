# Hot Path Audit — Relay Library (Post-Fix Pass)
**Date:** 2026-04-23  
**Scope:** Full library (`src/Relay/**`)  
**Auditor:** Claude Sonnet 4.6 / hot-path-audit skill (v2 — post-fix validation)  
**Tests at audit time:** 20/20 passing  
**Previous report:** `docs/reports/2026-04-23-hot-path-audit-relay.md`  
**Opus meta-review:** conducted and incorporated

---

## Corrections to v1 Report

Before the current state, the v1 audit contained one factual error identified by the Opus meta-review:

> **A1 cycle math was wrong.** `Volatile.Read` on x64 is an **acquire load** (`mov` + compiler barrier), NOT an `mfence`. Cost is ~4–5c on L1-warm, not ~15c. The ~15c figure in CLAUDE.md applies to `Volatile.Write` (release store = `xchg`/`lock`-equivalent). The savings from removing the redundant read in A1 are **~4–5c per Enqueue**, not ~15c.

All other v1 findings were confirmed as ✅ by the Opus review.

---

## Changes Applied Since v1

| ID | File | Change |
|----|------|--------|
| A1 | `SpscQueuePipe.cs:61` | `IsHealthy` → `_healthy` only; removes redundant `IsFull` acquire load |
| M1 | `SpscRingBuffer.cs:49–54` | Added `IsEmpty` (consumer-only, non-volatile head read) |
| M1 | `SpscQueuePipe.cs:191` | `ShouldKeepDraining` uses `_ring.IsEmpty` (eliminates 1 Volatile.Read) |
| M2 | `SpscQueuePipe.cs:173–177` | TryDrainToPrev race condition documented in comment |
| M4 | `RamPipe.cs` | `Dispose` guarded with `_disposed` flag — idempotent, no double-free |
| M6 | `FileStreamPipe.cs:75` | `TryRecoverBackend` catches `Exception` (was `IOException`) |
| M7 | `MmfPipe.cs` | `_position` published via `Volatile.Write`; read via `Volatile.Read` in `IsHealthy` |
| M10 | `SpscQueuePipeTests.cs` | Replaced no-op `RingFull_TriggersIsHealthyFalse` with `RingFull_FallsBackToNext_WhenConsumerNotStarted` |
| M10 | `RecoveryDrainTests.cs` | Added `RamPipe_Dispose_IsIdempotent` test |

---

## Hot Paths Audited

Same set as v1. `DispatchPipe.Enqueue` → `Accept` → `SpscRingBuffer.TryPublish` is the primary hot path at tick-rate frequency.

---

## Findings — Current State

### A1 — Double Volatile.Read per Enqueue → RESOLVED ✅

`SpscQueuePipe.IsHealthy` now returns `_healthy` only (single `volatile bool` acquire load, ~4c). The ring-full gate moved entirely to `TryPublish`, eliminating one acquire load per Enqueue.

**Hot-path flow (healthy, ring not full):**
1. `IsHealthy`: read `_healthy` (~4c) → true
2. `Accept → TryPublish`: `Volatile.Read(_head)` + write slot + `Volatile.Write(_tail)` (~25c total)
3. Total: **~29c** vs ~34c before

**Semantic note:** `IsHealthy` no longer reflects ring-full state. External `IsHealthy` checks for capacity probing are now inaccurate — by design. Ring-full items still correctly fall through to `Next` via `TryPublish` returning false.

---

### A4 — Bounds check in ring buffer indexing (LOW — open)

`_buffer[tail & _mask]` in `TryPublish` and `TryConsume`: `_mask` is an instance field (not derived from `_buffer.Length`). RyuJIT cannot prove `(tail & _mask) < _buffer.Length` through a field-equality chain, so bounds check elimination is unlikely. Each access likely incurs a compare + branch (~1c predicted branch, ~15c mispredicted).

**Status:** Open. Use `Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), (nint)(tail & _mask))` to guarantee elimination if profiler evidence confirms the check contributes meaningfully. Verify with disassembly (`dotnet-sos` or Sharplab) before acting.

---

### D14 — Delegate overhead in FilterPipe (LOW — open)

`_predicate(item)` = delegate indirect call (~3–5c) layered inside an already-virtual `Accept` dispatch. In isolation this is noise; only relevant if FilterPipe becomes a primary hot path rather than a downstream wrapper.

**Status:** Open. Actionable only with profiler evidence showing FilterPipe CPU contribution.

---

### E19b — `MmfPipe.WriteToBackend` uses checked MMF accessor (LOW — open)

`_view.Write(_position, ...)` performs internal bounds check per call (consumer thread / warm path). Direct pointer via `SafeMemoryMappedViewHandle.AcquirePointer` + `Unsafe.CopyBlockUnaligned` would eliminate it.

**Status:** Open. Consumer path only; benchmark before acting.

---

### F21 — `RamPipe` non-volatile head/tail (INFO — by contract)

Documented: single-thread contract. Not a bug.

---

### F22 — No thread-affinity API (LOW — open)

Consumer thread has no core-pinning. For strict latency deployments (HFT), thread migration invalidates L1/L2. Optional `coreAffinity` parameter on `Start()` would address this.

**Status:** Open. Low priority until requested.

---

### M2 — `TryDrainToPrev` concurrent producer race (MEDIUM — documented)

`TryDrainToPrev` calls `Prev.Enqueue` from the consumer thread. If the original producer concurrently resumes feeding `Prev`, two threads enter `Prev.Accept` simultaneously — SPSC violation. Race window = cache-coherency latency (~100–200ns). Documented in code comment.

**Full fix** requires either a quiescence protocol (producer signals stop before recovery drain) or an MPSC ring on the predecessor. Deferred pending a use-case that triggers this.

**Status:** Open / documented.

---

### H24a — `FilterPipe` behavioral invariants untested (MEDIUM — open)

Critical invariant: items failing the predicate MUST NOT reach `Next`. No test covers this. A single `FilterPipeTests.cs` with:
1. Passing item → reaches `_downstream`
2. Failing item → consumed silently, `Next.Accepted == 0`

**Status:** Open. Medium severity — this invariant is the only non-obvious semantic in the file.

---

### H24c — `MmfPipe` zero test coverage (HIGH — open)

No tests for: basic write/readback, capacity exhaustion (`IsHealthy → false`), fallback to `Next` when full, or `Volatile.Read/Write` correctness on `_position`.

**Status:** Open.

---

### H24d — `TcpPipe` zero test coverage (HIGH — open)

No tests for: basic write via loopback socket, reconnect backoff, send buffer flush.

**Status:** Open.

---

### H26 — No stress tests (HIGH — open)

No long-running tests. `RamPipe.Dispose` idempotency and `MmfPipe._position` race cannot be verified under sustained load.

**Status:** Open.

---

### H27 — No benchmark tests (HIGH — open)

No BenchmarkDotNet project. The stated cycle budget (`Enqueue ~32c`, `TryPublish ~25c`) remains unverifiable. The A1 optimization (~4–5c) cannot be confirmed without measurement.

**Status:** Open.

---

### M8 — `PipeConstraints` allows 32B T (INFO — not a bug)

Opus flagged 32B structs as "straddling cache lines." This is incorrect: a 32B struct at a 32B-aligned address occupies bytes 0–31 of its cache line — entirely within one 64B line. Two consecutive 32B entries share a cache line but neither straddles it. Sequential ring-buffer access benefits from this (2 entries fetched per cache line, prefetcher works well). **No action required.**

---

## Summary Table — All Findings

| ID | Severity | Component | Issue | Status |
|----|----------|-----------|-------|--------|
| A1 | MEDIUM | `SpscQueuePipe.IsHealthy` | Double acquire-load on Enqueue hot path | ✅ FIXED |
| A4 | LOW | `SpscRingBuffer.TryPublish/TryConsume` | Bounds check not guaranteed eliminated | Open |
| D14 | LOW | `FilterPipe._predicate` | Delegate indirect call overhead | Open |
| E19b | LOW | `MmfPipe.WriteToBackend` | MMF accessor bounds check per call | Open |
| F21 | INFO | `RamPipe._head/_tail` | Non-volatile (single-thread contract) | Open/By design |
| F22 | LOW | `SpscQueuePipe.Start` | No core-affinity API | Open |
| M1 | LOW | `SpscRingBuffer.Count` | 2 acquire-loads in `ShouldKeepDraining` | ✅ FIXED |
| M2 | MEDIUM | `TryDrainToPrev` | SPSC race if producer resumes concurrently | Open/Documented |
| M3 | INFO | `SpscQueuePipe.Stop` | Deadline/running store ordering | ✅ Not a bug (x64 TSO + volatile) |
| M4 | MEDIUM | `RamPipe.Dispose` | Double-free on second Dispose call | ✅ FIXED |
| M6 | LOW | `FileStreamPipe.TryRecoverBackend` | IOException-only catch misses non-IO open failures | ✅ FIXED |
| M7 | MEDIUM | `MmfPipe._position` | Producer reads stale capacity via non-volatile field | ✅ FIXED |
| M8 | INFO | `PipeConstraints` | 32B allowance "straddles" — false positive | ✅ Not a bug |
| M10 | LOW | `SpscQueuePipeTests` | Ring-full test was a no-op | ✅ FIXED |
| H24a | MEDIUM | `FilterPipe` | Predicate-fail → not-to-Next invariant untested | Open |
| H24c | HIGH | `MmfPipe` | Zero test coverage | Open |
| H24d | HIGH | `TcpPipe` | Zero test coverage | Open |
| H26 | HIGH | All | No stress tests | Open |
| H27 | HIGH | All | No BenchmarkDotNet suite | Open |

---

## Category Scores — Post-Fix

| Group | v1 Score | v2 Score | Delta | Notes |
|-------|----------|----------|-------|-------|
| A. CPU & Computation | 7/10 | 8/10 | +1 | A1 fixed; A4 (BCE) still open |
| B. Memory Layout | 10/10 | 10/10 | — | No change |
| C. Allocation & GC | 10/10 | 10/10 | — | No change |
| D. Language Runtime | 9/10 | 9/10 | — | D14 noise |
| E. Compiler & JIT | 8/10 | 8/10 | — | E19b consumer-path only |
| F. Concurrency | 9/10 | 9/10 | — | M4+M7 fixed; M2 documented |
| G. System Boundary | 9/10 | 9/10 | — | No change |
| H. Test Validation | 4/10 | 5/10 | +1 | 2 new tests; MmfPipe/TcpPipe/benchmarks still absent |

---

## Verdict

**Seven bugs/issues fixed since v1.** The two medium-severity correctness issues (RamPipe double-free, MmfPipe unsynchronized capacity read) and the hot-path micro-opt (double acquire-load) are resolved. The SPSC race in `TryDrainToPrev` is documented.

**Remaining actionable priorities (ordered):**

1. **(HIGH) H24c/H24d** — Add `MmfPipeTests` and `TcpPipeTests`. MmfPipe in particular has a new `Volatile` synchronization path that has no test coverage.
2. **(HIGH) H27** — Add `benchmarks/Relay.Benchmarks` (BenchmarkDotNet). The A1 optimization claims ~4–5c savings that cannot be verified without measurement.
3. **(MEDIUM) H24a** — Add `FilterPipeTests` covering the predicate-fail → not-to-Next invariant.
4. **(MEDIUM) M2** — Design a quiescence protocol for `TryDrainToPrev` when the chain topology permits it, or restrict `Prev` wiring to chains where the producer is guaranteed quiescent during recovery.
5. **(LOW) A4** — Verify BCE on `_buffer[x & _mask]` with disassembly; migrate to `Unsafe.Add` if check is confirmed present.
