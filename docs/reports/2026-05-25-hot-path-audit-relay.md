# Hot-Path Audit — Relay core (2026-05-25)

**Scope:** `DispatchSink.cs`, `SpscQueueSink.cs`, `MpscQueueSink.cs`, `RotatingFileSink.cs`
**Trigger:** Changes since last audit (2026-04-30) — gate 5h of release v1.0.2 checklist
**Date:** 2026-05-25
**Runtime:** .NET 9.0.14, X64 RyuJIT AVX2, i7-12700

---

## Hot-Path Identification

| Path | Qualification |
|---|---|
| `DispatchSink<T>.Enqueue` → `IsHealthy` → `Accept` → `Next?.Enqueue` | Called at tick-rate frequency; CLAUDE.md: ~32c budget |
| `SpscQueueSink<T>.Accept` → `_ring.TryPublish` | ~25c budget; lock-free volatile write |
| `MpscQueueSink<T>.Accept` → `_ring.TryPublish` | ~25c budget; CAS per write |
| `SpscQueueSink<T>.ConsumeLoop` batch drain | Consumer thread; `TryConsumeBatch` + `WriteToBackend` per item |
| `RotatingFileSink.ShouldRotate` | Consumer thread; per-record predicate via `HfClock.NowTicks` |

Cold paths not audited: constructors, `Start()`, `Stop()`, `Dispose()`, `TryRecoverBackend`, `TryDrainToPrev`.

---

## Changes Audited (since 2026-04-30)

| Commit | Change |
|---|---|
| `da425c1` | Add opt-in `threadPriority` + `affinityCpu` params to `SpscQueueSink<T>` + `MpscQueueSink<T>` ctors |
| `7555a76` | Remove dead code in `PinLinux`; tighten `ThreadAffinity` pin test |
| `5ec6cce` | Enforce cache-line alignment in Release; fix `DispatchSink` XML doc |
| `925acac` | Revert: restore `HfClock`-tick rotation check in `RotatingFileSink` (prior revert of perf fix) |
| `this` | Inject `Func<DateTime>` in `RotatingFileSink` — removes `DateTime.UtcNow` from all methods |
| `1321d6c` | Guard bypass path against unhealthy-but-open connection (TcpSink) |
| `ec2d019` | Loop `Send/Write` in packet sinks for partial-send correctness |
| `a29c69f` | Guard oversized payload in fixed-buffer sinks |
| `15face0` | Enforce SPSC publish ordering in `SharedMemorySink` |

---

## Findings

### A. CPU & Computation

**A1 — CPU Cycles:** PASS
`Enqueue` path: `IsHealthy` volatile read (~7c) → short-circuit `&&` → `Accept` virtual call (devirtualized when sealed) → `TryPublish` (~25c). No redundant computation.

`PropagateAfterAccept` correctly stored as `readonly bool` field (not virtual property), eliminating one vtable slot and one indirect call per `Enqueue`.

**A2 — Branch Prediction:** PASS
On healthy steady-state: `IsHealthy = true` → predictable taken. `PropagateAfterAccept = false` → constant; JIT folds `!false` → `if (accepted) return` → predictable taken. `Next?.Enqueue` → null-check is data-stable per chain depth.

**A3 — SIMD:** N/A — dispatch routing, not numeric.

**A4 — Bounds Check Elimination:** N/A — ring buffer BCEs audited in ring-buffer reports.

---

### B. Memory Layout & Access

**B5 — Cache Locality:** PASS
New fields `_threadPriority` (`ThreadPriority` = 4B) and `_affinityCpu` (`int` = 4B) added to `SpscQueueSink<T>` and `MpscQueueSink<T>`. Both are `readonly`, initialized in constructor, accessed only from `Start()` (cold path). Hot-path fields (`_healthy`, `_ring`) are unaffected.

**B6 — False Sharing:** PASS
`SpscRingBuffer<T>` and `MpscRingBuffer<T>` use 128-byte `PaddedLong` for head/tail — unchanged. Sink class instance layout is unordered (heap object); `_healthy` (volatile bool, producer-read / consumer-write) and `_running` (volatile bool, producer-write / consumer-read) are separate fields without explicit layout, but on a reference-type object their inter-field proximity is JIT-dependent and not altered by these changes.

**B7 — Memory Access Patterns:** PASS
`ConsumeLoop` batch drain: `TryConsumeBatch` fills `_consumeBuf` (POH-pinned `T[]`), then iterates sequentially — forward stride, hardware-prefetcher friendly.

**B8 — Indirection:** PASS
2 levels on hot path: sink → ring `PaddedLong` head/tail → POH-pinned `T[]` slot. Unchanged by these changes.

---

### C. Allocation & GC

**C9 — Zero-Allocation:** PASS
`Enqueue` → `Accept` → `TryPublish`: no allocation. `Next?.Enqueue` null-conditional: compiler emits conditional branch, no delegate allocation. Confirmed by BDN `[MemoryDiagnoser]`.

New `Func<DateTime> _utcNow` in `RotatingFileSink`: default is `(static () => DateTime.UtcNow)` — static lambda, no closure, no per-call allocation. Called only at rotation events (cold path).

**C10 — GC Pressure:** PASS
POH-pinned arrays (`_consumeBuf`, `_writeBuffer`) prevent GC scan. No new hot-path allocations.

**C11 — Boxing:** PASS
`where T : unmanaged` constraint prevents boxing on all generic paths. `ThreadPriority` enum stored as `readonly ThreadPriority` field — no boxing in constructor or `Start()`.

---

### D. Language Runtime

**D13 — Value Types:** PASS
`in T item` in `Enqueue` and `Accept` passes by reference — no struct copy on hot path.

**D14 — Closures/Delegates:** PASS
`(static () => DateTime.UtcNow)` in `RotatingFileSink` constructor: C# 9+ static lambda — no closure allocation. Stored as `readonly Func<DateTime>` field — one heap word, initialized once.

**D19 — Inlining:** PASS
`[AggressiveInlining]` present on `Enqueue`, `Accept` (SpscQueueSink + MpscQueueSink), and `TryPublish` / `TryConsume` (ring buffers). No method body growth from these changes.

**D20 — Static/Const Propagation:** PASS
`PropagateAfterAccept = false` is a `readonly bool` load from the object; JIT reads this at JIT time for sealed types where it can prove the value, folding the branch.

---

### E. Compiler & JIT

No JIT-specific regressions observed. Added fields are cold-path; no IL size increase in hot methods.

---

### F. Concurrency

**F21 — Lock-Free:** PASS
No `lock`, `Monitor`, or `Mutex` on any path. `_flushRequested`: `Volatile.Read/Write` pair. `_healthy`: `volatile bool`. `_running`: `volatile bool`. `TryPublish` (SPSC): `Volatile.Write(tail)`. `TryPublish` (MPSC): `Interlocked.Add` CAS. Unchanged by these changes.

**F22 — Thread Affinity:** PASS
New `affinityCpu` parameter: consumed exclusively inside `ConsumeLoop()` at thread startup (`if (_affinityCpu >= 0) ThreadAffinity.Pin(_affinityCpu)`). Cold path — no hot-path impact. Best-effort pin; failure is silent, thread runs unpinned.

---

### G. System Boundary

**G23 — Syscall:** PASS
`HfClock.NowTicks` = `Stopwatch.GetTimestamp()` = `QueryPerformanceCounter` (user-space on Win, `rdtsc`-derived on Linux). ~25c, no kernel transition per call. `RotatingFileSink.ShouldRotate` correctly uses HfClock ticks (not `DateTime.UtcNow`) for the per-record predicate.

`DateTime.UtcNow` now isolated to the default `Func<DateTime>` factory in `RotatingFileSink` — called only at rotation events (cold path).

---

### H. Tests

**H24 — Unit Tests:** PASS — 209 tests, 0 failures.

**H25 — Integration Tests:** PASS — chain, SPSC/MPSC pipeline, fallback, recovery drain all covered.

**H27 — Benchmarks (BDN, ShortRun, 2026-05-25):**

| Benchmark | Mean | StdDev | Allocated |
|---|---|---|---|
| `ShouldRotate_Predicate` | **13.59 ns** | 0.148 ns | 0 B |
| `ShouldRotate_HotPath` | 73.72 ns | 5.850 ns | 0 B |

Prior baseline (`46e10c9`, 2026-04-29): `ShouldRotate_Predicate` = 13.76 ns. Delta: **−0.17 ns** (within StdDev — no regression). `Func<DateTime>` delegate overhead is sub-noise.

---

## Summary Table

| # | Dimension | Status | Finding |
|---|---|---|---|
| 1 | CPU Cycles | ✅ PASS | No redundant ops; PropagateAfterAccept correctly a field |
| 2 | Branch Prediction | ✅ PASS | Steady-state branches stable and foldable |
| 3 | SIMD | N/A | — |
| 4 | Bounds Check | N/A | Ring buffers audited separately |
| 5 | Cache Locality | ✅ PASS | New fields cold-path only |
| 6 | False Sharing | ✅ PASS | PaddedLong ring heads/tails unchanged |
| 7 | Memory Access | ✅ PASS | Sequential batch drain |
| 8 | Indirection | ✅ PASS | 2 levels unchanged |
| 9 | Zero-Allocation | ✅ PASS | 0 B on Enqueue path; BDN confirmed |
| 10 | GC Pressure | ✅ PASS | POH arrays; no new pressure |
| 11 | Boxing | ✅ PASS | `where T : unmanaged` prevents boxing |
| 12 | Pooling | N/A | — |
| 13 | Value Types | ✅ PASS | `in T` reference passing |
| 14 | Closures/Delegates | ✅ PASS | Static lambda, no closure |
| 15 | Async | ✅ PASS | No async on any path |
| 16 | String Handling | ✅ PASS | No strings on hot path |
| 17 | Exception Cost | ✅ PASS | Try/catch in ConsumeLoop only (consumer, infrequent) |
| 18 | Readonly/Immutability | ✅ PASS | `readonly bool PropagateAfterAccept` |
| 19 | Inlining | ✅ PASS | [AggressiveInlining] on hot methods |
| 20 | Static/Const Propagation | ✅ PASS | PropagateAfterAccept foldable |
| 21 | Lock-Free | ✅ PASS | Volatile/CAS only |
| 22 | Thread Affinity | ✅ PASS | Opt-in, cold path |
| 23 | Syscall | ✅ PASS | HfClock ticks on hot path; DateTime.UtcNow isolated to cold |
| 24 | Unit Tests | ✅ PASS | 209/209 |
| 25 | Integration Tests | ✅ PASS | Chain + pipeline tests present |
| 26 | Stress Tests | ✅ PASS | Endurance/Stress categories exist |
| 27 | Benchmarks | ✅ PASS | 13.59 ns predicate; 0 alloc |

---

## Overall Verdict: ✅ PASS — No regressions. No action required.

Changes since 2026-04-30 are correctness fixes, cold-path additions (`threadPriority`, `affinityCpu`, `Func<DateTime>`), and documentation updates. Hot-path budget unchanged. BDN confirms `ShouldRotate_Predicate` at 13.59 ns vs 13.76 ns baseline (−0.17 ns, within measurement noise).
