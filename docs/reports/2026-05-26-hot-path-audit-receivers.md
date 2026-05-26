# Hot-Path Audit — Receivers (post v1.0.2 back-merge)

**Date:** 2026-05-26
**Scope:** `b0ed316..HEAD` — `PacketCallback` delegate, `PacketReceiver` base, `UdpReceiver<TState>`, `TcpReceiver<TState>`, `NamedPipeReceiver<TState>`, `SharedMemorySpscReceiver<TState>`, `RelayBuilder.From.cs`.
**Tier:** `ultra-low-latency` (project-wide; every `.cs` is hot path per `CLAUDE.md`).

## Audited files

| File | Hot-path role |
|---|---|
| `src/Relay/PacketCallback.cs` | Zero-alloc delegate over `ReadOnlySpan<byte>` |
| `src/Relay/PacketReceiver.cs` | Abstract base — caller-driven `Poll()` |
| `src/Relay/Receivers/UdpReceiver.cs` | Data-plane non-blocking UDP receive |
| `src/Relay/Receivers/TcpReceiver.cs` | Management-plane TCP frame receive |
| `src/Relay/Receivers/NamedPipeReceiver.cs` | Management-plane named-pipe receive |
| `src/Relay/Receivers/SharedMemorySpscReceiver.cs` | Hot-path SPSC MMF ring receive |
| `src/Relay/Builder/RelayBuilder.From.cs` | Cold-path factories (4 entry points) |
| `src/Relay/Builder/RelayBuilder.cs` | XML-doc edit only |

## Findings

### F0 — TcpReceiver / NamedPipeReceiver: bogus `frameLen` desyncs the wire  [HIGH — correctness] · **RESOLVED 2026-05-26**

> **Fix:** `fix/260526-receiver-desync-stall`. On invalid `frameLen`, the stream/pipe is torn down and `Poll()` throws `InvalidDataException`. Subsequent `Poll()` returns `false`. Regression tests in `tests/Relay.Tests/Receivers/ReceiverDesyncStallTests.cs`.


**Dim:** D24 (Unit tests), correctness.
**Files:** `TcpReceiver.cs:84-87`, `NamedPipeReceiver.cs:70-73`.

```csharp
if (!ReadExact(header)) return false;
int frameLen = BinaryPrimitives.ReadInt32BigEndian(header);
if (frameLen <= 0 || frameLen > _buffer.Length) return false;   // ← payload NEVER drained
```

When the 4-byte header has already been consumed and the decoded `frameLen` is invalid, `Poll` returns `false` but the supposed payload bytes remain on the wire. Next `Poll` reads 4 bytes from the *middle of that payload*, interprets as length, and the stream is permanently desynced. Frame protocol cannot recover.

Trusted-peer scenario (default `TcpSink`/`NamedPipeSink`): never triggers — sinks always send correct length prefixes. Hostile / mis-versioned / corrupted peer: **permanent silent failure**, no telegraph.

Fix: on bogus `frameLen`, do not return silently. Tear down stream (`Dispose`/null out `_stream`/`_pipe`) and surface `IOException` to the caller. Caller restarts the management session. Add unit test (sender writes header with negative length → receiver rejects + disconnects).

### F0b — SharedMemorySpscReceiver: bogus `frameLen` stalls the consumer  [HIGH — correctness] · **RESOLVED 2026-05-26**

> **Fix:** `fix/260526-receiver-desync-stall`. Constructor now validates `SHM_MAGIC` (`0x4C473200`); `Poll()` throws `InvalidDataException` on invalid `frameLen` instead of stalling. Regression tests in `tests/Relay.Tests/Receivers/ReceiverDesyncStallTests.cs`.


**Dim:** D24 (Unit tests), correctness.
**File:** `SharedMemorySpscReceiver.cs:94`.

```csharp
if (frameLen <= 0 || frameLen > _frameBuffer.Length) return false;
```

`_readIndex` is **not advanced** on rejection. Next `Poll` re-reads the same 4 bytes → same bogus length → indefinite stall. The producer (`SharedMemorySpscSink.Accept` lines 104–135) does not gate on `_readIndex` either, so the producer cannot rescue the consumer by overwriting the slot — and when it eventually wraps over the stuck slot, the consumer reads from a slot that has been overwritten partway, with no integrity tag to detect torn data.

Fix: validate magic + frameLen at receive; on rejection, mark receiver `_disposed` (or expose `IsHealthy`) and throw at next `Poll`. Either way, do not silently stall.

### F1 — TcpReceiver / NamedPipeReceiver: `Poll` can block mid-frame  [HIGH] · **RESOLVED 2026-05-26 (doc)**

> **Fix:** `fix/260526-receiver-desync-stall`. `TcpReceiver` XML rewritten — removed the misleading "non-blocking poll" claim; documents non-blocking at frame boundaries + possible mid-frame block on TCP segmentation; matches the `NamedPipeReceiver` management-plane wording. No behavioural change.


**Dim:** D23 (Syscall), D17 (Exception/blocking semantics).
**Files:** `TcpReceiver.cs:79-95`, `NamedPipeReceiver.cs:65-82`.

`Poll()` checks `DataAvailable` / `IsConnected` ONCE at frame start, then enters `ReadExact` which loops `Stream.Read` synchronously without timeout. If the sender publishes only the 4-byte header (or part of the payload) before stalling, the next `Read` blocks indefinitely on a stream in default blocking mode.

`TcpReceiver` XML doc claims **"Non-blocking poll"** — contradicts behavior. `NamedPipeReceiver` doc states blocking is acceptable for management-plane; consistent with implementation. TCP is the contradiction.

Mitigation options (pick one):
- (a) Set `_stream.ReadTimeout = N`, catch `IOException` with `SocketException(WSAETIMEDOUT/EAGAIN)`, stash partial bytes; finish on next Poll. Real non-blocking semantics.
- (b) Use frame-state machine: header (4B) read non-blocking; once header complete, read payload non-blocking; advance state on partial reads.
- (c) Document that mid-frame blocking is possible and that this receiver is for "management-plane" use only (matches NamedPipe doc).

Recommendation: (c) for v1.0.3 — already implicit in code remark `// [management-plane]`. Update public XML doc to remove "non-blocking" wording. (a)/(b) for future zero-copy data-plane.

### F2 — SharedMemorySpscReceiver: zero-copy is UNSAFE under current protocol  [WITHDRAWN]

**Dim:** D1, D9, D7. Initial recommendation withdrawn after sink re-read.

`SharedMemorySpscSink.Accept` (lines 104–135) does **not** gate on `_readIndex`: the producer advances `WriteIdx` unconditionally and silently overwrites unread slots on overrun. The current receiver design (copy into `_frameBuffer` then advance `_readIndex` then callback) is what guarantees the callback sees a consistent packet even when the producer overruns mid-callback. Zero-copy would expose the callback span to torn data from a concurrent producer write.

The copy is **the safety mechanism**, not an oversight. Keep as-is. Adjacent finding F-SHM below describes the underlying protocol gap.

### F-SHM — SharedMemorySpscSink lacks producer-side overrun gate  [MEDIUM — adjacent / out-of-scope but blocks F2]

**Dim:** D21 (Lock-Free / Contention), correctness.
**File:** `SharedMemorySpscSink.cs:104-135` (pre-existing, not in audit delta).

`Accept` reads `WriteIdx`, advances unconditionally, writes payload, publishes. Never compares `(WriteIdx + frameLen)` against `ReadIdx`. Consequences:
- Silent data loss on consumer-lag (acceptable per Log2 wire compat).
- Consumer's `_readIndex == writeIndex` empty-check is **ambiguous** between "empty" and "wrapped exactly one ring" — at saturation the consumer can deadlock or read corrupted frames.
- Without integrity tag (CRC/sentinel), receiver cannot detect torn frames from in-flight overruns.

Out of scope for this audit (sink is pre-v1.0.2 code). Flagged because F2's zero-copy depends on adding this gate or a per-slot publish bit. Track separately.

### F3 — NamedPipeReceiver: POH-pinned 4-byte `_header` field  [MEDIUM] · **RESOLVED 2026-05-26**

> **Fix:** `fix/260526-receiver-desync-stall`. Removed `_header` field and its POH allocation; `Poll` now uses `Span<byte> header = stackalloc byte[4]`, mirroring `TcpReceiver`. One less per-instance pinned object on the LOH/POH; cache-resident span on the hot path.


**Dim:** D9 (Zero-allocation), D5 (Cache locality).
**File:** `NamedPipeReceiver.cs:24, 42, 70`.

```csharp
private readonly byte[] _header;  // 4-byte length prefix; POH-pinned
...
_header = GC.AllocateUninitializedArray<byte>(4, pinned: true);
```

A 4-byte POH allocation pays the full pinned-object header overhead (~24 B on x64) plus a heap dereference per access. The sibling `TcpReceiver.Poll` (line 83) uses `stackalloc byte[4]` — strictly faster (zero load, cache-resident).

Fix: replace `_header` field with `Span<byte> header = stackalloc byte[4];` inside `Poll`, mirroring TCP.

### F4 — All Receivers: no BenchmarkDotNet harness  [MEDIUM]

**Dim:** D27 (Benchmark tests), D29 (Benchmark discipline), D31 (Regression detection).
**Files:** `benchmarks/Relay.Benchmarks/` — no `*Receiver*Benchmark.cs`.

`benchmarks/baselines/results/` covers Sinks (`FileSink`, `TcpSink`, `ChainBenchmark`) but no Receiver-side counterpart. Cycle budget for `UdpReceiver.Poll` / `TcpReceiver.Poll` / `SharedMemorySpscReceiver.Poll` is undocumented and unmeasured.

Required: add `Receivers/` benchmark subdir with at minimum one `[MemoryDiagnoser]` benchmark per receiver. Baseline before next release.

### F5 — UdpReceiver: two syscalls per successful packet  [LOW / by-design]

**Dim:** D23 (Syscall).
**File:** `UdpReceiver.cs:59-62`.

`_socket.Poll(0, SelectRead)` → `_socket.Receive(...)` = two syscalls when data is present (`select` then `recvfrom`). For sustained inbound traffic where every Poll succeeds, the `select` is wasted work.

Alternative: rely on non-blocking `Receive` throwing `SocketException(WouldBlock)`. Cost trade-off:
- Bursty workload (most polls = empty): current pattern wins; `Poll(0)` is ~1–3 µs, exception throw is ~10–100 µs.
- Saturated workload: receive-first pattern wins.

Coordination/management plane is bursty by nature — current choice is correct. Document the assumption in XML so future contributors don't "optimize" the wrong way.

### F6 — TcpReceiver / NamedPipeReceiver: `ReadExact` swallows EOS  [LOW]

**Dim:** D17 (Exception cost), D24 (Unit tests).
**Files:** `TcpReceiver.cs:97-107`, `NamedPipeReceiver.cs:84-94`.

`ReadExact` returns `false` on `read == 0`, which conflates "no data" with "peer disconnect". Caller (`Poll`) returns `false` and the next Poll repeats. No reconnect path; no exception telegraph.

Acceptable for management-plane (caller restarts session), but should be documented. Tests do not currently assert disconnect-mid-frame behavior — add coverage.

### F7 — SharedMemorySpscReceiver: missing magic/sanity check  [LOW]

**Dim:** D24 (Unit tests), defensive correctness.
**File:** `SharedMemorySpscReceiver.cs:71-74`.

Constructor reads `_dataCapacity = *(int*)(_ptr + DATA_CAP_OFF)` without verifying the magic at offset 0 (`0x4C473200` per `SharedMemorySpscSink.cs:49`). Opening a foreign MMF gives garbage capacity; subsequent `% _dataCapacity` may divide by zero or overflow.

Fix: validate magic in constructor; throw `InvalidDataException` on mismatch. Cold path — free.

### F8 — SharedMemorySpscReceiver: `% _dataCapacity` per Poll  [INFO]

**Dim:** D7 (Memory access patterns).

3 modulos per `Poll` (`payloadStart`, `_readIndex` advance, `ReadRing` chunk). Cannot be replaced with `& (capacity-1)` because `SharedMemorySpscSink` defines `_dataCapacity = totalCapacity - HEADER_SIZE`, which is not power-of-2 for typical sizes (e.g. 4 MiB − 128 B). A protocol change would be required — out of scope for this audit. Document the design choice (Log2 wire compatibility).

### F9 — PacketCallback<TState> delegate design  [POSITIVE]

**Dim:** D14 (Closures/Delegates), D11 (Boxing).
**File:** `PacketCallback.cs`.

`delegate void PacketCallback<TState>(TState, ReadOnlySpan<byte>)` — the canonical fix for the language constraint that bans `Action<ReadOnlySpan<byte>>`. `TState` threaded as a parameter eliminates closure allocation; static lambdas (`static (s, f) => ...`) compile to a single delegate instance. Excellent.

## Summary table

| # | Sev | Component | Dim | Action |
|---|---|---|---|---|
| F0 | H | Tcp/NamedPipe | corr | Bogus frameLen desyncs wire — disconnect on rejection, add test |
| F0b | H | SharedMemorySpsc | corr | Bogus frameLen stalls consumer — fail-fast/throw, add test |
| F1 | H | Tcp/NamedPipe | D23/D17 | Update XML doc or implement non-blocking state machine |
| F2 | — | SharedMemorySpsc | D1/D9 | **WITHDRAWN** — zero-copy unsafe without F-SHM gate |
| F-SHM | M | SharedMemorySpscSink (pre-existing) | D21 | Track separately: add producer-side overrun gate |
| F3 | M | NamedPipe | D9 | Replace POH `_header` field with `stackalloc byte[4]` |
| F4 | M | Udp + SharedMemorySpsc only | D27 | Add narrowed BDN harness (Tcp/Pipe = management-plane, defer) |
| F5 | L | Udp | D23 | Document `Poll(0)+Receive` rationale in XML |
| F6 | L | Tcp/NamedPipe | D17 | Document EOS-as-false; add disconnect-mid-frame test |
| F7 | L | SharedMemorySpsc | D24 | Validate magic in ctor (subsumed by F0b fix) |
| F8 | INFO | SharedMemorySpsc | D7 | Document non-power-of-2 capacity (protocol-locked) |
| F9 | + | PacketCallback | D14 | Keep as-is |

## Per-dimension scores

| Dim | Score (1–10) | Notes |
|---|---|---|
| 1. CPU Cycles | 7 | Lean per-receiver; SHM has avoidable copy |
| 2. Branch Prediction | 9 | Early returns predictable |
| 3. SIMD / Vectorization | n/a | Receive path has no SIMD opportunity |
| 4. Bounds Check Elimination | 8 | Span idioms correct |
| 5. Cache Locality | 8 | Field count modest; POH buffers cache-friendly |
| 6. False Sharing | 9 | SPSC; no contended fields |
| 7. Memory Access Patterns | 7 | Modulo per access in SHM ring |
| 8. Indirection Count | 8 | Sealed receiver classes; one delegate dispatch |
| 9. Zero-Allocation | 6 | F2 avoidable copy, F3 wasteful POH `_header` |
| 10. GC Pressure | 9 | POH for long-lived; no Gen0 churn |
| 11. Boxing / Unboxing | 10 | Generic TState — no boxing |
| 12. Pooling | 9 | One buffer per instance reused |
| 13. Value Types | 9 | ReadOnlySpan<byte> used correctly |
| 14. Closures & Delegates | 10 | PacketCallback<TState> = canonical zero-alloc form |
| 15. Async Overhead | 10 | None |
| 16. String Handling | 10 | None on hot path |
| 17. Exception Cost | 5 | Mid-frame Read may throw; no catch in Poll |
| 18. Readonly / Immutability | 8 | Fields readonly where possible |
| 19. Inlining & Devirt | 9 | `[AggressiveInlining]` on Poll, ReadRing |
| 20. Static / Const Propagation | 8 | SHM header offsets are const |
| 21. Lock-Free / Contention | 9 | Volatile.Read/Write only |
| 22. Thread Affinity & NUMA | 7 | Caller-owned; receiver doesn't pin |
| 23. Syscall / Kernel Transitions | 5 | F1 blocking risk, F5 double syscall, F8 mandatory |
| 24. Unit Tests | 7 | Tests exist; no alloc/disconnect assertions |
| 25. Integration Tests | 7 | RelayBuilderReceiverTests wires factories |
| 26. Stress Tests | 4 | No long-running receiver stress; no backpressure |
| 27. Benchmark Tests | 2 | No BDN for receivers |
| 28. Hot Path Identification | 9 | XML docs explicit on hot vs management plane |
| 29. Benchmark Discipline | 3 | No baseline for receivers |
| 30. Profiling Evidence | 5 | Existing benchmark dir; no receiver profile |
| 31. Regression Detection | 4 | No CI gate; no receiver baseline |

## Verdict

**BLOCKED ON v1.0.3 — F0 + F0b are correctness bugs.**

Mandatory before tag:
1. F0 — Tcp/NamedPipe disconnect on bogus frameLen, with unit test.
2. F0b — SharedMemorySpsc fail-fast on bogus frameLen, with unit test.
3. F1 — XML doc: remove "non-blocking" claim from `TcpReceiver` (or implement true non-blocking).
4. F4 — BDN harness for `UdpReceiver` + `SharedMemorySpscReceiver` only. Tcp/Pipe BDN deferred (management-plane).

Recommended next release: F3 stackalloc fix, F5/F6 docs, F-SHM protocol gate (separate ticket).

## Cycle budget — measured (2026-05-26, BDN v0.13.12)

Host: Windows 11, .NET 9.0.14, Intel i7-12700 (12P/8E cores), X64 RyuJIT AVX2.
Source: `benchmarks/Relay.Benchmarks/Receivers/{UdpReceiverBenchmark,SharedMemorySpscReceiverBenchmark}.cs`.

### UdpReceiver

| Method | Payload | Mean | StdDev | Allocated |
|---|---:|---:|---:|---:|
| Poll_Empty | — | 858 ns | 10.8 ns | 0 B |
| Roundtrip_PerFrame (send+receive) | 128 B | 4.69 µs | 169 ns | 72 B* |

*Allocation comes from `UdpClient.Send` (boxing of `IPEndPoint` arg), not the receiver path. Receiver `Poll_Empty` is zero-alloc as designed.

`Poll_Empty` cost ≈ 2 750 cycles @ 3.2 GHz — dominated by `Socket.Poll(0, SelectRead)` syscall (~1 syscall + select machinery). Matches F5 commentary: bursty-workload assumption holds — `Poll(0)` is the right primitive when most polls return empty.

### SharedMemorySpscReceiver

| Method | Payload | Mean | StdDev | Allocated |
|---|---:|---:|---:|---:|
| Poll_Empty | 64 B | 0.66 ns | 0.03 ns | 0 B |
| Poll_Empty | 256 B | 0.73 ns | 0.05 ns | 0 B |
| Poll_Empty | 1024 B | 0.65 ns | 0.01 ns | 0 B |
| Roundtrip_PerFrame (sink Enqueue + receiver Poll) | 64 B | 20.7 ns | 0.33 ns | 0 B |
| Roundtrip_PerFrame | 256 B | 24.3 ns | 0.15 ns | 0 B |
| Roundtrip_PerFrame | 1024 B | 38.4 ns | 0.30 ns | 0 B |

`Poll_Empty` = single `Volatile.Read(WriteIdx)` + `_readIndex == writeIndex` branch → essentially free (~2 cycles).

`Roundtrip_PerFrame` includes BOTH:
- Sink `Accept`: `Volatile.Read(WriteIdx)` + memcpy(payload) + `Thread.MemoryBarrier` + `Volatile.Write(WriteIdx)`.
- Receiver `Poll`: `Volatile.Read(WriteIdx)` + memcpy(payload → `_frameBuffer`) + `Volatile.Write(ReadIdx)` + callback.

Two memcpys of identical size each. Linear scaling (~18 ns delta from 64 B to 1024 B = ~36 GB/s effective memcpy throughput — within expected L1/L2 range). Confirms F2 analysis: zero-copy would save one memcpy ≈ 9 ns @ 1 KiB. Withdrawn because protocol provides no overrun gate.

### Regression baseline

The numbers above are the v1.0.3 baseline. Add CI gate when F-SHM (protocol overrun gate) lands and the F2 zero-copy fix becomes safe to apply. Until then, regression checks are manual against this report.

## Verdict — final

**PASS for v1.0.3 — F0, F0b, F1, F3 all resolved 2026-05-26 on `fix/260526-receiver-desync-stall`. F-SHM (sink producer-side overrun gate) tracked separately as adjacent, pre-existing.**

Numbers are good (zero-alloc holds, sub-100 ns roundtrip for SHM). Correctness bugs closed; perf baseline held; documentation aligned with behavior.
