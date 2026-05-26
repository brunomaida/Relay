---
slug: 260525-4-receivers
type: feat
---

### Summary
- `PacketCallback<TState>` delegate — zero-alloc span-compatible callback; sidesteps `Action<ReadOnlySpan<byte>>` C# restriction; static-lambda dispatch eliminates closure allocation
- `PacketReceiver` abstract base — passive receiver hierarchy (caller's loop drives via `Poll()`); optional `Next: PacketSink?` forward-chain
- `UdpReceiver<TState>` (hot path) — non-blocking `Poll()` via `Socket.Poll(0, SelectRead)`; `stackalloc byte[1432]` per call (MTU-safe, cache-hot, zero GC, matches fTL DispatchReceiver); configurable kernel RX buffer (default 1 MB)
- `TcpReceiver<TState>` (management-plane) — blocking `Accept()` + non-blocking `Poll()`; POH-pinned buffer pre-touched per fTL TcpGateway (eliminates soft page faults); wire format `[4B BE length][payload]` matching Relay TcpSink/NamedPipeSink; configurable kernel RX buffer
- `RelayBuilder.From<TState>()` and `RelayBuilder.FromTcp<TState>()` factory methods
- 8 new tests (5 unit, 3 integration on real loopback) — 221 unit + 3 integration passing

### Perf
estimated — `UdpReceiver.Poll` uses `stackalloc byte[1432]` per call: MTU-safe (1432 B < 1472 B Ethernet MTU), cache-hot (stack frame is L1-resident during callback), zero GC, matches fTL DispatchReceiver pattern; `TcpReceiver` buffer pre-touched in ctor: eliminates OS soft page faults on first receive (fTL TcpGateway validated pattern); kernel RX buffer configurable to avoid burst drops; `PacketCallback<TState>` with static lambda = zero closure allocation per frame
