---
slug: 260526-14-shm-pipe-receivers
type: feat
---

### Summary
- Add `SharedMemorySpscReceiver<TState>`: hot-path SPSC ring consumer; mirrors `SharedMemorySpscSink` wire format exactly (128-byte header, 4B BE length prefix, ring-wrapped payload, `Volatile.Read/Write` on ReadIndex/WriteIndex)
- Add `NamedPipeReceiver<TState>`: management-plane named-pipe receiver; compatible with `NamedPipeSink` wire format; `WaitForConnection` blocks on management thread, `Poll` reads synchronously (blocking acceptable on management-plane)
- Add `RelayBuilder.FromSharedMemory` and `RelayBuilder.FromNamedPipe` factory methods; `FromSharedMemory` carries `[SupportedOSPlatform("windows")]`

### Perf
estimated — `SharedMemorySpscReceiver.Poll` is hot-path: `Volatile.Read` on WriteIndex, ring-wrap arithmetic with `Math.Min` chunk, `Volatile.Write` on ReadIndex after consume; POH-pinned frame buffer pre-touched in ctor (eliminates soft page faults on first receive). `NamedPipeReceiver.Poll` is management-plane only; blocking `ReadFile` is acceptable and avoids the IOCP/SynchronizationContext complexity of async reads on synchronous callers.
