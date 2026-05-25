# Plano — Execução das 4 decisões pendentes (§2.2, §2.3, §2.4, §3.3)

## Context

Após executar o plano original de triagem do audit Gemini+Codex, restaram 4 itens da seção §2 (Recomendado / perf mensurável) e §3 (Opcional). A lição central de §2.1 (rejeitado por benchmark) é que **claims numéricos do audit não são aceitáveis como base** — `+5-20% MPSC throughput` previsto virou `+46-50% regressão sob alta contenção` quando medido. Os 4 itens restantes serão executados com disciplina mais rigorosa:

1. **Hipótese técnica explícita** antes de qualquer código — qual mecanismo de hardware/runtime explica o ganho esperado, e por que o cenário do Relay o ativa.
2. **Benchmark que mede o mecanismo, não o proxy** — se a hipótese é "elimina cache bounce X→Y", o benchmark deve isolar essa transação, não medir throughput end-to-end onde ruído de outros caches domina.
3. **Gate definido ANTES de rodar** — não ajustar threshold pós-fato.
4. **Disposição se gate falha** — definida em advance: revert, accept-with-caveat, ou rerun-with-config-X.
5. **Disciplina BDN** — runs em ambiente quiescent, comparação por **mediana**, ratio com IC 99.9% sem sobreposição.

Cada uma das 4 tasks abaixo segue esse template.

---

## §2.2 — Batch hook real no backend

### Hipótese técnica
**Mecanismo:** `SpscQueueSink<T>.ConsumeLoop` faz `TryConsumeBatch(_consumeBuf)` (até N items copiados do ring para buffer local), depois `for (i=0..N) WriteToBackend(in _consumeBuf[i])` — N **virtual calls** + N **writes** ao backend. Backends com buffer interno (`FileSink`, `TcpSink`, `NamedPipeSink`) já fazem batching, então as N calls fazem N append-to-internal-buffer com lógica de flush-quando-cheio. Substituindo por `WriteBatchToBackend(ReadOnlySpan<T>)` em um único call:
- Elimina N-1 virtual dispatch (~3c × (N-1), ~21c para N=8)
- Elimina N-1 entradas/saídas de `WriteToBackend` (prologue/epilogue, ~5c × (N-1))
- Backend escreve span inteiro no buffer de uma vez (`Buffer.MemoryCopy` em vez de loop)

**Magnitude esperada:** ~50-150c por batch de 32 items = ~2-5c por item. **Menor que o audit previu (8-25c/item)** — sem ilusões.

**Por que pode falhar / risco:**
- `WriteBatchToBackend` recebendo `ReadOnlySpan<T>` onde `T : unmanaged` exige `MemoryMarshal.AsBytes(span)` ou similar para chegar no backend (que tipicamente quer `ReadOnlySpan<byte>`). Custo: ~0c (reinterpret cast), mas API fica menos limpa.
- Se backend tem state per-item (e.g. compress por item, header per record), batch hook não ajuda — fica overhead inútil. Verificar caso-a-caso.
- JIT já pode estar inlinando `WriteToBackend` quando sink é `sealed` — devirtualização atual pode anular metade do ganho previsto.

### Implementação

**Files alterados (estimativa):**
- `src/Relay/SpscQueueSink.cs` — adicionar `protected virtual void WriteBatchToBackend(ReadOnlySpan<T> batch)` com default chamando o loop atual. Loop em `ConsumeLoop` chama `WriteBatchToBackend(_consumeBuf.AsSpan(0, consumed))`.
- `src/Relay/MpscQueueSink.cs` — idem.
- `src/Relay/SpscQueueSink.Packet.cs` + `src/Relay/MpscQueueSink.Packet.cs` — idem (signature: `ReadOnlySpan<byte>`).
- `src/Relay/Sinks/FileSink.cs` — override `WriteBatchToBackend` chamando `_stream.Write(MemoryMarshal.AsBytes(batch))` (typed) ou `_stream.Write(batch)` direto (packet — já é `ReadOnlySpan<byte>`).
- `src/Relay/Sinks/TcpSink.cs` — override usando `SendAll(MemoryMarshal.AsBytes(batch))`.
- `src/Relay/Sinks/RotatingFileSink.cs` — packet hierarchy, override.
- `src/Relay/Sinks/NamedPipeSink.cs` — packet, override.
- `src/Relay/Sinks/UnixSocketSink.cs` — packet, override.

Backends que NÃO se beneficiam (não overridar):
- `MmfSink<T>` — capacity check por item, struct-aligned write; batch span exige rewrite do per-record. Skip.
- `MemorySink<T>` — circular ring single-T write. Skip.
- `SharedMemorySpscSink` — frame-header per record, skip.

### Benchmark — design

**Existing:**
- `QueuePipeThroughputBenchmarks.Push_Single` — push 1 item/call, measures end-to-end. NÃO isola o ganho — domina latency do producer-side TryPublish.
- `QueuePipeThroughputBenchmarks.Push_Batch32` — push 32 items/call. Mais próximo do alvo. **Esse é o benchmark gate primário.**

**Novo benchmark complementar para isolar a hipótese:**
- `BatchHookBenchmark` (nova classe em `benchmarks/Relay.Benchmarks/Internal/`) — invoca diretamente `WriteToBackend` em loop vs `WriteBatchToBackend` com span de 32. Usa um sink concreto sealed (`InMemoryByteSink` que apenas faz `_total += span.Length`) para isolar dispatch+entry/exit cost de I/O real.
  - Methods: `Loop32_PerItemDispatch`, `Single32_BatchDispatch`
  - `[DisassemblyDiagnoser]` para confirmar devirtualization

**Gate (PRE-DEFINIDO):**
- `Push_Batch32` (typed throughput): median improvement **≥ 3%** vs `2026-05-24-pre-audit` baseline. (Audit previu 5-15%; após §2.1 falsificação, abaixo 3% atribuímos a ruído.)
- `BatchHookBenchmark.Single32_BatchDispatch` ≤ 0.7× `Loop32_PerItemDispatch` median (i.e. batch path é pelo menos 30% mais rápido na nano-bench isolada — se não, fix não engaja o mecanismo).
- Tests `dotnet test --filter "Category!=Endurance&Category!=Stress&Category!=Perf"` continuam 0 fails.

**Disposição se gate falha:**
- `BatchHookBenchmark` mostra <30% melhora: hipótese sobre dispatch cost falsa em .NET 9 (JIT já inlinou). REVERT.
- `BatchHookBenchmark` ok mas `Push_Batch32` <3%: hipótese isolada confirma mas não engaja em path real (provavelmente porque outras componentes dominam). Documentar finding, REVERT.
- Ambos ok: MERGE.

---

## §2.3 — CPU affinity + revisão `ThreadPriority`

### Hipótese técnica
**Mecanismo:** Default `ThreadPriority.BelowNormal` faz o consumer concorrer com qualquer thread Normal/AboveNormal no scheduler — Windows pode preemptá-lo. Sem affinity, scheduler pode migrar o consumer entre cores; cada migração: TLB shootdown (~500c) + L1d refill (~1000c, 64 lines × 16c) + L1i refill (~500c) + branch predictor reset (~hundreds of cycles). Total estimado: **2000-4000c por migração**.

Para um consumer rodando a 1M items/sec, mesmo 1 migração/sec não é nada em throughput (~0.4%), mas mata p99/p999 (uma mensagem fica 2-4µs pior). Em sparse-burst (1 item/5ms), p999 é dominado por essa migração.

**Pinning** (`SetThreadAffinityMask`) + `Priority = Normal` (ou AboveNormal):
- Elimina migrações
- Mantém cache quente entre invocações
- Aceita mais CPU quando há contenção

**Magnitude esperada (post-§2.1 calibration):** throughput **flat ou +0-2%** (não é onde mora o ganho); **p99/p999 -10% a -40%** sob carga sustentada misturada com outros workloads na máquina.

**Por que pode falhar / risco:**
- Em máquina dedicada (sem outros workloads), scheduler já mantém consumer no mesmo core empiricamente. Pinning sem benefício. Real win só aparece em ambiente compartilhado (servidor com ≥4 processos competindo).
- Pinning em CPU errado (e.g. um E-core do 12700 em vez de P-core) regrede severamente. Default deve ser "primeiro P-core livre".
- Sobre-comprometer (pinning múltiplos consumers ao mesmo core) é pior que default.
- ThreadPriority.AboveNormal pode causar starvation de outras threads do mesmo processo se mal usado (e.g. logger consumer matando UI thread).

### Implementação

**Files alterados:**
- `src/Relay/SpscQueueSink.cs` + packet — adicionar parâmetros opcionais ao ctor:
  ```csharp
  public SpscQueueSink<T>(
      int ringCapacity,
      int flushIntervalMs,
      string sinkName,
      ThreadPriority threadPriority = ThreadPriority.Normal,    // NEW default
      int affinityCpu = -1)                                       // NEW
  ```
  Default new = Normal (não BelowNormal). `affinityCpu = -1` = sem pin (preserva comportamento atual).
- Em `Start()`: se `_affinityCpu >= 0`, chamar P/Invoke pós-`_thread.Start()` para set affinity.
- `src/Relay/MpscQueueSink.cs` + packet — idem.
- `src/Relay/Internal/ThreadAffinity.cs` (NOVO) — P/Invoke wrapper:
  - Windows: `SetThreadAffinityMask(GetCurrentThread(), 1UL << cpu)` 
  - Linux: `pthread_setaffinity_np` via `sched_setaffinity` syscall
  - Fail-soft: se P/Invoke falha (e.g. CPU não existe), log warning silencioso (no logging dep) ou retorna false; ctor continua. Affinity é best-effort.

**Backward-compat:** mudança de default `BelowNormal → Normal` é potencial regressão pra usuários que dependem disso (improvável). Documentar em commit + breaking-changes/release-notes.

### Benchmark — design

**Novo benchmark necessário** (não existe equivalente atual):
- `QueueSinkLatencyBenchmark` em `benchmarks/Relay.Benchmarks/` — mede **per-message latency** end-to-end, capturando p50/p99/p999 manualmente (não via BDN's Mean):
  - Producer thread publica N=1M items, cada um carimba `HfClock.NowTicks` ANTES de `TryPublish`.
  - Consumer thread, em `WriteToBackend`, lê o ticks do item e calcula delta vs `HfClock.NowTicks` atual.
  - Acumula em histograma fixo (1024 buckets log-scale de 100ns a 1ms).
  - Pós-run: extrai p50, p99, p999, max.
  - **3 cenários:** `Default` (BelowNormal, no pin), `NormalPriority` (Normal, no pin), `NormalPinned` (Normal, affinityCpu=2 — P-core fixo).
  - Sustained load: producer roda em loop hot, sem yield, 30s.
  - **`[Trait("Category", "Perf")]`** para que rode só sob demanda.

**Auxiliar — `QueuePipeThroughputBenchmarks`:** rerun com novo default `Normal` priority. Esperado: flat ou +0-2% throughput.

**Gate (PRE-DEFINIDO):**
- `QueueSinkLatencyBenchmark.NormalPinned.p999` ≤ 0.8 × `Default.p999` mediana de 3 runs. (i.e. pin + Normal melhora p999 em ≥20%.)
- `QueueSinkLatencyBenchmark.NormalPriority.p999` ≤ 0.95 × `Default.p999` (Normal sem pin já dá pelo menos 5% — confirma que parte do ganho vem só da priority).
- `QueuePipeThroughputBenchmarks.Push_Batch32` post-fix: dentro de ±5% vs baseline (no regression).

**Disposição se gate falha:**
- p999 não melhora: hipótese sobre migration cost falsa neste ambiente (12700, máquina dedicada). REVERT do default change; manter os parâmetros como **opt-in** (mantém API mas default fica `BelowNormal`/`-1`). Usuário em ambiente compartilhado pode opt-in.
- p999 melhora mas throughput regrede >5%: trade-off ruim. REVERT default, manter opt-in.
- Ambos ok: MERGE com novo default.

---

## §2.4 — `WaitOnAddress` backoff

### Hipótese técnica
**Mecanismo atual:** `ConsumeLoop` quando ring vazio: 10× `SpinWait(20)` → 5× `Thread.Yield()` → `Thread.Sleep(1)`. No Windows com timer resolution default = 15.6ms, `Sleep(1)` pode dormir até 15.6ms. Mesmo com `timeBeginPeriod(1)` (1ms granularity), mínimo é ~1ms.

Em traffic **steady-state** (ring sempre tem items), backoff nunca aciona — sem mudança. Em traffic **sparse-bursty** (gap >20×SpinWait + 5×Yield = ~10µs), consumer dorme. Próximo item após o dormiar fica esperando até o sleep expirar = 1-15ms latency.

**Fix:** Substituir `Sleep(1)` por `WaitOnAddress(&_publishedCount, expected, sizeof(int), timeout=1ms)`:
- Consumer dorme em condição sobre uma palavra (e.g. ring's tail counter)
- Producer em `TryPublish` chama `WakeByAddressSingle(&_publishedCount)` após advance
- Wake é µs-level (~5-10µs latency end-to-end), não ms

Linux: `futex(FUTEX_WAIT_PRIVATE)` + `futex(FUTEX_WAKE_PRIVATE)`. Mesma semântica.

**Magnitude esperada:**
- Sparse-burst (1 item/5ms): p99/p999 latency cai de 1-15ms para 5-50µs. **Order of magnitude.**
- Steady-state: zero change (consumer não dorme).
- Risco específico: producer-side wake cost. `WakeByAddressSingle` é ~30-50c quando ninguém espera, ~µs quando alguém. Em 10M items/sec, +30c/item = 0.3 ns/item — mensurável.
  - **Mitigação:** producer só chama wake quando flag `_consumerSleeping` (atomic int) é set. Adiciona 1 atomic load no producer hot path (~3-5c). Trade-off favorável se sparse-burst é cenário real.

**Por que pode falhar / risco:**
- Em high-throughput steady-state, o atomic check pelo producer pode regredir throughput (~5c × items/sec).
- WaitOnAddress só está disponível em Win8+ — já é o caso, .NET 9 só roda em Win10+.
- Em Linux, `futex` exige `pthread_cond_t` wrapper ou raw syscall (raw é melhor para evitar libc overhead).
- Complexidade: estado do consumer (sleeping/running) precisa ser observável atomicamente pelo producer.

### Implementação

**Files alterados:**
- `src/Relay/Internal/ParkingLot.cs` (NOVO) — abstração platform-specific:
  - Windows: `[DllImport("kernel32")] WaitOnAddress`, `WakeByAddressSingle`, `WakeByAddressAll`
  - Linux: `[DllImport("libc")] sys_futex` via syscall (avoiding pthread_cond_t)
  - API: `Park(ref int address, int expected, int timeoutMs)`, `Unpark(ref int address)`
  - Fail-soft: se P/Invoke não disponível, fallback para `Thread.Sleep(timeoutMs)` (preserva comportamento atual)
- `src/Relay/SpscQueueSink.cs` + packet — adicionar `_consumerSleeping` flag (`int`, 0=running, 1=sleeping). Alterar `ConsumeLoop` backoff:
  ```csharp
  // após spin + yield exaustos:
  Interlocked.Exchange(ref _consumerSleeping, 1);
  if (TryConsume...) { Interlocked.Exchange(ref _consumerSleeping, 0); /* process */ }
  else { ParkingLot.Park(ref _publishedCount, lastSeen, 1); Interlocked.Exchange(ref _consumerSleeping, 0); }
  ```
- Em `_ring.TryPublish` ou na chamada de Enqueue após publish:
  ```csharp
  Volatile.Write(ref _publishedCount, _publishedCount + 1);
  if (Volatile.Read(ref _consumerSleeping) == 1) ParkingLot.Unpark(ref _publishedCount);
  ```
- `src/Relay/MpscQueueSink.cs` + packet — idem.

**API impacto:** zero — comportamento externo idêntico, só latency interna muda.

### Benchmark — design

**Novo benchmark necessário:**
- `SparseBurstLatencyBenchmark` em `benchmarks/Relay.Benchmarks/`:
  - 3 padrões de tráfego:
    - `SparseBurst_1Per5ms` (200 items/sec, isolated)
    - `MicroBurst_10Items_Per100ms` (100 items/sec in bursts)
    - `Steady_1M_PerSec` (steady, para detectar regressão de throughput)
  - Per-message latency histogram (mesmo design do `QueueSinkLatencyBenchmark` em §2.3).
  - 2 cenários: `WithSleepBackoff` (current), `WithParkingLot` (post-fix).
  - **`[Trait("Category", "Perf")]`**

**Gate (PRE-DEFINIDO):**
- `SparseBurst_1Per5ms.p99 (WithParkingLot)` ≤ 100µs. (`WithSleepBackoff` typically 1-15ms — ≥10× improvement minimum.)
- `Steady_1M_PerSec.p99 (WithParkingLot)` ≤ 1.05× `WithSleepBackoff.p99` (no regression — ≤5% slowdown acceptable to pay for sparse-burst win).
- `QueuePipeThroughputBenchmarks.Push_Single (post-fix)` median dentro de ±3% baseline.
- Tests pass.

**Disposição se gate falha:**
- Sparse-burst p99 não cai abaixo de 100µs: ParkingLot impl bug ou consumer não dorme realmente. Investigar antes de decidir.
- Steady-state regrede >5%: producer-side wake cost dominante. Tentar `_consumerSleeping` flag check otimizado (e.g. `Volatile.Read` em vez de `Interlocked`). Se ainda regrede, REVERT.
- Ambos ok: MERGE.

### Complexidade

Esta é a task mais complexa do plano (~2-3 dias). Razões:
- P/Invoke cross-platform com fallback
- Estado de sleep observável atomicamente
- Edge cases: producer publica DURANTE consumer ir-dormir (ABA-ish)
- Spurious wakes (futex)

**Sugestão de subdivisão:**
- §2.4.a — implementar `ParkingLot` standalone com tests de unidade (park-unpark, timeout, spurious wake)
- §2.4.b — integrar em `SpscQueueSink<T>` + packet (mais simples, single consumer)
- §2.4.c — integrar em `MpscQueueSink<T>` + packet (cuidado com múltiplos producers chamando wake)
- §2.4.d — rodar gates e decidir

---

## §3.3 — `HttpBatchSink` pooled allocations

### Hipótese técnica
**Mecanismo:** Em cada flush, 3 allocations:
- `batch.ToArray()` — copia bytes do buffer interno para novo `byte[]`
- `new ByteArrayContent(buffer)` — wrapper allocation
- `new HttpRequestMessage(...)` — request wrapper allocation

Em flush rate alto (>10/sec), gera Gen0 pressure mensurável. Não afeta `Enqueue` (que apenas appende ao buffer interno). Só afeta consumer thread.

**Fix:**
- `ArrayPool<byte>.Shared` para o buffer da batch
- Custom `PooledByteArrayContent : ByteArrayContent` que retorna o buffer ao pool em `Dispose`
- Reutilizar `HttpRequestMessage` (criar 1 vez, mudar `Content` por flush) — mas atenção: `HttpRequestMessage` não é reusável após Send (HttpClient throws). Alternativa: aceitar 1 alloc por flush.

**Magnitude esperada:** 
- Gen0 collections/sec: -80-95% sob flush rate alto
- Per-flush latency: -10-30µs em Gen0 stall avoidance (microscópico, mas existe em servidores carregados)

**Por que pode falhar / risco:**
- Em flush rate baixo (<1/sec), nada muda. Otimização inútil.
- `PooledByteArrayContent` exige cuidado com lifetime — HTTP stack pode segurar reference após Send retornar (e.g. retry, telemetry middleware). Retornar buffer ao pool prematuramente = bug.
- `HttpRequestMessage` non-reusable lockstep: 1 alloc/flush inevitável a menos que paginate o request manualmente.

### Implementação

**Files alterados:**
- `src/Relay.Sinks.Http/HttpBatchSink.cs` — método Flush:
  - Substituir `var buffer = batch.ToArray()` por `var buffer = ArrayPool<byte>.Shared.Rent(batchSize)` + `batch.CopyTo(buffer)`.
  - Substituir `using var content = new ByteArrayContent(buffer)` por `using var content = new PooledByteArrayContent(buffer, batchSize)`.
  - Manter `new HttpRequestMessage` (não otimizar — retry/middleware risk).
- `src/Relay.Sinks.Http/PooledByteArrayContent.cs` (NOVO) — derive de `HttpContent`, override `SerializeToStreamAsync`, devolve buffer no `Dispose`. ~30 LOC.

**Test risk:** middleware que retém content após Dispose pode crashar. Documentar limitation: "use only with default HttpClient, no DelegatingHandler that captures Content".

### Benchmark — design

**Existing:** Nenhum benchmark de `HttpBatchSink` na suíte atual. Precisa criar.

**Novo benchmark:**
- `HttpBatchSinkAllocBenchmark` em `benchmarks/Relay.Benchmarks/Sinks/`:
  - Mock HttpClient via custom HttpMessageHandler que retorna 200 sem network I/O (zero network noise).
  - Flush 1000 batches de 16KB cada.
  - **`[MemoryDiagnoser]`** para Gen0/Gen1/Gen2 counts.
  - Methods: `Flush_Current` (com ToArray), `Flush_Pooled` (com ArrayPool).
  - `[Trait("Category", "Perf")]`

**Gate (PRE-DEFINIDO):**
- `Flush_Pooled.Gen0/op` ≤ 0.3 × `Flush_Current.Gen0/op` (i.e. ≥70% redução).
- `Flush_Pooled.Mean` dentro de ±10% `Flush_Current.Mean` (latência idêntica ou melhor, não pior).
- Tests pass + add specific test: send 100 batches, verify all bytes received correctly at the mock server (no buffer-recycle-after-send bug).

**Disposição se gate falha:**
- Gen0 não reduz: `ArrayPool` retorna mesmo buffer ou bug em `PooledByteArrayContent`. Investigar.
- Latência regrede >10%: ArrayPool contention ou IL trampoline overhead. REVERT.
- Tests fail with corrupted bytes: lifetime bug. REVERT, escalate.
- Tudo ok: MERGE.

**Alternativa se gate falha:** §3.3 é OPCIONAL no plano original. Se gate falha, abandonar sem rerun e documentar finding. Não é correctness.

---

## Disciplina BDN comum a todas as 4 tasks

Para evitar repetir o noise de §2.1 (stddev 30%+ que dominou o sinal):

### Job config
Todos os benchmarks novos usam:
```csharp
[SimpleJob(launchCount: 1, warmupCount: 10, iterationCount: 20)]
[MemoryDiagnoser]
[HideColumns("Job", "IterationCount", "WarmupCount")]
```
20 iterations vs default 15 dá CI mais apertado. `[HideColumns]` para legibilidade do relatório.

### Ambiente quiescent
Antes de qualquer baseline ou gate run:
- Fechar IDEs, browsers, etc.
- Plug-in (não bateria) — Intel SpeedStep / thermal throttling muda resultados.
- `powercfg /setactive SCHEME_MIN` (high performance) durante o run.
- Não rodar `dotnet test` ou subagents em paralelo.

### Comparação
Sempre **mediana** + IC 99.9% (Error column do BDN). Considerar significativo apenas se IC's não sobreporem.

### Output
Cada PR perf-related no body inclui tabela `Method | Pre Mean | Post Mean | Pre Median | Post Median | Delta% | CI overlap?`.

---

## Sequência de execução

Ordem por risco crescente + dependências:

1. **§2.2 batch hook** — menor mudança de API, hipótese mais bem isolada via `BatchHookBenchmark`. Bom primeiro candidato para validar disciplina.
2. **§2.3 affinity** — médio. Precisa novo benchmark de latency histogram + decisão sobre default.
3. **§2.4 WaitOnAddress** — maior. Subdividir em §2.4.a/b/c/d. Subsequente a §2.3 pois compartilha o histogram benchmark scaffolding.
4. **§3.3 HttpBatchSink** — opcional. Só executar se §2.2 e §2.3 passaram com folga (sinal de que a disciplina está funcionando) E houver tempo.

---

## Verificação consolidada

Por task, ao final:
- Pre/post BDN arquivado em `benchmarks/artifacts/2026-05-24-<task-slug>/`
- Gate pass/fail documentado no PR body com tabela
- Test gate verde: `dotnet test Relay.sln -c Release --filter "Category!=Endurance&Category!=Stress&Category!=Perf"`
- Per-Perf testes rodados explicitamente quando aplicável
- Develop atualizada via fast-forward por task (manter linear)

Ao final do plano (todas as 4 tasks decididas, merged ou rejected):
- `bench/260524-baseline-pre-audit` merged em develop OU mantida como referência (decidir no fim — agora preserva ↔ depois disso, perde valor sem outras releases)
- Push de develop para origin/develop, se autorizado
- Sumário de findings em commit `chore: post-audit triage report` (opcional)

---

## Critérios de parada

A qualquer ponto:
- 3 das 4 tasks falsificadas por gate → revisitar a hipótese geral do audit (pode ser que `audit v4` já tenha esgotado a margem em hot path)
- Test gate quebra e não recupera em <30min → BLOCKED, escalate
- Context budget próximo do limite → encerrar sessão com summary, retomar em nova
