# Relay v1.0.3 — 2026-05-26

Feature release. Receiver hierarchy (UDP / TCP / NamedPipe / SharedMemorySpsc) + hot-path-audit hardening (F0 / F0b / F1 / F3 closed).

---

## Added

- **`PacketCallback<TState>`** delegate and **`PacketReceiver`** abstract base — zero-alloc span callback (sidesteps `Action<ReadOnlySpan<byte>>` restriction); passive caller-driven `Poll()` loop with optional `Next: PacketSink?` forward chain.
- **`UdpReceiver<TState>`** (hot path) — non-blocking `Poll()` via `Socket.Poll(0, SelectRead)`; `stackalloc byte[1432]` per call (MTU-safe, zero GC); configurable kernel RX buffer (default 1 MB).
- **`TcpReceiver<TState>`** (management-plane) — POH-pinned buffer; wire format `[4B BE length][payload]` matching `TcpSink`/`NamedPipeSink`; non-blocking at frame boundaries.
- **`SharedMemorySpscReceiver<TState>`** (hot path, Windows-only) — SPSC ring consumer matching `SharedMemorySpscSink` wire format; `Volatile.Read/Write` on indices; `SHM_MAGIC` validation on construction.
- **`NamedPipeReceiver<TState>`** (management-plane) + **`RelayBuilder.From<TState>`** / `FromTcp` / `FromSharedMemory` / `FromNamedPipe` factory methods.

## Fixed

- **`TcpReceiver.Poll` / `NamedPipeReceiver.Poll`**: invalid frame length (≤0 or > buffer) now tears down the stream/pipe and throws `InvalidDataException`; subsequent `Poll()` returns `false` (Audit F0).
- **`SharedMemorySpscReceiver` ctor**: validates `SHM_MAGIC` (`0x4C473200`); throws `InvalidDataException` on mismatch instead of accepting a foreign MMF (Audit F0b).
- **`SharedMemorySpscReceiver.Poll`**: invalid frame length now throws `InvalidDataException` instead of returning `false` without advancing `_readIndex`, which caused an infinite re-read stall (Audit F0b).
- **`TcpReceiver` XML doc**: removed misleading "non-blocking poll" wording; correctly documents non-blocking at frame boundaries with possible mid-frame block on TCP segmentation (Audit F1).
- **`NamedPipeReceiver`**: removed POH-pinned `_header` field; `Poll` uses `stackalloc byte[4]` instead — one fewer pinned object per instance (Audit F3).

## Performance

- Hot-path audit (27 dimensions) — PASS: zero allocations on hot path; all correctness bugs (F0/F0b/F1/F3) closed in this release. Reports: `docs/reports/2026-05-26-hot-path-audit-receivers.md`, `docs/reports/2026-05-26-resource-cost-map-receivers.md`.
- First BDN harness for receivers (`benchmarks/Relay.Benchmarks/Receivers/`). Intel i7-12700 / .NET 9.0.14: `SharedMemorySpscReceiver.Poll_Empty` **0.66 ns** / 0 B; roundtrip **20.7 ns** @ 64 B, **38.4 ns** @ 1 KiB / 0 B; `UdpReceiver` loopback roundtrip 128 B: **4.69 µs** / 0 B receiver path.

---

## Packages

| Package | Description |
|---------|-------------|
| `Relay` | Core pipeline — typed + packet sinks, receivers, builders, ring buffers, native memory |
| `Relay.Sinks.Http` | `HttpBatchSink` — HTTP POST with circuit breaker |
| `Relay.Sinks.Observability` | `SeqSink` — CLEF-over-HTTP to Seq |

**Runtime:** .NET 9 · **Language:** C# 13 · **Zero production dependencies** (core `Relay` package)
