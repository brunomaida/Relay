---
adr: none
---
### Docs
- Standardized legacy R-prefixed roadmap ids to the cross-project S-prefixed grammar (S<n>) (brunomaida/claude-config#64) in `docs/roadmap.md`, `docs/roadmap.config.json` (members, releases and aliases) and `changelog.d/docs-260921-5-doc-sync.md`. The `docs/roadmap.config.json` `tree` phase-group ids are a separate grouping namespace, not covered by this id map, and were left untouched. Regenerated `docs/_index.md` via `retrofit_docs.py` (pre-existing staleness, unrelated to the rename).

### Perf
- N/A: documentation-only id rename, no runtime code path changed.
