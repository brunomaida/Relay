---
title: "Architecture Decisions — Relay"
type: adr
solution: relay
created: 2026-09-16
---

# Architecture Decisions — Relay

Chronological log of decisions with lasting consequences on Relay's public API or hot-path shape.

---

## ADR-1: Parallel type hierarchies for typed and byte payloads; no shared base

#2026-04-24 ADR-1: W: the `unmanaged` constraint on `DispatchSink<T>` is incompatible with `ReadOnlySpan<byte>` as a type argument — a unifying design would force every consumer of the typed tree to pay for `ref struct` semantics it doesn't need D: `DispatchSink<T>` (fixed-layout struct payloads) and `PacketSink` (variable-length byte payloads) are two independent class hierarchies sharing no common base and no code beyond parallel method names (`Enqueue`, `Accept`, `IsHealthy`, `Flush`, `Dispose`); `SerializeSink<T>` bridges the two zero-copy via `MemoryMarshal.AsBytes` where a caller needs both I: parallelism costs nothing at runtime and keeps each hierarchy's JIT/inlining shape simple

## ADR-2: MPSC ring buffers use CAS-reservation tail plus padded-counter layout

#2026-04-23 ADR-2: W: a shared cache line for head/tail bookkeeping causes cache-line bouncing under multi-producer CAS contention; checking `_head` directly on every publish costs a cross-core volatile read even when the ring has ample space D: `MpscRingBuffer<T>` and `MpscByteRingBuffer` use three separately-padded 128-byte-isolated counters (`_claimedTail`, `_headCache`, `_head`); producers CAS on `_claimedTail`, write the payload, then `Volatile.Write` publish; producers read a locally cached head on the fast path and only take the cross-core `Volatile.Read` when the ring appears full I: mirrors a pattern validated in Log2's typed MPSC slot; amortizes the cross-core read cost across many publishes between refreshes

## ADR-3: `PropagateAfterAccept` is a `readonly` field, not a virtual property

#2026-04-26 ADR-3: W: a virtual property adds one vtable slot and one indirect call to the `Enqueue` hot path per invocation D: `DispatchSink<T>.PropagateAfterAccept` and `PacketSink.PropagateAfterAccept` are `readonly bool` fields set once in the constructor; `ForkSink<T>`/`ForkSink` pass `true` via the base constructor to opt into fork/audit propagation I: a `readonly` field read by a sealed type is a plain field load the JIT can constant-fold, collapsing the propagate branch entirely on the default-`false` path

## ADR-4: Hot-path structs must be a positive multiple of 64 bytes

#2026-04-24 ADR-4: W: unaligned struct sizes let adjacent ring slots share a cache line, causing false sharing between a producer write to one slot and a consumer read of the next D: any struct used as `T` in `DispatchSink<T>`/ring buffers must be a positive multiple of 64 bytes, enforced in DEBUG builds by `SinkConstraints.AssertCacheLineAligned<T>()` I: closes the same class of false-sharing bug at the payload level that the padded ring-buffer counters (ADR-2) close at the counter level, checked at DEBUG time so it costs nothing in Release

## ADR-5: `MemorySink`'s native-memory ring is the explicit last resort, not a default, with a safety-net finalizer

#2026-05-03 ADR-5: W: a managed array ring at last-resort size is large enough to be promoted to Gen2/LOH and pins GC scan time proportional to its size on every collection if not POH-allocated; native memory sidesteps GC entirely D: `MemorySink<T>` and its `PacketSink` counterpart allocate an unmanaged circular ring via `NativeMemory.AllocZeroed`, excluded from GC scanning, documented as the terminal fallback in a chain (`DrainTo(target)`, no automatic recovery, no eviction); ownership is explicit — `Dispose()` frees the buffer, and a finalizer frees the buffer as a safety net if a caller omits `Dispose()`, calling `GC.SuppressFinalize(this)` on the normal path so the finalizer never runs in correct usage I: the accepted tradeoff (manual lifetime management) is closed on its main risk — leak on `Dispose` omission — without reintroducing GC-scanned memory

## ADR-6: `Multi2Sink` CRTP variant is kept for API ergonomics despite no measured perf gain

#2026-04-24 ADR-6: W: BDN at N=2 shows `Multi2Sink` (3.31 ns) is measurably slower than the array-based `MultiSink` (3.18 ns), because JIT guarded devirtualization already devirtualizes `MultiSink`'s calls to sealed children at the call site D: `Multi2Sink<T,TC1,TC2>` is retained for compile-time type binding (a caller that wants `TC1`/`TC2` as static types, not `DispatchSink<T>[]`), not for speed; documentation states the BDN numbers explicitly and directs callers to prefer `Multi2Sink` only when compile-time binding is required I: API ergonomics is treated as a legitimate reason to keep a type independent of its benchmark result; no doc/measurement mismatch is shipped

## ADR-7: `NativeBuffer` is the single entry point for native allocation, with no relational alignment/byteCount validation

#2026-08-20 ADR-7: W: a sibling project (Wave) shipped a production memory leak by swapping `NativeMemory.AllocZeroed`'s `(elementCount, elementSize)` calloc-semantics argument order for the intended `(byteCount, alignment)` shape, multiplying a working set from an expected ~38MB to 406MB; Relay had no live instance of this bug but also no structural guard against a future call site reintroducing it D: every `NativeMemory.*` call in `src\Relay` (except `NativeBuffer.cs`'s own two internal calls) routes through `internal static unsafe class NativeBuffer`, exposing only single-purpose, single-argument-shape members (`AllocZeroedAligned`, `FreeAligned`, `AllocZeroed`, `Free`, `Clear`); each alloc/free wrapper tracks a `[ThreadStatic]` outstanding-bytes counter, asserted back to zero after `Dispose` and, via a same-thread reflection-invoked finalizer call, after the finalizer body runs — that finalizer test proves free-path routing (correct byte count, correct free family) only, not accounting reconciliation under a genuine GC-driven finalization, since a real finalizer runs on the CLR's dedicated finalizer thread and would decrement a different thread's `[ThreadStatic]` slot than the one incremented; a source-scan gate test (with positive/negative control fixtures) enforces that no call site bypasses the helper; relational validation between `byteCount` and `alignment` was considered and rejected because a small-capacity ring (e.g. `SpscRingBuffer<Payload1>(capacity: 4)`) legitimately calls `AllocZeroedAligned(byteCount: 4, alignment: 64)`, which `NativeMemory.AlignedAlloc` already handles correctly by padding I: the single-argument-shape design is the structural guard against the Wave bug class — there is no `(nuint, nuint)`-shaped member left to misuse

## ADR-8: `MemorySink` (packet) zeroes its buffer at construction as an accepted exception to ADR-7

#2026-08-20 ADR-8: W: `NativeBuffer` intentionally exposes exactly one aligned-allocation member, and that member always zeroes; adding a non-zeroing variant solely to preserve `MemorySink.Packet.cs`'s prior uninitialized-buffer behavior would add an unrequested extra API shape to a helper whose value proposition is one unambiguous member per allocation family D: `MemorySink` (the `PacketSink` variant, default capacity 4MiB) now allocates via `NativeBuffer.AllocZeroedAligned`, which zeroes the block immediately after allocation, instead of the previous non-zeroing `NativeMemory.AlignedAlloc` call — a real, observable behavior change, not a no-op refactor I: `MemorySink` (packet) is a terminal last-resort fallback (ADR-5) whose first writes happen during a failure event on some other sink, so zeroing 4MiB at construction pre-faults those pages off the failure path; construction is treated as one-time initialization, not steady-state hot path (`Enqueue`/`Accept`/`TryPublish`/`TryConsume`), so it is accepted as an explicit, documented exception to the repo's hot-path-only tier declaration rather than silent drift
