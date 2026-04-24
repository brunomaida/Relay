# Role
Infrastructure library for composable fallback dispatch pipelines over `T : unmanaged`.
Reason from hardware and runtime first principles. Measure before optimizing.

Single responsibility: receive `T`, deliver to the configured backend, and if delivery fails, forward to the next pipe in the chain. No logging. No telemetry. No orchestration.

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
    DispatchPipe.cs          ← abstract base (typed), Enqueue hot path
    BytePipe.cs              ← abstract base (byte payloads), parallel hierarchy
    SpscQueuePipe.cs         ← async delivery via SPSC ring + consumer thread
    SpscByteQueuePipe.cs     ← byte-variant SPSC consumer
    MpscQueuePipe.cs         ← async delivery via MPSC ring (multi-producer)
    MpscByteQueuePipe.cs     ← byte-variant MPSC consumer
    TeePipe.cs               ← primary + Next propagation (audit/bypass pattern)
    FanOutPipe.cs            ← broadcast (array-based + FanOut2Pipe CRTP variant)
    FilterPipe.cs            ← conditional gate
    NullPipe.cs              ← no-op sink / terminal fallback
    NullBytePipe.cs          ← byte-variant no-op sink
    Buffers/
      SpscRingBuffer.cs      ← lock-free SPSC ring, 128B padded head/tail
      SpscByteRingBuffer.cs  ← byte-variant length-prefixed ring
      MpscRingBuffer.cs      ← MPSC ring: CAS tail + HeadCache + inline Slot (Log2 FIX #18)
      MpscByteRingBuffer.cs  ← byte-variant MPSC: CAS reservation + header publish-bit
    Pipes/
      FileStreamPipe.cs      ← binary write to FileStream, POH buffer, backoff recovery
      MmfPipe.cs             ← MemoryMappedFile, capacity-only failure
      TcpPipe.cs             ← TCP socket, POH send buffer, backoff reconnect
      RamPipe.cs             ← native memory circular ring, last-resort fallback
    Builder/
      RelayBuilder.cs        ← static entry point: Start<T, THead>(head)
      PipeChain.cs           ← fluent chain builder, wires Next + Prev
    Memory/
      RelayMemory.cs         ← internal: PreFault + VirtualLock on ring buffer
    Internal/
      PipeConstraints.cs     ← internal: cache-line alignment assertion (DEBUG)
      HfClock.cs             ← internal: Stopwatch.GetTimestamp() wrapper
```

# Namespaces
| Namespace | Content |
|---|---|
| `Relay` | `DispatchPipe<T>`, `SpscQueuePipe<T>`, `MpscQueuePipe<T>`, `FanOutPipe<T>`, `FanOut2Pipe<T,TC1,TC2>`, `FilterPipe<T>`, `NullPipe<T>`, `TeePipe<T>`, `BytePipe`, `SpscByteQueuePipe`, `MpscByteQueuePipe`, `NullBytePipe` |
| `Relay.Buffers` | `SpscRingBuffer<T>`, `MpscRingBuffer<T>`, `SpscByteRingBuffer`, `MpscByteRingBuffer` (internal to lib) |
| `Relay.Pipes` | Concrete backends: `FileStreamPipe<T>`, `MmfPipe<T>`, `TcpPipe<T>`, `RamPipe<T>` |
| `Relay.Builder` | `RelayBuilder`, `PipeChain<T,THead>` |
| `Relay.Memory` | `RelayMemory` (internal) |
| `Relay.Internal` | `HfClock`, `PipeConstraints` (internal) |

# Performance — Everything Is Hot Path

**All code on the `Enqueue → Accept` path is hot path.** `Enqueue` is called at tick-rate frequency.

## Invariants
- **Zero allocations** in steady state. `GC.AllocateArray(pinned: true)` only in constructors.
- **No LINQ.** Not on any path.
- **No `lock` / `Monitor`.** Volatile + Interlocked only.
- **No `async`/`await`** in the dispatch or consume path.
- **No `DateTime.UtcNow`.** Use `HfClock.NowTicks` (`Stopwatch.GetTimestamp()`).
- **`[MethodImpl(AggressiveInlining)]`** on `Enqueue`, `Accept`, `TryPublish`, `TryConsume`, and `IsFull`.
- Hot path structs must be a positive multiple of 64 bytes (64, 128, 192, 256, 320, …) so adjacent ring slots never share a cache line. `PipeConstraints.AssertCacheLineAligned<T>()` enforces this in DEBUG.

## Cycle Budget (reference: Intel i9-12900K, hot caches)
| Operation | Cycles |
|---|---|
| `TryPublish` (ring not full) | ~25c |
| `IsHealthy` check (SpscQueuePipe, healthy) | ~7c |
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
- `Next` is set by `PipeChain.To()`. Pipes never assign their own `Next`.
- On any failure (`IsHealthy == false` OR `Accept == false`), `Next?.Enqueue(item)` is called.
- `Next == null` → silent drop. No exception, no log.

## `Prev` (recovery drain)
- Set by `PipeChain.To()` on `SpscQueuePipe<T>` instances only.
- Used in `TryDrainToPrev()`: when the predecessor recovers, the fallback pipe drains accumulated items back upstream.
- Only `SpscQueuePipe` participates in `Prev`-based drain. `DispatchPipe<T>` subclasses that are not `SpscQueuePipe` do not get `Prev` wired.

## `FanOutPipe<T>` semantics
- Delivers to **all children** on every `Enqueue`, regardless of individual child health.
- `IsHealthy` = OR over children (short-circuit: true as soon as one child is healthy).
- `Accept` always returns true. Fallback to `Next` only when **all** children are unhealthy (`IsHealthy == false`).
- Items are not re-delivered to unhealthy children; they silently miss them. Fan-out is not redundancy — it is broadcast.
- **`FanOut2Pipe<T, TC1, TC2>` CRTP variant:** prefer when `TC1` and `TC2` are `sealed` — JIT devirtualizes and inlines both `Enqueue` calls, saving ~6c. Requires concrete sealed types known at compile time.

## `FilterPipe<T>` semantics
- `Accept` returns true even when the predicate fails. Items that fail the predicate are silently consumed; they do NOT propagate to `Next`. This is intentional — filtered items must not trigger the fallback chain.

## `PropagateAfterAccept` and `TeePipe<T>`

- `DispatchPipe<T>.PropagateAfterAccept` is a virtual property, default `false`. When `true`,
  `Enqueue` continues to `Next?.Enqueue` **after** a successful local `Accept` — enabling
  tee / audit / bypass patterns without restructuring the chain.
- JIT note: sealed subclasses returning a compile-time constant (`=> false` or `=> true`)
  collapse the propagate branch at JIT time. Zero overhead on the default-false path.
- Semantics: propagation engages only after a **successful** `Accept`. If `IsHealthy` is false
  or `Accept` returns false, the item still falls through to `Next` as before — propagation
  is not an "always deliver to Next" flag.
- `TeePipe<T>` is the canonical propagate-true pipe: forwards to a primary via
  `_primary.Enqueue`, then the base `Enqueue` propagates to `Next`. `IsHealthy`, `Flush`, and
  `Dispose` mirror the primary.

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

**Prev-based recovery drain** applies to both MPSC consumer pipes just as for SPSC — the
drain runs on the single consumer thread, with the same narrow-window caveat on concurrent
resumed producers.

## Byte-pipe hierarchy

Parallel tree to `DispatchPipe<T>` for variable-length `ReadOnlySpan<byte>` payloads. The two
hierarchies share no types — the unmanaged constraint on `DispatchPipe<T>` is incompatible with
`ReadOnlySpan<byte>`, and a unifying generic would force `ref struct` on `T`. Parallelism is
cleaner and costs nothing at runtime.

### Types
- `BytePipe` — abstract base. `Enqueue(ReadOnlySpan<byte>)` short-circuits on `IsHealthy`, then
  `Accept`, falling through to `Next` on failure (or drops if `Next == null`). Same semantics as
  the typed tree.
- `SpscByteQueuePipe` — abstract SPSC consumer. Constructor takes `(int ringCapacity, int flushIntervalMs, string pipeName)`;
  subclasses implement `WriteToBackend(ReadOnlySpan<byte>)` / `FlushBackend` / `TryRecoverBackend` / `DisposeBackend`.
- `NullBytePipe` — singleton no-op (`NullBytePipe.Instance`).
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
| Use typed `DispatchPipe<T>` when... | Use `BytePipe` when... |
|---|---|
| Payload is a fixed-size unmanaged struct, multiple of 64B | Payload length varies per record |
| Zero-copy fixed-layout matters (Struct-of-arrays, SIMD) | Payload is already a serialized/encoded byte blob |
| `PipeConstraints.AssertCacheLineAligned<T>()` applies | You need a byte-oriented backend (text log, framed protocol) |

### Status (current)
- SPSC-only. No MPSC variant — add if multi-producer contention is demonstrated by BDN.
- No `PropagateAfterAccept` / tee variant — deferred until a consumer requires it.
- No dedicated builder (`ByteChain` / `ByteChainBuilder`) — chains are wired manually via
  `BytePipe.Next` (internal setter). Builder will be added once 2+ consumers need one.

## `SpscRingBuffer<T>` contract
- SPSC: one producer thread, one consumer thread. Violating this is undefined behaviour.
- Head and tail use `PaddedLong` (128B `[StructLayout(Explicit, Size=128)]`) to prevent false sharing.
- `TryPublish` uses `Volatile.Write` on tail (mfence on x64, ~15c). `TryConsume` uses `Volatile.Write` on head.
- Batched-write API (`TryReserveTail` / `WriteSlot` / `CommitTail`) enables a single mfence for N writes; use with `Thread.MemoryBarrier()` between write and commit.

## `SpscQueuePipe<T>` lifecycle
- `Start()` must be called before `Enqueue` — spawns consumer thread and pre-faults the ring.
- `Stop(drainTimeoutMs)` signals stop and joins with drain window. `Dispose()` calls `Stop()` — always dispose via `using` or explicit `Stop`.
- `IsConsuming` is false only after the consumer thread exits due to an unhandled exception.
- `ConsumerException` is set in the catch block; read only on cold path (diagnostic).

# Concrete Pipes

| Pipe | Backend | Failure trigger | Recovery |
|---|---|---|---|
| `FileStreamPipe<T>` | `FileStream`, POH write buffer | `IOException` in `FlushBuffer` | Reopen stream, backoff 1s → 60s |
| `TcpPipe<T>` | `TcpClient` + `NetworkStream`, POH send buffer | `Exception` in `FlushBuffer` | Reconnect, backoff 1s → 30s |
| `MmfPipe<T>` | `MemoryMappedViewAccessor` | Capacity exhaustion (`_position + sizeof(T) > maxBytes`) | None — capacity only |
| `RamPipe<T>` | `NativeMemory.AllocZeroed`, unsafe circular ring | Ring full | None — `DrainTo(target)` on recovery. **Must free in `Dispose`.** |

# Testing
- `dotnet test tests/Relay.Tests` must pass (0 failures) before any commit.
- Test file per concern: `DispatchPipeChainTests`, `SpscQueuePipeTests`, `FanOutPipeTests`, `RecoveryDrainTests`.
- Local test pipes extend `DispatchPipe<T>` or `SpscQueuePipe<T>` directly — no mocking frameworks.
- Tests that use `Thread.Sleep` for consumer timing must use short, deterministic windows. Prefer `Stop(drainTimeoutMs)` to signal completion rather than sleeping and asserting on side effects.

# Code Style
- **File size:** target < 200 LOC. Concrete pipes with complex recovery may reach 120–150 LOC; document if exceeding 200.
- **Comments:** default none. Add only when the WHY is non-obvious (invariant, platform quirk, correctness constraint).
- **XML docs:** `<summary>` on all public types and members. `<remarks>` for cross-cutting concerns (e.g. threading constraints, fallback semantics). No paraphrasing of method names.
- **Naming:** PascalCase public, camelCase locals, `_camelCase` private fields, `SCREAMING_SNAKE_CASE` for `const` only.
- **`using` directives:** explicit (no implicit usings). Group: `System.*` first, then `Relay.*`. No blank lines between groups.
- **`unsafe` methods:** annotate with `protected override unsafe void` — never suppress the compiler warning, use the `unsafe` keyword explicitly.

# Git Workflow
- Branch: `feature/<yyMMdd>-<slug>`, `fix/<yyMMdd>-<slug>`, `refactor/<yyMMdd>-<slug>`
- Base off `develop`; merge to `develop` when stable
- Commit message: Conventional Commits in English (`feat:`, `fix:`, `refactor:`, `chore:`, `docs:`, `test:`)
- When commit is authored by Claude: append `w/Claude` to the message (no `Co-Authored-By` trailer)
- Commit gate: all unit tests passing (`dotnet test tests/Relay.Tests`)

# Model Routing
- **Sonnet** — implementation, refactor, tests
- **Opus** — architectural decisions, new concrete pipes, performance analysis, scope review

# Response Format
1. Prose in **PT-BR**, direct, no filler
2. Complete and correct code — no partial snippets
3. XML docs and inline comments in **English**
4. Criticism justified by runtime cost (GC, cache miss, JIT IL size). Unmeasured optimization is premature.
