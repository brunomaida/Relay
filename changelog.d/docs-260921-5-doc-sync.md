### Docs
- `README.md`: removed the instruction to append an AI-attribution suffix to commit messages; added `UnixSocketSink`, `SharedMemorySpscSink` (with its `[Obsolete]` `SharedMemorySink` alias) and `Memory/NativeBuffer` to the source-layout table, type hierarchy and concrete-sink list.
- `docs\TOPOLOGY.md`: added `UnixSocketSink` and `SharedMemorySpscSink` to the `PacketSink` hierarchy, namespace list, concrete-sink detail and threading tables (the type previously listed there, `SharedMemorySink`, is an `[Obsolete]` shim).
- `docs\architecture-decisions.md`: added ADR-9 (SHM wire protocol, single producer) and ADR-10 (Http/Observability assembly split), retroactive and citing sources; unrecorded rationale is marked as such.
- `docs\roadmap.md`: repointed S14, S20, S22 to files that exist (`2026-07-02-hot-path-audit-relay.md`, `changelog.d\archived\1.0.5\260611-doc-taxonomy.md`, `2026-07-02-resource-cost-map-relay.md`); cleared S12's spec cell (file never committed).

### Perf
- N/A — documentation only
