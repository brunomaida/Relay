# Role
Relay — infrastructure library for composable fallback dispatch pipelines over `T : unmanaged`.
Single responsibility: receive `T`, deliver to the configured backend, and if delivery fails, forward to the next sink in the chain. No logging. No telemetry. No orchestration.

> Global tenets (HFT, code format, response format, base git workflow) defined in `~/.claude/CLAUDE.md`.

# Stack
- Runtime: **.NET 9.0** (`net9.0`)
- Language: **C# 13** (`<LangVersion>13</LangVersion>`)
- `ImplicitUsings`: **disabled** — every `using` directive must be explicit
- `AllowUnsafeBlocks`: **enabled** — unsafe required for native memory and `Unsafe.CopyBlockUnaligned`
- `Nullable`: **enabled** — no `#nullable` suppressions without justification
- Central package management: `Directory.Packages.props`
- **Zero external production dependencies.** Test project uses `xunit 2.9.2` and `FluentAssertions 6.12.1` — version in `Directory.Packages.props`; never pin ad-hoc in `.csproj`
- Adding any production dependency requires explicit justification in a commit message

# Project Layout

```
Relay.sln
Directory.Packages.props
src/
  Relay/
    Relay.csproj
    DispatchSink.cs          ← abstract base (typed), Enqueue hot path
    PacketSink.cs            ← abstract base (byte payloads), parallel hierarchy
    SpscQueueSink.cs         ← async delivery via SPSC ring + consumer thread
    SpscQueueSink.Packet.cs  ← byte-payload SPSC consumer (non-generic)
    MpscQueueSink.cs         ← async delivery via MPSC ring (multi-producer)
    MpscQueueSink.Packet.cs  ← byte-payload MPSC consumer (non-generic)
    ForkSink.cs              ← primary + Next propagation (audit/bypass pattern)
    MultiSink.cs             ← broadcast (array-based + Multi2Sink CRTP variant)
    FilterSink.cs            ← conditional gate
    NullSink.cs              ← no-op sink / terminal fallback
    NullSink.Packet.cs       ← byte-payload no-op sink (non-generic)
    Buffers/
      SpscRingBuffer.cs      ← lock-free SPSC ring, 128B padded head/tail
      SpscByteRingBuffer.cs  ← byte-variant length-prefixed ring
      MpscRingBuffer.cs      ← MPSC ring: CAS tail + HeadCache + inline Slot (Log2 FIX #18)
      MpscByteRingBuffer.cs  ← byte-variant MPSC: CAS reservation + header publish-bit
    Sinks/
      FileStreamSink.cs      ← binary write to FileStream, POH buffer, backoff recovery
      MmfSink.cs             ← MemoryMappedFile, capacity-only failure
      TcpSink.cs             ← TCP socket, POH send buffer, backoff reconnect
      RamSink.cs             ← native memory circular ring, last-resort fallback
    Builder/
      RelayBuilder.cs        ← static entry points: Start / StartSpsc / StartMpsc
      SinkChain.cs           ← fluent chain builder; To / Fork / When / Multi; wires Next + Prev
      MultiBuilder.cs        ← sub-builder: collects broadcast branches for MultiSink
      FilterBinding.cs       ← intermediate state: closes When(pred) with .To(downstream)
    Memory/
      RelayMemory.cs         ← internal: PreFault + VirtualLock on ring buffer
    Internal/
      SinkConstraints.cs     ← internal: cache-line alignment assertion (DEBUG)
      HfClock.cs             ← internal: Stopwatch.GetTimestamp() wrapper
```

# Namespaces
| Namespace | Content |
|---|---|
| `Relay` | `DispatchSink<T>`, `SpscQueueSink<T>`, `MpscQueueSink<T>`, `MultiSink<T>`, `Multi2Sink<T,TC1,TC2>`, `FilterSink<T>`, `NullSink<T>`, `ForkSink<T>`, `PacketSink`, `SpscQueueSink`, `MpscQueueSink`, `NullSink` |
| `Relay.Buffers` | `SpscRingBuffer<T>`, `MpscRingBuffer<T>`, `SpscByteRingBuffer`, `MpscByteRingBuffer` (internal to lib) |
| `Relay.Sinks` | Concrete backends: `FileStreamSink<T>`, `MmfSink<T>`, `TcpSink<T>`, `RamSink<T>` |
| `Relay.Builder` | `RelayBuilder`, `SinkChain<T,THead>`, `MultiBuilder<T>`, `FilterBinding<T,THead>` |
| `Relay.Memory` | `RelayMemory` (internal) |
| `Relay.Internal` | `HfClock`, `SinkConstraints` (internal) |

# Performance — Everything Is Hot Path

**All code on the `Enqueue → Accept` path is hot path.** `Enqueue` is called at tick-rate frequency.

## Invariants
- **Zero allocations** in steady state. `GC.AllocateArray(pinned: true)` only in constructors.
- **No LINQ.** Not on any path.
- **No `lock` / `Monitor`.** Volatile + Interlocked only.
- **No `async`/`await`** in the dispatch or consume path.
- **No `DateTime.UtcNow`.** Use `HfClock.NowTicks` (`Stopwatch.GetTimestamp()`).
- **`[MethodImpl(AggressiveInlining)]`** on `Enqueue`, `Accept`, `TryPublish`, `TryConsume`, and `IsFull`.
- Hot path structs must be a positive multiple of 64 bytes (64, 128, 192, 256, 320, …) so adjacent ring slots never share a cache line. `SinkConstraints.AssertCacheLineAligned<T>()` enforces this in DEBUG.

## Cycle Budget (reference: Intel i9-12900K, hot caches)
| Operation | Cycles |
|---|---|
| `TryPublish` (ring not full) | ~25c |
| `IsHealthy` check (SpscQueueSink, healthy) | ~7c |
| Successful `Enqueue` (depth 1) | ~32c |
| Fallback hop (unhealthy, SPSC target) | +4c per hop |
| `Volatile.Write` (mfence, x64) | ~15c |
| Virtual call (predicted) | ~3c |

# Design Invariants

## `IsHealthy` and `_healthy`
- `_healthy` is written **exclusively by the consumer thread** (on IOException or recovery). The producer never writes it.
- `IsHealthy` is a short-circuit pre-gate: if false, `Accept` is not called and the item goes directly to `Next`.
- `IsHealthy` and `Accept` are independent gates in `Enqueue`: both must return true for local delivery to succeed.

## Fallback and `Next`
- `Next` is set by `SinkChain.To()`. Pipes never assign their own `Next`.
- On any failure (`IsHealthy == false` OR `Accept == false`), `Next?.Enqueue(item)` is called.
- `Next == null` → silent drop. No exception, no log.

## `Prev` (recovery drain)
- Set by `SinkChain.To()` on `SpscQueueSink<T>` and `MpscQueueSink<T>` instances.
- Used in `TryDrainToPrev()`: when the predecessor recovers, the fallback pipe drains accumulated items back upstream.
- Only `SpscQueueSink` / `MpscQueueSink` participate in `Prev`-based drain. Other `DispatchSink<T>` subclasses do not get `Prev` wired.

## `MultiSink<T>` semantics
- Delivers to **all children** on every `Enqueue`, regardless of individual child health.
- `IsHealthy` = OR over children (short-circuit: true as soon as one child is healthy).
- `Accept` always returns true. Fallback to `Next` only when **all** children are unhealthy (`IsHealthy == false`).
- Items are not re-delivered to unhealthy children; they silently miss them. Multi-dispatch is not redundancy — it is broadcast.
- **`Multi2Sink<T, TC1, TC2>` CRTP variant:** prefer when `TC1` and `TC2` are `sealed` — JIT devirtualizes and inlines both `Enqueue` calls, saving ~6c. Requires concrete sealed types known at compile time.
- **`Multi2PacketSink<TC1, TC2>` (packet hierarchy CRTP variant):** parallel to `Multi2Sink`, fixed-arity 2-child broadcast for `PacketSink` chains. Same JIT devirtualization properties when `TC1`, `TC2` are sealed. Available since Phase 6.

## `FilterSink<T>` semantics
- `Accept` returns true even when the predicate fails. Items that fail the predicate are silently consumed; they do NOT propagate to `Next`. This is intentional — filtered items must not trigger the fallback chain.

## `PropagateAfterAccept` and `ForkSink<T>`

- `DispatchSink<T>.PropagateAfterAccept` is a virtual property, default `false`. When `true`,
  `Enqueue` continues to `Next?.Enqueue` **after** a successful local `Accept` — enabling
  fork / audit / bypass patterns without restructuring the chain.
- JIT note: sealed subclasses returning a compile-time constant (`=> false` or `=> true`)
  collapse the propagate branch at JIT time. Zero overhead on the default-false path.
- Semantics: propagation engages only after a **successful** `Accept`. If `IsHealthy` is false
  or `Accept` returns false, the item still falls through to `Next` as before — propagation
  is not an "always deliver to Next" flag.
- `ForkSink<T>` is the canonical propagate-true sink: forwards to a primary via
  `_primary.Enqueue`, then the base `Enqueue` propagates to `Next`. `IsHealthy`, `Flush`, and
  `Dispose` mirror the primary.

## Builder operators (`Relay.Builder`)

| Entry / operator | Purpose |
|---|---|
| `RelayBuilder.Start<T, THead>(head)` | Generic entry. Any `DispatchSink<T>` head. |
| `RelayBuilder.StartSpsc<T, THead>(head)` | Single-producer entry. `THead : SpscQueueSink<T>`. |
| `RelayBuilder.StartMpsc<T, THead>(head)` | Multi-producer entry. `THead : MpscQueueSink<T>`. |
| `.To(sink)` | Appends `sink` as fallback; wires `Prev` for queue sinks. |
| `.Fork(primary)` | Inserts `ForkSink<T>`; every item goes to `primary` then propagates. |
| `.When(pred).To(downstream)` | Inserts `FilterSink<T>`; items failing `pred` are silently consumed. |
| `.Multi(cfg)` | Broadcast via `MultiBuilder<T>` — array-based `MultiSink<T>`. |
| `.Multi<TC1,TC2>(c1, c2)` | 2-branch broadcast via CRTP `Multi2Sink<T,TC1,TC2>`. |

## MPSC ring buffers

Both `MpscRingBuffer<T>` (typed) and `MpscByteRingBuffer` (byte) share the Log2 `FIX #18`
layout:

- Three `PaddedLong` counters (128-byte isolated cache lines): `_claimedTail`, `_headCache`,
  `_head`. Zero false sharing between producer CAS, producer head-cache read, and consumer
  head write.
- **HeadCache**: producers read a local cached head on the fast path; the cross-core
  volatile read of `_head` happens only when the ring appears full. Under contention this
  eliminates the dominant cache-line bounce.
- Single consumer — multi-consumer is undefined behaviour.

**Typed (`MpscRingBuffer<T>`):**
- Per-slot inline struct `Slot { int Published; T Value; }` on a POH-pinned `Slot[]`.
- Producer CAS on `_claimedTail` by 1 → write `slot.Value` → `Volatile.Write(Published, 1)`.
- Consumer: `Volatile.Read(Published)` → copy out `T` → zero `Value` → `Volatile.Write(Published, 0)` → advance head.

**Byte (`MpscByteRingBuffer`):**
- Variable-length records on POH-pinned `byte[]`. Header = 4 bytes LE with high bit = publish flag.
- Producer CAS on `_claimedTail` by `recordSize` (or `wrapPadding + recordSize` on wrap).
  Writes payload, then `Volatile.Write` header = `len | 0x80000000`. Wrap case stamps
  `0xFFFFFFFF` padding marker at the original tail position.
- Consumer: `Volatile.Read` header → check high bit → if padding, skip to wrap + retry →
  zero-copy `ReadOnlySpan<byte>` return. `Advance()` clears the header (volatile zero) before
  advancing head — recycles the slot for the next producer generation.
- Max payload length: `2^31 - 2 = 0x7FFFFFFE` (one less than the padding sentinel).

**Prev-based recovery drain** applies to both MPSC consumer sinks just as for SPSC — the
drain runs on the single consumer thread, with the same narrow-window caveat on concurrent
resumed producers.

## Packet-sink hierarchy

Parallel tree to `DispatchSink<T>` for variable-length `ReadOnlySpan<byte>` payloads. The two
hierarchies share no types — the unmanaged constraint on `DispatchSink<T>` is incompatible with
`ReadOnlySpan<byte>`, and a unifying generic would force `ref struct` on `T`. Parallelism is
cleaner and costs nothing at runtime.

### Types
- `PacketSink` — abstract base. `Enqueue(ReadOnlySpan<byte>)` short-circuits on `IsHealthy`, then
  `Accept`, falling through to `Next` on failure (or drops if `Next == null`). Same semantics as
  the typed tree.
- `SpscQueueSink` — abstract SPSC consumer (non-generic). Constructor takes `(int ringCapacity, int flushIntervalMs, string sinkName)`;
  subclasses implement `WriteToBackend(ReadOnlySpan<byte>)` / `FlushBackend` / `TryRecoverBackend` / `DisposeBackend`.
  `Flush()` signals via `_flushRequested` — never calls `FlushBackend()` from the producer thread.
- `MpscQueueSink` — abstract MPSC consumer (non-generic). Same abstract API as `SpscQueueSink`; `Flush()` signals via `_flushRequested` — clear-before-run ordering.
- `NullSink` — singleton no-op (`NullSink.Instance`).
- `SpscByteRingBuffer` (internal) — lock-free length-prefixed SPSC ring.

### `SpscByteRingBuffer` invariants
- Capacity: power of two, minimum 16 bytes.
- Record = `[uint32 length (LE)][payload, 4-byte aligned]`. Effective record size = `4 + ((len + 3) & ~3)`.
- `_tail` is always 4-byte aligned — records advance tail by 4-byte multiples.
- Header (4 bytes) is therefore always contiguous — no header ever straddles the wrap point.
- **Payload is always contiguous too**: when a record would straddle the wrap point, the producer
  writes a padding marker (sentinel header `0xFFFFFFFF`) at the current tail position and restarts
  the record at offset 0. Consumer reads the padding marker and skips to the next wrap boundary.
- This eliminates the split-payload scratch copy required by naive byte ring designs —
  `TryPeek(out ReadOnlySpan<byte> payload, out int advanceBytes)` always returns a zero-copy span.
- Head/tail padding: same 128-byte `PaddedLong` as `SpscRingBuffer<T>`.

### When to use which tree
| Use typed `DispatchSink<T>` when... | Use `PacketSink` when... |
|---|---|
| Payload is a fixed-size unmanaged struct, multiple of 64B | Payload length varies per record |
| Zero-copy fixed-layout matters (Struct-of-arrays, SIMD) | Payload is already a serialized/encoded byte blob |
| `SinkConstraints.AssertCacheLineAligned<T>()` applies | You need a byte-oriented backend (text log, framed protocol) |

### Status (current)
- SPSC-only concrete sinks. `MpscQueueSink` abstract base exists; no concrete MPSC
  backends yet — add if multi-producer contention is demonstrated by BDN.
- `PacketSink.PropagateAfterAccept` is defined and documented — concrete tee/fork sinks
  for the packet hierarchy land in Phase 1 (`ForkSink` non-generic).
- No dedicated builder yet (`SinkChainBuilder` / `SinkChain<THead>` land in Phase 1).
  Chains are wired manually via `PacketSink.Next` (internal setter) until the builder ships.

## `SpscRingBuffer<T>` contract
- SPSC: one producer thread, one consumer thread. Violating this is undefined behaviour.
- Head and tail use `PaddedLong` (128B `[StructLayout(Explicit, Size=128)]`) to prevent false sharing.
- `TryPublish` uses `Volatile.Write` on tail (mfence on x64, ~15c). `TryConsume` uses `Volatile.Write` on head.
- Batched-write API (`TryReserveTail` / `WriteSlot` / `CommitTail`) enables a single mfence for N writes; use with `Thread.MemoryBarrier()` between write and commit.

## `SpscQueueSink<T>` lifecycle
- `Start()` must be called before `Enqueue` — spawns consumer thread and pre-faults the ring.
- `Stop(drainTimeoutMs)` signals stop and joins with drain window. `Dispose()` calls `Stop()` — always dispose via `using` or explicit `Stop`.
- `IsConsuming` is false only after the consumer thread exits due to an unhandled exception.
- `ConsumerException` is set in the catch block; read only on cold path (diagnostic).

# Concrete Sinks

| Sink | Backend | Failure trigger | Recovery |
|---|---|---|---|
| `FileStreamSink<T>` | `FileStream`, POH write buffer | `IOException` in `FlushBuffer` | Reopen stream, backoff 1s → 60s |
| `TcpSink<T>` | `TcpClient` + `NetworkStream`, POH send buffer | `Exception` in `FlushBuffer` | Reconnect, backoff 1s → 30s |
| `MmfSink<T>` | `MemoryMappedViewAccessor` | Capacity exhaustion (`_position + sizeof(T) > maxBytes`) | None — capacity only |
| `RamSink<T>` | `NativeMemory.AllocZeroed`, unsafe circular ring | Ring full | None — `DrainTo(target)` on recovery. **Must free in `Dispose`.** |

# Testing
- `dotnet test tests/Relay.Tests` must pass (0 failures) before any commit.
- Test file per concern: `DispatchSinkChainTests`, `SpscQueueSinkTests`, `MultiSinkTests`, `ForkSinkTests`, `RecoveryDrainTests`.
- Local test pipes extend `DispatchSink<T>` or `SpscQueueSink<T>` directly — no mocking frameworks.
- Tests that use `Thread.Sleep` for consumer timing must use short, deterministic windows. Prefer `Stop(drainTimeoutMs)` to signal completion rather than sleeping and asserting on side effects.

# Code Style
- **File size:** target < 200 LOC. Concrete pipes with complex recovery may reach 120–150 LOC; document if exceeding 200.
- **Comments:** default none. Add only when the WHY is non-obvious (invariant, platform quirk, correctness constraint).
- **XML docs:** `<summary>` on all public types and members. `<remarks>` for cross-cutting concerns (e.g. threading constraints, fallback semantics). No paraphrasing of method names.
- **`using` directives:** explicit (no implicit usings). Group: `System.*` first, then `Relay.*`. No blank lines between groups.
- **`unsafe` methods:** annotate with `protected override unsafe void` — never suppress the compiler warning, use the `unsafe` keyword explicitly.

# Git Workflow (override)
- Branch naming: `feature/<yyMMdd>-<slug>`, `fix/<yyMMdd>-<slug>`, `refactor/<yyMMdd>-<slug>` — no `<ref>` segment (lib has no issue tracker).
- Commit gate: `dotnet test tests/Relay.Tests` (0 failures required).

# Model Routing
- **Sonnet** — implementation, refactor, tests
- **Opus** — architectural decisions, new concrete pipes, performance analysis, scope review
