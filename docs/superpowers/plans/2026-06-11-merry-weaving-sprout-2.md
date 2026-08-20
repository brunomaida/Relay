# Plan: Relay Robustness + Hot-Path Fixes

## Context

Cost-map audit (2026-06-10, static+BDN) identified 2 crash paths that kill `FileStreamSink`'s
consumer thread under transient disk failure, plus 1 RDTSC per-record overhead in
`RotatingFileSink` and 1 non-inlineable recursion in `MpscByteRingBuffer`. Fable unavailable
for hot-path review — assessment based on the generated cost-map and BDN baseline only.

**No new abstractions. All changes are surgical.**

## Task 1 — FileStreamSink: 2 crash paths (🔴 block)

**File:** `src/Relay/Sinks/FileStreamSink.cs`

### Crash 1: `_bufferPos` not reset → IndexOutOfRangeException (FileStreamSink.cs:99–105)

`FlushBuffer` IOException handler catches and sets `_healthy=false` but does NOT reset
`_bufferPos`. Next `WriteToBackend` call executes `ref _writeBuffer[_bufferPos]` where
`_bufferPos == _writeBuffer.Length` → `IndexOutOfRangeException` escapes `ConsumeLoop`
(SpscQueueSink.cs:251) → consumer thread dies permanently. Ring contents lost.

**Fix:** add `_bufferPos = 0;` in the IOException catch block (FileStreamSink.cs:101):

```csharp
catch (IOException)
{
    _bufferPos       = 0; // prevent OOB on next WriteToBackend
    _healthy         = false;
    _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
}
```

### Crash 2: disposed `_stream` not nulled → ObjectDisposedException (FileStreamSink.cs:76–87)

`TryRecoverBackend`: `_stream?.Dispose()` at :76, then `OpenStream()` at :77 throws → catch at :82
does NOT null `_stream`. `FlushBackend` at :65 (no `_healthy` gate) calls `FlushBuffer` if
`_bufferPos > 0`, which calls `_stream!.Write(…)` on the disposed stream → `ObjectDisposedException`
(inherits `InvalidOperationException`, NOT `IOException`) escapes `catch (IOException)` in
`FlushBuffer` → escapes consumer → consumer thread dies permanently.

**Fix (2 changes):**

1. Null `_stream` in the recovery catch (FileStreamSink.cs:82):

```csharp
catch (Exception)
{
    _stream          = null; // drop disposed reference — FlushBuffer null-guards below
    _retryDelayMs    = Math.Min(_retryDelayMs * 2, RetryMaxDelayMs);
    _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
}
```

2. Add null guard at top of `FlushBuffer` (FileStreamSink.cs:94). Without this, nulling `_stream`
   merely swaps `ObjectDisposedException` for `NullReferenceException` — consumer still dies.
   Also incorporates the Crash 1 `_bufferPos = 0` fix, so both crashes share one corrected method:

```csharp
private void FlushBuffer()
{
    if (_stream is null) { _bufferPos = 0; return; }
    try
    {
        _stream.Write(_writeBuffer.AsSpan(0, _bufferPos));
        _bufferPos = 0;
    }
    catch (IOException)
    {
        _bufferPos       = 0; // Crash 1: prevent OOB on next WriteToBackend
        _healthy         = false;
        _retryAfterTicks = HfClock.NowTicks + (long)_retryDelayMs * TicksPerMs;
    }
}
```

**Note:** `_stream!.Write` in `WriteToBackend` also calls `FlushBuffer` when buffer is full.
The null guard handles both call sites (WriteToBackend and FlushBackend). Consistent with
`TcpSink.FlushBuffer` pattern: `if (_socket is null) { MarkUnhealthy(); return; }`.

**Immunity check (do not touch):** TcpSink uses `catch (Exception)` in FlushBuffer (:136)
and TryRecoverBackend (:106) — already immune. MmfSink has no FlushBuffer — already immune.

**Commit:** `fix(FileStreamSink): prevent consumer crash on sustained backend failure`

**Test:** add test that simulates IOException loop until buffer fills; assert `IsConsuming`
remains true after recovery. Existing gate: `dotnet test --filter "Category!=Endurance&..."`.

---

## Task 2 — Hot-path improvements

### 2a: RotatingFileSink.ShouldRotate — RDTSC throttle (🟡 47c → ~1c)

**File:** `src/Relay/Sinks/RotatingFileSink.cs:122–127`

`ShouldRotate` calls `HfClock.NowTicks` (RDTSC+LFENCE, ~47c effective on i7-12700) on
**every record**. BDN: ShouldRotate_Predicate = 13.38 ns; WriteToBackend = 34.18 ns.
Day-boundary check needs ≤1s precision — no need for per-record RDTSC.

**Fix:** add `_shouldRotateCounter` (plain `int` field, consumer-thread-only):

```csharp
private int _shouldRotateCounter;

private bool ShouldRotate(int incomingBytes)
{
    if (_currentFileBytes + incomingBytes > _maxBytes) return true;
    if ((++_shouldRotateCounter & 0xFF) == 0)  // every 256 records
        if (HfClock.NowTicks >= _nextDayBoundaryTicks) return true;
    return false;
}
```

Expected: predicate from ~13 ns → ~0.5 ns. Update `BenchInvokeShouldRotate` benchmark
comment to reflect the new expected range. Run `RotatingFileSinkBenchmarks` after to confirm.

### 2b: MpscByteRingBuffer.TryPeek — eliminate recursion (🟡 JIT inlineability)

**File:** `src/Relay/Buffers/MpscByteRingBuffer.cs:193`

`TryPeek` self-recurses when skipping a padding marker (line 193:
`return TryPeek(out payload, out advanceBytes);`). JIT cannot inline recursive methods →
extra stack frame + call cost on every wrapped record. At high throughput with large payloads,
every record that straddles the ring boundary pays this overhead.

**Fix:** replace the recursive tail call with an iterative jump — re-read the new head slot
inline without self-calling. The logic is a simple retry after advancing past the padding:

```csharp
// Instead of: return TryPeek(out payload, out advanceBytes);
// Use: idx = (idx + skip) & _mask; lenField = Volatile.Read(ref header at idx); continue/goto
```

Exact implementation: convert the method body into a `while(true)` loop and `continue` instead
of the recursive call. No semantic change — only control flow restructuring.

**Commit:** `perf: throttle RDTSC in RotatingFileSink.ShouldRotate; eliminate TryPeek recursion`

---

## Task 3 — Documentation fixes (🟡 correctness)

### 3a: MemorySink volatile comment (MemorySink.cs:16)

Remarks: "No CAS — volatile reads/writes on head and tail suffice for the SPSC invariant."
Code: `private long _head; private long _tail;` — plain `long`, zero volatile.
Safe under x86/x64 TSO with single producer, but the claim is factually wrong.

**Fix:** update remark to: "No CAS — plain reads/writes on `_head` and `_tail` suffice under
x86/x64 TSO (single producer). `DrainTo` callers must establish happens-before externally
(e.g., via `Volatile.Read` of the primary's `_healthy` flag) before the first `DrainTo` call."

### 3b: CLAUDE.md Multi2Sink cycle-saving claim (CLAUDE.md)

Current: "~6c saving" vs MultiSink for 2-child case.
BDN (commit ddf2f0b, N=2): Multi2 = 3.31 ns vs Multi = 3.18 ns — Multi2 is **0.45c slower**.
JIT GDV already devirtualizes sealed types; CRTP adds no measured benefit at N=2.

**Fix:** update CLAUDE.md to: "JIT GDV devirtualizes sealed types at call site; BDN (N=2):
Multi2=3.31 ns vs Multi=3.18 ns — no measured advantage. Prefer `Multi2Sink` when compile-time
type binding is explicitly required; do not use for expected performance gains at N=2."

**Commit:** `docs: fix MemorySink volatile comment; correct Multi2Sink BDN finding in CLAUDE.md`

---

## Task 4 — Optional hardening (🟢)

### 4a: MemorySink finalizer

Add `~MemorySink()` that calls `NativeMemory.Free(_buffer)` behind a `_disposed` guard.
Prevents native memory leak when caller omits `using`/`Dispose`.

### 4b: PacketSink._dropCount padding

`PacketSink._dropCount` (plain `long` at :41) shares cache line with `Next` and
`PropagateAfterAccept`. Concurrent `DropCount` monitoring from a second thread causes false
sharing (~200c± contended). `PaddedLong` is currently in `Relay.Buffers` (internal).

**Option chosen:** inline a local `[StructLayout(LayoutKind.Explicit, Size = 128)]` struct
in `PacketSink` — avoids moving `PaddedLong` to a shared namespace for one consumer.

**Commit:** `chore: add MemorySink finalizer; pad PacketSink._dropCount to prevent false sharing`

---

## Verification

- `dotnet test Relay.sln -c Release --filter "Category!=Endurance&Category!=Stress&Category!=Perf"` → 0 failures after each task
- Task 1: new test simulates IOException loop until `_writeBuffer` fills; assert consumer survives and recovers
- Task 2a: run `RotatingFileSinkBenchmarks.ShouldRotate_Predicate` — expect < 2 ns (from 13.38 ns)
- Task 2b: run MpscByteRingBuffer wrap-path tests; assert behavior unchanged
- Task 3: doc review only — no executable verification

## Model Routing

| Task | Model | Reason |
|---|---|---|
| Task 1 (crash fix) | **Sonnet** | Surgical 2-line patch, well-bounded |
| Task 2a (RDTSC throttle) | **Sonnet** | Mechanical counter addition |
| Task 2b (TryPeek iterative) | **Sonnet** | Control-flow rewrite; lock-free — Opus reviews diff |
| Task 3 (docs) | **Fable** | Memory-model semantics (TSO happens-before) + BDN interpretation require Fable validation |
| Task 4 (hardening) | **Sonnet** | Mechanical, well-understood patterns |
| Final diff review | **Opus** | Performance analysis scope (Relay CLAUDE.md) |

> [!DECISION] gh-issue-gate
> Relay has no issue tracker (CLAUDE.md git workflow: "no `<ref>` segment — lib has no issue
> tracker"). Gate is waived. Branch: `fix/260611-robustness-perf-fixes`.
