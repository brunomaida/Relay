# Hot Path Audit — Circular Ring Tests

**Date:** 2026-05-27  
**Scope:** `tests/Relay.Tests/Circular/PureSinkRingTests.cs`, `tests/Relay.Tests/Circular/ReceiverSinkRingTests.cs`  
**Trigger:** `banned-api-enforce` Gate 1 flagged `Thread.Sleep` + structural signals in both files  
**Gate:** Gate 2 — deep hot-path analysis  
**Auditor:** Claude (Sonnet 4.6)

---

## Audited Files

| File | Hot-Path Justification |
|------|------------------------|
| `PureSinkRingTests.cs` | Contains producer injection loop calling `ring.Entry.Enqueue(...)` at ~22k items/s during commit-gate tests. Structural signal: tight for-loop over `itemCount`. |
| `ReceiverSinkRingTests.cs` | Contains a receiver polling loop (`while (pollerRun == 1) { ... }`) running at bus speed. Structural signal: `Thread.SpinWait` inside a tight while loop. |

---

## Gate 1 Findings (from banned-api-enforce)

| File | Line(s) | Pattern | Classification |
|------|---------|---------|----------------|
| `PureSinkRingTests.cs` | 49 | `Thread.Sleep(pollMs)` — `WaitQuiesce` helper | Test orchestration |
| `PureSinkRingTests.cs` | 68 | `Thread.Sleep(pollMs)` — `WaitQuiescePacket` helper | Test orchestration |
| `PureSinkRingTests.cs` | 175, 180, 205, 210, 240, 245, 265, 270 | `Thread.Sleep(1_000)` — stress snapshot loop | Stress test (commit-gate excluded) |
| `ReceiverSinkRingTests.cs` | 55 | `Thread.Sleep(pollMs)` — `WaitQuiesce` helper | Test orchestration |
| `ReceiverSinkRingTests.cs` | 102, 178 | `Thread.SpinWait(20)` — poller idle back-off | Polling primitive (not `Thread.Sleep`) |
| `ReceiverSinkRingTests.cs` | 129, 208 | `Thread.Sleep(200)` — post-stop drain window | Test teardown |
| `ReceiverSinkRingTests.cs` | 199 | `Thread.Sleep(1_000)` — stress snapshot | Stress test (commit-gate excluded) |

---

## Audit Dimensions

### Step 1 — Hot Path Identification

**Delivery hot path:** `Enqueue(item)` → SPSC/MPSC ring buffer → consumer thread → `WriteToBackend` → `RingNext?.Enqueue(item)`.

**Paths executed by test code:**
| Code segment | On hot path? | Rationale |
|---|---|---|
| `ring.Entry.Enqueue(new Packet64 { ... })` injection loop | **Yes** — producer side of the ring | Calls into library hot path directly |
| `WaitQuiesce` polling loop with `Thread.Sleep` | **No** | Runs after injection completes; monitors quiescence |
| `Thread.SpinWait(20)` in receiver poller | **Yes** — tight loop, but idle branch only | Only reached when `Poll()` returns false (ring empty) |
| `Thread.Sleep(200)` post-stop drain window | **No** | Runs after `n2.Stop(); n1.Stop(); n0.Stop()` completes |
| `Thread.Sleep(1_000)` in stress snapshot loops | **No** | Between 1-second observation windows; `[Trait("Category","Stress")]` |

### A3 — CPU Cycles (injection loop)

The injection loop:
```csharp
for (long id = 0; id < itemCount; id++)
    ring.Entry.Enqueue(new Packet64 { HopCount = hopCount, Id = id });
```
- `Packet64` is a `struct` — stack-allocated. No allocation per iteration.
- `Enqueue` is `[AggressiveInlining]` on `DispatchSink<T>`. JIT inlines into caller.
- **No issue.**

The packet injection loop:
```csharp
Span<byte> buf = stackalloc byte[64];
for (long id = 0; id < itemCount; id++)
{
    buf.Clear();
    PacketLayout.WriteHop(buf, hopCount);
    PacketLayout.WriteId(buf, id);
    ring.Entry.Enqueue(buf);
}
```
- `stackalloc byte[64]` is outside the loop — single stack allocation.
- `buf.Clear()` is `Span.Clear()` → `Unsafe.InitBlockUnaligned` → single SIMD zero.
- `WriteHop`/`WriteId` are `Unsafe.WriteUnaligned` — direct register writes.
- **No issue.**

### C9 — Zero-Allocation (injection loops)

`new Packet64 { ... }` is a value-type struct literal. Assigned to stack slot. No heap allocation.  
`stackalloc byte[64]` is stack memory. No heap allocation.  
**No allocation on the producer hot path.**

### C9 — Zero-Allocation (WaitQuiesce)

`WaitQuiesce` creates no heap allocations. The `Func<long>` delegate is captured once at call site (potential single heap allocation at test setup, not per-poll). No issue in the polling body.

### D14 — Closures & Delegates

`WaitQuiesce(() => n0.Count + n1.Count + n2.Count)` — the lambda captures `n0`, `n1`, `n2` which are local variables. This creates one delegate allocation per call (cold — called once per test). Acceptable.

### F21 — Lock-Free / Contention

`Interlocked.Increment(ref ctr[0])` in the receiver callback: correct lock-free pattern. The callback runs on the poller thread; `ctr[0]` is a shared counter. **No issue.**

`Volatile.Read(ref pollerRun)` / `Volatile.Write(ref pollerRun, 0)`: correct volatile fence for cross-thread signalling. **No issue.**

### D17 — Exception Cost

No `try/catch` in any injection loop or poller. `WaitQuiesce` has no exception handling. **No issue.**

### Thread.Sleep — Gate 2 Analysis

Per CLAUDE.md: `Thread.Sleep → ManualResetEventSlim.Wait or Task.Delay (cold path)`.

**`WaitQuiesce` polling (`Thread.Sleep(pollMs)`, pollMs=20):**
- Purpose: Wait 20 ms between polls checking whether `TotalCount()` has stabilised.
- Hot path impact: **None.** Runs only after all items are injected, waiting for the ring to drain. The ring consumer threads continue running freely during this wait.
- Alternative: `ManualResetEventSlim.Wait` would require the ring to signal — but `TotalCount()` stability is not a signallable event from inside the ring nodes. A signal-based approach would require modifying production code to instrument test concerns. Not appropriate.
- The CLAUDE.md testing section explicitly allows `Thread.Sleep` in tests for "short, deterministic windows" — 20 ms qualifies.
- **VERDICT: Accepted.** Cold path, test-only, bounded window (2 × 20 ms stable = 40 ms minimum).

**`Thread.Sleep(200)` drain window (ReceiverSinkRingTests.cs:129, 208):**
- Purpose: After `Stop()` completes and shm is disposed, allows the poller thread to drain any frames already in the shared ring buffer before signalling shutdown.
- Hot path impact: **None.** Runs in test teardown after ring nodes are stopped.
- Alternative: Replace with a `ManualResetEventSlim` signalled from the poller after detecting no more frames. More correct but adds complexity; the 200 ms bound is adequate for the commit-gate test's 50-item workload.
- **VERDICT: Accepted.** Test teardown, bounded, not on delivery hot path.

**`Thread.Sleep(1_000)` in stress snapshot loops:**
- Entirely within `[Trait("Category","Stress")]` tests excluded from the commit gate.
- **VERDICT: Accepted.** No impact on commit gate.

### Thread.SpinWait — Gate 2 Analysis

`Thread.SpinWait(20)` executes the x86 PAUSE instruction 20 times (~20 × 5 ns = ~100 ns on modern Intel). It is NOT `Thread.Sleep`. It is the canonical adaptive back-off primitive for lock-free polling and is not in the banned-API list.

The poller loop:
```csharp
while (Volatile.Read(ref pollerRun) == 1)
{
    if (!receiver.Poll()) Thread.SpinWait(20);
}
```
- When frames are available: `Poll()` returns true, `SpinWait` is skipped. Full-speed polling.
- When idle: `SpinWait(20)` briefly backs off without yielding the OS scheduler. Correct pattern.
- This loop runs on a dedicated test thread, not on any Relay consumer thread.
- **VERDICT: Accepted.** Correct primitive. Not banned.

### H24/H25 — Test Coverage

Addressed by the feature itself — these ARE the test files. Coverage review is out of scope for this audit.

---

## Summary of Findings

| # | Severity | Component | Issue | Action |
|---|---|---|---|---|
| 1 | Info | `WaitQuiesce` (both files) | `Thread.Sleep(20)` not on delivery hot path | Accepted — test orchestration, cold path |
| 2 | Info | ReceiverSinkRingTests | `Thread.Sleep(200)` post-stop drain | Accepted — test teardown, cold path |
| 3 | Info | Stress tests | `Thread.Sleep(1_000)` snapshot timing | Accepted — commit-gate excluded |
| 4 | Info | ReceiverSinkRingTests | `Thread.SpinWait(20)` poller idle back-off | Accepted — correct primitive, not banned |
| 5 | None | Injection loops | `new Packet64` / `stackalloc byte[64]` | Zero-alloc confirmed |
| 6 | None | All | Lock/Monitor usage | None found |
| 7 | None | All | LINQ usage | None found |
| 8 | None | All | DateTime.UtcNow usage | None found (`Environment.TickCount64` used) |

---

## Scores

| Cluster | Score | Notes |
|---|---|---|
| A. CPU & Computation | 9/10 | Injection loops are struct + inlined Enqueue, zero overhead |
| B. Memory Layout | 10/10 | Packet structs are cache-line aligned; stackalloc in producer |
| C. Allocation & GC | 10/10 | Zero allocation on delivery hot path |
| D. Language Runtime | 9/10 | One delegate allocation per WaitQuiesce call (cold, acceptable) |
| E. Compiler/JIT | 9/10 | Enqueue is AggressiveInlining; no virtual dispatch in loops |
| F. Concurrency | 10/10 | Volatile + Interlocked only; no lock/Monitor |
| G. System Boundary | 9/10 | Thread.Sleep only in cold paths; SpinWait correct in poller |
| H. Test Validation | 10/10 | Commit-gate finite + stress/endurance gated correctly |

---

## Verdict

**PASS — No hot-path violations.**

All `Thread.Sleep` calls are in test orchestration code (WaitQuiesce polling, drain windows) or in `[Trait("Category","Stress")]` tests excluded from the commit gate. None are on the message delivery hot path. `Thread.SpinWait(20)` is the correct primitive for low-latency polling back-off and is not a banned API.

No changes to the flagged files are required. Gate 1 fired on structural signals (tight loops + `Thread.Sleep` in the same file) that do not co-locate on the same execution path.
