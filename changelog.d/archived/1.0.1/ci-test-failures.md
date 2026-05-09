### Fixed

- `SharedMemorySinkTests`: mark 4 facts as `[Fact(Skip = "Windows only")]` — `MemoryMappedFile.CreateOrOpen` with a named map throws `PlatformNotSupportedException` on Linux; the `[SupportedOSPlatform("windows")]` attribute is a Roslyn hint only and does not cause xUnit to skip at runtime.
- `BatchSinkTests.Flush_signal_drains_pending_payload`: replaced `Thread.Sleep(100)` with `sink.Stop(1_000)` — consumer thread runs at `BelowNormal` priority and was not guaranteed to be scheduled within 100 ms on a loaded CI VM; `Stop` joins the thread, giving a hard happens-before before assertion.

### Perf

No performance-sensitive code changed.
