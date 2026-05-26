---
slug: 260526-receiver-desync-stall
type: fix
---

### Summary
- `TcpReceiver<TState>.Poll` / `NamedPipeReceiver<TState>.Poll`: on invalid frame length (≤0 or > buffer), tear down the underlying stream/pipe and throw `InvalidDataException` instead of silently leaving the wire mid-frame. Subsequent `Poll()` returns `false`; the caller restarts the session.
- `SharedMemorySpscReceiver<TState>` ctor: validate `SHM_MAGIC` (`0x4C473200`) at header offset 0; throw `InvalidDataException` on mismatch instead of accepting a foreign/uninitialised MMF.
- `SharedMemorySpscReceiver<TState>.Poll`: throw `InvalidDataException` on invalid frame length instead of returning `false` without advancing `_readIndex` — the latter caused an infinite re-read stall.
- `TcpReceiver<TState>` XML doc: removed misleading "non-blocking poll" wording; documents non-blocking at frame boundaries + possible mid-frame block on TCP segmentation (matches `NamedPipeReceiver` management-plane wording). No behavioural change.
- `NamedPipeReceiver<TState>`: removed POH-pinned 4-byte `_header` field; `Poll` uses `Span<byte> header = stackalloc byte[4]` instead, mirroring `TcpReceiver`. One less per-instance pinned object.

### Audit
Findings F0, F0b, F1, F3 from `docs/reports/2026-05-26-hot-path-audit-receivers.md` (hot-path-audit, post v1.0.2). F0/F0b classified `HIGH — correctness`; F1 `HIGH — doc`; F3 `MEDIUM — alloc`. Regression tests in `tests/Relay.Tests/Receivers/ReceiverDesyncStallTests.cs` cover negative-length, oversized-length, and wrong-magic cases for all three receivers.

### Perf
Hot path unchanged on the happy path — added branches are predicted-not-taken and outside the inlined fast path. BDN baseline (`benchmarks/Relay.Benchmarks/Receivers/`) preserved: `SharedMemorySpscReceiver.Poll_Empty` ~0.66 ns, `Roundtrip_PerFrame` 20.7 ns @ 64 B / 38.4 ns @ 1 KiB / 0 B allocations.
