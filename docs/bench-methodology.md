---
title: "Relay — Benchmark Methodology"
type: bench-methodology
solution: Relay
created: 2026-06-11
---

# Relay — Benchmark Methodology

> Per-project methodology consumed by the `bench-report` skill. Prose is for humans; the machine
> block below drives verdict thresholds and the regression gate. Relay is **ultra-low-latency,
> zero-alloc** — ring-buffer, MPSC, and packet-sink hot paths measured at ns scale.

## Setup
| Parameter | Value |
|-----------|-------|
| Tool | BenchmarkDotNet (see `Relay.Benchmarks.csproj`) |
| Runtime | net9.0, Release |
| Diagnosers | `[MemoryDiagnoser]` on every benchmark (zero-alloc invariant) |

## Suites (all `micro`, ns scale, no external comparison)
ByteRingBuffer · ByteEnqueue · Enqueue · MultiEnqueue · MPSC · MpscByte · MpscByteContention ·
MpscContention · MpscSlotLayout · RingBuffer · FilterSink · PropagateBench · QueuePipeThroughput ·
PacketSinks (Chain · File · Filter · MpscPacket · MultiPacket · QueueSink · TcpSink) ·
Sinks (Batch · Mmf · NamedPipe · RamPacket · RotatingFile · SharedMemory · Unix · Udp) ·
Receivers (SharedMemorySpsc · Udp) · Baselines (TypedSinkBaseline) · Internal (QueueSinkLatency).
All are the project's own code → all gated.

## How to Run
```powershell
dotnet run -c Release --project benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -- --filter "*"
# then archive:
python ~/.claude/hooks/bench-history-cli.py ingest --repo . --tag micro --gc Server
```

## Noise Floor
**TODO — calibrate.** Run the suite 3× on the target machine and record the run-to-run variance (ns).
Set `noiseFloorNs` (machine block) to the observed indistinguishable band so sub-floor deltas are
treated as noise. Until calibrated it stays `10` (typical ultra-low-latency floor — tighten after
calibration).

## Decision rules
A delta is a regression when it is beyond `noiseSigma`·σ AND exceeds `regressionPct` (and, once set,
beyond `noiseFloorNs`). Any new allocation where a bench was 0-alloc is a hard regression.

## Interpreting Results
Focus on **Mean** and **Allocated**. `[Benchmark(Baseline = true)]` sets the in-suite Ratio reference.

## Known Limitations

`[MemoryDiagnoser]` only sees managed-heap allocations made inside the measured invocation. It cannot
detect `NativeMemory` over-allocation: `NativeMemory.Alloc`/`AllocZeroed`/`AlignedAlloc` bypass the GC
heap entirely, and every Relay ring/sink benchmark allocates its native buffer once in `[GlobalSetup]`
— outside the measured window, not per-invocation. A regression like the Wave `(elementCount,
elementSize)` argument-order bug (`docs/architecture-decisions.md`, 2026-08-20 entry) would report
0 B allocated and produce no BDN signal at all. This is why native-allocation footprint coverage lives
in `Relay.Tests` (`tests\Relay.Tests\Memory\NativeAllocationSizeTests.cs`,
`AllocationAccountingTests.cs`), not the benchmark suite.

## Pre-regression Gate
`bench-report gate` fails CI when a gated bench regresses beyond `gateMaxRegression` (statistically
significant changes only). The thresholds below are **conservative starting values** — tighten after
calibration.

## Machine knobs

The block below is parsed by the `bench-report` tooling. Edit the values; keep the sentinel comments
and the ```json fence intact.

<!-- bench-methodology:begin -->
```json
{
  "schemaVersion": 1,
  "version": 1,
  "defaults": { "regressionPct": 0.05, "noiseSigma": 2.0, "noiseFloorNs": 10, "gateMaxRegression": 0.05, "gated": true },
  "perTag": {
    "micro": { "run": "dotnet run -c Release --project benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -- --filter \"*\"" }
  }
}
```
<!-- bench-methodology:end -->

<!-- doc-links:auto -->
## Related

- [Bench history](reports/bench-history/bench-history.md)
