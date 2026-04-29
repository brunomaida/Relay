# Resource Cost Map — Relay

_generated 2026-04-29 · model x64-zen4/golden-cove v1 · static estimate, not benchmark · library mode_

## 0. Header
- scope: Relay .NET 9 library (`src/Relay/**/*.cs`), 31 source files
- entry count: 41 public methods/props across 17 public types (`DispatchSink<T>` + `PacketSink` parallel hierarchies)
- nodes analyzed: 78 (public + internal hot-path)
- blind subgraphs: 5 (`Predicate<T>` / `PacketPredicate` delegate body, backend OS I/O, JIT devirtualization assumption, `VirtualLock` NT kernel path, MPSC contention CAS-retry distribution)
- library mode: no `Main`; `calls/s` unknown; ranking by `cycles/call` desc

## 1. Per-Entry Cost Table (top 30 by cycles/call)

| Entry | cycles/call | bytes/call | tier | drivers | file:line |
|---|---|---|---|---|---|
| `SpscQueueSink<T>.Start` | ~1.05e5 | ~250 | RARE | `Thread.Start` (~1e5c) + `RelayMemory.PreFaultAndLock` (page-touch loop + `VirtualLock` syscall ~2e3c) | src/Relay/SpscQueueSink.cs:77 |
| `MpscQueueSink<T>.Start` | ~1.05e5 | ~250 | RARE | identical to SPSC + ring `PreFaultAndLock` | src/Relay/MpscQueueSink.cs:79 |
| `SpscQueueSink.Start` (packet) | ~1.05e5 | ~250 | RARE | identical (byte ring) | src/Relay/SpscQueueSink.Packet.cs:61 |
| `MpscQueueSink.Start` (packet) | ~1.05e5 | ~250 | RARE | identical (byte ring) | src/Relay/MpscQueueSink.Packet.cs:75 |
| `RotatingFileSink.RotateNow` | ~1e7 | varies | RARE | open new `FileStream` + `Cleanup` LINQ over `Directory.GetFiles` | src/Relay/Sinks/RotatingFileSink.cs:89 |
| `FileStreamSink<T>.ctor` | ~5.3e3 | 2.62e5 | COLD | `GC.AllocateArray<byte>(4096*EntrySize, pinned)` + `OpenStream` syscall | src/Relay/Sinks/FileStreamSink.cs:34 |
| `TcpSink<T>.ctor` | ~5e6 | 2.62e5 | COLD | RTT-bound `Socket.Connect` + POH alloc | src/Relay/Sinks/TcpSink.cs:40 |
| `MmfSink<T>.ctor` | ~1e4 | 64 | COLD | `MemoryMappedFile.CreateFromFile` + `CreateViewAccessor` + `AcquirePointer` | src/Relay/Sinks/MmfSink.cs:41 |
| `RamSink<T>.ctor` (default 8M slots × 64B = 512 MiB) | ~1e8 | 0 heap (native) | COLD | `NativeMemory.AllocZeroed` zero-fill dominates | src/Relay/Sinks/RamSink.cs:25 |
| `RamSink.ctor` (packet, 4 MiB default) | ~6.5e3 | 0 heap (native) | COLD | `NativeMemory.AlignedAlloc(64)` (no zero — ring fills linearly) | src/Relay/Sinks/RamSink.Packet.cs:36 |
| `SharedMemorySink.ctor` | ~5e3 | 64 | COLD | `MemoryMappedFile.CreateOrOpen` + `AcquirePointer` + CAS-init magic | src/Relay/Sinks/SharedMemorySink.cs:51 |
| `*.Stop` (any queue sink) | ~2e3 | 0 | WARM | `Thread.Join` kernel wait | src/Relay/SpscQueueSink.cs:92 |
| `FileStreamSink<T>.FlushBuffer` (non-empty) | ~5e3 | 0 | COLD | `FileStream.Write` syscall | src/Relay/Sinks/FileStreamSink.cs:87 |
| `TcpSink<T>.FlushBuffer` (non-empty, no spin) | ~2e3 | 0 | WARM | `Socket.Send` syscall | src/Relay/Sinks/TcpSink.cs:111 |
| `UdpSink.WriteToBackend` | ~2e3 | 0 | WARM | `Socket.Send` syscall **per record** — no batching | src/Relay/Sinks/UdpSink.cs:41 |
| `MmfSink<T>.FlushBackend` | ~2e3 | 0 | WARM | `MemoryMappedViewAccessor.Flush` (msync) | src/Relay/Sinks/MmfSink.cs:77 |
| `FileSink.FlushToStream` | ~5e3 | 0 | COLD | `FileStream.Write` + `Flush` | src/Relay/Sinks/FileSink.cs:76 |
| `NamedPipeSink.FlushBackend` | ~5e3 | 0 | COLD | `NamedPipeClientStream.Write` syscall | src/Relay/Sinks/NamedPipeSink.cs:50 |
| `RotatingFileSink.ShouldRotate` | ~50 | 0 | HOT (consumer) | `DateTime.UtcNow.Date` — called per-record in `WriteToBackend` | src/Relay/Sinks/RotatingFileSink.cs:82 |
| `MpscByteRingBuffer.TryPublish` (uncontended) | ~50 | 0 | ULTRA-HOT | `Interlocked.CompareExchange` 25c + `payload.CopyTo` 8c + 2× volatile header write 4c | src/Relay/Buffers/MpscByteRingBuffer.cs:111 |
| `SharedMemorySink.Accept` | ~50 | 0 | ULTRA-HOT | CAS loop on WriteIndex + 2× `WriteRing` (modular wrap) | src/Relay/Sinks/SharedMemorySink.cs:88 |
| `SpscByteRingBuffer.TryPublish` | ~35 | 0 | ULTRA-HOT | bounds + `Unsafe.WriteUnaligned` header + `payload.CopyTo` + `Volatile.Write` tail | src/Relay/Buffers/SpscByteRingBuffer.cs:84 |
| `Multi2Sink<T,TC1,TC2>.Accept` (sealed) | ~32 | 0 | ULTRA-HOT | 2× devirt+inlined `Enqueue` (~15c each) | src/Relay/MultiSink.cs:70 |
| `MmfSink<T>.WriteToBackend` | ~30 | sizeof(T) | ULTRA-HOT | bounds 2c + `Unsafe.CopyBlockUnaligned` 4c (T=64) + `Volatile.Write` 1c + adds | src/Relay/Sinks/MmfSink.cs:61 |
| `MpscRingBuffer<T>.TryPublish` (uncontended) | ~30 | 0 | ULTRA-HOT | `Interlocked.CompareExchange` 25c + slot store 1c + `Volatile.Write` Published 1c | src/Relay/Buffers/MpscRingBuffer.cs:100 |
| `RamSink.Accept` (packet) | ~20 | 0 | ULTRA-HOT | bounds 2c + uint header write + `CopyBlockUnaligned` payload + `Volatile.Write` tail | src/Relay/Sinks/RamSink.Packet.cs:58 |
| `HfClock.NowTicks` | ~20 | 0 | ULTRA-HOT | `Stopwatch.GetTimestamp` → RDTSC | src/Relay/Internal/HfClock.cs:8 |
| `TcpSink.WriteToBackend` (packet, hot path) | ~18 | 0 | ULTRA-HOT | `WriteUInt32BigEndian` 4c + `payload.CopyTo` 8c + adds | src/Relay/Sinks/TcpSink.Packet.cs:47 |
| `NamedPipeSink.WriteToBackend` | ~18 | 0 | ULTRA-HOT | identical to packet TcpSink — 4B BE prefix + copy | src/Relay/Sinks/NamedPipeSink.cs:39 |
| `UnixSocketSink.WriteToBackend` | ~18 | 0 | ULTRA-HOT | identical | src/Relay/Sinks/UnixSocketSink.cs:41 |
| `FilterSink<T>.Accept` | ~18 | 0 | ULTRA-HOT | `Predicate<T>` invoke 8c + branch + downstream `Enqueue` 10c | src/Relay/FilterSink.cs:30 |
| `FilterSink.Accept` (packet) | ~18 | 0 | ULTRA-HOT | identical w/ `PacketPredicate` | src/Relay/FilterSink.Packet.cs:42 |
| `DispatchSink<T>.Enqueue` (sealed sub, healthy, propagate=false) | ~15 | 0 | ULTRA-HOT | vol-read `_healthy` 1c + devirt `Accept` 3c + body N + early-return | src/Relay/DispatchSink.cs:44 |
| `PacketSink.Enqueue` (sealed sub, healthy, propagate=false) | ~15 | 0 | ULTRA-HOT | identical | src/Relay/PacketSink.cs:62 |
| `PacketSink.TryEnqueue` | ~13 | 0 | ULTRA-HOT | vol-read + devirt `Accept` (no fallthrough) | src/Relay/PacketSink.cs:54 |
| `ForkSink<T>.Accept` | ~17 | 0 | ULTRA-HOT | inline `_primary.Enqueue` (~15c) + return true | src/Relay/ForkSink.cs:36 |
| `ForkSink.Accept` (packet) | ~18 | 0 | ULTRA-HOT | inline `_primary.Enqueue` + vol-read healthy | src/Relay/ForkSink.Packet.cs:22 |
| `SerializeSink<T>.Accept` | ~5 + downstream | 0 | ULTRA-HOT | `MemoryMarshal.AsBytes` (no copy, ref-cast) + `_target.Enqueue` | src/Relay/SerializeSink.cs:30 |
| `SpscQueueSink<T>.Accept` | ~10 | 0 | ULTRA-HOT | inlines `SpscRingBuffer.TryPublish` | src/Relay/SpscQueueSink.cs:103 |
| `MpscQueueSink<T>.Accept` | ~30 | 0 | ULTRA-HOT | inlines `MpscRingBuffer.TryPublish` (CAS) | src/Relay/MpscQueueSink.cs:105 |
| `SpscRingBuffer<T>.TryPublish` (cache hit) | ~10 | 0 | ULTRA-HOT | plain tail load + cached-head cmp + slot store + `Volatile.Write` tail | src/Relay/Buffers/SpscRingBuffer.cs:84 |
| `SpscByteRingBuffer.TryPeek` | ~15 | 0 (zero-copy span) | ULTRA-HOT | plain head + cached-tail cmp + `Unsafe.ReadUnaligned` header + `MemoryMarshal.CreateReadOnlySpan` | src/Relay/Buffers/SpscByteRingBuffer.cs:144 |
| `MpscByteRingBuffer.TryPeek` | ~12 | 0 (zero-copy span) | ULTRA-HOT | volatile-read header + bit-test high bit + span build | src/Relay/Buffers/MpscByteRingBuffer.cs:174 |
| `MpscRingBuffer<T>.TryConsume` | ~12 | 0 | ULTRA-HOT | volatile-read Published + slot copy out + 2 volatile-writes | src/Relay/Buffers/MpscRingBuffer.cs:134 |
| `SpscRingBuffer<T>.TryPublishBatch` (N items, single fence) | ~5 + 2N | sizeof(T)·N | ULTRA-HOT | one cached-head cmp + N slot stores + `Thread.MemoryBarrier` 25c + `Volatile.Write` tail | src/Relay/Buffers/SpscRingBuffer.cs:176 |
| `SpscQueueSink<T>.EnqueueBatch` (healthy) | ~30 + 2N | 0 | ULTRA-HOT | health check + batch publish + per-overflow Next?.Enqueue | src/Relay/SpscQueueSink.cs:112 |
| `BatchSink.WriteToBackend` (consumer, fits) | ~10 | 0 | HOT (consumer) | bounds + `payload.CopyTo` to scratch | src/Relay/BatchSink.cs:44 |
| `MultiSink<T>.Accept` (N children) | ~7 + N×17 | 0 | ULTRA-HOT→HOT | foreach (struct enumerator) + virt `Enqueue` each | src/Relay/MultiSink.cs:37 |
| `MultiSink.Accept` (packet, N children) | ~7 + N×17 | 0 | ULTRA-HOT→HOT | identical | src/Relay/MultiSink.Packet.cs:28 |
| `NullSink<T>.Accept` / `NullSink.Accept` | ~1 | 0 | ULTRA-HOT | `return true` | src/Relay/NullSink.cs:17 |
| `*.IsHealthy` (vol-read `_healthy`) | ~1 | 0 | ULTRA-HOT | single `volatile bool` field | src/Relay/SpscQueueSink.cs:62 |
| `MmfSink<T>.IsHealthy` | ~4 | 0 | ULTRA-HOT | vol-read `_healthy` + vol-read `_position` + add + cmp | src/Relay/Sinks/MmfSink.cs:39 |
| `SharedMemorySink.IsHealthy` | ~10 | 0 | ULTRA-HOT | 2× volatile pointer-deref + modular distance + cmp | src/Relay/Sinks/SharedMemorySink.cs:75 |
| `PacketSink.Enqueue` (terminal drop, Next null) | ~25 | 0 | (cold path) | `Interlocked.Increment(ref _dropCount)` | src/Relay/PacketSink.cs:77 |

## 2. Top 20 Nodes by cycles/call (steady state, excluding ctors/Start)

| # | tier | cycles/call | bytes/call | symbol | file:line | dispatch | notes |
|---|---|---|---|---|---|---|---|
| 1 | WARM | ~5e3 | 0 | `FileStreamSink<T>.FlushBuffer` | src/Relay/Sinks/FileStreamSink.cs:87 | direct | `FileStream.Write` syscall |
| 2 | COLD | ~5e3 | 0 | `FileSink.FlushToStream` | src/Relay/Sinks/FileSink.cs:76 | direct | `FileStream.Write` + `Flush` |
| 3 | COLD | ~5e3 | 0 | `NamedPipeSink.FlushBackend` | src/Relay/Sinks/NamedPipeSink.cs:50 | direct | named-pipe write syscall |
| 4 | WARM | ~2e3 | 0 | `UdpSink.WriteToBackend` | src/Relay/Sinks/UdpSink.cs:41 | direct | **per-record** syscall, no batch |
| 5 | WARM | ~2e3 | 0 | `MmfSink<T>.FlushBackend` | src/Relay/Sinks/MmfSink.cs:77 | direct | `accessor.Flush` msync |
| 6 | WARM | ~2e3 | 0 | `TcpSink<T>.FlushBuffer` (no spin) | src/Relay/Sinks/TcpSink.cs:111 | direct | `Socket.Send` |
| 7 | WARM | ~2e3 | 0 | `*.Stop` | src/Relay/SpscQueueSink.cs:92 | direct | `Thread.Join` |
| 8 | HOT (consumer) | ~50 | 0 | `RotatingFileSink.ShouldRotate` | src/Relay/Sinks/RotatingFileSink.cs:82 | direct | `DateTime.UtcNow.Date` per record |
| 9 | ULTRA-HOT | ~50 | 0 | `MpscByteRingBuffer.TryPublish` (uncontended) | src/Relay/Buffers/MpscByteRingBuffer.cs:111 | inlined | CAS + 2× hdr volatile-write |
| 10 | ULTRA-HOT | ~50 | 0 | `SharedMemorySink.Accept` | src/Relay/Sinks/SharedMemorySink.cs:88 | inlined | CAS loop + 2× modular `WriteRing` |
| 11 | ULTRA-HOT | ~35 | 0 | `SpscByteRingBuffer.TryPublish` | src/Relay/Buffers/SpscByteRingBuffer.cs:84 | inlined | hdr write + payload CopyTo |
| 12 | ULTRA-HOT | ~32 | 0 | `Multi2Sink<T,TC1,TC2>.Accept` | src/Relay/MultiSink.cs:70 | devirt | 2 inlined `Enqueue` |
| 13 | ULTRA-HOT | ~30 | sizeof(T) | `MmfSink<T>.WriteToBackend` | src/Relay/Sinks/MmfSink.cs:61 | direct | unsafe ptr write + vol-write |
| 14 | ULTRA-HOT | ~30 | 0 | `MpscRingBuffer<T>.TryPublish` (uncontended) | src/Relay/Buffers/MpscRingBuffer.cs:100 | inlined | CAS + slot store + Published vol-write |
| 15 | ULTRA-HOT | ~30 | 0 | `MpscQueueSink<T>.Accept` | src/Relay/MpscQueueSink.cs:105 | inlined | forwards to TryPublish |
| 16 | ULTRA-HOT | ~20 | 0 | `RamSink.Accept` (packet) | src/Relay/Sinks/RamSink.Packet.cs:58 | inlined | uint hdr + payload CopyBlock + vol-write |
| 17 | ULTRA-HOT | ~20 | 0 | `HfClock.NowTicks` | src/Relay/Internal/HfClock.cs:8 | inlined | RDTSC |
| 18 | ULTRA-HOT | ~18 | 0 | `TcpSink.WriteToBackend` (packet) | src/Relay/Sinks/TcpSink.Packet.cs:47 | direct | 4B BE + CopyTo |
| 19 | ULTRA-HOT | ~18 | 0 | `NamedPipeSink.WriteToBackend` | src/Relay/Sinks/NamedPipeSink.cs:39 | direct | identical |
| 20 | ULTRA-HOT | ~18 | 0 | `UnixSocketSink.WriteToBackend` | src/Relay/Sinks/UnixSocketSink.cs:41 | direct | identical |
| 21 | ULTRA-HOT | ~18 | 0 | `FilterSink<T>.Accept` / `FilterSink.Accept` | src/Relay/FilterSink.cs:30 | direct | delegate 8c + Enqueue 10c |
| 22 | ULTRA-HOT | ~17 | 0 | `ForkSink<T>.Accept` / `ForkSink.Accept` | src/Relay/ForkSink.cs:36 | inlined | `_primary.Enqueue` |
| 23 | ULTRA-HOT | ~15 | 0 | `DispatchSink<T>.Enqueue` (sealed sub, healthy) | src/Relay/DispatchSink.cs:44 | devirt | `_healthy` + `Accept` + propagate-field test |
| 24 | ULTRA-HOT | ~15 | 0 | `PacketSink.Enqueue` (sealed sub, healthy) | src/Relay/PacketSink.cs:62 | devirt | identical |
| 25 | ULTRA-HOT | ~13 | 0 | `PacketSink.TryEnqueue` | src/Relay/PacketSink.cs:54 | devirt | non-fallthrough variant |
| 26 | ULTRA-HOT | ~12 | 0 | `MpscRingBuffer<T>.TryConsume` | src/Relay/Buffers/MpscRingBuffer.cs:134 | inlined | per-slot Published vol-read+write |
| 27 | ULTRA-HOT | ~10 | 0 | `BatchSink.WriteToBackend` (fits) | src/Relay/BatchSink.cs:44 | direct | scratch CopyTo |
| 28 | ULTRA-HOT | ~10 | 0 | `SpscQueueSink<T>.Accept` | src/Relay/SpscQueueSink.cs:103 | inlined | forwards to TryPublish |
| 29 | ULTRA-HOT | ~10 | 0 | `SpscRingBuffer<T>.TryPublish` / `TryConsume` | src/Relay/Buffers/SpscRingBuffer.cs:84 | inlined | cached-head/tail fast path |
| 30 | ULTRA-HOT | ~5 + dn | 0 | `SerializeSink<T>.Accept` | src/Relay/SerializeSink.cs:30 | inlined | ref-cast bytes + downstream Enqueue |

## 3. Hot Tree (call hierarchy)

Producer path — typed (`DispatchSink<T>.Enqueue`):
```
DispatchSink<T>.Enqueue                   src/Relay/DispatchSink.cs:44             ~15c   0B   ULTRA-HOT
├─ IsHealthy (virt → devirt sealed)        per subclass                              1-10c
├─ Accept (virt → devirt sealed)           per subclass                              varies
│  ├─ SpscQueueSink<T>.Accept              src/Relay/SpscQueueSink.cs:103           ~10c   ULTRA-HOT
│  │   └─ SpscRingBuffer.TryPublish        src/Relay/Buffers/SpscRingBuffer.cs:84   ~10c
│  ├─ MpscQueueSink<T>.Accept              src/Relay/MpscQueueSink.cs:105           ~30c   ULTRA-HOT
│  │   └─ MpscRingBuffer.TryPublish (CAS)  src/Relay/Buffers/MpscRingBuffer.cs:100  ~30c±  (retry under contention)
│  ├─ Multi2Sink.Accept (sealed)           src/Relay/MultiSink.cs:70                ~32c   ULTRA-HOT
│  ├─ MultiSink.Accept                     src/Relay/MultiSink.cs:37                7+N×17c
│  ├─ ForkSink.Accept                      src/Relay/ForkSink.cs:36                 ~17c
│  ├─ FilterSink.Accept                    src/Relay/FilterSink.cs:30               ~18c   (delegate 8c)
│  ├─ SerializeSink.Accept                 src/Relay/SerializeSink.cs:30            ~5c + packet downstream
│  ├─ RamSink.Accept                       src/Relay/Sinks/RamSink.cs:39            ~7c    ULTRA-HOT
│  └─ NullSink.Accept                      src/Relay/NullSink.cs:17                 ~1c
├─ if (accepted && !PropagateAfterAccept) return  (field load, JIT folds when sealed)
└─ Next?.Enqueue (fallback / fork propagate) +7c per hop
```

Producer path — packet (`PacketSink.Enqueue`):
```
PacketSink.Enqueue                         src/Relay/PacketSink.cs:62               ~15c   0B   ULTRA-HOT
├─ IsHealthy + Accept (devirt sealed)
│  ├─ SpscQueueSink.Accept                 src/Relay/SpscQueueSink.Packet.cs:87     ~35c   (TryPublish byte ring)
│  ├─ MpscQueueSink.Accept                 src/Relay/MpscQueueSink.Packet.cs:101    ~50c   (CAS + 2× hdr publish)
│  ├─ SharedMemorySink.Accept              src/Relay/Sinks/SharedMemorySink.cs:88   ~50c±  (CAS + modular WriteRing)
│  ├─ RamSink.Accept (packet)              src/Relay/Sinks/RamSink.Packet.cs:58     ~20c
│  ├─ ForkSink.Accept (packet)             src/Relay/ForkSink.Packet.cs:22          ~18c
│  ├─ MultiSink.Accept (packet)            src/Relay/MultiSink.Packet.cs:28         7+N×17c
│  ├─ FilterSink.Accept (packet)           src/Relay/FilterSink.Packet.cs:42        ~18c
│  └─ NullSink.Accept (packet)             src/Relay/NullSink.Packet.cs:15          ~1c
├─ if (PropagateAfterAccept) Next?.Enqueue (Fork only)
├─ if (Next is { } next) next.Enqueue       (fallback)
└─ Interlocked.Increment(_dropCount)       src/Relay/PacketSink.cs:77               ~25c   (terminal drop only)
```

Consumer path — typed SPSC (`SpscQueueSink<T>.ConsumeLoop`):
```
ConsumeLoop                                src/Relay/SpscQueueSink.cs:156           loop
├─ ShouldKeepDraining                      src/Relay/SpscQueueSink.cs:248           ~3c
├─ SpscRingBuffer.TryConsumeBatch          src/Relay/Buffers/SpscRingBuffer.cs:122  ~5+2N c per batch (single mfence amortized)
├─ WriteToBackend (per item, devirt sealed)
│  ├─ FileStreamSink.WriteToBackend        src/Relay/Sinks/FileStreamSink.cs:45     ~9c    + 5e3c every 4096
│  ├─ TcpSink.WriteToBackend               src/Relay/Sinks/TcpSink.cs:64            ~9c    + 2e3c every 4096
│  └─ MmfSink.WriteToBackend               src/Relay/Sinks/MmfSink.cs:61            ~30c
├─ idle: SpinWait(20) → Yield → Sleep(1)   src/Relay/SpscQueueSink.cs:181-194      QPC throttled to every 8 spins
├─ HfClock.NowTicks                        src/Relay/Internal/HfClock.cs:8          ~20c
└─ on flushDeadline: FlushBackend → TryRecoverBackend → TryDrainToPrev
```

Consumer path — packet SPSC (`SpscQueueSink.ConsumeLoop`):
```
ConsumeLoop                                src/Relay/SpscQueueSink.Packet.cs:110    loop
├─ SpscByteRingBuffer.TryPeek              src/Relay/Buffers/SpscByteRingBuffer.cs:144   ~15c (zero-copy span)
├─ WriteToBackend (devirt sealed)          per backend
│  ├─ FileSink.WriteToBackend              src/Relay/Sinks/FileSink.cs:45           ~10c
│  ├─ TcpSink.WriteToBackend (packet)      src/Relay/Sinks/TcpSink.Packet.cs:47     ~18c
│  ├─ UdpSink.WriteToBackend               src/Relay/Sinks/UdpSink.cs:41            ~2e3c (syscall per record)
│  ├─ NamedPipeSink.WriteToBackend         src/Relay/Sinks/NamedPipeSink.cs:39      ~18c
│  ├─ UnixSocketSink.WriteToBackend        src/Relay/Sinks/UnixSocketSink.cs:41     ~18c
│  ├─ FileSink/RotatingFileSink.WriteToBackend   src/Relay/Sinks/RotatingFileSink.cs:53  ~10c + ~50c rotation check (DateTime.UtcNow.Date)
│  └─ BatchSink.WriteToBackend             src/Relay/BatchSink.cs:44                ~10c
├─ Advance(advanceBytes)                   src/Relay/Buffers/SpscByteRingBuffer.cs:177  ~2c
├─ inner batch up to 256                                                              avoids re-entering idle path
└─ idle / flush identical to typed
```

## 4. Allocation Map (top 10 by bytes/call)

| # | bytes/call | symbol | file:line | kind |
|---|---|---|---|---|
| 1 | capacity×64B | `RamSink<T>.ctor` (default 8M slots, T=64B → 512 MiB) | src/Relay/Sinks/RamSink.cs:32 | `NativeMemory.AllocZeroed` (off-heap) |
| 2 | 4 MiB | `RamSink.ctor` (packet, default) | src/Relay/Sinks/RamSink.Packet.cs:41 | `NativeMemory.AlignedAlloc(64)` (off-heap, no zero) |
| 3 | capacity×sizeof(T) | `SpscRingBuffer<T>.ctor` | src/Relay/Buffers/SpscRingBuffer.cs:72 | `NativeMemory.AlignedAlloc(64)` (off-heap) |
| 4 | capacity×sizeof(Slot) | `MpscRingBuffer<T>.ctor` (Slot = 4 + sizeof(T)) | src/Relay/Buffers/MpscRingBuffer.cs:91 | `NativeMemory.AlignedAlloc(64)` (off-heap) |
| 5 | capacity bytes | `SpscByteRingBuffer.ctor` / `MpscByteRingBuffer.ctor` | src/Relay/Buffers/SpscByteRingBuffer.cs:76 | `GC.AllocateArray<byte>(pinned)` (POH) |
| 6 | 4096×sizeof(T) | `FileStreamSink<T>.ctor` / `TcpSink<T>.ctor` write/send buffer | src/Relay/Sinks/FileStreamSink.cs:41 | `GC.AllocateArray<byte>(pinned)` (POH) |
| 7 | 64-65 KB | packet `FileSink` / `RotatingFileSink` / `TcpSink` / `NamedPipeSink` / `UnixSocketSink` / `UdpSink` send/write buffer | src/Relay/Sinks/FileSink.cs:41 | `GC.AllocateArray<byte>(pinned)` (POH) |
| 8 | batchCapacity | `BatchSink.ctor` scratch | src/Relay/BatchSink.cs:34 | `GC.AllocateUninitializedArray<byte>(pinned)` (POH) |
| 9 | 256×sizeof(T) | `SpscQueueSink<T>` / `MpscQueueSink<T>` consume buffer | src/Relay/SpscQueueSink.cs:73 | `GC.AllocateArray<T>(pinned)` (POH) |
| 10 | 0 | steady-state hot path (producer + consumer) | — | zero-alloc invariant holds across both hierarchies |

## 5. Anti-Pattern Offenders

| severity | rule | symbol | file:line | evidence |
|---|---|---|---|---|
| **block** | `DateTime.UtcNow` on hot consumer path | `RotatingFileSink.ShouldRotate` | src/Relay/Sinks/RotatingFileSink.cs:85 | `DateTime.UtcNow.Date` called per `WriteToBackend` (every payload). CLAUDE.md hard-bans `DateTime.UtcNow` on hot paths. Replace with cached-day check via `HfClock.NowTicks` + threshold. |
| warn | unbatched syscall on hot path | `UdpSink.WriteToBackend` | src/Relay/Sinks/UdpSink.cs:50 | `Socket.Send` per record (~2000c). UDP is fire-and-forget, but a multi-payload batch via `SendPackets` or write buffer would amortize. |
| warn | LINQ + alloc in cold path | `RotatingFileSink.Cleanup` | src/Relay/Sinks/RotatingFileSink.cs:160 | `Directory.GetFiles` + `OrderByDescending.Skip.ToArray()` allocates iterator + array. Cold path (rotation only) — acceptable but flagged. |
| info | virtual/devirt branch | `DispatchSink<T>.Enqueue` / `PacketSink.Enqueue` | src/Relay/DispatchSink.cs:44 | 11 `DispatchSink<T>` + 9 `PacketSink` subclasses; mitigated when caller holds sealed concrete type (devirt). `PropagateAfterAccept` reduced to a non-virtual field load (saves 1 vtable slot per Enqueue). |
| info | delegate invoke | `FilterSink*.Accept` | src/Relay/FilterSink.cs:33 | 8c per call; user extension point. |
| info | `Thread.Sleep(1)` idle branch | `*.ConsumeLoop` | src/Relay/SpscQueueSink.cs:193 | 1e6c only when ring empty after spin+yield. No producer impact. |
| info | `foreach` over array | `MultiSink.Accept` / `IsHealthy` | src/Relay/MultiSink.cs:31 | array enumerator is struct → indexer; no alloc. |
| info | `Interlocked.Increment` on terminal drop | `PacketSink.Enqueue` | src/Relay/PacketSink.cs:77 | `_dropCount` increment — only on cold drop path (Next null AND local fail). Adds ~25c there. Not on accepted-path. |
| info | `stackalloc byte[4]` on hot path | `SharedMemorySink.Accept` | src/Relay/Sinks/SharedMemorySink.cs:108 | 0 heap; ~1c stack-pointer adjust. |
| none | heap alloc on `Enqueue`/`Accept`/`TryPublish`/`TryConsume` | — | — | none detected on either hierarchy. |
| none | `lock` / `Monitor` / `async` / boxing / LINQ on hot path | — | — | not present on producer or consumer steady state. |

## 6. Cache-Line Report

| symbol | sizeof | issue | file:line |
|---|---|---|---|
| `PaddedLong` | 128 | intentional — `[FieldOffset(64)]` Value sits on its own cache line; prevents producer-consumer false sharing | src/Relay/Buffers/SpscRingBuffer.cs:10 |
| `SpscRingBuffer<T>` head/tail/cachedTail/cachedHead | 4×128 = 512B header | producer-only `_tail` + `_cachedHead`, consumer-only `_head` + `_cachedTail` — no cross-thread cache-line bounce | src/Relay/Buffers/SpscRingBuffer.cs:32 |
| `MpscRingBuffer<T>` claimedTail/headCache/head | 3×128 = 384B header | matches Log2 FIX #18 — producer CAS, producer head-cache read, consumer head write all isolated | src/Relay/Buffers/MpscRingBuffer.cs:50 |
| `SpscByteRingBuffer` head/tail/cachedTail/cachedHead | 4×128 | same pattern, byte payload | src/Relay/Buffers/SpscByteRingBuffer.cs:43 |
| `MpscByteRingBuffer` claimedTail/headCache/head | 3×128 | same pattern, byte payload | src/Relay/Buffers/MpscByteRingBuffer.cs:64 |
| `MpscRingBuffer<T>.Slot { int Published; T Value; }` | 4 + sizeof(T) | unpadded; T multiple of 64B → slot ≥68B with 4-byte head, **adjacent slots may straddle cache lines** | src/Relay/Buffers/MpscRingBuffer.cs:40 |
| `T` payload | 32/64/128/256 | enforced by `SinkConstraints.AssertCacheLineAligned<T>()` (DEBUG) | src/Relay/Internal/SinkConstraints.cs |
| `MmfSink<T>._position` | 8B scalar | written by consumer via `Volatile.Write`, read by producer in `IsHealthy` via `Volatile.Read` — shares cache line with sibling fields → minor producer-side L1 miss on flush | src/Relay/Sinks/MmfSink.cs:36 |
| `SharedMemorySink` WriteIdx/ReadIdx | offsets 8 / 64 in MMF header | `[0..63]` line carries WriteIdx (and Magic/Capacity); `[64..127]` line carries ReadIdx — header layout is explicit per Log2 wire spec, single cache line per index. | src/Relay/Sinks/SharedMemorySink.cs:36 |
| `RamSink<T>._head`/`_tail` | 8B each, adjacent | single-thread contract; no false-sharing concern | src/Relay/Sinks/RamSink.cs:21 |
| `RamSink._head`/`_tail` (packet) | int×2 adjacent | SPSC non-concurrent contract (caller quiesces producer before drain) | src/Relay/Sinks/RamSink.Packet.cs:30 |

## 7. Syscalls & Kernel Boundaries

| symbol | file:line | kind | cycles | notes |
|---|---|---|---|---|
| `FileStream.Write` | src/Relay/Sinks/FileStreamSink.cs:91 | file I/O | ~5e3 | batched ~4096 items |
| `FileStream` ctor (Append) | src/Relay/Sinks/FileSink.cs:102 | file open | ~5e3 | one-time + on recovery |
| `Socket.Connect` (TCP) | src/Relay/Sinks/TcpSink.cs:183 | TCP handshake | ~5e6 | RTT-bound, cold path |
| `Socket.Send` (TCP) | src/Relay/Sinks/TcpSink.cs:124 | socket send | ~2e3 | batched in send buffer |
| `Socket.Send` (UDP) | src/Relay/Sinks/UdpSink.cs:50 | datagram | ~2e3 | **per record** — no batching |
| `Socket.Send` (Unix) | src/Relay/Sinks/UnixSocketSink.cs:57 | unix socket | ~1e3 | batched |
| `NamedPipeClientStream.Write` | src/Relay/Sinks/NamedPipeSink.cs:55 | named-pipe I/O | ~5e3 | batched |
| `NamedPipeClientStream.Connect(100ms)` | src/Relay/Sinks/NamedPipeSink.cs:85 | named-pipe connect | ~5e6 | cold path, 100ms timeout |
| `MemoryMappedViewAccessor.Flush` | src/Relay/Sinks/MmfSink.cs:77 | msync | ~2e3 | per flush interval |
| `MemoryMappedFile.CreateFromFile` / `CreateOrOpen` | src/Relay/Sinks/MmfSink.cs:52 | kernel mapping | ~5e3 | one-time |
| `SafeBuffer.AcquirePointer` | src/Relay/Sinks/MmfSink.cs:57 | pin handle | ~50 | one-time |
| `VirtualLock` | src/Relay/Memory/RelayMemory.cs:51 | NT kernel | ~2e3 | one-time, best-effort |
| `Stopwatch.GetTimestamp` (RDTSC) | src/Relay/Internal/HfClock.cs:11 | CPU instruction | ~20 | not a kernel boundary |
| `Thread.Start` | src/Relay/SpscQueueSink.cs:88 | kernel thread | ~1e5 | one-time |
| `Thread.Join` | src/Relay/SpscQueueSink.cs:98 | kernel wait | ~1e3-5e3 | on Stop/Dispose |
| `Thread.Sleep(1)` | src/Relay/SpscQueueSink.cs:193 | kernel yield | ~1e6 | idle branch only |
| `Thread.Yield` | src/Relay/SpscQueueSink.cs:188 | kernel | ~300 | idle branch only |
| `NativeMemory.AlignedAlloc` / `AllocZeroed` | src/Relay/Buffers/SpscRingBuffer.cs:72 | heap | ~60 + zero | one-time; zero dominates at 512 MiB |
| `NativeMemory.AlignedFree` / `Free` | src/Relay/Buffers/SpscRingBuffer.cs:207 | heap | ~60 | dispose only |
| `Directory.GetFiles` + `File.Delete` | src/Relay/Sinks/RotatingFileSink.cs:160 | dir scan + unlink | ~1e4 | rotation-time only |

## 8. Blind Subgraphs

| symbol | file:line | reason | hint-key |
|---|---|---|---|
| `Predicate<T>` body in `FilterSink<T>` | src/Relay/FilterSink.cs:33 | caller-supplied delegate; cost unknown | filter.predicate.cost |
| `PacketPredicate` body in `FilterSink` | src/Relay/FilterSink.Packet.cs:44 | caller-supplied delegate; cost unknown | filter.packet.predicate.cost |
| backend I/O latency (FS / TCP / UDP / Unix / NamedPipe) | src/Relay/Sinks/*.cs | OS + device + network dependent; model uses fixed syscall estimate only | io.device.latency |
| JIT devirtualization assumption | DispatchSink.cs / PacketSink.cs / Multi2Sink | effective only when caller holds sealed concrete type | jit.devirt.profile |
| `VirtualLock` NT kernel path | src/Relay/Memory/RelayMemory.cs:48 | best-effort; SeLockMemoryPrivilege required; failure silent | nt.virtuallock.privilege |
| MPSC CAS-retry distribution | `MpscRingBuffer.TryPublish` / `MpscByteRingBuffer.TryPublish` | retry count under contention is workload-dependent; HeadCache eliminates the steady-state miss but full-ring cases still bounce `_head` cross-core | mpsc.contention.profile |

## 9. Delta vs 2026-04-23

| change | symbol | file:line | weight before → after | verdict |
|---|---|---|---|---|
| rename | `*Pipe` → `*Sink` across all hierarchy | all | n/a | naming-only; cost identical to prior snapshot |
| new hierarchy | `PacketSink` + concrete byte-payload chain | src/Relay/PacketSink.cs:15 | n/a → ~15c Enqueue, ~25c terminal drop | parallel typed-tree; Interlocked.Increment on drop adds 25c only when Next null AND local fail |
| new | `PacketSink.TryEnqueue` (non-fallthrough variant) | src/Relay/PacketSink.cs:54 | n/a → ~13c | strict-accept variant for never-drop callers |
| new | `MpscQueueSink<T>` + `MpscRingBuffer<T>` | src/Relay/MpscQueueSink.cs:26 | n/a → ~30c TryPublish (uncontended); ~30c Accept | one CAS per write; HeadCache eliminates the steady-state cross-core `_head` read |
| new | `MpscQueueSink` (packet) + `MpscByteRingBuffer` | src/Relay/MpscQueueSink.Packet.cs:24 | n/a → ~50c TryPublish (uncontended) | CAS reservation + 2× volatile header publish on wrap |
| new | `SpscByteRingBuffer` + length-prefixed records | src/Relay/Buffers/SpscByteRingBuffer.cs:30 | n/a → ~35c TryPublish, ~15c TryPeek | wrap padding marker keeps payload contiguous → zero-copy span |
| new | `ForkSink<T>` / `ForkSink` (canonical PropagateAfterAccept=true) | src/Relay/ForkSink.cs:24 | replaces ad-hoc tee pattern | sealed const-true → JIT folds propagate branch in `Enqueue` |
| new | `SerializeSink<T>` typed→packet bridge | src/Relay/SerializeSink.cs:18 | n/a → ~5c + downstream | `MemoryMarshal.AsBytes` ref-cast, zero copy |
| new | `BatchSink` abstract scratch-buffer batcher | src/Relay/BatchSink.cs:21 | n/a → ~10c WriteToBackend | accumulates on consumer thread, periodic `OnFlush(batch)` |
| new backends | `UdpSink`, `NamedPipeSink`, `RotatingFileSink`, `SharedMemorySink`, `UnixSocketSink`, `FileSink`, `TcpSink` (packet) | src/Relay/Sinks/*.cs | n/a | populate packet hierarchy |
| changed | `DispatchSink<T>.Enqueue` adds `PropagateAfterAccept` field test | src/Relay/DispatchSink.cs:48 | ~15c → ~15c (sealed const folds) | non-virtual field saves 1 vtable slot vs prior virtual-prop design |
| changed | `SpscRingBuffer<T>` allocation switched to `NativeMemory.AlignedAlloc(64)` | src/Relay/Buffers/SpscRingBuffer.cs:72 | POH+8B align → off-heap+64B align | first slot now starts on cache-line boundary; eliminates straddle for T ≥ 64B |
| changed | `SpscQueueSink<T>` consumer uses `TryConsumeBatch` (256-item) | src/Relay/SpscQueueSink.cs:167 | per-item Volatile.Write head → 1 per batch | (N-1) mfences saved per batch |
| changed | `SpscQueueSink` (packet) consumer batches 256 `TryPeek/Advance` per loop iter | src/Relay/SpscQueueSink.Packet.cs:127 | per-record idle path → batched | reduces idle-spin entry rate; flush deadline still honored |
| changed | `Flush()` now signals `_flushRequested` (consumer-only `FlushBackend`) | src/Relay/SpscQueueSink.cs:147 | producer-side flush → consumer-thread-only | eliminates producer/consumer race over backend write buffer |
| changed | clear-before-run on `_flushRequested` | src/Relay/SpscQueueSink.cs:211 | overwrite race possible → fixed | per-MpscQueueSink.Flush race audit; same in Spsc/packet variants |
| confirmed | `Multi2Sink<T,TC1,TC2>` saves ~6c vs array `MultiSink<T>` (N=2) | src/Relay/MultiSink.cs:70 | identical to prior snapshot | |
| confirmed | `FilterSink*.Accept` returns true on predicate miss → silent consume, never propagates to Next | src/Relay/FilterSink.cs:31 | semantics preserved | |
| confirmed | `RamSink<T>.Accept` is fastest local write (~7c) | src/Relay/Sinks/RamSink.cs:39 | unchanged | |
| confirmed | `MmfSink<T>.WriteToBackend` ~30c — fastest durable backend | src/Relay/Sinks/MmfSink.cs:61 | unchanged | |
| **regression** | `RotatingFileSink.ShouldRotate` calls `DateTime.UtcNow.Date` per record | src/Relay/Sinks/RotatingFileSink.cs:85 | n/a → +50c per consumed payload | violates CLAUDE.md "no `DateTime.UtcNow` on hot paths"; consumer thread but ULTRA-HOT under load. Replace with `HfClock.NowTicks` + cached threshold; cache `_currentDay` against tick boundary. |
| **regression-candidate** | `UdpSink.WriteToBackend` issues `Socket.Send` per record | src/Relay/Sinks/UdpSink.cs:50 | n/a → 2e3c per payload | UDP is per-datagram by design but a 2e3c syscall per payload caps throughput at ~1.5M payloads/s/core; consider `SendPackets` or burst-write coalescing for high-rate use. |
| risk | `MpscRingBuffer<T>.Slot` unpadded → adjacent slots may straddle cache lines for T not aligned to 64B factor | src/Relay/Buffers/MpscRingBuffer.cs:40 | known | acceptable while T is enforced multiple of 64B; flag if `SinkConstraints` ever loosens |
| risk | drain-to-Prev SPSC/MPSC race window during recovery | src/Relay/SpscQueueSink.cs:238 | known narrow window | callers must quiesce producers before drain; documented in ConsumeLoop comment |
