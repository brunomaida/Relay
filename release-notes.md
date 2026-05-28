# Relay v1.0.4 — 2026-05-28

Test coverage release. Circular ring topology stress tests, steady-state throughput benchmarks, and JIT warmup harness for MPSC perf measurements.

---

## Added

- **Circular ring topology tests** (`PureSinkRingTests`, `BackendSinkRingTests`, `SaturationTests`, `ReceiverSinkRingTests`) — multi-hop ring scenarios exercising SPSC/MPSC chains under 30s stress + 5s warmup windows.
- **`CircularThroughputPerfTests`** — steady-state throughput benchmarks measuring sustained enqueue rate across circular topologies.
- **`MpscThroughputHarness`** — JIT warmup run added before MPSC perf measurements to eliminate first-run noise from throughput numbers.
- **`RingTopology`, `RingNode`, `RingTestReport`** — shared test infrastructure helpers for ring construction, node wiring, and telemetry reporting.

---

## Packages

| Package | Description |
|---------|-------------|
| `Relay` | Core pipeline — typed + packet sinks, receivers, builders, ring buffers, native memory |
| `Relay.Sinks.Http` | `HttpBatchSink` — HTTP POST with circuit breaker |
| `Relay.Sinks.Observability` | `SeqSink` — CLEF-over-HTTP to Seq |

**Runtime:** .NET 9 · **Language:** C# 13 · **Zero production dependencies** (core `Relay` package)
