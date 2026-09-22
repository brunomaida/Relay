### Docs
- `README.md`: removed the instruction to append an AI-attribution suffix to commit messages; added `UnixSocketSink`, `SharedMemorySpscSink` (with its `[Obsolete]` `SharedMemorySink` alias) and `Memory/NativeBuffer` to the source-layout table, type hierarchy and concrete-sink list.
- `docs\TOPOLOGY.md`: added `UnixSocketSink` and `SharedMemorySpscSink` to the `PacketSink` hierarchy, namespace list, concrete-sink detail and threading tables (the type previously listed there, `SharedMemorySink`, is an `[Obsolete]` shim).
- `docs\architecture-decisions.md`: added ADR-9 (SHM wire protocol, single producer) and ADR-10 (Http/Observability assembly split), retroactive and citing sources; unrecorded rationale is marked as such.
- `docs\roadmap.md`: repointed R14, R20, R22 to files that exist (`2026-07-02-hot-path-audit-relay.md`, `changelog.d\archived\1.0.5\260611-doc-taxonomy.md`, `2026-07-02-resource-cost-map-relay.md`); cleared R12's spec cell (file never committed).
- `changelog.d\docs-260917-0-adr-convention-migration.md`: corrected an unverified claim — the retroactive fragment said superseded ADR entries were moved to `docs\architecture-decisions-archive.md`; `git show 860ee8e:docs/architecture-decisions-archive.md` shows that file was created empty (ADR-1..8 were all live), so no entries were ever archived. Fragment now states that.
- `docs\architecture-decisions.md`: recommitted with its original LF line endings — the ADR-9/ADR-10 commit had rewritten the whole file to CRLF (`git diff --stat` vs `origin/develop` was 96 lines changed instead of the real 8-line addition; matches `README.md`/`docs\TOPOLOGY.md`, both LF). No content change beyond line endings.

### Perf
- N/A — documentation only
