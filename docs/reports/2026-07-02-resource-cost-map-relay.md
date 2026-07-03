---
title: "Resource Cost Map — Relay"
type: report
solution: Relay
status: draft
created: 2026-07-02
---

# Resource Cost Map — Relay

_generated 2026-07-02 · model x64-goldenCove v1 · static+BDN hybrid · BDN baseline commit ddf2f0b (i7-12700, .NET 9.0.14, 12 cores, GC=Server, BDN 0.13.12); static-only delta for 2026-06-11 (`c1818fa`, `9aae152`, `ab33f86`, `65b6cf1`) — not re-benchmarked, see §9_
_Cycles at 3.5 GHz (CLAUDE.md ref). i7-12700 P-core boost ≈4.9 GHz; cycles understate by ≤40% on boosted cores._

## 0. Header
- scope: `src/Relay/**/*.cs` (tier: ultra-low-latency; all files hot-path by declaration)
- mode: library — no `Main`; all public API is entry
- entry points: 14 (calls/s: `declared-hot` — derived from tier declaration; no hints file)
- nodes analyzed: ~65
- blind subgraphs: see §8
- budget: library mode — no weight/s; relative ranking by cycles/call
- supersedes: `docs/reports/2026-06-10-resource-cost-map-relay.md` (removed; scope covered 5 files touched by `c1818fa`/`9aae152`/`ab33f86`/`65b6cf1` on 2026-06-11, one day after that report's generation)

## 1. Per-Entry Cost Table
(library mode — calls/s = declared-hot; tier from project declaration)

| # | Entry | cycles/call | bytes/call | Drivers | file:line |
|---|---|---|---|---|---|
| 1 | `DispatchSink<T>.Enqueue` (healthy, depth=1) | 1c† | 0 | JIT inlines all for sealed types; IsHealthy (1c) + Accept (3c) collapses to ~1c | DispatchSink.cs:49 |
| 2 | `SpscRingBuffer<T>.TryPublish` (HeadCache hit) | 3c | 0 | L1 load cachedHead (4c) + store slot (1c) + Volatile.Write tail (1c); mfence amortized | SpscRingBuffer.cs:48 |
| 3 | `MpscRingBuffer<T>.TryPublish` (no contention) | 22c | 0 | Volatile.Read claimedTail (1c) + CAS (25c uncontended) + Volatile.Write Published (1c) | MpscRingBuffer.cs:93 |
| 4 | `MpscRingBuffer<T>.TryConsume` | 12c | 0 | Volatile.Read Published (1c) + InitBlockUnaligned T=64B (4c) + 2×Volatile.Write (2c) | MpscRingBuffer.cs:125 |
| 5 | `FilterSink<T>.Accept` (pass) | 4c | 0 | Predicate<T> delegate invoke (8c model; BDN total 3.8c — devirt likely) | FilterSink.cs:Accept |
| 6 | `MultiSink<T>.Accept` (N=2 children) | 11c | 0 | 2× virtual Enqueue (7c each); BDN 3.18 ns | MultiSink.cs:Accept |
| 7 | `Multi2Sink<T,TC1,TC2>.Accept` (N=2, sealed) | 12c | 0 | 2× devirt direct Enqueue (3c each); BDN 3.31 ns — **0.45c slower** than MultiSink in baseline; JIT GDV already devirtualizes sealed types at call site | MultiSink.cs:Multi2Sink.Accept |
| 8 | `PacketSink.Enqueue` (drop, Next=null, unhealthy) | 13c | 0 | Interlocked.Increment on `_dropCount.Value`, now 128B-padded (offset 64) — false-sharing removed, cost unchanged (25c uncontended CAS-class op) | PacketSink.cs:88 |
| 9 | `SpscQueueSink.ConsumeLoop` batch drain (per item) | 3c/item | 0 | TryConsumeBatch: 1 Volatile.Read(tail) + 1 Volatile.Write(head) per batch of 256 | SpscQueueSink.cs:196 |
| 10 | `RotatingFileSink.ShouldRotate` (amortized) | ~1.2c | 0 | RDTSC now throttled to 1-in-256 records via `_shouldRotateCounter & 0xFF`; fast path = size compare only (1c); sampled path (1/256) = 47c RDTSC | RotatingFileSink.cs:123-129 |
| 11 | `RotatingFileSink.WriteToBackend` (no rotation, amortized) | ~74c | 0 | ShouldRotate amortized (1.2c) + payload copy 64B (4c) + bookkeeping; down from 120c worst-every-call estimate | RotatingFileSink.cs:73 |
| 12 | `MemorySink<T>.Accept` (ring not full) | 8c | 0 | 2× plain long read (4c each) + store + increment (plain, no Volatile); unchanged — finalizer only touches cold `~MemorySink()` path | MemorySink.cs:49 |
| 13 | `HfClock.NowTicks` | 20c† | 0 | Stopwatch.GetTimestamp() = RDTSC+LFENCE; BDN evidence ~47c effective on i7-12700 | HfClock.cs:NowTicks |
| 14 | `MpscByteRingBuffer.TryPublish` (no wrap) | 30c | 0 | CAS (25c) + payload copy + Volatile.Write header | MpscByteRingBuffer.cs:112 |
| 15 | `MpscByteRingBuffer.TryPublish` (wrap) | 40c | 0 | +padding Volatile.Write at original pos; wrap adds ~10c | MpscByteRingBuffer.cs:112 |
| 16 | `MpscByteRingBuffer.TryPeek` (wrap-skip case) | 25c | 0 | Now an iterative `while(true)` loop (recursion removed) — same instruction cost per skip, but JIT can inline the whole method (no recursive-call barrier) | MpscByteRingBuffer.cs:174 |

† JIT-dependent; worst-case higher.

## 2. Top 20 Offenders (by cycles/call)

| # | cycles/call | BDN ns | symbol | file:line | note |
|---|---|---|---|---|---|
| 1 | ~74c | — (not re-benchmarked) | `RotatingFileSink.WriteToBackend` (amortized) | RotatingFileSink.cs:73 | down from 120c/34.18 ns — RDTSC throttle (§9) |
| 2 | 40c | — | `MpscByteRingBuffer.TryPublish` (wrap) | MpscByteRingBuffer.cs:112 | 2× Volatile.Write; TryPeek recursion removed (§9), TryPublish cost unchanged |
| 3 | 30c | — | `MpscByteRingBuffer.TryPublish` (no wrap) | MpscByteRingBuffer.cs:112 | CAS dominant |
| 4 | 25c | — | `MpscByteRingBuffer.TryPeek` (wrap-skip) | MpscByteRingBuffer.cs:174 | now iterative — inlinable, no recursive-call barrier |
| 5 | 22c | 6.35 | `MpscRingBuffer<T>.TryPublish` | MpscRingBuffer.cs:93 | CAS 25c uncontended; 7× SPSC |
| 6 | 21c | 6.09 | `PacketSink.Enqueue` byte depth=2 | PacketSink.cs:73 | MPSC byte ring overhead |
| 7 | 13c | 3.77 | `PacketSink.Enqueue` (drop path) | PacketSink.cs:88 | Interlocked, now on padded field — false-sharing resolved (§9) |
| 8 | 12c | — | `MpscRingBuffer<T>.TryConsume` | MpscRingBuffer.cs:125 | 2× Volatile.Write per slot |
| 9 | 12c | 3.31 | `Multi2Sink<T,TC1,TC2>.Accept` (N=2) | MultiSink.cs:Multi2Sink.Accept | CRTP 0.45c slower than MultiSink in baseline; JIT GDV likely explains parity |
| 10 | 11c | 3.18 | `MultiSink<T>.Accept` (N=2) | MultiSink.cs:Accept | virtual Enqueue per child |
| 11 | 8c | — | `MemorySink<T>.Accept` (ring not full) | MemorySink.cs:49 | unchanged; finalizer is cold-path only |
| 12 | 4c | 1.08 | `FilterSink<T>.Accept` (pass) | FilterSink.cs:Accept | delegate invoke; BDN beats model |
| 13 | 3c | 0.95 | `SpscRingBuffer<T>.TryPublish` | SpscRingBuffer.cs:48 | HeadCache hit fast path |
| 14 | 3c | 0.86 | `SpscRingBuffer<T>.RoundTrip` | SpscRingBuffer.cs:48 | publish+consume same thread |
| 15 | ~1.2c | — | `RotatingFileSink.ShouldRotate` (amortized) | RotatingFileSink.cs:123 | down from 47c every-call — throttled to 1-in-256 (§9) |
| 16 | 1c | 0.20 | `DispatchSink<T>.Enqueue` (healthy, depth=1) | DispatchSink.cs:49 | JIT inlines all for sealed types |

## 3. Hot Tree (by thread)

```
Producer path (tick-rate call)
DispatchSink<T>.Enqueue                    DispatchSink.cs:49
├─ IsHealthy (volatile bool read)          SpscQueueSink.cs:67      1c   declared-hot  0  HOT
├─ SpscQueueSink<T>.Accept                 SpscQueueSink.cs:130     0c   declared-hot  0  HOT (inlined)
│  └─ SpscRingBuffer<T>.TryPublish         SpscRingBuffer.cs:48     3c   declared-hot  0  HOT
│     ├─ _cachedHead check (L1 hit)        SpscRingBuffer.cs        4c   declared-hot  0  HOT
│     ├─ _basePtr[tail & mask] = item      SpscRingBuffer.cs        1c   declared-hot  0  HOT
│     └─ Volatile.Write(_tail.Value)       SpscRingBuffer.cs        1c   declared-hot  0  HOT
└─ Next?.Enqueue (fallback only)           DispatchSink.cs:53       7c   fallback      0  HOT

PacketSink.Enqueue (drop path)             PacketSink.cs:73
└─ Interlocked.Increment(_dropCount.Value) PacketSink.cs:88        25c   declared-hot  0  HOT (padded — no false sharing)

MpscByteRingBuffer.TryPeek (consumer)      MpscByteRingBuffer.cs:174
├─ Volatile.Read header @ head             MpscByteRingBuffer.cs:180 1c  declared-hot  0  HOT
└─ [padding] loop iteration (was recursive call) MpscByteRingBuffer.cs:190-196 24c declared-hot 0 HOT (now inlinable)

Consumer thread · SpscQueueSink.ConsumeLoop SpscQueueSink.cs:183
├─ TryConsumeBatch (batch=256)             SpscQueueSink.cs:196     3c/item  N/batch  0  HOT
│  └─ WriteToBackend(in _consumeBuf[i])    SpscQueueSink.cs:202     varies   N/batch  0  HOT
├─ Idle spin: Thread.SpinWait(20)          SpscQueueSink.cs:209     inf      idle     0  IDLE
├─ Idle yield: Thread.Yield()              SpscQueueSink.cs:216     inf      idle     0  IDLE
├─ Idle sleep: Thread.Sleep(1ms)           SpscQueueSink.cs:222     inf      idle     0  IDLE
├─ FlushBackend (on deadline/signal)       SpscQueueSink.cs:241     5e3c     1/flush  0  WARM
└─ TryRecoverBackend (on deadline)         SpscQueueSink.cs:244     varies   1/flush  0  WARM

Consumer thread · RotatingFileSink.WriteToBackend  RotatingFileSink.cs:73
├─ ShouldRotate(payload.Length)            RotatingFileSink.cs:77   1.2c amortized  declared-hot  0  HOT (was ⚠, throttled)
│  ├─ size compare (plain long)            RotatingFileSink.cs:125   1c  declared-hot  0  HOT
│  └─ HfClock.NowTicks (RDTSC, 1-in-256)   RotatingFileSink.cs:127  47c  1/256        0  WARM (sampled, was every-call)
├─ payload.CopyTo(_writeBuffer)            RotatingFileSink.cs:102   4c  declared-hot  0  HOT
└─ _currentFileBytes += payload.Length     RotatingFileSink.cs:104   1c  declared-hot  0  HOT
```

## 4. Allocation Map (top entries by bytes/s)

| # | bytes/call | symbol | file:line | kind |
|---|---|---|---|---|
| 1 | 0 | All hot-path entries | DispatchSink.cs:49 | All zero-alloc; GC.AllocateArray(pinned:true) in ctors only |

COLD paths only (not hot):
- `RotatingFileSink.TryOpenStream`: `string.Format(...)` — string alloc on rotation
- `RotatingFileSink.Cleanup`: LINQ `.ToArray()` — array alloc on file pruning
- `MemorySink<T>.~MemorySink()`: finalizer, zero-alloc (`NativeMemory.Free`); queued to finalizer thread only if `Dispose` was not called — GC pressure only in the misuse case it now guards against

## 5. Anti-Pattern Offenders (declared-hot path violations)

| Severity | Rule | symbol | file:line | evidence |
|---|---|---|---|---|
| warn | RDTSC per declared-hot record | `RotatingFileSink.ShouldRotate` | RotatingFileSink.cs:126 | **downgraded from prior report** — was every-call (13.38 ns), now throttled via `(++_shouldRotateCounter & 0xFF) == 0`, sampled 1-in-256; residual warn because the sampled call still costs 47c and day-boundary detection has ~256-record granularity |
| info | doc/measurement mismatch: CRTP | `Multi2Sink<T,TC1,TC2>` | MultiSink.cs:Multi2Sink | CLAUDE.md documents ~6c saving vs MultiSink; BDN: Multi2=3.31 ns vs Multi=3.18 ns at N=2 — Multi2 is 0.45c **slower** in baseline; unchanged since prior report, CLAUDE.md already carries the caveat |
| info | DateTime.UtcNow injected default | `RotatingFileSink._utcNow` | RotatingFileSink.cs:68 | default lambda `() => DateTime.UtcNow` violates CLAUDE.md; acceptable — sanctioned injection-seam default per CLAUDE.md, rotation path is COLD |

**Resolved since 2026-06-10 report** (see §9 for detail): false sharing on `PacketSink._dropCount` (padded), `FileStreamSink` double-crash bug (OOB write + disposed-stream reuse), `MpscByteRingBuffer.TryPeek` recursion, `MemorySink<T>` missing finalizer, `MemorySink<T>` remarks/code volatile-claim mismatch.

## 6. Cache-Line Report

| symbol | sizeof / issue | file:line |
|---|---|---|
| `PacketSink._dropCount` | now `PaddedDropCount` — `[StructLayout(Explicit, Size=128)]` with `Value` at offset 64; isolated from `Next`/`PropagateAfterAccept` on the same object | PacketSink.cs:42-48 |
| `MemorySink<T>._head/_tail` | plain `long` without PaddedLong; safe under single-producer-only invariant; remarks now correctly describe "plain reads/writes ... under x86/x64 TSO with a single producer" (previously claimed volatile) | MemorySink.cs:14-21 |

## 7. Syscalls & Kernel Boundaries

| symbol | file:line | kind | cycles |
|---|---|---|---|
| `FileStreamSink.FlushBuffer` | FileStreamSink.cs:95 | file I/O write | 5e3c |
| `TcpSink.FlushBuffer` | TcpSink.cs:131 | socket send | 2e3c |
| `MmfSink.FlushBackend` | MmfSink.cs:83 | FlushViewOfFile | 5e3c |
| `RotatingFileSink.FlushToStream` | RotatingFileSink.cs:144 | file I/O write + flush | 5e3c |
| `RotatingFileSink.Cleanup` | RotatingFileSink.cs:200 | Directory.GetFiles + File.Delete (COLD rotation) | varies |
| `SpscQueueSink.Start` | SpscQueueSink.cs:104 | Thread.Start (COLD setup) | — |
| `RelayMemory.PreFaultAndLock` | SpscQueueSink.cs:108 | VirtualLock P/Invoke (COLD setup) | — |

## 8. Blind Subgraphs

| symbol | file:line | reason |
|---|---|---|
| `FilterSink<T>._predicate` body | FilterSink.cs:Accept | caller-supplied `Predicate<T>` delegate; body unknown; cost ranges from 1c (field compare) to 500c+ (reflection) |
| `TcpSink.TryConnect` OS handshake | TcpSink.cs:184 | TCP SYN/ACK at kernel level; cost = OS scheduler + network RTT; not analyzable from source |
| `SpscByteRingBuffer` (full) | SpscByteRingBuffer.cs:TryPublish | not in scope of this read-pass; padding skip uses non-recursive `Unsafe.WriteUnaligned`, unaffected by the MPSC-side recursion fix |
| `RotatingFileSink._utcNow` body | RotatingFileSink.cs:68 | injected delegate; default `DateTime.UtcNow`; not RDTSC-based; acceptable only on COLD rotation path |

## 9. Delta vs `2026-06-10-resource-cost-map-relay.md`

| change | symbol | file:line | weight before → after | verdict |
|---|---|---|---|---|
| fixed (correctness) | `FileStreamSink.FlushBuffer` — no `_bufferPos` reset on IOException | FileStreamSink.cs:105 | block (crash risk) → resolved | ✅ |
| fixed (correctness) | `FileStreamSink.TryRecoverBackend` — disposed-stream reuse | FileStreamSink.cs:84 | block (crash risk) → resolved | ✅ |
| improved (perf) | `RotatingFileSink.ShouldRotate` | RotatingFileSink.cs:126 | 47c every-call → ~1.2c amortized (1-in-256 sampled) | ✅ |
| improved (perf, derived) | `RotatingFileSink.WriteToBackend` | RotatingFileSink.cs:73 | 120c → ~74c amortized | ✅ |
| fixed (cache) | `PacketSink._dropCount` false sharing | PacketSink.cs:42 | warn (unpadded) → resolved (128B explicit layout, offset 64) | ✅ |
| fixed (structural) | `MpscByteRingBuffer.TryPeek` recursion | MpscByteRingBuffer.cs:176 | warn (non-inlineable recursive) → resolved (iterative `while(true)`) | ✅ |
| fixed (correctness, cold) | `MemorySink<T>` missing finalizer | MemorySink.cs:74 | warn (native leak if Dispose omitted) → resolved (`~MemorySink()` + `GC.SuppressFinalize`) | ✅ |
| fixed (doc) | `MemorySink<T>` remarks/code volatile mismatch | MemorySink.cs:16 | warn (doc/code mismatch) → resolved (remarks now say "plain reads/writes") | ✅ |
| unchanged | `Multi2Sink<T,TC1,TC2>` CRTP no measured gain | MultiSink.cs:Multi2Sink | info (unchanged) | — |
| unchanged | `RotatingFileSink._utcNow` injected `DateTime.UtcNow` default | RotatingFileSink.cs:68 | info (unchanged, sanctioned) | — |

Net: 2 block-severity correctness offenders resolved, 3 warn-severity structural/perf offenders resolved, 1 warn downgraded (still present but ~40× cheaper amortized), 0 new offenders introduced by the 2026-06-11 changes.
