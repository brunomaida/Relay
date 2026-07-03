### Fixed
- `FileStreamSink<T>`: prevent consumer thread crash on sustained backend failure. `FlushBuffer` left `_bufferPos` unreset after an `IOException`, causing an out-of-bounds write on the next `WriteToBackend` call; `TryRecoverBackend` left a disposed `_stream` reference in place after a failed reopen, causing an uncaught `ObjectDisposedException` on the next flush. Both silently killed the consumer thread permanently.
  - Root-cause: #NA · Regression-test: `Relay.Tests.FileStreamSinkCrashTests.IOException_InFlushBuffer_DoesNotKillConsumer`, `Relay.Tests.FileStreamSinkCrashTests.RecoveryFailure_WithDisposedStream_DoesNotKillConsumer`
- `MemorySink<T>`: add a finalizer as a safety net against native-memory leaks when a caller omits `Dispose()`. `Dispose()` continues to be the primary release path and calls `GC.SuppressFinalize`.
  - Root-cause: #NA · Regression-test: `Relay.Tests.MemorySinkTests.Dispose_ReleasesNativeMemory_NoException`, `Relay.Tests.MemorySinkTests.Dispose_CalledTwice_IsIdempotent`

### Perf
- **estimated** — `PacketSink._dropCount` moved to a 128-byte explicit-layout padded struct (`PaddedDropCount`, offset 64), removing false sharing between the Interlocked-incremented counter and adjacent `Next`/`PropagateAfterAccept` fields on the same object; no BDN delta expected on the uncontended path, benefit is under concurrent `DropCount` monitoring.
- **estimated** — `RotatingFileSink.ShouldRotate`: RDTSC sampling (`HfClock.NowTicks`) throttled from every record to 1-in-256 via a counter mask. Prior baseline (2026-05-25 hot-path-audit): 13.59 ns/call, every-record RDTSC read (~47c model). Amortized cost is now dominated by the 255/256 skip path (~1c size compare); the sampled 1/256 call remains ~47c. Not re-benchmarked in this pass — see `docs/reports/2026-07-02-hot-path-audit-relay.md` §H27.
- **estimated** — `MpscByteRingBuffer.TryPeek`: wrap-padding skip rewritten from a recursive call to an iterative `while(true)` loop. JIT does not inline recursive methods; the iterative form is inlining-eligible at call sites, removing a non-inlined call/return pair on every wrap-skip. Same instruction count per skip iteration; no functional change (`Relay.Tests.MpscByteRingBufferTests.TryPublish_WrapAround_UsesPaddingMarker_ConsumerSkips` covers the skip path).

### Commits
- `65b6cf1` — fix(FileStreamSink): prevent consumer crash on sustained backend failure
- `ab33f86` — perf: throttle RDTSC in RotatingFileSink.ShouldRotate
- `9aae152` — perf: eliminate TryPeek recursion in MpscByteRingBuffer
- `c1818fa` — chore: add MemorySink finalizer; pad PacketSink._dropCount to prevent false sharing
