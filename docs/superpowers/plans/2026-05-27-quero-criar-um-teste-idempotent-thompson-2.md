# Plano: Throughput Perf Tests + Warmup nos Stress Tests Existentes

## Context

Os stress tests atuais em `Relay.Tests/Circular/` sofrem de dois problemas:
1. **Sem warmup explícito**: os primeiros 1–2 snapshots são contaminados por JIT compilation e escalonamento inicial de threads, distorcendo min/avg/stddev.
2. **Janela curta**: 5–10 snapshots não permitem stddev confiável nem detecção de GC drift ao longo do tempo.

BenchmarkDotNet **não é adequado** para ring topology: BDN mede latência de batch (startup + inject + drain), enquanto a métrica relevante aqui é throughput sustentado em steady-state (msg/s). Usar BDN incluiria overhead de criação de threads nos números.

Solução em duas partes:
- **Parte 1** — Adicionar warmup + aumentar duração nos stress tests existentes.
- **Parte 2** — Criar `CircularThroughputPerfTests.cs` (`[Trait("Category","Perf")]`) com protocolo de warmup longo + 30s de medição.

## Parte 1 — Ampliar Stress Tests Existentes

### Arquivos modificados

| Arquivo | Testes stress afetados |
|---|---|
| `PureSinkRingTests.cs` | `Ring3_Packet64_Infinite_5s_Throughput` → `_30s_` |
| | `Ring5_Packet128_Infinite_5s_Throughput` → `_30s_` |
| | `Ring8_Packet256_Infinite_10s_Throughput` → `_30s_` |
| | `Ring13_Packet320_Infinite_10s_Throughput` → `_30s_` |
| `BackendSinkRingTests.cs` | `Ring3_FileStreamSink_Packet64_Infinite_5s_Throughput` → `_30s_` |
| `SaturationTests.cs` | `Ring3_Packet64_SmallBuffer_5s_SaturationRateMeasured` → `_30s_` |
| `ReceiverSinkRingTests.cs` | `Ring3_SharedMemorySpscSink_Packet_Infinite_5s_Throughput` → `_30s_` |

### Protocolo atual → novo

```csharp
// ANTES
report.Start();
for (int s = 0; s < snapshots; s++)   // snapshots = 5 ou 10
{
    Thread.Sleep(1_000);
    report.Record(ring.TotalCount());
}

// DEPOIS
// Warmup: 5s sem gravação — JIT compila, ring atinge steady-state
Thread.Sleep(5_000);

const int snapshots = 30;
report.Start();
for (int s = 0; s < snapshots; s++)   // 30s de medição
{
    Thread.Sleep(1_000);
    report.Record(ring.TotalCount());
}
```

- Warmup: `Thread.Sleep(5_000)` antes de `report.Start()` (não grava snapshots)
- Snapshots: 5 ou 10 → **30** (30s de medição)
- `RingTestReport` já suporta `maxSnapshots` (default 120), não precisa mudar
- Rename de método: `_5s_` ou `_10s_` → `_30s_` (reflete duração real da janela de medição)
- `Stop(drainMs: 2_000)` permanece

### Testes finitos (commit gate): inalterados

Os testes sem `[Trait("Category","Stress")]` não são modificados.

---

## Parte 2 — CircularThroughputPerfTests.cs (novo)

**Arquivo:** `tests/Relay.Tests/Circular/CircularThroughputPerfTests.cs`

### Protocolo (mais rigoroso que stress)

```csharp
// 10s de warmup (duplo do stress)
Thread.Sleep(10_000);

const int snapshots = 30;
report.Start();
for (int s = 0; s < snapshots; s++)
{
    Thread.Sleep(1_000);
    report.Record(ring.TotalCount());
}
ring.Stop(drainMs: 2_000);
report.Stop();
report.Print(label, ring.TotalCount());
ring.TotalCount().Should().BeGreaterThan(0);
```

### Configurações (7 testes, ~6 min cada → ~42 min total)

**NodeCount scaling (Packet64, RingCap=8192, seed=512):**
| Teste | Nodes |
|---|---|
| Ring3_Packet64_Perf_30s_ThroughputSteadyState | 3 |
| Ring5_Packet64_Perf_30s_ThroughputSteadyState | 5 |
| Ring8_Packet64_Perf_30s_ThroughputSteadyState | 8 |
| Ring13_Packet64_Perf_30s_ThroughputSteadyState | 13 |

**PayloadSize scaling (Ring-3, RingCap=8192, seed=512):**
| Teste | Payload |
|---|---|
| Ring3_Packet128_Perf_30s_ThroughputSteadyState | 128B |
| Ring3_Packet256_Perf_30s_ThroughputSteadyState | 256B |
| Ring3_Packet320_Perf_30s_ThroughputSteadyState | 320B |

Todos usam `InfiniteRingTopology<T>` (entry `MpscRingNode<T>`) e `DecrementHops=false`.

---

## Infraestrutura reutilizada (sem modificações)

| Componente | Arquivo |
|---|---|
| `InfiniteRingTopology<T>` | `Helpers/RingTopology.cs` |
| `RingNodeConfig` | mesmo arquivo |
| `RingTestReport` | `Helpers/RingTestReport.cs` |
| `Packet64/128/256/320` | `Helpers/CircularPayloads.cs` |

---

## Revisão

- Implementação: **Sonnet** (tasks independentes, padrão claro)
- Revisão final de todo código gerado: **Opus**

---

## Verificação

```powershell
# Commit gate — deve continuar passando (≤60s):
dotnet test tests/Relay.Tests -c Release --filter "Category!=Endurance&Category!=Stress&Category!=Perf"

# Smoke: 1 stress test com novo protocolo (~36s):
dotnet test tests/Relay.Tests -c Release --filter "FullyQualifiedName~Ring3_Packet64_Infinite_30s"

# Todos os Perf tests (~42 min):
dotnet test tests/Relay.Tests -c Release --filter "Category=Perf" -v normal
```
