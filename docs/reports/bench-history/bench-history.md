# Benchmark History

_generated 2026-06-11T19:49:38Z · env `8e045e34` · 12th Gen Intel Core i7-12700 · .NET 9.0.14 (9.0.1426.11910) · 12 cores · GC=Server · BDN 0.13.12_

_Methodology: `docs/bench-methodology.md` (v1)_

> Derived file — **do not edit by hand**. Regenerate with `/bench-report ingest`.
> Full interactive view: [`bench-history.html`](bench-history.html)

## 2026-06-11

### Relay.Benchmarks.Sinks.UnixSocketSinkBenchmarks — 19:49:21Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.UnixSocketSinkBenchmarks.Push_Single(ItemCount: 100000)` | 3.08 ms | 145.63 µs | 3.22 ms | 135.93 KB | new |
| `Relay.Benchmarks.Sinks.UnixSocketSinkBenchmarks.Push_Single(ItemCount: 1000000)` | 20.79 ms | 1.32 ms | 21.64 ms | 135.94 KB | new |

### Relay.Benchmarks.Sinks.UdpSinkBenchmarks — 19:49:07Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.UdpSinkBenchmarks.Push_Single(ItemCount: 100000)` | 4.36 ms | 22.21 µs | 4.38 ms | 65.67 KB | new |
| `Relay.Benchmarks.Sinks.UdpSinkBenchmarks.Push_Single(ItemCount: 1000000)` | 10.75 ms | 116.63 µs | 10.84 ms | 65.67 KB | new |

### Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks — 19:48:55Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks.Accept_Single(PayloadSize: 64)` | 7.26 ns | 0.04 ns | 7.29 ns | 0 | new |
| `Relay.Benchmarks.Sinks.SharedMemorySinkBenchmarks.Accept_Single(PayloadSize: 256)` | 9.92 ns | 0.09 ns | 10.00 ns | 0 | new |

### Relay.Benchmarks.Sinks.RotatingFileSinkBenchmarks — 19:48:36Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.RotatingFileSinkBenchmarks.ShouldRotate_Predicate` | 0.31 ns | 0.03 ns | 0.34 ns | 0 | new |
| `Relay.Benchmarks.Sinks.RotatingFileSinkBenchmarks.ShouldRotate_HotPath` | 69.83 ns | 3.55 ns | 72.63 ns | 0 | new |

### Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks — 19:48:00Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks.Accept_Single(PayloadSize: 64)` | 5.06 ns | 0.15 ns | 5.20 ns | 0 | new |
| `Relay.Benchmarks.Sinks.RamPacketSinkBenchmarks.Accept_Single(PayloadSize: 256)` | 5.14 ns | 0.15 ns | 5.27 ns | 0 | new |

### Relay.Benchmarks.Sinks.NamedPipeSinkBenchmarks — 19:47:34Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.NamedPipeSinkBenchmarks.Push_Single(ItemCount: 10000)` | 663.97 µs | 118.17 µs | 779.65 µs | 134.57 KB | new |
| `Relay.Benchmarks.Sinks.NamedPipeSinkBenchmarks.Push_Single(ItemCount: 100000)` | 1.36 ms | 34.56 µs | 1.39 ms | 134.57 KB | new |

### Relay.Benchmarks.Sinks.MmfSinkBenchmarks — 19:47:21Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.MmfSinkBenchmarks.Push_Single(ItemCount: 100000)` | 23.41 ms | 8.91 ms | 30.07 ms | 18.38 KB | new |
| `Relay.Benchmarks.Sinks.MmfSinkBenchmarks.Push_Single(ItemCount: 1000000)` | 14.81 ms | 4.10 ms | 18.84 ms | 18.38 KB | new |

### Relay.Benchmarks.Sinks.BatchSinkBenchmarks — 19:47:20Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Sinks.BatchSinkBenchmarks.Enqueue_RingPublish` | 20.57 ns | 6.10 ns | 25.95 ns | 0 | new |

### Relay.Benchmarks.Receivers.UdpReceiverBenchmark — 19:47:11Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Receivers.UdpReceiverBenchmark.Poll_Empty` | 934.46 ns | 13.76 ns | 950.94 ns | 0 | new |
| `Relay.Benchmarks.Receivers.UdpReceiverBenchmark.Roundtrip_PerFrame` | 5.48 µs | 477.60 ns | 6.13 µs | 72 B | new |

### Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark — 19:46:52Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Poll_Empty(PayloadSize: 64)` | 1.09 ns | 0.30 ns | 1.25 ns | 0 | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Roundtrip_PerFrame(PayloadSize: 64)` | 20.83 ns | 0.02 ns | 20.86 ns | 0 | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Poll_Empty(PayloadSize: 256)` | 0.67 ns | 0.05 ns | 0.73 ns | 0 | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Roundtrip_PerFrame(PayloadSize: 256)` | 24.76 ns | 0.45 ns | 25.30 ns | 0 | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Poll_Empty(PayloadSize: 1024)` | 0.67 ns | 0.01 ns | 0.68 ns | 0 | new |
| `Relay.Benchmarks.Receivers.SharedMemorySpscReceiverBenchmark.Roundtrip_PerFrame(PayloadSize: 1024)` | 38.54 ns | 0.81 ns | 39.58 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.TcpSinkBenchmark — 19:45:27Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.TcpSinkBenchmark.TcpSink_Enqueue_128B` | 13.17 ns | 0.39 ns | 13.66 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks — 19:45:17Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single(ItemCount: 100000)` | 3.57 ms | 703.95 µs | 4.17 ms | 65.07 KB | new |
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 100000)` | 2.68 ms | 38.29 µs | 2.72 ms | 65.07 KB | new |
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single(ItemCount: 1000000)` | 15.57 ms | 468.37 µs | 15.87 ms | 65.07 KB | new |
| `Relay.Benchmarks.PacketSinks.QueueSinkPacketThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 1000000)` | 8.62 ms | 606.38 µs | 9.22 ms | 65.07 KB | new |

### Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks — 19:44:52Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth1_Healthy_Default` | 0.21 ns | 0.03 ns | 0.23 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth1_Healthy_Propagate_NoNext` | 0.20 ns | 0.01 ns | 0.21 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth2_Propagate_Fork` | 6.36 ns | 0.03 ns | 6.38 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.PropagatePacketBenchmarks.Depth2_Fork_Wrapped` | 4.55 ns | 0.05 ns | 4.59 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.MultiPacketIsHealthyBenchmarks — 19:43:45Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.MultiPacketIsHealthyBenchmarks.Multi_Packet_IsHealthy` | 0.39 ns | 0.02 ns | 0.40 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks — 19:43:29Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks.Multi_Packet_Enqueue` | 3.20 ns | 0.02 ns | 3.22 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks.Multi2_Packet_Enqueue` | 4.34 ns | 0.03 ns | 4.36 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks — 19:43:05Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single(ItemCount: 100000)` | 6.47 ms | 761.93 µs | 7.18 ms | 64.97 KB | new |
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 100000)` | 2.67 ms | 28.70 µs | 2.69 ms | 64.97 KB | new |
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single(ItemCount: 1000000)` | 19.35 ms | 1.73 ms | 21.06 ms | 64.96 KB | new |
| `Relay.Benchmarks.PacketSinks.MpscPacketQueueSinkThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 1000000)` | 8.12 ms | 90.16 µs | 8.20 ms | 64.97 KB | new |

### Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks — 19:42:40Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks.Filter_Packet_Pass` | 6.17 ns | 0.02 ns | 6.19 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.FilterPacketSinkBenchmarks.Filter_Packet_Reject` | 0.20 ns | 0.01 ns | 0.22 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.FileSinkBenchmark — 19:42:03Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.FileSinkBenchmark.FileSink_Enqueue_128B` | 7.55 ns | 0.07 ns | 7.62 ns | 0 | new |

### Relay.Benchmarks.PacketSinks.ChainBenchmark — 19:41:56Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PacketSinks.ChainBenchmark.TcpSink_NoPropagation` | 9.66 ns | 0.08 ns | 9.74 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.ChainBenchmark.FanOut_2Sinks` | 17.81 ns | 1.43 ns | 19.54 ns | 0 | new |
| `Relay.Benchmarks.PacketSinks.ChainBenchmark.SerializeSink_Overhead` | 6.34 ns | 0.28 ns | 6.66 ns | 0 | new |

### Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark — 19:41:26Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: Default)` | 98.38 ms | 573.11 µs | 99.00 ms | 17.32 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: Default)` | 99.14 ms | 460.01 µs | 99.56 ms | 17.04 KB | ≈ +0.8% |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: NormalPriority)` | 98.78 ms | 519.11 µs | 99.21 ms | 17.32 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: NormalPriority)` | 99.79 ms | 530.55 µs | 100.25 ms | 17.04 KB | ≈ +1.0% |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: NormalPinned)` | 98.41 ms | 1.37 ms | 100.09 ms | 17.32 KB | new |
| `Relay.Benchmarks.Internal.QueueSinkLatencyBenchmark.Measure_P999_Latency_Ns(Scenario: NormalPinned)` | 96.74 ms | 1.27 ms | 97.98 ms | 17.32 KB | ≈ -1.7% |

### Relay.Benchmarks.Baselines.TypedSinkBaselineBenchmark — 19:41:19Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.Baselines.TypedSinkBaselineBenchmark.TcpSinkTyped_Enqueue` | 0.34 ns | 0.00 ns | 0.34 ns | 0 | new |

### Relay.Benchmarks.RingBufferBenchmarks — 19:41:11Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty(Capacity: 64)` | 0.20 ns | 0.02 ns | 0.22 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip(Capacity: 64)` | 0.70 ns | 0.00 ns | 0.71 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full(Capacity: 64)` | 0.25 ns | 0.03 ns | 0.27 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32(Capacity: 64)` | 72.67 ns | 4.95 ns | 77.54 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty(Capacity: 1024)` | 0.23 ns | 0.00 ns | 0.23 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip(Capacity: 1024)` | 1.09 ns | 0.02 ns | 1.12 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full(Capacity: 1024)` | 0.22 ns | 0.01 ns | 0.23 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32(Capacity: 1024)` | 52.53 ns | 0.37 ns | 52.85 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryConsume_Empty(Capacity: 65536)` | 0.00 ns | 0.00 ns | 0.00 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip(Capacity: 65536)` | 1.22 ns | 0.02 ns | 1.24 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.TryPublish_Full(Capacity: 65536)` | 0.20 ns | 0.01 ns | 0.21 ns | 0 | new |
| `Relay.Benchmarks.RingBufferBenchmarks.RoundTrip_Batch32(Capacity: 65536)` | 66.87 ns | 0.50 ns | 67.23 ns | 0 | new |

### Relay.Benchmarks.QueuePipeThroughputBenchmarks — 19:38:10Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single(ItemCount: 100000)` | 2.08 ms | 47.19 µs | 2.13 ms | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32(ItemCount: 100000)` | 1.77 ms | 102.20 µs | 1.87 ms | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 100000)` | 123.43 ms | 786.33 µs | 124.19 ms | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32_SlowBackend(ItemCount: 100000)` | 122.87 ms | 198.42 µs | 123.05 ms | 16.93 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single(ItemCount: 100000)` | 3.93 ms | 128.55 µs | 4.01 ms | 16.82 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single_SlowBackend(ItemCount: 100000)` | 125.03 ms | 322.47 µs | 125.35 ms | 16.85 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single(ItemCount: 1000000)` | 4.66 ms | 72.17 µs | 4.73 ms | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32(ItemCount: 1000000)` | 3.46 ms | 105.36 µs | 3.56 ms | 16.92 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Single_SlowBackend(ItemCount: 1000000)` | 125.11 ms | 230.46 µs | 125.34 ms | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.Push_Batch32_SlowBackend(ItemCount: 1000000)` | 124.45 ms | 201.65 µs | 124.60 ms | 17.00 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single(ItemCount: 1000000)` | 14.52 ms | 438.34 µs | 14.80 ms | 16.82 KB | new |
| `Relay.Benchmarks.QueuePipeThroughputBenchmarks.MpscPush_Single_SlowBackend(ItemCount: 1000000)` | 127.41 ms | 531.63 µs | 127.85 ms | 16.85 KB | new |

### Relay.Benchmarks.PropagateBenchmarks — 19:36:48Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.PropagateBenchmarks.Depth1_Healthy_Default` | 0.21 ns | 0.01 ns | 0.22 ns | 0 | new |
| `Relay.Benchmarks.PropagateBenchmarks.Depth1_Healthy_Propagate_NoNext` | 0.24 ns | 0.00 ns | 0.24 ns | 0 | new |
| `Relay.Benchmarks.PropagateBenchmarks.Depth2_Propagate_Fork` | 0.68 ns | 0.00 ns | 0.69 ns | 0 | new |
| `Relay.Benchmarks.PropagateBenchmarks.Depth2_Fork_Wrapped` | 3.59 ns | 0.02 ns | 3.61 ns | 0 | new |

### Relay.Benchmarks.MultiIsHealthyBenchmarks — 19:35:49Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MultiIsHealthyBenchmarks.Multi_IsHealthy` | 0.44 ns | 0.00 ns | 0.44 ns | 0 | new |
| `Relay.Benchmarks.MultiIsHealthyBenchmarks.Multi2_IsHealthy` | 0.47 ns | 0.00 ns | 0.47 ns | 0 | new |

### Relay.Benchmarks.MultiEnqueueBenchmarks — 19:35:17Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MultiEnqueueBenchmarks.Multi_Enqueue` | 3.51 ns | 0.01 ns | 3.52 ns | 0 | new |
| `Relay.Benchmarks.MultiEnqueueBenchmarks.Multi2_Enqueue` | 3.47 ns | 0.00 ns | 3.47 ns | 0 | new |

### Relay.Benchmarks.MpscSlotLayoutBenchmarks — 19:34:53Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 1, Capacity: 1024)` | 1.078 s | 1.33 ms | 1.079 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 1, Capacity: 1024)` | 402.15 ms | 12.08 ms | 412.74 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 1, Capacity: 65536)` | 212.73 ms | 40.31 ms | 245.83 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 1, Capacity: 65536)` | 180.06 ms | 47.44 ms | 225.86 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 2, Capacity: 1024)` | 1.269 s | 596.94 ms | 1.856 s | 136 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 2, Capacity: 1024)` | 1.213 s | 654.66 ms | 1.856 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 2, Capacity: 65536)` | 291.94 ms | 14.44 ms | 305.98 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 2, Capacity: 65536)` | 291.13 ms | 3.05 ms | 294.14 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 4, Capacity: 1024)` | 1.107 s | 132.74 ms | 1.225 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 4, Capacity: 1024)` | 1.378 s | 477.80 ms | 1.692 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 4, Capacity: 65536)` | 547.43 ms | 56.99 ms | 603.51 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 4, Capacity: 65536)` | 396.62 ms | 55.54 ms | 443.15 ms | 136 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 8, Capacity: 1024)` | 1.327 s | 124.29 ms | 1.427 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 8, Capacity: 1024)` | 1.678 s | 276.25 ms | 1.891 s | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.StrideLayout_Throughput(ProducerCount: 8, Capacity: 65536)` | 937.65 ms | 21.34 ms | 958.06 ms | 424 B | new |
| `Relay.Benchmarks.MpscSlotLayoutBenchmarks.LegacySlotLayout_Throughput(ProducerCount: 8, Capacity: 65536)` | 736.48 ms | 27.18 ms | 763.29 ms | 88 B | new |

### Relay.Benchmarks.MpscContentionBenchmarks — 19:32:57Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount: 1)` | 280.05 ms | 30.54 ms | 303.50 ms | 464 B | new |
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount: 2)` | 343.67 ms | 165.68 ms | 446.09 ms | 464 B | new |
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount: 4)` | 519.57 ms | 210.93 ms | 704.36 ms | 464 B | new |
| `Relay.Benchmarks.MpscContentionBenchmarks.Mpsc_Throughput_TotalItems(ProducerCount: 8)` | 726.17 ms | 29.03 ms | 754.71 ms | 464 B | new |

### Relay.Benchmarks.MpscByteRingBufferBenchmarks — 19:32:40Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 64, PayloadSize: 8)` | 3.52 ns | 0.08 ns | 3.60 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 64, PayloadSize: 8)` | 7.62 ns | 0.14 ns | 7.76 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 64, PayloadSize: 8)` | 0.95 ns | 0.10 ns | 1.05 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 64, PayloadSize: 8)` | 0.25 ns | 0.01 ns | 0.26 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 64, PayloadSize: 64)` | 0.89 ns | 0.00 ns | 0.89 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 64, PayloadSize: 64)` | 1.49 ns | 0.00 ns | 1.49 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 64, PayloadSize: 64)` | 0.67 ns | 0.00 ns | 0.67 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 64, PayloadSize: 64)` | 0.23 ns | 0.00 ns | 0.23 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 64, PayloadSize: 256)` | 1.13 ns | 0.02 ns | 1.15 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 64, PayloadSize: 256)` | 1.56 ns | 0.03 ns | 1.59 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 64, PayloadSize: 256)` | 0.76 ns | 0.03 ns | 0.79 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 64, PayloadSize: 256)` | 0.23 ns | 0.00 ns | 0.24 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 64, PayloadSize: 1024)` | 0.89 ns | 0.02 ns | 0.90 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 64, PayloadSize: 1024)` | 1.56 ns | 0.03 ns | 1.58 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 64, PayloadSize: 1024)` | 0.70 ns | 0.06 ns | 0.76 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 64, PayloadSize: 1024)` | 0.28 ns | 0.08 ns | 0.36 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 1024, PayloadSize: 8)` | 3.48 ns | 0.03 ns | 3.51 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 1024, PayloadSize: 8)` | 7.39 ns | 0.11 ns | 7.50 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 1024, PayloadSize: 8)` | 0.94 ns | 0.08 ns | 1.01 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 1024, PayloadSize: 8)` | 0.23 ns | 0.01 ns | 0.24 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 1024, PayloadSize: 64)` | 3.58 ns | 0.02 ns | 3.59 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 1024, PayloadSize: 64)` | 7.40 ns | 0.01 ns | 7.42 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 1024, PayloadSize: 64)` | 0.88 ns | 0.00 ns | 0.88 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 1024, PayloadSize: 64)` | 0.25 ns | 0.02 ns | 0.26 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 1024, PayloadSize: 256)` | 5.35 ns | 0.03 ns | 5.38 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 1024, PayloadSize: 256)` | 8.92 ns | 0.15 ns | 9.07 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 1024, PayloadSize: 256)` | 0.84 ns | 0.01 ns | 0.86 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 1024, PayloadSize: 256)` | 0.25 ns | 0.02 ns | 0.27 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 1024, PayloadSize: 1024)` | 0.85 ns | 0.01 ns | 0.86 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 1024, PayloadSize: 1024)` | 1.52 ns | 0.02 ns | 1.54 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 1024, PayloadSize: 1024)` | 0.65 ns | 0.01 ns | 0.66 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 1024, PayloadSize: 1024)` | 0.24 ns | 0.01 ns | 0.24 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 65536, PayloadSize: 8)` | 3.54 ns | 0.10 ns | 3.63 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 65536, PayloadSize: 8)` | 7.35 ns | 0.11 ns | 7.45 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 65536, PayloadSize: 8)` | 0.91 ns | 0.04 ns | 0.94 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 65536, PayloadSize: 8)` | 0.23 ns | 0.01 ns | 0.24 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 65536, PayloadSize: 64)` | 3.84 ns | 0.03 ns | 3.86 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 65536, PayloadSize: 64)` | 7.70 ns | 0.18 ns | 7.87 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 65536, PayloadSize: 64)` | 0.91 ns | 0.04 ns | 0.94 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 65536, PayloadSize: 64)` | 0.25 ns | 0.01 ns | 0.26 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 65536, PayloadSize: 256)` | 6.32 ns | 0.04 ns | 6.35 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 65536, PayloadSize: 256)` | 9.42 ns | 0.03 ns | 9.45 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 65536, PayloadSize: 256)` | 0.89 ns | 0.02 ns | 0.91 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 65536, PayloadSize: 256)` | 0.25 ns | 0.01 ns | 0.26 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Spsc_RoundTrip(Capacity: 65536, PayloadSize: 1024)` | 16.21 ns | 0.10 ns | 16.30 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_RoundTrip_NoContention(Capacity: 65536, PayloadSize: 1024)` | 19.62 ns | 0.52 ns | 20.14 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPublish_Full(Capacity: 65536, PayloadSize: 1024)` | 0.89 ns | 0.02 ns | 0.90 ns | 0 | new |
| `Relay.Benchmarks.MpscByteRingBufferBenchmarks.Mpsc_TryPeek_Empty(Capacity: 65536, PayloadSize: 1024)` | 0.24 ns | 0.01 ns | 0.25 ns | 0 | new |

### Relay.Benchmarks.MpscByteContentionBenchmarks — 19:20:42Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount: 1)` | 1.135 s | 593.18 ms | 1.683 s | 128 B | new |
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount: 2)` | 991.66 ms | 828.70 ms | 1.809 s | 464 B | new |
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount: 4)` | 735.17 ms | 185.74 ms | 918.30 ms | 464 B | new |
| `Relay.Benchmarks.MpscByteContentionBenchmarks.Mpsc_Byte_Throughput_TotalItems(ProducerCount: 8)` | 1.368 s | 254.16 ms | 1.619 s | 464 B | new |

### Relay.Benchmarks.MpscBenchmarks — 19:20:04Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline(Capacity: 64)` | 1.07 ns | 0.03 ns | 1.10 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention(Capacity: 64)` | 7.03 ns | 0.05 ns | 7.08 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full(Capacity: 64)` | 0.26 ns | 0.07 ns | 0.33 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty(Capacity: 64)` | 0.21 ns | 0.01 ns | 0.22 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline(Capacity: 1024)` | 1.39 ns | 0.09 ns | 1.48 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention(Capacity: 1024)` | 7.57 ns | 0.01 ns | 7.58 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full(Capacity: 1024)` | 0.25 ns | 0.02 ns | 0.27 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty(Capacity: 1024)` | 0.20 ns | 0.01 ns | 0.20 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Spsc_TryPublish_Baseline(Capacity: 65536)` | 1.40 ns | 0.15 ns | 1.54 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_NoContention(Capacity: 65536)` | 8.64 ns | 0.33 ns | 8.86 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryPublish_Full(Capacity: 65536)` | 0.24 ns | 0.03 ns | 0.28 ns | 0 | new |
| `Relay.Benchmarks.MpscBenchmarks.Mpsc_TryConsume_Empty(Capacity: 65536)` | 0.22 ns | 0.01 ns | 0.23 ns | 0 | new |

### Relay.Benchmarks.FilterSinkBenchmarks — 19:17:05Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.FilterSinkBenchmarks.Filter_Pass` | 2.47 ns | 0.04 ns | 2.49 ns | 0 | new |
| `Relay.Benchmarks.FilterSinkBenchmarks.Filter_Reject` | 0.26 ns | 0.01 ns | 0.26 ns | 0 | new |

### Relay.Benchmarks.EnqueueBenchmarks — 19:16:38Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.EnqueueBenchmarks.Depth1_Healthy` | 0.24 ns | 0.03 ns | 0.26 ns | 0 | new |
| `Relay.Benchmarks.EnqueueBenchmarks.Depth2_AcceptReject` | 1.54 ns | 0.03 ns | 1.58 ns | 0 | new |
| `Relay.Benchmarks.EnqueueBenchmarks.Depth2_HeadUnhealthy` | 0.46 ns | 0.04 ns | 0.50 ns | 0 | new |
| `Relay.Benchmarks.EnqueueBenchmarks.Depth3_AllUnhealthy` | 1.87 ns | 0.01 ns | 1.88 ns | 0 | new |

### Relay.Benchmarks.ByteRingBufferBenchmarks — 19:15:27Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 64, PayloadSize: 8)` | 0.01 ns | 0.01 ns | 0.03 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 64, PayloadSize: 8)` | 3.41 ns | 0.05 ns | 3.46 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 64, PayloadSize: 8)` | 0.73 ns | 0.08 ns | 0.81 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 64, PayloadSize: 64)` | 0.00 ns | 0.00 ns | 0.00 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 64, PayloadSize: 64)` | 0.94 ns | 0.03 ns | 0.97 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 64, PayloadSize: 64)` | 0.43 ns | 0.01 ns | 0.43 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 64, PayloadSize: 256)` | 0.01 ns | 0.01 ns | 0.02 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 64, PayloadSize: 256)` | 0.91 ns | 0.03 ns | 0.93 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 64, PayloadSize: 256)` | 0.44 ns | 0.00 ns | 0.45 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 64, PayloadSize: 1024)` | 0.01 ns | 0.00 ns | 0.01 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 64, PayloadSize: 1024)` | 0.87 ns | 0.02 ns | 0.89 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 64, PayloadSize: 1024)` | 0.42 ns | 0.00 ns | 0.42 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 1024, PayloadSize: 8)` | 0.01 ns | 0.02 ns | 0.03 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 1024, PayloadSize: 8)` | 3.45 ns | 0.02 ns | 3.47 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 1024, PayloadSize: 8)` | 0.65 ns | 0.01 ns | 0.66 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 1024, PayloadSize: 64)` | 0.03 ns | 0.01 ns | 0.04 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 1024, PayloadSize: 64)` | 3.40 ns | 0.01 ns | 3.41 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 1024, PayloadSize: 64)` | 0.68 ns | 0.02 ns | 0.70 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 1024, PayloadSize: 256)` | 0.03 ns | 0.01 ns | 0.03 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 1024, PayloadSize: 256)` | 5.62 ns | 0.02 ns | 5.64 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 1024, PayloadSize: 256)` | 0.67 ns | 0.01 ns | 0.69 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 1024, PayloadSize: 1024)` | 0.02 ns | 0.01 ns | 0.03 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 1024, PayloadSize: 1024)` | 0.87 ns | 0.01 ns | 0.88 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 1024, PayloadSize: 1024)` | 0.43 ns | 0.01 ns | 0.44 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 65536, PayloadSize: 8)` | 0.01 ns | 0.01 ns | 0.02 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 65536, PayloadSize: 8)` | 3.38 ns | 0.04 ns | 3.42 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 65536, PayloadSize: 8)` | 0.65 ns | 0.01 ns | 0.66 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 65536, PayloadSize: 64)` | 0.00 ns | 0.00 ns | 0.01 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 65536, PayloadSize: 64)` | 3.55 ns | 0.01 ns | 3.55 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 65536, PayloadSize: 64)` | 0.66 ns | 0.01 ns | 0.67 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 65536, PayloadSize: 256)` | 0.02 ns | 0.00 ns | 0.02 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 65536, PayloadSize: 256)` | 6.28 ns | 0.02 ns | 6.30 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 65536, PayloadSize: 256)` | 0.68 ns | 0.00 ns | 0.68 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPeek_Empty(Capacity: 65536, PayloadSize: 1024)` | 0.45 ns | 0.01 ns | 0.46 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.RoundTrip(Capacity: 65536, PayloadSize: 1024)` | 15.65 ns | 0.07 ns | 15.72 ns | 0 | new |
| `Relay.Benchmarks.ByteRingBufferBenchmarks.TryPublish_Full(Capacity: 65536, PayloadSize: 1024)` | 0.67 ns | 0.01 ns | 0.68 ns | 0 | new |

### Relay.Benchmarks.ByteEnqueueBenchmarks — 19:03:52Z  ·  _micro · env 8e045e34 · 8817dad · baseline_

| benchId | Mean | StdDev | P99 | Alloc B/op | Δ vs prior |
|---|---|---|---|---|---|
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Healthy` | 0.24 ns | 0.05 ns | 0.29 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth2_Byte_AcceptReject` | 3.84 ns | 0.01 ns | 3.84 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth2_Byte_HeadUnhealthy` | 3.37 ns | 0.01 ns | 3.38 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth3_Byte_AllUnhealthy` | 4.54 ns | 0.14 ns | 4.67 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_TryEnqueue_Healthy` | 0.50 ns | 0.04 ns | 0.54 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_TryEnqueue_Reject` | 0.29 ns | 0.04 ns | 0.32 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Drop_NextNull_Unhealthy` | 4.02 ns | 0.02 ns | 4.04 ns | 0 | new |
| `Relay.Benchmarks.ByteEnqueueBenchmarks.Depth1_Byte_Drop_NextNull_Reject` | 4.21 ns | 0.14 ns | 4.29 ns | 0 | new |

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
