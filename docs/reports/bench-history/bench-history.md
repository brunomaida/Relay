# Benchmark History

_generated 2026-06-11T18:26:42Z · env `3417dcbf` · 12th Gen Intel Core i7-12700 · .NET 9.0.14 · 12 cores · BDN 0.13.12_

_Methodology: `docs/bench-methodology.md` (v1)_

> Derived file — **do not edit by hand**. Regenerate with `/bench-report ingest`.
> Full interactive view: [`bench-history.html`](bench-history.html)

## 2026-06-11

### Relay.Benchmarks.Sinks.RotatingFileSinkBenchmarks — 16:04:34Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.RotatingFileSinkBenchmarks.ShouldRotate_Predicate` | 0.03 ns | 0.01 ns | — | — | new |

## 2026-06-10

### Relay.Benchmarks.Sinks.UnixSocketSinkBenchmarks — 16:08:08Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.UnixSocketSinkBenchmarks.Push_Single(ItemCount=100000)` | 2.74 ms | 178.90 µs | — | 135.81 KB | new |
| `Relay.Benchmarks.Sinks.UnixSocketSinkBenchmarks.Push_Single(ItemCount=1000000)` | 15.97 ms | 524.20 µs | — | 135.83 KB | new |

### Relay.Benchmarks.Sinks.UdpSinkBenchmarks — 16:07:52Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.UdpSinkBenchmarks.Push_Single(ItemCount=100000)` | 4.03 ms | 26.10 µs | — | 65.55 KB | new |
| `Relay.Benchmarks.Sinks.UdpSinkBenchmarks.Push_Single(ItemCount=1000000)` | 10.23 ms | 314.70 µs | — | 65.55 KB | new |

### Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks — 16:07:41Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks.Accept_Single(PayloadSize=64)` | 7.04 ns | 0.12 ns | — | — | new |
| `Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks.Accept_Single(PayloadSize=256)` | 9.72 ns | 0.32 ns | — | — | new |

### Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks — 16:07:12Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks.Accept_Single(PayloadSize=64)` | 4.65 ns | 0.25 ns | — | — | new |
| `Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks.Accept_Single(PayloadSize=256)` | 4.38 ns | 0.04 ns | — | — | new |

### Relay.Benchmarks.Sinks.NamedPipeSinkBenchmarks — 16:06:47Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.NamedPipeSinkBenchmarks.Push_Single(ItemCount=10000)` | 456.70 µs | 18.20 µs | — | 134.46 KB | new |
| `Relay.Benchmarks.Sinks.NamedPipeSinkBenchmarks.Push_Single(ItemCount=100000)` | 1.07 ms | 53.57 µs | — | 134.46 KB | new |

### Relay.Benchmarks.Sinks.BatchSinkBenchmarks — 16:06:37Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.BatchSinkBenchmarks.Enqueue_RingPublish` | 11.32 ns | 1.32 ns | — | — | new |

### Relay.Benchmarks.Sinks.MmfSinkBenchmarks — 16:06:37Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.MmfSinkBenchmarks.Push_Single(ItemCount=100000)` | 6.98 ms | 259.20 µs | — | 18.38 KB | new |
| `Relay.Benchmarks.Sinks.MmfSinkBenchmarks.Push_Single(ItemCount=1000000)` | 11.80 ms | 827.30 µs | — | 18.10 KB | new |

### Relay.Benchmarks.Receivers.UdpReceiverBenchmark — 16:06:26Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Receivers.UdpReceiverBenchmark.Poll_Empty` | 850.00 ns | 4.60 ns | — | — | new |
| `Relay.Benchmarks.Receivers.UdpReceiverBenchmark.Roundtrip_PerFrame` | 4.60 µs | 150.36 ns | — | 72 B | new |

### Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark — 16:06:08Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Poll_Empty(PayloadSize=64)` | 1.20 ns | 0.03 ns | — | — | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Roundtrip_PerFrame(PayloadSize=64)` | 20.20 ns | 0.15 ns | — | — | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Poll_Empty(PayloadSize=256)` | 1.19 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Roundtrip_PerFrame(PayloadSize=256)` | 23.73 ns | 0.29 ns | — | — | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Poll_Empty(PayloadSize=1024)` | 0.96 ns | 0.29 ns | — | — | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Roundtrip_PerFrame(PayloadSize=1024)` | 43.91 ns | 0.06 ns | — | — | new |

### Relay.Benchmarks.PacketSinks.TcpSinkBenchmark — 16:04:44Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.TcpSinkBenchmark.TcpSink_Enqueue_128B` | 7.41 ns | 0.27 ns | — | — | new |

### Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks — 16:04:38Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single(ItemCount=100000)` | 5.03 ms | 593.70 µs | — | 64.95 KB | new |
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single_SlowBackend(ItemCount=100000)` | 2.55 ms | 139.70 µs | — | 64.95 KB | new |
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single(ItemCount=1000000)` | 18.38 ms | 615.00 µs | — | 64.95 KB | new |
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single_SlowBackend(ItemCount=1000000)` | 7.74 ms | 109.20 µs | — | 64.95 KB | new |

### Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks — 16:04:15Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth1_Healthy_Default` | 0.21 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth1_Healthy_Propagate_NoNext` | 0.21 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth2_Propagate_Fork` | 6.45 ns | 0.40 ns | — | — | new |
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth2_Fork_Wrapped` | 3.88 ns | 0.01 ns | — | — | new |

### Relay.Benchmarks.PacketSinks.MultiPacketIsHealthyBenchmarks — 16:02:59Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.MultiPacketIsHealthyBenchmarks.Multi_Packet_IsHealthy` | 0.43 ns | 0.02 ns | — | — | new |

### Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks — 16:02:44Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks.Multi_Packet_Enqueue` | 2.96 ns | 0.04 ns | — | — | new |
| `Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks.Multi2_Packet_Enqueue` | 3.88 ns | 0.04 ns | — | — | new |

### Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks — 16:02:21Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single(ItemCount=100000)` | 9.06 ms | 65.00 µs | — | 64.84 KB | new |
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single_SlowBackend(ItemCount=100000)` | 2.79 ms | 177.70 µs | — | 64.84 KB | new |
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single(ItemCount=1000000)` | 22.12 ms | 1.78 ms | — | 64.85 KB | new |
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single_SlowBackend(ItemCount=1000000)` | 7.91 ms | 11.80 µs | — | 64.85 KB | new |

### Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks — 16:01:52Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks.Filter_Packet_Pass` | 2.56 ns | 0.03 ns | — | — | new |
| `Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks.Filter_Packet_Reject` | 0.21 ns | 0.00 ns | — | — | new |

### Relay.Benchmarks.PacketSinks.FileSinkBenchmark — 16:01:14Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.FileSinkBenchmark.FileSink_Enqueue_128B` | 6.07 ns | 0.06 ns | — | — | new |

### Relay.Benchmarks.PacketSinks.ChainBenchmark — 16:01:04Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.ChainBenchmark.TcpSink_NoPropagation` | 12.71 ns | 0.24 ns | — | — | new |
| `Relay.Benchmarks.PacketSinks.ChainBenchmark.FanOut_2Sinks` | 14.29 ns | 0.78 ns | — | — | new |
| `Relay.Benchmarks.PacketSinks.ChainBenchmark.SerializeSink_Overhead` | 5.34 ns | 0.03 ns | — | — | new |

### Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark — 16:00:34Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Job=Job-KGASBN, IterationCount=5, Scenario=Default)` | 92.98 ms | 199.00 µs | — | 17.32 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Job=ShortRun, IterationCount=3, Scenario=Default)` | 93.79 ms | 146.00 µs | — | 17.32 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Job=Job-KGASBN, IterationCount=5, Scenario=NormalPriority)` | 92.80 ms | 691.00 µs | — | 17.32 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Job=ShortRun, IterationCount=3, Scenario=NormalPriority)` | 92.33 ms | 1.56 ms | — | 17.32 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Job=Job-KGASBN, IterationCount=5, Scenario=NormalPinned)` | 93.70 ms | 629.00 µs | — | 17.04 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Job=ShortRun, IterationCount=3, Scenario=NormalPinned)` | 93.77 ms | 480.00 µs | — | 17.32 KB | new |

### Relay.Benchmarks.Baselines.TypedSinkBaselineBenchmark — 16:00:28Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Baselines.TypedSinkBaselineBenchmark.TcpSinkTyped_Enqueue` | 0.33 ns | 0.00 ns | — | — | new |

### Relay.Benchmarks.RingBufferBenchmarks — 16:00:20Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty(Capacity=64)` | 0.20 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip(Capacity=64)` | 0.86 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full(Capacity=64)` | 0.21 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32(Capacity=64)` | 39.10 ns | 0.07 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty(Capacity=1024)` | 0.22 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip(Capacity=1024)` | 0.96 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full(Capacity=1024)` | 0.19 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32(Capacity=1024)` | 50.49 ns | 0.27 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty(Capacity=65536)` | 0.01 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip(Capacity=65536)` | 1.92 ns | 0.29 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full(Capacity=65536)` | 0.21 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32(Capacity=65536)` | 63.24 ns | 0.23 ns | — | — | new |

### Relay.Benchmarks.QueuePipeThroughputBenchmarks — 15:56:13Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single(ItemCount=100000)` | 1.63 ms | 155.70 µs | — | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32(ItemCount=100000)` | 1.61 ms | 49.30 µs | — | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single_SlowBackend(ItemCount=100000)` | 116.58 ms | 654.90 µs | — | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32_SlowBackend(ItemCount=100000)` | 116.89 ms | 1.40 ms | — | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single(ItemCount=100000)` | 4.18 ms | 624.80 µs | — | 16.82 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single_SlowBackend(ItemCount=100000)` | 119.23 ms | 1.74 ms | — | 16.90 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single(ItemCount=1000000)` | 4.11 ms | 60.70 µs | — | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32(ItemCount=1000000)` | 2.96 ms | 27.90 µs | — | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single_SlowBackend(ItemCount=1000000)` | 117.65 ms | 326.60 µs | — | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32_SlowBackend(ItemCount=1000000)` | 118.18 ms | 1.71 ms | — | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single(ItemCount=1000000)` | 14.56 ms | 228.90 µs | — | 16.82 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single_SlowBackend(ItemCount=1000000)` | 121.01 ms | 3.25 ms | — | 16.90 KB | new |

### Relay.Benchmarks.PropagateBenchmarks — 15:54:56Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PropagateBenchmarks.Depth1_Healthy_Default` | 0.17 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.PropagateBenchmarks.Depth1_Healthy_Propagate_NoNext` | 0.22 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.PropagateBenchmarks.Depth2_Propagate_Fork` | 4.47 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.PropagateBenchmarks.Depth2_Fork_Wrapped` | 3.36 ns | 0.04 ns | — | — | new |

### Relay.Benchmarks.MultiIsHealthyBenchmarks — 15:53:34Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MultiIsHealthyBenchmarks.Multi_IsHealthy` | 0.41 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MultiIsHealthyBenchmarks.Multi2_IsHealthy` | 0.45 ns | 0.00 ns | — | — | new |

### Relay.Benchmarks.MultiEnqueueBenchmarks — 15:53:04Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MultiEnqueueBenchmarks.Multi_Enqueue` | 3.18 ns | 0.04 ns | — | — | new |
| `Relay.Benchmarks.MultiEnqueueBenchmarks.Multi2_Enqueue` | 3.31 ns | 0.04 ns | — | — | new |

### Relay.Benchmarks.MpscSlotLayoutBenchmarks — 15:52:42Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount=1, Capacity=1024)` | 1.116 s | 306.96 ms | — | 88 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount=1, Capacity=1024)` | 884.10 ms | 431.25 ms | — | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount=1, Capacity=65536)` | 198.10 ms | 69.77 ms | — | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount=1, Capacity=65536)` | 213.50 ms | 62.11 ms | — | 88 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount=2, Capacity=1024)` | 831.70 ms | 226.60 ms | — | 88 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount=2, Capacity=1024)` | 663.70 ms | 264.23 ms | — | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount=2, Capacity=65536)` | 417.90 ms | 100.02 ms | — | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount=2, Capacity=65536)` | 275.30 ms | 76.03 ms | — | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount=4, Capacity=1024)` | 1.299 s | 264.90 ms | — | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount=4, Capacity=1024)` | 1.392 s | 111.24 ms | — | 88 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount=4, Capacity=65536)` | 632.20 ms | 92.36 ms | — | 88 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount=4, Capacity=65536)` | 394.70 ms | 109.08 ms | — | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount=8, Capacity=1024)` | 1.397 s | 303.32 ms | — | 88 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount=8, Capacity=1024)` | 2.163 s | 230.56 ms | — | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount=8, Capacity=65536)` | 896.60 ms | 87.97 ms | — | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount=8, Capacity=65536)` | 644.20 ms | 159.05 ms | — | 88 B | new |

### Relay.Benchmarks.MpscContentionBenchmarks — 15:50:39Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount=1)` | 162.30 ms | 121.77 ms | — | 752 B | new |
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount=2)` | 339.20 ms | 32.78 ms | — | 464 B | new |
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount=4)` | 325.40 ms | 38.89 ms | — | 464 B | new |
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount=8)` | 612.70 ms | 58.17 ms | — | 176 B | new |

### Relay.Benchmarks.MpscByteRingBufferBenchmarks — 15:50:24Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=64, PayloadSize=8)` | 3.22 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=64, PayloadSize=8)` | 7.36 ns | 0.18 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=64, PayloadSize=8)` | 0.84 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=64, PayloadSize=8)` | 0.43 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=64, PayloadSize=64)` | 0.89 ns | 0.03 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=64, PayloadSize=64)` | 1.41 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=64, PayloadSize=64)` | 0.62 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=64, PayloadSize=64)` | 0.20 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=64, PayloadSize=256)` | 0.95 ns | 0.22 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=64, PayloadSize=256)` | 1.37 ns | 0.03 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=64, PayloadSize=256)` | 0.61 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=64, PayloadSize=256)` | 0.20 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=64, PayloadSize=1024)` | 0.83 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=64, PayloadSize=1024)` | 1.36 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=64, PayloadSize=1024)` | 0.61 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=64, PayloadSize=1024)` | 0.20 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=1024, PayloadSize=8)` | 3.21 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=1024, PayloadSize=8)` | 7.22 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=1024, PayloadSize=8)` | 0.83 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=1024, PayloadSize=8)` | 0.41 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=1024, PayloadSize=64)` | 3.54 ns | 0.03 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=1024, PayloadSize=64)` | 8.05 ns | 0.04 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=1024, PayloadSize=64)` | 0.82 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=1024, PayloadSize=64)` | 0.40 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=1024, PayloadSize=256)` | 4.99 ns | 0.05 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=1024, PayloadSize=256)` | 9.05 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=1024, PayloadSize=256)` | 0.81 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=1024, PayloadSize=256)` | 0.44 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=1024, PayloadSize=1024)` | 0.85 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=1024, PayloadSize=1024)` | 1.40 ns | 0.05 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=1024, PayloadSize=1024)` | 0.62 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=1024, PayloadSize=1024)` | 0.18 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=65536, PayloadSize=8)` | 3.13 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=65536, PayloadSize=8)` | 7.31 ns | 0.19 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=65536, PayloadSize=8)` | 0.88 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=65536, PayloadSize=8)` | 0.40 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=65536, PayloadSize=64)` | 3.58 ns | 0.06 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=65536, PayloadSize=64)` | 7.96 ns | 0.06 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=65536, PayloadSize=64)` | 0.84 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=65536, PayloadSize=64)` | 0.38 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=65536, PayloadSize=256)` | 5.90 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=65536, PayloadSize=256)` | 9.35 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=65536, PayloadSize=256)` | 0.85 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=65536, PayloadSize=256)` | 0.44 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity=65536, PayloadSize=1024)` | 15.20 ns | 0.29 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity=65536, PayloadSize=1024)` | 19.82 ns | 0.87 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity=65536, PayloadSize=1024)` | 0.79 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity=65536, PayloadSize=1024)` | 0.40 ns | 0.01 ns | — | — | new |

### Relay.Benchmarks.MpscByteContentionBenchmarks — 15:38:31Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount=1)` | 1.959 s | 758.78 ms | — | 464 B | new |
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount=2)` | 563.80 ms | 383.11 ms | — | 464 B | new |
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount=4)` | 1.100 s | 306.65 ms | — | 464 B | new |
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount=8)` | 1.006 s | 60.66 ms | — | 464 B | new |

### Relay.Benchmarks.MpscBenchmarks — 15:37:49Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline(Capacity=64)` | 0.95 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention(Capacity=64)` | 6.35 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full(Capacity=64)` | 0.20 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty(Capacity=64)` | 0.23 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline(Capacity=1024)` | 1.25 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention(Capacity=1024)` | 7.24 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full(Capacity=1024)` | 0.22 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty(Capacity=1024)` | 0.21 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline(Capacity=65536)` | 1.23 ns | 0.04 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention(Capacity=65536)` | 7.07 ns | 0.05 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full(Capacity=65536)` | 0.21 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty(Capacity=65536)` | 0.22 ns | 0.00 ns | — | — | new |

### Relay.Benchmarks.FilterSinkBenchmarks — 15:33:34Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.FilterSinkBenchmarks.Filter_Pass` | 1.08 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.FilterSinkBenchmarks.Filter_Reject` | 0.24 ns | 0.01 ns | — | — | new |

### Relay.Benchmarks.EnqueueBenchmarks — 15:32:49Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.EnqueueBenchmarks.Depth1_Healthy` | 0.20 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.EnqueueBenchmarks.Depth2_AcceptReject` | 1.69 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.EnqueueBenchmarks.Depth2_HeadUnhealthy` | 0.62 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.EnqueueBenchmarks.Depth3_AllUnhealthy` | 1.84 ns | 0.00 ns | — | — | new |

### Relay.Benchmarks.ByteRingBufferBenchmarks — 15:31:25Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=64, PayloadSize=8)` | 0.00 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=64, PayloadSize=8)` | 3.27 ns | 0.05 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=64, PayloadSize=8)` | 0.70 ns | 0.03 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=64, PayloadSize=64)` | 0.01 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=64, PayloadSize=64)` | 0.84 ns | 0.04 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=64, PayloadSize=64)` | 0.39 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=64, PayloadSize=256)` | 0.02 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=64, PayloadSize=256)` | 0.94 ns | 0.03 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=64, PayloadSize=256)` | 0.41 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=64, PayloadSize=1024)` | 0.02 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=64, PayloadSize=1024)` | 0.86 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=64, PayloadSize=1024)` | 0.41 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=1024, PayloadSize=8)` | 0.00 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=1024, PayloadSize=8)` | 3.30 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=1024, PayloadSize=8)` | 0.62 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=1024, PayloadSize=64)` | 0.02 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=1024, PayloadSize=64)` | 3.37 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=1024, PayloadSize=64)` | 0.63 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=1024, PayloadSize=256)` | 0.06 ns | 0.03 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=1024, PayloadSize=256)` | 5.50 ns | 0.06 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=1024, PayloadSize=256)` | 0.61 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=1024, PayloadSize=1024)` | 0.01 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=1024, PayloadSize=1024)` | 1.26 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=1024, PayloadSize=1024)` | 0.40 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=65536, PayloadSize=8)` | 0.01 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=65536, PayloadSize=8)` | 3.22 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=65536, PayloadSize=8)` | 0.65 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=65536, PayloadSize=64)` | 0.00 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=65536, PayloadSize=64)` | 3.69 ns | 0.02 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=65536, PayloadSize=64)` | 0.63 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=65536, PayloadSize=256)` | 0.02 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=65536, PayloadSize=256)` | 6.04 ns | 0.03 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=65536, PayloadSize=256)` | 0.64 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity=65536, PayloadSize=1024)` | 0.03 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity=65536, PayloadSize=1024)` | 14.81 ns | 0.07 ns | — | — | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity=65536, PayloadSize=1024)` | 0.67 ns | 0.00 ns | — | — | new |

### Relay.Benchmarks.ByteEnqueueBenchmarks — 15:20:00Z  ·  _micro · env 3417dcbf · 92a7fc4 · low-confidence · backfill_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Healthy` | 0.20 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth2_Byte_AcceptReject` | 6.09 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth2_Byte_HeadUnhealthy` | 2.54 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth3_Byte_AllUnhealthy` | 3.11 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_TryEnqueue_Healthy` | 0.43 ns | 0.01 ns | — | — | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_TryEnqueue_Reject` | 0.21 ns | 0.00 ns | — | — | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Drop_NextNull_Unhealthy` | 3.77 ns | 0.05 ns | — | — | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Drop_NextNull_Reject` | 4.06 ns | 0.02 ns | — | — | new |
