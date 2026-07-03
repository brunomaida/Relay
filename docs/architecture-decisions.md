---
title: "Architecture Decisions — Relay"
type: adr
solution: Relay
created: 2026-07-02
---

# Architecture Decisions — Relay

Chronological log of decisions with lasting consequences on Relay's public API or hot-path shape. Each entry states the decision, the alternative considered, and why the alternative was rejected. Grounded in `CLAUDE.md`, `docs/TOPOLOGY.md`, and the commit history cited per entry — no speculative entries.

## 2026-04-24 — Parallel type hierarchies for typed and byte payloads; no shared base

**Decision:** `DispatchSink<T>` (`T : unmanaged`, fixed-layout struct payloads) and `PacketSink` (`ReadOnlySpan<byte>`, variable-length payloads) are two independent class hierarchies. They share no common base type and no code beyond parallel method names (`Enqueue`, `Accept`, `IsHealthy`, `Flush`, `Dispose`).

**Alternative considered:** a single generic hierarchy parameterized to cover both cases.

**Why rejected:** the `unmanaged` constraint on `DispatchSink<T>` is incompatible with `ReadOnlySpan<byte>` as a type argument (`ref struct` cannot satisfy a generic type parameter in the general case, and a unifying design would force every consumer of the typed tree to pay for `ref struct` semantics they don't need). `SerializeSink<T>` bridges the two trees zero-copy via `MemoryMarshal.AsBytes` where a caller needs both. Parallelism costs nothing at runtime and keeps each hierarchy's JIT/inlining shape simple. (Introduced in `43911f0`, "rename ByteSink to PacketSink, update derived class bases".)

## 2026-04-23 — MPSC ring buffers: CAS-reservation tail + padded-counter layout ("Log2 FIX #18")

**Decision:** both `MpscRingBuffer<T>` (typed) and `MpscByteRingBuffer` (byte) use three separately-padded 128-byte-isolated counters (`_claimedTail`, `_headCache`, `_head`) and a producer sequence of `Interlocked.CompareExchange` on `_claimedTail`, payload write, then `Volatile.Write` publish. Producers read a locally cached head (`_headCache`) on the fast path and only take the cross-core `Volatile.Read` of `_head` when the ring appears full.

**Alternative considered:** a single shared cache line for head/tail bookkeeping (simpler, smaller memory footprint), or checking `_head` directly on every publish.

**Why rejected:** a shared cache line under multi-producer CAS contention causes cache-line bouncing, which the padding eliminates. Checking `_head` directly on every publish costs a cross-core volatile read even when the ring has ample space; `_headCache` amortizes that cost across many publishes between refreshes. This layout mirrors a pattern validated in the Log2 project's typed MPSC slot ("FIX #18" — see `CLAUDE.md` § MPSC ring buffers, `docs/TOPOLOGY.md`). (Introduced in `f47c370` / `31edb5e`.)

## 2026-04-26 — `PropagateAfterAccept` is a `readonly` field, not a virtual property

**Decision:** `DispatchSink<T>.PropagateAfterAccept` and `PacketSink.PropagateAfterAccept` are `readonly bool` fields set once in the constructor, not virtual properties overridden per subclass.

**Alternative considered:** a virtual property (`protected virtual bool PropagateAfterAccept => false;`), which reads more naturally as "subclass declares its behavior."

**Why rejected:** a virtual property adds one vtable slot and one indirect call to the `Enqueue` hot path per invocation. A `readonly` field read by a sealed type is a plain field load the JIT can constant-fold at JIT time when it can prove the value — collapsing the propagate branch entirely on the default-`false` path. `ForkSink<T>` / `ForkSink` (non-generic) pass `true` via the base constructor to opt into fork/audit propagation. (Introduced in `66a4692`, "convert PropagateAfterAccept virtual property to readonly field".)

## 2026-04-24 — Hot-path structs must be a positive multiple of 64 bytes

**Decision:** any struct used as `T` in `DispatchSink<T>` / ring buffers must be sized as a positive multiple of 64 bytes (64, 128, 192, ...), enforced in DEBUG builds by `SinkConstraints.AssertCacheLineAligned<T>()`.

**Alternative considered:** no enforced alignment; let callers size structs freely and accept whatever ring-slot packing results.

**Why rejected:** unaligned struct sizes let adjacent ring slots share a cache line, causing false sharing between producer writes to one slot and consumer reads of the next — the exact class of bug the padded ring-buffer counters (see 2026-04-23 entry) exist to avoid at the counter level. Enforcing the invariant at the slot level closes the same class of bug for payload data, checked at DEBUG time so it costs nothing in Release. (Introduced in `43824dc`, "rename Pipe to Sink throughout", which added `SinkConstraints.cs`.)

## 2026-05-03 — `MemorySink<T>` / `MemorySink` (packet): native-memory ring is the explicit last resort, not a default

**Decision:** `MemorySink<T>` and its `PacketSink` counterpart allocate an unmanaged circular ring via `NativeMemory.AllocZeroed`, are excluded from GC scanning, and are documented as the terminal fallback in a chain (`DrainTo(target)` — no automatic recovery, no eviction). Ownership is explicit: `Dispose()` frees the buffer; as of 2026-06-11 (`c1818fa`) a finalizer (`~MemorySink()`) was added as a safety net that frees the buffer if a caller omits `Dispose()`, calling `GC.SuppressFinalize(this)` on the normal `Dispose()` path so the finalizer never runs in the correct-usage case.

**Alternative considered:** rely on a managed `T[]`/`byte[]` ring (simpler, no manual memory management) as the last-resort sink.

**Why rejected:** a managed array ring at "last resort" size (default `1 << 23` entries) is large enough to be promoted to Gen2/LOH and pins GC scan time proportional to its size on every collection if not POH-allocated; native memory sidesteps GC entirely. The tradeoff — manual lifetime management — was accepted, and the 2026-06-11 finalizer closes the main risk (leak on `Dispose` omission) without reintroducing GC-scanned memory. (Introduced in `a3b19f9`, "rename RamSink→MemorySink, add [Obsolete] compat shims"; finalizer added in `c1818fa`.)

## 2026-04-24 — `Multi2Sink<T,TC1,TC2>` / `Multi2PacketSink<TC1,TC2>`: CRTP variant kept despite no measured perf gain

**Decision:** a fixed-arity, compile-time-typed 2-child broadcast variant (`Multi2Sink<T,TC1,TC2>`) exists alongside the array-based `MultiSink<T>`, and is documented as **not** a performance optimization: BDN at N=2 shows `Multi2Sink` (3.31 ns) is measurably slower than `MultiSink` (3.18 ns), because JIT guarded-devirtualization (GDV) already devirtualizes the array-based sink's calls to sealed children at the call site.

**Alternative considered:** drop `Multi2Sink` entirely once BDN showed no advantage, or keep it silently documented as "the fast path."

**Why rejected (dropping):** `Multi2Sink` is retained for compile-time type binding (a caller that wants `TC1`/`TC2` as static types, not `DispatchSink<T>[]`), not for speed — API ergonomics is a legitimate reason to keep a type independent of its benchmark result. **Why rejected (silent claim):** shipping a documented "~6c savings" claim that BDN contradicts would be a doc/measurement mismatch (flagged as a WARN-severity offender in `docs/reports/*-resource-cost-map-relay.md` prior to correction); `CLAUDE.md` § `MultiSink<T>` semantics now states the BDN numbers explicitly and directs callers to prefer `Multi2Sink` only when compile-time binding is required, not for expected performance gains.
