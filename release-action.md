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
| 2b | Working tree clean | PASS | `.claude/settings.local.json` modified (tool settings — not release-relevant; intentionally excluded from release commit) |
| 2c | Synced with remote | PASS | No diff vs `origin/develop` |
| 2d | No pending branches | PASS | `refactor/260524-batch-hook` force-deleted (rejected; BDN artifacts only) |
| 2e | Commit log reviewed | PASS | 27 commits (incl. fix + prep) |
| 2f | CI green on develop | PASS | [Run #26406216104](https://github.com/brunomaida/Relay/actions/runs/26406216104) — conclusion: success |
| 3a | Build succeeds (zero warnings) | PASS | `dotnet build -c Release -warnaserror` — 0 warnings, 0 errors |
| 3b | No banned patterns | PASS | `DateTime.UtcNow` removed from all `RotatingFileSink` methods. Remaining occurrence on line 67 is the default `Func<DateTime>` factory definition — structurally required, cold-path only. Commit `a9c8e17`. |
| 4 | Tests pass | PASS | 209 passed, 6 skipped, 0 failed |
| 5a | CHANGELOG versioned | PASS | `[1.0.2] - 2026-05-25` entry added with Added / Fixed / Perf sections |
| 5b | Project docs up-to-date | N/A | No specific doc requirement in CLAUDE.md beyond XML docs |
| 5c | CLAUDE.md consistent | PASS | Last modified 2026-05-12 (13 days ago) |
| 5d | README.md current | PASS | Exists; table lists all projects |
| 5e | TOPOLOGY.md updated | N/A | Not required by CLAUDE.md |
| 5f | Benchmark report present | PASS | `docs/reports/2026-05-25-hot-path-audit-relay.md` (today) |
| 5g | README API references valid | PASS | All `Relay.*` type refs in README resolve to actual source types |
| 5h | Hot-path reports staleness | PASS | Audit run today: `docs/reports/2026-05-25-hot-path-audit-relay.md` — 27/27 PASS, no regressions |
| 5i | Bench history fresh | N/A | No `docs/perf/bench-history.csv` |
| 5j | README baseline drift | N/A | No `<!-- bench-baseline -->` markers in README |
| 5k | Bench refs inventory | N/A | No `docs/perf/bench-history.csv` |
| 5l | README auto-update | N/A | Gate 5j did not trigger |
| 6 | Version tags set | PASS | `Directory.Build.props`: `1.0.1` → `1.0.2`. Commit `874392e`. |
| 7 | No sensitive files | PASS | No sensitive files in `master..develop` diff. `.env` untracked but now in `.gitignore`. |

---

## Commits in this Release (develop → master)

```
874392e chore: prepare release v1.0.2 w/Claude
a9c8e17 fix: inject Func<DateTime> in RotatingFileSink; remove DateTime.UtcNow from methods
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

**All gates PASS or N/A.** Pronto para merge.

Run `/release-2-merge-master`.

## Post-release

- [ ] Verify CI/CD pipeline on master after merge
- [ ] Notify stakeholders
