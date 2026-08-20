# Plan: Relay Hot-Path Cost Map + Robustness Audit

## Context

User requested evaluation of performance improvements via `/resource-cost-mapping` combined
with the BDN baseline (`bench_history.md`, commit `ddf2f0b`, i7-12700 / .NET 9.0.14).
Also: robustness assessment under high load (memory leaks, failure modes, crash paths).

All source files have been read. Fable advisor review completed. Two FileStreamSink crash paths
discovered during analysis; those are the primary actionable findings alongside the cost map.

## Deliverables

1. **`docs/reports/2026-06-10-resource-cost-map-relay.md`** — library-mode cost map per skill
   output contract (§0–§8; no §9 Delta — no prior cost-map snapshot exists).
2. **`docs/reports/resource-cost-map.json`** — sibling JSON with full node set + schema version
   (required by the skill contract for CI gates).
3. **Chat summary (PT-BR)** — robustness findings with `file:line` refs, actionable next steps.
   NOT a separate document; only the cost map goes to disk per skill contract.

## Key Findings (pre-approved by Fable review)

### FileStreamSink — two crash paths converting transient disk failure into permanent sink death

**Path 1: buffer-full → IndexOutOfRangeException → consumer thread dies**
- `FileStreamSink.WriteToBackend` (FileStreamSink.cs:52–63): when `_bufferPos == _writeBuffer.Length`,
  calls `FlushBuffer()`. `FlushBuffer` catches `IOException` but does NOT reset `_bufferPos`
  (FileStreamSink.cs:99–105). Control returns → `ref _writeBuffer[_bufferPos]` where
  `_bufferPos == Length` → `IndexOutOfRangeException` → escapes ConsumeLoop catch →
  consumer thread dies permanently.
- **TcpSink**: immune (double bounds check: WriteToBackend:73–79 checks again after flush).
- **MmfSink**: immune (direct pointer write, no buffer flush path).

**Path 2: disposed stream → ObjectDisposedException → consumer thread dies**
- `TryRecoverBackend` (FileStreamSink.cs:76–78) disposes `_stream` then calls `OpenStream()`.
  If `OpenStream()` throws, `_stream` still references the disposed instance (catch at :82
  does not null it out). Next flush deadline: `FlushBackend` (FileStreamSink.cs:65–68) has
  no `_healthy` gate → `_stream!.Write(...)` on disposed stream → `ObjectDisposedException`
  (not caught by `catch (IOException)` at FlushBuffer:101) → consumer dies.
- **TcpSink**: immune (`catch (Exception)` in both FlushBuffer:136–140 and TryRecoverBackend:106–110).

### MemorySink — no finalizer, native memory leak

`NativeMemory.AllocZeroed` in constructor (MemorySink.cs:40). `NativeMemory.Free` only in
`Dispose` (MemorySink.cs:76). No finalizer → leak if `Dispose` not called. Since `MemorySink`
is not managed via `SpscQueueSink<T>.Start/Stop` chain, caller must ensure disposal.

Also: `_head`/`_tail` are plain `long` fields (MemorySink.cs:29–30). Remarks at :16–17 claim
"volatile reads/writes" — code has no volatile. Safe under SPSC invariant but comment is wrong.

### TryDrainToPrev — one-item loss on health flip

`SpscQueueSink.TryDrainToPrev` (SpscQueueSink.cs:270–274): `TryConsume(out var item)` pops
the item before `!Prev.IsHealthy` check at :272. On break, that item is dropped. Bounded:
max 1 item per health flip, shutdown-only path. Documented behavior but worth naming precisely.

### RotatingFileSink — per-record RDTSC in ShouldRotate

`ShouldRotate` (RotatingFileSink.cs:122–127) calls `HfClock.NowTicks` on every record.
BDN: `ShouldRotate_Predicate` = 13.38 ns (vs `ShouldRotate_HotPath` = 34.18 ns total).
The RDTSC dominates the predicate cost. Fix: throttle the clock check (e.g., every 256 records
via a counter) since day-boundary precision ≤ 1s. Would drop predicate from ~13 ns to ~0.5 ns.
Note: BDN benchmark was added as regression gate for the `DateTime.UtcNow` removal — the fix
landed but left per-record RDTSC, which remains 13 ns.

### Multi2Sink CRTP — BDN shows no advantage vs MultiSink

`Multi2Sink.Enqueue` BDN = 3.31 ns vs `MultiSink.Enqueue` = 3.18 ns (2-child case).
CLAUDE.md documents "~6c saving" — not reproduced in baseline. JIT likely already applies GDV
devirtualization at call site for sealed types. CRTP adds complexity without measured payoff
at N=2. Reported as doc-vs-measurement mismatch.

### PacketSink._dropCount — Interlocked on unpadded field

`PacketSink._dropCount` (PacketSink.cs:41) is a plain `long` with no PaddedLong isolation.
`Interlocked.Increment` = ~25c uncontended (BDN measured: 3.77 ns = ~13c total drop path).
Under monitoring from a second thread (reading `DropCount` via `Volatile.Read`), false sharing
with adjacent fields `Next` and `PropagateAfterAccept` on the same 64-byte line. Padding would
eliminate the bounce but is only observable under concurrent monitoring.

### MpscByteRingBuffer — non-inlineable recursion in TryPeek

Padding-skip on wrap uses recursion (`TryPeek` calling itself). JIT cannot inline recursive
methods → extra stack frame + call overhead on every wrapped record. Impact: one extra
indirect call per wrapped record; at high throughput with large payloads, wrap frequency ≈ 1/N.

### MpscRingBuffer.TryConsume — two Volatile.Write per slot

`TryConsume` (MpscRingBuffer.cs:137–138): writes `Volatile.Write(ref PublishedAt, 0)` + 
`Volatile.Write(ref _head.Value, pos + 1)` = 2 mfences per consumed item. Unavoidable for
MPSC correctness; `TryConsumeBatch` (MpscRingBuffer.cs:164–165) amortizes to 1 head write
for N items (N-1 savings), but per-slot Published clear is still per-item.

## CPU Reference

All ns→cycles conversions use **3.5 GHz** (CLAUDE.md convention). i7-12700 P-core boost = 4.9 GHz;
cycles understate by ≤40% on boosted cores. Header will note this.

## Cost Map Structure

Library mode: §0 Header → §1 Per-Entry Cost Table → §2 Top Offenders → §3 Hot Tree → 
§4 Allocation Map → §5 Anti-Pattern Offenders → §6 Cache-Line Report → §7 Syscalls →
§8 Blind Subgraphs. No §9 (no prior snapshot). Target ≤400 lines.

## Model Routing

| Etapa | Modelo | Justificativa |
|---|---|---|
| Geração do cost map + chat summary | **Sonnet** | Escrita de report e análise estruturada |
| Performance analysis / scope review | **Opus** | Relay CLAUDE.md: Opus para análise de performance |
| Adversarial review (Fable) | ✅ Concluído via `advisor()` nesta sessão | Frontier reservado para review final |

## Verification

- Open `docs/reports/2026-06-10-resource-cost-map-relay.md` and confirm:
  - Every row has `file:line`
  - §5 lists FileStreamSink IndexOutOfRangeException + ObjectDisposedException paths
  - §8 lists `FilterSink._predicate` body and TCP handshake as blind subgraphs
  - Tier column uses `declared-hot` not BDN-derived rates (library mode)
- Confirm `docs/reports/resource-cost-map.json` exists with `schemaVersion` field.
- No existing source files modified.
