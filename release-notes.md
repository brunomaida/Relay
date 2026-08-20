## Added
- `Relay.Memory.NativeBuffer` (internal): the single sanctioned entry point for `NativeMemory.*` under `src/Relay` — `AllocZeroedAligned`/`FreeAligned` (aligned family), `AllocZeroed`/`Free` (unaligned family), `Clear`, plus `[ThreadStatic]` outstanding-bytes counters. Guards against the `NativeMemory.AllocZeroed`/`Alloc` 2-arg overload confusion that shipped a production leak in a sibling project (406MB→38MB after fix).
- Native-allocation footprint tests: oracle-based (`_msize`/`_aligned_msize` via `ucrtbase.dll`, Windows-only) assertions that ring buffers and memory sinks allocate exactly the expected byte count, plus a regression test for the exact overload-swap bug shape.
- Allocation-accounting tests: `NativeBuffer`'s outstanding-bytes counters return to zero after `Dispose()` and after `MemorySink<T>` finalizer free-path.
- Self-proving source-scan gate: fails if any file other than `NativeBuffer.cs` calls `NativeMemory.*` directly, with positive/negative control fixtures proving the detector fires correctly.

## Changed
- All 4 production `NativeMemory.*` call sites (`SpscRingBuffer<T>`, `MpscRingBuffer<T>`, `MemorySink<T>`, `MemorySink` packet) now route through `NativeBuffer`. Behavior-preserving except `MemorySink` (packet), which now zeroes its buffer at construction (previously did not) — a deliberate exception, documented in `docs/architecture-decisions.md`, since construction is one-time init and this sink's first writes happen during a failure event.

## Fixed
- `Probe_Calibration_ReportsSaneSizeForKnownAllocation` asserted the Windows-only `ucrtbase.dll` oracle always calibrates, failing on non-Windows CI. Gated to Windows-only.

## Docs
- CLAUDE.md/TOPOLOGY.md hygiene pass: pruned drifted Project Layout tree, stale Phase-1 status, duplicated commit-gate/model-routing lines; fixed a false provenance tag on a BDN claim.
- `docs/architecture-decisions.md`, `docs/bench-methodology.md`: new entries for the `NativeBuffer` bug class and why footprint coverage lives in tests instead of `[MemoryDiagnoser]`.

## Performance
- N/A — all migrated call sites are constructor/`Reset()`/dispose-path only, never on the `Enqueue`→`Accept` hot path.
