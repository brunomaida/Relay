# Release Action — v1.0.4

**Date:** 2026-05-28
**Version:** 1.0.4
**Branch:** develop → master
**Stack:** .NET 9 / C# 13 (`Relay.sln`)
**Summary:** Circular ring topology tests, throughput benchmarks, and JIT warmup harness for MPSC perf testing.

---

## Pre-merge Checklist

| # | Gate | Status | Notes |
|---|------|--------|-------|
| 2a | Correct branch | PASS | On `develop` |
| 2b | Working tree clean | WARN | `.claude/settings.local.json` modified; 3 untracked `docs/superpowers/plans/` files — tooling/editor artifacts, not source |
| 2c | Synced with remote | PASS | `origin/develop` == `develop` |
| 2d | No pending branches | PASS | `feature/260525-4-relay-receivers` was fully merged (PR #13); deleted locally and remotely |
| 2e | Commit log reviewed | PASS | 22 commits |
| 2f | CI green on develop | PASS | [Run f546136](https://github.com/brunomaida/Relay/actions/runs/26573789052) — `success` |
| 3a | Build succeeds (zero warnings) | PASS | 0 warnings, 0 errors |
| 3b | No banned patterns | PASS | `Thread.Sleep` hits are all in test fixtures (documented exception); no `DateTime.Now` |
| 4 | Tests pass | PASS | 241 passed, 0 failed, 10 skipped (45s) |
| 5.0 | changelog.d/ consolidation | N/A | No fragments |
| 5a | CHANGELOG versioned | NEEDS ACTION | `[1.0.4]` entry missing — add before committing |
| 5b | Project docs up-to-date | N/A | No project-specific doc requirements |
| 5c | CLAUDE.md consistent | PASS | Updated 2026-05-26 (2 days ago, well under 90-day threshold) |
| 5d | README.md current | PASS | Exists; all 5 solution projects listed |
| 5e | TOPOLOGY.md updated | N/A | No `src/` files touched in this release |
| 5f | Benchmark report present | PASS | `2026-04-30-hot-path-performance-memory-relay.md` (28 days ago, within 30-day window) |
| 5g | README API references valid | PASS | No fully-qualified `Relay.*.Type.Method()` calls in README code blocks |
| 5h | Hot-path reports staleness | N/A | No `src/` files touched in this release |
| 5i | Bench history fresh | N/A | `docs/perf/bench-history.csv` not present |
| 5j | README baseline drift | N/A | No `<!-- bench-baseline -->` markers in README |
| 5k | Bench refs inventory | N/A | No bench-history.csv |
| 5l | README auto-update | N/A | Gate 5j not triggered |
| 5m | Declared docs freshness | N/A | No `doc-scopes:` block in CLAUDE.md |
| 6 | Version tags set | NEEDS UPDATE | `Directory.Build.props` has `1.0.3`; must be updated to `1.0.4` |
| 7 | No sensitive files | PASS | No .env, credentials, secrets, or keys in diff |

---

## Commits in this Release

```
f546136 test: add JIT warmup run to MpscThroughputHarness perf tests w/Claude
51aa865 feat: extend Circular stress tests to 30s with warmup + add Perf throughput benchmarks w/Claude
b830b92 test: add CircularThroughputPerfTests.cs steady-stage throughput benchmarks w/Claude
ef87500 test: extend Circular stress tests to 30s with 5s warmup (ReceiverSinkRingTests) w/Claude
d78f39f test: extend Circular stress tests to 30s with 5s warmup (SaturationTests) w/Claude
ec0815d test: extend Circular stress tests to 30s with 5s warmup (BackendSinkRingTests) w/Claude
8d01b45 test: fix RingTestReport.Start baseline seeding after warmup w/Claude
365f11d test: extend Circular stress tests to 30s with 5s warmup (PureSinkRingTests) w/Claude
5f2ea38 docs: add hot-path audit report for circular ring tests (Gate 2 PASS) w/Claude
08dacdc feat: add Circular ring topology tests w/Claude
2601d33 test: remove dead code and strengthen BackendSinkRingTests assertions w/Claude
5921a5b test: add Circular/ReceiverSinkRingTests.cs SharedMemory receiver ring tests w/Claude
36011bc test: add Circular/SaturationTests.cs saturation and backpressure tests w/Claude
0a25d3c test: add Circular/BackendSinkRingTests.cs backend ring tests w/Claude
ff70b39 test: add Circular/PureSinkRingTests.cs pure ring tests w/Claude
ccf47be test: add Circular/Helpers/RingTestReport.cs telemetry helper w/Claude
43f454e test: add Circular/Helpers/RingTopology.cs ring topology builders w/Claude
d73bb7e test: add Circular/Helpers/RingNode.cs ring node types w/Claude
928352a test: fix WriteHop/WriteId to use Unsafe.WriteUnaligned (no silent bool discard) w/Claude
e037c0c test: add CircularPayloads structs for ring topology tests w/Claude
2fb350c docs: update CLAUDE.md with quality audit improvements
8b8a357 chore: back-merge v1.0.3 release w/Claude
```

---

## Next Step

Two NEEDS ACTION items before proceeding:

1. **5a — CHANGELOG.md:** rename `[Unreleased]` → `[1.0.4] - 2026-05-28`, insert new empty `[Unreleased]` above
2. **6 — Version:** update `Directory.Build.props` — `<Version>`, `<AssemblyVersion>`, `<FileVersion>` all `1.0.3` → `1.0.4`
3. Commit: `chore: prepare release v1.0.4 w/Claude`
4. Run `/release-2-merge-master`

---

## Post-release

- [ ] Verify CI/CD pipeline
- [ ] Notify stakeholders
