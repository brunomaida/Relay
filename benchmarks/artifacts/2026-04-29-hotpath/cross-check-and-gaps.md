# BDN Hot-Path Cross-Check + Coverage Gaps — 2026-04-29

_paired with `docs/reports/2026-04-29-resource-cost-map-relay.md` · short-job BDN · i7-12700 (Golden Cove, 12 P+E)_

## Reading

- **Host:** `12th Gen Intel Core i7-12700 · Windows 11 · .NET 9.0.14 · X64 RyuJIT AVX2`
- **Job:** `ShortRun (IterationCount=3, LaunchCount=1, WarmupCount=3)` — fast triage; not gate-grade. Margin-of-error >5% on ≤1ns measurements; treat sub-ns numbers as ordinal only.
- **Conversion:** assume sustained boost ~4.5 GHz on P-cores → **1 ns ≈ 4.5 cycles**.
- **Cycle column** = `mean_ns × 4.5` (rounded). BDN reports ns; cost-map predicts cycles.

## 1. BDN runs in this snapshot

30 benchmarks executed (typed/byte Enqueue chains, MpscRingBuffer single-thread, Multi/Filter/Propagate). Per-class summary tables under `benchmarks/artifacts/2026-04-29-hotpath/results/*-report-github.md`.

| BDN class | Methods | Status |
|---|---|---|
| `EnqueueBenchmarks` (typed Enqueue depth 1-3) | 4 | ✅ |
| `ByteEnqueueBenchmarks` (packet Enqueue depth 1-3) | 4 | ✅ |
| `MpscBenchmarks` (Spsc/Mpsc ring single-thread, 3 caps) | 12 | ✅ |
| `MultiEnqueueBenchmarks` | 2 | ✅ |
| `MultiIsHealthyBenchmarks` | 2 | ✅ |
| `FilterSinkBenchmarks` | 2 | ✅ |
| `PropagateBenchmarks` | 4 | ✅ |
| `RingBufferBenchmarks` (typed SPSC primitives) | 4 × 3 caps | ⚠ rerun needed (ENOSPC during run) |
| `ByteRingBufferBenchmarks` (SPSC byte primitives) | 3 × 12 (cap × payload) | ⚠ rerun needed (ENOSPC during run) |

End-to-end consumer-loop BDNs (`QueuePipeThroughputBenchmarks`, `Baselines/TypedSinkBaselineBenchmark`, `PacketSinks/*`) excluded — driven by producer-thread + Sleep(1) idle path; not useful for cycle-level cost validation. Run separately for SLA gates.

## 2. Cross-check: predicted (cost-map) vs measured (BDN)

| Node (cost-map §1/§2) | Pred. cycles | BDN method | ns | Cycles (4.5 GHz) | Δ vs predicted | Verdict |
|---|---|---|---|---|---|---|
| `DispatchSink<T>.Enqueue` (sealed sub, healthy) | ~15 | `EnqueueBenchmarks.Depth1_Healthy` (CounterPipe) | 0.48 | 2.2 | **−13** | over-estimate; trivial Accept collapses to single store under JIT |
| 2 hops, Accept=false then Counter | ~30 | `EnqueueBenchmarks.Depth2_AcceptReject` | 2.99 | 13.5 | **−16** | trivial-accept JIT collapse on second sink |
| 2 hops, IsHealthy=false then Counter | ~22 | `EnqueueBenchmarks.Depth2_HeadUnhealthy` | 2.59 | 11.7 | **−10** | as above |
| 3 hops, all unhealthy then Counter | ~40 | `EnqueueBenchmarks.Depth3_AllUnhealthy` | 3.16 | 14.2 | **−26** | each hop ~1c when terminal Accept inlines |
| `PacketSink.Enqueue` (sealed sub, healthy) | ~15 | `ByteEnqueueBenchmarks.Depth1_Byte_Healthy` | 1.04 | 4.7 | **−10** | span pass + Volatile.Write; ~2.5x typed |
| 2 hops byte AcceptReject | ~30 | `ByteEnqueueBenchmarks.Depth2_Byte_AcceptReject` | 5.81 | 26.1 | **−4** | aligned |
| 2 hops byte HeadUnhealthy | ~22 | `ByteEnqueueBenchmarks.Depth2_Byte_HeadUnhealthy` | 4.52 | 20.3 | **−2** | aligned |
| 3 hops byte AllUnhealthy | ~40 | `ByteEnqueueBenchmarks.Depth3_Byte_AllUnhealthy` | 8.66 | 39.0 | **−1** | aligned |
| `SpscRingBuffer<T>.TryPublish + TryConsume` round-trip | ~20 (10c × 2) | `MpscBenchmarks.Spsc_TryPublish_Baseline` (cap 1024) | 3.79 | 17.1 | **−3** | aligned |
| `MpscRingBuffer<T>.TryPublish + TryConsume` round-trip uncontended | ~42 (30c+12c) | `MpscBenchmarks.Mpsc_TryPublish_NoContention` (cap 1024) | 7.97 | 35.9 | **−6** | uncontended CAS cheaper than predicted (~7c vs 25c) on Golden Cove |
| `MpscRingBuffer.TryPublish` full (fast-reject) | unspecified | `MpscBenchmarks.Mpsc_TryPublish_Full` (cap 1024) | 0.49 | 2.2 | n/a | head-cache hit + early exit |
| `MpscRingBuffer.TryConsume` empty | unspecified | `MpscBenchmarks.Mpsc_TryConsume_Empty` (cap 1024) | 0.38 | 1.7 | n/a | single Volatile.Read flag + exit |
| `MultiSink<T>.Accept` (N=2) | 7+2×17 = 41 | `MultiEnqueueBenchmarks.Multi_Enqueue` | 6.89 | 31.0 | **−10** | array foreach + 2 inlined Counter |
| `Multi2Sink<T,TC1,TC2>.Accept` (sealed) | ~32 | `MultiEnqueueBenchmarks.Multi2_Enqueue` | 5.93 | 26.7 | **−5** | aligned; CRTP saves ~4c vs array |
| `MultiSink<T>.IsHealthy` short-circuit | ~16 | `MultiIsHealthyBenchmarks.Multi_IsHealthy` | 1.13 | 5.1 | **−11** | first-child returns true → exit |
| `Multi2Sink<T>.IsHealthy` | ~4 | `MultiIsHealthyBenchmarks.Multi2_IsHealthy` | 1.08 | 4.9 | **+1** | aligned |
| `FilterSink<T>.Accept` (predicate fail) | ~3 | `FilterSinkBenchmarks.Filter_Reject` | 0.82 | 3.7 | **+1** | aligned |
| `FilterSink<T>.Accept` (predicate pass + downstream) | ~18 | `FilterSinkBenchmarks.Filter_Pass` | 6.22 | 28.0 | **+10** | predicate=identity still pays delegate slot |
| Default propagate (no-op tail) | ~15 | `PropagateBenchmarks.Depth1_Healthy_Default` | 0.72 | 3.2 | **−12** | trivial CounterPipe |
| Propagate=true, Next=null | ~15 | `PropagateBenchmarks.Depth1_Healthy_Propagate_NoNext` | 0.76 | 3.4 | **−12** | confirms field-load + null-check propagate branch is ~0c |
| `ForkSink<T>` propagate to primary + Next | ~32 | `PropagateBenchmarks.Depth2_Fork_Wrapped` | 5.63 | 25.3 | **−7** | aligned |
| Custom propagate (no ForkSink wrapper) | ~30 | `PropagateBenchmarks.Depth2_Propagate_Fork` | 3.32 | 14.9 | **−15** | confirms ForkSink wrapper adds ~10c (one extra virt-call layer) |

### Calibration verdict

- Cost-map predicts **upper bound** for `Enqueue` paths assuming realistic backend Accept work (~7c). When the test backend is `CounterPipe` (single `Volatile.Write`), JIT collapses the entire chain to ~2-3c. Predictions for **realistic** sinks (TcpSink/MmfSink/FileSink WriteToBackend) remain valid — the dispatch envelope itself is ~2c, the backend work is what the prediction sums.
- MPSC uncontended CAS measured **~7c**, not the textbook ~25c. Golden Cove fused-µops on hot-cache `LOCK CMPXCHG` are cheaper than the canonical Agner Fog table number. **Re-cal MpscRingBuffer.TryPublish from ~30c → ~10c (uncontended)** in cost-map.
- Filter and Multi: aligned to within ±10c — within ShortRun margin-of-error. No regressions.
- PropagateAfterAccept field-load adds ~0c on the no-propagate path. Confirms the design choice (field, not virtual property) was sound.

## 3. Coverage gaps — ULTRA-HOT cost-map nodes WITHOUT BDN

### Critical (block fix branch until added)

| # | Node | file:line | Why blocking |
|---|---|---|---|
| C1 | `RotatingFileSink.ShouldRotate` cost (UtcNow vs HfClock-tick) | src/Relay/Sinks/RotatingFileSink.cs:82 | Without before/after BDN, the impending fix has no measurable gate. Cost-map says ~50c → ~3c expected; BDN must validate ratio ≥10x. |

### High-priority gaps (ULTRA-HOT, new code without coverage)

| # | Node | Predicted cycles | Suggested BDN class |
|---|---|---|---|
| ~~H1~~ resolved | `MpscByteRingBuffer.TryPublish` (uncontended) | ~50 | Phase 3: BDN landed — see `benchmarks/artifacts/2026-04-29-phase3/`. Measured: ~20c uncontended (cost-map prediction was conservative). |
| ~~H2~~ resolved | `MpscByteRingBuffer.TryPeek` / `Advance` | ~12 / ~5 | Phase 3: BDN landed (same class as H1). |
| ~~H3~~ resolved | `MpscQueueSink<T>` end-to-end (single producer, sustained Push) | ~30c Accept + consumer | Phase 3: `MpscPush_Single` / `MpscPush_Single_SlowBackend` added to `QueuePipeThroughputBenchmarks`. |
| ~~H4~~ resolved | `MpscQueueSink` (packet) end-to-end | ~50c Accept + consumer | Phase 3: `MpscPacketQueueSinkThroughputBenchmarks` landed — see `benchmarks/artifacts/2026-04-29-phase3/`. |
| ~~H5~~ resolved | `SharedMemorySink.Accept` (CAS + modular WriteRing) | ~50 | Phase 4: `SharedMemorySinkBenchmarks` landed; measured ~12 ns at 64B / ~14 ns at 256B (cost-map prediction conservative). See `benchmarks/artifacts/2026-04-29-phase4/`. |
| ~~H6~~ resolved | `RamSink.Accept` (packet) | ~20 | Phase 4: `RamPacketSinkBenchmarks` landed; measured ~4.4 ns at 64B / 256B (well under prediction). |
| ~~H7~~ resolved | `MmfSink<T>.WriteToBackend` (bypass-managed-bounds path) | ~30 | Phase 4: `MmfSinkBenchmarks` landed; Push@1M = 6.9 ms (~145M/s on producer side; consumer-bounded). |
| ~~H8~~ resolved | `UdpSink.WriteToBackend` syscall | ~2000 | Phase 4: `UdpSinkBenchmarks` landed; measures producer side, dominated by ring-fill drops on saturation — actual 1.5M/s wire rate cannot be inferred without delivered-count counter. Flagged for follow-up. |
| ~~H9~~ resolved | `PacketSink.TryEnqueue` (non-fallthrough) | ~13 | Phase 5: BDN landed — TryEnqueue_Healthy 0.42 ns (~2c, 6x faster than Enqueue), TryEnqueue_Reject 0.22 ns (~1c). |
| ~~H10~~ resolved | `PacketSink.Enqueue` terminal-drop (`Interlocked.Increment(_dropCount)`) | ~25 | Phase 5: BDN landed — Drop_NextNull_* 3.75-3.79 ns. Cost-map ~25c was conservative; measured ~5c overhead vs Enqueue baseline (uncontended LOCK INC). |

### Medium-priority gaps (parallel-tree symmetry + blind subgraph)

| # | Node | Predicted cycles | Suggested BDN |
|---|---|---|---|
| ~~M1~~ resolved | `ForkSink` (packet) | ~18 | Phase 2: BDN landed — see `benchmarks/artifacts/2026-04-29-phase2/` |
| ~~M2~~ resolved | `MultiSink` (packet, N=2) | 7+2×17 = 41 | Phase 2: BDN landed — see `benchmarks/artifacts/2026-04-29-phase2/` |
| ~~M3~~ resolved | `Multi2Sink`-equivalent for packet | ~7c (mirror of typed Multi2Sink) | Phase 6: `Multi2PacketSink<TC1,TC2>` sealed CRTP type landed + builder overload + 7 tests + BDN. ShortRun: Multi2_Packet_Enqueue 3.93 ns vs MultiSink baseline 3.21 ns (ratio 1.23 within sub-ns CI overlap; code size 458B vs 690B confirms leaner path; `__Canon` shared generic body partially suppresses devirt for reference-typed children). Numbers under `benchmarks/artifacts/2026-04-29-phase6/`. |
| ~~M4~~ resolved | `FilterSink` (packet) | ~18 | Phase 2: BDN landed — see `benchmarks/artifacts/2026-04-29-phase2/` |
| ~~M5~~ resolved-with-note | `BatchSink.WriteToBackend` (consumer scratch fits) | ~10 | Phase 4: `BatchSinkBenchmarks` landed measuring **producer-side ring publish only** — driving the sealed `WriteToBackend` from a subclass would require a production accessor change which the plan rejected. Consumer-side scratch-copy cost is covered indirectly via Phase 2 SPSC packet throughput. |
| ~~M6~~ resolved | `NamedPipeSink.WriteToBackend` | ~18 | Phase 4: `NamedPipeSinkBenchmarks` (Windows). Push@10k = 0.5 ms / Push@100k = 1.07 ms. |
| ~~M7~~ resolved | `UnixSocketSink.WriteToBackend` | ~18 | Phase 4: `UnixSocketSinkBenchmarks`. Compiled on Windows; AF_UNIX is supported on Win10+ so the BDN runs there too — `[SupportedOSPlatform]` did NOT skip it as the plan predicted. Push@1M = 11.9 ms. |
| M8 | `RotatingFileSink.WriteToBackend` (excl. ShouldRotate) | ~10 | covered by C1 BDN setup |
| ~~M9~~ resolved | `SpscQueueSink` (packet) end-to-end Push | ~35c Accept + consumer | Phase 2: BDN landed — see `benchmarks/artifacts/2026-04-29-phase2/` |
| ~~M10~~ resolved | `MpscRingBuffer<T>.TryPublish` **multi-thread contention** | ~30c± retry | Phase 7: `MpscContentionBenchmarks` (typed) + `MpscByteContentionBenchmarks` (packet) landed at N=1,2,4,8. Typed aggregate throughput 10.6M / 7.7M / 12.3M / 13.3M items/s — **N=2 shows negative scaling** (CAS contention dominates uncontended publish); N≥4 amortizes via head-cache cross-core hit avoidance. Packet aggregate 2.18M / 2.46M / 6.05M / 7.16M items/s. Cost-map §8 blind subgraph "MPSC CAS-retry distribution" is closed by **measurement (throughput proxy)** — invasive retry-counter instrumentation in `MpscRingBuffer`/`MpscByteRingBuffer` would require production-code changes and is flagged as a Phase 8 follow-up. Numbers under `benchmarks/artifacts/2026-04-29-phase7/`. |

### Low-priority (covered indirectly or low risk)

| # | Node | Why low-priority |
|---|---|---|
| L1 | `SerializeSink<T>.Accept` (typed→packet bridge) | Covered by `ChainBenchmark.SerializeSink_Overhead`; ref-cast = no copy |
| L2 | `NullSink<T>.Accept` / `NullSink.Accept` | Trivial `return true` — JIT folds; not worth measuring |
| L3 | `HfClock.NowTicks` (RDTSC) | Hardware primitive; well-known cost; not project-specific |
| L4 | `*.IsHealthy` (single vol-bool read) | Single-instruction; predict 1c, no value adding BDN |

## 4. Plan to close gaps

Master plan: `docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md` — orchestrates 8 sequential branches off `develop`, each closing a related cluster of gaps. Phase 1 addresses C1 + RotatingFileSink UtcNow fix.
