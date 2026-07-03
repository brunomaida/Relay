---
title: "Roadmap — Relay"
type: report
solution: Relay
status: draft
created: 2026-06-18
---

# Roadmap — Relay

Zero-allocation .NET dispatch/sink relay library. Retrospective phase tracker
mined from git history + `docs/` + `CHANGELOG.md`. Release bands (1.0.0 → 1.0.4,
Unreleased) are derived from `CHANGELOG.md`; phase grouping is thematic.

| id | level | parent | title | reason | spec | plan | report | issue | status |
|----|----|----|----|----|----|----|----|----|----|
| R1  | 1 | — | Typed dispatch hierarchy + SPSC ring + concrete sinks | DispatchSink<T> hot path, lock-free SPSC ring, FileStream/Tcp/Mmf/Memory backends | docs/superpowers/specs/2026-04-24-relay-phase1-design.md | docs/superpowers/plans/2026-04-24-relay-phase1.md | docs/reports/2026-04-23-hot-path-audit-relay.md | | done |
| R2  | 1 | — | Composition sinks + fluent builder | Fork/Multi/Multi2/Filter/Null + RelayBuilder chain wiring (Next/Prev) | | | docs/reports/2026-04-23-bdn-vs-static-analysis.md | | done |
| R3  | 1 | — | Packet (byte) sink hierarchy | Parallel PacketSink tree over SpscByteRingBuffer for variable-length payloads | | | docs/reports/2026-04-23-bdn-byte-vs-typed.md | | done |
| R4  | 1 | — | SPSC ring hot-path optimization | Remote-index caching, bounds-check elimination, flush-deadline throttling | | | docs/reports/2026-04-23-resource-cost-map-relay.md | | done |
| R5  | 1 | — | BenchmarkDotNet suite | Full hot-path BDN coverage: ring buffers, enqueue depth, multi, filter | | | docs/reports/2026-04-24-bdn-typed-mpsc-propagate.md | | done |
| R6  | 1 | — | Naming canonicalization | Tee→Fork, FanOut→Multi, RamSink→MemorySink + [Obsolete] compat shims | | | | | done |
| R7  | 1 | — | Cost-map BDN coverage | 8-phase master plan: packet symmetry, MPSC byte, backends, Multi2, contention | | docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md | docs/reports/2026-04-29-resource-cost-map-relay.md | | done |
| R8  | 1 | — | RotatingFileSink day-boundary cache | Cache next UTC-midnight in HfClock ticks; remove DateTime.UtcNow.Date from consumer hot path | | docs/superpowers/plans/2026-04-29-rotatingfilesink-day-boundary-cache.md | docs/reports/2026-04-30-hot-path-audit-relay.md | | done |
| R9  | 1 | — | BatchSink — POH scratch accumulator | Batching base over SpscQueueSink; OversizedDropCount; OnFlush(full batch) | | | docs/reports/2026-04-26-hot-path-audit-relay.md | | done |
| R10 | 1 | — | HttpBatchSink + Relay.Sinks.Http | HTTP POST batch sink with circuit breaker + observability counters | | | | | done |
| R11 | 1 | — | SeqSink + Relay.Sinks.Observability | CLEF-over-HTTP sink to Seq /api/events/raw | | | | | done |
| R12 | 1 | — | CI/CD + NuGet packaging | Release workflow, GitHub Packages + NuGet publishing, symbols | docs/superpowers/specs/2026-04-30-release-skills-design.md | | | | done |
| R13 | 1 | — | Thread priority/affinity + RotatingFileSink fileNameFormat | Consumer-thread priority/CPU pinning; custom filename patterns; Func<DateTime> injection | | | | | done |
| R14 | 1 | — | Robustness fixes (v1.0.2) | SHM publish ordering, TcpSink health guard, partial-send loops, oversized-payload guard | | | docs/reports/2026-05-25-hot-path-audit-relay.md | | done |
| R15 | 1 | — | External-AI audit triage | Gemini 3.1 audit → triage + decision execution (§2.2/2.3/2.4/3.3) | docs/text-specs/2026-05-24-ExternalAI-Audit.md | docs/superpowers/plans/2026-05-24-analisando-docs-text-specs-2026-05-24-ex-resilient-cat.md | | | done |
| R16 | 1 | — | PacketReceiver hierarchy + PacketCallback | UDP/TCP/SharedMemory-SPSC/NamedPipe receivers; zero-alloc span callback; From factories | | | docs/reports/2026-05-26-hot-path-audit-receivers.md | | done |
| R17 | 1 | — | Receiver audit fixes (F0/F0b/F1/F3) | Frame-length validation, SHM_MAGIC check, doc correctness, drop pinned header | | | docs/reports/2026-05-26-resource-cost-map-receivers.md | | done |
| R18 | 1 | — | Circular ring topology test suite | Pure/Backend/Saturation/Receiver ring tests; 30s stress + 5s warmup | | docs/superpowers/plans/2026-05-27-quero-criar-um-teste-idempotent-thompson.md | docs/reports/2026-05-27-hot-path-audit-circular-ring-tests.md | | done |
| R19 | 1 | — | Throughput perf tests + warmup | Steady-state throughput benchmarks; JIT warmup in MPSC harness | | docs/superpowers/plans/2026-05-27-quero-criar-um-teste-idempotent-thompson-2.md | | | done |
| R20 | 1 | — | Doc taxonomy | YAML frontmatter, _index hub, relative cross-links, TOPOLOGY naming canon | | | changelog.d/260611-doc-taxonomy.md | | done |
| R21 | 1 | — | Bench methodology + bench-history | bench-methodology doc; bench-history bootstrap via backfill; micro baselines | | | | | done |
| R22 | 1 | — | Resource cost map + robustness/perf fixes | Cost-map refresh; FileStreamSink crash guard, RDTSC throttle, MpscByteRingBuffer recursion fix | | docs/superpowers/plans/2026-06-11-merry-weaving-sprout.md | docs/reports/2026-06-10-resource-cost-map-relay.md | | done |
