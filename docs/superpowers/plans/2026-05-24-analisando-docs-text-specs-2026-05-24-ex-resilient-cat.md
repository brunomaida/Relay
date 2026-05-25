# Relatório de Triagem — `docs/text-specs/2026-05-24-ExternalAI-Audit.md`

## Context

O documento `docs/text-specs/2026-05-24-ExternalAI-Audit.md` reúne dois pareceres externos (Gemini 3.1 e Codex 5.5) sobre o Relay. Vários itens são **bugs de correção reais** (uso multi-processo do `SharedMemorySink`, envios parciais de socket nos sinks packet, overflow de buffer em `FileSink`), outros são **ganhos de performance mensuráveis** (zero-fill no MPSC, batch hook real, afinidade de CPU), e outros ainda são **falsos positivos ou trade-offs deliberados** (stride MPSC, “cache trashing” em `IsFull/IsEmpty`, exclusão de testes Stress).

O objetivo deste relatório é separar com clareza:

| Categoria | Critério |
|---|---|
| **OBRIGATÓRIO** | Bug de correção comprovado por leitura do código — sem risco mensurar, é só não corromper |
| **RECOMENDADO** | Melhoria de performance/robustez com benchmark existente que valida ROI antes do merge |
| **OPCIONAL** | Ganho marginal, doc-only, ou requer benchmark adicional para justificar |
| **DESCARTAR** | Falso positivo, design intencional, ou refactor desproporcional ao ganho |

Cada item lista o arquivo:linha do código real, a benchmark que mede o impacto (quando aplicável), e a justificativa.

---

## 0 — Baseline Freeze (pré-requisito)

**Antes de qualquer mudança da §1 ou §2, gravar baseline novo.** Justificativa: a baseline tracked atual em `benchmarks/artifacts/2026-04-29-hotpath/` tem 24 dias e cobre apenas `ByteEnqueueBenchmarks`, `EnqueueBenchmarks`, `FilterSinkBenchmarks`, `MpscBenchmarks`. Não cobre `MpscContentionBenchmarks`, `MpscSlotLayoutBenchmarks`, `SharedMemorySinkBenchmarks`, `RotatingFileSinkBenchmarks`, `HttpBatchSink`, `QueuePipeThroughputBenchmarks` nem variantes Packet. Comparar PRs futuros contra essa baseline seria maçã vs pera — houve commits desde então (`133e6ff`, `ac608b3`, `665402e`, etc.).

### Procedimento

1. Branch isolada para baseline (não mexe em `develop`):
   ```
   git switch develop && git pull
   git switch -c bench/260524-baseline-pre-audit
   ```
2. Rodar suite completa em Release, ambiente quiescent (sem outros processos pesados):
   ```
   dotnet run -c Release --project benchmarks/Relay.Benchmarks --
   ```
3. Copiar resultados para diretório versionado:
   ```
   mkdir benchmarks/artifacts/2026-05-24-pre-audit
   cp -r BenchmarkDotNet.Artifacts/results/* benchmarks/artifacts/2026-05-24-pre-audit/results/
   cp BenchmarkDotNet.Artifacts/BenchmarkRun-*.log benchmarks/artifacts/2026-05-24-pre-audit/
   ```
4. Commit + merge para `develop` (não passa pela suíte de testes, só doc):
   ```
   git add benchmarks/artifacts/2026-05-24-pre-audit/
   git commit -m "bench: freeze baseline before external-audit triage"
   ```
5. Sanity check: comparar ad-hoc o subset comum (`MpscBenchmarks`, `EnqueueBenchmarks`) entre `2026-04-29-hotpath` e `2026-05-24-pre-audit`. Se houver delta >10% sem explicação, investigar antes de prosseguir — pode haver regressão silenciosa entre as duas datas.

### Cobertura mínima exigida

A baseline `2026-05-24-pre-audit` deve incluir **todos os benchmarks listados nos gates da §2**:

| Item §2 | Benchmark gate obrigatório |
|---|---|
| 2.1 zero-fill | `MpscContentionBenchmarks`, `MpscSlotLayoutBenchmarks` |
| 2.2 batch hook | `QueueSinkThroughputBenchmarks`, `QueueSinkPacketThroughputBenchmarks` |
| 2.3 afinidade | `QueueSinkThroughputBenchmarks` (cenário sustained) — adicionar se não houver |
| 2.4 WaitOnAddress backoff | **novo** benchmark sparse-burst — criar ANTES de mexer no backoff |
| 2.5 RotatingFileSink absolute time | `RotatingFileSinkBenchmarks.ShouldRotate_HotPath` |
| 2.6 SinkConstraints release | n/a (cold-path, sem gate) |

Se algum benchmark da tabela não existir hoje, **criar antes do freeze**. Não rodar baseline sem cobertura completa — caso contrário PR da §2 vai inventar um “antes” que não existe.

---

## 1 — OBRIGATÓRIO (correção)

### 1.1 `SharedMemorySink` — CAS publica `WriteIndex` antes do payload
- **Arquivos:** `src/Relay/Sinks/SharedMemorySink.cs:96-103` (CAS) → `:108-121` (copy)
- **Fonte:** Gemini §Lógica, Codex §Lógica (ambos flagam)
- **Defeito:** `Interlocked.CompareExchange` no `WRITE_IDX_OFF` é feito antes de `payload.CopyTo`. Leitor em outro processo lê `WriteIndex` atualizado e consome bytes ainda não escritos. Reordering de store também derruba a invariante mesmo single-producer no `WriteRing`.
- **Correção:** 2-phase commit. Reservar local (`_claimedTail` privado por processo) → copiar payload → `sfence` (`Thread.MemoryBarrier()` ou `Volatile.Write` da `WriteIndex`) só depois. Ou flag `Ready` por frame em vez de cursor único.
- **Benchmark:** `SharedMemorySinkBenchmarks` mede caminho atual; reproduzir com leitor concorrente em teste de stress packet IPC.
- **Custo:** +10–30 ciclos/frame (Codex) — irrelevante perto da gravidade.
- **Nota:** Tipo declara “MPSC-tolerant” mas semântica de publicação não sustenta (Codex). Ou corrige semântica, ou restringe tipo a SPSC e renomeia.

### 1.2 `TcpSink.Packet` / `UnixSocketSink` — `Socket.Send` único sem loop de envio parcial
- **Arquivos:** `src/Relay/Sinks/TcpSink.Packet.cs:66`, `src/Relay/Sinks/UnixSocketSink.cs:57`
- **Fonte:** Codex §Erros
- **Defeito:** Um único `_socket.Send(span)` sem checar retorno. Sob backpressure ou `SocketFlags.None` o kernel pode aceitar menos bytes, gerando frame truncado no peer.
- **Referência:** `src/Relay/Sinks/TcpSink.cs:118-158` (variante tipada) já tem o loop correto — copiar o pattern.
- **Correção:** loop `while (sent < total) sent += _socket.Send(span.Slice(sent))` + recovery em falha como na versão tipada.
- **Custo hot path:** 0–5 ciclos quando envia tudo de uma vez (fast path inalterado).
- **Benchmark:** `TcpSinkBenchmark` (packet) — deve continuar verde; correção é cold path no caminho rápido.

### 1.3 Sinks com buffer fixo — payload > buffer quebra consumer
- **Arquivos:** `src/Relay/Sinks/FileSink.cs:45-54` e equivalentes em `RotatingFileSink`, `TcpSink`, `NamedPipeSink`, `UnixSocketSink`
- **Fonte:** Codex §Erros
- **Defeito:** Após flush condicional, `payload.CopyTo(_writeBuffer.AsSpan(_filled))` é executado sem comparar `payload.Length` contra `_writeBuffer.Length`. Payload maior que o buffer escreve fora do array → exceção, thread morre, pipeline interrompido.
- **Correção:** Se `payload.Length > _writeBuffer.Length`, escrever direto no backend (path bypass) ou retornar `false` em `Accept` para cair no `Next`.
- **Custo hot path:** 1 branch previsível (~1 ciclo).
- **Benchmark:** nenhum atual mede esse path; pode-se medir no `FileSinkBenchmark` com payload acima do buffer (cenário separado).

### 1.4 `SinkChain.Packet.cs` — não cabos `Prev` para `MpscQueueSink`
- **Arquivo:** `src/Relay/Builder/SinkChain.Packet.cs:22`
- **Fonte:** Codex §Lógica
- **Defeito:** Builder packet faz `if (sink is SpscQueueSink spsc) spsc.Prev = _tail;` — falta o case análogo para `MpscQueueSink`. Drain reverso de recovery para MPSC packet fica inoperante quando a chain é construída via builder.
- **Referência:** `src/Relay/Builder/SinkChain.cs:106-107` (genérico) já cobre ambos os tipos — replicar.
- **Correção:** adicionar `else if (sink is MpscQueueSink mpsc) mpsc.Prev = _tail;` no método `To`.
- **Custo hot path:** 0 (cold path do builder).
- **Teste:** adicionar caso em `RecoveryDrainTests` (packet MPSC).

---

## 2 — RECOMENDADO (perf mensurável)

### 2.1 Remover zero-fill no consumo do `MpscRingBuffer<T>`
- **Arquivo:** `src/Relay/Buffers/MpscRingBuffer.cs:136` (TryConsume) e `:159` (TryConsumeBatch)
- **Fonte:** Codex §Perf (#1 do veredito)
- **Análise:** `Unsafe.InitBlockUnaligned(... ValueAt(pos), 0, sizeof(T))` é redundante. O gating de publicação é o `Published` flag — produtor só reentra na slot quando `Published == 0`, e nesse ponto sobrescreve `Value` completo. Zerar Value escreve cache line extra e força ownership consumer→producer de volta.
- **Ganho esperado (Codex):** ~20–80 c/item sem contenção, ~60–150 c/item cross-core. Throughput MPSC +5–20%.
- **Benchmark gate:** `MpscContentionBenchmarks` (N=1,2,4,8) e `MpscSlotLayoutBenchmarks` antes/depois. PR só merge se ambos ≥ +5% mediana.
- **Risco:** zero. T é `unmanaged` (sem refs GC), produtor reescreve antes do próximo consumo.

### 2.2 Batch hook real no backend (eliminar dispatch virtual por item)
- **Arquivos:** `src/Relay/SpscQueueSink.cs:167-173`, `src/Relay/MpscQueueSink.cs:149-155`
- **Fonte:** Codex §Arch (#3 do veredito)
- **Análise:** `TryConsumeBatch` copia ring→`_consumeBuf`, depois `for(i) WriteToBackend(in _consumeBuf[i])` — N chamadas virtuais + N writes individuais. Backends com buffer interno (`FileSink`, `TcpSink`) poderiam receber o span inteiro.
- **Correção:** adicionar `protected virtual void WriteBatchToBackend(ReadOnlySpan<T> batch)` com default chamando o loop atual. Sinks tipo `FileSink` sobrescrevem para gravar em uma só chamada.
- **Ganho esperado (Codex):** ~8–25 c/item; QueueSinkThroughputBenchmarks +5–15%.
- **Benchmark gate:** `QueueSinkThroughputBenchmarks` e variantes packet. Aceitar só se ≥ +5% mediana.

### 2.3 Afinidade de CPU + revisão de `ThreadPriority`
- **Arquivos:** `src/Relay/SpscQueueSink.cs:82-89`, `src/Relay/MpscQueueSink.cs:85-92` (default `BelowNormal`)
- **Fonte:** Codex §Perf (#1 do veredito), Gemini §Arch
- **Análise:** `ThreadPriority.BelowNormal` como default em consumer ultra-low-latency é antinatural — favorece thread em pool genérico em vez do consumer crítico. Codex aponta p999 +20–60% com pinning. Migração de core custa milhares de ciclos (TLB shootdown, L1/L2 refill).
- **Correção:** (a) tornar `ThreadPriority` parâmetro do ctor com default `Normal`; (b) adicionar `IdealProcessor`/`AffinityMask` opcional via P/Invoke (`SetThreadAffinityMask` Win, `pthread_setaffinity_np` Linux).
- **Benchmark gate:** novo cenário em `QueueSinkThroughputBenchmarks` rodando com pin vs sem pin sob carga sustentada. Aceitar se p999 mediana melhora ≥ 20%.
- **Não fazer:** consolidar threads em pool customizado (Gemini §Arch). Refactor desproporcional, quebra ownership single-consumer dos rings SPSC/MPSC. Documentar limite recomendado (ex: ≤ N_cores sinks ativos) e ponto.

### 2.4 Backoff sem `Thread.Sleep(1)` no `ConsumeLoop`
- **Arquivos:** `src/Relay/SpscQueueSink.cs:179-196`, `src/Relay/MpscQueueSink.cs:161-178`
- **Fonte:** Gemini §Perf
- **Análise:** Após 10×`SpinWait(20)` + 5×`Yield()`, cai em `Thread.Sleep(1)`. No Windows com timer default (15.6 ms) ou mesmo com `timeBeginPeriod(1)` (1 ms), libera CPU por uma janela enorme em contexto ultra-low-latency. Só importa quando ring está vazio, mas é exatamente onde p999 da próxima mensagem mora.
- **Correção:** substituir o sleep por `WaitOnAddress` (Win) / `futex` (Linux) com timeout curto, sinalizado pelo produtor após `TryPublish`. Sem timeout dispara fallback após N ms.
- **Ganho esperado:** p999/p9999 cai de ms para µs no padrão sparse-bursty.
- **Risco:** complexidade de pareamento producer/consumer wake. Comportamento atual é OK para steady-state (consumer não dorme); o ganho é em bursts.
- **Benchmark gate:** novo cenário “sparse burst” (publish 1 a cada 5 ms) medindo p99/p999 end-to-end. Não há hoje — criar antes de mexer.

### 2.5 `RotatingFileSink` — ancorar em `DateTime.UtcNow` absoluto
- **Arquivo:** `src/Relay/Sinks/RotatingFileSink.cs:65-66, 101, 187-193`
- **Fonte:** Gemini §Lógica
- **Análise:** `_nextDayBoundaryTicks = HfClock.NowTicks + (msUntilMidnight * Frequency / 1000)` mistura referencial absoluto (midnight UTC) com referencial relativo (`Stopwatch.GetTimestamp()`). Hibernação ou drift de TSC desincroniza a rotação.
- **Correção:** armazenar `_currentDay` (DateTime UTC date) e checar `if (DateTime.UtcNow.Date > _currentDay) RotateNow();` no hot check. Cost: ~30–40c (UtcNow + comparison) — fora do steady-state batch real.
- **Benchmark:** `RotatingFileSinkBenchmarks` (`ShouldRotate_HotPath`) já existe. Deve manter ≤ +5% de overhead.

### 2.6 `SinkConstraints.AssertCacheLineAligned<T>` — promover a release
- **Arquivos:** `src/Relay/Internal/SinkConstraints.cs:18-19` (`[Conditional("DEBUG")]`); callers em `SpscQueueSink.cs:69`, `MpscQueueSink.cs:72`
- **Fonte:** Codex §Mem
- **Análise:** Cold-path no ctor. Custo desprezível em release. Tipo errado em prod = ~50–200 c/item por bounce de cache line. Garantia barata.
- **Correção:** remover `[Conditional("DEBUG")]` ou criar versão release-checked (`EnsureCacheLineAligned`) chamada pelos ctors.
- **Risco:** consumidores existentes com T desalinhado quebram em release. Aceitável — é o ponto.

---

## 3 — OPCIONAL (docs ou ganhos marginais)

### 3.1 Comentário enganoso em `DispatchSink<T>`
- **Arquivo:** `src/Relay/DispatchSink.cs:8`
- **Fonte:** Codex §Naming
- **Defeito:** XML doc lista “32, 64, 128, or 256 bytes”, mas `SinkConstraints` exige múltiplo positivo de 64. T=32 explode em runtime.
- **Correção:** alinhar comentário com “positive multiple of 64 bytes”.

### 3.2 XML doc de thread-safety nos sinks
- **Fonte:** Gemini §Naming
- **Análise:** Adicionar `<remarks>` indicando topologia (SPSC/MPSC/lock-free) em `MemorySink`, `FilterSink`, `MultiSink` etc. Evita usuário envolvendo `Enqueue` em `lock`.

### 3.3 `HttpBatchSink` — alocações por flush
- **Arquivo:** `src/Relay.Sinks.Http/HttpBatchSink.cs:93-96`
- **Fonte:** Codex §Mem
- **Análise:** `batch.ToArray()` + `ByteArrayContent` + `HttpRequestMessage` por flush. Não afeta `Enqueue`, mas em alto flush gera Gen0. ArrayPool + `PooledByteArrayContent` customizado resolve.
- **Decisão:** opcional — usuário do `Relay.Sinks.Http` pode tolerar. Só justificar com benchmark de Gen0/latency sob flush alto.

---

## 4 — DESCARTAR (falsos positivos ou design deliberado)

### 4.1 “IsFull/IsEmpty causam cache trashing” (Gemini)
- **Veredito:** FALSO.
- **Evidência:** `SpscRingBuffer.cs:50-61`: produtor lê `_head` via `Volatile.Read` 1× (não trashing), com `_cachedHead` (linhas 88–91) reduzindo ainda mais. `MpscRingBuffer.cs:60-64`: `IsEmpty` lê per-slot `Published` flag, não o cursor compartilhado. Head/tail/claimedTail em `PaddedLong` (128B isolados).
- **Ação:** nenhuma.

### 4.2 “Stride 64+sizeof(T) destrói localidade espacial” (Gemini)
- **Veredito:** FALSO / by-design.
- **Evidência:** `MpscRingBuffer.cs:81`. Layout é `[Published flag, cache-line isolated][Value]` por slot. Isolação do `Published` é necessária — produtores fazem CAS em flags de slots vizinhos concorrentemente; sem padding teríamos false sharing severo. O commit `1358588` (audit v4) já investiu nessa decisão.
- **Trade-off:** prefetcher entrega só ~50% utilização para varredura sequencial de Value. Esse é o preço pago para eliminar bounces de cache line entre produtores MPSC. Compensa.
- **Ação:** documentar trade-off em `<remarks>` do tipo. Não alterar layout.

### 4.3 “Thread explosion → migrar para thread pool customizado” (Gemini)
- **Veredito:** REJEITAR.
- **Análise:** Cada `SpscQueueSink` exige consumer único proprietário do head do ring — isso é parte do contrato SPSC. Pool genérico violaria o single-consumer e exigiria sincronização nova. Custo de N threads é assumido; usuário com >50 sinks ativos pode multiplexar externamente. Documentar limite recomendado (ex: “1 sink ativo por core físico”).

### 4.4 “Stress tests não excluídos por filtro” (Codex)
- **Veredito:** FALSO.
- **Evidência:** `[Trait("Category", "Stress")]` aplicado nos arquivos relevantes (`StressTests.cs:29,62`, `SpscByteRingBufferTests.cs:163`, etc.); filtro `Category!=Stress` no gate do CLAUDE.md exclui corretamente.
- **Ação:** nenhuma. (Pode haver teste lento sem categoria — auditar separado, fora desta análise.)

---

## 5 — Resumo executivo

| Categoria | Itens | Custo total (eng) | Ganho |
|---|---|---|---|
| Obrigatório | 1.1 → 1.4 | ~1–2 dias | Elimina corrupção IPC, frame truncado, overflow buffer, drain MPSC quebrado |
| Recomendado | 2.1 → 2.6 | ~3–5 dias + benchmarks | MPSC +5–20%, throughput +5–15%, p999 +20–60% (com pinning), correção time drift |
| Opcional | 3.1 → 3.3 | <1 dia | Clareza de contrato, Gen0 menor em HttpBatch |
| Descartar | 4.1 → 4.4 | 0 | Não fazer |

---

## 6 — Verificação

- Antes de cada mudança da seção §2 (Recomendado), rodar:
  ```
  dotnet run -c Release --project benchmarks/Relay.Benchmarks -- --filter <nome>
  ```
  e arquivar resultado em `docs/benchmarks/`.
- Cada PR da seção §2 inclui no commit body “before/after” da benchmark gate específica do item.
- Cada PR da seção §1 inclui teste novo cobrindo o caso de falha:
  - 1.1 — teste IPC com reader concorrente lendo cursor parcial
  - 1.2 — mock socket retornando `Send` parcial
  - 1.3 — payload > buffer enviado ao sink, validar não-throw
  - 1.4 — chain packet com MPSC + falha no head, verificar drain reverso
- Gate de commit do CLAUDE.md continua válido para tudo:
  ```
  dotnet test Relay.sln -c Release --filter "Category!=Endurance&Category!=Stress&Category!=Perf"
  ```

---

## 7 — Sequência de execução sugerida

Ordem por risco/dependência:

0. **§0 Baseline Freeze** — pré-requisito de tudo. Gravar `benchmarks/artifacts/2026-05-24-pre-audit/` antes de tocar em qualquer arquivo de `src/`. Criar benchmark sparse-burst novo se for executar §2.4.
1. §1.4 (`SinkChain.Packet` Prev) — 30 min, isolado, destrava drain.
2. §1.3 (oversized payload) — 1 sink por vez, padronizar.
3. §1.2 (partial Send) — replicar pattern do `TcpSink<T>`.
4. §1.1 (`SharedMemorySink` 2-phase commit) — design review + teste IPC concorrente.
5. §3.1 (comentário `DispatchSink`) + §2.6 (`SinkConstraints` release) — junto, ambos baratos.
6. §2.5 (`RotatingFileSink` tempo absoluto) — único, com benchmark gate.
7. §2.1 (zero-fill MPSC) → mede; §2.2 (batch hook) → mede; §2.3 (afinidade) → mede. Em ordem decrescente de ROI esperado.
8. §2.4 (`WaitOnAddress` backoff) — só após ter benchmark sparse-burst novo.
9. §3.2/§3.3 — quando sobrar tempo.
