# Benchmark History

_generated 2026-06-12T13:07:29Z · env `c15f4501` · 12th Gen Intel Core i7-12700 · .NET 9.0.14 (9.0.1426.11910) · 12 cores · BDN 0.13.12_

_Methodology: `docs/bench-methodology.md` (v1)_

> Derived file — **do not edit by hand**. Regenerate with `/bench-report ingest`.
> Full interactive view: [`bench-history.html`](bench-history.html)

## 2026-06-12

### Relay.Benchmarks.Sinks.UnixSocketSinkBenchmarks — 13:06:32Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.UnixSocketSinkBenchmarks.Push_Single(ItemCount: 100000)` | 3.68 ms | 50.93 µs | 3.73 ms | 136.01 KB | new |
| `Relay.Benchmarks.Sinks.UnixSocketSinkBenchmarks.Push_Single(ItemCount: 1000000)` | 18.88 ms | 2.40 ms | 21.24 ms | 135.98 KB | new |

### Relay.Benchmarks.Sinks.UdpSinkBenchmarks — 13:06:18Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.UdpSinkBenchmarks.Push_Single(ItemCount: 100000)` | 4.33 ms | 124.10 µs | 4.45 ms | 65.67 KB | new |
| `Relay.Benchmarks.Sinks.UdpSinkBenchmarks.Push_Single(ItemCount: 1000000)` | 10.17 ms | 173.10 µs | 10.29 ms | 65.68 KB | new |

### Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks — 13:06:06Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks.Accept_Single(PayloadSize: 64)` | 7.23 ns | 0.06 ns | 7.28 ns | 0 | new |
| `Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks.Accept_Single(PayloadSize: 256)` | 10.35 ns | 0.20 ns | 10.55 ns | 0 | new |

### Relay.Benchmarks.Sinks.RotatingFileSinkBenchmarks — 13:05:48Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.RotatingFileSinkBenchmarks.ShouldRotate_Predicate` | 0.25 ns | 0.03 ns | 0.28 ns | 0 | new |
| `Relay.Benchmarks.Sinks.RotatingFileSinkBenchmarks.ShouldRotate_HotPath` | 24.31 ns | 6.08 ns | 30.30 ns | 0 | new |

### Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks — 13:05:29Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks.Accept_Single(PayloadSize: 64)` | 4.51 ns | 0.02 ns | 4.53 ns | 0 | new |
| `Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks.Accept_Single(PayloadSize: 256)` | 5.02 ns | 0.02 ns | 5.03 ns | 0 | new |

### Relay.Benchmarks.Sinks.NamedPipeSinkBenchmarks — 13:05:03Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.NamedPipeSinkBenchmarks.Push_Single(ItemCount: 10000)` | 548.78 µs | 13.88 µs | 558.06 µs | 134.57 KB | new |
| `Relay.Benchmarks.Sinks.NamedPipeSinkBenchmarks.Push_Single(ItemCount: 100000)` | 1.20 ms | 36.19 µs | 1.23 ms | 134.57 KB | new |

### Relay.Benchmarks.Sinks.MmfSinkBenchmarks — 13:04:51Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.MmfSinkBenchmarks.Push_Single(ItemCount: 100000)` | 7.28 ms | 70.15 µs | 7.35 ms | 18.38 KB | new |
| `Relay.Benchmarks.Sinks.MmfSinkBenchmarks.Push_Single(ItemCount: 1000000)` | 11.28 ms | 322.32 µs | 11.59 ms | 18.38 KB | new |

### Relay.Benchmarks.Sinks.BatchSinkBenchmarks — 13:04:50Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.BatchSinkBenchmarks.Enqueue_RingPublish` | 13.64 ns | 1.39 ns | 14.91 ns | 0 | new |

### Relay.Benchmarks.Receivers.UdpReceiverBenchmark — 13:04:37Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Receivers.UdpReceiverBenchmark.Poll_Empty` | 882.95 ns | 4.55 ns | 887.99 ns | 0 | new |
| `Relay.Benchmarks.Receivers.UdpReceiverBenchmark.Roundtrip_PerFrame` | 5.07 µs | 32.93 ns | 5.11 µs | 72 B | new |

### Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark — 13:04:19Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Poll_Empty(PayloadSize: 64)` | 0.65 ns | 0.03 ns | 0.68 ns | 0 | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Roundtrip_PerFrame(PayloadSize: 64)` | 19.65 ns | 0.27 ns | 19.89 ns | 0 | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Poll_Empty(PayloadSize: 256)` | 1.19 ns | 0.03 ns | 1.21 ns | 0 | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Roundtrip_PerFrame(PayloadSize: 256)` | 23.54 ns | 0.65 ns | 24.37 ns | 0 | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Poll_Empty(PayloadSize: 1024)` | 0.63 ns | 0.02 ns | 0.65 ns | 0 | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Roundtrip_PerFrame(PayloadSize: 1024)` | 41.06 ns | 0.36 ns | 41.29 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.TcpSinkBenchmark — 13:02:55Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.TcpSinkBenchmark.TcpSink_Enqueue_128B` | 9.83 ns | 1.43 ns | 10.95 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks — 13:02:47Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single(ItemCount: 100000)` | 5.07 ms | 978.19 µs | 6.03 ms | 65.07 KB | new |
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 100000)` | 2.62 ms | 129.08 µs | 2.74 ms | 65.07 KB | new |
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single(ItemCount: 1000000)` | 15.10 ms | 911.43 µs | 15.98 ms | 65.07 KB | new |
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 1000000)` | 8.04 ms | 137.14 µs | 8.12 ms | 65.07 KB | new |

### Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks — 13:02:22Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth1_Healthy_Default` | 0.22 ns | 0.03 ns | 0.24 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth1_Healthy_Propagate_NoNext` | 0.26 ns | 0.04 ns | 0.30 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth2_Propagate_Fork` | 5.11 ns | 0.12 ns | 5.23 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth2_Fork_Wrapped` | 4.48 ns | 0.07 ns | 4.56 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.MultiPacketIsHealthyBenchmarks — 13:01:03Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.MultiPacketIsHealthyBenchmarks.Multi_Packet_IsHealthy` | 0.41 ns | 0.02 ns | 0.43 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks — 13:00:46Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks.Multi_Packet_Enqueue` | 3.07 ns | 0.05 ns | 3.10 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks.Multi2_Packet_Enqueue` | 4.19 ns | 0.11 ns | 4.29 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks — 13:00:23Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single(ItemCount: 100000)` | 7.65 ms | 1.50 ms | 8.57 ms | 64.97 KB | new |
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 100000)` | 2.53 ms | 23.18 µs | 2.55 ms | 64.97 KB | new |
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single(ItemCount: 1000000)` | 16.52 ms | 222.11 µs | 16.66 ms | 64.97 KB | new |
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 1000000)` | 7.75 ms | 152.09 µs | 7.88 ms | 64.97 KB | new |

### Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks — 12:59:57Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks.Filter_Packet_Pass` | 6.34 ns | 0.05 ns | 6.36 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks.Filter_Packet_Reject` | 0.25 ns | 0.01 ns | 0.25 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.FileSinkBenchmark — 12:59:19Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.FileSinkBenchmark.FileSink_Enqueue_128B` | 7.02 ns | 0.04 ns | 7.05 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.ChainBenchmark — 12:59:13Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.ChainBenchmark.TcpSink_NoPropagation` | 13.29 ns | 0.16 ns | 13.47 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.ChainBenchmark.FanOut_2Sinks` | 13.97 ns | 0.32 ns | 14.33 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.ChainBenchmark.SerializeSink_Overhead` | 5.90 ns | 0.12 ns | 6.03 ns | 0 | new |

### Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark — 12:58:42Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: Default)` | 93.76 ms | 625.58 µs | 94.52 ms | 17.32 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: Default)` | 94.98 ms | 1.79 ms | 96.72 ms | 17.32 KB | ≈ +1.3% |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: NormalPriority)` | 93.94 ms | 914.23 µs | 94.77 ms | 17.32 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: NormalPriority)` | 96.26 ms | 1.13 ms | 97.08 ms | 17.32 KB | ≈ +2.5% |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: NormalPinned)` | 97.10 ms | 788.60 µs | 97.81 ms | 17.32 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: NormalPinned)` | 95.15 ms | 567.65 µs | 95.50 ms | 17.32 KB | ≈ -2.0% |

### Relay.Benchmarks.Baselines.TypedSinkBaselineBenchmark — 12:58:35Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Baselines.TypedSinkBaselineBenchmark.TcpSinkTyped_Enqueue` | 0.33 ns | 0.00 ns | 0.33 ns | 0 | new |

### Relay.Benchmarks.RingBufferBenchmarks — 12:58:28Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty(Capacity: 64)` | 0.21 ns | 0.02 ns | 0.22 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip(Capacity: 64)` | 0.64 ns | 0.02 ns | 0.66 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full(Capacity: 64)` | 0.17 ns | 0.01 ns | 0.18 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32(Capacity: 64)` | 49.74 ns | 0.32 ns | 50.02 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty(Capacity: 1024)` | 0.21 ns | 0.04 ns | 0.25 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip(Capacity: 1024)` | 0.96 ns | 0.01 ns | 0.97 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full(Capacity: 1024)` | 0.18 ns | 0.01 ns | 0.19 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32(Capacity: 1024)` | 52.82 ns | 0.41 ns | 53.19 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty(Capacity: 65536)` | 0.05 ns | 0.01 ns | 0.06 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip(Capacity: 65536)` | 2.25 ns | 0.24 ns | 2.47 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full(Capacity: 65536)` | 0.24 ns | 0.01 ns | 0.24 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32(Capacity: 65536)` | 65.20 ns | 0.34 ns | 65.51 ns | 0 | new |

### Relay.Benchmarks.QueuePipeThroughputBenchmarks — 12:54:17Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single(ItemCount: 100000)` | 1.97 ms | 76.36 µs | 2.02 ms | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32(ItemCount: 100000)` | 1.81 ms | 29.61 µs | 1.84 ms | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 100000)` | 121.80 ms | 1.35 ms | 122.80 ms | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32_SlowBackend(ItemCount: 100000)` | 121.73 ms | 1.91 ms | 123.55 ms | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single(ItemCount: 100000)` | 4.29 ms | 200.51 µs | 4.44 ms | 16.82 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single_SlowBackend(ItemCount: 100000)` | 122.51 ms | 586.09 µs | 122.95 ms | 16.90 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single(ItemCount: 1000000)` | 4.33 ms | 213.52 µs | 4.53 ms | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32(ItemCount: 1000000)` | 3.89 ms | 417.24 µs | 4.30 ms | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 1000000)` | 124.59 ms | 886.45 µs | 125.46 ms | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32_SlowBackend(ItemCount: 1000000)` | 125.80 ms | 2.54 ms | 128.30 ms | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single(ItemCount: 1000000)` | 14.39 ms | 428.13 µs | 14.68 ms | 16.83 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single_SlowBackend(ItemCount: 1000000)` | 124.43 ms | 2.53 ms | 126.22 ms | 16.92 KB | new |

### Relay.Benchmarks.PropagateBenchmarks — 12:52:53Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PropagateBenchmarks.Depth1_Healthy_Default` | 0.25 ns | 0.02 ns | 0.26 ns | 0 | new |
| `Relay.Benchmarks.PropagateBenchmarks.Depth1_Healthy_Propagate_NoNext` | 0.28 ns | 0.04 ns | 0.30 ns | 0 | new |
| `Relay.Benchmarks.PropagateBenchmarks.Depth2_Propagate_Fork` | 0.85 ns | 0.01 ns | 0.86 ns | 0 | new |
| `Relay.Benchmarks.PropagateBenchmarks.Depth2_Fork_Wrapped` | 3.44 ns | 0.01 ns | 3.45 ns | 0 | new |

### Relay.Benchmarks.MultiIsHealthyBenchmarks — 12:51:28Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MultiIsHealthyBenchmarks.Multi_IsHealthy` | 0.43 ns | 0.03 ns | 0.46 ns | 0 | new |
| `Relay.Benchmarks.MultiIsHealthyBenchmarks.Multi2_IsHealthy` | 0.45 ns | 0.03 ns | 0.47 ns | 0 | new |

### Relay.Benchmarks.MultiEnqueueBenchmarks — 12:50:57Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MultiEnqueueBenchmarks.Multi_Enqueue` | 3.25 ns | 0.06 ns | 3.31 ns | 0 | new |
| `Relay.Benchmarks.MultiEnqueueBenchmarks.Multi2_Enqueue` | 3.38 ns | 0.06 ns | 3.43 ns | 0 | new |

### Relay.Benchmarks.MpscSlotLayoutBenchmarks — 12:50:35Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 1, Capacity: 1024)` | 667.50 ms | 29.50 ms | 685.41 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 1, Capacity: 1024)` | 970.12 ms | 577.30 ms | 1.540 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 1, Capacity: 65536)` | 135.40 ms | 46.13 ms | 176.14 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 1, Capacity: 65536)` | 175.28 ms | 14.43 ms | 185.19 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 2, Capacity: 1024)` | 1.556 s | 740.45 ms | 2.286 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 2, Capacity: 1024)` | 1.244 s | 351.87 ms | 1.584 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 2, Capacity: 65536)` | 287.90 ms | 59.87 ms | 346.93 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 2, Capacity: 65536)` | 219.51 ms | 103.52 ms | 321.00 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 4, Capacity: 1024)` | 1.072 s | 452.65 ms | 1.513 s | 88 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 4, Capacity: 1024)` | 1.223 s | 257.42 ms | 1.460 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 4, Capacity: 65536)` | 513.75 ms | 24.26 ms | 534.55 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 4, Capacity: 65536)` | 339.96 ms | 42.80 ms | 379.44 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 8, Capacity: 1024)` | 2.045 s | 135.03 ms | 2.162 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 8, Capacity: 1024)` | 2.441 s | 487.41 ms | 2.922 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 8, Capacity: 65536)` | 967.22 ms | 59.39 ms | 1.022 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 8, Capacity: 65536)` | 626.65 ms | 65.47 ms | 691.25 ms | 424 B | new |

### Relay.Benchmarks.MpscContentionBenchmarks — 12:48:29Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount: 1)` | 201.42 ms | 28.64 ms | 221.91 ms | 464 B | new |
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount: 2)` | 318.32 ms | 58.74 ms | 359.93 ms | 464 B | new |
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount: 4)` | 302.39 ms | 89.73 ms | 385.42 ms | 464 B | new |
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount: 8)` | 646.53 ms | 109.51 ms | 716.50 ms | 464 B | new |

### Relay.Benchmarks.MpscByteRingBufferBenchmarks — 12:48:15Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 64, PayloadSize: 8)` | 3.32 ns | 0.03 ns | 3.35 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 64, PayloadSize: 8)` | 7.09 ns | 0.03 ns | 7.12 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 64, PayloadSize: 8)` | 0.82 ns | 0.03 ns | 0.84 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 64, PayloadSize: 8)` | 0.25 ns | 0.01 ns | 0.26 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 64, PayloadSize: 64)` | 0.84 ns | 0.03 ns | 0.85 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 64, PayloadSize: 64)` | 1.44 ns | 0.03 ns | 1.47 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 64, PayloadSize: 64)` | 0.63 ns | 0.01 ns | 0.64 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 64, PayloadSize: 64)` | 0.21 ns | 0.01 ns | 0.21 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 64, PayloadSize: 256)` | 0.82 ns | 0.02 ns | 0.85 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 64, PayloadSize: 256)` | 1.39 ns | 0.02 ns | 1.41 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 64, PayloadSize: 256)` | 0.59 ns | 0.01 ns | 0.60 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 64, PayloadSize: 256)` | 0.23 ns | 0.02 ns | 0.24 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 64, PayloadSize: 1024)` | 0.88 ns | 0.05 ns | 0.91 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 64, PayloadSize: 1024)` | 1.38 ns | 0.06 ns | 1.44 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 64, PayloadSize: 1024)` | 0.62 ns | 0.01 ns | 0.63 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 64, PayloadSize: 1024)` | 0.23 ns | 0.04 ns | 0.27 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 1024, PayloadSize: 8)` | 3.83 ns | 0.04 ns | 3.87 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 1024, PayloadSize: 8)` | 6.83 ns | 0.02 ns | 6.84 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 1024, PayloadSize: 8)` | 0.81 ns | 0.03 ns | 0.83 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 1024, PayloadSize: 8)` | 0.22 ns | 0.03 ns | 0.24 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 1024, PayloadSize: 64)` | 3.52 ns | 0.03 ns | 3.55 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 1024, PayloadSize: 64)` | 7.28 ns | 0.07 ns | 7.35 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 1024, PayloadSize: 64)` | 0.87 ns | 0.02 ns | 0.88 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 1024, PayloadSize: 64)` | 0.20 ns | 0.01 ns | 0.22 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 1024, PayloadSize: 256)` | 5.19 ns | 0.03 ns | 5.22 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 1024, PayloadSize: 256)` | 8.43 ns | 0.15 ns | 8.53 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 1024, PayloadSize: 256)` | 0.85 ns | 0.02 ns | 0.86 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 1024, PayloadSize: 256)` | 0.23 ns | 0.03 ns | 0.26 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 1024, PayloadSize: 1024)` | 0.94 ns | 0.05 ns | 0.98 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 1024, PayloadSize: 1024)` | 1.50 ns | 0.11 ns | 1.60 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 1024, PayloadSize: 1024)` | 0.64 ns | 0.04 ns | 0.67 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 1024, PayloadSize: 1024)` | 0.23 ns | 0.02 ns | 0.25 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 65536, PayloadSize: 8)` | 3.37 ns | 0.05 ns | 3.42 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 65536, PayloadSize: 8)` | 7.15 ns | 0.03 ns | 7.18 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 65536, PayloadSize: 8)` | 0.77 ns | 0.01 ns | 0.78 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 65536, PayloadSize: 8)` | 0.20 ns | 0.01 ns | 0.21 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 65536, PayloadSize: 64)` | 3.51 ns | 0.00 ns | 3.51 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 65536, PayloadSize: 64)` | 7.31 ns | 0.20 ns | 7.51 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 65536, PayloadSize: 64)` | 0.84 ns | 0.01 ns | 0.85 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 65536, PayloadSize: 64)` | 0.18 ns | 0.02 ns | 0.19 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 65536, PayloadSize: 256)` | 6.10 ns | 0.02 ns | 6.12 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 65536, PayloadSize: 256)` | 8.93 ns | 0.17 ns | 9.09 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 65536, PayloadSize: 256)` | 0.87 ns | 0.02 ns | 0.88 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 65536, PayloadSize: 256)` | 0.24 ns | 0.01 ns | 0.25 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 65536, PayloadSize: 1024)` | 14.79 ns | 0.07 ns | 14.86 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 65536, PayloadSize: 1024)` | 17.80 ns | 0.08 ns | 17.86 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 65536, PayloadSize: 1024)` | 0.82 ns | 0.02 ns | 0.84 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 65536, PayloadSize: 1024)` | 0.25 ns | 0.01 ns | 0.26 ns | 0 | new |

### Relay.Benchmarks.MpscByteContentionBenchmarks — 12:35:04Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount: 1)` | 522.63 ms | 173.00 ms | 664.86 ms | 464 B | new |
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount: 2)` | 796.04 ms | 239.83 ms | 969.92 ms | 464 B | new |
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount: 4)` | 998.90 ms | 271.01 ms | 1.189 s | 464 B | new |
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount: 8)` | 1.574 s | 426.39 ms | 1.960 s | 128 B | new |

### Relay.Benchmarks.MpscBenchmarks — 12:34:31Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline(Capacity: 64)` | 0.96 ns | 0.01 ns | 0.97 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention(Capacity: 64)` | 6.65 ns | 0.05 ns | 6.68 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full(Capacity: 64)` | 0.21 ns | 0.03 ns | 0.23 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty(Capacity: 64)` | 0.24 ns | 0.04 ns | 0.27 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline(Capacity: 1024)` | 1.24 ns | 0.04 ns | 1.28 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention(Capacity: 1024)` | 6.89 ns | 0.03 ns | 6.91 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full(Capacity: 1024)` | 0.25 ns | 0.02 ns | 0.26 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty(Capacity: 1024)` | 0.25 ns | 0.02 ns | 0.27 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline(Capacity: 65536)` | 1.69 ns | 0.09 ns | 1.78 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention(Capacity: 65536)` | 7.57 ns | 0.03 ns | 7.60 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full(Capacity: 65536)` | 0.21 ns | 0.00 ns | 0.21 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty(Capacity: 65536)` | 0.25 ns | 0.02 ns | 0.27 ns | 0 | new |

### Relay.Benchmarks.FilterSinkBenchmarks — 12:30:28Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.FilterSinkBenchmarks.Filter_Pass` | 2.41 ns | 0.02 ns | 2.42 ns | 0 | new |
| `Relay.Benchmarks.FilterSinkBenchmarks.Filter_Reject` | 0.23 ns | 0.03 ns | 0.26 ns | 0 | new |

### Relay.Benchmarks.EnqueueBenchmarks — 12:29:50Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.EnqueueBenchmarks.Depth1_Healthy` | 0.22 ns | 0.04 ns | 0.25 ns | 0 | new |
| `Relay.Benchmarks.EnqueueBenchmarks.Depth2_AcceptReject` | 0.65 ns | 0.02 ns | 0.67 ns | 0 | new |
| `Relay.Benchmarks.EnqueueBenchmarks.Depth2_HeadUnhealthy` | 1.45 ns | 0.03 ns | 1.47 ns | 0 | new |
| `Relay.Benchmarks.EnqueueBenchmarks.Depth3_AllUnhealthy` | 1.74 ns | 0.03 ns | 1.76 ns | 0 | new |

### Relay.Benchmarks.ByteRingBufferBenchmarks — 12:28:29Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 64, PayloadSize: 8)` | 0.01 ns | 0.01 ns | 0.01 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 64, PayloadSize: 8)` | 3.26 ns | 0.04 ns | 3.29 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 64, PayloadSize: 8)` | 0.66 ns | 0.05 ns | 0.70 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 64, PayloadSize: 64)` | 0.03 ns | 0.01 ns | 0.04 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 64, PayloadSize: 64)` | 0.94 ns | 0.01 ns | 0.95 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 64, PayloadSize: 64)` | 0.43 ns | 0.02 ns | 0.45 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 64, PayloadSize: 256)` | 0.00 ns | 0.00 ns | 0.00 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 64, PayloadSize: 256)` | 1.00 ns | 0.04 ns | 1.03 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 64, PayloadSize: 256)` | 0.45 ns | 0.03 ns | 0.48 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 64, PayloadSize: 1024)` | 0.02 ns | 0.02 ns | 0.04 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 64, PayloadSize: 1024)` | 0.90 ns | 0.05 ns | 0.94 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 64, PayloadSize: 1024)` | 0.63 ns | 0.04 ns | 0.65 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 1024, PayloadSize: 8)` | 0.01 ns | 0.00 ns | 0.01 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 1024, PayloadSize: 8)` | 3.42 ns | 0.10 ns | 3.51 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 1024, PayloadSize: 8)` | 0.71 ns | 0.02 ns | 0.73 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 1024, PayloadSize: 64)` | 0.01 ns | 0.01 ns | 0.01 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 1024, PayloadSize: 64)` | 3.33 ns | 0.06 ns | 3.39 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 1024, PayloadSize: 64)` | 0.66 ns | 0.01 ns | 0.66 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 1024, PayloadSize: 256)` | 0.01 ns | 0.01 ns | 0.02 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 1024, PayloadSize: 256)` | 5.45 ns | 0.03 ns | 5.48 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 1024, PayloadSize: 256)` | 0.64 ns | 0.01 ns | 0.64 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 1024, PayloadSize: 1024)` | 0.02 ns | 0.02 ns | 0.04 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 1024, PayloadSize: 1024)` | 0.88 ns | 0.02 ns | 0.90 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 1024, PayloadSize: 1024)` | 0.42 ns | 0.01 ns | 0.43 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 65536, PayloadSize: 8)` | 0.02 ns | 0.00 ns | 0.02 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 65536, PayloadSize: 8)` | 3.36 ns | 0.02 ns | 3.38 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 65536, PayloadSize: 8)` | 0.66 ns | 0.02 ns | 0.68 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 65536, PayloadSize: 64)` | 0.04 ns | 0.01 ns | 0.05 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 65536, PayloadSize: 64)` | 3.70 ns | 0.02 ns | 3.72 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 65536, PayloadSize: 64)` | 0.65 ns | 0.01 ns | 0.66 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 65536, PayloadSize: 256)` | 0.00 ns | 0.01 ns | 0.01 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 65536, PayloadSize: 256)` | 5.94 ns | 0.05 ns | 5.99 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 65536, PayloadSize: 256)` | 0.64 ns | 0.00 ns | 0.64 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 65536, PayloadSize: 1024)` | 0.04 ns | 0.03 ns | 0.07 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 65536, PayloadSize: 1024)` | 14.60 ns | 0.25 ns | 14.76 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 65536, PayloadSize: 1024)` | 0.62 ns | 0.05 ns | 0.67 ns | 0 | new |

### Relay.Benchmarks.ByteEnqueueBenchmarks — 12:16:56Z  ·  _micro · env c15f4501 · eb15567 · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Healthy` | 0.25 ns | 0.05 ns | 0.30 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth2_Byte_AcceptReject` | 3.53 ns | 0.19 ns | 3.71 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth2_Byte_HeadUnhealthy` | 1.48 ns | 0.01 ns | 1.48 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth3_Byte_AllUnhealthy` | 3.10 ns | 0.04 ns | 3.14 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_TryEnqueue_Healthy` | 0.46 ns | 0.02 ns | 0.47 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_TryEnqueue_Reject` | 0.25 ns | 0.01 ns | 0.26 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Drop_NextNull_Unhealthy` | 3.85 ns | 0.02 ns | 3.87 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Drop_NextNull_Reject` | 3.93 ns | 0.10 ns | 4.02 ns | 0 | new |
