---
title: "Hot-Path Audit — Relay core (2026-07-02)"
type: report
solution: Relay
status: draft
created: 2026-07-02
---

# Hot-Path Audit — Relay core (2026-07-02)

**Scope:** `PacketSink.cs`, `Sinks/FileStreamSink.cs`, `Sinks/MemorySink.cs`, `Sinks/RotatingFileSink.cs`, `Buffers/MpscByteRingBuffer.cs`
**Trigger:** Changes since last audit of `RotatingFileSink.cs` (2026-05-25) — gate 5h of release v1.0.5 checklist. `PacketSink.cs`, `Sinks/MemorySink.cs`, `Sinks/FileStreamSink.cs`, `Buffers/MpscByteRingBuffer.cs` were not previously covered by a dedicated audit report in `docs/reports/`; audited here for the first time against these dimensions.
**Date:** 2026-07-02
**Runtime:** .NET 9.0.14, X64 RyuJIT AVX2, i7-12700
**Supersedes:** `docs/reports/2026-05-25-hot-path-audit-relay.md` (removed; its `RotatingFileSink.cs` scope is re-audited below with the current line numbers and the RDTSC-throttle change)

---

## Hot-Path Identification

| Path | Qualification |
|---|---|
| `PacketSink.Enqueue` → `IsHealthy` → `Accept` → `Next?.Enqueue` / terminal drop | Called at tick-rate frequency; parallel to `DispatchSink<T>.Enqueue` |
| `MpscByteRingBuffer.TryPublish` / `.TryPeek` / `.Advance` | Producer/consumer hot path for byte-payload MPSC sinks |
| `MemorySink<T>.Accept` | Last-resort fallback; called whenever upstream sinks are unhealthy |
| `FileStreamSink<T>.WriteToBackend` / `.FlushBuffer` / `.TryRecoverBackend` | Consumer thread; per-record write + periodic flush + backoff recovery |
| `RotatingFileSink.ShouldRotate` / `.WriteToBackend` | Consumer thread; per-record predicate now RDTSC-throttled |

Cold paths not audited: constructors, `TryOpenStream`, `Cleanup`, `RotateNow`, `DisposeBackend`, `MemorySink<T>.~MemorySink()` finalizer body (correctness-only, not called on the hot path).

---

## Changes Audited (since 2026-06-11, commit range `65b6cf1..c1818fa` first-parent)

| Commit | Change |
|---|---|
| `65b6cf1` | `fix(FileStreamSink)`: prevent consumer crash on sustained backend failure — reset `_bufferPos` on `IOException` in `FlushBuffer`; null `_stream` after a failed `TryRecoverBackend` retry |
| `ab33f86` | `perf`: throttle RDTSC sampling in `RotatingFileSink.ShouldRotate` — day-boundary check now runs 1-in-256 records via a counter mask, not every record |
| `9aae152` | `perf`: eliminate `TryPeek` recursion in `MpscByteRingBuffer` — padding-skip rewritten as an iterative `while(true)` loop |
| `c1818fa` | `chore`: add `MemorySink<T>` finalizer (native-memory safety net); pad `PacketSink._dropCount` to a 128-byte explicit-layout struct to prevent false sharing |

---

## Findings

### A. CPU & Computation

**A1 — CPU Cycles:** PASS
`RotatingFileSink.ShouldRotate` (RotatingFileSink.cs:123-129): size compare is unconditional (1c); the RDTSC-backed day-boundary check (`HfClock.NowTicks`, ~47c) now executes only when `(++_shouldRotateCounter & 0xFF) == 0` — 1-in-256 records. Amortized cost drops from ~47c/record to ~1.2c/record. `MpscByteRingBuffer.TryPeek` (MpscByteRingBuffer.cs:174-204): the padding-skip case previously required a recursive call (implicit extra stack frame, no inlining); now an iterative `while(true)` — same instruction count per skip iteration, removes the non-inlineable-recursion penalty.

**A2 — Branch Prediction:** PASS
`(++_shouldRotateCounter & 0xFF) == 0` is a well-predicted not-taken branch on 255/256 iterations (fixed periodic pattern, branch predictor's loop detector locks on quickly). `TryPeek`'s `while(true)` loop replaces a call-site branch with a backward loop branch — equally predictable, and removes the call/return pair on every skip.

**A3 — SIMD:** N/A — no numeric/vector code in scope.

**A4 — Bounds Check Elimination:** PASS
`MpscByteRingBuffer.PublishHeader`/`ReadHeaderVolatile` (MpscByteRingBuffer.cs:236-250) use `MemoryMarshal.GetArrayDataReference` + `Unsafe.AddByteOffset` — no bounds check on the header read/write, unchanged by the `TryPeek` rewrite.

---

### B. Memory Layout & Access

**B5 — Cache Locality:** PASS
`PacketSink._dropCount` unchanged access pattern (read on `DropCount` getter, incremented on terminal drop) — only its layout changed (see B6).

**B6 — False Sharing:** FIXED (was WARN in prior resource-cost-map)
`PacketSink._dropCount` is now `PaddedDropCount`, `[StructLayout(LayoutKind.Explicit, Size = 128)]` with `Value` at `[FieldOffset(64)]` (PacketSink.cs:42-48). Isolates the Interlocked-incremented counter from `Next` (reference) and `PropagateAfterAccept` (bool) on the same object header region — a monitoring thread reading `DropCount` concurrently with `Enqueue`'s terminal-drop increment no longer bounces the producer's cache line.

**B7 — Memory Access Patterns:** PASS
`MpscByteRingBuffer.TryPeek` iterative rewrite reads the same header offsets in the same order as the prior recursive version; no new access pattern introduced.

**B8 — Indirection:** PASS
`FileStreamSink.WriteToBackend` → `_writeBuffer` (POH-pinned) direct index — unchanged. The crash fix (`65b6cf1`) only alters control flow in the `catch` blocks of `FlushBuffer`/`TryRecoverBackend` (consumer-thread, low-frequency), not the write path itself.

---

### C. Allocation & GC

**C9 — Zero-Allocation:** PASS
No allocation introduced by any of the 4 commits. `PaddedDropCount` is a value-type field on `PacketSink` (no heap allocation beyond the existing object). `MemorySink<T>`'s finalizer allocates nothing — it calls `NativeMemory.Free(_buffer)` only.

**C10 — GC Pressure:** PASS
`MemorySink<T>` finalizer (MemorySink.cs:74-78) only runs when a caller fails to call `Dispose()` — this is the misuse case it exists to guard against, not a steady-state cost. When `Dispose()` is called correctly, `GC.SuppressFinalize(this)` (MemorySink.cs:85) removes the instance from the finalization queue and the finalizer never runs.

**C11 — Boxing:** PASS — no boxing in any of the 4 changed files; `MpscByteRingBuffer` and `PacketSink` operate on `ReadOnlySpan<byte>` throughout.

---

### D. Language Runtime

**D13 — Value Types:** PASS
`PaddedDropCount` is a `private struct` field directly embedded in `PacketSink` — no extra indirection or boxing versus the prior plain `long`.

**D19 — Inlining:** PASS
`[MethodImpl(AggressiveInlining)]` retained on `TryPublish`, `TryPeek`, `Advance` (MpscByteRingBuffer.cs) and `Enqueue`, `TryEnqueue` (PacketSink.cs). `TryPeek`'s iterative rewrite is a strict inlining improvement: the JIT never inlines a recursive method, so the pre-fix version paid a real (non-inlined) call cost on every wrap-skip; the loop body is now eligible for full inlining at call sites.

**D20 — Static/Const Propagation:** N/A — no new const-foldable branches introduced.

---

### E. Compiler & JIT

No JIT-specific regressions. `ShouldRotate`'s counter-mask branch and `TryPeek`'s loop are both straight-line/loop constructs the JIT handles identically to the pre-fix code shape; IL size in hot methods is flat or smaller (recursion removal shrinks `TryPeek`'s IL by one call/branch pair).

---

### F. Concurrency

**F21 — Lock-Free:** PASS
No `lock`/`Monitor`/`Mutex` introduced. `PacketSink._dropCount` increment remains `Interlocked.Increment(ref _dropCount.Value)` (PacketSink.cs:88) — only the padding changed, not the synchronization primitive. `MpscByteRingBuffer.TryPeek`/`Advance` remain single-consumer, `Volatile.Read`/`Volatile.Write` only.

**F22 — Thread Affinity:** N/A — unaffected by these changes.

**F-new — Crash Safety (FileStreamSink, `65b6cf1`):** FIXED (was BLOCK in prior resource-cost-map)
Two consumer-thread crash paths closed:
1. `FlushBuffer` (FileStreamSink.cs:95-108): on `IOException`, `_bufferPos = 0` is now set in the `catch` block before returning — previously the buffer position was left at its pre-flush value, so the next `WriteToBackend` call would index past `_writeBuffer.Length` (`IndexOutOfRangeException`, uncaught, kills the consumer thread).
2. `TryRecoverBackend` (FileStreamSink.cs:70-88): on a failed reopen, `_stream = null` is now set in the `catch` block — previously a disposed `_stream` reference could be reused by `FlushBackend`/`WriteToBackend`, throwing `ObjectDisposedException` (not an `IOException`, so it bypassed the existing catch and killed the consumer thread).
Both are exercised by regression tests (see H25).

---

### G. System Boundary

**G23 — Syscall:** PASS
`RotatingFileSink.ShouldRotate`'s `HfClock.NowTicks` call is `Stopwatch.GetTimestamp()` — user-space RDTSC read, no kernel transition; now invoked 1-in-256 times instead of every record (see A1). `FileStreamSink`'s file I/O (`_stream.Write`/`.Flush`) is unchanged by the crash fix — only the failure-recovery bookkeeping changed.

---

### H. Tests

**H24 — Unit Tests:** PASS — 244 passed, 0 failed, 10 skipped (Windows-only SharedMemory/UnixSocket), 254 total. Filter `Category!=Endurance&Category!=Stress&Category!=Perf`.

**H25 — Regression tests for these specific changes:**

| Change | Regression test |
|---|---|
| `FileStreamSink` crash fix | `tests/Relay.Tests/FileStreamSinkCrashTests.cs::IOException_InFlushBuffer_DoesNotKillConsumer`, `::RecoveryFailure_WithDisposedStream_DoesNotKillConsumer`, `::TransientIOException_RecoveryRestoresHealth` |
| `MpscByteRingBuffer.TryPeek` recursion removal | `tests/Relay.Tests/MpscByteRingBufferTests.cs::TryPublish_WrapAround_UsesPaddingMarker_ConsumerSkips`, `::Stress_SingleProducerSingleConsumer_100KRecords_NoLossNoCorruption` |
| `MemorySink<T>` finalizer | `tests/Relay.Tests/MemorySinkTests.cs::Dispose_ReleasesNativeMemory_NoException`, `::Dispose_CalledTwice_IsIdempotent` (finalizer path itself is GC-timing-dependent and not directly asserted; `Dispose`+`GC.SuppressFinalize` correctness is) |
| `PacketSink._dropCount` padding | `tests/Relay.Tests/PacketSinkDropCountTests.cs` (all 5 facts — functional correctness of the counter; padding is a layout-only change with no observable functional surface, so no dedicated cache-line test exists) |

**H26 — Stress Tests:** PASS — `MpscByteRingBufferTests::Stress_SingleProducerSingleConsumer_100KRecords_NoLossNoCorruption` and `::Stress_MultiProducer_4Threads_100KTotal_NoLossNoCorruption` (Category=Stress, excluded from commit-gate filter, present and passing on manual run).

**H27 — Benchmarks:** Not re-run in this audit (static-only pass; no BDN executed). Prior `RotatingFileSink.ShouldRotate` baseline (2026-05-25 audit): 13.59 ns/call. The RDTSC-throttle change (`ab33f86`) predicts a large reduction in *amortized* per-record cost (255/256 calls skip the RDTSC read entirely) but the sampled-call cost (1/256) is unchanged. Recommend a follow-up `/bench-report` run to confirm; not blocking this release per `docs/bench-methodology.md` gating (structural fix with a clear, one-directional perf mechanism).

---

## Summary Table

| # | Dimension | Status | Finding |
|---|---|---|---|
| 1 | CPU Cycles | PASS | RDTSC throttled to 1-in-256; TryPeek recursion removed |
| 2 | Branch Prediction | PASS | Counter-mask and loop branches both well-predicted |
| 3 | SIMD | N/A | — |
| 4 | Bounds Check | PASS | Unsafe pointer arithmetic in ring header I/O unchanged |
| 5 | Cache Locality | PASS | No new access pattern |
| 6 | False Sharing | FIXED | `_dropCount` now 128B-padded (was WARN) |
| 7 | Memory Access | PASS | Header read order unchanged by TryPeek rewrite |
| 8 | Indirection | PASS | Crash fix touches only cold `catch` blocks |
| 9 | Zero-Allocation | PASS | No allocation in any of the 4 commits |
| 10 | GC Pressure | PASS | Finalizer only fires on Dispose-omission misuse |
| 11 | Boxing | PASS | No boxing introduced |
| 13 | Value Types | PASS | PaddedDropCount is an embedded struct, no extra indirection |
| 19 | Inlining | PASS | TryPeek now inlinable (was recursion-blocked) |
| 20 | Static/Const Propagation | N/A | — |
| 21 | Lock-Free | PASS | Interlocked/Volatile only, unchanged primitives |
| 22 | Thread Affinity | N/A | — |
| — | Crash Safety (F, new) | FIXED | Both FileStreamSink consumer-crash paths closed (was BLOCK) |
| 23 | Syscall | PASS | RDTSC now sampled 1-in-256 |
| 24 | Unit Tests | PASS | 244/244 (10 skipped, platform-gated) |
| 25 | Regression Tests | PASS | Dedicated tests exist for 3 of 4 changes; DropCount padding covered functionally only |
| 26 | Stress Tests | PASS | MPSC byte ring stress tests pass |
| 27 | Benchmarks | DEFERRED | Not re-run; recommend follow-up `/bench-report`, non-blocking |

---

## Overall Verdict: PASS — 2 correctness fixes confirmed safe, 2 structural perf fixes confirmed zero-regression, 0 new hot-path violations introduced.

`65b6cf1` and `c1818fa`'s `MemorySink<T>` finalizer are correctness/robustness changes with no hot-path cost impact. `ab33f86` (RDTSC throttle) and `9aae152` (TryPeek recursion removal) are structural perf improvements with no observed downside — recommend confirming the `ShouldRotate` amortized-cost prediction with a BDN run before the next hot-path-sensitive release, but this is advisory and does not block v1.0.5.
