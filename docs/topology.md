# Relay Library — Topology

```
================================================================================
                         TYPE HIERARCHY
================================================================================

  DispatchSink<T>  (abstract, T : unmanaged)
       │
       │  Next: DispatchSink<T>?      ← set by SinkChain.To()
       │  IsHealthy: bool             ← abstract; consumer thread writes, producer reads
       │  PropagateAfterAccept: bool  ← protected readonly field (ctor param, default false)
       │  Enqueue(in T)               ← sealed hot path: (IsHealthy && Accept) then Next or return
       │  Accept(in T): bool          ← abstract; returns false to trigger fallback
       │  Flush() / Dispose()         ← abstract lifecycle
       │
       ├── SpscQueueSink<T>  (abstract)
       │       │  _ring: SpscRingBuffer<T>       ← SPSC lock-free, POH pinned
       │       │  _healthy: volatile bool        ← consumer thread only
       │       │  Prev: DispatchSink<T>?         ← set by SinkChain.To() on fallback nodes
       │       │  IsHealthy => _healthy && !_ring.IsFull
       │       │  Accept   => _ring.TryPublish(item)
       │       │  Start() / Stop(ms)             ← spawns / joins consumer thread
       │       │  IsConsuming / ConsumerException
       │       │
       │       ├── FileStreamSink<T>  (sealed)  ← FileStream + POH write buffer
       │       ├── TcpSink<T>         (sealed)  ← TcpClient + POH send buffer
       │       └── MmfSink<T>         (sealed)  ← MemoryMappedViewAccessor
       │
       ├── MpscQueueSink<T>  (abstract)
       │       │  _ring: MpscRingBuffer<T>       ← MPSC lock-free (Log2 FIX #18 layout)
       │       │  Prev: DispatchSink<T>?
       │       │  (same lifecycle API as SpscQueueSink<T>)
       │
       ├── MultiSink<T>      (sealed)  ← broadcast to DispatchSink<T>[] children
       ├── Multi2Sink<T, TC1, TC2>  (sealed)  ← CRTP 2-child, JIT devirtualizes
       ├── ForkSink<T>       (sealed)  ← primary + Next propagation (propagate-after-accept)
       ├── FilterSink<T>     (sealed)  ← conditional gate + downstream sink
       ├── NullSink<T>       (sealed)  ← singleton no-op sink
       └── SerializeSink<T>  (sealed)  ← bridge: typed T → PacketSink (MemoryMarshal.AsBytes)

  RamSink<T>  (sealed)  ← direct subclass of DispatchSink<T>
       │  _buffer: T* (NativeMemory.AllocZeroed)
       │  _head / _tail: long  ← single-threaded, not SPSC ring
       │  IsHealthy => _tail - _head < _capacity
       │  Accept   => native pointer write
       │  DrainTo(DispatchSink<T>) ← called externally on recovery

  ─────────────────────────────────────────────────────────────────────────────
  PacketSink  (abstract, byte payloads)
       │
       │  Next: PacketSink?
       │  IsHealthy: bool
       │  Enqueue(ReadOnlySpan<byte>)  ← hot path: IsHealthy && Accept || Next?.Enqueue
       │  Accept(ReadOnlySpan<byte>): bool
       │  Flush() / Dispose()
       │
       ├── SpscQueueSink  (abstract, non-generic)
       │       │  _ring: SpscByteRingBuffer      ← lock-free length-prefixed SPSC ring
       │       │  _healthy: volatile bool
       │       │  Prev: PacketSink?              ← drain-to-prev on recovery
       │       │
       │       ├── FileSink          (sealed)  ← byte append to FileStream, POH buffer
       │       ├── RotatingFileSink  (sealed)  ← size + date rotation, max-file cleanup
       │       ├── NamedPipeSink     (sealed)  ← length-prefixed named-pipe client
       │       ├── UdpSink           (sealed)  ← UDP datagrams
       │       ├── TcpSink           (sealed)  ← length-framed TCP, POH send buffer
       │       └── BatchSink  (abstract)
       │               │  _scratch: byte[] POH  ← accumulates payloads per batch
       │               │  OversizedDropCount    ← observable counter
       │               │  OnFlush(ReadOnlySpan<byte>)  ← abstract; receives full batch
       │               │
       │               └── HttpBatchSink  (abstract)
       │                       │  circuit breaker (cbFailures + cbOpenDurationMs)
       │                       │  HttpFailureCount / BreakerOpenCount / DroppedBatchCount
       │                       │
       │                       └── SeqSink  (sealed)  ← CLEF/HTTP → Seq /api/events/raw
       │
       ├── MpscQueueSink  (abstract, non-generic)
       │       │  _ring: MpscByteRingBuffer
       │       │  (same lifecycle API as SpscQueueSink)
       │
       ├── ForkSink    (sealed, non-generic)  ← primary + Next propagation
       ├── MultiSink   (sealed, non-generic)  ← broadcast to PacketSink[] children
       ├── Multi2PacketSink<TC1, TC2>  (sealed)  ← CRTP 2-child broadcast, JIT devirtualizes
       ├── FilterSink  (sealed, non-generic)  ← conditional gate
       └── NullSink    (singleton)            ← NullSink.Instance

  RamSink  (sealed, non-generic)  ← direct subclass of PacketSink
       │  Native memory fill-once buffer, linear layout with 4-byte BE headers
       │  DrainTo(PacketSink) ← called externally on recovery

  SharedMemorySink  (sealed)  ← synchronous PacketSink, Log2 MMF wire protocol
       │  Named MemoryMappedFile ring (128-byte header + data area)
       │  No consumer thread — writes synchronously on producer thread


================================================================================
                         ASSEMBLY GRAPH
================================================================================

  Relay.csproj  (src/Relay)
  ├── namespace Relay          DispatchSink<T>, SpscQueueSink<T>, MpscQueueSink<T>,
  │                            MultiSink<T>, Multi2Sink<T,TC1,TC2>, ForkSink<T>,
  │                            FilterSink<T>, NullSink<T>, SerializeSink<T>,
  │                            PacketSink, SpscQueueSink, MpscQueueSink,
  │                            ForkSink, MultiSink, Multi2PacketSink<TC1,TC2>,
  │                            FilterSink, NullSink, BatchSink
  ├── namespace Relay.Buffers  SpscRingBuffer<T>, MpscRingBuffer<T>,
  │                            SpscByteRingBuffer, MpscByteRingBuffer    [internal]
  ├── namespace Relay.Sinks    FileStreamSink<T>, MmfSink<T>, TcpSink<T>, RamSink<T>
  │                            FileSink, RotatingFileSink, NamedPipeSink,
  │                            UdpSink, TcpSink, RamSink, SharedMemorySink
  ├── namespace Relay.Builder  RelayBuilder, SinkChain<T,THead>, MultiBuilder<T>,
  │                            FilterBinding<T,THead>                    [typed chain]
  │                            SinkChainBuilder, SinkChain<THead>,
  │                            FilterBinding<THead>                      [packet chain]
  ├── namespace Relay.Memory   RelayMemory                               [internal]
  └── namespace Relay.Internal HfClock, SinkConstraints                 [internal]

  Relay.Sinks.Http.csproj  (src/Relay.Sinks.Http)
  └── namespace Relay.Sinks.Http     HttpBatchSink
        depends on → Relay.csproj

  Relay.Sinks.Observability.csproj  (src/Relay.Sinks.Observability)
  └── namespace Relay.Sinks.Observability.Seq     SeqSink
        depends on → Relay.csproj, Relay.Sinks.Http.csproj

  Relay.Tests.csproj
  └── InternalsVisibleTo → Relay, Relay.Sinks.Http, Relay.Sinks.Observability

  Relay.Benchmarks.csproj
  └── InternalsVisibleTo → Relay


================================================================================
                         ENQUEUE HOT PATH
================================================================================

  caller.Enqueue(in item)          [producer thread]
       │
       ▼
  DispatchSink<T>.Enqueue(in T item):
  ┌─────────────────────────────────────────────────────────────────┐
  │  bool accepted = IsHealthy && Accept(in item);                  │
  │  if (accepted && !PropagateAfterAccept) return;  ← fast exit    │
  │  Next?.Enqueue(in item);                         ← fallback/fork│
  └─────────────────────────────────────────────────────────────────┘
       │
       ├─ IsHealthy = true, Accept = true, PropagateAfterAccept = false
       │      → item delivered locally, return
       │
       ├─ IsHealthy = true, Accept = true, PropagateAfterAccept = true (ForkSink)
       │      → item delivered locally, Next?.Enqueue continues
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

  PropagateAfterAccept:
    readonly bool field set in DispatchSink<T>(bool propagateAfterAccept = false).
    ForkSink<T> passes true; all other concrete sinks pass false (default).
    Sealed subclasses with a compile-time constant collapse the branch at JIT time.
    Propagation fires ONLY after successful Accept. Failure path is unchanged.


================================================================================
                         CHAIN TOPOLOGIES
================================================================================

  T1 — Single SpscQueueSink (depth 1)
  ─────────────────────────────────────
  caller ──▶ [FileStreamSink<T>]

  T2 — Serial depth 2
  ────────────────────
  caller ──▶ [FileStreamSink<T>] ──(Next)──▶ [RamSink<T>]
             unhealthy? ───────────────────────────────▶

  T3 — Serial depth 3
  ────────────────────
  caller ──▶ [FileStreamSink<T>] ──▶ [TcpSink<T>] ──▶ [RamSink<T>]

  T4 — Multi broadcast to 2 children
  ────────────────────────────────────
  caller ──▶ [MultiSink<T>] ──▶ c1 [FileStreamSink<T>]
                             └──▶ c2 [TcpSink<T>]
             all-fail? ──▶ (Next / drop)

  T5 — Multi + serial fallback
  ─────────────────────────────
  caller ──▶ [MultiSink<T>] ──▶ c1 [FileStreamSink<T>]
             │               └──▶ c2 [TcpSink<T>]
             └──(Next)──▶ [RamSink<T>]       ← only when all children unhealthy

  T6 — Filter gate
  ─────────────────
  caller ──▶ [FilterSink<T>] ──(predicate true)──▶ [FileStreamSink<T>]
                              ──(predicate false)─▶ (consumed, no fallback)

  T7 — Multi2Sink CRTP variant (sealed TC1, TC2 → JIT devirtualizes)
  ───────────────────────────────────────────────────────────────────
  caller ──▶ [Multi2Sink<Entry, FileStreamSink<Entry>, TcpSink<Entry>>]
                          ├── _c1.Enqueue(item)   [inlined by JIT]
                          └── _c2.Enqueue(item)   [inlined by JIT]

  T8 — Fork (propagate-after-accept)
  ────────────────────────────────────
  caller ──▶ [ForkSink<T>(primary=Audit)] ──▶ Audit.Enqueue   [local]
                                          └──(Next)──▶ [MainSink<T>]

  T9 — Typed → Packet bridge
  ───────────────────────────
  caller ──▶ [SerializeSink<T>] ──▶ [SpscQueueSink (FileSink / SeqSink)]
             (MemoryMarshal.AsBytes, zero alloc)

  T10 — Packet HTTP batch → Seq
  ──────────────────────────────
  caller ──▶ [SeqSink] ──(circuit-breaker open)──▶ [RamSink / NullSink]


================================================================================
                         MULTI ROUTING RULES
================================================================================

  MultiSink<T>.IsHealthy:
    short-circuit OR over children:
      for each child: if child.IsHealthy → return true
      (all fail) → return false → triggers Next?.Enqueue

  MultiSink<T>.Accept(item):
    for each child: child.Enqueue(item)   ← always, regardless of health
    return true                            ← never triggers fallback via Accept

  Key: fallback to Next only when IsHealthy = false (all children unhealthy).
  Partial failure (some healthy, some not) → healthy children receive; unhealthy drop.

  FilterSink<T>.Accept(item):
    if predicate(item): _downstream.Enqueue(item)
    return true         ← always; filtered items do NOT propagate to Next


================================================================================
                         SpscQueueSink<T> — CONSUMER THREAD
================================================================================

  [Producer thread]              [Consumer thread: "relay-{sinkName}"]
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

  (SpscQueueSink non-generic follows the same loop; WriteToBackend receives ReadOnlySpan<byte>)


================================================================================
                         BatchSink — CONSUMER THREAD (PACKET)
================================================================================

  BatchSink extends SpscQueueSink (non-generic). Adds a POH scratch buffer.

  [Consumer thread: ConsumeLoop → WriteToBackend]:
  ┌─────────────────────────────────────────────────────────────────┐
  │  WriteToBackend(payload):                                       │
  │    if payload.Length > BatchCapacity → OversizedDropCount++     │
  │    if _offset + payload.Length > BatchCapacity → FlushScratch() │
  │    payload.CopyTo(_scratch[_offset..]) ; _offset += length      │
  │                                                                 │
  │  FlushBackend() → FlushScratch()                                │
  │    if _offset == 0 → return                                     │
  │    OnFlush(_scratch[0.._offset]) ; _offset = 0                  │
  └─────────────────────────────────────────────────────────────────┘

  HttpBatchSink.OnFlush(batch):
    if HfClock.NowTicks < _breakerOpenUntilTicks → DroppedBatchCount++, return
    POST batch to _endpoint via HttpClient.SendAsync().GetResult()
    success → reset _failures
    failure → IncrementFailureAndMaybeOpenBreaker()


================================================================================
                         RECOVERY — Prev DRAIN
================================================================================

  Setup (by SinkChain.To):
    .Start<T, P1>(p1).To(p2).Build()
         │
         └─ p2 is SpscQueueSink<T> / MpscQueueSink<T>? → p2.Prev = p1

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
    p1 = FileStreamSink<T> (primary, was broken)
    p2 = RamSink<T> (fallback, accumulated items during p1 outage)
    On p1 recovery → p2 drains back to p1 → normal path resumes

  Only SpscQueueSink<T> / MpscQueueSink<T> nodes get Prev. Other DispatchSink<T>
  subclasses without a consumer thread (Multi, Fork, Filter, Null, Ram) do not participate.

  Same mechanism applies to packet-hierarchy SpscQueueSink / MpscQueueSink nodes.


================================================================================
                         BUILDER ASSEMBLY
================================================================================

  ── Typed chain (DispatchSink<T>) ──────────────────────────────────────────

  RelayBuilder.Start<T, THead>(head)          ← generic entry
  RelayBuilder.StartSpsc<T, THead>(head)      ← THead : SpscQueueSink<T>
  RelayBuilder.StartMpsc<T, THead>(head)      ← THead : MpscQueueSink<T>
       │
       └─▶ SinkChain<T, THead> { _head = head, _tail = head }

  Operators on SinkChain<T, THead>:

  .To(next):
       if next is SpscQueueSink<T> / MpscQueueSink<T>:
           next.Prev = _tail          ← recovery drain link
       _tail.Next = next              ← fallback routing link
       _tail = next

  .Fork(primary):
       fork = new ForkSink<T>(primary)
       _tail.Next = fork ; _tail = fork

  .When(pred).To(downstream):
       filter = new FilterSink<T>(pred, downstream)
       _tail.Next = filter            ← filter.Next never fires (Accept=true always)
       _tail = downstream             ← subsequent .To extends downstream's chain

  .Multi(cfg)      (cfg builds a MultiBuilder<T> → MultiSink<T>)
  .Multi<TC1,TC2>(c1, c2)    ← sealed CRTP variant (Multi2Sink<T,TC1,TC2>)

  chain.Build():
       return _head                   ← typed head, ready to Enqueue

  ── Packet chain (PacketSink) ───────────────────────────────────────────────

  SinkChainBuilder.Start<THead>(head)         ← any PacketSink head
  SinkChainBuilder.StartSpsc<THead>(head)     ← THead : SpscQueueSink
  SinkChainBuilder.StartMpsc<THead>(head)     ← THead : MpscQueueSink
       │
       └─▶ SinkChain<THead> { Head = head, _tail = head }

  Operators on SinkChain<THead>:

  .To(sink):
       if sink is SpscQueueSink: sink.Prev = _tail
       _tail.Next = sink ; _tail = sink

  .Fork(primary):
       fork = new ForkSink(primary) ; _tail.Next = fork ; _tail = fork

  .When(pred).To(downstream):
       filter = new FilterSink(pred, downstream)
       _tail.Next = filter ; _tail = downstream

  .Multi(params PacketSink[] children):
       multi = new MultiSink(children) ; _tail.Next = multi ; _tail = multi

  .Multi<TC1,TC2>(c1, c2):
       multi = new Multi2PacketSink<TC1,TC2>(c1, c2) ; _tail.Next = multi ; _tail = multi

  Example typed assembly:
  ┌──────────────────────────────────────────────────────────────────┐
  │  var head = RelayBuilder                                         │
  │      .Start<Entry, FileStreamSink<Entry>>(file)                  │
  │      .To(tcp)                                                    │
  │      .To(ram)                                                    │
  │      .Build();                                                   │
  │                                                                  │
  │  Wired state:                                                    │
  │    file.Next = tcp                                               │
  │    tcp.Next  = ram    (tcp is SpscQueueSink → tcp.Prev = file)   │
  │    ram.Next  = null   (ram is DispatchSink → no Prev)            │
  │                                                                  │
  │  ram.Prev is NOT set — RamSink<T> is not SpscQueueSink<T>.       │
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

  Batched-write API (for Multi2Sink with shared mfence):
    TryReserveTail(out tail) → reserve slot without write (uses _cachedHead)
    WriteSlot(tail, in item) → Unsafe.Add write, no fence
    Thread.MemoryBarrier()   → single mfence for N writes
    CommitTail(tail)         → Volatile.Write advances tail


================================================================================
                         MPSC RING BUFFER PROTOCOL
================================================================================

  Three PaddedLong counters (128-byte isolated cache lines):
    _claimedTail  ← CAS by producers
    _headCache    ← producer local copy of _head (avoids cross-core reads)
    _head         ← consumer advance

  Typed (MpscRingBuffer<T>):
    Slot { int Published; T Value } on POH-pinned Slot[].
    Producer: CAS(_claimedTail, +1) → write slot.Value → Volatile.Write(Published, 1)
    Consumer: Volatile.Read(Published) → copy out T → zero Value →
              Volatile.Write(Published, 0) → advance _head

  Byte (MpscByteRingBuffer):
    Variable-length records on POH-pinned byte[]. Header = 4 bytes LE + high-bit publish flag.
    Producer: CAS(_claimedTail, +recordSize) → write payload →
              Volatile.Write(header, len | 0x80000000)
    Wrap case: stamp 0xFFFFFFFF padding at original tail; restart at offset 0.
    Consumer: Volatile.Read(header) → check high bit → skip padding → zero-copy Span return.
              Advance(): clears header (volatile zero) before advancing _head.


================================================================================
                         CONCRETE SINKS — BACKEND DETAIL
================================================================================

  ── Typed (DispatchSink<T>) ─────────────────────────────────────────────────

  FileStreamSink<T>
  ─────────────────
  Ring → [consumer thread] → POH write buffer (4096 × sizeof(T)) → FileStream
                               on full OR flush interval: FileStream.Write(span)
  Failure:  IOException in FlushBuffer → _healthy = false
  Recovery: TryRecoverBackend — reopen FileStream, re-flush buffer
            backoff: 1s → 2s → 4s … → 60s (exponential, capped)
  Default:  ringCapacity=524288, flushInterval=250ms

  TcpSink<T>
  ──────────
  Ring → [consumer thread] → POH send buffer (4096 × sizeof(T)) → NetworkStream
                               on full OR flush interval: NetworkStream.Write(span)
  Failure:  Exception in FlushBuffer → _healthy = false
  Recovery: TryRecoverBackend — dispose+reconnect TcpClient, re-flush buffer
            backoff: 1s → 2s … → 30s (exponential, capped)
  Default:  ringCapacity=16384, flushInterval=250ms

  MmfSink<T>
  ──────────
  Ring → [consumer thread] → MemoryMappedViewAccessor.Write<T>(_position, ref item)
                               on flush interval: _view.Flush()
  Failure:  capacity exhaustion (_position + sizeof(T) > maxBytes) → IsHealthy = false
  Recovery: none — capacity is permanent; TryRecoverBackend is a no-op
  Note:     MemoryMappedViewAccessor.Write never throws IOException

  RamSink<T>  (direct DispatchSink<T> subclass — no consumer thread)
  ────────────────────────────────────────────────────────────────────
  Enqueue → Accept → native pointer write (_buffer[_tail & _mask] = item; _tail++)
  No threading: single-threaded, synchronous write (hot path thread)
  Failure:  ring full (_tail - _head >= _capacity) → IsHealthy = false
  Recovery: DrainTo(target) — called externally by the recovery coordinator
            Drains in FIFO order; stops if target becomes unhealthy
  Dispose:  NativeMemory.Free(_buffer)
  Default:  capacity = 1 << 23  (~8M entries, ~512MB for T=64B)

  ── Packet (PacketSink) ─────────────────────────────────────────────────────

  FileSink
  ────────
  Ring → [consumer thread] → POH write buffer → FileStream.Append
  Optional header written once when stream position == 0 (file is new).
  Failure:  IOException → _healthy = false; backoff 1s → 60s
  Default:  writeBufferCapacity=65536, ringCapacity=65536, flushInterval=200ms

  RotatingFileSink
  ────────────────
  Like FileSink but rotates by max-byte size and/or calendar date.
  Cleanup: deletes oldest files above maxFiles count.
  Optional header written once per new file.

  NamedPipeSink
  ─────────────
  Ring → [consumer thread] → NamedPipeClientStream → 4-byte BE length prefix + payload
  Compatible with Input2Log NamedPipe receiver.
  Failure:  IOException → _healthy = false; backoff 1s → 30s

  UdpSink
  ───────
  Ring → [consumer thread] → Socket.SendTo (UDP datagram)
  No framing — each enqueued payload is one datagram.
  Failure:  SocketException → _healthy = false; backoff 1s → 10s

  TcpSink  (packet)
  ─────────────────
  Ring → [consumer thread] → TcpClient → NetworkStream → length-framed payload
  Same reconnect semantics as typed TcpSink<T>.

  BatchSink → HttpBatchSink → SeqSink
  ────────────────────────────────────
  Ring → [consumer thread] → _scratch accumulator → POST to HTTP endpoint
  SeqSink target: /api/events/raw (CLEF JSON lines)
  Content-Type: application/vnd.serilog.clef
  Auth: X-Seq-ApiKey header (optional)
  Circuit breaker: open after cbFailures consecutive failures, probe after cbOpenDurationMs
  Default: ringCapacity=65536, batchCapacity=65536, flushInterval=1000ms,
           cbFailures=3, cbOpenDurationMs=30000ms

  RamSink  (packet, direct PacketSink subclass)
  ──────────────────────────────────────────────
  Fill-once native memory ring. Linear layout with 4-byte host-order headers.
  DrainTo(PacketSink) for recovery. No consumer thread.

  SharedMemorySink
  ────────────────
  Synchronous (no consumer thread). Named MemoryMappedFile ring.
  Wire protocol: 128-byte MMF header (magic + capacity + write/read index)
                 + data area with 4-byte BE length-prefixed records.
  Compatible with Log2 SharedMemorySink consumer.


================================================================================
                         THREADING MODEL
================================================================================

  Thread                 Priority     Owner                     Role
  ──────────────────────────────────────────────────────────────────────────────
  Producer (caller)      (any)        Application               Calls Enqueue
  relay-file             BelowNormal  FileStreamSink<T>         Drain ring → FileStream
  relay-tcp              BelowNormal  TcpSink<T>                Drain ring → NetworkStream
  relay-mmf              BelowNormal  MmfSink<T>                Drain ring → MemoryMappedFile
  relay-file-<name>      BelowNormal  FileSink                  Drain ring → FileStream (bytes)
  relay-file-<name>      BelowNormal  RotatingFileSink          Drain ring → rotating file
  relay-pipe-<name>      BelowNormal  NamedPipeSink             Drain ring → NamedPipe
  relay-udp              BelowNormal  UdpSink                   Drain ring → UDP socket
  relay-tcp (packet)     BelowNormal  TcpSink (packet)          Drain ring → TCP stream
  relay-seq              BelowNormal  SeqSink                   Drain ring → HTTP POST
  relay-{name}           BelowNormal  SpscQueueSink subclass    Custom backend

  RamSink, RamSink (packet), SharedMemorySink have NO dedicated thread.
  Writes happen synchronously on the producer thread.
  MultiSink, ForkSink, FilterSink, NullSink, SerializeSink have NO threads.

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
  │ CAS Interlocked        │ MpscRingBuffer: _claimedTail reservation         │
  │ Interlocked.Increment  │ BatchSink._oversizedDropCount (consumer thread)  │
  │                        │ HttpBatchSink counters (consumer thread)         │
  │ Thread.MemoryBarrier() │ Multi2Sink batched-write (shared mfence for N)   │
  └────────────────────────┴──────────────────────────────────────────────────┘

  No lock / Monitor anywhere in the hot path.
  No async/await on any dispatch path.


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

  Write buffer (FileStreamSink<T> / TcpSink<T>):
    GC.AllocateArray<byte>(4096 * sizeof(T), pinned: true)
    Lives on POH (Pinned Object Heap) — never moved by GC
    Capacity: 4096 entries × sizeof(T) bytes

  RamSink<T> native ring:
    NativeMemory.AllocZeroed(capacity × sizeof(T))
    Unmanaged heap — not subject to GC pressure
    Freed explicitly in Dispose via NativeMemory.Free

  BatchSink scratch buffer:
    GC.AllocateUninitializedArray<byte>(batchCapacity, pinned: true)
    POH — never moved. Reused across flushes. Offset reset to 0 after each OnFlush.


================================================================================
                         MEMORY INITIALIZATION (RelayMemory)
================================================================================

  SpscQueueSink<T>.Start() calls RelayMemory.PreFaultAndLock(ring.Buffer):

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
  │ SpscQueueSink<T>.IsHealthy (healthy, L1 hot)        │    ~7c   │   ~2 ns  │
  │ SpscQueueSink<T>.IsHealthy (unhealthy, short-circ.) │    ~1c   │  <1 ns   │
  │ Volatile.Write (mfence, x64)                        │   ~15c   │   ~4 ns  │
  │ Virtual call (predicted branch)                     │    ~3c   │  <1 ns   │
  │ T1: Enqueue depth-1 (success, SpscQueueSink)        │   ~32c   │   ~9 ns  │
  │ T2: Enqueue depth-2 (success, p1 healthy)           │   ~32c   │   ~9 ns  │
  │ T2: Enqueue depth-2 (fallback, p1 unhealthy→p2)     │   ~12c   │   ~3 ns  │
  │ T3: Enqueue depth-3 (p1+p2 unhealthy→p3 RamSink)    │   ~16c   │   ~5 ns  │
  │ T4: Multi 2 children (both healthy, array-based)    │   ~74c   │  ~21 ns  │
  │ T4: Multi 2 children (CRTP Multi2Sink, sealed)      │   ~68c   │  ~19 ns  │
  │ T5: Multi all-fail → Next (RamSink)                 │   ~19c   │   ~5 ns  │
  │ Drop (Next == null, depth 1)                        │    ~2c   │  <1 ns   │
  │ Drop (Next == null, depth 3)                        │   ~14c   │   ~4 ns  │
  └─────────────────────────────────────────────────────┴──────────┴──────────┘

  Key insight: fallback is often CHEAPER than success because:
    - Unhealthy short-circuit = ~1c (vs ~7c IsHealthy normal)
    - RamSink has no mfence (vs ~15c in SpscQueueSink.TryPublish)
    - Hot path degradation is lighter than normal hot path

  Multi scales linearly: Total ≈ N × 35c + 6c for N healthy SpscQueueSink children.

  Default ring capacities vs memory footprint (T = 64B):
  ┌─────────────────────┬────────────────┬──────────────────┐
  │ Sink                │ Ring capacity  │ Ring memory      │
  ├─────────────────────┼────────────────┼──────────────────┤
  │ FileStreamSink<T>   │ 524,288        │  ~32 MB          │
  │ TcpSink<T>          │  16,384        │   ~1 MB          │
  │ MmfSink<T>          │  65,536        │   ~4 MB          │
  │ RamSink<T>          │ 8,388,608      │ ~512 MB (native) │
  ├─────────────────────┼────────────────┼──────────────────┤
  │ FileSink            │  65,536 bytes  │  ~64 KB          │
  │ SeqSink             │  65,536 bytes  │  ~64 KB          │
  └─────────────────────┴────────────────┴──────────────────┘


================================================================================
                         RECOMMENDED TOPOLOGIES
================================================================================

  Recording (primary local file, typed):
    FileStreamSink<T> → RamSink<T>
    ~32c success │ ~12c fallback │ ~2c drop

  Recording with remote redundancy:
    FileStreamSink<T> → TcpSink<T> → RamSink<T>
    ~32c success │ ~36c fallback-to-tcp │ ~16c fallback-to-ram

  IPC dispatch to multiple destinations:
    Multi2Sink<T, FileStreamSink<T>, TcpSink<T>>  →  RamSink<T>
    ~68c success │ ~19c all-fail-to-ram

  Symbol-filtered recording:
    FilterSink<T> → FileStreamSink<T>
    ~45c pass │ ~8c blocked (silently consumed)

  Typed pipeline → Seq observability sink:
    SerializeSink<T> → SeqSink → RamSink (packet)
    Typed structs serialized zero-copy; CLEF encoding is producer responsibility.

  Rules of thumb:
    - Each additional serial hop costs 0c on the success path, +4c per unhealthy hop.
    - Multi broadcast multiplies by N. Keep N ≤ 4.
    - RamSink as last resort adds ~0c on success path, ~6c on all-children-fail path.
    - Multi2Sink (CRTP) saves ~6c over MultiSink when children are sealed types.
```
