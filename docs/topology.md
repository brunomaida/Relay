# Relay Library — Topology

```
================================================================================
                         TYPE HIERARCHY
================================================================================

  DispatchPipe<T>  (abstract, T : unmanaged)
       │
       │  Next: DispatchPipe<T>?      ← set by PipeChain.To()
       │  IsHealthy: bool             ← abstract; consumer thread writes, producer reads
       │  Enqueue(in T)               ← sealed hot path: IsHealthy && Accept || Next?.Enqueue
       │  Accept(in T): bool          ← abstract; returns false to trigger fallback
       │  Flush() / Dispose()         ← abstract lifecycle
       │
       ├── SpscQueuePipe<T>  (abstract)
       │       │  _ring: SpscRingBuffer<T>     ← SPSC lock-free, POH pinned
       │       │  _healthy: volatile bool      ← consumer thread only
       │       │  Prev: DispatchPipe<T>?       ← set by PipeChain.To() on fallback nodes
       │       │  IsHealthy => _healthy && !_ring.IsFull
       │       │  Accept   => _ring.TryPublish(item)
       │       │  Start() / Stop(ms)           ← spawns / joins consumer thread
       │       │  IsConsuming / ConsumerException
       │       │
       │       ├── FileStreamPipe<T>   (sealed)  ← FileStream + POH write buffer
       │       ├── TcpPipe<T>          (sealed)  ← TcpClient + POH send buffer
       │       ├── MmfPipe<T>          (sealed)  ← MemoryMappedViewAccessor
       │       └── RamPipe<T>  ──────────────────── (NOT SpscQueuePipe — see below)
       │
       ├── MultiPipe<T>    (sealed)     ← broadcast to DispatchPipe<T>[] children
       ├── Multi2Pipe<T, TC1, TC2>  (sealed)  ← CRTP 2-child, JIT devirtualizes
       ├── ForkPipe<T>     (sealed)    ← primary + Next propagation (propagate-after-accept)
       ├── FilterPipe<T>   (sealed)    ← conditional gate + downstream pipe
       └── NullPipe<T>     (sealed)    ← singleton no-op sink

  RamPipe<T>  (sealed)  ← direct subclass of DispatchPipe<T>
       │  _buffer: T* (NativeMemory.AllocZeroed)
       │  _head / _tail: long  ← single-threaded, not SPSC ring
       │  IsHealthy => _tail - _head < _capacity
       │  Accept   => native pointer write
       │  DrainTo(DispatchPipe<T>) ← called externally on recovery


================================================================================
                         ASSEMBLY GRAPH
================================================================================

  Relay.csproj
  ├── namespace Relay          DispatchPipe, SpscQueuePipe, MpscQueuePipe,
  │                            MultiPipe, Multi2Pipe, ForkPipe,
  │                            FilterPipe, NullPipe
  ├── namespace Relay.Buffers  SpscRingBuffer<T>            [internal]
  ├── namespace Relay.Pipes    FileStreamPipe, TcpPipe, MmfPipe, RamPipe
  ├── namespace Relay.Builder  RelayBuilder, PipeChain<T,THead>
  ├── namespace Relay.Memory   RelayMemory                  [internal]
  └── namespace Relay.Internal HfClock, PipeConstraints     [internal]

  Relay.Tests.csproj
  └── InternalsVisibleTo → Relay  (accesses Next internal set, Prev internal set)


================================================================================
                         ENQUEUE HOT PATH
================================================================================

  caller.Enqueue(in item)          [producer thread]
       │
       ▼
  DispatchPipe<T>.Enqueue(in T item):
  ┌────────────────────────────────────────────────────────────┐
  │  if (IsHealthy && Accept(in item)) return;  ← short-circuit │
  │  Next?.Enqueue(in item);                    ← fallback      │
  └────────────────────────────────────────────────────────────┘
       │
       ├─ IsHealthy = true, Accept = true
       │      → item delivered locally, return
       │
       ├─ IsHealthy = false  (backend broken or ring full)
       │      → Accept not called (short-circuit)
       │      → Next?.Enqueue(item)   [hop to fallback]
       │
       ├─ IsHealthy = true, Accept = false  (ring full during call)
       │      → Next?.Enqueue(item)   [hop to fallback]
       │
       └─ Next == null
              → silent drop (no exception, no log)

  Gate semantics:
  ┌──────────────┬───────────────────────────────────────────────────────┐
  │ Gate         │ Who writes / What detects                             │
  ├──────────────┼───────────────────────────────────────────────────────┤
  │ IsHealthy    │ Consumer thread (_healthy flag) or capacity check     │
  │              │ → backend broken  OR ring full at moment of check     │
  ├──────────────┼───────────────────────────────────────────────────────┤
  │ Accept(item) │ Result of _ring.TryPublish — ring full during call    │
  │              │ → definitive gate; race between IsHealthy and TryPub  │
  └──────────────┴───────────────────────────────────────────────────────┘


================================================================================
                         CHAIN TOPOLOGIES
================================================================================

  T1 — Single SpscQueuePipe (depth 1)
  ─────────────────────────────────
  caller ──▶ [FileStreamPipe]

  T2 — Serial depth 2
  ───────────────────
  caller ──▶ [FileStreamPipe] ──(Next)──▶ [RamPipe]
             unhealthy? ──────────────────────────▶

  T3 — Serial depth 3
  ───────────────────
  caller ──▶ [FileStreamPipe] ──▶ [TcpPipe] ──▶ [RamPipe]

  T4 — Multi broadcast to 2 children
  ──────────────────────────────────
  caller ──▶ [MultiPipe] ──▶ c1 [FileStreamPipe]
                         └──▶ c2 [TcpPipe]
             all-fail? ──▶ (Next / drop)

  T5 — Multi + serial fallback
  ────────────────────────────
  caller ──▶ [MultiPipe] ──▶ c1 [FileStreamPipe]
             │             └──▶ c2 [TcpPipe]
             └──(Next)──▶ [RamPipe]         ← only when all children unhealthy

  T6 — Filter gate
  ────────────────
  caller ──▶ [FilterPipe] ──(predicate true)──▶ [FileStreamPipe]
                           ──(predicate false)─▶ (consumed, no fallback)

  T7 — Multi2 CRTP variant (sealed TC1, TC2 → JIT devirtualizes)
  ───────────────────────────────────────────────────────────────
  caller ──▶ [Multi2Pipe<Entry, FileStreamPipe<Entry>, TcpPipe<Entry>>]
                          ├── _c1.Enqueue(item)   [inlined by JIT]
                          └── _c2.Enqueue(item)   [inlined by JIT]

  T8 — Fork (propagate-after-accept)
  ──────────────────────────────────
  caller ──▶ [ForkPipe(primary=Audit)] ──▶ Audit.Enqueue   [local]
                                       └──(Next)──▶ [MainPipe]


================================================================================
                         MULTI ROUTING RULES
================================================================================

  MultiPipe<T>.IsHealthy:
    short-circuit OR over children:
      for each child: if child.IsHealthy → return true
      (all fail) → return false → triggers Next?.Enqueue

  MultiPipe<T>.Accept(item):
    for each child: child.Enqueue(item)   ← always, regardless of health
    return true                            ← never triggers fallback via Accept

  Key: fallback to Next only when IsHealthy = false (all children unhealthy).
  Partial failure (some healthy, some not) → healthy children receive; unhealthy drop.

  FilterPipe<T>.Accept(item):
    if predicate(item): _downstream.Enqueue(item)
    return true         ← always; filtered items do NOT propagate to Next


================================================================================
                         SpscQueuePipe — CONSUMER THREAD
================================================================================

  [Producer thread]              [Consumer thread: "relay-{pipeName}"]
       │                                │
  Enqueue(item)                  Start() → new Thread(ConsumeLoop)
       │                                │
  _ring.TryPublish ──────────────▶  ConsumeLoop():
  (Volatile.Write tail)              ┌─────────────────────────────────┐
                                     │  while ShouldKeepDraining():    │
                                     │    TryConsume → WriteToBackend  │
                                     │    batch up to 256 entries      │
                                     │    idle: SpinWait→Yield→Sleep   │
                                     │    flush check:                 │
                                     │      after batch: always        │
                                     │      spin phase: every 8 iters  │
                                     │      yield/sleep: always        │
                                     │    on flush deadline:           │
                                     │      FlushBackend()             │
                                     │      TryRecoverBackend()        │
                                     │      TryDrainToPrev()           │
                                     └─────────────────────────────────┘
                                            │         │
                                     catch(Exception) │
                                       _consumerException = ex
                                       _healthy stays false
                                            │
                                     finally: FlushBackend, DisposeBackend

  ShouldKeepDraining():
    _running = true    → continue
    ring empty         → stop
    within deadline    → continue draining (Stop sets _drainDeadlineTicks)
    past deadline      → stop

  Stop(drainTimeoutMs):
    Volatile.Write(_drainDeadlineTicks, now + timeout)
    _running = false
    _thread.Join(timeout)


================================================================================
                         RECOVERY — Prev DRAIN
================================================================================

  Setup (by PipeChain.To):
    .Start<T, P1>(p1).To(p2).Build()
         │
         └─ p2 is SpscQueuePipe<T>? → p2.Prev = p1

  Runtime: TryDrainToPrev() called on flush interval by p2's consumer thread:

  [p2 consumer thread]
       │
       ├─ p2.Prev (= p1) healthy?  → drain ring back to p1
       │      while ring.TryConsume(out item):
       │          if !p1.IsHealthy: break
       │          p1.Enqueue(item)          ← items routed upstream
       │
       └─ p1 still unhealthy → skip (items remain in p2's ring)

  Use case:
    p1 = FileStreamPipe (primary, was broken)
    p2 = RamPipe (fallback, accumulated items during p1 outage)
    On p1 recovery → p2 drains back to p1 → normal path resumes

  Only SpscQueuePipe / MpscQueuePipe nodes get Prev. DispatchPipe subclasses
  without a consumer thread (Multi, Fork, Filter, Null, Ram) do not participate.


================================================================================
                         BUILDER ASSEMBLY
================================================================================

  RelayBuilder.Start<T, THead>(head)          ← generic entry
  RelayBuilder.StartSpsc<T, THead>(head)      ← THead : SpscQueuePipe<T>
  RelayBuilder.StartMpsc<T, THead>(head)      ← THead : MpscQueuePipe<T>
       │
       └─▶ PipeChain<T, THead> { _head = head, _tail = head }

  Operators on PipeChain<T, THead>:

  .To(next):
       if next is SpscQueuePipe<T> / MpscQueuePipe<T>:
           next.Prev = _tail          ← recovery drain link
       _tail.Next = next              ← fallback routing link
       _tail = next

  .Fork(primary):
       fork = new ForkPipe<T>(primary)
       _tail.Next = fork ; _tail = fork

  .When(pred).To(downstream):
       filter = new FilterPipe<T>(pred, downstream)
       _tail.Next = filter            ← filter.Next never fires (Accept=true always)
       _tail = downstream             ← subsequent .To extends downstream's chain

  .Multi(cfg)      (cfg builds a MultiBuilder<T> → MultiPipe<T>)
  .Multi<TC1,TC2>(c1, c2)    ← sealed CRTP variant (Multi2Pipe)

  chain.Build():
       return _head                   ← typed head, ready to Enqueue

  Example assembly:
  ┌──────────────────────────────────────────────────────────────────┐
  │  var head = RelayBuilder                                         │
  │      .Start<Entry, FileStreamPipe<Entry>>(file)                  │
  │      .To(tcp)                                                    │
  │      .To(ram)                                                    │
  │      .Build();                                                   │
  │                                                                  │
  │  Wired state:                                                    │
  │    file.Next = tcp                                               │
  │    tcp.Next  = ram          (tcp is SpscQueuePipe → tcp.Prev = file)  │
  │    ram.Next  = null         (ram is DispatchPipe → no Prev)      │
  │                                                                  │
  │  ram.Prev is NOT set — RamPipe is not SpscQueuePipe.             │
  │  Recovery drain is tcp → file (tcp drains to its predecessor).   │
  └──────────────────────────────────────────────────────────────────┘


================================================================================
                         SPSC RING BUFFER PROTOCOL
================================================================================

  Memory layout (false-sharing prevention):
  ┌─────────────────────────────┐   ┌─────────────────────────────┐
  │  _head  (PaddedLong, 128B)  │   │  _tail  (PaddedLong, 128B)  │
  │  Value at offset [64]       │   │  Value at offset [64]       │
  │  Written by: consumer only  │   │  Written by: producer only  │
  │  Read by:    producer       │   │  Read by:    consumer       │
  └─────────────────────────────┘   └─────────────────────────────┘
  ←── separate cache lines ──────────────────────────────────────────▶

  TryPublish(in T item):                  TryConsume(out T item):
  ──────────────────────                  ──────────────────────
  tail = _tail.Value                      head = _head.Value
  wrap = tail - Capacity                  if head >= _cachedTail:
  if _cachedHead <= wrap:                     _cachedTail = Volatile.Read(_tail)
      _cachedHead = Volatile.Read(_head)      if head >= _cachedTail:
      if _cachedHead <= wrap:                     item = default; return false [EMPTY]
          return false  [FULL]            item = buf*[head & mask]
  buf*[tail & mask] = item                Volatile.Write(_head, head+1)
  Volatile.Write(_tail, tail+1)           return true
  return true

  * Unsafe.Add(GetArrayDataReference, idx) — no bounds check (mask guarantees range).

  _cachedHead / _cachedTail: producer-only / consumer-only snapshots of the remote index.
  Volatile.Read of the remote index only occurs when the buffer appears full or empty.
  On the fast path (buffer has space / has items) the cross-core read is skipped entirely.

  IsFull:
    _tail.Value - Volatile.Read(_head.Value) >= Capacity
    (read by producer as part of IsHealthy check; no cache used here — cold path)

  Batched-write API (for Multi2 with shared mfence):
    TryReserveTail(out tail) → reserve slot without write (uses _cachedHead)
    WriteSlot(tail, in item) → Unsafe.Add write, no fence
    Thread.MemoryBarrier()   → single mfence for N writes
    CommitTail(tail)         → Volatile.Write advances tail


================================================================================
                         CONCRETE PIPES — BACKEND DETAIL
================================================================================

  FileStreamPipe<T>
  ─────────────────
  Ring → [consumer thread] → POH write buffer (4096 × sizeof(T)) → FileStream
                               on full OR flush interval: FileStream.Write(span)
  Failure:  IOException in FlushBuffer → _healthy = false
  Recovery: TryRecoverBackend — reopen FileStream, re-flush buffer
            backoff: 1s → 2s → 4s … → 60s (exponential, capped)
  Default:  ringCapacity=524288, flushInterval=250ms

  TcpPipe<T>
  ──────────
  Ring → [consumer thread] → POH send buffer (4096 × sizeof(T)) → NetworkStream
                               on full OR flush interval: NetworkStream.Write(span)
  Failure:  Exception in FlushBuffer → _healthy = false
  Recovery: TryRecoverBackend — dispose+reconnect TcpClient, re-flush buffer
            backoff: 1s → 2s … → 30s (exponential, capped)
  Default:  ringCapacity=16384, flushInterval=250ms

  MmfPipe<T>
  ──────────
  Ring → [consumer thread] → MemoryMappedViewAccessor.Write<T>(_position, ref item)
                               on flush interval: _view.Flush()
  Failure:  capacity exhaustion (_position + sizeof(T) > maxBytes) → IsHealthy = false
  Recovery: none — capacity is permanent; TryRecoverBackend is a no-op
  Note:     MemoryMappedViewAccessor.Write never throws IOException

  RamPipe<T>  (direct DispatchPipe<T> subclass — no consumer thread)
  ────────────────────────────────────────────────────────────────────
  Enqueue → Accept → native pointer write (_buffer[_tail & _mask] = item; _tail++)
  No threading: single-threaded, synchronous write (hot path thread)
  Failure:  ring full (_tail - _head >= _capacity) → IsHealthy = false
  Recovery: DrainTo(target) — called externally by the recovery coordinator
            Drains in FIFO order; stops if target becomes unhealthy
  Dispose:  NativeMemory.Free(_buffer)
  Default:  capacity = 1 << 23  (~8M entries, ~512MB for T=64B)


================================================================================
                         THREADING MODEL
================================================================================

  Thread             Priority       Owner                  Role
  ────────────────────────────────────────────────────────────────────────────
  Producer (caller)  (any)          Application            Calls Enqueue
  relay-file         BelowNormal    FileStreamPipe         Drain ring → FileStream
  relay-tcp          BelowNormal    TcpPipe                Drain ring → NetworkStream
  relay-mmf          BelowNormal    MmfPipe                Drain ring → MemoryMappedFile
  relay-{name}       BelowNormal    SpscQueuePipe subclass Custom backend

  RamPipe has NO dedicated thread. Writes happen synchronously on the producer thread.
  MultiPipe, ForkPipe, FilterPipe, NullPipe have NO threads. Pure hot-path dispatch.

  Synchronization:
  ┌────────────────────────┬──────────────────────────────────────────────────┐
  │ Primitive              │ Where used                                       │
  ├────────────────────────┼──────────────────────────────────────────────────┤
  │ Volatile.Write (tail)  │ SpscRingBuffer.TryPublish  (producer)            │
  │ Volatile.Read  (tail)  │ SpscRingBuffer.TryConsume  (consumer)            │
  │ Volatile.Write (head)  │ SpscRingBuffer.TryConsume  (consumer)            │
  │ Volatile.Read  (head)  │ SpscRingBuffer.TryPublish / IsFull (producer)    │
  │ volatile bool _healthy │ Written: consumer thread only                    │
  │                        │ Read:    producer thread (IsHealthy check)       │
  │ volatile bool _running │ Written: Start/Stop (caller thread)              │
  │                        │ Read:    consumer thread (ConsumeLoop condition) │
  │ Thread.MemoryBarrier() │ Multi2 batched-write (shared mfence for N pubs)  │
  └────────────────────────┴──────────────────────────────────────────────────┘

  No lock / Monitor anywhere in the hot path.
  No async/await on any dispatch path.
  No Interlocked on the hot path (counters are opt-in at subclass level).


================================================================================
                         MEMORY LAYOUT
================================================================================

  SpscRingBuffer<T> (object on managed heap):
  ┌──────────────────────────────────────────────────────────────────┐
  │  object header          (8B)                                     │
  │  T[] _buffer ref        (8B) → T[] on POH (capacity × sizeof(T))│
  │  int  _mask             (4B)                                     │
  │  int  Capacity          (4B)                                     │
  │  ─── alignment gap to 128B boundary ───                          │
  │  PaddedLong _head       (128B) ← written by consumer            │
  │  PaddedLong _cachedTail (128B) ← consumer-only copy of _tail    │
  │  PaddedLong _tail       (128B) ← written by producer            │
  │  PaddedLong _cachedHead (128B) ← producer-only copy of _head    │
  └──────────────────────────────────────────────────────────────────┘

  PaddedLong:
  ┌────────────────────────────────────────────────────────────┐
  │  [StructLayout(Explicit, Size=128)]                        │
  │  [FieldOffset(64)] public long Value                       │
  │  → Value is isolated to offset 64..71 within the 128B slot │
  │  → Cannot share a cache line with adjacent PaddedLong      │
  └────────────────────────────────────────────────────────────┘

  Write buffer (FileStreamPipe / TcpPipe):
    GC.AllocateArray<byte>(4096 * sizeof(T), pinned: true)
    Lives on POH (Pinned Object Heap) — never moved by GC
    Capacity: 4096 entries × sizeof(T) bytes

  RamPipe native ring:
    NativeMemory.AllocZeroed(capacity × sizeof(T))
    Unmanaged heap — not subject to GC pressure
    Freed explicitly in Dispose via NativeMemory.Free


================================================================================
                         MEMORY INITIALIZATION (RelayMemory)
================================================================================

  SpscQueuePipe.Start() calls RelayMemory.PreFaultAndLock(ring.Buffer):

  fixed (T* ptr = array):
      for page in [0, total, step=4096]:  Volatile.Read(ptr[page])  ← pre-fault
      Volatile.Read(ptr[total - 1])                                  ← last byte
      if Windows: VirtualLock(ptr, total)  ← best-effort, SeLockMemory privilege

  Effect: all ring pages are in RAM before first Enqueue.
          VirtualLock prevents OS from paging out the ring during operation.
          Failure of VirtualLock is silently ignored (requires privilege).


================================================================================
                         PERFORMANCE REFERENCE
================================================================================

  Platform: Intel Core i9-12900K (Alder Lake P-core), hot caches, single-threaded.
  T = 64B struct (1 cache line).

  ┌─────────────────────────────────────────────────────┬──────────┬──────────┐
  │ Operation                                           │ Cycles   │ ~ns      │
  ├─────────────────────────────────────────────────────┼──────────┼──────────┤
  │ SpscRingBuffer.TryPublish (ring not full, cache hit)│   ~8-12c │   ~3 ns  │
  │ SpscRingBuffer.TryPublish (ring not full, no cache) │   ~25c   │   ~7 ns  │
  │ SpscQueuePipe.IsHealthy (healthy, L1 hot)           │    ~7c   │   ~2 ns  │
  │ SpscQueuePipe.IsHealthy (unhealthy, short-circuit)  │    ~1c   │  <1 ns   │
  │ Volatile.Write (mfence, x64)                        │   ~15c   │   ~4 ns  │
  │ Virtual call (predicted branch)                     │    ~3c   │  <1 ns   │
  │ T1: Enqueue depth-1 (success, SpscQueuePipe)        │   ~32c   │   ~9 ns  │
  │ T2: Enqueue depth-2 (success, p1 healthy)           │   ~32c   │   ~9 ns  │
  │ T2: Enqueue depth-2 (fallback, p1 unhealthy→p2)     │   ~12c   │   ~3 ns  │
  │ T3: Enqueue depth-3 (p1+p2 unhealthy→p3 RamPipe)    │   ~16c   │   ~5 ns  │
  │ T4: Multi 2 children (both healthy, array-based)    │   ~74c   │  ~21 ns  │
  │ T4: Multi 2 children (CRTP Multi2Pipe, sealed)      │   ~68c   │  ~19 ns  │
  │ T5: Multi all-fail → Next (RamPipe)                 │   ~19c   │   ~5 ns  │
  │ Drop (Next == null, depth 1)                        │    ~2c   │  <1 ns   │
  │ Drop (Next == null, depth 3)                        │   ~14c   │   ~4 ns  │
  └─────────────────────────────────────────────────────┴──────────┴──────────┘

  Key insight: fallback is often CHEAPER than success because:
    - Unhealthy short-circuit = ~1c (vs ~7c IsHealthy normal)
    - RamPipe has no mfence (vs ~15c in SpscQueuePipe.TryPublish)
    - Hot path degradation is lighter than normal hot path

  Multi scales linearly: Total ≈ N × 35c + 6c for N healthy SpscQueuePipe children.

  Default ring capacities vs memory footprint (T = 64B):
  ┌────────────────────┬────────────────┬──────────────────┐
  │ Pipe               │ Ring capacity  │ Ring memory      │
  ├────────────────────┼────────────────┼──────────────────┤
  │ FileStreamPipe<T>  │ 524,288        │  ~32 MB          │
  │ TcpPipe<T>         │  16,384        │   ~1 MB          │
  │ MmfPipe<T>         │  65,536        │   ~4 MB          │
  │ RamPipe<T>         │ 8,388,608      │ ~512 MB (native) │
  └────────────────────┴────────────────┴──────────────────┘


================================================================================
                         RECOMMENDED TOPOLOGIES
================================================================================

  Recording (primary local file):
    FileStreamPipe<T> → RamPipe<T>
    ~32c success │ ~12c fallback │ ~2c drop

  Recording with remote redundancy:
    FileStreamPipe<T> → TcpPipe<T> → RamPipe<T>
    ~32c success │ ~36c fallback-to-tcp │ ~16c fallback-to-ram

  IPC dispatch to multiple destinations:
    Multi2Pipe<T, FileStreamPipe<T>, TcpPipe<T>>  →  RamPipe<T>
    ~68c success │ ~19c all-fail-to-ram

  Symbol-filtered recording:
    FilterPipe<T> → FileStreamPipe<T>
    ~45c pass │ ~8c blocked (silently consumed)

  Rules of thumb:
    - Each additional serial hop costs 0c on the success path, +4c per unhealthy hop.
    - Multi broadcast multiplies by N. Keep N ≤ 4.
    - RamPipe as last resort adds ~0c on success path, ~6c on all-children-fail path.
    - Multi2Pipe (CRTP) saves ~6c over MultiPipe when children are sealed types.
```
