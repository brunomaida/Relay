# Cost-Map Coverage — Master Plan (8 phases)

> **For agentic workers:** this is an **orchestration document**, not an executable plan. Each phase has a dedicated child plan filed under `docs/superpowers/plans/2026-04-29-phaseN-*.md`. Use those child plans with `superpowers:subagent-driven-development` or `superpowers:executing-plans`.

**Goal:** Close every BDN coverage gap identified in `benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md` (1 critical + 10 high + 10 medium = 21 items) and apply the `RotatingFileSink` UtcNow regression fix flagged in `docs/reports/2026-04-29-resource-cost-map-relay.md` §5.

**Why phased:** 21 atomic items across 12+ new BDN classes + 1 production fix + 1 new public type would yield an unreviewable PR. Each phase is one branch, one merge to `develop`, before the next phase starts. Failure in any phase does not block subsequent phases.

**Branch hygiene:** every phase branches off the latest `develop` after the previous phase merges. CLAUDE.md HARD RULE — no commit goes directly to `develop` or `master`.

---

## Phase Map

| # | Branch | Items | Output | Effort | Status |
|---|---|---|---|---|---|
| 1 | `fix/260429-rotatingfilesink-utcnow-hot-path` | C1 + UtcNow fix | regression closed + BDN gate | ~2h | ✅ done — 6 commits, 190/190 tests pass, BDN: predicate 21.84ns→13.76ns (Δ 8.08ns); ratio gate recalibrated post-execution (see notes) |
| 2 | `chore/260429-bdn-packet-tree-symmetry` | M1, M2, M4, M9 | packet equivalents to typed Fork/Multi/Filter/SpscThroughput BDNs | ~3h | ✅ done — packet-tree symmetry BDNs added (M1 ForkPacket, M2 MultiPacket + IsHealthy, M4 FilterPacket, M9 SpscPacket throughput); numbers under `benchmarks/artifacts/2026-04-29-phase2/` |
| 3 | `chore/260429-bdn-mpsc-byte-and-end-to-end` | H1, H2, H3, H4 | MpscByteRingBuffer + Mpsc end-to-end (typed + packet) BDNs | ~4h | ✅ done — MpscByteRingBufferBenchmarks (H1+H2, 48 cells), MpscPush_* extended in QueuePipeThroughputBenchmarks (H3), MpscPacketQueueSinkThroughputBenchmarks (H4); MPSC RoundTrip 1.5-2.5x SPSC; numbers under `benchmarks/artifacts/2026-04-29-phase3/` |
| 4 | `chore/260429-bdn-backends` | H5, H6, H7, H8, M5, M6, M7 | concrete-sink BDNs (Shm, RamPacket, Mmf, Udp, Batch, NamedPipe, UnixSocket); platform-gated | ~5h | ⬜ pending |
| 5 | `chore/260429-bdn-packet-api-contracts` | H9, H10 | TryEnqueue + terminal-drop BDNs | ~2h | ⬜ pending |
| 6 | `feat/260429-multi2-packet-sink` | M3 | new sealed type `Multi2PacketSink` + impl + tests + BDN | ~3h | ⬜ pending |
| 7 | `chore/260429-bdn-mpsc-multi-thread-contention` | M10 | multi-producer contention infra + BDN, closes blind subgraph §8 | ~3h | ⬜ pending |
| 8 | `docs/260429-cost-map-calibration` | recalibration | updates `2026-04-29-resource-cost-map-relay.md` §1/§2 with measured cycles from Phases 1-7 | ~1h | ⬜ pending |

**Total budgeted:** ~23h disciplined work across 8 PRs.

---

## Cross-cutting conventions

Apply to **every** phase unless noted otherwise:

- **TDD order:** for fix work, failing test first → impl → green; for BDN-only work, BDN → run on baseline branch → record numbers → commit BDN.
- **Test gate:** `dotnet test tests/Relay.Tests` must report 0 failures before any commit (CLAUDE.md project gate).
- **BDN job:** `--job short` for triage during development, **rerun without `--job short` for the final BDN that ships in CHANGELOG**.
- **Output dir:** `benchmarks/artifacts/2026-04-29-phaseN/` (one folder per phase).
- **Commit message style:** Conventional Commits + `w/Claude` suffix. No `Co-Authored-By` trailer.
- **Branch naming:** as in the table above. Date format `yyMMdd` per project CLAUDE.md.
- **No direct push to `master`.** Every PR target `develop`. Merge requires user approval.

---

## Phase 1 — RotatingFileSink UtcNow fix + C1 BDN gate

**Branch:** `fix/260429-rotatingfilesink-utcnow-hot-path`
**Plan:** `docs/superpowers/plans/2026-04-29-rotatingfilesink-day-boundary-cache.md` (amended with Task 0 — BDN baseline before fix).

**Items closed:** C1.

**Acceptance:**
- `RotatingFileSinkBenchmarks.ShouldRotate_HotPath` ratio ≥10x after fix (~50c → ~3c).
- Existing 4 `RotatingFileSinkTests` still pass.
- New `Enqueue_DayBoundaryCrossed_RotatesToNextFile` xUnit test passes.
- Cost-map §5 / §9 entries marked resolved with commit SHA.

---

## Phase 2 — Packet-tree symmetry BDNs

**Branch:** `chore/260429-bdn-packet-tree-symmetry`
**Plan:** `docs/superpowers/plans/2026-04-29-phase2-packet-symmetry.md` (written when Phase 1 merges).

**Items closed:** M1 (ForkSink packet), M2 (MultiSink packet N=2), M4 (FilterSink packet), M9 (SpscQueueSink packet end-to-end Push).

**New BDN classes:**
- `benchmarks/Relay.Benchmarks/PacketSinks/PropagatePacketBenchmarks.cs` — mirror of `PropagateBenchmarks`, uses `ByteCounterPipe` + `ForkSink` (packet) + `Next?.Enqueue` chains.
- `benchmarks/Relay.Benchmarks/PacketSinks/MultiPacketEnqueueBenchmarks.cs` — mirror of `MultiEnqueueBenchmarks`, no Multi2-equivalent yet (M3 is Phase 6).
- `benchmarks/Relay.Benchmarks/PacketSinks/FilterPacketSinkBenchmarks.cs` — mirror of `FilterSinkBenchmarks` with `PacketPredicate`.
- Extend `QueuePipeThroughputBenchmarks` (rename to `QueueSinkThroughputBenchmarks` if not already done) with `Push_Single_Packet` / `Push_Batch_Packet` over `SpscQueueSink` (packet base) — needs `TestSpscPacketSink` helper analogous to `TestSpscPipe`.

**Acceptance:** every new BDN runs to completion under `--job short`. Numbers documented in `benchmarks/artifacts/2026-04-29-phase2/`. No production code changes.

---

## Phase 3 — MPSC byte ring + Mpsc end-to-end

**Branch:** `chore/260429-bdn-mpsc-byte-and-end-to-end`
**Plan:** `docs/superpowers/plans/2026-04-29-phase3-mpsc-byte-and-end-to-end.md` (written when Phase 2 merges).

**Items closed:** H1, H2, H3, H4.

**New BDN classes:**
- `benchmarks/Relay.Benchmarks/MpscByteRingBufferBenchmarks.cs` — parallel to `ByteRingBufferBenchmarks`. `[Params(Capacity, PayloadSize)]` matrix; `TryPublish_Empty`, `RoundTrip`, `TryPublish_Full`.
- Extend `QueueSinkThroughputBenchmarks` with `MpscPush_Single` / `MpscPush_Batch` (typed) using a `TestMpscPipe` helper.
- New `benchmarks/Relay.Benchmarks/PacketSinks/MpscPacketQueueSinkThroughputBenchmarks.cs` end-to-end packet variant with `TestMpscPacketSink`.

**Acceptance:** Mpsc throughput ≥ 80% of Spsc baseline at single-producer, no allocs in steady state.

---

## Phase 4 — Concrete backend BDNs

**Branch:** `chore/260429-bdn-backends`
**Plan:** `docs/superpowers/plans/2026-04-29-phase4-backends.md` (written when Phase 3 merges).

**Items closed:** H5, H6, H7, H8, M5, M6, M7.

**New BDN classes:**
- `benchmarks/Relay.Benchmarks/Sinks/SharedMemorySinkBenchmarks.cs` — `[SupportedOSPlatform("windows")]`. Measures `SharedMemorySink.Accept` (CAS loop + modular `WriteRing`).
- `benchmarks/Relay.Benchmarks/Sinks/RamPacketSinkBenchmarks.cs` — `RamSink` (packet) `Accept` cost.
- `benchmarks/Relay.Benchmarks/Sinks/MmfSinkBenchmarks.cs` — typed `MmfSink<Entry64>.WriteToBackend` via `TestMmfPipe` consumer driver. **Validates the "fastest durable backend" claim.**
- `benchmarks/Relay.Benchmarks/Sinks/UdpSinkBenchmarks.cs` — `UdpSink.WriteToBackend` (loopback). **Validates the throughput-ceiling claim.**
- `benchmarks/Relay.Benchmarks/Sinks/BatchSinkBenchmarks.cs` — abstract base scratch-buffer copy cost via a test subclass.
- `benchmarks/Relay.Benchmarks/Sinks/NamedPipeSinkBenchmarks.cs` — `[SupportedOSPlatform("windows")]`. Loopback NamedPipe server.
- `benchmarks/Relay.Benchmarks/Sinks/UnixSocketSinkBenchmarks.cs` — `[SupportedOSPlatform("linux")] [SupportedOSPlatform("macos")]`. Loopback unix-domain server.

**Acceptance:** all BDNs run on at least one supported platform; platform-gated BDNs are silently skipped on the other platforms (BDN's `[SupportedOSPlatform]` honors this). Cost-map §1/§2 will be updated in Phase 8 with measured cycles.

---

## Phase 5 — Packet API contract BDNs

**Branch:** `chore/260429-bdn-packet-api-contracts`
**Plan:** `docs/superpowers/plans/2026-04-29-phase5-packet-api.md` (written when Phase 4 merges).

**Items closed:** H9, H10.

**Extensions to `ByteEnqueueBenchmarks`:**
- `Depth1_Byte_TryEnqueue_Healthy` — non-fallthrough variant on healthy sealed sub.
- `Depth1_Byte_TryEnqueue_Reject` — non-fallthrough variant when `Accept` returns false (no Next traversal regardless).
- `Depth1_Byte_Drop_NextNull_Unhealthy` — terminal drop hits `Interlocked.Increment(_dropCount)`.
- `Depth1_Byte_Drop_NextNull_Reject` — same with Accept=false.

**Acceptance:** TryEnqueue ratio < 1.0 vs Enqueue (TryEnqueue is strictly cheaper on the accepted path because it skips the propagate/fallthrough field test). Drop path adds ~25c (Interlocked) over the no-drop path; documented.

---

## Phase 6 — Multi2PacketSink design gap

**Branch:** `feat/260429-multi2-packet-sink`
**Plan:** `docs/superpowers/plans/2026-04-29-phase6-multi2-packet.md` (written when Phase 5 merges).

**Items closed:** M3.

**New production code:**
- `src/Relay/MultiSink.Packet.cs` — append `Multi2PacketSink<TC1, TC2> where TC1 : PacketSink where TC2 : PacketSink` sealed CRTP type. Mirrors `Multi2Sink<T,TC1,TC2>` semantics (fixed-arity broadcast, OR-reduction `IsHealthy`, both children always called).
- `src/Relay/Builder/SinkChain.Packet.cs` — append `.Multi<TC1,TC2>(TC1, TC2)` overload analogous to typed `SinkChain<T,THead>.Multi<TC1,TC2>`.
- `tests/Relay.Tests/MultiPacketSinkTests.cs` — extend with `Multi2PacketSink` cases (broadcast, IsHealthy, Flush, Dispose).
- `benchmarks/Relay.Benchmarks/PacketSinks/MultiPacketEnqueueBenchmarks.cs` (created in Phase 2) — append `Multi2_Packet_Enqueue` baseline.

**Acceptance:** `Multi2PacketSink` enqueue ≤ `MultiSink` packet (N=2) measured ratio (CRTP saves ≥10% on Golden Cove). Existing typed `Multi2Sink` tests not affected. CLAUDE.md `MultiSink<T>` semantics doc updated to mention parallel packet type.

---

## Phase 7 — MPSC multi-thread contention

**Branch:** `chore/260429-bdn-mpsc-multi-thread-contention`
**Plan:** `docs/superpowers/plans/2026-04-29-phase7-mpsc-contention.md` (written when Phase 6 merges).

**Items closed:** M10 (closes blind subgraph cost-map §8).

**New BDN class:**
- `benchmarks/Relay.Benchmarks/MpscContentionBenchmarks.cs` — uses `[Params(producerCount: 1, 2, 4, 8)]` and `Thread`-based producer harness (BDN's built-in `[Benchmark]` does not parallelize automatically). Single consumer thread under `MpscRingBuffer<Entry64>` and `MpscByteRingBuffer`.
- Implementation pattern: `[GlobalSetup]` spawns `producerCount` threads pinned to distinct cores via `ProcessorAffinity`; benchmark method coordinates them via `ManualResetEventSlim` start gate; counts retries via a shared `_retryCount` field; reports throughput (items/s) and average retries/Publish.

**Acceptance:** retry distribution under 4-producer load documented in `benchmarks/artifacts/2026-04-29-phase7/`. Cost-map §8 row for "MPSC CAS-retry distribution" downgraded from blind subgraph to "measured: <X>% retry rate at <Y> producers".

---

## Phase 8 — Cost-map calibration

**Branch:** `docs/260429-cost-map-calibration`
**Plan:** `docs/superpowers/plans/2026-04-29-phase8-calibration.md` (written when Phase 7 merges).

**Items closed:** — recalibrates the cost-map document itself.

**Edits to `docs/reports/2026-04-29-resource-cost-map-relay.md`:**
- §1 Per-Entry Cost Table — replace `~Xc` predictions with `~Yc (BDN: Z ns @ 4.5 GHz)` for every node now covered by a BDN.
- §2 Top 20 — re-rank by measured cycles where available.
- §9 Delta — add new row group "BDN-calibrated":
  - `MpscRingBuffer.TryPublish` ~30c → ~10c (uncontended, measured).
  - `MultiSink.Accept` ~41c → ~31c.
  - `Multi2Sink.Accept` ~32c → ~27c.
  - `FilterSink.Accept` (Pass) ~18c → ~28c.
  - `Multi_IsHealthy` ~16c → ~5c (short-circuit + JIT-fold of trivial `IsHealthy`).
  - `DispatchSink.Enqueue` (sealed) — annotation: prediction is upper-bound assuming non-trivial `Accept`; trivial `Accept` collapses to ~3c.
- §8 Blind Subgraphs — remove the MPSC-contention entry (now measured in Phase 7).

**Acceptance:** every node in §1 / §2 carries either a BDN reference (`(BDN: <ns> ns)` annotation) or an explicit "static estimate, not measured" note. No silent estimates.

---

## What lives in this orchestration vs the child plans

This master plan is a **map**, not the implementation. Every phase has a dedicated child plan that contains the bite-sized TDD-style steps an executor follows. Child plans are written **just-in-time** as the previous phase merges — keeps them aligned with the actual `develop` HEAD at the time of execution.

Order of operations for the implementer:
1. Read this master plan.
2. Read the **current** phase child plan.
3. Check out the phase branch off latest `develop`.
4. Execute the child plan with `superpowers:subagent-driven-development`.
5. Open PR. Merge after approval. Master + master plan updated with PR link.
6. Move to next phase.
