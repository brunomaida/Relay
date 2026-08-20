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

## 2026-08-20 — `NativeBuffer`: single entry point for native allocation; no relational alignment/byteCount validation

**Decision:** every `NativeMemory.*` call in `src\Relay` (except the two internal calls inside `NativeBuffer.cs` itself) routes through `internal static unsafe class NativeBuffer` (`Relay.Memory`), which exposes only single-purpose, single-argument-shape members: `AllocZeroedAligned(nuint byteCount, int alignment = 64)` / `FreeAligned(void* ptr, nuint byteCount)` for the aligned family, `AllocZeroed(nuint byteCount)` / `Free(void* ptr, nuint byteCount)` for the unaligned family, and `Clear(void* ptr, nuint byteCount)` for post-construction zeroing of already-owned memory (used by `MpscRingBuffer.Reset()`). Each alloc/free wrapper increments/decrements a `[ThreadStatic]` outstanding-bytes counter (`AlignedBytesOutstanding` / `UnalignedBytesOutstanding`), which the new footprint/accounting tests assert returns to zero after `Dispose` — and, via a same-thread reflection-invoked finalizer call, after the finalizer body runs on the allocating thread. That finalizer test proves free-path routing (correct byte count, correct free family), not accounting reconciliation under genuine GC-driven finalization: because the counters are `[ThreadStatic]`, a real finalizer runs on the CLR's dedicated finalizer thread, so it would decrement a different thread's slot than the one incremented, and the counters are not expected to reconcile in that case. Four production call sites (`SpscRingBuffer<T>`, `MpscRingBuffer<T>`, `MemorySink<T>`, `MemorySink` packet variant) plus `MpscRingBuffer.Reset()`'s `Clear` call were migrated. A source-scan gate test (`NativeAllocationGateTests.NativeMemory_IsNotCalledOutsideNativeBuffer`, with positive-control fixtures proving the detector actually fires and negative-control fixtures proving it doesn't false-positive on `<see cref="NativeMemory..."/>` doc references) makes the "single entry point" claim self-enforcing rather than a convention that silently rots.

**Bug class this guards against:** a sibling project (Wave, `Wave.Definitions.SpscRingBuffer<T>`) shipped a production memory leak by calling `NativeMemory.AllocZeroed(bufferBytes, (nuint)sizeof(T))` — the two-argument overload is `(elementCount, elementSize)` (`calloc` semantics), not `(bytes, alignment)`. Since `bufferBytes` was already `capacity * sizeof(T)`, the real allocation became `capacity * sizeof(T)²`. Measured in production: 406MB→38MB and 294MB→33MB after a 2-line fix (Wave issue #136). Relay had no live instance of this bug — every existing call site already used a correct 1-arg `AllocZeroed(byteCount)` or 2-arg `AlignedAlloc(byteCount, alignment)` shape — but had no structural guard against a future call site reintroducing it. `NativeBuffer`'s single-argument-shape API makes the swapped-argument call not compile (there is no `(nuint, nuint)`-shaped member left to misuse), and the source-scan gate turns "a new call site bypasses the helper" into a test failure instead of a silent regression.

**Alternative considered:** add relational validation inside `AllocZeroedAligned` (e.g. `byteCount >= alignment` or `byteCount % alignment == 0`) as an additional safety net against misuse.

**Why rejected:** `SpscRingBuffer<Payload1>(capacity: 4)` legitimately calls `AllocZeroedAligned(byteCount: 4, alignment: 64)` — a byte count smaller than, and not a multiple of, the alignment. `NativeMemory.AlignedAlloc` handles this correctly (it just pads), so any relational check between `byteCount` and `alignment` would reject a legitimate, already-shipping call shape from a small-capacity ring. The single-argument-shape design is the actual guard against the Wave bug class; relational validation would be redundant against that bug and actively wrong against `Payload1`-sized rings.

### Recorded exception: `MemorySink` (packet) now zeroes its buffer at construction

**What changed:** `MemorySink` (the `PacketSink` variant, `src\Relay\Sinks\MemorySink.Packet.cs`, default capacity 4 MiB) previously allocated via `NativeMemory.AlignedAlloc` directly, which never zeroes. It now allocates via `NativeBuffer.AllocZeroedAligned`, which zeroes the block immediately after allocation (`NativeMemory.Clear`). This is a real, observable behavior change — the buffer's initial contents changed from uninitialized garbage to zero — not a no-op refactor.

**Why it happened:** `NativeBuffer` intentionally exposes exactly one aligned-allocation member, and that member always zeroes. A non-zeroing aligned variant, kept solely to preserve `MemorySink.Packet.cs`'s prior behavior, would have added an unrequested extra API shape to a helper whose entire value proposition is "one unambiguous member per allocation family" — the opposite of what this task set out to build. Zeroing was already `MemorySink<T>`'s (unaligned family, `NativeMemory.AllocZeroed`) behavior; making the aligned family consistent with it was a side effect of collapsing both call sites onto `NativeBuffer`, not a requested feature.

**Why it's acceptable:** `MemorySink` (packet) is the terminal last-resort fallback in a chain (see the 2026-05-03 entry above) — its first writes happen during a failure event on some other sink. Zeroing 4 MiB at construction pre-faults those pages off the failure path instead of on it, which is beneficial for this sink's role, not merely harmless.

**Tension with `CLAUDE.md`, and the ruling:** `Libs\Relay\CLAUDE.md` declares `tier: ultra-low-latency — all .cs files are hot-path; no cold-path exceptions`, read literally that would forbid any construction-time cost added for correctness elsewhere. The controller ruling is to **accept** this zeroing as-is: construction is one-time initialization, not steady-state hot path (`Enqueue` / `Accept` / `TryPublish` / `TryConsume`), and the "no cold-path exceptions" rule's intent — per repo convention — targets per-message/per-tick code, not object construction. This entry is the record of that interpretation; it is an explicit exception, not silent drift.
