# Release Action — v1.0.0

**Date:** 2026-05-09
**Version:** 1.0.0
**Branch:** develop -> master (master does not exist yet — will be created on merge)
**Stack:** .NET 9 / C# 13 (`Relay.sln`)
**Summary:** First stable release of Relay. Composable fallback dispatch pipeline for unmanaged structs. Full SPSC/MPSC support, typed and packet hierarchies, builder API, and concrete sinks.

---

## Pre-merge Checklist

| # | Gate | Status | Notes |
|---|------|--------|-------|
| 2a | Correct branch | PASS | on `develop` |
| 2b | Working tree clean | PASS | `.claude/scheduled_tasks.lock` untracked only |
| 2c | Synced with remote | PASS | `develop` == `origin/develop` |
| 2d | No pending branches | WARN | 10 local branches excluded from release by user decision (future work / in-progress) |
| 2e | Commit log reviewed | PASS | 30 commits |
| 2f | CI green on develop | WARN | No CI runs found — CI not configured or never pushed with workflow |
| 3a | Build succeeds (zero warnings) | PASS | `dotnet build -c Release -warnaserror` — 0 warnings, 0 errors |
| 3b | No banned patterns | WARN | `DateTime.UtcNow` in `RotatingFileSink.cs:51,97,174` (cold paths: ctor, rotation, boundary computation — hot-path fix already landed). `Thread.Sleep` in `SpscQueueSink.cs:193`, `SpscQueueSink.Packet.cs:150`, `MpscQueueSink.cs:175`, `MpscQueueSink.Packet.cs:170` (consumer backoff — intentional empty-ring design). No hot-path violations. |
| 4 | Tests pass | PASS | 212 passed, 1 skipped, 0 failed — filter was `Category!=Endurance` (pre-fix); re-run with `Category!=Endurance&Category!=Stress&Category!=Perf` |
| 5a | CHANGELOG versioned | PASS | `[1.0.0] - 2026-05-09` entry present; new empty `[Unreleased]` above it |
| 5b | Project docs up-to-date | PASS | `docs/topology.md` updated 2026-05-03; includes RamSink→MemorySink rename |
| 5c | CLAUDE.md consistent | PASS | Last modified today (2026-05-09) |
| 5d | README.md current | PASS | All 5 solution projects listed (`Relay`, `Relay.Sinks.Http`, `Relay.Sinks.Observability`, `Relay.Tests`, `Relay.Benchmarks`) |
| 5e | TOPOLOGY.md updated | PASS | `docs/topology.md` exists; updated with architectural changes in this release |
| 5f | Benchmark report present | PASS | `docs/reports/2026-04-30-*` — 9 days ago |
| 6 | Version tags set | PASS | `Directory.Build.props` created with `<Version>1.0.0</Version>` |
| 7 | No sensitive files | PASS | No `.env`, credentials, secrets, keys, or PEM files in history |

---

## Commits in this Release (develop, 30 most recent)

```
a8afe39 docs: fix stale RamSink→MemorySink refs in CLAUDE.md project layout w/Claude
bbd60a0 Merge branch 'refactor/rename-ramsink-memorysink' into develop
cc3551c chore: clean up CLAUDE.md redundancies + expand settings permissions w/Claude
81341d8 docs: update CHANGELOG/README/topology after RamSink->MemorySink rename w/Claude
a3b19f9 refactor: rename RamSink→MemorySink, add [Obsolete] compat shims w/Claude
edd2ef5 Merge pull request #12 from brunomaida/fix/260430-h26-m2
5f9ac88 fix: remove Timeout from sync stress tests — xUnit only supports Timeout on async [Fact] w/Claude
f645812 fix: close M2 drain race + add H26 stress suite w/Claude
88030c3 Merge pull request #11 from brunomaida/fix/260430-audit-v4-fixes
f574b58 docs: add hot-path check notes and performance/memory report w/Claude
b890380 fix: correct MpscSlotLayoutBenchmarks consumer-thread deadlock; add BDN results to audit report w/Claude
1358588 fix: resolve v4 audit findings — ForkSink.Packet, MpscQueueSink ctor, packet ConsumeLoop flush split, MpscRingBuffer stride layout, SharedMemorySink fast path w/Claude
726549e Merge pull request #10 from brunomaida/docs/260429-phase6-public-api-docs
546c74e docs: add Multi2PacketSink to TOPOLOGY — missed in prior commit w/Claude
c803f1a docs: add Multi2PacketSink<TC1,TC2> to CHANGELOG, README, TOPOLOGY w/Claude
f6bca17 Merge pull request #9 from brunomaida/docs/260429-cost-map-calibration
dac3eab docs: Phase 8 calibration — fold all Phase 1-7 BDN measurements into cost-map; close blind subgraph §8 w/Claude
06ffbc2 chore: absorb session permission strays w/Claude
6a16046 Merge pull request #8 from brunomaida/chore/260429-bdn-mpsc-multi-thread-contention
4e49d4f docs: mark Phase 7 done -- MPSC contention BDNs landed w/Claude
2b2a19e test: Phase 7 BDN runs -- MPSC multi-producer contention numbers w/Claude
915ad84 test: add MpscByteContentionBenchmarks (M10 packet -- multi-producer throughput) w/Claude
990f24e test: add MpscContentionBenchmarks (M10 typed -- multi-producer throughput at N=1,2,4,8) w/Claude
9278b08 docs: add Phase 7 child plan w/Claude
86a8864 Merge pull request #7 from brunomaida/feat/260429-multi2-packet-sink
b17e63e docs: mark Phase 6 done — Multi2PacketSink CRTP variant landed w/Claude
18b9c0d docs: CLAUDE.md mentions Multi2PacketSink as parallel CRTP variant for packet hierarchy w/Claude
eed0223 test: extend MultiPacketEnqueueBenchmarks w/ Multi2_Packet_Enqueue (M3 — CRTP packet variant) w/Claude
7a9841c feat: add SinkChain<THead>.Multi<TC1,TC2>(c1,c2) overload for packet CRTP variant w/Claude
b7da403 feat: add Multi2PacketSink<TC1,TC2> CRTP variant for packet hierarchy w/Claude
```

---

## Actions Required Before `/release-2-merge-master`

### 1. Version CHANGELOG.md (gate 5a)

In `CHANGELOG.md`, rename:
```
## [Unreleased]
```
to:
```
## [1.0.0] - 2026-05-09
```
Then insert a new empty section above it:
```markdown
## [Unreleased]

---

## [1.0.0] - 2026-05-09
...existing content...
```

### 2. Set version in project files (gate 6)

Create `Directory.Build.props` at repo root:
```xml
<Project>
  <PropertyGroup>
    <Version>1.0.0</Version>
    <AssemblyVersion>1.0.0</AssemblyVersion>
    <FileVersion>1.0.0</FileVersion>
  </PropertyGroup>
</Project>
```

### 3. Commit

```
chore: prepare release v1.0.0 w/Claude
```

### 4. Run `/release-2-merge-master`

---

## Post-release

- [ ] Verify GitHub Packages publish (if CI/CD is configured)
- [ ] Notify stakeholders
- [ ] Tag `v1.0.0` on master
