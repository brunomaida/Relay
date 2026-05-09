# Relay v1.0.0 — 2026-05-09

First stable release. Composable fallback dispatch pipeline for `unmanaged` structs — .NET 9 / C# 13.
Zero allocation on the hot path. No locks. No LINQ. No `async`.

---

## Added

- **Core typed pipeline** — `DispatchSink<T>`, `SpscQueueSink<T>`, `MpscQueueSink<T>`: SPSC/MPSC lock-free ring delivery with consumer thread, flush cadence, and `Prev`-based recovery drain.
- **Packet hierarchy** — `PacketSink`, `SpscQueueSink`, `MpscQueueSink` (non-generic): parallel tree for variable-length `ReadOnlySpan<byte>` payloads; zero-copy peek via sentinel padding marker.
- **Broadcast** — `MultiSink<T>`, `Multi2Sink<T,TC1,TC2>` (CRTP), `Multi2PacketSink<TC1,TC2>`: OR-health broadcast; CRTP variants devirtualize both `Enqueue` calls at JIT time (~6c savings typed, ~1.23× overhead packet vs array-based).
- **Fork / Filter** — `ForkSink<T>` (`PropagateAfterAccept`), `FilterSink<T>` (silent-consume on predicate fail, does not trigger fallback).
- **Concrete sinks** — `FileStreamSink<T>` (POH buffer, backoff 1s→60s), `TcpSink<T>` (POH buffer, backoff 1s→30s), `MmfSink<T>` (capacity-only), `MemorySink<T>` (NativeMemory circular ring, drain-on-recovery). `RamSink<T>` retained as `[Obsolete]` compat shim — planned removal in 2.0.
- **Fluent builder** — `RelayBuilder.Start/StartSpsc/StartMpsc` → `.To()` / `.Fork()` / `.When(pred).To()` / `.Multi(cfg)` / `.Multi<TC1,TC2>(c1,c2)`. Auto-wires `Next` and `Prev`.

## Performance

- **SPSC ring fast path**: `TryPublish`/`TryConsume` ~25c → ~8–12c via `_cachedHead`/`_cachedTail` snapshots (128B-padded `PaddedLong`); bounds-check elimination via `Unsafe.Add` + `MemoryMarshal.GetArrayDataReference`.
- **MPSC ring (Log2 FIX #18)**: three isolated 128B cache lines (`_claimedTail`, `_headCache`, `_head`) eliminate false sharing under multi-producer contention; HeadCache avoids cross-core volatile read on fast path.
- **`RotatingFileSink.ShouldRotate`**: `21.84 ns → 13.76 ns` — removed `DateTime.UtcNow.Date` per consumed payload; caches UTC-midnight boundary in `HfClock` ticks, resamples wall-clock only inside `RotateNow` (cold path).
- **Flush deadline throttling**: `HfClock.NowTicks` checked every 8 spin iterations (mask `0x7`) instead of every iteration — amortizes QPC cost (~25c + LFENCE stall) across batches.

## Fixed

- `MpscQueueSink<T>`: closed consumer drain race under H26 stress conditions; added stress suite coverage.
- `FileStreamSink<T>`: fixed recovery race in stream-reopen path.
- `SpscRingBuffer<T>`: added batched-write API (`TryReserveTail`/`WriteSlot`/`CommitTail`) for single-mfence N-slot publish.
- `SinkChain.To()`: extended `Prev` wiring to `MpscQueueSink<T>` (previously SPSC-only).

---

## Packages

| Package | Description |
|---------|-------------|
| `Relay` | Core pipeline — typed + packet sinks, builders, ring buffers, native memory |
| `Relay.Sinks.Http` | `HttpBatchSink` — HTTP POST with circuit breaker |
| `Relay.Sinks.Observability` | `SeqSink` — CLEF-over-HTTP to Seq |

**Runtime:** .NET 9 · **Language:** C# 13 · **Zero production dependencies** (core `Relay` package)
