# Circular Ring Topology Tests — Relay.Tests

## Context

Criar testes de performance/stress/endurance que exercitem topologias de sinks em anel circular. Objetivo: medir throughput sustentado, saturação de backpressure e pressão GC em cadeiras de múltiplos nós interconectados. O "circular" é implementado no `WriteToBackend` do consumer thread — não via `sink.Next` (que causaria recursão infinita no hot path).

---

## Critical Constraint: SPSC vs MPSC no nó de entrada

Em topologia **infinita** (producer contínuo), o último nó do ring chama `Node-1.Enqueue` no seu consumer thread — simultaneamente ao producer thread. Isso viola o contrato SPSC.

| Modo | Nó-1 (entrada) | Outros nós |
|---|---|---|
| **Finite** (producer faz join antes do ring fechar) | SpscRingNode OK | SPSC/MPSC/Sync livre |
| **Infinite** (producer contínuo) | **MpscRingNode obrigatório** | SPSC/MPSC/Sync livre |

---

## Topologias ASCII

### Ring-3 Finite (SPSC)
```
   P → [N1:Spsc] → [N2:Spsc] → [N3:Spsc] ─┐
        ↑                                    │  WriteToBackend N3 → N1
        └────────────────────────────────────┘
   sink.Next = NullSink<T>.Instance em todos os nós
```

### Ring-3 Infinite (MPSC entry)
```
   P ──→ [N1:Mpsc] → [N2:Spsc] → [N3:Spsc] ─┐
   (P + T3)↑                                  │
            └──────────────────────────────────┘
```

### Ring-5 Finite
```
   P → [N1] → [N2] → [N3] → [N4] → [N5] ─┐
        ↑                                   │
        └───────────────────────────────────┘
   (todos SpscRingNode; Infinite: N1=Mpsc)
```

### Ring-8 Mixed (finite; Infinite: N1→Mpsc)
```
   P → [N1:Spsc] → [N2:Spsc] → [N3:Mpsc] → [N4:Sync+MemorySink(8M)] →
       [N5:Spsc] → [N6:Spsc] → [N7:Mpsc] → [N8:Spsc] ─┐
        ↑                                                │
        └────────────────────────────────────────────────┘
   SyncNode em N4: Accept() count++ → _next.Enqueue (profundidade de stack = 1 por hop)
```

### Ring-13 Mixed — tabela de configuração
| Idx | Tipo | ringCapacity | flushMs | Nota |
|---|---|---|---|---|
| 1 | Mpsc (∞) / Spsc (fin) | 65536 | 10 | Entry node |
| 2 | Spsc | 32768 | 10 | |
| 3 | Spsc | 32768 | 10 | |
| 4 | Sync+Mem | 1<<23 | — | Synchronous, MemorySink backing |
| 5 | Spsc | 32768 | 10 | |
| 6 | Mpsc | 32768 | 10 | |
| 7 | Spsc | 16384 | 5 | Ring menor → mede saturação |
| 8 | Spsc | 32768 | 10 | |
| 9 | Mpsc | 65536 | 10 | |
| 10 | Spsc | 32768 | 10 | |
| 11 | Sync+Mem | 1<<23 | — | Segundo nó síncrono |
| 12 | Spsc | 32768 | 10 | |
| 13 | Spsc | 32768 | 10 | Fecha para N1 |

```
   P → [N1] → [N2] → [N3] → [N4:Sync] → [N5] → [N6] → [N7] →
       [N8] → [N9] → [N10] → [N11:Sync] → [N12] → [N13] ─┐
        ↑                                                   │
        └───────────────────────────────────────────────────┘
```

### Receiver + Sink Ring (PacketSink hierarchy, Windows-only)
```
   P → [PacketNode#1:Spsc] → SharedMemorySpscSink("relay-in") 
                                         │ MMF #1
                                         ▼
                             SharedMemorySpscReceiver("relay-in")
                                  [PollerThread T_in]
                                         │
   ┌─────────────────────────────────────┘
   ├→ [PacketNode#2:Spsc] → [PacketNode#3:Mpsc] → [PacketNode#4:Spsc] → [PacketNode#5:Spsc]
   │                                                                              │
   │                                                                              ▼
   │                                                             SharedMemorySpscSink("relay-out")
   │                                                                         │ MMF #2
   │                                                                         ▼
   │                                                        SharedMemorySpscReceiver("relay-out")
   │                                                             [PollerThread T_out]
   │                                                                         │
   └─────────────────────────────────────────────────────────────────────────┘
   (re-injeta em PacketNode#1 — fecha o ring)
   
   2 MMFs necessários: SharedMemorySpscSink é single-producer-per-instance
```

### Fluxo de dados Finite (hop counter no payload)
```
   Struct: [ HopCount:long | Id:long | padding... ]
                  ↑
                  1º campo — Unsafe.As<T,long> funciona em qualquer tamanho

   WriteToBackend:
     count++
     hopCount = Unsafe.As<T,long>(ref item)   // lê 1º campo
     hopCount--
     Unsafe.As<T,long>(ref item) = hopCount
     if (hopCount > 0) _next.Enqueue(in item)
     else droppedTerminal++
   
   Teste termina: sum(counter[i]) == N * K  (todos os hops processados)
```

### Fluxo de dados Infinite (saturação)
```
   while (sw.Elapsed < duration) { Node1.Enqueue(in item); injected++; }
   
   Backpressure:
     Ring cheio → Accept retorna false → sink.Next = NullSink absorve drop
     (ou CountingTerminalSink em saturation tests para contagem)
   
   Shutdown: Stop producer → Thread.Sleep(2s) → StopAll(ring, drainMs:5000)
   Assertion: totalProcessed ≥ injected * (ringSize - 1)  // ≥95% delivery
              gen0Δ == 0 && gen1Δ == 0
```

---

## Payload Structs (múltiplos de 64B; HopCount sempre no offset 0)

| Struct | Size | Campos extras |
|---|---|---|
| `Packet64` | 64B | `HopCount:long, Id:long` (pad 48B via Size=64) |
| `Packet128` | 128B | `HopCount:long, Id:long, Seq:long` (pad 104B) |
| `Packet256` | 256B | `HopCount:long, Id:long, Seq:long` (pad 232B) |
| `Packet320` | 320B | `HopCount:long, Id:long, Seq:long` (pad 296B) |

---

## Arquivos a Criar

```
tests/Relay.Tests/Circular/
  Helpers/
    CircularPayloads.cs      ← Packet64/128/256/320; PacketLayout (byte ring header helpers)
    RingNode.cs              ← SpscRingNode<T>, MpscRingNode<T>, SyncRingNode<T>,
                                BackendRingNode<T>, PacketRingNode, BackendPacketRingNode,
                                PacketMpscRingNode, CountingTerminalSink<T>
    RingTopology.cs          ← BuildSpscRing<T>, BuildMixedRingMpscEntry<T>, BuildPacketRing,
                                RingNodeConfig, StartAll, StopAll, TotalCount, PollerThread
    RingTestReport.cs        ← coleta janelas de throughput, GC, memória; Print(ITestOutputHelper)
  PureSinkRingTests.cs       ← Ring-3/5/8/13 sem backend I/O; finite + infinite; múltiplos payloads
  BackendSinkRingTests.cs    ← ring + cada sink concreto de /Relay/Sinks; finite + infinite
  ReceiverSinkRingTests.cs   ← Windows-only; Receiver ring; backends de rede (Tcp/Udp/NamedPipe)
  SaturationTests.cs         ← small rings, backpressure, drop accounting
```

---

## Implementação dos Helpers

### `RingNode.cs` — classes internas sealed

**`SpscRingNode<T> : SpscQueueSink<T>`**
```csharp
internal sealed class SpscRingNode<T> : SpscQueueSink<T> where T : unmanaged
{
    internal DispatchSink<T>? _next;  // ring forward — NOT sink.Next
    private long _count;              // consumer-thread-only → plain increment, no Interlocked
    private long _droppedTerminal;
    private readonly bool _decrementHops;

    public long Count => Volatile.Read(ref _count);
    public long DroppedTerminal => Volatile.Read(ref _droppedTerminal);

    protected override unsafe void WriteToBackend(in T item)
    {
        _count++;
        if (_decrementHops)
        {
            ref long hops = ref Unsafe.As<T, long>(ref Unsafe.AsRef(in item));
            if (--hops <= 0) { _droppedTerminal++; return; }
        }
        _next?.Enqueue(in item);
    }
    // FlushBackend, TryRecoverBackend, DisposeBackend: vazios
}
```

**`MpscRingNode<T> : MpscQueueSink<T>`** — mesma estrutura, herda MPSC.

**`BackendRingNode<T> : SpscQueueSink<T>`** — wraps qualquer backend real (`FileStreamSink<T>`, `MmfSink<T>`, etc.)
```csharp
internal sealed class BackendRingNode<T> : SpscQueueSink<T> where T : unmanaged
{
    private readonly DispatchSink<T> _backend; // backend real (tem próprio consumer thread)
    internal DispatchSink<T>? _next;
    private long _count;

    public long Count => Volatile.Read(ref _count);

    protected override void WriteToBackend(in T item)
    {
        _count++;
        _backend.Enqueue(in item);   // escreve no backend (async, próprio consumer thread)
        _next?.Enqueue(in item);     // forwarda no ring
    }
    // FlushBackend → _backend.Flush(); TryRecoverBackend → vazio; DisposeBackend → _backend.Dispose()
}
```
Uso: nós com I/O real no ring. Backends file-based (sem servidor necessário) em BackendSinkRingTests; backends de rede também em BackendSinkRingTests com listener/drain dedicado.

**`SyncRingNode<T> : DispatchSink<T>`** — `Accept()` (chama no producer OU consumer thread)
- Counter: `Interlocked.Increment` (multi-thread)
- Backing optional `MemorySink<T>` para saturação
- `Accept` retorna false quando MemorySink cheia (sink.Next = CountingTerminalSink absorve)

**`PacketRingNode : SpscQueueSink` (packet non-generic)**
- Scratch buffer: `GC.AllocateUninitializedArray<byte>(maxPayload, pinned: true)` no ctor
- `WriteToBackend`: lê hop via `PacketLayout.ReadHop`, decrementa, copia para scratch com header atualizado, chama `_next.Enqueue(scratch.AsSpan(0, len))`
- Custo: 1 memcpy por hop por nó (documentado; zero alocação em steady state)
- **`BackendPacketRingNode`**: mesma ideia que `BackendRingNode<T>` mas para hierarquia Packet — wraps `FileSink`, `RotatingFileSink`, etc.

**`CountingTerminalSink<T> : DispatchSink<T>`** — conta drops para saturation tests

### `RingTopology.cs`
```csharp
internal static class RingTopology
{
    public static SpscRingNode<T>[] BuildSpscRing<T>(int n, int ringCap, int flushMs, bool decrementHops) ...
    public static DispatchSink<T>[] BuildMixedRingMpscEntry<T>(RingNodeConfig[] configs) ...
    public static void StartAll<T>(DispatchSink<T>[] ring) ...  // chama Start() em Spsc/Mpsc nodes + backends
    public static void StopAll<T>(DispatchSink<T>[] ring, int drainMs) ... // ordem forward: 1..N
    public static long TotalCount<T>(DispatchSink<T>[] ring) ... // soma _count de todos
    // packet variants idem
}

// RingNodeConfig descreve cada nó:
internal record struct RingNodeConfig(
    RingNodeKind Kind,       // Spsc | Mpsc | Sync | BackendSpsc | BackendMpsc
    int RingCapacity,
    int FlushMs,
    DispatchSink? Backend);  // null para nós puros

internal sealed class PollerThread : IDisposable
{
    // Thread dedicada que chama recv.Poll() em loop com SpinWait(20) quando Poll()==false
    // Priority = AboveNormal; IsBackground = true
}
```

---

## Topologias com Backends Reais

### Ring-3 com FileStreamSink + MmfSink (typed)
```
   P → [N1:BackendSpsc(FileStreamSink<T>, temp-file)] →
       [N2:BackendSpsc(MmfSink<T>, temp-mmf)]         →
       [N3:Spsc(puro)]                                 ─┐
        ↑                                               │
        └───────────────────────────────────────────────┘
   Teardown: Delete temp files.
```

### Ring-5 com backends Packet (FileSink + RotatingFileSink)
```
   P → [N1:BackendPacket(FileSink, temp-file)]                      →
       [N2:BackendPacket(RotatingFileSink, temp-dir, maxBytes=1MB)] →
       [N3:PacketRingNode(puro)]                                     →
       [N4:BackendPacket(FileSink, temp-file2)]                      →
       [N5:PacketRingNode(puro)]                                     ─┐
        ↑                                                              │
        └──────────────────────────────────────────────────────────────┘
```

### Ring-3 com backends de rede (BackendSinkRingTests)
```
   P → [N1:Mpsc(puro)] → [N2:BackendSpsc(TcpSink<T>, loopback:portX)] → [N3:Spsc(puro)] ─┐
        ↑                          │ (consumer thread envia sobre TCP)                      │
        └──────────────────────────┼────────────────────────────────────────────────────────┘
                                   ▼
                         TcpListener drain thread (descarta, não re-injeta)
   
   Variantes: TcpSink<T> | UdpSink | NamedPipeSink | SharedMemorySpscSink em N2.
   Cada sink de rede = listener/drain dedicado que descarta sem re-injetar (evita backpressure artificial).
```

### Receiver + Sink Ring — ring atravessa IPC via Receiver (Windows-only)
```
   P → [PacketNode#1:Spsc] → SharedMemorySpscSink("relay-in")
                                         │ MMF #1
                                         ▼
                             SharedMemorySpscReceiver("relay-in")
                                  [PollerThread T_in]
                                         │
   ┌─────────────────────────────────────┘
   ├→ [PacketNode#2:Spsc] → [PacketNode#3:Mpsc] → [PacketNode#4:Spsc] → [PacketNode#5:Spsc]
   │                                                                              │
   │                                                                              ▼
   │                                                             SharedMemorySpscSink("relay-out")
   │                                                                         │ MMF #2
   │                                                                         ▼
   │                                                        SharedMemorySpscReceiver("relay-out")
   │                                                             [PollerThread T_out]
   │                                                                         │
   └─────────────────────────────────────────────────────────────────────────┘
   (re-injeta em PacketNode#1 — fecha o ring via callback do receiver)
   2 MMFs: SharedMemorySpscSink é single-producer-per-instance
```

---

## Testes — Categorias e Duração

Convenção de nome: `[Topology]_[Backend]_[Mode]_[EstimatedTime]_[Assertion]`

### `PureSinkRingTests.cs` — rings sem backend I/O

| Método | Topology | Modo | Category |
|---|---|---|---|
| `Ring3_Spsc_Finite_30s_AllItemsCirculate` | Ring-3 Spsc | Finite | — |
| `Ring3_MpscEntry_Infinite_60s_ZeroGcPressure` | Ring-3 Mpsc-entry | ∞ | Perf |
| `Ring5_Spsc_Finite_30s_AllItemsCirculate` | Ring-5 Spsc | Finite | — |
| `Ring5_MpscEntry_Infinite_120s_NoGcPressure` | Ring-5 Mpsc-entry | ∞ | Stress |
| `Ring8_Mixed_Finite_60s_AllItemsCirculate` | Ring-8 mixed | Finite | — |
| `Ring8_Mixed_MpscEntry_Infinite_300s_ZeroGcPressure` | Ring-8 Mpsc-entry mixed | ∞ | Stress |
| `Ring13_Mixed_Finite_60s_AllItemsCirculate` | Ring-13 mixed | Finite | — |
| `Ring13_Mixed_MpscEntry_Infinite_600s_EnduranceZeroGcPressure` | Ring-13 Mpsc-entry | ∞ | Endurance |
| `Ring3_Packet320_Spsc_Finite_30s_AllItemsCirculate` | Ring-3, Packet320 | Finite | — |
| `Ring8_Packet128_MpscEntry_Infinite_120s_NoGcPressure` | Ring-8, Packet128 | ∞ | Stress |
| `Ring8_Packet256_MpscEntry_Infinite_120s_NoGcPressure` | Ring-8, Packet256 | ∞ | Stress |

### `BackendSinkRingTests.cs` — ring com cada sink concreto de `/Relay/Sinks`

Cada teste usa Ring-3 (entry=Mpsc para ∞, Spsc para finite) com o sink concreto em N2.
`BackendRingNode<T>` ou `BackendPacketRingNode` — backend escreve na mídia real + forwarda no ring.

**Typed sinks (`DispatchSink<T>`, hierarquia `Entry64`):**

| Método | Backend | Modo | Category |
|---|---|---|---|
| `FileStreamSink_Ring3_Spsc_Finite_30s_AllItemsWrittenToFile` | FileStreamSink<T> (temp file) | Finite | — |
| `FileStreamSink_Ring3_MpscEntry_Infinite_60s_ZeroGcPressure` | FileStreamSink<T> | ∞ | Perf |
| `FileStreamSink_Ring3_MpscEntry_Infinite_120s_NoGcPressure` | FileStreamSink<T> | ∞ | Stress |
| `MmfSink_Ring3_Spsc_Finite_30s_AllItemsWrittenToMmf` | MmfSink<T> (temp mmf) | Finite | — |
| `MmfSink_Ring3_MpscEntry_Infinite_60s_ZeroGcPressure` | MmfSink<T> | ∞ | Perf |
| `TcpSink_Ring3_Spsc_Finite_30s_AllItemsDelivered` | TcpSink<T> (loopback) | Finite | — |
| `TcpSink_Ring3_MpscEntry_Infinite_120s_NoGcPressure` | TcpSink<T> | ∞ | Stress |
| `MemorySink_Ring3_Spsc_Finite_30s_AllItemsInNativeRing` | MemorySink<T> (SyncNode) | Finite | — |

**Packet sinks (`PacketSink`, hierarquia byte):**

| Método | Backend | Modo | Category |
|---|---|---|---|
| `FileSink_Ring3_Spsc_Finite_30s_AllFramesWritten` | FileSink (temp file) | Finite | — |
| `FileSink_Ring3_MpscEntry_Infinite_60s_ZeroGcPressure` | FileSink | ∞ | Perf |
| `RotatingFileSink_Ring3_Spsc_Finite_30s_AllFramesWritten` | RotatingFileSink (temp dir) | Finite | — |
| `RotatingFileSink_Ring3_MpscEntry_Infinite_120s_NoGcPressure` | RotatingFileSink | ∞ | Stress |
| `TcpSinkPacket_Ring3_Spsc_Finite_30s_AllFramesDelivered` | TcpSink/packet (loopback) | Finite | — |
| `TcpSinkPacket_Ring3_MpscEntry_Infinite_60s_ZeroGcPressure` | TcpSink/packet | ∞ | Perf |
| `UdpSink_Ring3_Spsc_Finite_30s_AllDatagramsSent` | UdpSink (loopback) | Finite | — |
| `UdpSink_Ring3_MpscEntry_Infinite_60s_ZeroGcPressure` | UdpSink | ∞ | Perf |
| `NamedPipeSink_Ring3_Spsc_Finite_30s_AllFramesDelivered` | NamedPipeSink | Finite | — |
| `NamedPipeSink_Ring3_MpscEntry_Infinite_60s_ZeroGcPressure` | NamedPipeSink | ∞ | Perf |
| `SharedMemorySink_Ring3_Spsc_Finite_30s_AllFramesDelivered` | SharedMemorySpscSink | Finite | — |
| `SharedMemorySink_Ring3_MpscEntry_Infinite_60s_ZeroGcPressure` | SharedMemorySpscSink | ∞ | Perf |

**Nota sobre sinks de rede:** listener/drain thread dedicado por teste — descarta sem re-injetar.
TcpSink/NamedPipe/SharedMemory: `PollerThread` com `SpinWait(20)`. UdpSink: `UdpClient.Receive` em loop.
SharedMemory/NamedPipe: Windows-only com Skip em não-Windows.

### `ReceiverSinkRingTests.cs` (Windows-only) — ring atravessa IPC via Receiver

| Método | Topology | Modo | Category |
|---|---|---|---|
| `ReceiverRing_SharedMemory_3Nodes_Finite_30s_FrameSurvivesMmfHop` | 1 MMF, 3 PacketNodes | Finite | — |
| `ReceiverRing_SharedMemory_5Nodes_Infinite_60s_AllNodesReceive` | 2 MMFs, 5 PacketNodes | ∞ | Perf |
| `ReceiverRing_SharedMemory_5Nodes_Infinite_120s_NoGcPressure` | 2 MMFs, 5 PacketNodes | ∞ | Stress |

### `SaturationTests.cs` — backpressure e drop accounting

| Método | Behavior | Category |
|---|---|---|
| `Ring5_SmallRings256_Burst_30s_TerminalCounterTracksLost` | rings cheios → drops no terminal | — |
| `Ring5_SmallRings256_SustainedOverflow_30s_NullSinkAbsorbsDrops` | NullSink como fallback | — |
| `Ring8_SyncNodeMemorySinkFull_30s_FallsThroughToTerminalCounter` | Mem cheia → sink.Next absorve | — |
| `Ring13_BurstThenStop_30s_SumProcessedPlusDropsEqualsInjected` | invariante de balanço | — |

---

## Telemetria — `RingTestReport` helper

Um helper `RingTestReport` (em `Helpers/`) coleta e imprime ao final de cada teste.

### Coleta (cold path — apenas 1x/segundo no loop do producer)
```csharp
// No loop do producer (a cada Stopwatch.Frequency ticks = 1s):
long totalNow = RingTopology.TotalCount(ring);
long windowItems = totalNow - lastCheckpointCount;
report.RecordWindow(windowItems);   // salva em long[] pré-alocado
```
`long[]` alocado no construtor do report (fora do hot path). `RecordWindow` é `O(1)`, sem alocação.

### Output de exemplo
```
=== Ring-8 Mixed — 5 Min Stress ===
Duration:              300.41 s
Injected:          1,234,567,890 items
Total processed:   9,234,567,890 hops  (≈ 7.48× injected)
Laps completed:    1,154,320,986  (finite mode)

Throughput (items/s across all nodes, 1s windows):
  Max:    32,456,789
  Avg:    30,782,115
  Min:    28,901,234
  StdDev: 1,234,567

Per-node:
  N1  Mpsc(puro)                  count=1,234,567,890  drops=        0  backend=—
  N2  Spsc(FileStreamSink<T>)     count=1,234,123,456  drops=        0  backend=OK
  N3  Mpsc(MmfSink<T>)            count=1,233,987,654  drops=      432  backend=OK
  N4  Sync(MemorySink 8M)         count=1,233,654,321  drops=      111  backend=—
  N5  Spsc(puro)                  count=1,233,321,000  drops=        0  backend=—
  N6  Mpsc(puro)                  count=1,232,987,654  drops=      346  backend=—
  N7  Spsc(small ring 16K)        count=1,232,654,321  drops=      679  backend=—
  N8  Spsc(puro)                  count=1,199,271,594  drops=  443,073  backend=—
  Terminal (NullSink absorbed):   drops=  123,456

GC pressure:
  gen0Δ=0  gen1Δ=0  gen2Δ=0   ← GATE (falha teste se != 0)

Memory:
  Before: 48.3 MB working set  /  12.1 MB managed heap
  After:  49.1 MB working set  /  12.1 MB managed heap
  Delta:  +0.8 MB WS  /  +0.0 MB heap  (esperado: 0 managed alloc)
```

### Campos coletados pelo `RingTestReport`

| Campo | Fonte | Timing |
|---|---|---|
| `Injected` | contador do loop | final |
| `TotalProcessed` | `RingTopology.TotalCount()` | final |
| `LapsCompleted` | `TotalProcessed / (N * itemCount)` | final (finite only) |
| `WindowThroughput[]` | snapshot 1s do TotalCount | durante teste |
| `Min/Avg/Max/StdDev throughput` | calculado de `WindowThroughput[]` | final |
| `PerNode.Count` | `Volatile.Read(ref node._count)` | final |
| `PerNode.Drops` | `Volatile.Read(ref node._droppedTerminal)` | final |
| `PerNode.BackendHealthy` | `node._backend?.IsHealthy` | final |
| `GC gen0/gen1/gen2 Δ` | `GC.CollectionCount()` before+after | antes + final |
| `WorkingSet64 Δ` | `Process.GetCurrentProcess()` | antes + final (cold path) |
| `GC.GetTotalMemory(false) Δ` | managed heap | antes + final |

`Process.GetCurrentProcess()` é chamado **apenas uma vez** antes e uma vez depois — nunca no hot path.

---

## Ordem de Implementação

1. `Helpers/CircularPayloads.cs` — structs de payload (~50 LOC)
2. `Helpers/RingNode.cs` — SpscRingNode, MpscRingNode, SyncRingNode, BackendRingNode, PacketRingNode, BackendPacketRingNode, CountingTerminalSink (~300 LOC)
3. `Helpers/RingTopology.cs` — factories, RingNodeConfig, StartAll/StopAll/TotalCount, PollerThread (~200 LOC)
4. `Helpers/RingTestReport.cs` — coleta janelas 1s, GC, memória, Print (~120 LOC)
5. `PureSinkRingTests.cs` — Ring-3 finite (smoke) → Ring-5 → Ring-8 → Ring-13; finite então infinite por topology
6. `BackendSinkRingTests.cs` — file-based sinks primeiro (FileStreamSink, MmfSink, FileSink, RotatingFileSink), depois sinks de rede (TcpSink, UdpSink, NamedPipeSink, SharedMemorySink)
7. `SaturationTests.cs` — rings pequenos, drive Accept-false, verificar drop accounting
8. `ReceiverSinkRingTests.cs` — Windows-only, Receiver ring (SharedMemory)

---

## Verificação

```powershell
# Commit gate (exclui long-running)
dotnet test tests/Relay.Tests --filter "FullyQualifiedName~Circular&Category!=Stress&Category!=Endurance&Category!=Perf" -c Release

# Stress (opt-in)
dotnet test tests/Relay.Tests --filter "Category=Stress&FullyQualifiedName~Circular" -c Release

# Endurance (opt-in)
dotnet test tests/Relay.Tests --filter "Category=Endurance&FullyQualifiedName~Circular" -c Release
```

---

## Riscos e Decisões

| Risco | Decisão |
|---|---|
| Ring-3 infinite + SPSC entry → undefined behavior | Nó-1 = MpscRingNode em todos os infinite tests |
| `sink.Next = firstSink` → recursão infinita | `sink.Next = NullSink<T>.Instance`; circularity via `_next` em WriteToBackend |
| SyncRingNode + stack depth | Max 2 SyncNodes não-adjacentes em Ring-8/13; profundidade limitada a 2 |
| MemorySink fill permanente (sem recovery em test) | Ring-8/13 mixed: MemorySink capacity=1<<23; saturation tests: capacity pequena intencional |
| Packet hop rewrite: 1 memcpy/hop/nó | POH-pinned scratch no ctor; zero alocação em steady state; documentado |
| Receiver ring: 2 MMFs necessários | SharedMemorySpscSink = single-producer; 2 MMFs + 2 PollerThreads |
| SharedMemory/NamedPipe/UnixSocket: Windows-only | `[SupportedOSPlatform("windows")]` + Skip em não-Windows |
| `banned-api-enforce` flagará DateTime | Usar apenas `Stopwatch` |
| BackendRingNode: backend tem consumer thread próprio | `StartAll` chama `Start()` nos backends também; `StopAll` para backends APÓS ring nodes (drain in-flight primeiro) |
| Backends de rede em BackendSinkRingTests precisam de listeners | Listener/drain thread dedicado por teste; descarta sem re-injetar (evita backpressure artificial) |
| `Process.GetCurrentProcess()` alocação | Chamado apenas 1× antes e 1× depois; nunca no hot path |
| `RingTestReport.WindowThroughput[]` sizing | Pré-alocar `long[maxDurationSeconds + 10]` no ctor; RecordWindow é O(1) sem alloc |

---

## Arquivos de Referência (leitura durante implementação)

- `src/Relay/SpscQueueSink.cs` — lifecycle Start/Stop, WriteToBackend contrato
- `src/Relay/MpscQueueSink.cs` — MPSC variante
- `src/Relay/SpscQueueSink.Packet.cs` — packet non-generic base
- `src/Relay/NullSink.cs` — singleton pattern
- `src/Relay/Sinks/MemorySink.cs` — semântica fill-once
- `src/Relay/Sinks/SharedMemorySpscSink.cs` — single-producer constraint
- `src/Relay/Receivers/SharedMemorySpscReceiver.cs` — Poll() contract
- `tests/Relay.Tests/StressTests.cs` — template GC gate + ITestOutputHelper
