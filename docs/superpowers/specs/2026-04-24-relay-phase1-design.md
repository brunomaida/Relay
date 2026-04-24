# Relay Fase 1 — Design spec

Data: 2026-04-24  
Contexto: pré-requisito para integração Log2 → Relay (Fase 2)

---

## Objetivo

Completar a hierarquia `PacketSink` no Relay com infraestrutura, builder e sinks concretos,
corrigindo bugs existentes e garantindo performance igual ou superior ao baseline Log2 atual.
Ao final desta fase, Relay está pronto para receber o adapter Log2 → Relay (Fase 2) sem
regressões de CPU ou memória.

---

## Decisões de arquitetura

### 1. `PacketSink` substitui `ByteSink`

`ByteSink` não comunica que o payload é uma unidade discreta de tamanho variável. `PacketSink`
é o nome correto: packet = unidade autocontida, variável, entregue atomicamente.

Renomeações diretas (breaking change aceitável — Relay não tem consumers externos ainda):

| Antes | Depois |
|---|---|
| `ByteSink` | `PacketSink` |
| `SpscByteQueueSink` | `SpscQueueSink` (non-generic) |
| `MpscByteQueueSink` | `MpscQueueSink` (non-generic) |
| `NullByteSink` | `NullSink` (non-generic) |
| `SpscByteRingBuffer` | sem rename (interno) |
| `MpscByteRingBuffer` | sem rename (interno) |

Concretos novos (packet): `TcpSink`, `UdpSink`, `FileSink`, `RamSink` (non-generic).  
Existentes typed: `TcpSink<T>`, `FileStreamSink<T>`, `RamSink<T>`, `MmfSink<T>` — inalterados.  
Coexistência: C# distingue por arity (`TcpSink` vs `TcpSink<T>`), sem conflito de namespace.

### 2. Sem `CircuitBreaker` em Relay

`_healthy` flag + backoff exponencial em `TryRecoverBackend` é o circuit breaker do Relay.
Qualquer IOException abre o breaker (`_healthy = false`). `TryRecoverBackend` é o half-open
probe. `Log2.Sinks.Infrastructure.CircuitBreaker` permanece exclusivo de `ChannelSinkBase`.

### 3. `SerializeSink<T>` — único ponto de cruzamento entre hierarquias

Typed → packet via `MemoryMarshal.AsBytes`: zero copy, zero alloc. Concretos packet são
usados por ambos os mundos através deste bridge. Elimina duplicação de `TcpSink<T>` + `TcpSink`
para quem precisar de TCP em cadeias typed.

### 4. SPSC para todos os concretos da Fase 1

Log2 tem um único consumer thread — único produtor para o Relay. MPSC não é necessário para
o bridge Log2 → Relay. `MpscQueueSink` (non-generic) existe como base para uso direto do
Relay com múltiplos produtores — sem concretos MPSC nesta fase.

### 5. Sem `RotatingFileSink` em Relay core

Rotação requer semântica de formato (headers, contagem de registros, políticas de cleanup).
Relay é agnóstico de formato. `FileSink` é simples: abre, acumula, faz flush, fecha. Rotação
fica em `Log2.Sinks` ou futuro `Relay.Sinks.Local`.

### 6. `FileSink` com header opcional

`ReadOnlyMemory<byte>? header` no construtor. Escrito uma vez no `Start()`, antes de qualquer
payload. Conteúdo definido pelo caller — Relay não conhece o formato.

---

## Hierarquia completa após Fase 1

```
PacketSink (abstract)
├── SpscQueueSink (abstract, SPSC consumer thread)
│   ├── TcpSink
│   ├── UdpSink
│   └── FileSink
├── MpscQueueSink (abstract, MPSC consumer thread)  ← base only, sem concretos
├── NullSink
├── ForkSink
├── MultiSink
├── FilterSink
└── RamSink                                         ← sem consumer thread
```

---

## Namespaces

| Namespace | Conteúdo |
|---|---|
| `Relay` | `PacketSink`, `SpscQueueSink`, `MpscQueueSink`, `NullSink`, `ForkSink`, `MultiSink`, `FilterSink`, `SerializeSink<T>`, `PacketPredicate` |
| `Relay.Sinks` | `TcpSink`, `UdpSink`, `FileSink`, `RamSink` |
| `Relay.Builder` | `SinkChainBuilder`, `SinkChain<THead>`, `MultiBuilder`, `FilterBinding<THead>` |

---

## Correção de bug — `SpscQueueSink.Flush()` cross-thread

**Problema:** `SpscByteQueueSink.Flush()` chama `FlushBackend()` diretamente. `FlushBackend()`
é documentado como "consumer thread only." `Flush()` pode ser chamado de qualquer thread.
Race condition com o consumer thread.

**Fix:** idêntico ao `SpscQueueSink<T>` typed.

```csharp
private int _flushRequested;

public override void Flush() => Volatile.Write(ref _flushRequested, 1);
```

No `ConsumeLoop`, checar junto ao deadline:

```csharp
bool flushDue = Volatile.Read(ref _flushRequested) == 1
             || HfClock.NowTicks >= flushDeadline;
if (flushDue)
{
    Volatile.Write(ref _flushRequested, 0);
    FlushBackend();
    TryRecoverBackend();
    TryDrainToPrev();
    flushDeadline = HfClock.NowTicks + _flushIntervalTicks;
}
```

---

## `PacketSink` — contrato

```csharp
public abstract class PacketSink : IDisposable
{
    public PacketSink? Next { get; internal set; }
    public abstract bool IsHealthy { get; }

    // JIT elimina branch em sealed subclasses com retorno constante.
    public virtual bool PropagateAfterAccept => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(ReadOnlySpan<byte> payload)
    {
        if (IsHealthy && Accept(payload))
        {
            if (PropagateAfterAccept) Next?.Enqueue(payload);
            return;
        }
        Next?.Enqueue(payload);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract bool Accept(ReadOnlySpan<byte> payload);

    public abstract void Flush();
    public abstract void Dispose();
}
```

---

## `SerializeSink<T>`

```csharp
public sealed class SerializeSink<T> : DispatchSink<T> where T : unmanaged
{
    private readonly PacketSink _target;

    public SerializeSink(PacketSink target) => _target = target;

    public override bool IsHealthy => _target.IsHealthy;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in T item)
    {
        _target.Enqueue(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in item, 1)));
        return true;
    }

    public override void Flush()   => _target.Flush();
    public override void Dispose() => _target.Dispose();
}
```

`Accept` retorna `true` sempre: a cadeia packet assumiu responsabilidade de entrega. Cadeia
typed não recebe sinal de falha pós-aceite — equivalente à semântica de `ForkSink<T>`.

---

## `ForkSink`

```csharp
public sealed class ForkSink : PacketSink
{
    private readonly PacketSink _primary;

    public ForkSink(PacketSink primary) => _primary = primary;

    public override bool PropagateAfterAccept => true;
    public override bool IsHealthy            => _primary.IsHealthy;

    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        _primary.Enqueue(payload);
        return _primary.IsHealthy;
    }

    public override void Flush()   { _primary.Flush();   Next?.Flush();   }
    public override void Dispose() { _primary.Dispose(); Next?.Dispose(); }
}
```

---

## `MultiSink`

```csharp
public sealed class MultiSink : PacketSink
{
    private readonly PacketSink[] _children;

    public MultiSink(params PacketSink[] children) => _children = children;

    public override bool IsHealthy
    {
        get { foreach (var c in _children) if (c.IsHealthy) return true; return false; }
    }

    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        foreach (var c in _children) c.Enqueue(payload);
        return true;
    }

    public override void Flush()   { foreach (var c in _children) c.Flush();   Next?.Flush(); }
    public override void Dispose() { foreach (var c in _children) c.Dispose(); }
}
```

Sem `Multi2Sink` CRTP — YAGNI. Array de 2–4 elementos com loop é suficiente nesta fase.

---

## `FilterSink`

```csharp
public delegate bool PacketPredicate(ReadOnlySpan<byte> payload);

public sealed class FilterSink : PacketSink
{
    private readonly PacketPredicate _predicate;
    private readonly PacketSink      _downstream;

    public FilterSink(PacketPredicate predicate, PacketSink downstream)
    {
        _predicate  = predicate;
        _downstream = downstream;
    }

    // Sempre true. Se IsHealthy espelhasse o downstream, `PacketSink.Enqueue` puláaria o
    // Accept quando downstream ficasse unhealthy e o payload vazaria para Next — violando
    // o contrato "itens reprovados pelo predicado nunca propagam". O downstream gerencia
    // sua própria saúde e sua própria cadeia de fallback.
    public override bool IsHealthy => true;

    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        if (_predicate(payload)) _downstream.Enqueue(payload);
        return true; // itens que falham: consumidos, sem propagação para Next
    }

    public override void Flush()   => _downstream.Flush();
    public override void Dispose() => _downstream.Dispose();
}
```

---

## `SinkChainBuilder`

```csharp
public static class SinkChainBuilder
{
    public static SinkChain<THead> Start<THead>(THead head)
        where THead : PacketSink      => new(head);

    public static SinkChain<THead> StartSpsc<THead>(THead head)
        where THead : SpscQueueSink   => new(head);

    public static SinkChain<THead> StartMpsc<THead>(THead head)
        where THead : MpscQueueSink   => new(head);
}

public sealed class SinkChain<THead> where THead : PacketSink
{
    public THead Head { get; }
    private PacketSink _tail;

    internal SinkChain(THead head) { Head = head; _tail = head; }

    public SinkChain<THead> To(PacketSink sink)
    {
        _tail.Next = sink;
        if (sink is SpscQueueSink spsc) spsc.Prev = _tail;
        _tail = sink;
        return this;
    }

    public SinkChain<THead> Fork(PacketSink primary)
    {
        var fork = new ForkSink(primary);
        _tail.Next = fork;
        _tail = fork;
        return this;
    }

    public FilterBinding<THead> When(PacketPredicate predicate) => new(this, predicate);

    public SinkChain<THead> Multi(params PacketSink[] children)
    {
        var multi = new MultiSink(children);
        _tail.Next = multi;
        _tail = multi;
        return this;
    }

    public static implicit operator PacketSink(SinkChain<THead> chain) => chain.Head;

    // Chamado por FilterBinding.To: instala o filter no tail corrente e avança o tail
    // para o downstream — o filter é terminal para o predicado, o downstream estende a cadeia.
    internal void AppendFilter(FilterSink filter, PacketSink downstream)
    {
        _tail.Next = filter;
        _tail      = downstream;
    }
}

public sealed class FilterBinding<THead> where THead : PacketSink
{
    private readonly SinkChain<THead> _chain;
    private readonly PacketPredicate  _predicate;

    internal FilterBinding(SinkChain<THead> chain, PacketPredicate predicate)
    {
        _chain     = chain;
        _predicate = predicate;
    }

    public SinkChain<THead> To(PacketSink downstream)
    {
        var filter = new FilterSink(_predicate, downstream);
        _chain.AppendFilter(filter, downstream);
        return _chain;
    }
}
```

---

## Sinks concretos

### `TcpSink`

- Estende `SpscQueueSink`
- POH send buffer: `GC.AllocateArray<byte>(capacity, pinned: true)`, default 64 KB
- Socket: non-blocking, `NoDelay = true`, `KeepAlive = true`
- Framing: 4 bytes Big-Endian length-prefix antes de cada payload (compatível com
  Input2Log TCP/NamedPipe/UnixSocket/SharedMemory — `BinaryPrimitives.ReadInt32BigEndian`
  em `src/Input2Log/TcpInput.cs:64`)
- `WriteToBackend`: acumula no send buffer; flush automático quando cheio
- `FlushBackend`: `_socket.Send(buffered)` + reset contador
- `TryRecoverBackend`: fecha socket, recria, conecta; backoff 1 000 → 30 000 ms exponencial
- `DisposeBackend`: `Shutdown + Close`

### `UdpSink`

- Estende `SpscQueueSink`
- Socket pré-conectado, `DontFragment = true`
- `maxPayload` configurável, default 65 507 bytes
- `WriteToBackend`: `_socket.Send(payload)` direto — sem buffer de acumulação
- `FlushBackend`: no-op
- `TryRecoverBackend`: recria socket pré-conectado; backoff 1 000 → 10 000 ms

### `FileSink`

- Estende `SpscQueueSink`
- POH write buffer: `GC.AllocateArray<byte>(capacity, pinned: true)`, default 64 KB
- `ReadOnlyMemory<byte>? header`: escrito no `Start()` antes de qualquer payload
- `WriteToBackend`: acumula no write buffer; flush automático quando cheio
- `FlushBackend`: `_stream.Write(buffered)` + `_stream.Flush()` + reset
- `TryRecoverBackend`: fecha + reabre `FileMode.Append`; backoff 1 000 → 60 000 ms
- Sem rotação

### `RamSink`

Fallback de última instância. Sem consumer thread — acumula durante outage do primário
e é reproduzido por `DrainTo(target)` na recuperação.

**Contrato de thread:**
- SPSC não-concorrente. `Accept` roda no producer; `DrainTo` no consumer de recuperação.
  NUNCA simultâneos. Caller garante quiescência do producer antes de invocar `DrainTo`.
- Sem CAS. `Volatile.Write`/`Volatile.Read` em `_head` e `_tail` bastam — a ausência
  de concorrência real elimina a necessidade de barreira mais forte.

**Layout e capacidade:**
- Buffer: `NativeMemory.AlignedAlloc(capacity, 64)`, `capacity` power-of-two, default 4 MB.
- Record: `[uint32 length (host order)][payload][padding até múltiplo de 4 bytes]`.
  `recordSize = 4 + ((payloadLen + 3) & ~3)`. Length em host-order por ser buffer
  process-local — não atravessa wire.
- Fill-once não-circular. Buffer enche linearmente de 0 a `_capacity`. Quando o
  próximo record não couber, `Accept` retorna `false` e `IsHealthy` fica `false`.
- Drenagem parcial NÃO libera capacidade. Só a drenagem total (`_head >= _tail`)
  reseta `_head = _tail = 0` — somente então o buffer volta a aceitar writes.
- Espaço disponível: `_capacity - _tail` (posição absoluta, não delta).

**API:**
- `Accept(payload)`: `if (_tail + recordSize > _capacity) return false;` depois escreve
  length + payload + padding e `Volatile.Write(ref _tail, _tail + recordSize)`.
- `IsHealthy`: `_tail < _capacity` (aproximação conservadora; `Accept` é autoritativo).
- `DrainTo(PacketSink target)`: lê de `_head` até `Volatile.Read(_tail)`, chama
  `target.Enqueue(span)`; para se `target.IsHealthy == false`; no fim da drenagem
  total, zera os ponteiros.
- `Dispose`: `NativeMemory.AlignedFree`. Idempotente via flag `_disposed`.

---

## Benchmarks e gates

### Baseline

Executar antes de qualquer commit da Fase 1 com BDN + `MemoryDiagnoser` +
`HardwareCounters(CacheMisses, BranchMispredictions)`. Salvar em
`benchmarks/baselines/2026-04-24-before-phase1.json`.

Benchmarks de baseline:

| Benchmark | Configuração |
|---|---|
| `EnqueueThroughput` | Log2 `TcpSink` direto, 1 produtor, steady state |
| `FanOut` | Log2 `SinkRouter` com 2, 3, 4 sinks |
| `FileWrite` | Log2 `FileSink`, 1 produtor, payload 128B |
| `UdpEnqueue` | Log2 `UdpSink`, 1 produtor |

### Gates — Camada 1 (infraestrutura)

- `dotnet test` passa sem falhas
- Build release: verificar IL de `PropagateAfterAccept` em sealed subclass — branch deve ser
  eliminado pelo JIT (inspecionar com `dotnet-dump` ou Rider IL viewer)

### Gates — Camada 2 (concretos)

| Métrica | Gate |
|---|---|
| `ns/op` vs baseline | ≤ +10% |
| `B/op` steady state | 0 |
| `Gen0` steady state | 0 |
| `ns/op` fan-out 2 sinks | ≤ baseline × 1,15 |
| `ns/op` fan-out 4 sinks | ≤ baseline × 1,35 |

### Benchmarks novos (sem baseline anterior)

| Benchmark | O que mede |
|---|---|
| `PacketChainFallback` | custo de fallback com sink primário unhealthy |
| `SerializeSinkOverhead` | custo de `SerializeSink<T>` vs `TcpSink<T>` direto |
| `RamSinkDrainTo` | tempo de `DrainTo` com buffer cheio → `TcpSink` |
| `FlushLatency` | intervalo entre `Flush()` e entrega confirmada no backend |

`SerializeSinkOverhead` gate adicional: overhead de `MemoryMarshal.AsBytes` +
double `IsHealthy` check ≤ 5 ciclos vs caminho typed direto.

---

## Testes

### Camada 1

| Arquivo | Cobertura |
|---|---|
| `PacketSinkChainTests` | chain N nós, fallback, PropagateAfterAccept, drop silencioso |
| `ForkSinkTests` | tee healthy, fallback unhealthy, Flush/Dispose propagam |
| `MultiSinkTests` | broadcast, IsHealthy OR, Next quando todos unhealthy |
| `FilterSinkTests` | passa predicado → downstream; falha → descartado; Next nunca acionado |
| `SinkChainBuilderTests` | To, Fork, When/To, Multi — wiring correto de Next e Prev |
| `SerializeSinkTests` | conversão correta, IsHealthy espelha target, zero alloc |
| `SpscQueueSinkFlushTests` | Flush cross-thread não chama FlushBackend; consumer processa na iteração seguinte |

### Camada 2

| Arquivo | Cobertura |
|---|---|
| `TcpSinkTests` | framing 4B BE, payload íntegro, falha/recuperação, drain, dispose idempotente |
| `UdpSinkTests` | payload sem framing, excede maxPayload → unhealthy, recuperação |
| `FileSinkTests` | header uma vez, acumulação, flush interval, append em recuperação, dispose |
| `RamSinkTests` | buffer cheio → false, DrainTo ordenado, DrainTo com target unhealthy, Dispose sem leak |

**Convenções:**

- Nenhum mock framework — receivers fake estendem `PacketSink` ou usam sockets em loopback
- `Thread.Sleep` apenas com `Stop(drainTimeoutMs)` como gate de conclusão
- Nomes: `MethodName_Condition_ExpectedResult`
- Cada arquivo de teste < 300 LOC; helpers em `tests/Relay.Tests/TestSinks/`

---

## Camadas de entrega

```
Camada 1 — infraestrutura (desbloqueia Fase 2 com Camada 2)
  PacketSink (rename + PropagateAfterAccept)
  SpscQueueSink / MpscQueueSink / NullSink (renames + bug fix Flush)
  ForkSink / MultiSink / FilterSink / SerializeSink<T>
  SinkChainBuilder / SinkChain<THead> / FilterBinding<THead>
  Testes Camada 1

Camada 2 — transportes primários (gate de performance obrigatório)
  BDN baseline (Log2 atual) → salvar JSON
  TcpSink
  UdpSink
  FileSink
  RamSink
  Benchmarks novos
  Testes Camada 2

Camada 3 — IPC (pode correr em paralelo com Fase 2)
  NamedPipeSink
  UnixSocketSink
  MmfSink
  SharedMemorySink  ← requer teste de interop Input2Log antes de qualquer código
```

---

## Fora de escopo nesta fase

- `RotatingFileSink` — responsabilidade de Log2 ou `Relay.Sinks.Local` (futuro)
- `Multi2Sink` CRTP non-generic — YAGNI até BDN demonstrar ganho
- Concretos MPSC — base `MpscQueueSink` existe; concretos sob demanda
- Qualquer conhecimento de `LogRecord` ou formato Log2 dentro de Relay
- Camada 3 IPC (scoped separadamente)
