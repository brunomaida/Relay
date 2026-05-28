# Relay

**Composable fallback dispatch pipeline for `unmanaged` structs — .NET 9 / C# 13**

Zero allocation on the hot path. No locks. No LINQ. No `async`. Each item is delivered to the first healthy backend in the chain; on failure, it falls through to the next pipe automatically.

- **Automatic fallback chain** — `DispatchSink<T>` delivers to the first healthy backend; on any failure it forwards to `Next` automatically, no producer involvement
- **Zero allocation steady state** — POH buffers allocated once at construction; `Enqueue` costs ~32 cycles, no heap allocation on the hot path
- **No locks, no `async`** — `Volatile` + `Interlocked` only; consumer threads run synchronous loops; `async`/`await` never enters dispatch or consume
- **Recovery drain** — when a failed sink recovers, items accumulated in the fallback drain back upstream automatically via the `Prev` pointer
- **Typed + packet hierarchies** — `DispatchSink<T>` for `T : unmanaged` fixed structs; `PacketSink` for variable-length `ReadOnlySpan<byte>` payloads; `SerializeSink<T>` bridges both zero-copy
- **Lock-free SPSC and MPSC rings** — 128-byte padded counters; `HeadCache` eliminates cross-core volatile reads under producer contention
- **Fluent builder** — `RelayBuilder` / `SinkChainBuilder` wire `Next` + `Prev`, broadcast branches (`MultiSink`), conditional gates (`FilterSink`), and fork/audit patterns (`ForkSink`)
- **Zero production dependencies** — `src/Relay` has no NuGet references

---

## Overview

Relay is an infrastructure library for building composable, resilient dispatch pipelines over blittable (`T : unmanaged`) data. The core abstraction is `DispatchSink<T>`: a node that tries to deliver an item to its local backend, and on any failure — including transient I/O errors, connection drops, or capacity limits — forwards the item to the next sink in the chain.

A parallel hierarchy, `PacketSink`, handles variable-length `ReadOnlySpan<byte>` payloads with the same fallback semantics. `SerializeSink<T>` bridges the two trees zero-copy via `MemoryMarshal.AsBytes`.

Pipelines are assembled with a fluent builder (`RelayBuilder` for typed chains, `SinkChainBuilder` for packet chains) and require no external orchestrator. Each sink manages its own health, recovery, and backpressure. The producer calls a single method (`Enqueue`) at whatever rate it needs; the library handles the rest.

**Intended use cases:**

- High-frequency event recording (market data, telemetry, sensor feeds)
- Resilient structured log sinks that survive I/O outages without losing records
- Tiered persistence: fast local storage → remote TCP receiver → RAM buffer → drop
- Fan-out broadcast to multiple independent consumers (file + network simultaneously)
- Conditional routing / filtering before delivery
- HTTP batch delivery to observability backends (Seq, custom CLEF endpoints)

---

## How It Works

```
Producer
   │
   ▼
[Sink 1 — IsHealthy? → Accept(item)]
   │ failure (IsHealthy=false OR Accept=false)
   ▼
[Sink 2 — IsHealthy? → Accept(item)]
   │ failure
   ▼
[Sink N — last resort, e.g. MemorySink]
   │ failure (ring full)
   ▼
 silent drop
```

**`Enqueue` is the only public entry point.** It is always synchronous from the producer's perspective. `SpscQueueSink<T>` subclasses buffer items in a lock-free SPSC ring and drain them on a dedicated consumer thread — the producer never blocks waiting for I/O.

**Recovery drain:** when a failed sink recovers, items accumulated in the downstream fallback sink are drained back upstream automatically on the next flush interval.

Full type hierarchy, ring-buffer internals, builder operators, and recommended topologies: [`docs/topology.md`](docs/topology.md).

---

## Project Structure

| Project | Layer | Description |
|---|---|---|
| `src/Relay` | Library | Core pipeline: typed + packet sinks, builders, ring buffers, native memory |
| `src/Relay.Sinks.Http` | Library | `HttpBatchSink` — HTTP POST with circuit breaker, built on `BatchSink` |
| `src/Relay.Sinks.Observability` | Library | `SeqSink` — CLEF-over-HTTP to Seq, built on `HttpBatchSink` |
| `tests/Relay.Tests` | Tests | xUnit tests per concern (chain, SPSC, MPSC, multi-broadcast, recovery drain, HTTP batch) |
| `benchmarks/Relay.Benchmarks` | Benchmarks | BenchmarkDotNet micro-benchmarks; run with `--inProcess` |

**Source layout inside `src/Relay`:**

| File / Folder | Role |
|---|---|
| `DispatchSink<T>` | Abstract typed base — `Enqueue` hot path, `IsHealthy` / `Accept` contract |
| `SpscQueueSink<T>` | Abstract SPSC async base — ring + consumer thread + recovery drain |
| `MpscQueueSink<T>` | Abstract MPSC async base — multi-producer ring + consumer thread |
| `MultiSink<T>` / `Multi2Sink<T,TC1,TC2>` | Broadcast to N typed children |
| `ForkSink<T>` | Propagate-after-accept: primary + continuation to `Next` |
| `FilterSink<T>` | Conditional gate — silently consumes non-matching items |
| `NullSink<T>` | No-op terminal typed sink |
| `SerializeSink<T>` | Bridge: typed `T` → `PacketSink` via `MemoryMarshal.AsBytes` |
| `PacketSink` | Abstract byte-payload base — same fallback semantics as typed tree |
| `SpscQueueSink` | Abstract non-generic SPSC base for byte payloads |
| `MpscQueueSink` | Abstract non-generic MPSC base for byte payloads |
| `ForkSink` / `MultiSink` / `Multi2PacketSink<TC1,TC2>` / `FilterSink` / `NullSink` | Packet-hierarchy counterparts; `Multi2PacketSink` is the CRTP 2-child variant |
| `BatchSink` | POH scratch accumulator; flushes on fill or interval; `OnFlush` hook |
| `Sinks/FileStreamSink<T>` | Binary append to `FileStream`, POH buffer, backoff recovery |
| `Sinks/TcpSink<T>` | TCP send, POH buffer, reconnect with exponential backoff |
| `Sinks/MmfSink<T>` | Memory-mapped file write, capacity-only failure mode |
| `Sinks/MemorySink<T>` | Native memory ring, last-resort typed sink, `DrainTo` on recovery |
| `Sinks/FileSink` | Byte append to `FileStream`, optional file header, backoff recovery |
| `Sinks/RotatingFileSink` | Like `FileSink` with size + date rotation and file-count cleanup |
| `Sinks/NamedPipeSink` | Length-prefixed named-pipe client (Input2Log compatible) |
| `Sinks/UdpSink` | UDP datagrams, one datagram per enqueued payload |
| `Sinks/TcpSink` | Length-framed TCP (packet hierarchy) |
| `Sinks/MemorySink` | Native memory linear buffer, last-resort packet sink |
| `Sinks/SharedMemorySink` | Synchronous MMF ring (Log2 wire protocol) |
| `PacketCallback<TState>` | Zero-alloc delegate for `ReadOnlySpan<byte>` callbacks (TState avoids closure capture) |
| `PacketReceiver` | Abstract receiver base — passive, driven by caller's `Poll()` loop; optional `Next` forward-chain |
| `Receivers/UdpReceiver<TState>` | Non-blocking UDP receive (`Socket.Poll(0, SelectRead)` + `stackalloc 1432B`) — hot path |
| `Receivers/TcpReceiver<TState>` | TCP frame receive — non-blocking at frame boundaries; management-plane (mid-frame may block on segmentation) |
| `Receivers/NamedPipeReceiver<TState>` | Synchronous named-pipe receive — management-plane |
| `Receivers/SharedMemorySpscReceiver<TState>` | Windows-only SPSC MMF ring consumer (Log2 wire protocol) — hot path |
| `Buffers/SpscRingBuffer<T>` | Lock-free SPSC ring with 128-byte padded head/tail |
| `Buffers/MpscRingBuffer<T>` | Lock-free MPSC ring (Log2 FIX #18 layout) |
| `Buffers/SpscByteRingBuffer` | Length-prefixed SPSC byte ring |
| `Buffers/MpscByteRingBuffer` | Length-prefixed MPSC byte ring with publish-bit header |
| `Builder/RelayBuilder` + `SinkChain<T,THead>` | Fluent typed chain assembly |
| `Builder/SinkChainBuilder` + `SinkChain<THead>` | Fluent packet chain assembly |
| `Builder/RelayBuilder.From*` | Factories: `From` (UDP), `FromTcp`, `FromSharedMemory`, `FromNamedPipe` |
| `Memory/RelayMemory` | `PreFault` + `VirtualLock` on ring buffer pages |
| `Internal/HfClock` | `Stopwatch.GetTimestamp()` wrapper — never `DateTime.UtcNow` |
| `Internal/SinkConstraints` | DEBUG assertion: `T` must be cache-line-aligned |

---

## Getting Started

**Prerequisites:** .NET 9 SDK.

```bash
git clone https://github.com/brunomaida/Relay.git
cd Relay
dotnet build
dotnet test tests/Relay.Tests
```

**Struct requirements (typed tree):** `T` must be `unmanaged` and a positive multiple of 64 bytes (64, 128, 192, 256, …). In DEBUG builds, `SinkConstraints.AssertCacheLineAligned<T>()` enforces this at construction time.

```csharp
[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct LogEntry
{
    [FieldOffset(0)]  public long  TimestampTicks;
    [FieldOffset(8)]  public int   Level;
    [FieldOffset(12)] public int   Code;
    [FieldOffset(16)] public fixed char Message[24]; // 24 chars = 48 bytes
}
```

---

## Use Cases & Examples

### 1 — Structured Log Sink (File with memory fallback)

Binary log records written to a local file. If the file system fails, records accumulate in native memory. When the file recovers, the memory sink drains back upstream automatically via the `Prev` pointer wired by the builder.

```csharp
using Relay;
using Relay.Builder;
using Relay.Sinks;

var head = RelayBuilder
    .Start<LogEntry, FileStreamSink<LogEntry>>(
        new FileStreamSink<LogEntry>("/var/log/app.bin"))
    .To(new MemorySink<LogEntry>())        // last-resort ring while file is down
    .Build();

head.Start();                             // spawns consumer thread

var entry = new LogEntry
{
    TimestampTicks = HfClock.NowTicks,
    Level          = 2,
    Code           = 1001,
};
head.Enqueue(in entry);                   // ~32 cycles, zero alloc

// Shutdown
head.Stop(drainTimeoutMs: 5_000);
head.Dispose();
```

---

### 2 — Tiered Persistence (File → MMF → Memory)

Three-tier fallback: fast append log, then a fixed-size memory-mapped snapshot, then native memory ring as the ultimate sink.

```csharp
var head = RelayBuilder
    .Start<MarketTick, FileStreamSink<MarketTick>>(
        new FileStreamSink<MarketTick>("/data/ticks.bin"))
    .To(new MmfSink<MarketTick>("/data/ticks.mmf", maxBytes: 512 * 1024 * 1024))
    .To(new MemorySink<MarketTick>())
    .Build();

head.Start();
head.Enqueue(in tick);
```

`MmfSink` never throws `IOException` — its failure mode is capacity exhaustion, at which point items fall to the memory ring.

---

### 3 — Remote TCP Dispatcher (Primary → Backup → Memory)

Items delivered to a primary TCP receiver first. On disconnect, the SPSC ring absorbs the burst while reconnection retries with exponential backoff (1 s → 30 s). On recovery, items accumulated downstream drain back upstream.

```csharp
var head = RelayBuilder
    .Start<TradeEvent, TcpSink<TradeEvent>>(
        new TcpSink<TradeEvent>("risk-engine.internal", port: 9090))
    .To(new TcpSink<TradeEvent>("backup-engine.internal", port: 9090))
    .To(new MemorySink<TradeEvent>())
    .Build();

head.Start();
head.Enqueue(in tradeEvent);
```

Each `TcpSink<T>` runs its own consumer thread. The producer is never blocked by a reconnect attempt.

---

### 4 — Multi Broadcast (File + TCP simultaneously)

Every item is delivered to **all children**, regardless of individual child health. `MultiSink` is broadcast, not redundancy — a child that is unhealthy silently misses items.

```csharp
var head = RelayBuilder
    .Start<SensorReading, MultiSink<SensorReading>>(
        new MultiSink<SensorReading>(
            new FileStreamSink<SensorReading>("/data/sensors.bin"),
            new TcpSink<SensorReading>("dashboard.local", 9200)))
    .To(new MemorySink<SensorReading>())   // fallback when ALL children are unhealthy
    .Build();

head.Start();
head.Enqueue(in reading);
```

**Performance-critical variant:** when both children are `sealed` types, prefer `Multi2Sink<T,TC1,TC2>` — the JIT devirtualizes both `Enqueue` calls, saving ~6 cycles.

```csharp
var multi = new Multi2Sink<SensorReading, FileStreamSink<SensorReading>, TcpSink<SensorReading>>(
    new FileStreamSink<SensorReading>("/data/sensors.bin"),
    new TcpSink<SensorReading>("dashboard.local", 9200));
```

---

### 5 — Conditional Routing / Filtering

Only route items matching a predicate. Items that do not match are silently consumed — they do **not** propagate to `Next`. This is intentional: a filtered item is not an error.

```csharp
// Only persist ERROR-level entries to the slow file sink
var errorSink = new FileStreamSink<LogEntry>("/var/log/errors.bin");

var head = RelayBuilder
    .Start<LogEntry, FilterSink<LogEntry>>(
        new FilterSink<LogEntry>(e => e.Level >= 3, errorSink))
    .Build();

errorSink.Start();
head.Enqueue(in entry);   // INFO entries are discarded; ERROR entries go to file
```

---

### 6 — Custom Backend (extend `SpscQueueSink<T>`)

Implement your own backend — a database writer, a message queue producer, a WebSocket dispatcher — by subclassing `SpscQueueSink<T>` and overriding four methods.

```csharp
using Relay;
using Relay.Sinks;

public sealed class CustomSink<T> : SpscQueueSink<T> where T : unmanaged
{
    public CustomSink()
        : base(ringCapacity: 8_192, flushIntervalMs: 100, sinkName: "custom") { }

    protected override unsafe void WriteToBackend(in T item) { /* write item */ }
    protected override void FlushBackend()                   { /* flush */      }
    protected override void TryRecoverBackend()              { _healthy = true; }
    protected override void DisposeBackend()                 { /* cleanup */    }
}
```

Wire it into a chain like any other sink:

```csharp
var head = RelayBuilder
    .Start<MetricSample, CustomSink<MetricSample>>(new CustomSink<MetricSample>())
    .To(new FileStreamSink<MetricSample>("/data/metrics-fallback.bin"))
    .Build();
```

---

### 7 — NullSink Terminal

Use `NullSink<T>` as a guaranteed-healthy terminal to prevent silent drops in chains where you want explicit no-op behaviour rather than a missing `Next`.

```csharp
var head = RelayBuilder
    .Start<TradeEvent, FileStreamSink<TradeEvent>>(
        new FileStreamSink<TradeEvent>("/data/trades.bin"))
    .To(NullSink<TradeEvent>.Instance)    // singleton, zero allocation
    .Build();
```

---

### 8 — HTTP Observability Sink (Seq via CLEF)

Byte payloads (CLEF-encoded JSON lines, produced externally) batched and POSTed to a Seq server. The circuit breaker prevents cascade failures on Seq outage.

```csharp
using Relay.Builder;
using Relay.Sinks;
using Relay.Sinks.Observability.Seq;

var http = new HttpClient();
var seq  = new SeqSink(http, serverUrl: "http://seq:5341", apiKey: "my-key");

var chain = SinkChainBuilder
    .StartSpsc(seq)
    .To(new MemorySink())              // buffer during breaker-open window
    .Head;

seq.Start();
seq.Enqueue(clefLine);                 // ReadOnlySpan<byte>, zero alloc publish
```

---

## Pipeline Topologies

The `Enqueue` / `Poll` hot paths short-circuit on health and fall through to `Next`
automatically — no producer involvement required.

---

### Simple

#### 1. Single sink

```
  Producer
     │
     ▼
  ┌──────────────────┐
  │  FileStreamSink  │
  └──────────────────┘
    (no Next — silent drop on failure)
```

```csharp
var head = RelayBuilder
    .StartSpsc<Tick, FileStreamSink<Tick>>(fileSink)
    .Build();
```

---

#### 2. Serial fallback — depth 2

Items delivered to the first healthy sink. On failure, fall through to the next.

```
  Producer
     │
     ▼
  ┌──────────────────┐  fail   ┌─────────────┐
  │  FileStreamSink  │────────▶│   TcpSink   │──▶ (drop)
  └──────────────────┘         └─────────────┘
```

```csharp
var head = RelayBuilder
    .StartSpsc<Tick, FileStreamSink<Tick>>(fileSink)
    .To(tcpSink)
    .Build();
```

---

#### 3. Serial fallback — depth 3

Classic tiered persistence: fast local file → remote TCP → native memory ring.

```
  Producer
     │
     ▼
  ┌──────────────────┐  fail   ┌─────────────┐  fail   ┌──────────────┐
  │  FileStreamSink  │────────▶│   TcpSink   │────────▶│  MemorySink  │
  └──────────────────┘         └─────────────┘         └──────────────┘
```

```csharp
var head = RelayBuilder
    .StartSpsc<Tick, FileStreamSink<Tick>>(fileSink)
    .To(tcpSink)
    .To(memorySink)
    .Build();
```

---

### Intermediate

#### 4. Broadcast — `MultiSink`

Every item goes to **all** children regardless of individual health. `Next` is reached
only when all children are unhealthy simultaneously.

```
  Producer
     │
     ▼
  ┌─────────────┐────▶ ┌──────────────────┐
  │  MultiSink  │      │  FileStreamSink  │
  └─────────────┘      └──────────────────┘
        │
        └────▶ ┌─────────────┐
               │   TcpSink   │
               └─────────────┘
  (all unhealthy → Next or drop)
```

```csharp
var head = RelayBuilder
    .Start<Tick, MultiSink<Tick>>(
        new MultiSink<Tick>(fileSink, tcpSink))
    .Build();
```

---

#### 5. Broadcast — `Multi2Sink` (CRTP, 2 sealed children)

Same semantics as `MultiSink` but the JIT devirtualizes both `Enqueue` calls when `TC1`
and `TC2` are sealed types, saving ~6 cycles per dispatch.

```
  Producer
     │
     ▼
  ┌──────────────────────┐────▶ ┌──────────────────┐  (sealed TC1)
  │  Multi2Sink<T,F,C>   │      │  FileStreamSink  │
  └──────────────────────┘      └──────────────────┘
        │
        └────▶ ┌─────────────┐  (sealed TC2)
               │   TcpSink   │
               └─────────────┘
  JIT devirtualizes both calls — ~6c saved vs array MultiSink
```

```csharp
var multi = new Multi2Sink<Tick, FileStreamSink<Tick>, TcpSink<Tick>>(
    fileSink, tcpSink);

var head = RelayBuilder
    .Start<Tick, Multi2Sink<Tick, FileStreamSink<Tick>, TcpSink<Tick>>>(multi)
    .Build();
```

---

#### 6. Fork / audit — `ForkSink`

Every item reaches the audit sink (via `Accept`) **and** propagates to `Next`
(`PropagateAfterAccept = true`). Both deliveries are synchronous on the producer thread.

```
  Producer
     │
     ▼
  ┌─────────────┐────▶ ┌──────────────────┐
  │  ForkSink   │      │   MemorySink     │  (audit — receives every item)
  └─────────────┘      └──────────────────┘
        │
        │ PropagateAfterAccept = true
        ▼
  ┌──────────────────┐
  │  FileStreamSink  │  (main delivery chain)
  └──────────────────┘
```

```csharp
var head = RelayBuilder
    .Start<Tick, ForkSink<Tick>>(new ForkSink<Tick>(memorySink))
    .To(fileSink)
    .Build();
```

---

#### 7. Conditional gate — `FilterSink`

Items that fail the predicate are **silently consumed** — they do not propagate to `Next`.
This is intentional: a filtered item is not an error.

```
  Producer
     │
     ▼
  ┌──────────────────────┐  pred true   ┌──────────────────┐
  │     FilterSink       │─────────────▶│  FileStreamSink  │
  │   (price > 0)        │              └──────────────────┘
  └──────────────────────┘
        │
        pred false
        │
      (silent drop — does not reach Next)
```

```csharp
var head = RelayBuilder
    .Start<Tick, FilterSink<Tick>>(
        new FilterSink<Tick>(t => t.Price > 0, fileSink))
    .Build();
```

---

### Complex

#### 8. Selective fallback — `FilterSink` as conditional fallback

All items go to `FileStreamSink`. On failure, only high-priority items fall through to
`TcpSink`; the rest are silently discarded. Use `.When().To()` on the builder.

```
  Producer
     │
     ▼
  ┌──────────────────┐
  │  FileStreamSink  │──(healthy)──▶ (deliver)
  └──────────────────┘
        │ fail
        ▼
  ┌───────────────────────┐  pred true   ┌─────────────┐
  │      FilterSink       │─────────────▶│   TcpSink   │
  │  (priority == High)   │              └─────────────┘
  └───────────────────────┘
        │
        pred false ──▶ (drop — low priority discarded on file failure)
```

```csharp
var head = RelayBuilder
    .StartSpsc<Tick, FileStreamSink<Tick>>(fileSink)
    .When(t => t.Priority == Priority.High)
    .To(tcpSink)
    .Build();
```

---

#### 9. Broadcast with per-branch fallback

Each broadcast branch is an independent fallback chain. Build each branch first, then
pass the branch heads as children to `MultiSink`.

```
  Producer
     │
     ▼
  ┌─────────────┐────▶ ┌──────────────────┐  fail   ┌──────────────┐
  │  MultiSink  │      │  FileStreamSink  │────────▶│  MemorySink  │
  └─────────────┘      └──────────────────┘         └──────────────┘
        │
        └────▶ ┌─────────────┐  fail   ┌──────────────┐
               │   TcpSink   │────────▶│  MemorySink  │
               └─────────────┘         └──────────────┘
```

```csharp
var fileBranch = RelayBuilder
    .StartSpsc<Tick, FileStreamSink<Tick>>(fileSink)
    .To(new MemorySink<Tick>())
    .Build();

var tcpBranch = RelayBuilder
    .StartSpsc<Tick, TcpSink<Tick>>(tcpSink)
    .To(new MemorySink<Tick>())
    .Build();

var head = RelayBuilder
    .Start<Tick, MultiSink<Tick>>(new MultiSink<Tick>(fileBranch, tcpBranch))
    .Build();
```

---

#### 10. Audit + broadcast — `ForkSink` + `MultiSink`

Every item is recorded in the audit ring, then broadcast to both file and TCP. The fork
is the head; its `Next` is the `MultiSink`.

```
  Producer
     │
     ▼
  ┌─────────────┐────▶ ┌──────────────────┐
  │  ForkSink   │      │   MemorySink     │  (audit ring — every item)
  └─────────────┘      └──────────────────┘
        │
        │ propagates
        ▼
  ┌─────────────┐────▶ ┌──────────────────┐
  │  MultiSink  │      │  FileStreamSink  │
  └─────────────┘      └──────────────────┘
        │
        └────▶ ┌─────────────┐
               │   TcpSink   │
               └─────────────┘
```

```csharp
var multi = new MultiSink<Tick>(fileSink, tcpSink);

var head = RelayBuilder
    .Start<Tick, ForkSink<Tick>>(new ForkSink<Tick>(auditSink))
    .To(multi)
    .Build();
```

---

#### 11. Full production pipeline

Gate → audit → broadcast with per-branch fallback. Build inside-out: branches first,
then multi, then fork, then filter.

```
  Producer
     │
     ▼
  ┌──────────────────────┐
  │     FilterSink       │──(false)──▶ (drop)
  │   (price > 0)        │
  └──────────────────────┘
        │ true
        ▼
  ┌─────────────┐────▶ ┌──────────────────┐
  │  ForkSink   │      │   MemorySink     │  (audit ring)
  └─────────────┘      └──────────────────┘
        │ propagates
        ▼
  ┌─────────────┐────▶ ┌──────────────────┐  fail   ┌──────────────┐
  │  MultiSink  │      │  FileStreamSink  │────────▶│  MemorySink  │
  └─────────────┘      └──────────────────┘         └──────────────┘
        │
        └────▶ ┌─────────────┐  fail   ┌──────────────┐
               │   TcpSink   │────────▶│  MemorySink  │
               └─────────────┘         └──────────────┘
```

```csharp
// Build inner chains first (inside-out)
var fileBranch = RelayBuilder
    .StartSpsc<Tick, FileStreamSink<Tick>>(fileSink)
    .To(new MemorySink<Tick>()).Build();

var tcpBranch = RelayBuilder
    .StartSpsc<Tick, TcpSink<Tick>>(tcpSink)
    .To(new MemorySink<Tick>()).Build();

var multi = new MultiSink<Tick>(fileBranch, tcpBranch);
var fork  = new ForkSink<Tick>(auditSink);

var forkChain = RelayBuilder
    .Start<Tick, ForkSink<Tick>>(fork)
    .To(multi)
    .Build();

var head = new FilterSink<Tick>(t => t.Price > 0, forkChain);
```

---

### Input Layer — Receivers

`PacketReceiver` subclasses are **passive** — the caller's coordination loop drives them
by calling `Poll()`. Each `Poll()` attempts a non-blocking receive; if a frame arrives,
it invokes `callback(state, frame)` (caller inline processing) and optionally forwards
the frame to `Next` (a `PacketSink` chain).

#### 12. Receive and process inline (callback only)

The simplest pattern: frame consumed entirely in the callback. No sink forward.

```
  [UDP :9090]
       │
       ▼
  ┌──────────────────────┐
  │  UdpReceiver<TState> │  stackalloc 1 432 B — MTU-safe, zero GC
  └──────────────────────┘
       │  Poll()
       ▼
  callback(state, frame)    ← caller processes inline
  (Next == null — frame not forwarded)
```

```csharp
var recv = RelayBuilder.From(
    local:    new IPEndPoint(IPAddress.Any, 9090),
    state:    engine,
    callback: static (eng, frame) => eng.HandlePacket(frame));

// Coordination loop (single thread — no lock needed)
while (running) recv.Poll();
```

---

#### 13. Receive, process, and persist

Callback for inline processing plus `Next` forward to a `PacketSink` fallback chain for
durable storage. Both paths execute synchronously per `Poll()` call.

```
  [UDP :9090]
       │
       ▼
  ┌──────────────────────┐
  │  UdpReceiver<TState> │  Next = head of sink chain
  └──────────────────────┘
       │  Poll()
       ├──▶ callback(state, frame)   ← caller inline processing
       │
       └──▶ Next.Enqueue(frame)
                  │
                  ▼
            ┌──────────────────┐  fail   ┌──────────────┐
            │   FileSink       │────────▶│  MemorySink  │
            └──────────────────┘         └──────────────┘
```

```csharp
var sinkChain = SinkChainBuilder
    .StartSpsc(new FileSink("/data/frames.bin"))
    .To(new MemorySink())
    .Head;

sinkChain.Start();

var recv = RelayBuilder.From(
    local:    new IPEndPoint(IPAddress.Any, 9090),
    state:    engine,
    callback: static (eng, frame) => eng.HandlePacket(frame),
    next:     sinkChain);

while (running) recv.Poll();
```

---

#### 14. Multi-protocol input → shared sink chain

Two receivers (different transports) both forward to the same sink chain. Safe when both
are polled from the same coordination thread (single producer to the sink).
Use `MpscQueueSink` as the head if receivers run on separate threads.

```
  [UDP :9090]              [SharedMemory "feed"]
       │                           │
       ▼                           ▼
  ┌────────────────┐    ┌───────────────────────────┐
  │  UdpReceiver   │    │  SharedMemorySpscReceiver  │
  └────────────────┘    └───────────────────────────┘
       │                           │
       │  Next = sinkChain         │  Next = sinkChain (same instance)
       └───────────────────────────┘
                    │
                    ▼ Next.Enqueue(frame)
              ┌─────────────────────┐  fail   ┌──────────────┐
              │  RotatingFileSink   │────────▶│  MemorySink  │
              └─────────────────────┘         └──────────────┘
```

```csharp
var sinkChain = SinkChainBuilder
    .StartSpsc(new RotatingFileSink("/data/frames", maxBytes: 256 * 1024 * 1024))
    .To(new MemorySink())
    .Head;

sinkChain.Start();

var udpRecv = RelayBuilder.From(
    new IPEndPoint(IPAddress.Any, 9090), engine,
    static (eng, frame) => eng.HandleUdp(frame),
    next: sinkChain);

var shmRecv = RelayBuilder.FromSharedMemory(
    "feed", engine,
    static (eng, frame) => eng.HandleShm(frame),
    next: sinkChain);

// Single coordination thread — both receivers share the SPSC sink safely
while (running) { udpRecv.Poll(); shmRecv.Poll(); }
```

---

## Key Concepts

| Concept | Detail |
|---|---|
| **`T : unmanaged`** | Every typed pipeline is bound to a blittable struct. No boxing, no GC pressure. |
| **`IsHealthy` gate** | Checked on every `Enqueue` call (~7 c when healthy). If false, `Accept` is skipped and `Next` is called directly. `_healthy` is written only by the consumer thread. |
| **`PropagateAfterAccept`** | `protected readonly bool` field (default `false`). When `true`, `Enqueue` continues to `Next` after a successful local `Accept` — fork/audit pattern. `ForkSink<T>` sets it to `true`; all others leave it `false`. |
| **SPSC ring** | Lock-free single-producer / single-consumer ring. Head and tail are 128-byte padded (`PaddedLong`) to prevent false sharing. `TryPublish` costs ~25 c. |
| **MPSC ring** | Lock-free multi-producer ring (Log2 FIX #18). Three isolated `PaddedLong` counters (`_claimedTail`, `_headCache`, `_head`) eliminate false sharing under producer contention. |
| **Consumer thread** | Each `SpscQueueSink<T>` / `SpscQueueSink` subclass spawns one background thread (`relay-<name>`). The thread spins → yields → sleeps on idle and flushes on a configurable interval. |
| **Recovery drain** | On each flush interval, if `Prev.IsHealthy` is true, the fallback sink drains its accumulated items back upstream via `TryDrainToPrev`. |
| **BatchSink** | POH-pinned scratch buffer that accumulates byte payloads from the SPSC ring and calls `OnFlush` when full or on the flush interval. Base for `HttpBatchSink`. |
| **Circuit breaker** | `HttpBatchSink` opens the breaker after `cbFailures` consecutive HTTP failures, dropping batches for `cbOpenDurationMs` before probing again. |
| **POH buffer** | `GC.AllocateArray<byte>(size, pinned: true)` — allocated once, never moved by the GC, safe for native I/O. |
| **`HfClock`** | All timestamps use `Stopwatch.GetTimestamp()`. `DateTime.UtcNow` is never used on any hot path. |
| **Cache-line alignment** | `sizeof(T)` must be a positive multiple of 64B so adjacent ring slots never share a cache line. Enforced in DEBUG by `SinkConstraints.AssertCacheLineAligned<T>()`. |
| **Zero production dependencies** | `src/Relay` has no NuGet references. `Relay.Sinks.Http` / `Relay.Sinks.Observability` use `System.Net.Http` only (inbox). Tests use xUnit 2.9.2 and FluentAssertions 6.12.1. |

---

## Performance

All measurements: Intel i9-12900K, hot caches, Release build.

### Cycle budget

| Operation | Cycles |
|---|---|
| `IsHealthy` check (SPSC sink, healthy) | ~7c |
| `TryPublish` (SPSC ring, not full) | ~25c |
| Successful `Enqueue` (chain depth 1) | ~32c |
| Fallback hop to SPSC `Next` (unhealthy) | +4c per hop |
| `Volatile.Write` (mfence, x64) | ~15c |
| Virtual call (predicted, branch target buffer) | ~3c |

### Steady-state guarantees

| Metric | Value |
|---|---|
| Heap allocations (hot path) | 0 — all buffers POH-pinned at construction |
| GC roots in ring slots | 0 — `T : unmanaged` only |
| Locks on hot path | 0 — `Volatile` + `Interlocked.CompareExchange` only |
| `async`/`await` on hot path | 0 — consumer threads are synchronous |

---

## Development

**Build & test:**

```bash
dotnet build
dotnet test tests/Relay.Tests
```

**Branch workflow:**

| Prefix | Use |
|---|---|
| `feature/<yyMMdd>-<slug>` | New functionality |
| `fix/<yyMMdd>-<slug>` | Bug fixes |
| `refactor/<yyMMdd>-<slug>` | Restructuring without behaviour change |

Base all branches off `develop`. Merge back to `develop` when stable.

**Commit convention:** Conventional Commits in English (`feat:`, `fix:`, `refactor:`, `chore:`, `docs:`, `test:`). Append `w/Claude` when the commit is co-authored by Claude Code.

---

## Resumo (PT-BR)

**Relay** é uma biblioteca de infraestrutura para construir pipelines de despacho com fallback automático em .NET 9. Duas hierarquias paralelas cobrem os casos de uso principais:

- **`DispatchSink<T>`** — payloads tipados (`T : unmanaged`). Zero alocação no estado estável. `Enqueue(in item)` custa ~32 ciclos em um i9-12900K com caches quentes.
- **`PacketSink`** — payloads de comprimento variável (`ReadOnlySpan<byte>`). Mesma semântica de fallback. `SerializeSink<T>` faz a ponte entre as duas árvores via `MemoryMarshal.AsBytes` sem cópia.

O produtor chama um único método — `Enqueue` — e a biblioteca cuida do roteamento: entrega ao primeiro backend saudável da cadeia, e em caso de falha (I/O, desconexão, capacidade esgotada), encaminha automaticamente ao próximo sink. Quando o backend se recupera, os itens acumulados no fallback são drenados de volta upstream (mecanismo `Prev` drain).

**Sinks concretos (tipados):** `FileStreamSink<T>`, `TcpSink<T>`, `MmfSink<T>`, `MemorySink<T>`

**Sinks concretos (packet):** `FileSink`, `RotatingFileSink`, `NamedPipeSink`, `UdpSink`, `TcpSink`, `MemorySink`, `SharedMemorySink`, `SeqSink` (CLEF/HTTP via `BatchSink` → `HttpBatchSink`)

**Casos de uso principais:**
- Gravação de eventos de alta frequência em arquivo binário com fallback em RAM
- Sink de log estruturado resiliente a falhas de disco ou rede
- Dispatcher TCP primário + secundário + ring nativo como última camada
- Fan-out broadcast para múltiplos consumidores simultâneos (`MultiSink`, `Multi2Sink`, `Multi2PacketSink`)
- Filtro condicional antes da entrega (`FilterSink`)
- Envio batch de eventos CLEF para Seq com circuit breaker (`SeqSink`)

**Garantias de desempenho:** zero alocação em steady state, sem `lock`/`Monitor`, sem `async`/`await` no caminho quente.
