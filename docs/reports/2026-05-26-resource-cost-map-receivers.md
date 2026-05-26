# Resource Cost Map — receivers (post v1.0.2)

_generated 2026-05-26 · model x64-zen4 v1 · library mode · static estimate, not benchmark_

## 0. Header

- scope: `src/Relay/PacketCallback.cs`, `src/Relay/PacketReceiver.cs`, `src/Relay/Receivers/*.cs`, `src/Relay/Builder/RelayBuilder.From.cs`
- mode: **library** (`Relay.dll`; no `Main`; rates `unknown` unless integrator hints)
- entry points: 4 hot `Poll()` + 4 ctor + 4 factory + 2 accept/wait — 14 total (hinted: 0, unknown: 14)
- nodes analyzed: 27
- blind subgraphs: `PacketSink.Enqueue` (polymorphic; ≥8 concrete impls); `Socket.Poll`/`Socket.Receive`/`Stream.Read`/`NamedPipeServerStream.Read` (kernel boundary)
- calibration: BDN 2026-05-26 (`docs/reports/2026-05-26-hot-path-audit-receivers.md` §Cycle budget)
- budget: n/a (library mode); ULTRA-HOT share limit n/a

## 1. Per-Entry Cost Table _(library mode replaces §1 Budget Summary)_

| # | Entry | file:line | cycles/call | bytes/call | calls/s | drivers |
|---|---|---|---:|---:|---|---|
| 1 | `SharedMemorySpscReceiver<TState>.Poll` (no-frame) | `Receivers/SharedMemorySpscReceiver.cs:82` | ~2 | 0 | unknown | `Volatile.Read` + branch |
| 2 | `SharedMemorySpscReceiver<TState>.Poll` (1×64B frame, no-wrap) | `Receivers/SharedMemorySpscReceiver.cs:82` | ~37 (BDN 20.7 ns roundtrip ÷ 2 sides) | 0 | unknown | 2× `ReadRing` memcpy + 2× `Volatile.Read/Write` |
| 3 | `SharedMemorySpscReceiver<TState>.Poll` (1×1024B frame, no-wrap) | `Receivers/SharedMemorySpscReceiver.cs:82` | ~69 (BDN 38.4 ns ÷ 2) | 0 | unknown | memcpy dominates (~36 GB/s effective L1/L2) |
| 4 | `UdpReceiver<TState>.Poll` (no-data) | `Receivers/UdpReceiver.cs:57` | ~3 088 (BDN 858 ns) | 0 | unknown | `Socket.Poll(0,SelectRead)` syscall |
| 5 | `UdpReceiver<TState>.Poll` (data, 128B) | `Receivers/UdpReceiver.cs:57` | ~4 800 (model) | 0 | unknown | `Socket.Poll` syscall + `Socket.Receive` syscall |
| 6 | `TcpReceiver<TState>.Poll` (no-data) | `Receivers/TcpReceiver.cs:79` | ~2 050 (model) | 0 | unknown | `_stream.DataAvailable` syscall + null-branch |
| 7 | `TcpReceiver<TState>.Poll` (data, no-segmentation) | `Receivers/TcpReceiver.cs:79` | ~6 100 (model) | 0 | unknown | `DataAvailable` + 2× `Stream.Read` syscalls |
| 8 | `NamedPipeReceiver<TState>.Poll` (no-data) | `Receivers/NamedPipeReceiver.cs:65` | ~50 (model) | 0 | unknown | `IsConnected` (cached user-side) + `_pipe.Read` blocks if connected — see F1 |
| 9 | `NamedPipeReceiver<TState>.Poll` (data) | `Receivers/NamedPipeReceiver.cs:65` | ~5 100 (model) | 0 | unknown | 2× `_pipe.Read` syscalls |
| 10 | `PacketReceiver` ctor / Dispose | various | n/a | varies | unknown | cold path |
| 11 | `RelayBuilder.From*` factories | `Builder/RelayBuilder.From.cs:21,35,51,65` | ~6 | per-recv ctor | unknown | cold; one allocation per call |
| 12 | `TcpReceiver.Accept` | `Receivers/TcpReceiver.cs:62` | unknown | varies | unknown | blocking accept; cold |
| 13 | `NamedPipeReceiver.WaitForConnection` | `Receivers/NamedPipeReceiver.cs:58` | unknown | 0 | unknown | blocking; cold |
| 14 | `PacketCallback<TState>` invoke | `PacketCallback.cs:16` | ~8 | 0 | unknown | delegate invoke; sealed-direct after GDV |

drivers column points at the cycle-dominant operation; absolute calls/s is the integrator's hint.

## 2. Top entries (by cycles/call · library mode rank)

| # | weight (rank) | cycles/call | bytes/call | symbol | file:line | dispatch |
|---|---:|---:|---:|---|---|---|
| 1 | rank-1 | ~6 100 | 0 | `TcpReceiver<TState>.Poll` (data path) | `Receivers/TcpReceiver.cs:79` | sealed-direct |
| 2 | rank-2 | ~5 100 | 0 | `NamedPipeReceiver<TState>.Poll` (data path) | `Receivers/NamedPipeReceiver.cs:65` | sealed-direct |
| 3 | rank-3 | ~4 800 | 0 | `UdpReceiver<TState>.Poll` (data path) | `Receivers/UdpReceiver.cs:57` | sealed-direct |
| 4 | rank-4 | ~3 088 | 0 | `UdpReceiver<TState>.Poll` (empty) | `Receivers/UdpReceiver.cs:57` | sealed-direct |
| 5 | rank-5 | ~2 050 | 0 | `TcpReceiver<TState>.Poll` (empty) | `Receivers/TcpReceiver.cs:79` | sealed-direct |
| 6 | rank-6 | ~69 | 0 | `SharedMemorySpscReceiver<TState>.Poll` 1 KiB | `Receivers/SharedMemorySpscReceiver.cs:82` | sealed-direct |
| 7 | rank-7 | ~37 | 0 | `SharedMemorySpscReceiver<TState>.Poll` 64 B | `Receivers/SharedMemorySpscReceiver.cs:82` | sealed-direct |
| 8 | rank-8 | ~50 | 0 | `NamedPipeReceiver<TState>.Poll` (empty) | `Receivers/NamedPipeReceiver.cs:65` | sealed-direct |
| 9 | rank-9 | ~8 | 0 | `PacketCallback<TState>` invoke | `PacketCallback.cs:16` | delegate-invoke (devirt-friendly) |
| 10 | rank-10 | ~2 | 0 | `SharedMemorySpscReceiver<TState>.Poll` empty | `Receivers/SharedMemorySpscReceiver.cs:82` | sealed-direct |

## 3. Hot Tree (per receiver)

```
UdpReceiver<TState>.Poll                       Receivers/UdpReceiver.cs:57   ~4800  unknown  0     (data path)
├─ Socket.Poll(0, SelectRead)                  Receivers/UdpReceiver.cs:59   ~2000  unknown  0     syscall
├─ stackalloc byte[1432]                       Receivers/UdpReceiver.cs:61   0      unknown  0     stack
├─ Socket.Receive(frame, None)                 Receivers/UdpReceiver.cs:62   ~2000  unknown  0     syscall (recvfrom)
├─ PacketCallback<TState> invoke               Receivers/UdpReceiver.cs:65   ~8     unknown  0     delegate
└─ Next?.Enqueue(frame)                        Receivers/UdpReceiver.cs:66   ~8±    unknown  0     polymorphic (PacketSink)

TcpReceiver<TState>.Poll                       Receivers/TcpReceiver.cs:79   ~6100  unknown  0     (data path)
├─ NetworkStream.DataAvailable                 Receivers/TcpReceiver.cs:81   ~2000  unknown  0     syscall (Available)
├─ stackalloc byte[4]                          Receivers/TcpReceiver.cs:83   0      unknown  0     stack
├─ ReadExact(header)                           Receivers/TcpReceiver.cs:84   ~2000  unknown  0     1× Stream.Read syscall
├─ BinaryPrimitives.ReadInt32BigEndian         Receivers/TcpReceiver.cs:86   ~5     unknown  0     bswap+load
├─ frameLen validation                         Receivers/TcpReceiver.cs:87   ~2     unknown  0     2 branches — F0 silent-desync gate
├─ _buffer.AsSpan(0, frameLen)                 Receivers/TcpReceiver.cs:89   ~3     unknown  0     stack span
├─ ReadExact(payload)                          Receivers/TcpReceiver.cs:90   ~2000± unknown  0     N× Stream.Read; blocks mid-frame (F1)
├─ PacketCallback<TState> invoke               Receivers/TcpReceiver.cs:92   ~8     unknown  0     delegate
└─ Next?.Enqueue(payload)                      Receivers/TcpReceiver.cs:93   ~8±    unknown  0     polymorphic

NamedPipeReceiver<TState>.Poll                 Receivers/NamedPipeReceiver.cs:65  ~5100 unknown 0   (data path)
├─ NamedPipeServerStream.IsConnected           Receivers/NamedPipeReceiver.cs:67  ~50  unknown  0   user-side flag
├─ ReadExact(_header)                          Receivers/NamedPipeReceiver.cs:70  ~2000 unknown 0   syscall; F3: _header is POH 4B (heap load)
├─ BinaryPrimitives.ReadInt32BigEndian         Receivers/NamedPipeReceiver.cs:72  ~5   unknown  0   bswap+load
├─ frameLen validation                         Receivers/NamedPipeReceiver.cs:73  ~2   unknown  0   2 branches — F0 silent-desync gate
├─ ReadExact(payload)                          Receivers/NamedPipeReceiver.cs:77  ~2000± unknown 0  N× Pipe.Read; mid-frame block (F1)
├─ PacketCallback<TState> invoke               Receivers/NamedPipeReceiver.cs:79  ~8   unknown  0   delegate
└─ Next?.Enqueue(payload)                      Receivers/NamedPipeReceiver.cs:80  ~8±  unknown  0   polymorphic

SharedMemorySpscReceiver<TState>.Poll          Receivers/SharedMemorySpscReceiver.cs:82  ~37 unknown 0  (64B, no-wrap)
├─ Volatile.Read WriteIdx                      Receivers/SharedMemorySpscReceiver.cs:84  ~6   unknown  0  acq load on x64
├─ branch _readIndex == writeIndex             Receivers/SharedMemorySpscReceiver.cs:85  ~1   unknown  0
├─ stackalloc byte[4] + ReadRing(lenBuf)       Receivers/SharedMemorySpscReceiver.cs:90-91  ~4 unknown 0  L1 hit
├─ BinaryPrimitives.ReadInt32BigEndian         Receivers/SharedMemorySpscReceiver.cs:92  ~5   unknown  0
├─ frameLen validation                         Receivers/SharedMemorySpscReceiver.cs:94  ~2   unknown  0   F0b silent-stall gate
├─ % _dataCapacity (×3 sites)                  Receivers/SharedMemorySpscReceiver.cs:97,102,119  ~20 unknown 0  int divide; F8 protocol-locked
├─ ReadRing(payload)                           Receivers/SharedMemorySpscReceiver.cs:99  ~size  unknown  0  memcpy (linear in frameLen; F2 withdrawn)
├─ Volatile.Write ReadIdx                      Receivers/SharedMemorySpscReceiver.cs:103 ~1   unknown  0  store
├─ PacketCallback<TState> invoke               Receivers/SharedMemorySpscReceiver.cs:105 ~8   unknown  0  delegate
└─ Next?.Enqueue(payload)                      Receivers/SharedMemorySpscReceiver.cs:106 ~8±  unknown  0  polymorphic
```

## 4. Allocation Map (top by bytes/call)

| # | bytes/call | bytes/s | symbol | file:line | kind |
|---|---:|---:|---|---|---|
| 1 | 0 | unknown | `UdpReceiver.Poll` | `Receivers/UdpReceiver.cs:57` | `stackalloc 1432B` — stack |
| 2 | 0 | unknown | `TcpReceiver.Poll` | `Receivers/TcpReceiver.cs:79` | `stackalloc 4B` header + POH `_buffer` reused |
| 3 | 0 | unknown | `NamedPipeReceiver.Poll` | `Receivers/NamedPipeReceiver.cs:65` | POH `_header` (F3 waste) + POH `_buffer` reused |
| 4 | 0 | unknown | `SharedMemorySpscReceiver.Poll` | `Receivers/SharedMemorySpscReceiver.cs:82` | `stackalloc 4B` + POH `_frameBuffer` reused |
| 5 | 0 (BDN-verified) | unknown | All Poll paths | — | BDN `[MemoryDiagnoser]` shows 0 B/op on Poll_Empty and Roundtrip for SHM 64/256/1024 |

Constructor allocations (cold):
- `UdpReceiver` ctor: 1× `Socket` (~120 B Gen0).
- `TcpReceiver` ctor: 1× POH `byte[bufferSize]` (default 65 536 B) + `TcpListener` (~64 B).
- `NamedPipeReceiver` ctor: 1× POH `byte[4]` (`_header` — F3 wasteful 24 B+ pin) + 1× POH `byte[bufferSize]`.
- `SharedMemorySpscReceiver` ctor: 1× POH `byte[maxFrameSize]` + `MemoryMappedFile` handle.

## 5. Anti-Pattern Offenders (library mode — no ULTRA-HOT tier without rate hint)

| Severity | Rule | symbol | file:line | evidence |
|---|---|---|---|---|
| info | syscall-per-call | `UdpReceiver.Poll` data path | `Receivers/UdpReceiver.cs:59,62` | 2 syscalls per successful packet (F5; by-design for bursty workload) |
| info | syscall-per-call | `TcpReceiver.Poll` data path | `Receivers/TcpReceiver.cs:81,84,90` | 3+ syscalls per frame; F1 mid-frame blocking |
| info | syscall-per-call | `NamedPipeReceiver.Poll` data path | `Receivers/NamedPipeReceiver.cs:67,70,77` | 3+ syscalls per frame; F1 mid-frame blocking |
| warn | wasteful-POH-pin | `NamedPipeReceiver._header` | `Receivers/NamedPipeReceiver.cs:24,42,70` | F3: 4-byte POH alloc; sibling TCP uses `stackalloc` |
| warn | correctness-not-perf | `TcpReceiver.Poll` bogus frameLen | `Receivers/TcpReceiver.cs:87` | F0: header consumed, payload not — permanent wire desync |
| warn | correctness-not-perf | `NamedPipeReceiver.Poll` bogus frameLen | `Receivers/NamedPipeReceiver.cs:73` | F0: same as TCP |
| warn | correctness-not-perf | `SharedMemorySpscReceiver.Poll` bogus frameLen | `Receivers/SharedMemorySpscReceiver.cs:94` | F0b: `_readIndex` not advanced → infinite stall |
| info | modulo-on-non-pow2 | `SharedMemorySpscReceiver.Poll` | `Receivers/SharedMemorySpscReceiver.cs:97,102,119` | F8: 3× `% _dataCapacity` per Poll; protocol-locked (Log2 wire compat) |
| info | virtual-dispatch | `Next?.Enqueue` polymorphic | all `Poll` tail calls | `PacketSink` base; concrete sink unknown at receiver site (GDV may kick in if pipeline is sealed) |

No `block` severity — `block` requires ULTRA-HOT tier which needs a rate hint not present in library mode.

## 6. Cache-Line Report

| symbol | sizeof | issue | file:line |
|---|---:|---|---|
| `UdpReceiver<TState>` instance | ~40 B (5 ref slots) | one cache line | `Receivers/UdpReceiver.cs:21` |
| `TcpReceiver<TState>` instance | ~64 B (7 ref slots + int) | exactly one cache line | `Receivers/TcpReceiver.cs:18` |
| `NamedPipeReceiver<TState>` instance | ~48 B (6 ref slots) | one cache line | `Receivers/NamedPipeReceiver.cs:19` |
| `SharedMemorySpscReceiver<TState>` instance | ~80 B (8 ref + 2 int + bool) | spans 2 lines | `Receivers/SharedMemorySpscReceiver.cs:27` |

No `T : unmanaged` payload structs introduced; cache-line alignment assertion (`SinkConstraints.AssertCacheLineAligned<T>`) does not apply to the receiver delta — class layout only.

## 7. Syscalls & Kernel Boundaries

| symbol | file:line | kind | calls/s | cycles |
|---|---|---|---|---:|
| `Socket.Poll(0, SelectRead)` | `Receivers/UdpReceiver.cs:59` | `select`/WSAEventSelect probe | unknown (per Poll) | ~2000 |
| `Socket.Receive(Span<byte>, None)` | `Receivers/UdpReceiver.cs:62` | `recvfrom` | unknown (per data Poll) | ~2000 |
| `NetworkStream.DataAvailable` | `Receivers/TcpReceiver.cs:81` | `ioctlsocket(FIONREAD)` (Windows) | unknown (per Poll) | ~2000 |
| `NetworkStream.Read` | `Receivers/TcpReceiver.cs:102` | `recv` | unknown (≥2 per frame) | ~2000 each |
| `NamedPipeServerStream.IsConnected` | `Receivers/NamedPipeReceiver.cs:67` | user-side flag (no syscall) | unknown (per Poll) | ~50 |
| `NamedPipeServerStream.Read` | `Receivers/NamedPipeReceiver.cs:89` | `ReadFile` on pipe | unknown (≥2 per frame) | ~2000 each |
| `MemoryMappedFile.OpenExisting` (ctor only) | `Receivers/SharedMemorySpscReceiver.cs:63` | `OpenFileMapping` | once | ~5000 |
| `Volatile.Read/Write` on MMF | `Receivers/SharedMemorySpscReceiver.cs:84,103` | user-mode atomic load/store | per Poll | ~6 / ~1 |

Notable: SHM receiver has **zero syscalls** on the hot path. UDP minimum 1 syscall (no data) / 2 (data). TCP minimum 1 syscall (no data) / 3+ (data). Named pipe minimum 0 syscalls (no data; IsConnected is user-side) / 2+ (data).

## 8. Blind Subgraphs

| symbol | file:line | reason | hint-key |
|---|---|---|---|
| `PacketSink.Enqueue` (polymorphic) | `PacketSink.cs:66` | ≥8 concrete impls (`MemorySink`, `FileStreamSink`, `TcpSink`, `MmfSink`, `NullSink`, `ForkSink`, `MultiSink`, queue sinks); GDV-dependent | `dispatch.next_concrete: <type-name>` |
| `Socket.Poll` / `Socket.Receive` / `Stream.Read` / `NamedPipeServerStream.Read` | external | kernel boundary; per-call cost depends on packet size, queue depth, IRQ rate | n/a |
| `PacketCallback<TState>` body | user-supplied | callback content is integrator's code; cost set by them | `delegates.<callback-name>.cycles_per_call` |
| `MemoryMappedViewAccessor.AcquirePointer` | `Receivers/SharedMemorySpscReceiver.cs:67` | ctor-only; cold | n/a |

## 9. Delta vs prior — _no prior cost map for this scope; this file is the first snapshot._

---

## Caveats

- Library mode: all `calls/s` are `unknown`. Ranking is by `cycles/call`. Integrator hints needed to compute absolute `weight/s`.
- Cycle estimates calibrated to BDN where measured (SHM Poll, UDP Poll); modelled from cost table where not (TCP, NamedPipe). TCP/NamedPipe model error band: ±30%.
- `Next?.Enqueue` cost shown as `~8±` is just the dispatch jump; the callee's body adds 30–500 c depending on which sink terminates the chain (terminal `NullSink` ≈ 5 c, terminal `MmfSink` ≈ 80 c, terminal queue sink ≈ 30 c). Integrators should re-rank with their pipeline's concrete tail.
- `% _dataCapacity` cost (20 c) is the integer-divide path. JIT cannot strength-reduce because `_dataCapacity` is a field, not constant, and the protocol bars power-of-2 (header subtraction). Acceptable per F8.
