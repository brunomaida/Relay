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
    DispatchPipe.cs          ← abstract base, Enqueue hot path
    SpscQueuePipe.cs         ← async delivery via SPSC ring + consumer thread
    FanOutPipe.cs            ← broadcast (array-based + FanOut2Pipe CRTP variant)
    FilterPipe.cs            ← conditional gate
    NullPipe.cs              ← no-op sink / terminal fallback
    Buffers/
      SpscRingBuffer.cs      ← lock-free SPSC ring, 128B padded head/tail
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
tests/
  Relay.Tests/
    Relay.Tests.csproj
    DispatchPipeChainTests.cs
    SpscQueuePipeTests.cs
    FanOutPipeTests.cs
    RecoveryDrainTests.cs
```

# Namespaces
| Namespace | Content |
|---|---|
| `Relay` | `DispatchPipe<T>`, `SpscQueuePipe<T>`, `FanOutPipe<T>`, `FanOut2Pipe<T,TC1,TC2>`, `FilterPipe<T>`, `NullPipe<T>` |
| `Relay.Buffers` | `SpscRingBuffer<T>` (internal to lib) |
| `Relay.Pipes` | Concrete backends: `FileStreamPipe<T>`, `MmfPipe<T>`, `TcpPipe<T>`, `RamPipe<T>` |
| `Relay.Builder` | `RelayBuilder`, `PipeChain<T,THead>` |
| `Relay.Memory` | `RelayMemory` (internal) |
| `Relay.Internal` | `HfClock`, `PipeConstraints` (internal) |

# Performance — Everything Is Hot Path

`Enqueue` is the sole public entry point and is called at tick-rate frequency.
**All code on the `Enqueue → Accept` path must be treated as hot path.**

## Invariants
- **Zero allocations** in steady state. `GC.AllocateArray(pinned: true)` only in constructors.
- **No LINQ.** Not on any path.
- **No `lock` / `Monitor`.**  Volatile + Interlocked only.
- **No `async`/`await`** in the dispatch or consume path.
- **No `DateTime.UtcNow`.** Use `HfClock.NowTicks` (`Stopwatch.GetTimestamp()`).
- **`[MethodImpl(AggressiveInlining)]`** on `Enqueue`, `Accept`, `TryPublish`, `TryConsume`, and `IsFull`.
- Hot path structs must be cache-line sized: 32, 64, 128, or 256 bytes. `PipeConstraints.AssertCacheLineAligned<T>()` enforces this in DEBUG.

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

## `FilterPipe<T>` semantics
- `Accept` returns true even when the predicate fails. Items that fail the predicate are silently consumed; they do NOT propagate to `Next`. This is intentional — filtered items must not trigger the fallback chain.

## `FanOut2Pipe<T, TC1, TC2>` CRTP variant
- Prefer over `FanOutPipe<T>` when `TC1` and `TC2` are `sealed` — JIT devirtualizes and inlines `Enqueue`, saving ~6c.
- Requires children to be concrete sealed types known at compile time.

## `SpscRingBuffer<T>` contract
- SPSC: one producer thread, one consumer thread. Violating this is undefined behaviour.
- Head and tail use `PaddedLong` (128B `[StructLayout(Explicit, Size=128)]`) to prevent false sharing.
- `TryPublish` uses `Volatile.Write` on tail (mfence on x64, ~15c). `TryConsume` uses `Volatile.Write` on head.
- Batched-write API (`TryReserveTail` / `WriteSlot` / `CommitTail`) enables a single mfence for N writes; use with `Thread.MemoryBarrier()` between write and commit.

## `SpscQueuePipe<T>` lifecycle
- `Start()` must be called before `Enqueue` — spawns consumer thread and pre-faults the ring.
- `Stop(drainTimeoutMs)` signals stop and joins with drain window.
- `Dispose()` calls `Stop()`. Always dispose via `using` or explicit `Stop`.
- `IsConsuming` is false only after the consumer thread exits due to an unhandled exception.
- `ConsumerException` is set in the catch block; read only on cold path (diagnostic).

# Concrete Pipes

| Pipe | Backend | Failure trigger | Recovery |
|---|---|---|---|
| `FileStreamPipe<T>` | `FileStream`, POH write buffer | `IOException` in `FlushBuffer` | Reopen stream, backoff 1s → 60s |
| `TcpPipe<T>` | `TcpClient` + `NetworkStream`, POH send buffer | `Exception` in `FlushBuffer` | Reconnect, backoff 1s → 30s |
| `MmfPipe<T>` | `MemoryMappedViewAccessor` | Capacity exhaustion (`_position + sizeof(T) > maxBytes`) | None — capacity only |
| `RamPipe<T>` | `NativeMemory.AllocZeroed`, unsafe circular ring | Ring full | None — `DrainTo(target)` on recovery |

# Builder Usage

```csharp
using Relay.Builder;

// Serial: FileStream → TCP → RAM
var head = RelayBuilder
    .Start<Entry, FileStreamPipe<Entry>>(new FileStreamPipe<Entry>("/data/out.bin"))
    .To(new TcpPipe<Entry>("backup", 9090))
    .To(new RamPipe<Entry>())
    .Build();

head.Start(); // SpscQueuePipe nodes only
head.Enqueue(in entry);

// Fan-out + serial fallback
var fan = RelayBuilder
    .Start<Entry, FanOutPipe<Entry>>(
        new FanOutPipe<Entry>(
            new FileStreamPipe<Entry>("/data/primary.bin"),
            new TcpPipe<Entry>("remote", 9090)))
    .To(new RamPipe<Entry>())
    .Build();
```

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

# Invariants Never to Break
1. `_healthy` is written only by the consumer thread. Producer reads only.
2. `Next` is assigned only by `PipeChain.To()`. Pipes do not self-wire.
3. `Accept` on `FanOutPipe` always returns true. Only `IsHealthy == false` triggers fallback.
4. `FilterPipe.Accept` always returns true. Filtered items never reach `Next`.
5. `SpscRingBuffer` is single-producer / single-consumer. No multi-threaded access.
6. `NativeMemory.AllocZeroed` allocations in `RamPipe` must be freed in `Dispose`.
7. No external dependencies in `src/Relay`. The lib must compile with no NuGet references.

# Git Workflow
- Branch: `feature/<yyMMdd>-<slug>`, `fix/<yyMMdd>-<slug>`, `refactor/<yyMMdd>-<slug>`
- Base off `develop`; merge to `develop` when stable
- Commit message: Conventional Commits in English (`feat:`, `fix:`, `refactor:`, `chore:`, `docs:`, `test:`)
- When commit is authored by Claude: append `w/Claude` to the message (no `Co-Authored-By` trailer)
- Commit gate: all unit tests passing (`dotnet test tests/Relay.Tests`)

# Model Routing
- **Sonnet** — default for implementation, refactor, tests
- **Opus** — architectural decisions, new concrete pipes, performance analysis, scope review
- **Haiku** — never; not used in this project

# Response Format
1. Prose in **PT-BR**, direct, no filler
2. Complete and correct code — no partial snippets
3. XML docs and inline comments in **English**
4. Criticism justified by runtime cost (GC, cache miss, JIT IL size). Unmeasured optimization is premature.
