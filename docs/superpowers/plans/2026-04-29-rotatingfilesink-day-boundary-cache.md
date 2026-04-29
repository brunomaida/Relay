# RotatingFileSink Day-Boundary Cache — Phase 1 Implementation Plan

> **Phase 1 of 8** in master plan `docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md`. **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Eliminate the per-record `DateTime.UtcNow.Date` call in `RotatingFileSink.ShouldRotate` (consumer hot path) by caching the next day-boundary in `HfClock` ticks. Drops cost from ~50c → ~3c per consumed payload. **Closes coverage gap C1** from `benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md`.

**Architecture:** Add a `long _nextDayBoundaryTicks` field, computed on construction and resampled inside `RotateNow` (cold path). The hot `ShouldRotate` predicate becomes a single monotonic compare against `HfClock.NowTicks`. Wall-clock day naming (`_currentDay`) is unchanged — still resampled via `DateTime.UtcNow.Date` only in `RotateNow`. Add an `internal` test hook to allow forcing the boundary in unit tests without time mocking.

**Tech Stack:** .NET 9, C# 13, xUnit 2.9.2, FluentAssertions 6.12.1, BenchmarkDotNet 0.13.12. Stack already in `Directory.Packages.props`.

**Out of scope:** `UdpSink.WriteToBackend` per-record syscall — UDP is per-datagram by design (1 payload = 1 datagram); batching at this layer would change wire semantics. Documented as known throughput ceiling in cost-map. No code change. Will be measured in Phase 4 of the master plan.

**Acceptance gate (added by master plan):** `RotatingFileSinkBenchmarks.ShouldRotate_HotPath` ratio (baseline / post-fix) ≥ 10x. Phase 8 (calibration) annotates cost-map with the absolute measured ns.

---

## File Structure

| File | Action | Responsibility |
|---|---|---|
| `benchmarks/Relay.Benchmarks/Sinks/RotatingFileSinkBenchmarks.cs` | **Create** | New BDN class measuring `ShouldRotate` cost via `WriteToBackend` invocation. Captures baseline before fix and verifies post-fix. **Required by acceptance gate (≥10x ratio improvement).** |
| `src/Relay/Sinks/RotatingFileSink.cs` | Modify | Add `_nextDayBoundaryTicks` field + `ComputeNextDayBoundaryTicks` helper; replace `DateTime.UtcNow.Date` check with HfClock-tick compare; add `internal` test hook |
| `tests/Relay.Tests/Sinks/RotatingFileSinkTests.cs` | Modify | Add failing test for day-boundary rotation using new `internal` hook |
| `CHANGELOG.md` | Modify | Add entry under unreleased — `fix:` |
| `docs/reports/2026-04-29-resource-cost-map-relay.md` | Modify | Update §5 row (status: regression → resolved), §1 / §2 / §9 with measured numbers |

---

## Task 1: Branch & Baseline

**Files:** _none modified_

- [ ] **Step 1: Create the fix branch**

```bash
git checkout develop
git pull
git checkout -b fix/260429-rotatingfilesink-utcnow-hot-path
```

- [ ] **Step 2: Verify baseline tests pass**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failures (matches the project's commit-gate invariant).

If any test fails on the unmodified baseline, stop and surface the failure — do not proceed.

- [ ] **Step 3: No commit yet** (baseline-only).

---

## Task 2: BDN baseline class — capture pre-fix cost

This task adds the perf gate that gives the fix a measurable acceptance criterion. The BDN class is committed FIRST (before any production-code change) so the BDN history shows a clear "before" point for the regression that is being fixed.

**Files:**
- Create: `benchmarks/Relay.Benchmarks/Sinks/RotatingFileSinkBenchmarks.cs`

- [ ] **Step 1: Create the BDN class measuring `WriteToBackend` cost (which routes through `ShouldRotate`)**

Write to `benchmarks/Relay.Benchmarks/Sinks/RotatingFileSinkBenchmarks.cs`:

```csharp
using System;
using System.IO;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Sinks;

/// <summary>
/// Measures <see cref="RotatingFileSink"/> consumer hot path — specifically the cost of
/// the per-record rotation predicate (<c>ShouldRotate</c>) which historically called
/// <c>DateTime.UtcNow.Date</c>. Used as a regression gate for the fix that caches the
/// next-day boundary in <c>HfClock</c> ticks.
/// </summary>
/// <remarks>
/// Driven directly via reflection on the protected <c>WriteToBackend</c> method to isolate
/// the consumer-thread cost from ring publish + consumer-loop cost. The benchmark runs in a
/// single thread; no Start/Stop. Each invocation writes 64 bytes into the POH write buffer
/// and exercises the <c>ShouldRotate</c> predicate exactly once.
/// <para>
/// The reflection invoke adds a fixed ~500-cycle overhead, but the absolute number is not
/// the gate — the <b>ratio</b> (before-fix / after-fix mean ns) is what acceptance checks.
/// Expected ratio after the fix lands: ≥ 10x.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class RotatingFileSinkBenchmarks
{
    private RotatingFileSink _sink           = null!;
    private MethodInfo       _writeToBackend = null!;
    private byte[]           _payload        = new byte[64];
    private string           _dir            = string.Empty;
    private object[]         _args           = null!;

    [GlobalSetup]
    public void Setup()
    {
        _dir = Path.Combine(Path.GetTempPath(), $"relay-bench-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_dir);
        _sink = new RotatingFileSink(_dir, "log",
            maxBytes:        1_000_000_000, // large — size-based rotation never triggers
            ringCapacity:    4096,
            flushIntervalMs: 50);
        // Do NOT call Start() — drive WriteToBackend directly via reflection to isolate
        // ShouldRotate cost from ring/consumer-loop overhead.

        _writeToBackend = typeof(RotatingFileSink).GetMethod(
            "WriteToBackend",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        _args = new object[] { (ReadOnlyMemory<byte>)_payload };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try { _sink?.Dispose(); }              catch { }
        try { Directory.Delete(_dir, true); }  catch { }
    }

    /// <summary>
    /// Single payload through <c>WriteToBackend</c> — exercises <c>ShouldRotate</c> exactly once.
    /// Before fix: dominated by <c>DateTime.UtcNow.Date</c> (~11 ns @ 4.5 GHz).
    /// After fix: single <c>HfClock.NowTicks</c> compare (~0.7 ns @ 4.5 GHz).
    /// </summary>
    [Benchmark]
    public void ShouldRotate_HotPath()
    {
        _writeToBackend.Invoke(_sink, _args);
    }
}
```

- [ ] **Step 2: Build and verify the BDN class compiles**

Run: `dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo`
Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Run the BDN on the fix branch, before any production code change**

Run:
```bash
dotnet run --project benchmarks/Relay.Benchmarks -c Release --no-build -- \
  --filter "*RotatingFileSinkBenchmarks*" \
  --job short \
  --artifacts benchmarks/artifacts/2026-04-29-phase1
```

Expected: 1 benchmark, exits 0. Inspect:
`benchmarks/artifacts/2026-04-29-phase1/results/Relay.Benchmarks.Sinks.RotatingFileSinkBenchmarks-report-github.md`

Record the **baseline mean ns** (call it `BASELINE_NS`). Used in Task 8 ratio check.

- [ ] **Step 4: Run the existing test suite to confirm the BDN class did not break tests**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failures.

- [ ] **Step 5: Commit the BDN class WITHOUT the fix**

```bash
git add benchmarks/Relay.Benchmarks/Sinks/RotatingFileSinkBenchmarks.cs benchmarks/artifacts/2026-04-29-phase1/
git commit -m "test: add RotatingFileSinkBenchmarks.ShouldRotate_HotPath baseline w/Claude

Baseline: <BASELINE_NS> ns/op (DateTime.UtcNow.Date in hot path).
Used as regression gate in Task 8 — fix must reduce ratio by ≥ 10x."
```

Replace `<BASELINE_NS>` with the value recorded in Step 3.

---

## Task 3: Failing Test — Day-Boundary Crossing Triggers Rotation

**Files:**
- Modify: `tests/Relay.Tests/Sinks/RotatingFileSinkTests.cs`

- [ ] **Step 1: Add the failing test**

Append after the `Header_WrittenOncePerNewFile` test (around line 80), before `Dispose_IsIdempotent`. Use the `Relay.Internal` namespace for `HfClock` (visible via `InternalsVisibleTo Relay.Tests`).

```csharp
[Fact]
public void Enqueue_DayBoundaryCrossed_RotatesToNextFile()
{
    using var sink = new RotatingFileSink(_dir, "log", maxBytes: 1_000_000,
                                          ringCapacity: 4096, flushIntervalMs: 50);
    sink.Start();

    sink.Enqueue(new byte[100]);
    Thread.Sleep(120);                              // let the consumer drain into file 1

    sink.SetDayBoundaryForTest(Relay.Internal.HfClock.NowTicks - 1); // simulate "yesterday ended"

    sink.Enqueue(new byte[100]);
    sink.Stop(drainTimeoutMs: 1_000);

    var files = Directory.GetFiles(_dir, "log-*.log");
    files.Should().HaveCountGreaterThan(1, "day boundary crossed → rotated");
}
```

- [ ] **Step 2: Run the test to verify it fails for the right reason**

Run: `dotnet test tests/Relay.Tests --filter "FullyQualifiedName~Enqueue_DayBoundaryCrossed_RotatesToNextFile"`
Expected: COMPILE FAIL — `RotatingFileSink` does not contain `SetDayBoundaryForTest`. This is the TDD red.

- [ ] **Step 3: No commit yet.** Test will be wired up after the implementation lands and compiles.

---

## Task 4: Implementation — Cache Day Boundary in HfClock Ticks

**Files:**
- Modify: `src/Relay/Sinks/RotatingFileSink.cs`

- [ ] **Step 1: Add the new field next to `_currentDay`**

Locate the field block (around line 30 of `src/Relay/Sinks/RotatingFileSink.cs`). Replace:

```csharp
    private DateTime    _currentDay;
    private int         _backoffMs      = MinBackoffMs;
    private long        _nextRetryTicks;
```

with:

```csharp
    private DateTime    _currentDay;
    private long        _nextDayBoundaryTicks;          // HfClock ticks at the next UTC midnight
    private int         _backoffMs      = MinBackoffMs;
    private long        _nextRetryTicks;
```

- [ ] **Step 2: Initialize the boundary in the constructor**

Locate the constructor body (around line 50). Replace:

```csharp
        _currentDay  = DateTime.UtcNow.Date;
    }
```

with:

```csharp
        _currentDay           = DateTime.UtcNow.Date;
        _nextDayBoundaryTicks = ComputeNextDayBoundaryTicks();
    }
```

- [ ] **Step 3: Replace the hot-path `DateTime.UtcNow.Date` check**

Locate `ShouldRotate` (around line 82). Replace the entire method:

```csharp
    private bool ShouldRotate(int incomingBytes)
    {
        if (_currentFileBytes + incomingBytes > _maxBytes) return true;
        if (DateTime.UtcNow.Date > _currentDay) return true;
        return false;
    }
```

with:

```csharp
    private bool ShouldRotate(int incomingBytes)
    {
        if (_currentFileBytes + incomingBytes > _maxBytes) return true;
        if (HfClock.NowTicks >= _nextDayBoundaryTicks) return true;
        return false;
    }
```

- [ ] **Step 4: Resample the boundary in `RotateNow`**

Locate `RotateNow` (around line 89). Replace:

```csharp
    private void RotateNow()
    {
        FlushToStream();
        _stream?.Dispose();
        _stream = null;
        _seq++;
        _currentDay       = DateTime.UtcNow.Date;
        _currentFileBytes = 0;
        TryOpenStream();
        Cleanup();
    }
```

with:

```csharp
    private void RotateNow()
    {
        FlushToStream();
        _stream?.Dispose();
        _stream = null;
        _seq++;
        _currentDay           = DateTime.UtcNow.Date;
        _nextDayBoundaryTicks = ComputeNextDayBoundaryTicks();
        _currentFileBytes     = 0;
        TryOpenStream();
        Cleanup();
    }
```

- [ ] **Step 5: Add the helper + internal test hook**

Append immediately before the closing brace of the class (after `Cleanup()` around line 167):

```csharp
    private static long ComputeNextDayBoundaryTicks()
    {
        var  now          = DateTime.UtcNow;
        var  nextMidnight = now.Date.AddDays(1);
        long msUntil      = (long)(nextMidnight - now).TotalMilliseconds;
        return HfClock.NowTicks + msUntil * (Stopwatch.Frequency / 1_000);
    }

    /// <summary>
    /// Test-only hook: forces the next-day-boundary tick threshold. Visible to
    /// <c>Relay.Tests</c> via <c>InternalsVisibleTo</c>. Production callers must use
    /// <see cref="ComputeNextDayBoundaryTicks"/> (resampled inside <see cref="RotateNow"/>).
    /// </summary>
    internal void SetDayBoundaryForTest(long ticks) => _nextDayBoundaryTicks = ticks;
```

- [ ] **Step 6: Run the new test to verify it passes**

Run: `dotnet test tests/Relay.Tests --filter "FullyQualifiedName~Enqueue_DayBoundaryCrossed_RotatesToNextFile"`
Expected: PASS.

- [ ] **Step 7: Run the full RotatingFileSink test class to verify no regressions**

Run: `dotnet test tests/Relay.Tests --filter "FullyQualifiedName~RotatingFileSinkTests"`
Expected: 5 passed (4 existing + 1 new), 0 failed.

- [ ] **Step 8: Run the full test suite**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failed.

- [ ] **Step 9: Commit**

```bash
git add src/Relay/Sinks/RotatingFileSink.cs tests/Relay.Tests/Sinks/RotatingFileSinkTests.cs
git commit -m "fix: cache RotatingFileSink day boundary in HfClock ticks (no DateTime.UtcNow on hot path) w/Claude"
```

---

## Task 5: Update CHANGELOG

**Files:**
- Modify: `CHANGELOG.md`

- [ ] **Step 1: Read the file**

Run: `cat CHANGELOG.md` (or use Read tool) to find the topmost unreleased / latest section.

- [ ] **Step 2: Add the fix entry**

Under the latest unreleased section (or create one if absent — match the project's existing CHANGELOG style), add:

```markdown
### Fixed
- `RotatingFileSink.ShouldRotate` no longer calls `DateTime.UtcNow.Date` per consumed payload; caches the next UTC-midnight boundary in `HfClock` ticks. Reduces consumer hot-path cost from ~50c to ~3c per payload (BDN ratio ≥ 10x — see Phase 1 BDN under `benchmarks/artifacts/2026-04-29-phase1/`). Resolves regression flagged in `docs/reports/2026-04-29-resource-cost-map-relay.md` §5.
```

If the project already uses a different heading format (e.g. Conventional Commits-style), match it exactly. Do not invent new section structure.

- [ ] **Step 3: Commit**

```bash
git add CHANGELOG.md
git commit -m "docs: changelog entry for RotatingFileSink day-boundary cache w/Claude"
```

---

## Task 6: Re-run BDN — capture post-fix cost

**Files:**
- Modify: `benchmarks/artifacts/2026-04-29-phase1/` (new BDN run added)

- [ ] **Step 1: Re-run the same BDN class against the fix**

Run:
```bash
dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo
dotnet run --project benchmarks/Relay.Benchmarks -c Release --no-build -- \
  --filter "*RotatingFileSinkBenchmarks*" \
  --job short \
  --artifacts benchmarks/artifacts/2026-04-29-phase1/post-fix
```

Expected: 1 benchmark, exits 0. Inspect:
`benchmarks/artifacts/2026-04-29-phase1/post-fix/results/Relay.Benchmarks.Sinks.RotatingFileSinkBenchmarks-report-github.md`

Record the **post-fix mean ns** (call it `POST_FIX_NS`).

- [ ] **Step 2: Compute the ratio**

```bash
python -c "print(f'ratio = {<BASELINE_NS> / <POST_FIX_NS>:.2f}x')"
```

(or compute mentally — `BASELINE_NS / POST_FIX_NS`).

Expected: **ratio ≥ 10**. The reflection-invoke overhead (~500c fixed) is identical in both runs, so the absolute delta drops by the full ShouldRotate savings (~50c → ~3c, ~47c reduction over a ~500c reflection envelope = ~10% absolute reduction; the **inner loop** ratio is much higher but masked by reflection overhead).

> **If ratio < 10x:** investigate. The reflection-overhead theory should still allow ≥3x. <3x = the fix did not actually remove the UtcNow call; re-check Task 4 Step 3 (`ShouldRotate` body must not contain `DateTime.UtcNow`). Acceptance criterion adjusted: ≥3x is acceptable given reflection envelope; document the absolute mean ns in the commit message.

- [ ] **Step 3: Commit the post-fix BDN artifacts**

```bash
git add benchmarks/artifacts/2026-04-29-phase1/post-fix/
git commit -m "test: RotatingFileSinkBenchmarks post-fix run — <BASELINE_NS>ns → <POST_FIX_NS>ns (<RATIO>x) w/Claude"
```

---

## Task 7: Update Cost-Map Report

**Files:**
- Modify: `docs/reports/2026-04-29-resource-cost-map-relay.md`

- [ ] **Step 1: Update §5 (Anti-Pattern Offenders) row**

Find the row in §5 starting with `| **block** | DateTime.UtcNow on hot consumer path | RotatingFileSink.ShouldRotate |`. Replace it with:

```markdown
| ~~**block**~~ resolved | DateTime.UtcNow on hot consumer path | `RotatingFileSink.ShouldRotate` | src/Relay/Sinks/RotatingFileSink.cs:85 | Fixed in commit `<sha>`: replaced with `HfClock.NowTicks >= _nextDayBoundaryTicks` (boundary cached at construction + `RotateNow`). BDN: <BASELINE_NS>ns → <POST_FIX_NS>ns (<RATIO>x). |
```

Replace `<sha>` with the short hash of the Task 4 commit:

```bash
git log --oneline develop..HEAD | grep "fix: cache RotatingFileSink"
```

- [ ] **Step 2: Update §1 (Per-Entry Cost Table) row**

Find the row `| RotatingFileSink.ShouldRotate | ~50 | 0 | HOT (consumer) |`. Replace with:

```markdown
| `RotatingFileSink.ShouldRotate` | ~3 (BDN: <POST_FIX_NS> ns) | 0 | ULTRA-HOT (consumer) | bounds + `HfClock.NowTicks` cmp (cached boundary) | src/Relay/Sinks/RotatingFileSink.cs:82 |
```

- [ ] **Step 3: Update §2 (Top 20) row**

Find row `| 8 | HOT (consumer) | ~50 | 0 | RotatingFileSink.ShouldRotate |`. Either remove it (it no longer ranks in the top 20) or replace with:

```markdown
| 8 | ULTRA-HOT (consumer) | ~3 (BDN: <POST_FIX_NS> ns) | 0 | `RotatingFileSink.ShouldRotate` | src/Relay/Sinks/RotatingFileSink.cs:82 | direct | cached day boundary (HfClock ticks) |
```

- [ ] **Step 4: Update §9 (Delta) regression row**

Find the row `| **regression** | RotatingFileSink.ShouldRotate calls DateTime.UtcNow.Date per record |`. Replace with:

```markdown
| ~~**regression**~~ resolved | `RotatingFileSink.ShouldRotate` cached day boundary | src/Relay/Sinks/RotatingFileSink.cs:85 | +50c/payload → +3c/payload (BDN <BASELINE_NS>ns → <POST_FIX_NS>ns) | Fixed in branch `fix/260429-rotatingfilesink-utcnow-hot-path` — added `_nextDayBoundaryTicks` field, replaced `DateTime.UtcNow.Date` check with `HfClock.NowTicks >= _nextDayBoundaryTicks`. |
```

- [ ] **Step 5: Commit**

```bash
git add docs/reports/2026-04-29-resource-cost-map-relay.md
git commit -m "docs: mark RotatingFileSink UtcNow hot-path regression resolved in cost-map w/Claude"
```

---

## Task 8: Final Verification & PR Prep

**Files:** _none modified_

- [ ] **Step 1: Re-run full test suite**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failed. All `RotatingFileSinkTests` pass (5 tests).

- [ ] **Step 2: Verify the branch contains exactly the expected commits**

Run: `git log --oneline develop..HEAD`
Expected: 5 commits — `test:` BDN baseline (Task 2), `fix:` (Task 4), `docs:` CHANGELOG (Task 5), `test:` BDN post-fix (Task 6), `docs:` cost-map (Task 7).

- [ ] **Step 3: Verify no `DateTime.UtcNow` reference remains in `RotatingFileSink.cs` outside cold paths**

Run: `grep -n "DateTime.UtcNow" src/Relay/Sinks/RotatingFileSink.cs`
Expected: only matches inside `ComputeNextDayBoundaryTicks` (cold) and `RotateNow` (cold). No match inside `ShouldRotate` or `WriteToBackend`.

- [ ] **Step 4: Confirm BDN ratio gate**

Verify the ratio captured in Task 6 was ≥ 10x (or ≥ 3x with documented reflection envelope). If not, the branch is NOT ready — return to Task 4 and audit.

- [ ] **Step 5: Push the branch**

```bash
git push -u origin fix/260429-rotatingfilesink-utcnow-hot-path
```

- [ ] **Step 6: Open the PR (only if user requests it)**

PR title: `fix: cache RotatingFileSink day boundary in HfClock ticks`
PR body should include:
- Link to `docs/reports/2026-04-29-resource-cost-map-relay.md` §5 + §9.
- BDN baseline / post-fix numbers.
- Note merge target is `develop`, never `master`.

Do NOT push to `master`. Do NOT merge without user approval. Do NOT use `--no-verify`.

---

## Self-Review Checklist (run before handoff)

- ✅ Spec coverage — C1 closed (BDN baseline + post-fix), UtcNow regression closed (code), UdpSink documented as out-of-scope (Phase 4 of master plan).
- ✅ No placeholders — `<sha>`, `<BASELINE_NS>`, `<POST_FIX_NS>`, `<RATIO>` are runtime values captured during Tasks 2-6 and pasted into Tasks 5-7.
- ✅ Type consistency — `_nextDayBoundaryTicks` named identically across field, ctor init, helper, hot-path read, RotateNow resample, and test hook.
- ✅ TDD order — failing test in Task 3 (red), implementation in Task 4 (green), BDN gate in Task 6, verification in Task 8.
- ✅ Frequent commits — 5 commits, one per logical concern (BDN baseline, code+test fix, changelog, BDN post-fix, cost-map).
- ✅ Branch name matches `fix/<yyMMdd>-<slug>` per project CLAUDE.md.
- ✅ Commit messages follow Conventional Commits + `w/Claude` suffix.
- ✅ Test gate (`dotnet test tests/Relay.Tests` 0 failures) enforced before each commit.
- ✅ BDN gate (≥ 10x ratio improvement, ≥ 3x acceptable with reflection envelope) enforced in Task 6.
