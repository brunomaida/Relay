# Relay

<!-- auto:header -->
**Composable fallback dispatch pipeline for `unmanaged` structs — .NET 9 / C# 13**

Zero allocation on the hot path. No locks. No LINQ. No `async`. Each item is delivered to the first healthy backend in the chain; on failure, it falls through to the next pipe automatically.
<!-- /auto:header -->

---

<!-- auto:overview -->
## Overview

Relay is an infrastructure library for building composable, resilient dispatch pipelines over blittable (`T : unmanaged`) data. The core abstraction is `DispatchPipe<T>`: a node that tries to deliver an item to its local backend, and on any failure — including transient I/O errors, connection drops, or capacity limits — forwards the item to the next pipe in the chain.

Pipelines are assembled with a fluent builder and require no external orchestrator. Each pipe manages its own health, recovery, and backpressure. The producer calls a single method (`Enqueue`) at whatever rate it needs; the library handles the rest.

**Intended use cases:**

- High-frequency event recording (market data, telemetry, sensor feeds)
- Resilient structured log sinks that survive I/O outages without losing records
- Tiered persistence: fast local storage → remote TCP receiver → RAM buffer → drop
- Fan-out broadcast to multiple independent consumers (file + network simultaneously)
- Conditional routing / filtering before delivery
<!-- /auto:overview -->

---

<!-- auto:architecture -->
## How It Works

```
Producer
   │
   ▼
[Pipe 1 — IsHealthy? → Accept(item)]
   │ failure (IsHealthy=false OR Accept=false)
   ▼
[Pipe 2 — IsHealthy? → Accept(item)]
   │ failure
   ▼
[Pipe N — last resort, e.g. RamPipe]
   │ failure (ring full)
   ▼
 silent drop
```

**`Enqueue` is the only public entry point.** It is always synchronous from the producer's perspective. `SpscQueuePipe<T>` subclasses buffer items in a lock-free SPSC ring and drain them on a dedicated consumer thread — the producer never blocks waiting for I/O.

**Recovery drain:** when a failed pipe recovers, items accumulated in the downstream fallback pipe are drained back upstream automatically on the next flush interval.
<!-- /auto:architecture -->

---

<!-- auto:project-structure -->
## Project Structure

| Project | Layer | Description |
|---|---|---|
| `src/Relay` | Library | Core pipeline: pipes, builder, ring buffer, native memory |
| `tests/Relay.Tests` | Tests | xUnit tests per concern (chain, SPSC, fan-out, recovery drain) |

**Source layout inside `src/Relay`:**

| File / Folder | Role |
|---|---|
| `DispatchPipe<T>` | Abstract base — `Enqueue` hot path, `IsHealthy` / `Accept` contract |
| `SpscQueuePipe<T>` | Abstract async base — SPSC ring + consumer thread + recovery |
| `FanOutPipe<T>` / `FanOut2Pipe<T,TC1,TC2>` | Broadcast to N children |
| `FilterPipe<T>` | Conditional gate — silently consumes non-matching items |
| `NullPipe<T>` | No-op terminal sink |
| `Pipes/FileStreamPipe<T>` | Binary append to `FileStream`, POH buffer, backoff recovery |
| `Pipes/TcpPipe<T>` | TCP send, POH buffer, reconnect with exponential backoff |
| `Pipes/MmfPipe<T>` | Memory-mapped file write, capacity-only failure mode |
| `Pipes/RamPipe<T>` | Native memory circular ring, last-resort sink, `DrainTo` on recovery |
| `Buffers/SpscRingBuffer<T>` | Lock-free SPSC ring with 128-byte padded head/tail |
| `Builder/RelayBuilder` + `PipeChain<T,THead>` | Fluent chain assembly |
| `Memory/RelayMemory` | `PreFault` + `VirtualLock` on ring buffer pages |
| `Internal/HfClock` | `Stopwatch.GetTimestamp()` wrapper — never `DateTime.UtcNow` |
| `Internal/PipeConstraints` | DEBUG assertion: `T` must be cache-line-aligned |
<!-- /auto:project-structure -->

---

<!-- auto:getting-started -->
## Getting Started

**Prerequisites:** .NET 9 SDK.

```bash
git clone https://github.com/brunomaida/Relay.git
cd Relay
dotnet build
dotnet test tests/Relay.Tests
```

**Struct requirements:** `T` must be `unmanaged` and a positive multiple of 64 bytes (64, 128, 192, 256, …). In DEBUG builds, `PipeConstraints.AssertCacheLineAligned<T>()` enforces this at construction time.

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
<!-- /auto:getting-started -->

---

<!-- auto:use-cases -->
## Use Cases & Examples

### 1 — Structured Log Sink (File with RAM fallback)

Binary log records written to a local file. If the file system fails, records accumulate in RAM. When the file recovers, the RAM pipe drains back upstream automatically via the `Prev` pointer wired by the builder.

```csharp
using Relay;
using Relay.Builder;
using Relay.Pipes;

var head = RelayBuilder
    .Start<LogEntry, FileStreamPipe<LogEntry>>(
        new FileStreamPipe<LogEntry>("/var/log/app.bin"))
    .To(new RamPipe<LogEntry>())          // last-resort ring while file is down
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

### 2 — Tiered Persistence (File → MMF → RAM)

Three-tier fallback: fast append log, then a fixed-size memory-mapped snapshot, then native RAM ring as the ultimate sink.

```csharp
var head = RelayBuilder
    .Start<MarketTick, FileStreamPipe<MarketTick>>(
        new FileStreamPipe<MarketTick>("/data/ticks.bin"))
    .To(new MmfPipe<MarketTick>("/data/ticks.mmf", maxBytes: 512 * 1024 * 1024))
    .To(new RamPipe<MarketTick>())
    .Build();

head.Start();
head.Enqueue(in tick);
```

`MmfPipe` never throws `IOException` — its failure mode is capacity exhaustion, at which point items fall to the RAM ring.

---

### 3 — Remote TCP Dispatcher (Internal → Remote fallback)

Items delivered to a local TCP receiver first. On disconnect, the SPSC ring absorbs the burst while reconnection retries with exponential backoff (1 s → 30 s). On recovery, items accumulated downstream drain back upstream.

```csharp
var head = RelayBuilder
    .Start<TradeEvent, TcpPipe<TradeEvent>>(
        new TcpPipe<TradeEvent>("risk-engine.internal", port: 9090))
    .To(new TcpPipe<TradeEvent>("backup-engine.internal", port: 9090))
    .To(new RamPipe<TradeEvent>())
    .Build();

head.Start();
head.Enqueue(in tradeEvent);
```

Each `TcpPipe` runs its own consumer thread. The producer is never blocked by a reconnect attempt.

---

### 4 — Fan-Out Broadcast (File + TCP simultaneously)

Every item is delivered to **all children**, regardless of individual child health. `FanOutPipe` is broadcast, not redundancy — a child that is unhealthy silently misses items.

```csharp
var head = RelayBuilder
    .Start<SensorReading, FanOutPipe<SensorReading>>(
        new FanOutPipe<SensorReading>(
            new FileStreamPipe<SensorReading>("/data/sensors.bin"),
            new TcpPipe<SensorReading>("dashboard.local", 9200)))
    .To(new RamPipe<SensorReading>())     // fallback when ALL children are unhealthy
    .Build();

head.Start();
head.Enqueue(in reading);
```

**Performance-critical variant:** when both children are `sealed` types, prefer `FanOut2Pipe<T,TC1,TC2>` — the JIT devirtualizes both `Enqueue` calls, saving ~6 cycles.

```csharp
var fan = new FanOut2Pipe<SensorReading, FileStreamPipe<SensorReading>, TcpPipe<SensorReading>>(
    new FileStreamPipe<SensorReading>("/data/sensors.bin"),
    new TcpPipe<SensorReading>("dashboard.local", 9200));
```

---

### 5 — Conditional Routing / Filtering

Only route items matching a predicate. Items that do not match are silently consumed — they do **not** propagate to `Next`. This is intentional: a filtered item is not an error.

```csharp
// Only persist ERROR-level entries to the slow file sink
var errorSink = new FileStreamPipe<LogEntry>("/var/log/errors.bin");

var head = RelayBuilder
    .Start<LogEntry, FilterPipe<LogEntry>>(
        new FilterPipe<LogEntry>(e => e.Level >= 3, errorSink))
    .Build();

errorSink.Start();
head.Enqueue(in entry);   // INFO entries are discarded; ERROR entries go to file
```

---

### 6 — Custom Backend (extend `SpscQueuePipe<T>`)

Implement your own backend — a database writer, a message queue producer, a UDP broadcaster — by subclassing `SpscQueuePipe<T>` and overriding four methods.

```csharp
using Relay;
using Relay.Pipes;

public sealed class UdpPipe<T> : SpscQueuePipe<T> where T : unmanaged
{
    private readonly UdpClient _udp;
    private readonly IPEndPoint _endpoint;
    private readonly byte[] _sendBuffer;
    private static readonly int EntrySize = Unsafe.SizeOf<T>();

    public UdpPipe(string host, int port)
        : base(ringCapacity: 8_192, flushIntervalMs: 100, pipeName: "udp")
    {
        _endpoint   = new IPEndPoint(IPAddress.Parse(host), port);
        _udp        = new UdpClient();
        _sendBuffer = GC.AllocateArray<byte>(EntrySize, pinned: true);
    }

    protected override unsafe void WriteToBackend(in T item)
    {
        Unsafe.CopyBlockUnaligned(
            ref _sendBuffer[0],
            ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in item)),
            (uint)EntrySize);
        _udp.Send(_sendBuffer, EntrySize, _endpoint);
    }

    protected override void FlushBackend()         { /* UDP is datagram — no buffer */ }
    protected override void TryRecoverBackend()    { _healthy = true; }  // UDP is connectionless
    protected override void DisposeBackend()       => _udp.Dispose();
}
```

Wire it into a chain like any other pipe:

```csharp
var head = RelayBuilder
    .Start<MetricSample, UdpPipe<MetricSample>>(
        new UdpPipe<MetricSample>("metrics.local", 9125))
    .To(new FileStreamPipe<MetricSample>("/data/metrics-fallback.bin"))
    .Build();
```

---

### 7 — NullPipe Terminal

Use `NullPipe<T>` as a guaranteed-healthy terminal to prevent silent drops in chains where you want explicit no-op behaviour rather than a missing `Next`.

```csharp
var head = RelayBuilder
    .Start<TradeEvent, FileStreamPipe<TradeEvent>>(
        new FileStreamPipe<TradeEvent>("/data/trades.bin"))
    .To(NullPipe<TradeEvent>.Instance)    // singleton, zero allocation
    .Build();
```
<!-- /auto:use-cases -->

---

<!-- auto:key-concepts -->
## Key Concepts

| Concept | Detail |
|---|---|
| **`T : unmanaged`** | Every pipeline is typed to a blittable struct. No boxing, no GC pressure. |
| **`IsHealthy` gate** | Checked on every `Enqueue` call (~7 c when healthy). If false, `Accept` is skipped and `Next` is called directly. `_healthy` is written only by the consumer thread. |
| **SPSC ring** | Lock-free single-producer / single-consumer ring. Head and tail are 128-byte padded (`PaddedLong`) to prevent false sharing. `TryPublish` costs ~25 c. |
| **Consumer thread** | Each `SpscQueuePipe<T>` subclass spawns one background thread (`relay-<name>`). The thread spins → yields → sleeps on idle, and flushes on a configurable interval. |
| **Recovery drain** | On each flush interval, if `Prev.IsHealthy` is true, the fallback pipe drains its accumulated items back upstream via `TryDrainToPrev`. |
| **POH send buffer** | `GC.AllocateArray<byte>(size, pinned: true)` — allocated once in the constructor, never moved by the GC, safe to pin for native I/O. |
| **`HfClock`** | All timestamps use `Stopwatch.GetTimestamp()`. `DateTime.UtcNow` is never used on any hot path. |
| **Cache-line alignment** | `sizeof(T)` must be a positive multiple of 64B so adjacent ring slots never share a cache line. Enforced in DEBUG by `PipeConstraints.AssertCacheLineAligned<T>()`. |
| **Zero external dependencies** | `src/Relay` has no NuGet references. Tests use xUnit 2.9.2 and FluentAssertions 6.12.1 via central package management. |
<!-- /auto:key-concepts -->

---

<!-- auto:development -->
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

**Model routing:**
- Sonnet — implementation, refactor, tests
- Opus — architectural decisions, new concrete pipes, performance analysis
<!-- /auto:development -->

---

<!-- auto:resumo-ptbr -->
## Resumo (PT-BR)

**Relay** é uma biblioteca de infraestrutura para construir pipelines de despacho com fallback automático sobre structs blittáveis (`T : unmanaged`) em .NET 9.

O produtor chama um único método — `Enqueue(in item)` — e a biblioteca cuida do roteamento: entrega ao primeiro backend saudável da cadeia, e em caso de falha (I/O, desconexão, capacidade esgotada), encaminha automaticamente ao próximo pipe. Quando o backend se recupera, os itens acumulados no fallback são drenados de volta upstream.

**Casos de uso principais:**
- Gravação de eventos de alta frequência em arquivo binário com fallback em RAM
- Sink de log estruturado resiliente a falhas de disco ou rede
- Dispatcher TCP primário + secundário + ring nativo como última camada
- Fan-out broadcast para múltiplos consumidores simultâneos
- Filtro condicional antes da entrega

**Garantias de desempenho:** zero alocação em steady state, sem `lock`/`Monitor`, sem `async`/`await` no caminho quente. `Enqueue` custa ~32 ciclos em condições normais em um i9-12900K com caches quentes.
<!-- /auto:resumo-ptbr -->
