# Context

Continuação do `/release-3-execute` para Relay v1.0.4. release-notes.md aprovado (somente mudanças da versão atual). publish-target=none (tag + GitHub Release apenas). CI monitor=yes.

## Estado atual
- Branch: `master` (após merge develop → master)
- release-notes.md: escrito para v1.0.4
- Workflows CI/CD: já existem e corretos (release.yml dispara em tags `v*`)

---

## Phase 4 — Execução (steps restantes)

### 1. Build final no master
```
dotnet build Relay.sln -c Release -warnaserror
```

### 2. Tests finais no master
```
dotnet test Relay.sln -c Release --no-build --logger "console;verbosity=minimal" --filter "Category!=Endurance&Category!=Stress&Category!=Perf"
```

### 3. Tag + push
```
git tag v1.0.4
git push origin master --tags
```
→ Dispara `release.yml` no GitHub Actions.

### 4. Monitor CI
- `gh run list` para detectar run da tag
- `gh run watch <run_id>` até conclusão
- Se falhar: diagnose → auto-heal loop (max 3 ciclos)

### 5. GitHub Release
```
gh release create v1.0.4 --title "v1.0.4" --notes-file release-notes.md --repo brunomaida/Relay
```

### 6. Back-merge master → develop
```
git checkout develop
git merge master --no-ff -m "chore: back-merge v1.0.4 release w/Claude"
git push origin develop
```

---

## Verification
- Tag `v1.0.4` visível em `git tag`
- CI verde: `gh run list --branch v1.0.4`
- GitHub Release criado: `gh release view v1.0.4`
- develop ahead of master: `git log master..develop --oneline`
