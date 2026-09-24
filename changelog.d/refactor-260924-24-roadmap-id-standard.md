---
adr: none
---
### Docs
- Standardized roadmap ids to the cross-project `S<n>`/`S<n>.<m>` grammar (brunomaida/claude-config#64): `R1`..`R22` -> `S1`..`S22` in `docs/roadmap.md`, `docs/roadmap.config.json` (members/releases/aliases) and `changelog.d/docs-260921-5-doc-sync.md`. The `docs/roadmap.config.json` `tree` phase-group ids (`P0`..`P6`) are a separate grouping namespace, not covered by this id map, and were left untouched. Regenerated `docs/_index.md` via `retrofit_docs.py` (pre-existing staleness, unrelated to the rename).

### Perf
- N/A: documentation-only id rename, no runtime code path changed.
