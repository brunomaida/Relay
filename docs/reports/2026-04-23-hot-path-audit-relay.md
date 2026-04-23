# Hot Path Audit — Relay Library
**Date:** 2026-04-23  
**Scope:** Full library (`src/Relay/**`)  
**Auditor:** Claude Sonnet 4.6 / hot-path-audit skill  
**Tests at audit time:** 19/19 passing

---

## Hot Paths Identified

All code on the `Enqueue → Accept` axis qualifies. Call frequency is tick-rate (>10k/sec per CLAUDE.md). No profiler evidence was available; justification is the explicit design intent of the library.

| Path | File | Justification |
|------|------|---------------|
| `DispatchPipe.Enqueue` | `DispatchPipe.cs:33` | Primary producer entry point, tick-rate |
| `SpscRingBuffer.TryPublish` | `SpscRingBuffer.cs:65` | Called by `SpscQueuePipe.Accept` on every Enqueue |
| `SpscRingBuffer.TryConsume` | `SpscRingBuffer.cs:82` | Consumer inner loop, up to 256/batch |
| `SpscQueuePipe.IsHealthy` | `SpscQueuePipe.cs:57` | Called once per Enqueue as pre-gate |
| `FanOutPipe.Accept` + `FanOut2Pipe.Accept` | `FanOutPipe.cs:35,68` | Fan-out on every broadcast Enqueue |
| `FilterPipe.Accept` | `FilterPipe.cs:31` | Predicate eval per item |
| `RamPipe.Accept` | `RamPipe.cs:38` | Last-resort fallback, active during backend failure |

Consumer thread (`ConsumeLoop`) is warm path, not direct producer hot path.

---

## Findings

### A1 — Double Volatile.Read per Enqueue (MEDIUM)

**Files:** `SpscQueuePipe.cs:57`, `SpscRingBuffer.cs:43–46`, `SpscRingBuffer.cs:65–74`

`SpscQueuePipe.IsHealthy` calls `_ring.IsFull`, which executes `Volatile.Read(ref _head.Value)`. Then `Accept` calls `TryPublish`, which **also** executes `Volatile.Read(ref _head.Value)` unconditionally.

On the hot path (healthy, ring not full):
- `IsHealthy`: 1× `Volatile.Read(head)` → ~15c (or ~5c L1 hit)
- `TryPublish`: 1× `Volatile.Read(head)` → ~15c (or ~5c L1 hit)
- Total: **2× Volatile.Read** of the same value, each a potential cross-core invalidation when the consumer is running concurrently.

**Fix:** Remove `IsFull` from `IsHealthy` on `SpscQueuePipe<T>`. Let `TryPublish` be the sole arbiter of ring capacity. Semantics are preserved: when ring is full, `TryPublish` returns false → `Accept` returns false → `Enqueue` falls to `Next`. `IsHealthy` becomes a pure backend-health indicator.

```csharp
// SpscQueuePipe.cs:57 — before
public override bool IsHealthy => _healthy && !_ring.IsFull;

// after
public override bool IsHealthy => _healthy;
```

Saves 1× `Volatile.Read` (~15c worst-case, ~5c L1-warm) on every successful Enqueue. No semantic regression for the fallback chain.

**Side note:** `TryPublish` can be further optimized with a cached head value (standard SPSC optimization): read `_head` only when `_tail - _cachedHead >= Capacity`. This would save the `Volatile.Read` in the common case (ring not near full).

---

### A4 — Ring buffer bounds check not explicitly eliminated (LOW)

**File:** `SpscRingBuffer.cs:72`, `SpscRingBuffer.cs:89`

```csharp
_buffer[tail & _mask] = item;   // TryPublish
item = _buffer[head & _mask];   // TryConsume
```

The JIT can prove `(x & mask) < capacity` where `mask = capacity - 1` (power-of-two), but this proof requires pattern recognition of the bitwise-AND idiom. If the JIT fails to eliminate the bounds check, each access incurs a compare + branch.

To guarantee elimination: use `Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), (nint)(tail & _mask))`. At present no codegen evidence confirms or denies the JIT is eliminating the check. Low severity; optimize only with disassembly proof.

---

### D14 — Delegate call overhead in FilterPipe (LOW)

**File:** `FilterPipe.cs:18,33`

`_predicate` is `Predicate<T>` (a delegate). Delegate invocation goes through an indirect call via `_target` + `_methodPtr` (~3–5c overhead vs a direct call). For high-frequency filter paths this adds up.

Alternative API: `FilterPipe<T, TPredicate>` where `TPredicate : struct, IPredicate<T>` allows JIT to devirtualize and inline the predicate at zero call overhead. Trade-off: API verbosity. Low priority unless FilterPipe is measured on the critical path.

---

### E19 — `FanOutPipe.Accept` virtual dispatch per child (INFO — by design)

**File:** `FanOutPipe.cs:37`

`foreach (var c in _children) c.Enqueue(in item)` — each `c.Enqueue` is a virtual call through `DispatchPipe<T>`. JIT cannot devirtualize since `c` is the abstract base. For 2 children this is 2 virtual calls vs `FanOut2Pipe`'s 0 (when children are sealed).

This is documented and `FanOut2Pipe<T,TC1,TC2>` is the provided mitigation. **No action required.**

---

### E19b — `MmfPipe.WriteToBackend` uses checked accessor API (LOW)

**File:** `MmfPipe.cs:46`

```csharp
_view.Write(_position, ref Unsafe.AsRef(in item));
```

`MemoryMappedViewAccessor.Write` performs an internal bounds check on every call. Since `_position` is only incremented by `EntrySize` (checked in `IsHealthy` before overflow), the bounds check is provably redundant.

Alternative: acquire the native pointer once in the constructor via `SafeMemoryMappedViewHandle.AcquirePointer` and use `Unsafe.CopyBlockUnaligned` directly. This eliminates the per-call bounds check. This is on the consumer thread (warm path, not producer hot path), so impact is moderate.

---

### F21 — `RamPipe` fields are non-volatile (INFO — by contract)

**File:** `RamPipe.cs:21–22`

`_head` and `_tail` are plain `long` with no volatile/fence. The contract ("call from a single thread") is documented in both the XML summary and CLAUDE.md. However, if a caller uses `RamPipe` in a chain where the producer calls `Enqueue` concurrently with `DrainTo`, data races will occur silently.

No fix required; contract is clear. Consider a DEBUG assertion (similar to `PipeConstraints`) to detect multi-threaded misuse.

---

### F22 — No thread-affinity API for consumer thread (LOW)

**File:** `SpscQueuePipe.cs:71–82`

Consumer thread `Priority = BelowNormal`. No core pinning. For latency-critical deployments, OS migration of the consumer thread invalidates L1/L2 cache. Typical fix: expose an optional `CoreAffinity` parameter to `Start()` and call `Thread.BeginThreadAffinity()` + P/Invoke `SetThreadAffinityMask`. Out of scope until a user requests it; worth tracking.

---

### H24a — No tests for `FilterPipe` (MEDIUM)

**Files:** `tests/` — no `FilterPipeTests.cs`

`FilterPipe` has two behavioral invariants:
1. Items passing the predicate reach `_downstream`.
2. Items failing the predicate are silently consumed — they do NOT reach `Next`.

Invariant 2 is critical (avoids spurious fallback triggers) and is completely untested. A single test verifying that failed-predicate items don't appear in `Next` would close this gap.

---

### H24b — No zero-allocation assertions in tests (LOW)

No test uses `GC.CollectionCount(0)` before/after a burst to assert zero allocations on the hot path. An allocation regression (e.g., a boxing change or delegate capture) would be invisible in CI.

---

### H24c — No tests for `MmfPipe` (HIGH)

**File:** `Pipes/MmfPipe.cs` — zero test coverage

`MmfPipe` has unique behavior: it is the only pipe with capacity-only failure (no IOException, no recovery). Its `IsHealthy` override is different from the base. No test covers:
- Basic write and readback
- Capacity exhaustion (`IsHealthy` → false)
- Items routed to `Next` when capacity is full

---

### H24d — No tests for `TcpPipe` (HIGH)

**File:** `Pipes/TcpPipe.cs` — zero test coverage

`TcpPipe` has reconnect logic and backoff. No test covers even the basic write path (local loopback socket). A minimal test using `TcpListener` on `127.0.0.1` would cover the happy path and verify the send buffer is flushed.

---

### H26 — No stress tests (HIGH)

No long-running tests (minutes) to detect:
- Memory growth (native memory leak in `RamPipe.Dispose` omission path)
- GC pressure under sustained throughput
- Consumer thread degradation or livelock

---

### H27 — No benchmark tests (HIGH)

**Files:** `tests/` — no BenchmarkDotNet project

The CLAUDE.md states specific cycle budgets (`TryPublish ~25c`, `Enqueue depth-1 ~32c`) but there is no benchmark to verify or track these. Any change to `SpscRingBuffer`, `DispatchPipe.Enqueue`, or `SpscQueuePipe.IsHealthy` could silently regress these targets.

Required: a `benchmarks/Relay.Benchmarks` project with BenchmarkDotNet covering:
- `DispatchPipe.Enqueue` (single pipe, hot cache)
- `SpscRingBuffer.TryPublish` + `TryConsume` roundtrip
- `FanOutPipe.Accept` (2 children) vs `FanOut2Pipe.Accept`
- End-to-end throughput: producer vs consumer thread (FileStreamPipe, TcpPipe)

---

## Summary Table

| # | Dimension | Severity | Component | Issue | Action |
|---|-----------|----------|-----------|-------|--------|
| A1 | CPU Cycles | MEDIUM | `SpscQueuePipe.IsHealthy` + `TryPublish` | 2× `Volatile.Read(head)` per Enqueue | Remove `IsFull` from `IsHealthy` |
| A4 | Bounds Check | LOW | `SpscRingBuffer.TryPublish/TryConsume` | Bounds check elimination unverified | Verify JIT output; consider `Unsafe.Add` |
| D14 | Closures | LOW | `FilterPipe._predicate` | Delegate indirect call per item | CRTP generic predicate (optional API change) |
| E19b | Inlining | LOW | `MmfPipe.WriteToBackend` | `MemoryMappedViewAccessor.Write` bounds check | Direct unsafe pointer via `AcquirePointer` |
| F21 | Lock-Free | INFO | `RamPipe._head/_tail` | Non-volatile; single-thread contract | Add DEBUG concurrency assertion |
| F22 | Thread Affinity | LOW | `SpscQueuePipe.Start()` | No core pinning API | Expose optional `coreAffinity` param |
| H24a | Unit Tests | MEDIUM | `FilterPipe` | No tests for predicate-fail → not-to-Next | Add `FilterPipeTests` |
| H24b | Unit Tests | LOW | All hot-path pipes | No zero-alloc GC assertions | Add allocation-count assertions in burst tests |
| H24c | Unit Tests | HIGH | `MmfPipe` | Zero coverage | Add `MmfPipeTests` with capacity exhaustion |
| H24d | Unit Tests | HIGH | `TcpPipe` | Zero coverage | Add `TcpPipeTests` with loopback socket |
| H26 | Stress Tests | HIGH | All pipes | No long-running tests | Add stress test project |
| H27 | Benchmarks | HIGH | All hot paths | No BenchmarkDotNet suite | Add `Relay.Benchmarks` project |

---

## Category Scores

| Group | Score | Notes |
|-------|-------|-------|
| A. CPU & Computation | 7/10 | Double-volatile issue is real; rest is excellent |
| B. Memory Layout | 10/10 | PaddedLong 128B, POH pinned, sequential access |
| C. Allocation & GC | 10/10 | Zero alloc steady state, POH, no boxing possible |
| D. Language Runtime | 9/10 | `in T` everywhere; delegate in FilterPipe is minor |
| E. Compiler & JIT | 8/10 | AggressiveInlining on all hot methods; sealed types |
| F. Concurrency | 9/10 | SPSC-correct volatile-only; RamPipe contract clear |
| G. System Boundary | 9/10 | Batched I/O; RDTSC clock |
| H. Test Validation | 4/10 | Critical gaps: MmfPipe/TcpPipe coverage, no benchmarks |

---

## Verdict

**Architecture is sound.** The library correctly applies SPSC ring, cache-line padding, POH pinning, in-parameter passing, and AggressiveInlining across all hot paths. Steady-state allocations are zero; no locks, no LINQ, no async on any dispatch path.

**Two actionable improvements:**
1. **A1 (MEDIUM, ~15c win per Enqueue):** Remove `IsFull` from `SpscQueuePipe.IsHealthy` to eliminate redundant `Volatile.Read`. Single-line change; no behavioral regression.
2. **Test coverage (HIGH):** `MmfPipe`, `TcpPipe`, `FilterPipe` behavioral invariants, and a BenchmarkDotNet project are all missing. The stated cycle budgets in CLAUDE.md are unverifiable without benchmarks.
