# Release Action — v1.0.2

**Date:** 2026-05-25
**Version:** 1.0.2
**Branch:** develop -> master
**Stack:** .NET 9 / C# 13 (Relay.sln)
**Summary:** Bug fixes e melhorias de performance no pipeline de despacho.

---

## Pre-merge Checklist

| # | Gate | Status | Notes |
|---|------|--------|-------|
| 2a | Correct branch | PASS | On `develop` |
| 2b | Working tree clean | **FAIL** | `release-notes.md` modified (unstaged); `.claude/settings.local.json` modified; `.env` untracked in repo root |
| 2c | Synced with remote | PASS | No diff vs `origin/develop` |
| 2d | No pending branches | PASS | `refactor/260524-batch-hook` force-deleted (was rejected; contained only BDN artifacts) |
| 2e | Commit log reviewed | PASS | 25 commits |
| 2f | CI green on develop | PASS | [Run #26406216104](https://github.com/brunomaida/Relay/actions/runs/26406216104) — conclusion: success |
| 3a | Build succeeds (zero warnings) | PASS | `dotnet build -c Release -warnaserror` — 0 warnings, 0 errors |
| 3b | No banned patterns | **FAIL** | `DateTime.UtcNow` at `src/Relay/Sinks/RotatingFileSink.cs:65`, `:132`, `:210` |
| 4 | Tests pass | PASS | 209 passed, 6 skipped, 0 failed |
| 5a | CHANGELOG versioned | **NEEDS ACTION** | Has `[Unreleased]` but no `[1.0.2]` entry; no `changelog.d/` fragments to consolidate |
| 5b | Project docs up-to-date | N/A | No specific doc requirement in CLAUDE.md beyond XML docs |
| 5c | CLAUDE.md consistent | PASS | Last modified 2026-05-12 (13 days ago) |
| 5d | README.md current | PASS | Exists; table lists all projects (Relay, Http, Observability, Tests, Benchmarks) |
| 5e | TOPOLOGY.md updated | N/A | Not required by CLAUDE.md |
| 5f | Benchmark report present | PASS | Most recent: `docs/reports/2026-04-30-hot-path-audit-relay.md` (25 days ago) |
| 5g | README API references valid | PASS | All `Relay.*` type refs in README resolve to actual source types |
| 5h | Hot-path reports staleness | **NEEDS ACTION** | `DispatchSink.cs`, `MpscQueueSink.cs`, `SpscQueueSink.cs` modified after last report (2026-04-30). Run `/hot-path-audit` or `/resource-cost-mapping` before releasing. |
| 5i | Bench history fresh | N/A | No `docs/perf/bench-history.csv` |
| 5j | README baseline drift | N/A | No `<!-- bench-baseline -->` markers in README |
| 5k | Bench refs inventory | N/A | No `docs/perf/bench-history.csv` |
| 5l | README auto-update | N/A | Gate 5j did not trigger |
| 6 | Version tags set | **NEEDS UPDATE** | `Directory.Build.props` still at `1.0.1`; update to `1.0.2` |
| 7 | No sensitive files | PASS | No sensitive files in `master..develop` diff. Note: `.env` exists untracked — confirm not release-relevant |

---

## Issues to Resolve Before Merge

### 2b — Working tree not clean
Commit or stash open changes before merging:
```
M  .claude/settings.local.json  (tool settings — stash or commit separately)
M  release-notes.md             (update content, then commit)
?? .claude/scheduled_tasks.lock (runtime lock — add to .gitignore)
?? .env                         (confirm not release-relevant; add to .gitignore if absent)
```

### 3b — Banned pattern: `DateTime.UtcNow` in `RotatingFileSink.cs`
```
src/Relay/Sinks/RotatingFileSink.cs:65   _currentDay = DateTime.UtcNow.Date;
src/Relay/Sinks/RotatingFileSink.cs:132  _currentDay = DateTime.UtcNow.Date;
src/Relay/Sinks/RotatingFileSink.cs:210  var now = DateTime.UtcNow;
```
CLAUDE.md forbids `DateTime.UtcNow` on all paths (tier: ultra-low-latency, no cold-path exceptions).
Commit `925acac` reverted from HfClock-tick back to `DateTime.UtcNow` for wall-clock correctness (XML doc at line 235 has rationale). If this is an accepted exception, document it explicitly in CLAUDE.md or via a comment — then re-run checklist.

### 5a — CHANGELOG needs `[1.0.2]` entry
Manually update `CHANGELOG.md`:
1. Rename `## [Unreleased]` → `## [1.0.2] - 2026-05-25`
2. Fill body with changes from the 25 commits in this release
3. Insert new empty `## [Unreleased]` section above it

### 5h — Hot-path reports stale
`DispatchSink.cs`, `MpscQueueSink.cs`, `SpscQueueSink.cs` modified after last audit (2026-04-30).
Run `/hot-path-audit` or `/resource-cost-mapping`, or accept staleness and continue manually.

### 6 — Version not updated
Update `Directory.Build.props`:
```xml
<Version>1.0.2</Version>
<AssemblyVersion>1.0.2</AssemblyVersion>
<FileVersion>1.0.2</FileVersion>
```

---

## Commits in this Release (develop → master)

```
f6b437f bench: archive 2026-05-25 final validation BDN run
925acac revert: restore HfClock-tick rotation check in RotatingFileSink (perf)
68a9dda revert: drop oversized-payload bypass from TcpSink.Packet (perf regression)
902df81 docs: add 2026-05-24 external-audit triage source + plans
192392d chore: archive §2.1 zero-fill rejection artifacts
ea2f553 bench: freeze baseline before external-audit triage
7555a76 fix: remove dead code in PinLinux; tighten ThreadAffinity pin test
da425c1 feat: add opt-in threadPriority and affinityCpu params to queue sink ctors
878fa95 docs: tighten MultiSink/ForkSink thread-safety remarks
e720b73 docs: annotate sink thread-safety topology in XML <remarks>
e8b8c42 chore: rename benchmark artifact dir to ASCII
3be513d fix: anchor RotatingFileSink rotation in absolute UTC date
5ec6cce fix: enforce cache-line alignment in Release + fix DispatchSink XML doc
8ccc600 test: skip MMF test on Linux + tighten reader loop coverage
cb598bb refactor: add [Obsolete] SharedMemorySink shim + update benchmark
15face0 fix: enforce SPSC publish ordering in SharedMemorySink
1321d6c fix: also guard bypass path against unhealthy-but-open connection
ec2d019 fix: loop Send/Write in packet sinks for partial-send correctness
a29c69f fix: guard oversized payload in fixed-buffer sinks
dee59d4 test: dispose POH-pinned rings in MpscPacket prev-wire test
94b5b5b fix: wire MpscQueueSink Prev in SinkChain.Packet builder
133e6ff chore: add tier declarations to Relay and RelayStream
ac608b3 feat: add optional fileNameFormat to RotatingFileSink constructor
665402e chore: bump GitHub Actions to Node.js 24 compatible versions
3b715a2 chore: back-merge v1.0.1 release w/Claude
```

---

## Next Step

**Blockers (must resolve before proceed):**
1. **2b** — commit/stash working tree changes (`release-notes.md`, settings)
2. **3b** — fix `DateTime.UtcNow` hits OR document exception in CLAUDE.md
3. **5a** — update CHANGELOG.md with `[1.0.2] - 2026-05-25` entry
4. **5h** — run hot-path audit OR accept staleness explicitly
5. **6** — update version to `1.0.2` in `Directory.Build.props`

Once all gates PASS or are explicitly accepted:
```
git add Directory.Build.props CHANGELOG.md release-notes.md
git commit -m "chore: prepare release v1.0.2 w/Claude"
```
Then run `/release-2-merge-master`.

## Post-release

- [ ] Verify CI/CD pipeline on master after merge
- [ ] Notify stakeholders
