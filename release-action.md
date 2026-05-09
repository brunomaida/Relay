# Release Action — v1.0.1

**Date:** 2026-05-09
**Version:** 1.0.1
**Branch:** develop -> master
**Stack:** .NET 9 / C# 13
**Summary:** Patch: corrige falhas de CI (testes Windows-only skipped no Linux; flush de BatchSink determinístico via Stop())

---

## Pre-merge Checklist

| # | Gate | Status | Notes |
|---|------|--------|-------|
| 2a | Correct branch | PASS | develop |
| 2b | Working tree clean | PASS | .env e scheduled_tasks.lock são untracked (não rastreados) |
| 2c | Synced with remote | PASS | |
| 2d | No pending branches | PASS | 10 branches existentes — todos já merged em develop |
| 2e | Commit log reviewed | PASS | 5 commits |
| 2f | CI green on develop | PASS | https://github.com/brunomaida/Relay/actions/runs/25613909090 |
| 3a | Build succeeds (zero warnings) | PASS | 0 warnings, 0 errors |
| 3b | No banned patterns | PASS | DateTime.UtcNow em RotatingFileSink cold-path (construtor + rotação) — aceitável |
| 4 | Tests pass | PASS | 199 passed, 5 skipped, 0 failed |
| 5a | CHANGELOG versioned | PASS | [1.0.1] - 2026-05-09 inserido; novo [Unreleased] acima |
| 5b | Project docs up-to-date | PASS | Apenas testes alterados — sem impacto em docs de projeto |
| 5c | CLAUDE.md consistent | PASS | Atualizado em 2026-05-09 |
| 5d | README.md current | PASS | Todos os 5 projetos presentes |
| 5e | TOPOLOGY.md updated | N/A | Sem mudanças arquiteturais |
| 5f | Benchmark report present | WARN | Último relatório: 2026-04-30 (>30 dias) — não bloqueia |
| 6 | Version tags set | PASS | Directory.Build.props → 1.0.1 |
| 7 | No sensitive files | PASS | |

## Commits nesta Release

```
37f6ee8 chore: track release-notes.md + Claude Code permission allowlist update
8a8a341 fix: merge ci-test-failures fix into develop
219e035 chore: add changelog fragment for CI test fix
efc0803 fix: skip Windows-only SharedMemory tests on Linux CI + deterministic BatchSink flush sync
7fb0e0b chore: back-merge v1.0.0 release w/Claude
```

## Status

Todos os gates PASS (5f WARN — não bloqueia). Pronto para `/release-2-merge-master`.

## Post-release

- [ ] Verificar CI/CD pipeline
- [ ] NuGet publicado
- [ ] Tag v1.0.1 no GitHub
