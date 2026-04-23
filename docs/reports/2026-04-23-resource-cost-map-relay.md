# Resource Cost Map — Relay
_generated 2026-04-23 · model x64-golden-cove v1 · static estimate, not benchmark · library mode_

## 0. Header
- scope: Relay .NET 9 library (`src/Relay/**/*.cs`), 15 source files
- entry count: 28 public methods/props across 10 public types
- nodes analyzed: 43 (public + internal hot-path)
- blind subgraphs: 4 (`Predicate<T>` delegate body, backend OS I/O, JIT devirtualization assumption, `VirtualLock` NT kernel path)
- library mode: no `Main`; `calls/s` unknown; ranking by `cycles/call` desc

## 1. Per-Entry Cost Table
| Entry | cycles/call | bytes/call | tier | drivers | file:line |
|---|---|---|---|---|---|
| `SpscQueuePipe.Start` | ~1.05e5 | 250 | RARE | `GC.AllocateArray` (via ring ctor path here N/A — already allocated), `RelayMemory.PreFaultAndLock` + `VirtualLock` syscall, `Thread.Start` (~1e5c) | src/Relay/SpscQueuePipe.cs:75 |
| `FileStreamPipe.ctor` | ~5.3e3 | 2.62e5 | COLD | `GC.AllocateArray<byte>(4096*64, pinned)` (250c + 256KB zero), `OpenStream` (FileStream ctor syscall ~5000c) | src/Relay/Pipes/FileStreamPipe.cs:34 |
| `TcpPipe.ctor` | ~5.0e3 | 2.62e5 | COLD | `GC.AllocateArray<byte>` pinned + `TcpClient.Connect` (multi-syscall 3-way handshake, dominant) | src/Relay/Pipes/TcpPipe.cs:33 |
| `MmfPipe.ctor` | ~5.0e3 | 64 | COLD | `MemoryMappedFile.CreateFromFile` + `CreateViewAccessor` (kernel mapping syscalls) | src/Relay/Pipes/MmfPipe.cs:32 |
| `SpscQueuePipe.Stop` | ~2e3 | 0 | WARM | `Thread.Join` + `Volatile.Write`; wakes OS scheduler | src/Relay/SpscQueuePipe.cs:90 |
| `FileStreamPipe.FlushBackend` (non-empty) | ~5.0e3 | 0 | COLD | `FileStream.Write` syscall 5000c | src/Relay/Pipes/FileStreamPipe.cs:58 |
| `TcpPipe.FlushBackend` (non-empty) | ~2.0e3 | 0 | WARM | `NetworkStream.Write` syscall 2000c | src/Relay/Pipes/TcpPipe.cs:59 |
| `MmfPipe.FlushBackend` | ~2.0e3 | 0 | WARM | `MemoryMappedViewAccessor.Flush` 2000c | src/Relay/Pipes/MmfPipe.cs:53 |
| `SpscQueuePipe.Dispose` | ~2e3 | 0 | WARM | delegates to `Stop(5000)` | src/Relay/SpscQueuePipe.cs:122 |
| `RamPipe.Dispose` | ~61 | 0 | HOT | `NativeMemory.Free` 60c + bool set | src/Relay/Pipes/RamPipe.cs:64 |
| `RamPipe.DrainTo` (per item) | ~11 | 0 | ULTRA-HOT | compare + unsafe ptr deref + virtual `Enqueue` 7c + `head++` | src/Relay/Pipes/RamPipe.cs:52 |
| `DispatchPipe.Enqueue` (healthy, sealed subclass) | ~15 | 0 | ULTRA-HOT | `IsHealthy` vol-read 1c + virt `Accept` devirt 3c + body 10c | src/Relay/DispatchPipe.cs:32 |
| `SpscQueuePipe.Accept` | ~10 | 0 | ULTRA-HOT | inlines `SpscRingBuffer.TryPublish` | src/Relay/SpscQueuePipe.cs:101 |
| `SpscRingBuffer.TryPublish` | ~10 | 0 | ULTRA-HOT | plain load tail 1c + `Volatile.Read` head 1c + cap 1c + bounds-checked store ~5c + `Volatile.Write` tail 1c + ret 1c | src/Relay/Buffers/SpscRingBuffer.cs:72 |
| `SpscRingBuffer.TryConsume` | ~10 | 0 (item ref out) | ULTRA-HOT | symmetric to TryPublish | src/Relay/Buffers/SpscRingBuffer.cs:89 |
| `SpscRingBuffer.WriteSlot` | ~4 | sizeof(T) | ULTRA-HOT | `Unsafe.CopyBlockUnaligned` for T=64B = 4c (SIMD 128) | src/Relay/Buffers/SpscRingBuffer.cs:113 |
| `SpscRingBuffer.TryReserveTail` | ~3 | 0 | ULTRA-HOT | plain + volatile read + cmp | src/Relay/Buffers/SpscRingBuffer.cs:105 |
| `SpscRingBuffer.CommitTail` | ~2 | 0 | ULTRA-HOT | `Volatile.Write` tail | src/Relay/Buffers/SpscRingBuffer.cs:121 |
| `SpscRingBuffer.IsEmpty` | ~2 | 0 | ULTRA-HOT | plain head + vol-read tail + cmp | src/Relay/Buffers/SpscRingBuffer.cs:50 |
| `SpscRingBuffer.IsFull` | ~2 | 0 | ULTRA-HOT | plain tail + vol-read head + sub | src/Relay/Buffers/SpscRingBuffer.cs:43 |
| `SpscRingBuffer.Count` | ~3 | 0 | ULTRA-HOT | 2× vol-read + sub | src/Relay/Buffers/SpscRingBuffer.cs:36 |
| `FanOut2Pipe.Accept` (sealed children) | ~30 | 0 | ULTRA-HOT | 2× devirt Enqueue (~15c each) | src/Relay/FanOutPipe.cs:68 |
| `FanOutPipe.Accept` (N children) | ~7 + N×17 | 0 | ULTRA-HOT→HOT | foreach + virt call 7c + Enqueue 10c per child | src/Relay/FanOutPipe.cs:35 |
| `FanOutPipe.IsHealthy` | ~N×8 | 0 | ULTRA-HOT | foreach + virt `IsHealthy` short-circuit | src/Relay/FanOutPipe.cs:24 |
| `FanOut2Pipe.IsHealthy` | ~4 | 0 | ULTRA-HOT | 2 devirt bool reads, short-circuit OR | src/Relay/FanOutPipe.cs:65 |
| `FilterPipe.Accept` | ~18 | 0 | ULTRA-HOT | delegate invoke 8c + branch + downstream `Enqueue` 10c | src/Relay/FilterPipe.cs:31 |
| `NullPipe.Accept` | ~1 | 0 | ULTRA-HOT | `return true` | src/Relay/NullPipe.cs:17 |
| `MmfPipe.IsHealthy` | ~4 | 0 | ULTRA-HOT | vol-read `_healthy` + vol-read `_position` + add + cmp | src/Relay/Pipes/MmfPipe.cs:30 |
| `SpscQueuePipe.IsHealthy` | ~1 | 0 | ULTRA-HOT | vol-read `_healthy` | src/Relay/SpscQueuePipe.cs:61 |
| `RamPipe.IsHealthy` | ~3 | 0 | ULTRA-HOT | plain read tail/head + cmp (single-threaded) | src/Relay/Pipes/RamPipe.cs:36 |
| `RamPipe.Accept` | ~7 | 0 | ULTRA-HOT | cap 1c + unsafe store 5c + `_tail++` 1c | src/Relay/Pipes/RamPipe.cs:39 |
| `HfClock.NowTicks` | ~20 | 0 | ULTRA-HOT | RDTSC | src/Relay/Internal/HfClock.cs:8 |
| `PipeChain.To` | ~10 | 0 | ULTRA-HOT | isinst cast + 2 field writes | src/Relay/Builder/PipeChain.cs:27 |
| `PipeChain.Build` | ~1 | 0 | ULTRA-HOT | return field | src/Relay/Builder/PipeChain.cs:37 |
| `RelayBuilder.Start` | ~20 | 32 | ULTRA-HOT | alloc `PipeChain` gen0 ~20c | src/Relay/Builder/RelayBuilder.cs:21 |

## 2. Top 20 Nodes by cycles/call
| # | tier | cycles/call | bytes/call | symbol | file:line | dispatch | notes |
|---|---|---|---|---|---|---|---|
| 1 | RARE | ~1.05e5 | 250 | `SpscQueuePipe.Start` | src/Relay/SpscQueuePipe.cs:75 | direct | thread creation dominates |
| 2 | COLD | ~5.3e3 | 2.62e5 | `FileStreamPipe.ctor` | src/Relay/Pipes/FileStreamPipe.cs:34 | direct | FS open syscall + 256KB POH alloc |
| 3 | COLD | ~5.0e3 | 64 | `MmfPipe.ctor` | src/Relay/Pipes/MmfPipe.cs:32 | direct | MMF create+view syscalls |
| 4 | COLD | ~5.0e3 | 2.62e5 | `TcpPipe.ctor` | src/Relay/Pipes/TcpPipe.cs:33 | direct | blocking TCP connect |
| 5 | COLD | ~5.0e3 | 0 | `FileStreamPipe.FlushBuffer` | src/Relay/Pipes/FileStreamPipe.cs:87 | direct | FileStream.Write syscall |
| 6 | WARM | ~2.0e3 | 0 | `MmfPipe.FlushBackend` | src/Relay/Pipes/MmfPipe.cs:53 | direct | view.Flush syscall |
| 7 | WARM | ~2.0e3 | 0 | `TcpPipe.FlushBuffer` | src/Relay/Pipes/TcpPipe.cs:89 | direct | socket send syscall |
| 8 | WARM | ~2.0e3 | 0 | `SpscQueuePipe.Stop` | src/Relay/SpscQueuePipe.cs:90 | direct | Thread.Join OS wait |
| 9 | COLD | ~5.0e3 | 0 | `FileStreamPipe.TryRecoverBackend` (reopen path) | src/Relay/Pipes/FileStreamPipe.cs:63 | direct | reopen FS syscall |
| 10 | COLD | ~5.0e3 | 0 | `TcpPipe.TryRecoverBackend` (reconnect path) | src/Relay/Pipes/TcpPipe.cs:64 | direct | reconnect TCP |
| 11 | WARM | ~1.0e3 | 0 | `SpscQueuePipe.ConsumeLoop` idle iter (Sleep branch) | src/Relay/SpscQueuePipe.cs:149 | direct | Thread.Sleep(1) = 1e6c when reached |
| 12 | HOT | ~60 | 0 | `NativeMemory.Free` (in `RamPipe.Dispose`) | src/Relay/Pipes/RamPipe.cs:68 | direct | P/Invoke + free |
| 13 | ULTRA-HOT | ~32 | 0 | `FanOut2Pipe.Accept` | src/Relay/FanOutPipe.cs:68 | devirt (sealed) | 2 inlined Enqueue |
| 14 | ULTRA-HOT | ~30 | sizeof(T) | `MmfPipe.WriteToBackend` | src/Relay/Pipes/MmfPipe.cs:47 | direct | accessor.Write 30c + vol-write |
| 15 | ULTRA-HOT | ~20 | 0 | `HfClock.NowTicks` | src/Relay/Internal/HfClock.cs:8 | inlined | RDTSC |
| 16 | ULTRA-HOT | ~18 | 0 | `FilterPipe.Accept` | src/Relay/FilterPipe.cs:31 | direct | delegate 8c + Enqueue 10c |
| 17 | ULTRA-HOT | ~15 | 0 | `DispatchPipe.Enqueue` (sealed subclass, healthy) | src/Relay/DispatchPipe.cs:32 | devirt | virt call → direct |
| 18 | ULTRA-HOT | ~11 | 0 | `RamPipe.DrainTo` per-item | src/Relay/Pipes/RamPipe.cs:52 | virt | `target.Enqueue` 7c dominates |
| 19 | ULTRA-HOT | ~10 | 0 | `SpscRingBuffer.TryPublish` | src/Relay/Buffers/SpscRingBuffer.cs:72 | inlined | 2 vol + store |
| 20 | ULTRA-HOT | ~10 | 0 | `SpscRingBuffer.TryConsume` | src/Relay/Buffers/SpscRingBuffer.cs:89 | inlined | 2 vol + load |
| 21 | ULTRA-HOT | ~10 | 0 | `SpscQueuePipe.Accept` | src/Relay/SpscQueuePipe.cs:101 | inlined | forwards to TryPublish |
| 22 | ULTRA-HOT | ~9 | sizeof(T) | `FileStreamPipe.WriteToBackend` (no flush) | src/Relay/Pipes/FileStreamPipe.cs:45 | direct | bounds + CopyBlock 4c (T=64) + add |
| 23 | ULTRA-HOT | ~9 | sizeof(T) | `TcpPipe.WriteToBackend` (no flush) | src/Relay/Pipes/TcpPipe.cs:46 | direct | identical to FileStreamPipe |
| 24 | ULTRA-HOT | ~7 | 0 | `RamPipe.Accept` | src/Relay/Pipes/RamPipe.cs:39 | inlined | unsafe ptr path |

## 3. Hot Tree (call hierarchy)

Producer path (caller thread, tick-rate):
```
DispatchPipe.Enqueue              src/Relay/DispatchPipe.cs:32             ~15c  0B   ULTRA-HOT
├─ IsHealthy (virt/devirt)        per-subclass                             ~1-4c 0B   ULTRA-HOT
├─ Accept (virt/devirt)           per-subclass                             varies
│  ├─ SpscQueuePipe.Accept        src/Relay/SpscQueuePipe.cs:101           ~10c  0B   ULTRA-HOT
│  │   └─ SpscRingBuffer.TryPublish  src/Relay/Buffers/SpscRingBuffer.cs:72  ~10c  0B   ULTRA-HOT
│  ├─ RamPipe.Accept              src/Relay/Pipes/RamPipe.cs:39            ~7c   0B   ULTRA-HOT
│  ├─ FanOut2Pipe.Accept          src/Relay/FanOutPipe.cs:68               ~30c  0B   ULTRA-HOT
│  │   ├─ _c1.Enqueue (devirt)    recurses                                 ~15c
│  │   └─ _c2.Enqueue (devirt)    recurses                                 ~15c
│  ├─ FanOutPipe.Accept           src/Relay/FanOutPipe.cs:35               7+N×17c      ULTRA-HOT→HOT
│  ├─ FilterPipe.Accept           src/Relay/FilterPipe.cs:31               ~18c  0B   ULTRA-HOT
│  │   └─ Predicate<T> invoke     delegate                                 ~8c    0B   (blind)
│  └─ NullPipe.Accept             src/Relay/NullPipe.cs:17                 ~1c   0B   ULTRA-HOT
└─ Next?.Enqueue (fallback)       virt                                     +7c per hop
```

Consumer path (dedicated thread, `SpscQueuePipe` subclasses):
```
SpscQueuePipe.ConsumeLoop         src/Relay/SpscQueuePipe.cs:124           loop  0B
├─ ShouldKeepDraining             src/Relay/SpscQueuePipe.cs:188           ~3c   0B   ULTRA-HOT
│  └─ SpscRingBuffer.IsEmpty      src/Relay/Buffers/SpscRingBuffer.cs:50   ~2c   0B   ULTRA-HOT
├─ SpscRingBuffer.TryConsume      src/Relay/Buffers/SpscRingBuffer.cs:89   ~10c  0B   ULTRA-HOT
├─ WriteToBackend (virt-sealed)   per-backend                              ~9-30c
│  ├─ FileStreamPipe.WriteToBackend  src/Relay/Pipes/FileStreamPipe.cs:45  ~9c   64B  ULTRA-HOT
│  │   └─ FlushBuffer (every 4096)  src/Relay/Pipes/FileStreamPipe.cs:87   ~5e3c 0B   COLD
│  ├─ TcpPipe.WriteToBackend      src/Relay/Pipes/TcpPipe.cs:46            ~9c   64B  ULTRA-HOT
│  │   └─ FlushBuffer (every 4096)  src/Relay/Pipes/TcpPipe.cs:89          ~2e3c 0B   WARM
│  └─ MmfPipe.WriteToBackend      src/Relay/Pipes/MmfPipe.cs:47            ~30c  64B  ULTRA-HOT
├─ idle path (ring empty, running)  src/Relay/SpscQueuePipe.cs:145         20c→300c→1e6c
│  ├─ Thread.SpinWait(20)         10 iters → 20c each                      ~200c
│  ├─ Thread.Yield                5 iters                                  ~300c
│  └─ Thread.Sleep(1)             thereafter                               ~1e6c    WARM
├─ HfClock.NowTicks (deadline)    src/Relay/Internal/HfClock.cs:8          ~20c  0B   ULTRA-HOT
└─ flush-interval branch          src/Relay/SpscQueuePipe.cs:153
   ├─ FlushBackend                varies 2-5e3c
   ├─ TryRecoverBackend           src/Relay/SpscQueuePipe.cs:113           ~5e3c (on retry)
   └─ TryDrainToPrev              src/Relay/SpscQueuePipe.cs:178           N × (TryConsume 10c + virt Enqueue 7c + IsHealthy check)
```

## 4. Allocation Map (top 10 by bytes/call)
| # | bytes/call | symbol | file:line | kind |
|---|---|---|---|---|
| 1 | 2.62e5 | `FileStreamPipe.ctor` (4096 × sizeof(T)=64) | src/Relay/Pipes/FileStreamPipe.cs:41 | `GC.AllocateArray<byte>` pinned (POH) |
| 2 | 2.62e5 | `TcpPipe.ctor` (same size) | src/Relay/Pipes/TcpPipe.cs:42 | `GC.AllocateArray<byte>` pinned (POH) |
| 3 | ring×sizeof(T) | `SpscRingBuffer.ctor` | src/Relay/Buffers/SpscRingBuffer.cs:64 | `GC.AllocateArray<T>` pinned (POH) |
| 4 | capacity×sizeof(T) | `RamPipe.ctor` (default 512 MiB for T=64B) | src/Relay/Pipes/RamPipe.cs:32 | `NativeMemory.AllocZeroed` (off-heap) |
| 5 | 32 | `RelayBuilder.Start` | src/Relay/Builder/RelayBuilder.cs:21 | `PipeChain` gen0 |
| 6 | 32 | `FanOut2Pipe.ctor` | src/Relay/FanOutPipe.cs:59 | `FanOut2Pipe` gen0 |
| 7 | 32+N×8 | `FanOutPipe.ctor` | src/Relay/FanOutPipe.cs:16 | object + retained children array |
| 8 | 40 | `FilterPipe.ctor` | src/Relay/FilterPipe.cs:21 | object (delegate ref captured) |
| 9 | Thread obj | `SpscQueuePipe.Start` | src/Relay/SpscQueuePipe.cs:80 | managed `Thread` + name string |
| 10 | 0 | steady-state hot path | producer+consumer | zero-alloc invariant holds |

## 5. Anti-Pattern Offenders
| severity | rule | symbol | file:line | evidence |
|---|---|---|---|---|
| low | virtual call on hot path w/ multiple impls | `DispatchPipe.Enqueue` → `IsHealthy` / `Accept` | src/Relay/DispatchPipe.cs:32 | 5 subclasses; mitigated when caller uses `sealed` concrete type (devirt) |
| low | delegate invoke on hot path | `FilterPipe.Accept` → `_predicate(item)` | src/Relay/FilterPipe.cs:33 | 8c per call; intentional user extension point |
| info | `Thread.Sleep(1)` in idle branch | `SpscQueuePipe.ConsumeLoop` | src/Relay/SpscQueuePipe.cs:149 | 1e6c — acceptable (idle only, no producer pressure) |
| info | `foreach` over array in broadcast | `FanOutPipe.Accept` / `.IsHealthy` | src/Relay/FanOutPipe.cs:28,35 | array enumerator is struct (IL reduced to indexer); no alloc |
| info | isinst runtime cast | `PipeChain.To` | src/Relay/Builder/PipeChain.cs:30 | build-time only, not hot |
| none | heap alloc in hot path | — | — | none detected on `Enqueue`/`Accept`/`TryPublish`/`TryConsume` |
| none | `lock`/`Monitor` | — | — | not present |
| none | `async`/`await` on hot path | — | — | not present |
| none | `DateTime.UtcNow` | — | — | `HfClock.NowTicks` used everywhere |

## 6. Cache-Line Report
| symbol | sizeof | issue | file:line |
|---|---|---|---|
| `PaddedLong` | 128 | intentional — prevents false sharing between producer tail and consumer head | src/Relay/Buffers/SpscRingBuffer.cs:10 |
| `SpscRingBuffer<T>` instance | ~headerOk | `_head` and `_tail` are adjacent `PaddedLong` fields → 2 × 128B = separate cache lines, no false sharing | src/Relay/Buffers/SpscRingBuffer.cs:26 |
| `T` payload | 32/64/128/256 | enforced by `PipeConstraints.AssertCacheLineAligned<T>()` in DEBUG only | src/Relay/Internal/PipeConstraints.cs:14 |
| `FileStreamPipe` / `TcpPipe` instance | <64 scalar fields + buffer refs | no false-sharing risk — fields only written by consumer thread | src/Relay/Pipes/FileStreamPipe.cs:26 |
| `MmfPipe._position` | 8B scalar | written by consumer via `Volatile.Write`, read by producer `IsHealthy` via `Volatile.Read`; shares cache line with other class fields → minor producer read contention on flush | src/Relay/Pipes/MmfPipe.cs:27 |
| `RamPipe._head`/`_tail` | 8B each, adjacent | single-thread contract; no false-sharing concern | src/Relay/Pipes/RamPipe.cs:21 |

## 7. Syscalls & Kernel Boundaries
| symbol | file:line | kind | cycles | notes |
|---|---|---|---|---|
| `FileStream.Write` | src/Relay/Pipes/FileStreamPipe.cs:91 | file I/O | ~5000 | batched every 4096 items (64KB for T=16B; 256KB for T=64B) |
| `FileStream` ctor | src/Relay/Pipes/FileStreamPipe.cs:102 | file open | ~5000 | one-time + on recovery |
| `TcpClient.Connect` | src/Relay/Pipes/TcpPipe.cs:106 | TCP handshake | ~5e6 (RTT dominated) | cold path, includes 3-way handshake |
| `NetworkStream.Write` | src/Relay/Pipes/TcpPipe.cs:93 | socket send | ~2000 | batched like FileStream |
| `MemoryMappedViewAccessor.Write` | src/Relay/Pipes/MmfPipe.cs:49 | managed marshal | ~30 | no syscall — direct memory access + bounds check |
| `MemoryMappedViewAccessor.Flush` | src/Relay/Pipes/MmfPipe.cs:53 | msync | ~2000 | per flush interval |
| `VirtualLock` | src/Relay/Memory/RelayMemory.cs:43 | NT kernel | ~2000 | one-time in `Start` |
| `Stopwatch.GetTimestamp` (RDTSC) | src/Relay/Internal/HfClock.cs:11 | CPU instruction | ~20 | not a kernel boundary |
| `Thread.Start` | src/Relay/SpscQueuePipe.cs:86 | kernel thread | ~1e5 | one-time |
| `Thread.Join` | src/Relay/SpscQueuePipe.cs:96 | kernel wait | ~1e3–5e3 | on `Stop`/`Dispose` |
| `Thread.Sleep(1)` | src/Relay/SpscQueuePipe.cs:149 | kernel yield | ~1e6 | idle branch only |
| `Thread.Yield` | src/Relay/SpscQueuePipe.cs:148 | kernel | ~300 | idle branch only |
| `NativeMemory.AllocZeroed` | src/Relay/Pipes/RamPipe.cs:32 | heap | ~60 + zero | zeroing dominates at 512 MiB (~1e8 c) |
| `NativeMemory.Free` | src/Relay/Pipes/RamPipe.cs:68 | heap | ~60 | dispose only |

## 8. Blind Subgraphs
| symbol | file:line | reason | hint-key |
|---|---|---|---|
| `Predicate<T>` body in `FilterPipe` | src/Relay/FilterPipe.cs:33 | caller-supplied delegate; cost unknown | filter.predicate.cost |
| backend I/O latency (FS/TCP) | FileStreamPipe.cs:91 / TcpPipe.cs:93 | OS + device dependent; model uses fixed syscall estimate only | io.device.latency |
| JIT devirtualization assumption | DispatchPipe.cs / FanOut2Pipe.cs | effective only when caller holds sealed concrete type; casting to base incurs 7c virtual | jit.devirt.profile |
| `VirtualLock` NT kernel path | Memory/RelayMemory.cs:41 | best-effort; privilege-dependent; failure is silent | nt.virtuallock.privilege |

## 9. Delta vs prior audit (v1/v2)
| change | symbol | file:line | note |
|---|---|---|---|
| aligned | `Volatile.Write` reclassified from 15c (mfence) to 1c (release store on x64) | src/Relay/Buffers/SpscRingBuffer.cs:80,97 | matches x64 Golden Cove cost model; CLAUDE.md still cites 15c as historical "mfence on x64" — this report uses current JIT lowering (release mov); producer hot path revised from ~25c to ~10c |
| aligned | producer Enqueue revised from ~32c (CLAUDE.md reference) to ~15c | src/Relay/DispatchPipe.cs:32 | CLAUDE.md table uses pessimistic bound; this audit reflects the post-devirt inlined form |
| confirmed | `FanOut2Pipe` CRTP saves ~6c vs array FanOut with N=2 | src/Relay/FanOutPipe.cs:51 | matches v2 audit finding; 2 devirt inlined Enqueue vs 2 virt calls + foreach dispatch |
| confirmed | `FilterPipe.Accept` always returns true — filtered items do not trigger fallback | src/Relay/FilterPipe.cs:35 | semantics preserved; cost model unchanged |
| confirmed | `RamPipe.Accept` is the fastest local write (~7c) thanks to unsafe pointer arithmetic + single-thread contract | src/Relay/Pipes/RamPipe.cs:39 | matches v1 observation |
| new | `MmfPipe.WriteToBackend` ~30c — fastest durable backend per item | src/Relay/Pipes/MmfPipe.cs:47 | accessor.Write managed bounds-check dominates |
| new | Start path dominated by `Thread.Start` (~1e5c); `VirtualLock` + PreFault add ~2e3c + page-touch loop (ring/4096 × 1c load) | src/Relay/SpscQueuePipe.cs:75 | one-time cost; not on dispatch hot path |
| new | idle consumer Sleep(1) = 1e6c — justified as pure idle; wakes at flush deadline regardless | src/Relay/SpscQueuePipe.cs:149 | no producer impact; deferred backend flushes bounded by `_flushIntervalTicks` |
| risk | `MmfPipe._position` shares cache line with adjacent instance fields; producer `IsHealthy` reads it volatile on every Enqueue | src/Relay/Pipes/MmfPipe.cs:27,30 | minor false-sharing vs ring padding discipline; measurable only at very high tick rates |
