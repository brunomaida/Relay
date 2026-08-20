---
type: plan
status: approved
solution: Libs\Relay
---

# Plan: Native-allocation footprint regression guard (Relay)

Cross-repo spec/context: `C:\Development\Libs\docs\superpowers\plans\2026-08-20-native-alloc-footprint-guard.md` — read it for full background. Related issue: [Relay#20](https://github.com/brunomaida/Relay/issues/20).

## Global Constraints

- No public API surface change. All new members are `internal`.
- Follow repo tier: `Libs\Relay\CLAUDE.md` declares `tier: ultra-low-latency` — the new helper sits in constructors only (cold path relative to `TryPublish`/`TryConsume`); do not touch any hot-path method.
- `AlignedAlloc`'s existing 2-arg calls (`(bytes, alignment)`) are legitimate and must NOT be flagged by the new gate — only `Alloc`/`AllocZeroed` 2-arg calls are the bug shape.
- Do not add relational validation like `byteCount >= alignment` or `byteCount % alignment == 0` in the new helper — `SpscRingBuffer<Payload1>(capacity: 4)` legitimately calls with `byteCount=4, alignment=64` and must keep working.
- Free-path asymmetry matters: aligned allocations pair only with `AlignedFree`; unaligned only with `Free`. Never mix.
- `xUnit` + no FluentAssertions unless the project already references it — check `Relay.Tests.csproj` and match existing style.

## Task 1: `NativeBuffer` helper, migrate all 4 call sites, and footprint/accounting tests

**Files (production):**
- New: `src\Relay\Memory\NativeBuffer.cs` — `internal static unsafe class NativeBuffer` in namespace `Relay.Memory`, sibling of the existing `RelayMemory` class in the same folder.
- Edit: `src\Relay\Buffers\SpscRingBuffer.cs:71-73`, `src\Relay\Buffers\MpscRingBuffer.cs:82-84`, `src\Relay\Sinks\MemorySink.cs:42,74-78`, `src\Relay\Sinks\MemorySink.Packet.cs:44,114`.

**`NativeBuffer` surface:**
```
internal static unsafe class NativeBuffer
{
    internal static void* AllocZeroedAligned(nuint byteCount, int alignment = 64);
    internal static void FreeAligned(void* ptr, nuint byteCount);
    internal static void* AllocZeroed(nuint byteCount);
    internal static void Free(void* ptr, nuint byteCount);

    // accounting, [ThreadStatic], incremented inside the wrappers above
    [ThreadStatic] internal static long AlignedBytesOutstanding;
    [ThreadStatic] internal static long UnalignedBytesOutstanding;
}
```
- `AllocZeroedAligned` validates `alignment` is a power of two and `<= 4096`, plus `byteCount > 0`, then calls `NativeMemory.AlignedAlloc(byteCount, (nuint)alignment)` and increments `AlignedBytesOutstanding` by `byteCount`. `FreeAligned` calls `NativeMemory.AlignedFree(ptr)` and decrements by `byteCount`.
- `AllocZeroed`/`Free` mirror this for the unaligned family (`NativeMemory.AllocZeroed(byteCount)` / `NativeMemory.Free(ptr)`).
- `[ThreadStatic]` (not `Interlocked`) — Relay's test collections run in parallel; a global counter would race across concurrently-constructed buffers. Each test's constructor call and its own assertions run on the same thread, so the counter is isolated by construction.
- XML doc on the class explaining why this exists (the Wave incident, one sentence) and why relational alignment/byteCount validation is deliberately absent (breaks the `Payload1`/small-capacity case).

**Call-site migration** (each becomes a single-argument call):
- `SpscRingBuffer.cs:71-73`: replace the two-line `_bytesAllocated = capacity * sizeof(T); _basePtr = (T*)NativeMemory.AlignedAlloc(...)` with `_bytesAllocated = capacity * sizeof(T); _basePtr = (T*)NativeBuffer.AllocZeroedAligned((nuint)_bytesAllocated);` and the paired free site (constructor around line 207) to `NativeBuffer.FreeAligned(_basePtr, (nuint)_bytesAllocated);`
- `MpscRingBuffer.cs:82-84` (stride-based) and its free at line 184: same pattern, using `_bytesAllocated = capacity * _stride`.
- `MemorySink.cs:42` (unaligned family) and its two free paths at lines 74-78 (`Dispose` + finalizer) — both must route through `NativeBuffer.Free`, since the finalizer is a second free path that needs identical accounting.
- `MemorySink.Packet.cs:44` (aligned family, byte-addressed — no `sizeof(T)` factor, `byteCount == capacity`) and its free at line 114.

**Test file (new):** `tests\Relay.Tests\Memory\NativeAllocationSizeTests.cs`
- Payload structs (shared, put in this file or a sibling `Memory\TestPayloads.cs`): `Payload1` (1 byte), `Payload8`, `Payload64`, `Payload256` — `[StructLayout(LayoutKind.Sequential, Size = N)]`.
- Use `_msize`/`_aligned_msize` from `ucrtbase.dll` as the oracle (P/Invoke via `NativeLibrary.Load("ucrtbase.dll")` + `GetExport` + `delegate* unmanaged[Cdecl]`). **Mandatory calibration test first**: `Probe_Calibration_ReportsSaneSizeForKnownAllocation` — allocate a known 1 MiB block via `NativeBuffer.AllocZeroed`, assert the probe reports `>= 1 MiB && < 2 MiB`; if the probe returns 0/-1/garbage, `Skip` the whole class with an explicit reason rather than silently passing everything downstream.
- Assertion band (allocators round up): `actual >= expected && actual < expected * 2 + 4096`.
- One `[Theory]` per payload type (xUnit can't parameterize a generic type param from `InlineData`) delegating to a private generic helper, capacities {4, 64, 1024}:
  - `SpscRingBuffer_Payload{1,8,64,256}_BlockSizeMatchesCapacityTimesSizeOfT`
  - `MpscRingBuffer_Payload{1,8,64,256}_BlockSizeMatchesCapacityTimesStride` — note the **stride** (`64 + sizeof(T)`), not just `sizeof(T)`.
  - `MemorySinkTyped_Payload{1,8,64,256}_BlockSizeMatchesCapacityTimesSizeOfT` — unaligned family, use `_msize` not `_aligned_msize`.
  - `MemorySinkPacket_BlockSizeMatchesCapacityBytes` — byte-addressed, `expected == capacity`.
- **Regression test for the exact Wave bug shape:** `NativeBuffer_AllocZeroedAligned_SwappedArguments_Throws` — call `AllocZeroedAligned(byteCount: 64, alignment: 4_194_304)` and assert it throws (alignment validation catches the swap).
- `NativeBuffer_AllocZeroedAligned_RejectsNonPowerOfTwoAlignment` — throws.

**Test file (new):** `tests\Relay.Tests\Memory\AllocationAccountingTests.cs`
- `[Collection("NativeAccounting")]` to disable xUnit parallelism for this class only (counters are thread-static per-thread but the collection avoids surprises from xUnit running tests of this class on different threads mid-run — verify actual xUnit thread-reuse behavior locally and adjust if the counter reads flaky; if `[ThreadStatic]` proves insufficient in practice, note that in your report rather than silently switching to `Interlocked` without recording why).
- `SpscRingBuffer_Dispose_ReturnsAllOutstandingAlignedBytes` — outstanding delta returns to 0 after `Dispose`.
- `MemorySinkTyped_Dispose_ReturnsAllOutstandingUnalignedBytes` — 0.
- `MemorySinkTyped_Finalizer_ReturnsOutstandingBytes` — exercise the finalizer path (e.g. via `GC.Collect()`/`GC.WaitForPendingFinalizers()` after dropping the last reference without explicit `Dispose`), assert the counter returns to 0.

**Mandatory negative proof (do this before committing, revert after):** temporarily change one migrated call site back to the buggy 2-arg shape (e.g. `NativeMemory.AllocZeroed(bufferBytes, (nuint)sizeof(T))` directly, bypassing `NativeBuffer`), run the footprint tests, confirm they go red, then revert. Report the before/after test output in your report file — this is what proves the suite actually catches the defect class, not just today's correct code.

**Report:** DONE / DONE_WITH_CONCERNS / NEEDS_CONTEXT / BLOCKED, commits, one-line test summary (pass count), and explicit confirmation the negative-proof step was run with its result.

## Task 2: Self-proving source-scan gate test

**File (new):** `tests\Relay.Tests\Memory\NativeAllocationGateTests.cs`

- Rule: no direct `NativeMemory.*` reference anywhere under `src\Relay` except inside `src\Relay\Memory\NativeBuffer.cs`. Phrase the detector as "no direct call at all" — not "no 2-argument call" — since `NativeBuffer.cs` itself legitimately contains 2-arg `AlignedAlloc` calls internally, and arity alone can't discriminate once the helper exists.
- `NativeMemory_IsNotCalledOutsideNativeBuffer` — scan every `.cs` under the source root; assert zero files other than `NativeBuffer.cs` contain a `NativeMemory.` token (exclude comment/XML-doc lines — `SpscRingBuffer.cs` has a `<see cref="NativeMemory.AlignedAlloc.../>` doc reference that must not trip the scanner).
- `Detector_FlagsKnownViolationFixture` — **positive control, not optional.** Feed the same detector logic a fixture string containing a raw `NativeMemory.AllocZeroed(bytes, (nuint)sizeof(T));` call and assert it IS flagged. A gate with no negative-control test can be a regex that matches nothing and silently never fires — this test is what proves it doesn't.
- `Detector_IgnoresXmlDocReferences` — explicit case for the `<see cref=.../>` false-positive above.
- `SourceRoot_Resolves` — fails (does not silently skip) if the source root directory can't be located. Locate it via `AssemblyMetadataAttribute` injected in `Relay.Tests.csproj`: add `<AssemblyMetadata Include="RelaySourceRoot" Value="$(MSBuildThisFileDirectory)..\..\src\Relay" />` and read it via `Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()` — do not resolve the path by walking up from `AppContext.BaseDirectory`.
- Also ban `using static System.Runtime.InteropServices.NativeMemory;` in the same scan (otherwise the token check is trivially evadable).

**Report:** same contract as Task 1.

## Task 3: Documentation

- `changelog.d\<slug>.md` (repo convention — check an existing fragment for the exact frontmatter/format) describing the hardening, no `### Perf` block needed (behavior-preserving, tests only).
- `docs\architecture-decisions.md` — new entry documenting: the bug class, why `NativeBuffer` exists, and the decision NOT to add relational alignment/byteCount validation (with the `Payload1` counter-example).
- `docs\bench-methodology.md` — short note: `[MemoryDiagnoser]` cannot detect `NativeMemory` over-allocation (bypasses GC heap; allocation happens in `[GlobalSetup]`, outside the measured window) — this is why footprint coverage lives in `Relay.Tests`, not the benchmark suite.

**Report:** same contract.

## Verification (whole branch)

`dotnet test` on `Relay.Tests` — all green. Confirm via `git log` that the negative-proof revert (Task 1) left no trace in the final diff.
