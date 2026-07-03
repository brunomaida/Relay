# Plan: Add Pipeline Topology Diagrams to README.md

## Context

README.md has one generic ASCII diagram (lines 39–53) showing a linear fallback chain. It
does not show the combinator patterns (Multi, Fork, Filter) or the Receiver input layer
that feed packets into sink chains. The goal is a `## Pipeline Topologies` section with 14
flow diagrams organized in four groups:

- **Simple** — serial fallback chains (3 patterns)  
- **Intermediate** — broadcast, fork, filter combinators (4 patterns)  
- **Complex** — composed combinations (4 patterns)  
- **Input layer — Receivers** — how `PacketReceiver` subclasses feed into sink chains (3 patterns)

Each diagram: runtime flow first (unicode box-drawing), builder/factory snippet below.
Serves both newcomers evaluating adoption and integrators looking for a composition reference.

---

## Insertion Point

**Insert between line 61 (`---`) and line 63 (`## Project Structure`).**

New section header: `## Pipeline Topologies`  
Rough line growth: ~260–290 lines (429 → ~700).

---

## Diagram Conventions

| Symbol | Meaning |
|---|---|
| `┌──┐` / `│  │` / `└──┘` | Sink or receiver node |
| `────▶` | Data flow (healthy / success path) |
| `fail` + `────▶` | Fallback path (IsHealthy=false or Accept=false) |
| `▼` | Propagation to Next (ForkSink PropagateAfterAccept) |
| `├────▶` / `└────▶` | MultiSink broadcast branches |
| `├──▶` cb / `└──▶` Next | Receiver: callback path + forward-to-sink path |
| `(drop)` | Terminal — no Next wired |
| `(silent drop)` | FilterSink predicate miss |

---

## Section Content — All 14 Diagrams

Insert the following block between the `---` at line 61 and `## Project Structure` at line 63:

````markdown
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
````

---

## Implementation Steps

1. **Read** `README.md` to confirm line 61 is `---` and line 63 is `## Project Structure`.
2. **Edit** `README.md`: insert the block above between the `---` at line 61 and
   `## Project Structure` at line 63. Use the `Edit` tool.
3. **Verify** README renders correctly: 14 diagrams present, markdown fences closed.
4. **Run** `dotnet build src/Relay` — smoke test (docs-only change).

---

## Verification

- `## Pipeline Topologies` section with 4 sub-sections:
  - Simple (1–3), Intermediate (4–7), Complex (8–11), Input Layer — Receivers (12–14)
- 14 numbered diagrams, each with an ASCII flow block and a builder/factory snippet.
- `docs/topology.md` link at line 59 intact.
- Build passes: `dotnet build src/Relay`.
