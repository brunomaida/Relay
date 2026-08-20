# Release Action — v1.2.0

**Date:** 2026-08-20
**Version:** 1.2.0
**Branch:** develop -> master
**Stack:** .NET (Relay.sln)
**Summary:** Adds `Relay.Memory.NativeBuffer` — the sole sanctioned entry point for `NativeMemory.*` calls under `src/Relay` — with a self-proving source-scan gate and oracle-based footprint regression tests catching the two-arg `NativeMemory.AllocZeroed` overload-confusion bug class. Migrates all 4 production native-allocation call sites through it; documents the change and a CLAUDE.md/TOPOLOGY hygiene pass.

---

## Pre-merge Checklist

| # | Gate | Status | Notes |
|---|------|--------|-------|
| 2a | Correct branch | PASS | on `develop` |
| 2b | Working tree clean | PASS | committed pending docs (plan files, roadmap.html), gitignored `.obsidian/` |
| 2c | Synced with remote | PASS | pushed 2ff7ffd, a8afb31 |
| 2d | No pending branches | PASS | deleted 3 stale local-only branches (feature/260610-0-bench-history-seed, feature/260610-bench-methodology, fix/260706-0-remove-w-claude-attribution) — all already superseded on develop |
| 2e | Commit log reviewed | PASS | 15 commits since master |
| 2f | CI green on develop | PASS | run 32403357213, sha a8afb31 |
| 3a | Build succeeds (zero warnings) | PASS | `dotnet build -c Release -warnaserror` — 0/0 |
| 3b | No banned patterns | PASS | one `DateTime.UtcNow` default-lambda fallback in `RotatingFileSink` ctor (`_utcNow ?? (() => DateTime.UtcNow)`) — matches the prescribed injected-clock pattern, not a violation |
| 4 | Tests pass | PASS | 295 passed, 0 failed, 10 skipped (platform-conditional: MMF/Unix-socket) |
| 4a | Security test coverage | N/A | no `security-surface:` declared; user confirmed no auth/PII/financial/public-internet surface |
| 5.0 | changelog.d fragments consolidated | PASS | 2 fragments; 1 was missing required `### Perf` section — added, then consolidated into `[1.2.0]`, archived to `changelog.d/archived/1.2.0/` |
| 5a | CHANGELOG versioned | PASS | `[1.2.0] - 2026-08-20` added, new empty `[Unreleased]` above |
| 5b | Project docs up-to-date | PASS | architecture-decisions.md, bench-methodology.md, TOPOLOGY.md, CLAUDE.md all updated for this release's scope |
| 5c | CLAUDE.md consistent | PASS | last modified today |
| 5d | README.md current | PASS | exists, project structure section present |
| 5e | TOPOLOGY.md updated | PASS | updated same day as src/ changes |
| 5f | Benchmark report present | WARN | most recent BDN report is from April 2026 — no BDN re-run needed this release (ctor/dispose-path only change, per fragment's own Perf note) |
| 5g | README API references valid | N/A | no `RootNamespace` detected in `Directory.Build.props` |
| 5h | Hot-path reports fresh | NEEDS ACTION → accepted | 2026-07-02 report stale vs. touches to `MpscRingBuffer.cs`/`SpscRingBuffer.cs`/`MemorySink.cs`/`MemorySink.Packet.cs`; user accepted staleness — changes are ctor/`Reset()`/dispose-path `NativeBuffer` delegation only, never `Enqueue`/`Accept` |
| 5i | Bench history fresh | N/A | `docs/perf/bench-history.csv` not used by this project (uses `docs/reports/bench-history/` instead) |
| 5j | README baseline drift | N/A | no `<!-- bench-baseline -->` markers in README |
| 5k | Bench refs inventory | N/A | gated on 5i's CSV, which is N/A |
| 5l | README auto-update | N/A | 5j did not trigger |
| 5m | Declared docs freshness | PASS | `docs/architecture-decisions.md` scope (`*.sln`, `**/*.csproj`, `Directory.Build.props`, `Directory.Packages.props`) — no such files changed since last doc update |
| 5n | README quality audit | PASS | 5n.1 all critical+supplementary sections present; 5n.2 ASCII diagrams present (Pipeline Topologies section); 5n.3 Performance section present |
| 5o | Doc-taxonomy completeness | PASS | docs-doctor: archetype `lib-hotpath`, all required files present |
| 5p | Doc-index freshness | PASS | `docs/_index.md` current |
| 5q | External lib DLL freshness | N/A | no `.claude/config/libs-manifest.json` |
| 5r | Flow-graph sync check | N/A | no `flow-graph:` declared in CLAUDE.md |
| 6 | Version tags set | PASS | `Directory.Build.props` updated `1.1.0` → `1.2.0` (version-bump confirmed: MINOR, `### Added` section present, no public-API break — `NativeBuffer` is internal) |
| 7 | No sensitive files | PASS | no matches |

## Commits in this Release

```
a8afb31 fix: gate ucrtbase.dll calibration test to Windows-only
2ff7ffd docs: add pending plan docs and roadmap, ignore .obsidian vault config
6a26ee1 docs: correct NativeBuffer summary and ThreadStatic finalizer accounting claims
261afe4 docs: document NativeBuffer helper, footprint guard tests, and packet-sink zeroing exception
65c2834 test: harden gate against vacuous-pass on empty/over-filtered file scan
c924cec test: add self-proving source-scan gate for NativeMemory.* usage
3e6ce4d test: add native-allocation footprint guard for Relay ring buffers
0cbc84e Merge pull request #19 from brunomaida/chore/260813-doc-scopes-flow-graph
009c3de chore: declare doc-scopes ADR freshness gate (Gate 5m)
c356431 Merge branch 'docs/260812-0-flows-sync-topology' into develop
e6e8c9f docs(topology): add flows-sync Assembly Roles and Dependency Direction Graph
84942d4 Merge branch 'fix/260724-claudemd-relay' into develop
b300e51 docs: fix false provenance tag on Multi2/Multi BDN claim
d67284e docs: prune stale/dup sections in Relay CLAUDE.md
8427ac5 chore: back-merge v1.1.0 release
```

## Next Step

All gates PASS or N/A (WARN on 5f acceptable; NEEDS ACTION on 5h accepted by user as staleness with no hot-path behavior change; 4a N/A confirmed by user).

1. ~~CHANGELOG.md~~ — done (Step 5.0 consolidated the 2 fragments into `[1.2.0]`).
2. ~~Set version in `Directory.Build.props`~~ — done (`1.1.0` → `1.2.0`).
3. Commit: `chore: prepare release v1.2.0`
4. Run `/release-2-merge-master`

## Post-release

- [ ] Verify CI/CD pipeline (if applicable)
- [ ] Notify stakeholders
