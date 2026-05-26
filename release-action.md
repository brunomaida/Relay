# Release Action — v1.0.3

**Date:** 2026-05-26
**Version:** 1.0.3
**Branch:** develop → master
**Stack:** .NET 9 / C# 13 (`Relay.sln`)
**Summary:** Receiver hierarchy launch (UDP / TCP / NamedPipe / SharedMemorySpsc) + hot-path-audit hardening of frame-length validation (F0 / F0b / F1 / F3).

---

## Pre-merge Checklist

| # | Gate | Status | Notes |
|---|---|---|---|
| 2a | Correct branch | **PASS** | on `develop` |
| 2b | Working tree clean | **PASS** | release prep committed in `30b14de`; `.claude/settings.local.json` excluded (local user state) |
| 2c | Synced with remote | **PASS** | `origin/develop` matches `develop` |
| 2d | No pending branches | **WARN** | local `feature/260525-4-relay-receivers` still exists (already merged via PR #13). Safe to delete: `git branch -D feature/260525-4-relay-receivers` |
| 2e | Commit log reviewed | **PASS** | 5 commits since v1.0.2 (see below) |
| 2f | CI green on `develop` | **PASS** | run 26464262517 — conclusion `success`, head `3c30845` |
| 3a | Build succeeds (zero warnings, `-warnaserror`) | **PASS** | `dotnet build Relay.sln -c Release -warnaserror` — 0 warnings, 0 errors |
| 3b | No banned patterns | **WARN** | `src/Relay/Sinks/RotatingFileSink.cs:67` uses `DateTime.UtcNow` as the default factory for an injectable `Func<DateTime>`. Established convention (clock injection point exists); not a domain rule using `DateTime` directly. Per `banned-api-enforce` skill: warning, not blocking |
| 4 | Tests pass | **PASS** | 228 / 228 passed, 10 skipped (pre-existing Windows-only / Linux-only); commit-gate filter `Category!=Endurance&Category!=Stress&Category!=Perf` |
| 5.0 | Changelog fragments consolidated | **PASS** | 4 fragments found in `changelog.d/`. One pair was a slug-rename duplicate (`260525-4-receivers.md` ≡ `feature-260525-4-relay-receivers.md`); kept the `feature-…` form per project convention (commit a5768e8) and removed the slug-only file. 3 unique fragments consolidated into `CHANGELOG.md` § `[1.0.3] - 2026-05-26`; originals moved to `changelog.d/archived/1.0.3/` |
| 5a | CHANGELOG versioned | **PASS** | `## [1.0.3] - 2026-05-26` populated; new empty `## [Unreleased]` preserved above |
| 5b | Project docs up-to-date | **PASS** | `docs/reports/2026-05-26-hot-path-audit-receivers.md` + `docs/reports/2026-05-26-resource-cost-map-receivers.md` cover the receiver delta. `docs/topology.md` not strictly required by CLAUDE.md gate but **consider updating** (PacketReceiver hierarchy is currently undocumented there) |
| 5c | CLAUDE.md consistent | **PASS** | last modified 2026-05-12 (14 days ago); no references to removed/renamed projects |
| 5d | README.md current | **PASS** | source-layout table patched to add `PacketCallback` / `PacketReceiver` / `Receivers/Udp\|Tcp\|NamedPipe\|SharedMemorySpsc` rows + `Builder/RelayBuilder.From*` factories row. (Full `/create-readme` rebuild deferred — surgical patch sufficed to bring the table current) |
| 5e | TOPOLOGY.md updated | **N/A** | CLAUDE.md does not mandate `TOPOLOGY.md` as a release gate. `docs/topology.md` exists and lowercased; lib-internal — see note under 5b |
| 5f | Benchmark report present | **WARN** | no `docs/reports/*bdn*\|*bench*\|*perf*` file within last 30 days. New BDN data ships in `docs/reports/2026-05-26-hot-path-audit-receivers.md` § Cycle budget — but its filename matches `*hot-path-audit*` not the gate's glob. Substantively satisfied |
| 5g | README public API references valid | **N/A** | no `<RootNamespace>` declared in `Directory.Build.props`; gate skipped per skill |
| 5h | Hot-path reports staleness | **PASS** | `docs/reports/2026-05-26-hot-path-audit-receivers.md` + `docs/reports/2026-05-26-resource-cost-map-receivers.md` cover this release's scope; scope last touched 2026-05-26 = report date |
| 5i | Bench history fresh | **N/A** | `docs/perf/bench-history.csv` does not exist |
| 5j | README baseline drift | **N/A** | no `<!-- bench-baseline:start/end -->` markers in README |
| 5k | Bench refs inventory | **N/A** | depends on bench-history.csv |
| 5l | README auto-update | **N/A** | gate 5j N/A |
| 6 | Version tags set | **PASS** | `Directory.Build.props` bumped to `1.0.3` — `Version`, `AssemblyVersion`, `FileVersion` all `1.0.3` (commit `30b14de`) |
| 7 | No sensitive files | **PASS** | `git diff master..develop --name-only` — 0 matches against env / credentials / secrets / key / pem |

## Commits in this Release (v1.0.2 → develop)

```
3c30845 fix: receivers — F0/F0b correctness + F1 doc + F3 stackalloc (#16)
b7e2477 feat: add SharedMemorySpscReceiver and NamedPipeReceiver (#15)
852b386 Merge pull request #13 from brunomaida/feature/260525-4-relay-receivers
a5768e8 chore: rename changelog fragment to match branch slug convention w/Claude
58dfed1 feat: Issue #4 slice 4b — PacketCallback + Receiver hierarchy (UDP + TCP) w/Claude
```

## Audit Findings (post v1.0.2)

| ID | Sev | Status |
|---|---|---|
| F0 — Tcp/Pipe wire desync on bogus frameLen | HIGH | **CLOSED** (PR #16) |
| F0b — SharedMemorySpsc consumer stall on bogus frameLen | HIGH | **CLOSED** (PR #16) |
| F1 — TcpReceiver "non-blocking" doc lie | HIGH (doc) | **CLOSED** (PR #16) |
| F3 — NamedPipeReceiver `_header` POH waste | MEDIUM | **CLOSED** (PR #16) |
| F2 — SHM zero-copy fast path | — | **WITHDRAWN** (unsafe without F-SHM gate) |
| F-SHM — SharedMemorySpscSink lacks producer-side overrun gate | MEDIUM (adjacent, pre-existing) | tracked separately |

## Next Step

All non-N/A gates either PASS, WARN-acceptable, or have explicit follow-up:

1. **Delete** stale local branch: `git branch -D feature/260525-4-relay-receivers`.
2. **Bump version** in `Directory.Build.props` — `<Version>`, `<AssemblyVersion>`, `<FileVersion>` from `1.0.2` to `1.0.3`.
3. **Stage + commit** release prep (CHANGELOG, README, archived fragments, version bump):
   `git add CHANGELOG.md README.md changelog.d/ Directory.Build.props release-action.md`
   `git commit -m "chore: prepare release v1.0.3 w/Claude"`
4. **Run** `/release-2-merge-master` to merge `develop` → `master`, tag `v1.0.3`, and trigger `/release-3-execute`.

Optional (recommended): update `docs/topology.md` to add the `PacketReceiver` hierarchy section before tagging (not blocking).

## Post-release

- [ ] Verify GitHub Actions release workflow on tag push.
- [ ] Confirm GitHub Release notes generated from `CHANGELOG.md` § `[1.0.3]`.
- [ ] Back-merge `master` → `develop` to bring the version-bump commit back (per project release-3 convention).
