# Changelog

Todas as alterações relevantes do projeto Relay são documentadas aqui.  
Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/).

---

## [Unreleased]

### fix: `RotatingFileSink.ShouldRotate` — remove `DateTime.UtcNow.Date` from consumer hot path

`RotatingFileSink.ShouldRotate` no longer calls `DateTime.UtcNow.Date` per consumed payload;
caches the next UTC-midnight boundary in `HfClock` ticks. Reduces consumer hot-path cost from
~50c to ~3c per payload (BDN ratio gate ≥ 10x — see Phase 1 BDN under
`benchmarks/artifacts/2026-04-29-phase1/`). Resolves regression flagged in
`docs/reports/2026-04-29-resource-cost-map-relay.md` §5.

**Files touched:** `src/Relay/Sinks/RotatingFileSink.cs`,
`tests/Relay.Tests/Sinks/RotatingFileSinkTests.cs`,
`benchmarks/Relay.Benchmarks/Sinks/RotatingFileSinkBenchmarks.cs`.

---

### refactor!: renomeia `TeePipe`→`ForkPipe`, `FanOutPipe`→`MultiPipe`, `FanOut2Pipe`→`Multi2Pipe` + builder expandido

**Breaking.** Renomeações públicas em `namespace Relay` e nova superfície fluente em
`Relay.Builder`.

**Renames:**
- `TeePipe<T>` → `ForkPipe<T>` (`src/Relay/TeePipe.cs` → `ForkPipe.cs`)
- `FanOutPipe<T>` → `MultiPipe<T>`, `FanOut2Pipe<T,TC1,TC2>` → `Multi2Pipe<T,TC1,TC2>`
  (`src/Relay/FanOutPipe.cs` → `MultiPipe.cs`)
- Testes: `TeePipeTests` → `ForkPipeTests`, `FanOutPipeTests` → `MultiPipeTests`,
  `Examples/TeeAuditMpscSmoke` → `Examples/ForkAuditMpscSmoke`
- Benchmarks: `FanOutEnqueueBenchmarks` / `FanOutIsHealthyBenchmarks` → `MultiEnqueueBenchmarks` /
  `MultiIsHealthyBenchmarks`; `Depth2_Propagate_Tee` / `Depth2_Tee_Wrapped` → `_Fork_`

**Builder novo (`Relay.Builder`):**
- `RelayBuilder.StartSpsc<T,THead>(head)` — constraint `THead : SpscQueuePipe<T>`, didático.
- `RelayBuilder.StartMpsc<T,THead>(head)` — constraint `THead : MpscQueuePipe<T>`, didático.
- `PipeChain<T,THead>.Fork(primary)` — insere `ForkPipe<T>` no tail; propaga após accept.
- `PipeChain<T,THead>.When(pred).To(downstream)` — gate condicional via `FilterBinding<T,THead>`.
- `PipeChain<T,THead>.Multi(cfg)` — sub-builder `MultiBuilder<T>`; coleta branches, emite
  `MultiPipe<T>`. Cada branch pode ter sub-chain própria (fallback por ramo).
- `PipeChain<T,THead>.Multi<TC1,TC2>(c1,c2)` — overload sealed-CRTP → `Multi2Pipe`.
- `PipeChain.To(next)` agora também wira `Prev` para `MpscQueuePipe<T>` (antes: só SPSC) —
  alinhado com o contrato `TryDrainToPrev` documentado em `CLAUDE.md`.

**Rationale:**
- `Tee` → `Fork`: separação sem ambiguidade de redundância (tee sugeria apenas side-effect Unix).
- `FanOut` → `Multi(plex)`: termo descreve broadcast N-way em linguagem de domínio sem sugerir
  redundância. Variante curta `Multi` prevaleceu sobre `Multiplex`.
- Builder fluente cobre fallback/fork/filter/multi sem obrigar construção manual de pipes
  intermediários. Cada operador mapeia 1:1 a tipo existente — nada novo em runtime.

**Files touched:** `src/Relay/{ForkPipe,MultiPipe}.cs`, `src/Relay/Builder/*`,
`src/Relay/Buffers/SpscRingBuffer.cs` (comentário), `tests/Relay.Tests/{ForkPipeTests,
MultiPipeTests,Examples/ForkAuditMpscSmoke}.cs`, `benchmarks/Relay.Benchmarks/{MultiBenchmarks,
PropagateBenchmarks}.cs`, `CLAUDE.md`, `README.md`, `docs/topology.md`.

---

### perf: Otimizações hot-path nos SPSC ring buffers

**Performance impact:** `TryPublish`/`TryConsume` fast path ~25c → ~8-12c. Spin idle path ~60-80c/iter → ~25-40c/iter.

**What changed:**
- Remote index caching em `SpscRingBuffer<T>` e `SpscByteRingBuffer`: produtor mantém
  `_cachedHead`, consumidor mantém `_cachedTail` como snapshots `PaddedLong` (128B padded).
  `Volatile.Read` cross-core ocorre apenas quando o buffer aparenta cheio/vazio — caminho
  feliz vira load L1-owned, eliminando tráfego MESIF entre cores.
- Bounds check elimination em ambos os ring buffers: `_buffer[idx & _mask]` substituído por
  `Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(...))`. Em `SpscByteRingBuffer`, headers
  migrados de `BinaryPrimitives` para `Unsafe.WriteUnaligned`/`Unsafe.ReadUnaligned`; spans de
  payload construídos via `MemoryMarshal.CreateSpan`/`CreateReadOnlySpan` — elimina bounds
  checks em `AsSpan(pos, 4)` e `AsSpan(pos + HeaderSize, len)`. `System.Buffers.Binary` removido.
- Flush deadline throttling em `SpscQueuePipe<T>` e `SpscByteQueuePipe`: `HfClock.NowTicks`
  (QPC, ~25c + LFENCE stall) antes verificado em toda iteração do loop; agora: sempre após
  batch ativo, a cada 8 iterações no spin phase (máscara `0x7`), sempre em yield/sleep.
- `docs/topology.md` atualizado: protocolo SPSC, memory layout, performance reference e
  descrição do consumer loop refletem os novos invariantes.

**Files touched:** `src/Relay/Buffers/SpscRingBuffer.cs`,
`src/Relay/Buffers/SpscByteRingBuffer.cs`, `src/Relay/SpscQueuePipe.cs`,
`src/Relay/SpscByteQueuePipe.cs`, `docs/topology.md`

---

### docs: BDN report — byte hierarchy vs typed baseline (`210b200`)

Relatório BDN comparando `SpscByteRingBuffer` e `SpscByteQueuePipe` contra a baseline tipada
(`SpscRingBuffer<T>`). Cobre `TryPeek_Empty`, `RoundTrip`, `TryPublish_Full` e profundidades
de chain 1/2/3 com payload fixo de 64B.

**Files touched:** `docs/reports/2026-04-23-bdn-byte-vs-typed.md`

---

### docs: byte-pipe hierarchy no CLAUDE.md (`34024b9`, `ffd4bbd`)

Documentação da hierarquia BytePipe adicionada ao CLAUDE.md: `SpscByteRingBuffer` (sentinel
padding marker, alinhamento 4B), quando usar cada árvore, e itens diferidos (MPSC /
PropagateAfterAccept / builder). Correção de typo subsequente.

**Files touched:** `CLAUDE.md`

---

### test: benchmarks byte hierarchy A/B vs typed (`7314426`)

- `ByteRingBufferBenchmarks`: `TryPeek_Empty`, `RoundTrip`, `TryPublish_Full` com sweep de
  `Capacity × PayloadSize`.
- `ByteEnqueueBenchmarks`: Depth1/2/3 com payload 64B para comparação direta contra
  `EnqueueBenchmarks` da hierarquia tipada.

**Files touched:** `benchmarks/Relay.Benchmarks/ByteRingBufferBenchmarks.cs`,
`benchmarks/Relay.Benchmarks/ByteEnqueueBenchmarks.cs`

---

### test: suite xUnit para hierarquia byte-pipe (`0353184`)

47 testes cobrindo `SpscByteRingBuffer`, `SpscByteQueuePipe` e `BytePipe` chain:
construtor/validação, round-trip, wrap/padding marker, ring cheio/fallback,
lifecycle (Start/Stop/Dispose), crash exposure via `ConsumerException`,
`TryDrainToPrev` recovery, `Flush` propagation e fidelidade byte-a-byte.

**Files touched:** `tests/Relay.Tests/SpscByteRingBufferTests.cs`,
`tests/Relay.Tests/SpscByteQueuePipeTests.cs`, `tests/Relay.Tests/BytePipeChainTests.cs`

---

### feat: hierarquia BytePipe com SPSC byte ring (`2e2e223`)

Árvore paralela a `DispatchPipe<T>` para payloads `ReadOnlySpan<byte>` de comprimento variável:

- `SpscByteRingBuffer`: ring lock-free length-prefixed. Cada record = `[uint32 length LE]
  [payload, 4-byte aligned]`. Wrap tratado via padding marker (`0xFFFFFFFF`) — payload sempre
  contíguo, sem scratch buffer. Zero-copy peek via `TryPeek(out ReadOnlySpan<byte>, out int)`.
- `SpscByteQueuePipe`: base abstrata com consumer thread, flush cadence e `TryDrainToPrev`,
  espelhando `SpscQueuePipe<T>`.
- `NullBytePipe`: sink terminal no-op, singleton.

**Files touched:** `src/Relay/Buffers/SpscByteRingBuffer.cs`, `src/Relay/BytePipe.cs`,
`src/Relay/SpscByteQueuePipe.cs`, `src/Relay/NullBytePipe.cs`

---

### refactor: PipeConstraints relaxado para múltiplos positivos de 64B (`c7bcf3b`)

`PipeConstraints.AssertCacheLineAligned<T>()` antes exigia potência de 2; agora aceita qualquer
múltiplo positivo de 64 bytes (64, 128, 192, 256 …). Adicionados testes unitários cobrindo
todos os casos de borda. README e CLAUDE.md atualizados para refletir o invariante correto.

**Files touched:** `src/Relay/Internal/PipeConstraints.cs`,
`tests/Relay.Tests/PipeConstraintsTests.cs`, `CLAUDE.md`, `README.md`

---

### docs: hot-path audit reports e comparação BDN vs análise estática (`107ee28`)

- `docs/topology.md`: topologia completa do sistema — type hierarchy, assembly graph, enqueue
  hot path, chain topologies, fanout/filter routing rules, consumer thread, recovery drain,
  builder assembly, protocolo SPSC, memory layout, concrete pipes, threading model,
  performance reference e topologias recomendadas.
- `docs/reports/2026-04-23-hot-path-audit-relay.md`: auditoria hot-path v1 (27 dimensões).
- `docs/reports/2026-04-23-hot-path-audit-relay-v2.md`: auditoria revisada com gap H27
  (ausência de benchmarks no CI).
- `docs/reports/2026-04-23-bdn-vs-static-analysis.md`: resultados BDN medidos vs estimativas
  de análise estática.
- `docs/reports/2026-04-23-resource-cost-map-relay.md`: mapa de custo de recursos por path tier.

**Files touched:** `docs/topology.md`, `docs/reports/` (4 arquivos)

---

### fix: correções de threading e lifecycle em ring buffer e concrete pipes (`bcf72d2`)

- `SpscRingBuffer<T>`: adicionada batched-write API (`TryReserveTail`/`WriteSlot`/`CommitTail`)
  para publicação atômica de múltiplos slots com único mfence.
- `SpscQueuePipe<T>`: revisão do contract de `IsHealthy` (ring full não é falha permanente);
  separação dos gates `IsHealthy` e `Accept` na documentação.
- `FileStreamPipe<T>`: correção de condition race no path de recuperação.
- `MmfPipe<T>`: `TryRecoverBackend` explicitado como no-op (capacidade é permanente).
- `RamPipe<T>`: `DrainTo` corrigido para checar `IsHealthy` do target em cada item.
- Testes de regressão adicionados em `RecoveryDrainTests` e `SpscQueuePipeTests`.

**Files touched:** `src/Relay/Buffers/SpscRingBuffer.cs`, `src/Relay/SpscQueuePipe.cs`,
`src/Relay/Pipes/FileStreamPipe.cs`, `src/Relay/Pipes/MmfPipe.cs`,
`src/Relay/Pipes/RamPipe.cs`, `tests/Relay.Tests/RecoveryDrainTests.cs`,
`tests/Relay.Tests/SpscQueuePipeTests.cs`

---

### test: correção de DCE e InvocationCount nos benchmarks BDN (`c7c18e8`)

Benchmarks com `InvocationCount=1` e ausência de consumo do valor de retorno causavam dead-code
elimination pelo JIT, zerando as medições. Corrigido com sink de resultado e ajuste de
`IterationCount`/`WarmupCount` para workloads de alocação vs throughput.

**Files touched:** `benchmarks/Relay.Benchmarks/EnqueueBenchmarks.cs`,
`benchmarks/Relay.Benchmarks/FanOutBenchmarks.cs`,
`benchmarks/Relay.Benchmarks/FilterPipeBenchmarks.cs`,
`benchmarks/Relay.Benchmarks/RingBufferBenchmarks.cs`

---

### feat: suite de benchmarks BDN (`dd659a0`)

Suite BenchmarkDotNet cobrindo o hot path completo:

- `RingBufferBenchmarks`: `TryConsume_Empty` (baseline), `RoundTrip`, `TryPublish_Full`
  com sweep de capacidade (64 → 4KB L1, 1024 → 64KB L2, 65536 → 4MB L3 spill).
- `EnqueueBenchmarks`: `Depth1_Healthy`, `Depth2_Healthy`, `Depth2_P1Unhealthy`,
  `Depth3_P1P2Unhealthy` — custo real de cada hop na chain.
- `FanOutBenchmarks`: FanOut array-based vs CRTP `FanOut2Pipe` (devirtualização JIT).
- `FilterPipeBenchmarks`: predicate pass vs block, medindo custo do gate condicional.

**Files touched:** `Relay.sln`, `benchmarks/Relay.Benchmarks/` (projeto completo)

---

### chore: CLAUDE.md otimizado — remoção de invariantes redundantes (`6a4b96c`)

Seção de invariantes duplicados removida do CLAUDE.md; informações consolidadas nas seções
de performance e design invariants existentes.

**Files touched:** `CLAUDE.md`

---

### docs: README com exemplos e casos de uso (`3cd84e2`)

README completo: visão geral, quick start, topologias de chain (T1–T7), semântica de fallback,
concrete pipes, byte-pipe hierarchy, builder API e ciclo de vida.

**Files touched:** `README.md`

---

### feat: commit inicial — biblioteca Relay (`67be819`)

Implementação inicial completa:

- `DispatchPipe<T>`: base abstrata com `Enqueue` hot path (`IsHealthy && Accept || Next?.Enqueue`),
  fallback chain e contrato de zero alocação.
- `SpscQueuePipe<T>`: base abstrata com SPSC ring lock-free (`SpscRingBuffer<T>`, 128B padded
  head/tail), consumer thread dedicado, flush cadence, recovery drain via `TryDrainToPrev`.
- Concrete pipes: `FileStreamPipe<T>` (POH write buffer, backoff 1s→60s),
  `TcpPipe<T>` (POH send buffer, backoff 1s→30s), `MmfPipe<T>` (capacity-only failure),
  `RamPipe<T>` (NativeMemory circular ring, drain-on-recovery).
- `FanOutPipe<T>` e `FanOut2Pipe<T,TC1,TC2>`: broadcast para N filhos; CRTP 2-child com
  devirtualização JIT (~6c vs array-based).
- `FilterPipe<T>`: gate condicional; items filtrados consumidos silenciosamente (não propagam
  ao fallback chain).
- `NullPipe<T>`: sink terminal singleton.
- `RelayBuilder` / `PipeChain<T,THead>`: fluent builder com wire automático de `Next` e `Prev`.
- `RelayMemory`: pre-fault + `VirtualLock` do ring buffer no `Start()`.
- `HfClock`: wrapper `Stopwatch.GetTimestamp()`.
- `PipeConstraints`: assert cache-line alignment em DEBUG.
- Suite de testes: `DispatchPipeChainTests`, `FanOutPipeTests`, `RecoveryDrainTests`,
  `SpscQueuePipeTests` — 25 arquivos, 1714 linhas.

**Files touched:** 25 arquivos — `src/Relay/` completo + `tests/Relay.Tests/` completo
