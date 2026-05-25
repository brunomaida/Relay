# Relay v1.0.2 — 2026-05-25

Patch release. Corretude em sinks de pacote e sinks concretos; opt-in de affinity/prioridade de thread no consumer; remoção de `DateTime.UtcNow` de métodos de produção em `RotatingFileSink`.

---

## Added

- **`SpscQueueSink<T>` / `MpscQueueSink<T>`**: parâmetros opcionais `threadPriority` e `affinityCpu` no construtor — controle de prioridade e pinning de CPU do consumer thread. Default: `BelowNormal`, sem pinning.
- **`RotatingFileSink`**: parâmetro opcional `fileNameFormat` — padrões de nome de arquivo customizados (`{0}` = prefix, `{1}` = date, `{2}` = seq).

## Fixed

- **`RotatingFileSink`**: injeção de `Func<DateTime>` elimina `DateTime.UtcNow` de todos os métodos de produção. `ShouldRotate` predicate (hot path) não alterado.
- **`RotatingFileSink`**: ancoragem do dia a UTC absoluta na rotação — previne drift em uptimes longos.
- **`SinkConstraints`**: `AssertCacheLineAligned<T>()` ativado em Release (era só DEBUG).
- **`SharedMemorySink`**: ordering SPSC corrigido (`Volatile.Write` faltante antes do copy no consumer).
- **`TcpSink`**: guarda bypass path contra conexão unhealthy-but-open.
- **Packet sinks** (`TcpSink`, `NamedPipeSink`): loop `Send/Write` para corretude em partial-send.
- **Fixed-buffer sinks**: guarda payload > capacidade do buffer.
- **`SinkChain.Packet` builder**: `MpscQueueSink.Prev` não era wired — recovery drain nunca disparava em chains MPSC packet.

## Perf

Nenhuma regressão. `RotatingFileSink.ShouldRotate` predicate: **13.59 ns** (baseline 13.76 ns). Audit: `docs/reports/2026-05-25-hot-path-audit-relay.md`.

---

## Packages

| Package | Description |
|---------|-------------|
| `Relay` | Core pipeline — typed + packet sinks, builders, ring buffers, native memory |
| `Relay.Sinks.Http` | `HttpBatchSink` — HTTP POST with circuit breaker |
| `Relay.Sinks.Observability` | `SeqSink` — CLEF-over-HTTP to Seq |

**Runtime:** .NET 9 · **Language:** C# 13 · **Zero production dependencies** (core `Relay` package)
