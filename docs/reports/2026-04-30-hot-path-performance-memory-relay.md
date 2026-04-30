# Relay hot-path performance and memory report - 2026-04-30

_Base: `docs/reports/2026-04-29-resource-cost-map-relay.md`, `docs/topology.md`, current `src/Relay/**/*.cs`, and BDN artifacts from phases 1-7. Cycle conversion used by the prior report: ~4.5 GHz, so 1 ns ~= 4.5 cycles._

## 1. Executive verdict

The implemented changes hold up. The steady-state producer and consumer hot paths remain allocation-free: no heap allocation was found in `Enqueue`, `TryEnqueue`, `Accept`, `TryPublish`, `TryPeek`, `TryConsume`, or the normal `WriteToBackend` buffer-copy paths. BDN microbenchmarks also report `0 B` / `-` allocated for the hot primitives.

The biggest producer-side costs are now intentional synchronization or I/O boundaries:

| Area | Cost shape | Allocation | Verdict |
|---|---:|---:|---|
| Typed SPSC publish | ~8-12c publish primitive; SPSC round-trip measured ~3.79 ns ~= 17c | 0 B/call | healthy |
| Typed MPSC publish | round-trip measured ~7.97 ns ~= 36c uncontended; contention dominates with multiple producers | 0 B/call | healthy, but topology choice matters |
| Packet SPSC/MPSC byte rings | 64B SPSC round-trip ~3.5 ns ~= 16c; MPSC round-trip ~7.8-8.6 ns ~= 35-39c | 0 B/call | healthy |
| Packet terminal/dispatch | `Enqueue` healthy ~2.68 ns ~= 12c; `TryEnqueue` ~0.42 ns ~= 2c | 0 B/call | healthy |
| File/TCP/pipe buffered writes | small copy per payload, syscall only on flush/buffer full | 0 B/call | healthy |
| UDP write | syscall per payload, ~2000c static model | 0 B/call | conditional optimization only |
| Shared memory write | measured 11.8-14.0 ns ~= 53-63c | 0 B/call | one plausible local optimization |
| Packet consumer idle loop | extra `HfClock.NowTicks` in packet loops during spin | 0 B/call | one low-risk cleanup |

## 2. Hot tree cost review

### 2.1 Producer path - typed

| Hot-tree node | Current cost | Allocation | Notes |
|---|---:|---:|---|
| `DispatchSink<T>.Enqueue` (`src/Relay/DispatchSink.cs:45`) | trivial sealed benchmark: 0.478 ns ~= 2c; static upper bound still ~15c when real `Accept` work is present | 0 B | The dispatch envelope is no longer the bottleneck. |
| `SpscQueueSink<T>.Accept -> SpscRingBuffer<T>.TryPublish` (`src/Relay/SpscQueueSink.cs:104`, `src/Relay/Buffers/SpscRingBuffer.cs:84`) | SPSC publish+consume round-trip: ~3.79 ns ~= 17c at cap 1024; publish half is in the expected ~8-12c range | 0 B | Native 64B-aligned ring removes prior POH slot-straddle risk. |
| `MpscQueueSink<T>.Accept -> MpscRingBuffer<T>.TryPublish` (`src/Relay/MpscQueueSink.cs:105`, `src/Relay/Buffers/MpscRingBuffer.cs:100`) | uncontended round-trip: ~7.97 ns ~= 36c; phase 7 aggregate throughput: N=1 10.6M/s, N=2 7.7M/s, N=4 12.3M/s, N=8 13.3M/s | 0 B | MPSC is correct for multi-producer, but not a free scaling primitive. Use SPSC where topology allows. |
| `MultiSink<T>.Accept` vs `Multi2Sink<T,...>.Accept` (`src/Relay/MultiSink.cs:37`, `src/Relay/MultiSink.cs:70`) | array N=2: 6.89 ns ~= 31c; typed `Multi2`: 5.93 ns ~= 27c | 0 B | Typed `Multi2` still has a small measured cycle win. |
| `FilterSink<T>.Accept` (`src/Relay/FilterSink.cs:30`) | pass: 6.22 ns ~= 28c; reject: 0.82 ns ~= 4c | 0 B | Delegate cost is the expected extension-point cost. |
| `SerializeSink<T>.Accept` (`src/Relay/SerializeSink.cs:30`) | ~5c + packet downstream, static | 0 B | Uses `MemoryMarshal.AsBytes`; no copy and no allocation. |
| `RamSink<T>.Accept` (`src/Relay/Sinks/RamSink.cs:39`) | static ~7c | 0 B/call | Fast fallback; large native cold allocation is the tradeoff. |

### 2.2 Producer path - packet

| Hot-tree node | Current cost | Allocation | Notes |
|---|---:|---:|---|
| `PacketSink.Enqueue` (`src/Relay/PacketSink.cs:62`) | healthy: 2.68 ns ~= 12c; terminal drop: 3.75-3.79 ns ~= 17c | 0 B | Drop count is cheap enough and only on terminal failure. |
| `PacketSink.TryEnqueue` (`src/Relay/PacketSink.cs:54`) | healthy: 0.42 ns ~= 2c; reject: 0.22 ns ~= 1c | 0 B | Use at call sites that explicitly do not want fallback/drop semantics. This is already implemented. |
| `SpscQueueSink.Accept -> SpscByteRingBuffer.TryPublish` (`src/Relay/SpscQueueSink.Packet.cs:87`, `src/Relay/Buffers/SpscByteRingBuffer.cs:84`) | 64B SPSC byte-ring round-trip: ~3.5 ns ~= 16c | 0 B | Healthy. |
| `MpscQueueSink.Accept -> MpscByteRingBuffer.TryPublish` (`src/Relay/MpscQueueSink.Packet.cs:101`, `src/Relay/Buffers/MpscByteRingBuffer.cs:112`) | 64B MPSC byte-ring round-trip: ~7.8-8.6 ns ~= 35-39c | 0 B | Healthy; contention remains topology-sensitive. |
| `SharedMemorySink.Accept` (`src/Relay/Sinks/SharedMemorySink.cs:88`) | 64B: 11.79 ns ~= 53c; 256B: 13.96 ns ~= 63c | 0 B | CAS + modular `WriteRing`; common-case fast path could be worth testing. |
| `RamSink.Accept` packet (`src/Relay/Sinks/RamSink.Packet.cs:58`) | 64B/256B: ~4.41 ns ~= 20c | 0 B | Fastest packet-local sink. |
| Packet `MultiSink` vs `Multi2PacketSink` (`src/Relay/MultiSink.Packet.cs:28`, `src/Relay/MultiSink.Packet.cs:73`) | array N=2: 3.21 ns ~= 14c; packet `Multi2`: 3.93 ns ~= 18c | 0 B | Do not choose packet `Multi2` for cycle savings; its win is code size/reasoning, not measured speed. |
| `FilterSink` packet (`src/Relay/FilterSink.Packet.cs:42`) | pass: 3.24 ns ~= 15c; reject: 0.83 ns ~= 4c | 0 B | Healthy. |

### 2.3 Consumer paths

| Hot-tree node | Current cost | Allocation | Notes |
|---|---:|---:|---|
| Typed `SpscQueueSink<T>.ConsumeLoop` (`src/Relay/SpscQueueSink.cs:156`) | `TryConsumeBatch`: static ~5 + 2N cycles per batch; one head publish per batch | 0 B | Correctly amortized. |
| Typed `MpscQueueSink<T>.ConsumeLoop` (`src/Relay/MpscQueueSink.cs:137`) | per-slot published flag remains; head advances once per batch | 0 B | Correct for MPSC. |
| Packet `SpscQueueSink.ConsumeLoop` (`src/Relay/SpscQueueSink.Packet.cs:110`) | `TryPeek` + `Advance` per record; inner batch avoids idle re-entry | 0 B | Payload path is fine; idle clock check has one local cleanup. |
| Packet `MpscQueueSink.ConsumeLoop` (`src/Relay/MpscQueueSink.Packet.cs:127`) | same packet loop shape with MPSC byte ring | 0 B | Same idle cleanup applies. |
| `RotatingFileSink.ShouldRotate` (`src/Relay/Sinks/RotatingFileSink.cs:84`) | post-fix predicate BDN: 13.76 ns wall; actionable delta vs old path: -8.08 ns ~= -36c | 0 B | Prior `DateTime.UtcNow.Date` regression is resolved. Full `WriteToBackend` is 66.22 ns wall and still 0 B. |
| Buffered `FileSink` / `TcpSink` / `NamedPipeSink` / `UnixSocketSink` | ~10-18c copy/prefix per payload; syscall on flush | 0 B/call | Healthy. |
| `UdpSink.WriteToBackend` (`src/Relay/Sinks/UdpSink.cs:41`) | static ~2000c syscall per payload | 0 B/call | Only optimizable by changing UDP batching semantics or using platform-specific multi-send. |

## 3. Memory and allocation map

Steady-state hot path allocation is 0 B/call. The meaningful memory cost is cold/startup capacity:

| Component | Memory shape | Managed/Native | Hot-path allocation? |
|---|---:|---|---:|
| `SpscRingBuffer<T>` | `capacity * sizeof(T)` | native, 64B aligned | no |
| `MpscRingBuffer<T>` | `capacity * sizeof(Slot)` where slot is `int Published + T` | native, 64B aligned base | no |
| `SpscByteRingBuffer` / `MpscByteRingBuffer` | `capacity` bytes | pinned managed array / POH | no |
| Typed file/TCP write buffer | `4096 * sizeof(T)`; T=64B => 256 KiB | pinned managed array / POH | no |
| Packet file/TCP/pipe/unix buffers | usually ~64 KiB | pinned managed array / POH | no |
| `BatchSink` scratch | `batchCapacity` | pinned managed array / POH | no |
| `RamSink<T>` default | 8,388,608 slots; T=64B => ~512 MiB | native | no |
| `RamSink` packet default | 4 MiB | native | no |

No memory optimization is recommended for the normal hot path. Capacity tuning is deployment policy: lowering ring sizes saves memory but reduces burst tolerance and changes fallback/drop behavior.

## 4. Possible gains, only where justified

### G1 - Packet consumer idle clock check

**Classification:** recommended small cleanup. It affects idle CPU/power and flush polling overhead, not payload-copy throughput.

**Where:**

- `src/Relay/SpscQueueSink.Packet.cs:160`
- `src/Relay/MpscQueueSink.Packet.cs:180`

**Current shape:**

```csharp
bool flushDue = Volatile.Read(ref _flushRequested) == 1
             || HfClock.NowTicks >= flushDeadline;
if (checkDeadline && flushDue)
{
    Volatile.Write(ref _flushRequested, 0);
    FlushBackend();
    TryRecoverBackend();
    TryDrainToPrev();
    flushDeadline = HfClock.NowTicks + _flushIntervalTicks;
}
```

In the spin phase, `checkDeadline` is false for most iterations, but the packet loops still call `HfClock.NowTicks` unless `_flushRequested == 1`. That contradicts the topology intent: "spin phase: every 8 iters". The typed loops already use the cheaper shape.

**What to do:**

Align packet loops with the typed loops:

```csharp
bool flushNow    = Volatile.Read(ref _flushRequested) == 1;
bool deadlineHit = checkDeadline && HfClock.NowTicks >= flushDeadline;

if (flushNow || deadlineHit)
{
    if (flushNow) Volatile.Write(ref _flushRequested, 0);
    FlushBackend();
    if (deadlineHit)
    {
        TryRecoverBackend();
        TryDrainToPrev();
    }
    flushDeadline = HfClock.NowTicks + _flushIntervalTicks;
}
```

If exact current packet semantics must be preserved for `TryRecoverBackend` on manual flush, keep those calls unconditional inside the block; the important performance part is `deadlineHit = checkDeadline && ...`.

**Expected gain:**

Up to 8 avoided `Stopwatch.GetTimestamp` calls in the first 10 empty spin iterations when there is no explicit flush request. Using the cost map's ~20c per `HfClock.NowTicks`, that is up to ~160c saved per empty spin cycle. Memory gain: none.

### G2 - SharedMemorySink contiguous-write fast path

**Classification:** conditional; worth implementing only if `SharedMemorySink` is a real high-rate path.

**Where:**

- `src/Relay/Sinks/SharedMemorySink.cs:88` (`Accept`)
- `src/Relay/Sinks/SharedMemorySink.cs:119` (`WriteRing`)

**Current shape:**

After reserving with CAS, every payload uses a `stackalloc byte[4]` length buffer plus two modular `WriteRing` calls, even when the frame is fully contiguous in the MMF data area.

**What to do:**

Add a common-case branch after the CAS reservation:

```csharp
byte* data = _ptr + HEADER_SIZE;
if (oldIdx + frameLen <= _dataCapacity)
{
    BinaryPrimitives.WriteInt32BigEndian(new Span<byte>(data + oldIdx, 4), payload.Length);
    payload.CopyTo(new Span<byte>(data + oldIdx + 4, payload.Length));
    return true;
}

// existing wrapped path
Span<byte> lenBuf = stackalloc byte[4];
BinaryPrimitives.WriteInt32BigEndian(lenBuf, payload.Length);
WriteRing(data, _dataCapacity, oldIdx, lenBuf);
WriteRing(data, _dataCapacity, (oldIdx + 4) % _dataCapacity, payload);
return true;
```

**Expected gain:**

Measured current cost is ~11.79 ns for 64B and ~13.96 ns for 256B, 0 B allocated. The unavoidable part is the CAS plus payload copy; the avoidable part is stackalloc + two modular `WriteRing` loops in the non-wrap case. A realistic target is a small single-digit ns gain (~5-15c) on contiguous writes, gated by BDN. Memory gain: none.

### G3 - UDP batching is a protocol-level option, not a transparent fix

**Classification:** conditional; do not implement as a default replacement.

**Where:**

- `src/Relay/Sinks/UdpSink.cs:41`

**Current shape:**

`UdpSink.WriteToBackend` emits one UDP datagram per payload, so the consumer pays a syscall per delivered record. The static model estimates ~2000c per record. The existing BDN push benchmark measures producer-side enqueue/saturation, not delivered datagrams, so it cannot prove wire throughput.

**What to do first:**

Add a delivered-count benchmark or receiver-side counter before changing code. If the wire protocol can accept multiple logical payloads in one UDP datagram, add a separate opt-in sink that batches length-delimited payloads up to MTU / `maxPayload`. Do not silently change `UdpSink` datagram semantics.

**Expected gain if protocol allows batching:**

Syscall cost can be amortized from ~2000c per payload to roughly ~2000c per batch plus framing/copy cost. Memory impact would be one reusable send buffer. Without protocol permission, expected safe gain is zero.

## 5. Non-gains rejected by this pass

| Candidate | Decision |
|---|---|
| `RotatingFileSink.Cleanup` LINQ allocation | Cold rotation path only; not hot-path memory. |
| Padding `MpscRingBuffer<T>.Slot` to a full cache line | Could double ring memory and the phase 7 data points at CAS contention first. Not justified without a slot-layout experiment. |
| Moving packet byte rings from pinned arrays to native memory | No per-call allocation today; added unsafe complexity has no proven cycle win. |
| Replacing packet `MultiSink` with `Multi2PacketSink` for speed | Phase 6 shows packet `Multi2` was slightly slower at N=2, though smaller in code size. |
| Lowering default ring capacities in code | This is workload policy, not a universal performance fix. Tune constructor values per deployment. |

## 6. Recommended next actions

1. Implement G1 if idle CPU matters for packet sinks; it is local and low risk.
2. Implement G2 only behind a BDN gate for `SharedMemorySinkBenchmarks.Accept_Single`.
3. Treat G3 as a protocol/project decision. Add delivered-count measurement before any UDP batching design.
4. Keep the zero-allocation invariant as a release gate for all hot-path benchmarks.
