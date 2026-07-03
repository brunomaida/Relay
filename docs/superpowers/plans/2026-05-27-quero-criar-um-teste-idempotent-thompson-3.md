# Status: Testes 30s em Sinks, Chains e Circular

## Pergunta

"Temos testes de Sinks completos, encadeados e circulares, com durações de 30s também?"

---

## Resposta direta: Não

| Categoria | 30s Warmup + Snapshot | Observação |
|---|---|---|
| **Circular** (ring topology) | ✅ Stress 5s+30s / Perf 10s+30s | Concluído na sessão anterior |
| **Ring buffers (raw)** | ❌ Sem warmup | `MpscThroughputHarness`, `SpscByteRingBufferTests` stress — correctness ou throughput cru sem protocolo |
| **Sinks individuais** | ❌ Sem 30s | Todos correctness-gate (finitos) |
| **Chains encadeadas** | ❌ Sem 30s | Todos correctness-gate (finitos) |

---

## Análise dos testes non-Circular

### Stress de correctness (não precisam de 30s)

Estes testes medem **ausência de corrupção/perda**, não throughput. O timeout de 30s é um teto, não uma janela de medição. JIT warmup não distorce resultados de correctness.

| Arquivo | Testes Stress | Propósito |
|---|---|---|
| `MpscByteRingBufferTests.cs` | 100K records, 4 threads | No-loss, no-corruption |
| `MpscRingBufferTests.cs` | 8 producers × 50K | No-corruption sob contention |
| `SpscByteRingBufferTests.cs` | 1M records | No-loss single producer |
| `SharedMemorySinkTests.cs` | 20K frames 4KB | No partial-frame race |

**Veredicto:** Corretos como estão. Não requerem warmup.

### MpscThroughputHarness (gaps reais)

`tests/Relay.Tests/MpscThroughputHarness.cs` — 2 testes `[Trait("Category","Perf")]`:
- `Typed_MpscRingBuffer_Throughput_Sweep_1_2_4_8_Producers`
- `Byte_MpscRingBuffer_Throughput_Sweep_1_2_4_8_Producers`

**Problemas:**
1. **Sem warmup** — primeira rodada inclui JIT compilation
2. **Sem RingTestReport** — métricas brutas (items/elapsed), sem stddev, min/avg/max, GC tracking
3. Mede throughput do ring buffer cru (sem circular re-injection) — método de medição diferente dos Circular perf tests é adequado, mas a ausência de warmup não é

**Gap:** O protocolo de warmup não foi aplicado aqui.

---

## Plano: Corrigir MpscThroughputHarness

### Mudança mínima — `MpscThroughputHarness.cs`

Adicionar uma rodada de warmup antes do sweep principal em cada teste:

```csharp
// ANTES — sweep direto, primeira rodada = JIT
foreach (int producerCount in new[] { 1, 2, 4, 8 })
{
    // ... measure run
}

// DEPOIS — warmup 1 run, depois sweep
// Warmup: JIT-compile the hot path with 1 producer, discard result
RunOnce(producerCount: 1, items: 200_000, discard: true);

foreach (int producerCount in new[] { 1, 2, 4, 8 })
{
    // ... measure run (same as before)
}
```

- Sem RingTestReport — o harness usa `Stopwatch` direto (adequado para "items/s por producer count", que é uma medida batch, não steady-state interval)
- Apenas acrescentar 1 rodada de warmup antes do loop — cirúrgico

### Arquivos modificados

| Arquivo | Mudança |
|---|---|
| `tests/Relay.Tests/MpscThroughputHarness.cs` | Warmup run antes do sweep em ambos os testes |

### O que NÃO fazer

- **Não criar throughput tests para chains lineares** — chains lineares esgotam items; não há steady-state natural. Escopo futuro se demonstrado por profiling.
- **Não adicionar RingTestReport ao harness** — o modelo batch (inject-all-drain) do harness é diferente do modelo interval dos Circular tests; misturar seria incorreto.
- **Não alterar testes de correctness** — propósito diferente, não requerem warmup.

---

## Verificação

```powershell
# Commit gate — deve continuar passando:
dotnet test tests/Relay.Tests -c Release --filter "Category!=Endurance&Category!=Stress&Category!=Perf"

# Smoke — 1 perf test com warmup:
dotnet test tests/Relay.Tests -c Release --filter "FullyQualifiedName~Typed_MpscRingBuffer_Throughput" -v normal
```
