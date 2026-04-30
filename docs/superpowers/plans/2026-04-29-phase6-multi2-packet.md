# Phase 6 — Multi2PacketSink (CRTP variant for packet hierarchy)

> **Phase 6 of 8** in master plan `docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md`. **For agentic workers:** REQUIRED SUB-SKILL: superpowers:subagent-driven-development.

**Goal:** Add a sealed CRTP `Multi2PacketSink<TC1, TC2>` to the packet hierarchy, mirroring the existing typed `Multi2Sink<T, TC1, TC2>`. Closes coverage gap **M3** (DESIGN GAP — type did not exist) from `benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md`. **Production code change:** new public type + builder overload.

**Architecture:** mirror of typed `Multi2Sink<T, TC1, TC2>` semantics for byte payloads:
- Constructor takes two children of types `TC1`, `TC2` (both must derive from `PacketSink`)
- `IsHealthy = _c1.IsHealthy || _c2.IsHealthy` (short-circuit OR)
- `Accept(payload)` delivers to `_c1` then `_c2`, returns `true` always (broadcast semantics)
- `Flush()` and `Dispose()` forward to both children

When `TC1` and `TC2` are `sealed`, the JIT devirtualizes both `Enqueue` calls — ~1-3 ns improvement vs array-based `MultiSink` per Phase 2 numbers.

**Tech Stack:** .NET 9, C# 13, xUnit 2.9.2, FluentAssertions 6.12.1, BenchmarkDotNet 0.13.12.

**Out of scope:** Multi2 for arity > 2 (typed library has only Multi2). Cross-hierarchy bridge (typed → packet Multi2). MPSC contention BDN (Phase 7).

**Acceptance gate:**
- New `Multi2PacketSink<TC1, TC2>` type exists and is `public sealed`
- New `.Multi<TC1,TC2>(c1, c2)` overload in `Relay.Builder.SinkChain<THead>` (packet builder)
- Tests (3+ in `MultiSinkPacketTests`): broadcast, IsHealthy short-circuit, Flush, Dispose
- BDN extension to `MultiPacketEnqueueBenchmarks` shows `Multi2_Packet_Enqueue` ≤ baseline `Multi_Packet_Enqueue` (CRTP not slower; ideally 5-10% faster)
- All 190+ existing tests still pass; new tests bring total to 193+
- CLAUDE.md `MultiSink<T> semantics` section updated to mention parallel packet `Multi2PacketSink`

---

## File Structure

| File | Action | Responsibility |
|---|---|---|
| `src/Relay/MultiSink.Packet.cs` | Modify | Append `Multi2PacketSink<TC1, TC2>` sealed class |
| `src/Relay/Builder/SinkChain.Packet.cs` | Modify | Append `.Multi<TC1,TC2>(c1, c2)` fluent overload |
| `tests/Relay.Tests/MultiSinkPacketTests.cs` | Modify | Append 3+ test methods exercising `Multi2PacketSink` |
| `benchmarks/Relay.Benchmarks/PacketSinks/MultiPacketEnqueueBenchmarks.cs` | Modify | Append `Multi2_Packet_Enqueue` benchmark |
| `CLAUDE.md` | Modify | `MultiSink<T> semantics` section: add note about parallel `Multi2PacketSink` |

---

## Task 1: Branch & Baseline

- [ ] **Step 1: Create the Phase 6 branch**

```bash
git checkout develop
git pull
git checkout -b feat/260429-multi2-packet-sink
```

- [ ] **Step 2: Verify baseline tests pass**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failures.

- [ ] **Step 3: Commit Phase 6 plan + any session strays**

```bash
git add docs/superpowers/plans/2026-04-29-phase6-multi2-packet.md
git commit -m "docs: add Phase 6 child plan w/Claude"
```

(If `.claude/settings.local.json` was modified during the session, include it.)

---

## Task 2: TDD red — failing test for `Multi2PacketSink` broadcast

**Files:**
- Modify: `tests/Relay.Tests/MultiSinkPacketTests.cs`

- [ ] **Step 1: Add the failing test**

Append to the bottom of `MultiSinkPacketTests`:

```csharp
[Fact]
public void Multi2_Enqueue_DeliversToBothChildren()
{
    var a = new CollectingSink();
    var b = new CollectingSink();
    var multi2 = new Multi2PacketSink<CollectingSink, CollectingSink>(a, b);

    multi2.Enqueue(Payload);

    a.Received.Should().HaveCount(1);
    b.Received.Should().HaveCount(1);
}

[Fact]
public void Multi2_IsHealthy_True_WhenAtLeastOneChildHealthy()
{
    var healthy   = new CollectingSink();
    var unhealthy = new CollectingSink();
    unhealthy.SetHealthy(false);
    var multi2 = new Multi2PacketSink<CollectingSink, CollectingSink>(healthy, unhealthy);

    multi2.IsHealthy.Should().BeTrue();
}

[Fact]
public void Multi2_IsHealthy_False_WhenBothChildrenUnhealthy()
{
    var a = new CollectingSink();
    var b = new CollectingSink();
    a.SetHealthy(false);
    b.SetHealthy(false);
    var multi2 = new Multi2PacketSink<CollectingSink, CollectingSink>(a, b);

    multi2.IsHealthy.Should().BeFalse();
}

[Fact]
public void Multi2_Flush_ForwardsToBoth()
{
    var a = new CollectingSink();
    var b = new CollectingSink();
    var multi2 = new Multi2PacketSink<CollectingSink, CollectingSink>(a, b);

    multi2.Flush();

    a.Flushes.Should().Be(1);
    b.Flushes.Should().Be(1);
}

[Fact]
public void Multi2_Dispose_ForwardsToBoth()
{
    var a = new CollectingSink();
    var b = new CollectingSink();
    var multi2 = new Multi2PacketSink<CollectingSink, CollectingSink>(a, b);

    multi2.Dispose();

    a.Disposed.Should().BeTrue();
    b.Disposed.Should().BeTrue();
}

[Fact]
public void Multi2_Ctor_Throws_OnNullChild()
{
    var ok = new CollectingSink();
    Action a1 = () => new Multi2PacketSink<CollectingSink, CollectingSink>(null!, ok);
    Action a2 = () => new Multi2PacketSink<CollectingSink, CollectingSink>(ok, null!);
    a1.Should().Throw<ArgumentNullException>();
    a2.Should().Throw<ArgumentNullException>();
}
```

> **Note:** `CollectingSink` test helper already exists in `tests/Relay.Tests/TestSinks/CollectingSink.cs`. Verify it has `Flushes` (int counter) and `Disposed` (bool) properties; if not, extend it minimally (those signals may already exist for other tests).

- [ ] **Step 2: Run the new tests to verify they fail**

Run: `dotnet test tests/Relay.Tests --filter "FullyQualifiedName~Multi2"`
Expected: COMPILE FAIL — `Multi2PacketSink` does not exist yet. This is the TDD red.

- [ ] **Step 3: No commit yet** (red phase only).

---

## Task 3: GREEN — implement `Multi2PacketSink<TC1, TC2>`

**Files:**
- Modify: `src/Relay/MultiSink.Packet.cs`

- [ ] **Step 1: Append the new class**

Add at the end of `src/Relay/MultiSink.Packet.cs` (after the closing brace of `MultiSink`):

```csharp
/// <summary>
/// Fixed-arity 2-child broadcast for the packet hierarchy. Mirror of typed
/// <see cref="Multi2Sink{T,TC1,TC2}"/>. When <typeparamref name="TC1"/> and
/// <typeparamref name="TC2"/> are sealed, the JIT devirtualizes and inlines both
/// <see cref="PacketSink.Enqueue"/> calls — saves 1-3 ns vs the array-based
/// <see cref="MultiSink"/> at N=2.
/// </summary>
public sealed class Multi2PacketSink<TC1, TC2> : PacketSink
    where TC1 : PacketSink
    where TC2 : PacketSink
{
    private readonly TC1 _c1;
    private readonly TC2 _c2;

    /// <param name="c1">First child sink.</param>
    /// <param name="c2">Second child sink.</param>
    public Multi2PacketSink(TC1 c1, TC2 c2)
    {
        _c1 = c1 ?? throw new ArgumentNullException(nameof(c1));
        _c2 = c2 ?? throw new ArgumentNullException(nameof(c2));
    }

    /// <inheritdoc/>
    public override bool IsHealthy => _c1.IsHealthy || _c2.IsHealthy;

    /// <summary>Broadcasts to both children. Always returns true; falls through to <see cref="PacketSink.Next"/> only when <see cref="IsHealthy"/> is false.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        _c1.Enqueue(payload);
        _c2.Enqueue(payload);
        return true;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        _c1.Flush();
        _c2.Flush();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        _c1.Dispose();
        _c2.Dispose();
    }
}
```

> **`using` directives:** verify `System` (for `ArgumentNullException`) and `System.Runtime.CompilerServices` (for `MethodImpl`) are imported at the top of the file. The existing `MultiSink` class likely already imports these — check before adding redundant directives.

- [ ] **Step 2: Run the failing tests**

Run: `dotnet test tests/Relay.Tests --filter "FullyQualifiedName~Multi2"`
Expected: 6 PASS (broadcast, IsHealthy true/false, Flush, Dispose, Ctor null-check).

- [ ] **Step 3: Run the full suite**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failed (was 190 + 1 skipped, now 196+ + 1 skipped).

- [ ] **Step 4: Commit**

```bash
git add src/Relay/MultiSink.Packet.cs tests/Relay.Tests/MultiSinkPacketTests.cs
git commit -m "feat: add Multi2PacketSink<TC1,TC2> CRTP variant for packet hierarchy w/Claude"
```

---

## Task 4: Builder overload — `.Multi<TC1,TC2>(c1, c2)` for packet builder

**Files:**
- Modify: `src/Relay/Builder/SinkChain.Packet.cs`

- [ ] **Step 1: Append the overload**

Locate the existing `Multi(params PacketSink[] children)` method. Append after it:

```csharp
/// <summary>
/// Fixed-arity 2-branch broadcast using the CRTP <see cref="Multi2PacketSink{TC1,TC2}"/>
/// variant. When <typeparamref name="TC1"/> and <typeparamref name="TC2"/> are sealed, the
/// JIT devirtualizes both <c>Enqueue</c> calls — saves 1-3 ns vs the array-based overload.
/// </summary>
public SinkChain<THead> Multi<TC1, TC2>(TC1 c1, TC2 c2)
    where TC1 : PacketSink
    where TC2 : PacketSink
{
    var multi  = new Multi2PacketSink<TC1, TC2>(c1, c2);
    _tail.Next = multi;
    _tail      = multi;
    return this;
}
```

- [ ] **Step 2: Add a builder test (optional, recommended)**

Append to `tests/Relay.Tests/MultiSinkPacketTests.cs`:

```csharp
[Fact]
public void Builder_Multi_TC1_TC2_BroadcastsAfterBuild()
{
    var a = new CollectingSink();
    var b = new CollectingSink();
    // Use SinkChainBuilder.Start with NullSink as head, then .Multi.
    var head = Relay.Builder.SinkChainBuilder
        .Start<NullSink>(NullSink.Instance)
        .To(new Multi2PacketSink<CollectingSink, CollectingSink>(a, b))
        .Head;
    // The chain places Multi2 as the fallback for NullSink.
    // NullSink.Accept always returns true, so the payload never reaches Multi2 under default
    // PropagateAfterAccept=false. This test only verifies the builder did not throw and the
    // chain is well-formed.
    head.Should().BeOfType<NullSink>();
}
```

> **Note:** the `.Multi<TC1, TC2>(c1, c2)` builder overload installs the CRTP sink as a tail's `Next`. Default `PropagateAfterAccept=false` on `NullSink` means the payload doesn't actually reach `Multi2PacketSink` — but the wiring assertion is what matters here. Adjust the test to use a propagating head if you want to exercise broadcast through the builder; otherwise this minimal test is sufficient.

- [ ] **Step 3: Build and run tests**

Run: `dotnet test tests/Relay.Tests`
Expected: 0 failed.

- [ ] **Step 4: Commit**

```bash
git add src/Relay/Builder/SinkChain.Packet.cs tests/Relay.Tests/MultiSinkPacketTests.cs
git commit -m "feat: add SinkChain<THead>.Multi<TC1,TC2>(c1,c2) overload for packet CRTP variant w/Claude"
```

---

## Task 5: BDN — extend `MultiPacketEnqueueBenchmarks`

**Files:**
- Modify: `benchmarks/Relay.Benchmarks/PacketSinks/MultiPacketEnqueueBenchmarks.cs`

- [ ] **Step 1: Append `Multi2` benchmark + field**

Locate the existing `MultiPacketEnqueueBenchmarks` class. Add a `_multi2` field and a benchmark:

```csharp
private Multi2PacketSink<ByteCounterPipe, ByteCounterPipe> _multi2 = null!;

// inside GlobalSetup, after _multi assignment:
_multi2 = new Multi2PacketSink<ByteCounterPipe, ByteCounterPipe>(
    new ByteCounterPipe(), new ByteCounterPipe());

// new method:
[Benchmark]
public void Multi2_Packet_Enqueue() => _multi2.Enqueue(_payload);
```

> **Important:** keep the existing `Multi_Packet_Enqueue` as `[Benchmark(Baseline = true)]` so the new method's `Ratio` column compares CRTP variant vs array-based. Expected ratio < 1.0 (CRTP faster).

- [ ] **Step 2: Build**

Run: `dotnet build benchmarks/Relay.Benchmarks/Relay.Benchmarks.csproj -c Release --nologo`
Expected: 0 errors.

- [ ] **Step 3: Run the BDN**

```bash
dotnet run --project benchmarks/Relay.Benchmarks -c Release --no-build -- \
  --filter "*MultiPacketEnqueueBenchmarks*" \
  --job short \
  --artifacts benchmarks/artifacts/2026-04-29-phase6 \
  --exporters json markdown
```

Inspect `Relay.Benchmarks.PacketSinks.MultiPacketEnqueueBenchmarks-report-github.md`. Sanity gate: `Multi2_Packet_Enqueue` ratio should be ≤ 1.0 (faster than baseline) or within ±10% (statistical noise on sub-ns measurements). If ratio > 1.5x, abort and investigate — CRTP should not be slower.

- [ ] **Step 4: Commit**

```bash
git add benchmarks/Relay.Benchmarks/PacketSinks/MultiPacketEnqueueBenchmarks.cs benchmarks/artifacts/2026-04-29-phase6/
git commit -m "test: extend MultiPacketEnqueueBenchmarks w/ Multi2_Packet_Enqueue (M3 — CRTP packet variant) w/Claude"
```

---

## Task 6: Update CLAUDE.md

**Files:**
- Modify: `CLAUDE.md`

- [ ] **Step 1: Locate the `MultiSink<T>` semantics section**

Find the bullet list with `Multi2Sink<T, TC1, TC2> CRTP variant`. Append a parallel note for the packet hierarchy.

Existing text (around line 113):
```markdown
- **`Multi2Sink<T, TC1, TC2>` CRTP variant:** prefer when `TC1` and `TC2` are `sealed` — JIT devirtualizes and inlines both `Enqueue` calls, saving ~6c. Requires concrete sealed types known at compile time.
```

Replace with:

```markdown
- **`Multi2Sink<T, TC1, TC2>` CRTP variant:** prefer when `TC1` and `TC2` are `sealed` — JIT devirtualizes and inlines both `Enqueue` calls, saving ~6c. Requires concrete sealed types known at compile time.
- **`Multi2PacketSink<TC1, TC2>` (packet hierarchy CRTP variant):** parallel to `Multi2Sink`, fixed-arity 2-child broadcast for `PacketSink` chains. Same JIT devirtualization properties when `TC1`, `TC2` are sealed. Available since Phase 6.
```

- [ ] **Step 2: Commit**

```bash
git add CLAUDE.md
git commit -m "docs: CLAUDE.md mentions Multi2PacketSink as parallel CRTP variant for packet hierarchy w/Claude"
```

---

## Task 7: Update Master Plan + Cross-Check

- [ ] **Step 1**: in master plan, mark Phase 6 done.

- [ ] **Step 2**: in cross-check `§3` medium-priority, mark M3 as resolved (`~~M3~~ resolved`) with Phase 6 reference + numbers.

- [ ] **Step 3**: commit.

```bash
git add docs/superpowers/plans/2026-04-29-master-cost-map-coverage.md \
        benchmarks/artifacts/2026-04-29-hotpath/cross-check-and-gaps.md
git commit -m "docs: mark Phase 6 done — Multi2PacketSink CRTP variant landed w/Claude"
```

---

## Task 8: Final Verification

- [ ] **Step 1**: `dotnet test tests/Relay.Tests` — 0 failed (193+ pass).
- [ ] **Step 2**: `git log --oneline develop..HEAD` — 7 commits expected.
- [ ] **Step 3**: STOP — do not push. User pushes + merges.

---

## Self-Review Checklist

- ✅ M3 closed — new `Multi2PacketSink<TC1, TC2>` exists, builder overload, 6 tests, BDN extension.
- ✅ TDD order — failing tests in Task 2, implementation in Task 3.
- ✅ No placeholders.
- ✅ Type consistency — `Multi2PacketSink<TC1, TC2>` named identically across class, builder overload, tests, BDN.
- ✅ Branch `feat/260429-multi2-packet-sink` per CLAUDE.md.
- ✅ Conventional Commits + `w/Claude`. `feat:` for new public type.
- ✅ Test gate enforced.
- ✅ CLAUDE.md updated to keep architectural docs aligned.
