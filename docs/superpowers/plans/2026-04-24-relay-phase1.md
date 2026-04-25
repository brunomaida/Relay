# Relay Fase 1 — PacketSink Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the complete PacketSink hierarchy in Relay — renames, bug fixes, infrastructure sinks, builder, and concrete transport sinks — with zero performance regression versus the typed-sink baseline.

**Architecture:** PacketSink replaces ByteSink as the variable-length byte dispatch base. Camada 1 (Tasks 1–10) delivers infrastructure with no concrete I/O backends; Camada 2 (Tasks 11–15) delivers TcpSink, UdpSink, FileSink, RamSink gated by BDN benchmarks. Each task commits after all tests pass.

**Tech Stack:** .NET 9, C# 13, xUnit 2.9.2, FluentAssertions 6.12.1, BenchmarkDotNet

**Branch:** `feature/260424-packet-sink-phase1` off `develop`

**Spec:** `docs/superpowers/specs/2026-04-24-relay-phase1-design.md`

---

## File Map

### Renamed (old deleted, new created)
| Old | New | Class |
|---|---|---|
| `src/Relay/ByteSink.cs` | `src/Relay/PacketSink.cs` | `PacketSink` |
| `src/Relay/SpscByteQueueSink.cs` | `src/Relay/SpscQueueSink.Packet.cs` | `SpscQueueSink` |
| `src/Relay/MpscByteQueueSink.cs` | `src/Relay/MpscQueueSink.Packet.cs` | `MpscQueueSink` |
| `src/Relay/NullByteSink.cs` | `src/Relay/NullSink.Packet.cs` | `NullSink` |

### Created — src
| File | Class |
|---|---|
| `src/Relay/ForkSink.Packet.cs` | `ForkSink` (non-generic) |
| `src/Relay/MultiSink.Packet.cs` | `MultiSink` (non-generic) |
| `src/Relay/FilterSink.Packet.cs` | `FilterSink`, `PacketPredicate` |
| `src/Relay/SerializeSink.cs` | `SerializeSink<T>` |
| `src/Relay/Builder/SinkChainBuilder.cs` | `SinkChainBuilder` |
| `src/Relay/Builder/SinkChain.Packet.cs` | `SinkChain<THead>` |
| `src/Relay/Builder/FilterBinding.Packet.cs` | `FilterBinding<THead>` |
| `src/Relay/Sinks/TcpSink.cs` | `TcpSink` |
| `src/Relay/Sinks/UdpSink.cs` | `UdpSink` |
| `src/Relay/Sinks/FileSink.cs` | `FileSink` |
| `src/Relay/Sinks/RamSink.cs` | `RamSink` |

### Created — tests
| File | Covers |
|---|---|
| `tests/Relay.Tests/TestSinks/CollectingSink.cs` | Collects payloads, configurable health |
| `tests/Relay.Tests/TestSinks/TrackingSpscSink.cs` | Tracks which thread calls consumer methods |
| `tests/Relay.Tests/SpscQueueSinkFlushTests.cs` | Flush cross-thread bug fix |
| `tests/Relay.Tests/PacketSinkChainTests.cs` | Chain, fallback, PropagateAfterAccept |
| `tests/Relay.Tests/ForkSinkPacketTests.cs` | Tee, fallback, Flush/Dispose propagation |
| `tests/Relay.Tests/MultiSinkPacketTests.cs` | Broadcast, IsHealthy OR, Next fallback |
| `tests/Relay.Tests/FilterSinkPacketTests.cs` | Predicate gate, drop, isolation |
| `tests/Relay.Tests/SerializeSinkTests.cs` | MemoryMarshal correctness, zero alloc |
| `tests/Relay.Tests/SinkChainBuilderTests.cs` | To/Fork/When/Multi wiring |
| `tests/Relay.Tests/TcpSinkTests.cs` | Framing, delivery, recovery, dispose |
| `tests/Relay.Tests/UdpSinkTests.cs` | Datagram, maxPayload enforcement, recovery |
| `tests/Relay.Tests/FileSinkTests.cs` | Header, accumulation, append-recovery, dispose |
| `tests/Relay.Tests/RamSinkTests.cs` | Capacity, DrainTo order, partial drain, Dispose |

### Created — benchmarks
| File | Measures |
|---|---|
| `benchmarks/Relay.Benchmarks/Baselines/TypedSinkBaselineBenchmark.cs` | TcpSink\<T\>, FileStreamSink\<T\> throughput |
| `benchmarks/Relay.Benchmarks/PacketSinks/TcpSinkBenchmark.cs` | TcpSink throughput |
| `benchmarks/Relay.Benchmarks/PacketSinks/FileSinkBenchmark.cs` | FileSink throughput |
| `benchmarks/Relay.Benchmarks/PacketSinks/ChainBenchmark.cs` | Fan-out, fallback, SerializeSink overhead |

---

## Task 1: Branch + BDN baseline

**Files:**
- Create: `benchmarks/Relay.Benchmarks/Baselines/TypedSinkBaselineBenchmark.cs`
- Create: `benchmarks/baselines/.gitkeep`

- [ ] **Step 1: Create feature branch**

```bash
git checkout develop
git pull
git checkout -b feature/260424-packet-sink-phase1
```

- [ ] **Step 2: Create baseline benchmark**

`benchmarks/Relay.Benchmarks/Baselines/TypedSinkBaselineBenchmark.cs`:

```csharp
using System;
using System.Net;
using System.Net.Sockets;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.Baselines;

/// <summary>Baseline: typed sink throughput before PacketSink changes.</summary>
[MemoryDiagnoser]
public class TypedSinkBaselineBenchmark
{
    private TcpListener?        _listener;
    private TcpSink<Event64>?   _tcpSink;
    private Event64             _event;

    [GlobalSetup]
    public void Setup()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        int port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _listener.AcceptTcpClientAsync(); // accept in background, discard

        _tcpSink = new TcpSink<Event64>("127.0.0.1", port);
        _tcpSink.Start();
        _event = new Event64 { Value = 42L };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _tcpSink?.Stop(1_000);
        _listener?.Stop();
    }

    [Benchmark(Baseline = true)]
    public void TcpSinkTyped_Enqueue() => _tcpSink!.Enqueue(in _event);

    // 64-byte unmanaged struct satisfying SinkConstraints.AssertCacheLineAligned<T>().
    [System.Runtime.InteropServices.StructLayout(
        System.Runtime.InteropServices.LayoutKind.Sequential, Size = 64)]
    private struct Event64 { public long Value; }
}
```

- [ ] **Step 3: Create baselines output directory**

```bash
mkdir -p benchmarks/baselines
touch benchmarks/baselines/.gitkeep
```

- [ ] **Step 4: Run baseline and save output**

```bash
cd benchmarks/Relay.Benchmarks
dotnet run -c Release -- --filter "*Baseline*" --exporters json \
  --artifacts ../../benchmarks/baselines
```

Save the generated JSON as `benchmarks/baselines/2026-04-24-before-phase1.json`.
Expected: `TcpSinkTyped_Enqueue` completes, `Allocated = 0 B`, `Gen0 = 0`.

- [ ] **Step 5: Commit baseline**

```bash
git add benchmarks/
git commit -m "chore: add typed-sink BDN baseline before PacketSink phase1 w/Claude"
```

---

## Task 2: Rename byte hierarchy → packet hierarchy

**Files:**
- Delete: `src/Relay/ByteSink.cs`
- Create: `src/Relay/PacketSink.cs`
- Modify: `src/Relay/SpscByteQueueSink.cs` — base class reference only (full rename in Task 3)
- Modify: `src/Relay/MpscByteQueueSink.cs` — base class reference
- Modify: `src/Relay/NullByteSink.cs` — base class reference
- Modify: `src/Relay/CLAUDE.md` — update type table

- [ ] **Step 1: Create PacketSink.cs**

`src/Relay/PacketSink.cs`:

```csharp
using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Abstract base for a composable fallback dispatch pipeline over variable-length byte payloads.
/// Parallel hierarchy to <see cref="DispatchSink{T}"/>; share no types by design.
/// </summary>
/// <remarks>
/// Payload semantics: the <c>ReadOnlySpan&lt;byte&gt;</c> passed to <see cref="Accept"/> is valid
/// for the duration of the call only. Implementations that buffer must copy before returning.
/// </remarks>
public abstract class PacketSink : IDisposable
{
    /// <summary>Next sink in the fallback chain. Set by builder or test wiring.</summary>
    public PacketSink? Next { get; internal set; }

    /// <summary>
    /// True when this sink can accept payloads. Written exclusively by the consumer thread
    /// (on backend failure or recovery); never written by the producer.
    /// </summary>
    public abstract bool IsHealthy { get; }

    /// <summary>
    /// When true, <see cref="Enqueue"/> continues to <see cref="Next"/> after a successful
    /// local <see cref="Accept"/> — enabling fork/audit patterns.
    /// JIT eliminates the branch in sealed subclasses returning a compile-time constant.
    /// </summary>
    public virtual bool PropagateAfterAccept => false;

    /// <summary>
    /// Routes <paramref name="payload"/>: delivers locally when healthy, then propagates to
    /// <see cref="Next"/> if <see cref="PropagateAfterAccept"/> is true. Falls through to
    /// <see cref="Next"/> on failure, or drops silently when <c>Next == null</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(ReadOnlySpan<byte> payload)
    {
        if (IsHealthy && Accept(payload))
        {
            if (PropagateAfterAccept) Next?.Enqueue(payload);
            return;
        }
        Next?.Enqueue(payload);
    }

    /// <summary>
    /// Attempts to deliver <paramref name="payload"/> to this sink's local buffer or backend.
    /// Returns true on success, false to trigger fallback to <see cref="Next"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract bool Accept(ReadOnlySpan<byte> payload);

    /// <inheritdoc cref="DispatchSink{T}.Flush"/>
    public abstract void Flush();

    /// <inheritdoc/>
    public abstract void Dispose();
}
```

- [ ] **Step 2: Update SpscByteQueueSink, MpscByteQueueSink, NullByteSink base class references**

In each file, change `: ByteSink` to `: PacketSink` and update any `ByteSink?` field/property types to `PacketSink?`. Do NOT rename the classes yet.

`src/Relay/SpscByteQueueSink.cs` — change line 1 of class declaration and `Prev` type:
```csharp
// Before:
public abstract class SpscByteQueueSink : ByteSink
internal ByteSink? Prev { get; set; }
// After:
public abstract class SpscByteQueueSink : PacketSink
internal PacketSink? Prev { get; set; }
```

`src/Relay/MpscByteQueueSink.cs`:
```csharp
// Before:
public abstract class MpscByteQueueSink : ByteSink
internal PacketSink? Prev { get; set; }  // already updated if it referenced ByteSink
// After: same pattern
public abstract class MpscByteQueueSink : PacketSink
```

`src/Relay/NullByteSink.cs`:
```csharp
// Before:
public sealed class NullByteSink : ByteSink
// After:
public sealed class NullByteSink : PacketSink
```

- [ ] **Step 3: Delete ByteSink.cs**

```bash
git rm src/Relay/ByteSink.cs
```

- [ ] **Step 4: Build and test**

```bash
dotnet build src/Relay/Relay.csproj -c Release
dotnet test tests/Relay.Tests --no-build -c Release
```

Expected: 0 build errors, 0 test failures.

- [ ] **Step 5: Commit**

```bash
git add src/Relay/
git commit -m "refactor: rename ByteSink to PacketSink, update derived class bases w/Claude"
```

---

## Task 3: Rename SpscByteQueueSink → SpscQueueSink + fix Flush cross-thread bug

**Files:**
- Delete: `src/Relay/SpscByteQueueSink.cs`
- Create: `src/Relay/SpscQueueSink.Packet.cs`
- Create: `tests/Relay.Tests/SpscQueueSinkFlushTests.cs`

- [ ] **Step 1: Write failing test**

`tests/Relay.Tests/SpscQueueSinkFlushTests.cs`:

```csharp
using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Relay.Tests;

public sealed class SpscQueueSinkFlushTests
{
    [Fact]
    public void Flush_CalledFromExternalThread_DoesNotCallFlushBackendOnCallerThread()
    {
        using var sink = new TrackingSpscSink(ringCapacity: 4_096, flushIntervalMs: 60_000);
        sink.Start();

        int callerThread = Environment.CurrentManagedThreadId;

        sink.Flush();

        // FlushBackend must NOT have been called on the caller's thread yet.
        sink.FlushBackendThreadId.Should().BeNull(
            "Flush() must signal the consumer, not call FlushBackend() directly");

        sink.Stop(drainTimeoutMs: 500);

        // After stop, consumer has run: FlushBackend was called by the consumer thread.
        sink.FlushBackendThreadId.Should().NotBeNull();
        sink.FlushBackendThreadId.Should().NotBe(callerThread,
            "FlushBackend() must run on the consumer thread, not the caller's thread");
    }

    [Fact]
    public void Flush_CalledBeforeStop_ConsumerProcessesRequest()
    {
        using var sink = new TrackingSpscSink(ringCapacity: 4_096, flushIntervalMs: 60_000);
        sink.Start();

        byte[] payload = [1, 2, 3];
        sink.Enqueue(payload);
        sink.Flush();
        sink.Stop(drainTimeoutMs: 1_000);

        sink.WriteCount.Should().Be(1);
        sink.FlushBackendThreadId.Should().NotBeNull();
    }

    // Test-only SpscQueueSink subclass that records which thread calls consumer methods.
    private sealed class TrackingSpscSink : SpscQueueSink
    {
        public int? FlushBackendThreadId { get; private set; }
        public int  WriteCount           { get; private set; }

        public TrackingSpscSink(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(ReadOnlySpan<byte> payload) => WriteCount++;
        protected override void FlushBackend()    => FlushBackendThreadId = Environment.CurrentManagedThreadId;
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }
}
```

- [ ] **Step 2: Run test — expect compile error (SpscQueueSink does not exist yet)**

```bash
dotnet test tests/Relay.Tests --filter "SpscQueueSinkFlushTests" 2>&1 | head -20
```

Expected: build error — `SpscQueueSink` type not found.

- [ ] **Step 3: Create SpscQueueSink.Packet.cs with bug fix**

`src/Relay/SpscQueueSink.Packet.cs`:

```csharp
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Relay.Buffers;
using Relay.Internal;
using Relay.Memory;

namespace Relay;

/// <summary>
/// Abstract base for a <see cref="PacketSink"/> that buffers payloads in a lock-free SPSC ring
/// and delivers them via a dedicated consumer thread. Subclasses implement the backend.
/// </summary>
public abstract class SpscQueueSink : PacketSink
{
    private const int SpinIter  = 10;
    private const int YieldIter = 5;
    private const int SleepMs   = 1;
    private const int BatchSize = 256;

    private readonly SpscByteRingBuffer _ring;
    private readonly long               _flushIntervalTicks;
    private readonly string             _pipeName;

    private Thread?       _thread;
    private volatile bool _running;
    private int           _flushRequested;     // Volatile-signalled by Flush(); read by consumer.
    private Exception?    _consumerException;
    private long          _drainDeadlineTicks;

    /// <summary>
    /// Backend health. Written exclusively by the consumer thread on IOException or recovery.
    /// The producer reads it via <see cref="IsHealthy"/> for short-circuit gating.
    /// </summary>
    protected volatile bool _healthy = true;

    /// <summary>Predecessor in the fallback chain. Wired by <see cref="Builder.SinkChain{THead}"/>.</summary>
    internal PacketSink? Prev { get; set; }

    /// <inheritdoc/>
    public override bool IsHealthy => _healthy;

    /// <summary>False only when the consumer thread exited due to an unhandled exception.</summary>
    public bool IsConsuming => _running && _consumerException is null;

    /// <summary>Non-null when the consumer thread crashed. Read on cold path only.</summary>
    public Exception? ConsumerException => _consumerException;

    /// <param name="ringCapacity">SPSC ring capacity in bytes. Must be a positive power of two.</param>
    /// <param name="flushIntervalMs">Max milliseconds between forced flushes.</param>
    /// <param name="pipeName">Optional thread-name suffix for debugger visibility.</param>
    protected SpscQueueSink(int ringCapacity, int flushIntervalMs, string pipeName = "")
    {
        _ring               = new SpscByteRingBuffer(ringCapacity);
        _flushIntervalTicks = (long)flushIntervalMs * (Stopwatch.Frequency / 1_000);
        _pipeName           = pipeName;
    }

    /// <summary>Pre-faults the ring buffer and starts the consumer thread.</summary>
    public void Start()
    {
        if (_running) return;
        _running = true;
        RelayMemory.PreFaultAndLock(_ring.Buffer);
        _thread = new Thread(ConsumeLoop)
        {
            Name         = string.IsNullOrEmpty(_pipeName) ? "relay-packet" : $"relay-packet-{_pipeName}",
            IsBackground = true,
            Priority     = ThreadPriority.BelowNormal
        };
        _thread.Start();
    }

    /// <summary>Signals the consumer to stop and waits up to <paramref name="drainTimeoutMs"/> ms.</summary>
    public void Stop(int drainTimeoutMs = 5_000)
    {
        if (!_running) return;
        Volatile.Write(ref _drainDeadlineTicks,
            HfClock.NowTicks + (long)drainTimeoutMs * (Stopwatch.Frequency / 1_000));
        _running = false;
        _thread?.Join(TimeSpan.FromMilliseconds(drainTimeoutMs));
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload) => _ring.TryPublish(payload);

    /// <summary>Writes a single payload to the backend. Called exclusively on the consumer thread.</summary>
    protected abstract void WriteToBackend(ReadOnlySpan<byte> payload);

    /// <summary>Flushes pending writes to the backend. Called on the consumer thread.</summary>
    protected abstract void FlushBackend();

    /// <summary>Attempts recovery after a backend failure. Called on the consumer thread.</summary>
    protected abstract void TryRecoverBackend();

    /// <summary>Closes the backend and releases resources. Called in the consumer finally block.</summary>
    protected abstract void DisposeBackend();

    /// <summary>
    /// Signals the consumer thread to flush. Never calls <see cref="FlushBackend"/> directly —
    /// that method is consumer-thread-only.
    /// </summary>
    public override void Flush() => Volatile.Write(ref _flushRequested, 1);

    /// <inheritdoc/>
    public override void Dispose() => Stop();

    private void ConsumeLoop()
    {
        try
        {
            long flushDeadline = HfClock.NowTicks + _flushIntervalTicks;
            int  idleSpin      = 0;

            while (ShouldKeepDraining())
            {
                bool checkDeadline;

                if (_ring.TryPeek(out var payload, out int advance))
                {
                    WriteToBackend(payload);
                    _ring.Advance(advance);
                    idleSpin = 0;

                    int batch = 1;
                    while (batch < BatchSize && _ring.TryPeek(out payload, out advance))
                    {
                        WriteToBackend(payload);
                        _ring.Advance(advance);
                        batch++;
                    }
                    checkDeadline = true;
                }
                else if (_running)
                {
                    if (idleSpin < SpinIter)
                    {
                        Thread.SpinWait(20);
                        checkDeadline = (idleSpin & 0x7) == 0;
                    }
                    else if (idleSpin < SpinIter + YieldIter)
                    {
                        Thread.Yield();
                        checkDeadline = true;
                    }
                    else
                    {
                        Thread.Sleep(SleepMs);
                        checkDeadline = true;
                    }
                    idleSpin++;
                }
                else
                {
                    checkDeadline = true;
                }

                bool flushDue = Volatile.Read(ref _flushRequested) == 1
                             || HfClock.NowTicks >= flushDeadline;
                if (checkDeadline && flushDue)
                {
                    Volatile.Write(ref _flushRequested, 0);
                    FlushBackend();
                    TryRecoverBackend();
                    TryDrainToPrev();
                    flushDeadline = HfClock.NowTicks + _flushIntervalTicks;
                }
            }
        }
        catch (Exception ex)
        {
            _consumerException = ex;
        }
        finally
        {
            FlushBackend();
            DisposeBackend();
        }
    }

    // Drains accumulated payloads back to Prev when it recovers.
    // SPSC caution: Prev.Enqueue is called from this consumer thread. If the original
    // producer resumes feeding Prev concurrently, two threads enter Prev.Accept simultaneously
    // — a narrow race window. Ensure the producer quiesces before drain runs in SPSC scenarios.
    private void TryDrainToPrev()
    {
        if (Prev is not { IsHealthy: true }) return;
        while (_ring.TryPeek(out var payload, out int advance))
        {
            if (!Prev.IsHealthy) break;
            Prev.Enqueue(payload);
            _ring.Advance(advance);
        }
    }

    private bool ShouldKeepDraining()
    {
        if (_running) return true;
        if (_ring.IsEmpty) return false;
        long deadline = Volatile.Read(ref _drainDeadlineTicks);
        return deadline == 0 || HfClock.NowTicks < deadline;
    }
}
```

- [ ] **Step 4: Delete SpscByteQueueSink.cs**

```bash
git rm src/Relay/SpscByteQueueSink.cs
```

- [ ] **Step 5: Run test — expect pass**

```bash
dotnet test tests/Relay.Tests --filter "SpscQueueSinkFlushTests"
```

Expected: 2 passed, 0 failed.

- [ ] **Step 6: Run full test suite**

```bash
dotnet test tests/Relay.Tests
```

Expected: all existing tests pass.

- [ ] **Step 7: Commit**

```bash
git add src/Relay/ tests/Relay.Tests/
git commit -m "refactor: rename SpscByteQueueSink to SpscQueueSink, fix Flush cross-thread bug w/Claude"
```

---

## Task 4: Rename MpscQueueSink + NullSink

**Files:**
- Delete: `src/Relay/MpscByteQueueSink.cs`, `src/Relay/NullByteSink.cs`
- Create: `src/Relay/MpscQueueSink.Packet.cs`, `src/Relay/NullSink.Packet.cs`

- [ ] **Step 1: Create MpscQueueSink.Packet.cs**

Copy `src/Relay/MpscByteQueueSink.cs` content. Change:
- Class: `MpscByteQueueSink` → `MpscQueueSink`
- Base: already `PacketSink` from Task 2
- `Prev` type: `PacketSink?`
- Thread name: `"relay-packet-mpsc"` prefix
- Apply the same `_flushRequested` fix from Task 3 (same pattern as `SpscQueueSink`)

`src/Relay/MpscQueueSink.Packet.cs` — key differences from SpscQueueSink:
```csharp
namespace Relay;

/// <summary>Abstract base for a PacketSink backed by an MPSC ring buffer and consumer thread.</summary>
public abstract class MpscQueueSink : PacketSink
{
    // Same structure as SpscQueueSink but using MpscByteRingBuffer.
    // _flushRequested fix applies identically.
    // Flush() => Volatile.Write(ref _flushRequested, 1)
    // ConsumeLoop checks flushDue = Volatile.Read(ref _flushRequested) == 1 || deadline elapsed

    private readonly MpscByteRingBuffer _ring;
    // ... identical consumer loop structure to SpscQueueSink
    // Only difference: ring type and TryConsumeBatch API
}
```

Full implementation follows the exact same pattern as `SpscQueueSink` with `MpscByteRingBuffer`
substituted for `SpscByteRingBuffer`. `TryPeek`/`Advance` calls replaced by `MpscByteRingBuffer`
equivalents. Replicate the full `ConsumeLoop` from Task 3 with these substitutions.

- [ ] **Step 2: Create NullSink.Packet.cs**

`src/Relay/NullSink.Packet.cs`:

```csharp
namespace Relay;

/// <summary>No-op <see cref="PacketSink"/> terminal. Always healthy; silently discards payloads.</summary>
public sealed class NullSink : PacketSink
{
    /// <summary>Shared singleton. Use instead of allocating a new instance.</summary>
    public static readonly NullSink Instance = new();

    /// <inheritdoc/>
    public override bool IsHealthy => true;

    /// <inheritdoc/>
    protected override bool Accept(System.ReadOnlySpan<byte> payload) => true;

    /// <inheritdoc/>
    public override void Flush() { }

    /// <inheritdoc/>
    public override void Dispose() { }
}
```

- [ ] **Step 3: Delete old files**

```bash
git rm src/Relay/MpscByteQueueSink.cs src/Relay/NullByteSink.cs
```

- [ ] **Step 4: Build and test**

```bash
dotnet build src/Relay/Relay.csproj -c Release
dotnet test tests/Relay.Tests
```

Expected: 0 errors, all tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Relay/
git commit -m "refactor: rename MpscByteQueueSink and NullByteSink to non-generic packet variants w/Claude"
```

---

## Task 5: TestSinks helpers + PacketSink chain tests

**Files:**
- Create: `tests/Relay.Tests/TestSinks/CollectingSink.cs`
- Create: `tests/Relay.Tests/PacketSinkChainTests.cs`

- [ ] **Step 1: Create CollectingSink**

`tests/Relay.Tests/TestSinks/CollectingSink.cs`:

```csharp
using System;
using System.Collections.Generic;
using Relay;

namespace Relay.Tests.TestSinks;

/// <summary>Collects accepted payloads. IsHealthy is configurable.</summary>
internal sealed class CollectingSink : PacketSink
{
    private readonly List<byte[]> _received = new();
    private bool _healthy = true;

    public IReadOnlyList<byte[]> Received => _received;
    public int AcceptCallCount { get; private set; }

    public override bool IsHealthy => _healthy;

    public void SetHealthy(bool value) => _healthy = value;

    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        AcceptCallCount++;
        _received.Add(payload.ToArray());
        return true;
    }

    public override void Flush()   { }
    public override void Dispose() { }
}
```

- [ ] **Step 2: Write PacketSinkChainTests**

`tests/Relay.Tests/PacketSinkChainTests.cs`:

```csharp
using System;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class PacketSinkChainTests
{
    private static readonly byte[] Payload = [1, 2, 3, 4];

    [Fact]
    public void Enqueue_HealthySink_AcceptsPayload()
    {
        var sink = new CollectingSink();
        sink.Enqueue(Payload);
        sink.Received.Should().HaveCount(1);
        sink.Received[0].Should().Equal(Payload);
    }

    [Fact]
    public void Enqueue_UnhealthySink_FallsThrough_ToNext()
    {
        var primary = new CollectingSink();
        var fallback = new CollectingSink();
        primary.Next = fallback;
        primary.SetHealthy(false);

        primary.Enqueue(Payload);

        primary.Received.Should().BeEmpty();
        fallback.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Enqueue_NullNext_UnhealthySink_DropsPayload()
    {
        var sink = new CollectingSink();
        sink.SetHealthy(false);

        var act = () => sink.Enqueue(Payload);
        act.Should().NotThrow();
    }

    [Fact]
    public void Enqueue_PropagateAfterAccept_True_PayloadReachesNext()
    {
        var fork = new PropagatingCollectingSink();
        var next = new CollectingSink();
        fork.Next = next;

        fork.Enqueue(Payload);

        fork.Received.Should().HaveCount(1);
        next.Received.Should().HaveCount(1);
        next.Received[0].Should().Equal(Payload);
    }

    [Fact]
    public void Enqueue_PropagateAfterAccept_False_NextNotCalled()
    {
        var sink = new CollectingSink(); // PropagateAfterAccept = false (default)
        var next = new CollectingSink();
        sink.Next = next;

        sink.Enqueue(Payload);

        sink.Received.Should().HaveCount(1);
        next.Received.Should().BeEmpty();
    }

    [Fact]
    public void Enqueue_ThreeNodeChain_FallsToTerminal()
    {
        var a = new CollectingSink();
        var b = new CollectingSink();
        var c = new CollectingSink();
        a.Next = b;
        b.Next = c;
        a.SetHealthy(false);
        b.SetHealthy(false);

        a.Enqueue(Payload);

        a.Received.Should().BeEmpty();
        b.Received.Should().BeEmpty();
        c.Received.Should().HaveCount(1);
    }

    // Sealed subclass with PropagateAfterAccept = true for testing the propagate path.
    private sealed class PropagatingCollectingSink : CollectingSink
    {
        public override bool PropagateAfterAccept => true;
    }
}
```

- [ ] **Step 3: Run tests**

```bash
dotnet test tests/Relay.Tests --filter "PacketSinkChainTests"
```

Expected: 6 passed, 0 failed.

- [ ] **Step 4: Commit**

```bash
git add tests/Relay.Tests/
git commit -m "test: add CollectingSink helper and PacketSink chain tests w/Claude"
```

---

## Task 6: ForkSink (non-generic)

**Files:**
- Create: `src/Relay/ForkSink.Packet.cs`
- Create: `tests/Relay.Tests/ForkSinkPacketTests.cs`

- [ ] **Step 1: Write failing tests**

`tests/Relay.Tests/ForkSinkPacketTests.cs`:

```csharp
using System;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class ForkSinkPacketTests
{
    private static readonly byte[] Payload = [10, 20, 30];

    [Fact]
    public void Enqueue_PrimaryHealthy_PayloadReachesBothPrimaryAndNext()
    {
        var primary = new CollectingSink();
        var next    = new CollectingSink();
        var fork    = new ForkSink(primary);
        fork.Next   = next;

        fork.Enqueue(Payload);

        primary.Received.Should().HaveCount(1);
        next.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Enqueue_PrimaryUnhealthy_OnlyNextReceivesPayload()
    {
        var primary = new CollectingSink();
        var next    = new CollectingSink();
        var fork    = new ForkSink(primary);
        fork.Next   = next;
        primary.SetHealthy(false);

        fork.Enqueue(Payload);

        primary.Received.Should().BeEmpty("Accept not called on unhealthy primary via ForkSink");
        next.Received.Should().HaveCount(1);
    }

    [Fact]
    public void IsHealthy_MirrorsPrimary()
    {
        var primary = new CollectingSink();
        var fork    = new ForkSink(primary);

        fork.IsHealthy.Should().BeTrue();
        primary.SetHealthy(false);
        fork.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public void Flush_PropagatesTo_PrimaryAndNext()
    {
        var primary = new FlushTrackingSink();
        var next    = new FlushTrackingSink();
        var fork    = new ForkSink(primary);
        fork.Next   = next;

        fork.Flush();

        primary.Flushed.Should().BeTrue();
        next.Flushed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_PropagatesTo_PrimaryAndNext()
    {
        var primary = new FlushTrackingSink();
        var next    = new FlushTrackingSink();
        var fork    = new ForkSink(primary);
        fork.Next   = next;

        fork.Dispose();

        primary.Disposed.Should().BeTrue();
        next.Disposed.Should().BeTrue();
    }

    private sealed class FlushTrackingSink : PacketSink
    {
        public bool Flushed  { get; private set; }
        public bool Disposed { get; private set; }
        public override bool IsHealthy => true;
        protected override bool Accept(ReadOnlySpan<byte> payload) => true;
        public override void Flush()   => Flushed  = true;
        public override void Dispose() => Disposed = true;
    }
}
```

- [ ] **Step 2: Run — expect compile error (ForkSink does not exist)**

```bash
dotnet test tests/Relay.Tests --filter "ForkSinkPacketTests" 2>&1 | head -10
```

Expected: build error `ForkSink type not found`.

- [ ] **Step 3: Implement ForkSink**

`src/Relay/ForkSink.Packet.cs`:

```csharp
using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// <see cref="PacketSink"/> that delivers to a primary sink and propagates to <see cref="PacketSink.Next"/>
/// after a successful accept — enabling tee/audit patterns.
/// </summary>
public sealed class ForkSink : PacketSink
{
    private readonly PacketSink _primary;

    /// <param name="primary">Sink that receives a copy of every payload.</param>
    public ForkSink(PacketSink primary) => _primary = primary;

    /// <inheritdoc/>
    public override bool PropagateAfterAccept => true;

    /// <inheritdoc/>
    public override bool IsHealthy => _primary.IsHealthy;

    /// <summary>Enqueues to <paramref name="primary"/>; base propagates to Next on success.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        _primary.Enqueue(payload);
        return _primary.IsHealthy;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        _primary.Flush();
        Next?.Flush();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        _primary.Dispose();
        Next?.Dispose();
    }
}
```

- [ ] **Step 4: Run tests — expect pass**

```bash
dotnet test tests/Relay.Tests --filter "ForkSinkPacketTests"
```

Expected: 5 passed, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add src/Relay/ tests/Relay.Tests/
git commit -m "feat: add ForkSink (non-generic) with tee and PropagateAfterAccept w/Claude"
```

---

## Task 7: MultiSink (non-generic)

**Files:**
- Create: `src/Relay/MultiSink.Packet.cs`
- Create: `tests/Relay.Tests/MultiSinkPacketTests.cs`

- [ ] **Step 1: Write failing tests**

`tests/Relay.Tests/MultiSinkPacketTests.cs`:

```csharp
using System;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class MultiSinkPacketTests
{
    private static readonly byte[] Payload = [5, 6, 7];

    [Fact]
    public void Enqueue_AllChildrenReceivePayload()
    {
        var a    = new CollectingSink();
        var b    = new CollectingSink();
        var c    = new CollectingSink();
        var multi = new MultiSink(a, b, c);

        multi.Enqueue(Payload);

        a.Received.Should().HaveCount(1);
        b.Received.Should().HaveCount(1);
        c.Received.Should().HaveCount(1);
    }

    [Fact]
    public void IsHealthy_True_WhenAtLeastOneChildHealthy()
    {
        var healthy   = new CollectingSink();
        var unhealthy = new CollectingSink();
        unhealthy.SetHealthy(false);
        var multi = new MultiSink(healthy, unhealthy);

        multi.IsHealthy.Should().BeTrue();
    }

    [Fact]
    public void IsHealthy_False_WhenAllChildrenUnhealthy()
    {
        var a = new CollectingSink();
        var b = new CollectingSink();
        a.SetHealthy(false);
        b.SetHealthy(false);
        var multi = new MultiSink(a, b);

        multi.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public void Enqueue_AllChildrenUnhealthy_FallsToNext()
    {
        var a    = new CollectingSink();
        var b    = new CollectingSink();
        var next = new CollectingSink();
        a.SetHealthy(false);
        b.SetHealthy(false);
        var multi = new MultiSink(a, b);
        multi.Next = next;

        multi.Enqueue(Payload);

        next.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Enqueue_UnhealthyChild_StillReceivesPayload_ViaEnqueue()
    {
        // MultiSink broadcasts to all children regardless of individual health.
        // Unhealthy children receive via their own Enqueue (which falls to their Next).
        var healthy   = new CollectingSink();
        var unhealthy = new CollectingSink();
        unhealthy.SetHealthy(false);
        var multi = new MultiSink(healthy, unhealthy);

        multi.Enqueue(Payload);

        // healthy received it directly
        healthy.Received.Should().HaveCount(1);
        // unhealthy.Accept not called (IsHealthy=false in Enqueue gate),
        // but Enqueue was called — its Next (null) drops it silently.
        unhealthy.AcceptCallCount.Should().Be(0);
    }
}
```

- [ ] **Step 2: Run — expect compile error**

```bash
dotnet test tests/Relay.Tests --filter "MultiSinkPacketTests" 2>&1 | head -10
```

- [ ] **Step 3: Implement MultiSink**

`src/Relay/MultiSink.Packet.cs`:

```csharp
using System;

namespace Relay;

/// <summary>
/// Broadcasts every payload to all child <see cref="PacketSink"/> instances.
/// <see cref="PacketSink.Next"/> is used only when all children are unhealthy.
/// </summary>
public sealed class MultiSink : PacketSink
{
    private readonly PacketSink[] _children;

    /// <param name="children">One or more sinks to broadcast to.</param>
    public MultiSink(params PacketSink[] children) => _children = children;

    /// <inheritdoc/>
    public override bool IsHealthy
    {
        get
        {
            foreach (var c in _children)
                if (c.IsHealthy) return true;
            return false;
        }
    }

    /// <summary>Broadcasts to all children. Returns true always; Next reached only via base IsHealthy gate.</summary>
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        foreach (var c in _children)
            c.Enqueue(payload);
        return true;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        foreach (var c in _children) c.Flush();
        Next?.Flush();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        foreach (var c in _children) c.Dispose();
    }
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Relay.Tests --filter "MultiSinkPacketTests"
```

Expected: 5 passed, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add src/Relay/ tests/Relay.Tests/
git commit -m "feat: add MultiSink (non-generic) broadcast for packet payloads w/Claude"
```

---

## Task 8: FilterSink + PacketPredicate

**Files:**
- Create: `src/Relay/FilterSink.Packet.cs`
- Create: `tests/Relay.Tests/FilterSinkPacketTests.cs`

- [ ] **Step 1: Write failing tests**

`tests/Relay.Tests/FilterSinkPacketTests.cs`:

```csharp
using System;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class FilterSinkPacketTests
{
    private static readonly byte[] MatchPayload    = [0xFF, 2, 3];
    private static readonly byte[] NoMatchPayload  = [0x00, 2, 3];

    private static bool FirstByteIsFF(ReadOnlySpan<byte> p) => p.Length > 0 && p[0] == 0xFF;

    [Fact]
    public void Accept_MatchingPayload_RoutesToDownstream()
    {
        var downstream = new CollectingSink();
        var filter     = new FilterSink(FirstByteIsFF, downstream);

        filter.Enqueue(MatchPayload);

        downstream.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Accept_NonMatchingPayload_IsDiscarded_NotForwardedAnywhere()
    {
        var downstream = new CollectingSink();
        var next       = new CollectingSink();
        var filter     = new FilterSink(FirstByteIsFF, downstream);
        filter.Next    = next;

        filter.Enqueue(NoMatchPayload);

        downstream.Received.Should().BeEmpty();
        next.Received.Should().BeEmpty("filtered items must not reach Next");
    }

    [Fact]
    public void IsHealthy_AlwaysTrue_EvenWhenDownstreamUnhealthy()
    {
        // FilterSink must NOT mirror downstream health: if it did, PacketSink.Enqueue would
        // skip Accept when downstream is unhealthy, causing filtered items to leak into Next.
        var downstream = new CollectingSink();
        var filter     = new FilterSink(FirstByteIsFF, downstream);

        filter.IsHealthy.Should().BeTrue();
        downstream.SetHealthy(false);
        filter.IsHealthy.Should().BeTrue("filter is itself never unhealthy; only downstream can fail");
    }

    [Fact]
    public void Enqueue_DownstreamUnhealthy_NonMatchingPayload_NeverReachesNext()
    {
        // Regression: before the fix, IsHealthy mirrored downstream. When downstream became
        // unhealthy, the base Enqueue routed the payload straight to Next without running
        // the predicate — a filtered item would leak into Next. This must never happen.
        var downstream = new CollectingSink();
        var next       = new CollectingSink();
        downstream.SetHealthy(false);
        var filter     = new FilterSink(FirstByteIsFF, downstream);
        filter.Next    = next;

        filter.Enqueue(NoMatchPayload);

        next.Received.Should().BeEmpty("non-matching payload must be consumed by the filter, never by Next");
    }

    [Fact]
    public void Enqueue_AlwaysReturnsTrue_NextNeverTriggeredViaAccept()
    {
        // Accept always returns true, so Enqueue never calls Next?.Enqueue via the fallback path.
        var downstream = new CollectingSink();
        var next       = new CollectingSink();
        var filter     = new FilterSink(FirstByteIsFF, downstream);
        filter.Next    = next;

        filter.Enqueue(MatchPayload);   // passes filter
        filter.Enqueue(NoMatchPayload); // fails filter

        next.Received.Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Run — expect compile error**

```bash
dotnet test tests/Relay.Tests --filter "FilterSinkPacketTests" 2>&1 | head -10
```

- [ ] **Step 3: Implement FilterSink**

`src/Relay/FilterSink.Packet.cs`:

```csharp
using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Delegate for evaluating whether a packet payload should be forwarded.
/// The span is valid for the duration of the call only — do not store it.
/// </summary>
public delegate bool PacketPredicate(ReadOnlySpan<byte> payload);

/// <summary>
/// Conditional gate: payloads matching the predicate are forwarded to <paramref name="downstream"/>;
/// non-matching payloads are silently consumed and do NOT propagate to <see cref="PacketSink.Next"/>.
/// </summary>
public sealed class FilterSink : PacketSink
{
    private readonly PacketPredicate _predicate;
    private readonly PacketSink      _downstream;

    /// <param name="predicate">True → forward to downstream. False → discard.</param>
    /// <param name="downstream">Receives payloads that pass the predicate.</param>
    public FilterSink(PacketPredicate predicate, PacketSink downstream)
    {
        _predicate  = predicate;
        _downstream = downstream;
    }

    /// <summary>
    /// Always true. If this mirrored <paramref name="downstream"/> health, the base
    /// <see cref="PacketSink.Enqueue"/> would skip <see cref="Accept"/> whenever downstream
    /// failed, routing the payload straight to <see cref="PacketSink.Next"/> — violating the
    /// "filtered items never propagate to Next" invariant. Downstream owns its own fallback chain.
    /// </summary>
    public override bool IsHealthy => true;

    /// <summary>
    /// Forwards matching payloads to downstream. Always returns true — filtered items are consumed,
    /// not propagated to <see cref="PacketSink.Next"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        if (_predicate(payload)) _downstream.Enqueue(payload);
        return true;
    }

    /// <inheritdoc/>
    public override void Flush()   => _downstream.Flush();

    /// <inheritdoc/>
    public override void Dispose() => _downstream.Dispose();
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Relay.Tests --filter "FilterSinkPacketTests"
```

Expected: 4 passed, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add src/Relay/ tests/Relay.Tests/
git commit -m "feat: add FilterSink and PacketPredicate for conditional packet routing w/Claude"
```

---

## Task 9: SerializeSink\<T\>

**Files:**
- Create: `src/Relay/SerializeSink.cs`
- Create: `tests/Relay.Tests/SerializeSinkTests.cs`

- [ ] **Step 1: Write failing tests**

`tests/Relay.Tests/SerializeSinkTests.cs`:

```csharp
using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class SerializeSinkTests
{
    [System.Runtime.InteropServices.StructLayout(
        System.Runtime.InteropServices.LayoutKind.Sequential, Size = 64)]
    private struct Event64 { public long Value; }

    [Fact]
    public void Accept_ConvertsStructToBytes_CorrectLength()
    {
        var downstream  = new CollectingSink();
        var serialize   = new SerializeSink<Event64>(downstream);
        var item        = new Event64 { Value = 0xDEAD_BEEF_CAFE_1234L };

        serialize.Enqueue(in item);

        downstream.Received.Should().HaveCount(1);
        downstream.Received[0].Should().HaveCount(64);
    }

    [Fact]
    public void Accept_ConvertsStructToBytes_CorrectContent()
    {
        var downstream = new CollectingSink();
        var serialize  = new SerializeSink<Event64>(downstream);
        var item       = new Event64 { Value = 42L };

        serialize.Enqueue(in item);

        byte[] expected = new byte[64];
        MemoryMarshal.Write(expected.AsSpan(), (long)42L);
        downstream.Received[0].Should().Equal(expected);
    }

    [Fact]
    public void IsHealthy_MirrorsTarget()
    {
        var downstream = new CollectingSink();
        var serialize  = new SerializeSink<Event64>(downstream);

        serialize.IsHealthy.Should().BeTrue();
        downstream.SetHealthy(false);
        serialize.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public void Accept_ZeroAllocation_VerifiedViaGcMetrics()
    {
        var downstream = new CollectingSink();
        var serialize  = new SerializeSink<Event64>(downstream);
        var item       = new Event64 { Value = 1L };

        // Warm up JIT.
        serialize.Enqueue(in item);
        downstream.Received.Clear();

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1_000; i++)
            serialize.Enqueue(in item);
        long after  = GC.GetAllocatedBytesForCurrentThread();

        (after - before).Should().Be(0, "SerializeSink.Accept must not allocate");
    }
}
```

Note: `CollectingSink.Received` needs a `Clear()` method. Add to `CollectingSink`:
```csharp
public void Clear() => _received.Clear();
```

- [ ] **Step 2: Run — expect compile error**

```bash
dotnet test tests/Relay.Tests --filter "SerializeSinkTests" 2>&1 | head -10
```

- [ ] **Step 3: Implement SerializeSink\<T\>**

`src/Relay/SerializeSink.cs`:

```csharp
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Relay;

/// <summary>
/// Bridge from the typed <see cref="DispatchSink{T}"/> hierarchy to the <see cref="PacketSink"/>
/// hierarchy. Converts <typeparamref name="T"/> to a <c>ReadOnlySpan&lt;byte&gt;</c> via
/// <see cref="MemoryMarshal.AsBytes{T}"/> — zero copy, zero allocation.
/// </summary>
/// <remarks>
/// <see cref="DispatchSink{T}.Accept"/> always returns <c>true</c>: the packet chain assumes
/// responsibility for delivery. The typed chain receives no per-record delivery signal.
/// Health is mirrored from the target packet sink for typed-chain short-circuiting.
/// </remarks>
/// <typeparam name="T">Unmanaged struct. Must be a multiple of 64 bytes (cache-line aligned).</typeparam>
public sealed class SerializeSink<T> : DispatchSink<T> where T : unmanaged
{
    private readonly PacketSink _target;

    /// <param name="target">Packet sink that receives the serialized bytes.</param>
    public SerializeSink(PacketSink target) => _target = target;

    /// <inheritdoc/>
    public override bool IsHealthy => _target.IsHealthy;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in T item)
    {
        _target.Enqueue(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in item, 1)));
        return true;
    }

    /// <inheritdoc/>
    public override void Flush()   => _target.Flush();

    /// <inheritdoc/>
    public override void Dispose() => _target.Dispose();
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Relay.Tests --filter "SerializeSinkTests"
```

Expected: 4 passed, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add src/Relay/ tests/Relay.Tests/
git commit -m "feat: add SerializeSink<T> bridge from typed dispatch to PacketSink chain w/Claude"
```

---

## Task 10: SinkChainBuilder

**Files:**
- Create: `src/Relay/Builder/SinkChainBuilder.cs`
- Create: `src/Relay/Builder/SinkChain.Packet.cs`
- Create: `src/Relay/Builder/FilterBinding.Packet.cs`
- Create: `tests/Relay.Tests/SinkChainBuilderTests.cs`

- [ ] **Step 1: Write failing tests**

`tests/Relay.Tests/SinkChainBuilderTests.cs`:

```csharp
using System;
using FluentAssertions;
using Relay.Builder;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class SinkChainBuilderTests
{
    private static readonly byte[] Payload = [1, 2, 3];

    [Fact]
    public void To_WiresNextCorrectly()
    {
        var primary  = new CollectingSink();
        var fallback = new CollectingSink();

        SinkChainBuilder.Start(primary).To(fallback);
        primary.SetHealthy(false);
        primary.Enqueue(Payload);

        fallback.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Fork_InsertsForkSinkBetweenHeadAndNext()
    {
        var audit  = new CollectingSink();
        var next   = new CollectingSink();
        var head   = new CollectingSink();

        SinkChainBuilder.Start(head).Fork(audit).To(next);
        head.Enqueue(Payload);

        // head is healthy: head accepts, then ForkSink forwards to audit and Next
        audit.Received.Should().HaveCount(1);
        next.Received.Should().HaveCount(1);
    }

    [Fact]
    public void When_To_InsertsFilterSink()
    {
        var downstream = new CollectingSink();
        var head       = new CollectingSink();

        SinkChainBuilder.Start(head)
            .When(p => p.Length > 0 && p[0] == 0xFF)
            .To(downstream);

        head.Next!.Enqueue([0xFF, 1]);
        head.Next!.Enqueue([0x00, 1]);

        downstream.Received.Should().HaveCount(1);
        downstream.Received[0][0].Should().Be(0xFF);
    }

    [Fact]
    public void Multi_InsertsBroadcastSink()
    {
        var a    = new CollectingSink();
        var b    = new CollectingSink();
        var head = new CollectingSink();
        head.SetHealthy(false);

        SinkChainBuilder.Start(head).Multi(a, b);
        head.Enqueue(Payload);

        a.Received.Should().HaveCount(1);
        b.Received.Should().HaveCount(1);
    }

    [Fact]
    public void ImplicitOperator_ReturnsHeadAsPacketSink()
    {
        var head  = new CollectingSink();
        PacketSink sink = SinkChainBuilder.Start(head);

        sink.Should().BeSameAs(head);
    }

    [Fact]
    public void When_To_To_ChainContinuesFromDownstream_FilterNotOverwritten()
    {
        // Regression: FilterBinding.To must advance the chain tail to downstream, so a
        // subsequent .To(b) appends b to downstream (not to the attach point, which would
        // overwrite the filter).
        var head       = new CollectingSink();
        var downstream = new CollectingSink();
        var b          = new CollectingSink();

        SinkChainBuilder.Start(head)
            .When(p => p.Length > 0 && p[0] == 0xFF)
            .To(downstream)
            .To(b);

        // head.Next must be a FilterSink (filter installed between head and downstream)
        head.Next.Should().BeOfType<FilterSink>();
        // downstream.Next must be b (chain continued from downstream)
        downstream.Next.Should().BeSameAs(b);
    }
}
```

- [ ] **Step 2: Run — expect compile error**

```bash
dotnet test tests/Relay.Tests --filter "SinkChainBuilderTests" 2>&1 | head -10
```

- [ ] **Step 3: Implement SinkChainBuilder**

`src/Relay/Builder/SinkChainBuilder.cs`:

```csharp
namespace Relay.Builder;

/// <summary>Static entry points for building <see cref="PacketSink"/> fallback chains.</summary>
public static class SinkChainBuilder
{
    /// <summary>Starts a chain with any <see cref="PacketSink"/> head.</summary>
    public static SinkChain<THead> Start<THead>(THead head)
        where THead : PacketSink => new(head);

    /// <summary>Starts a chain with an SPSC queue sink head.</summary>
    public static SinkChain<THead> StartSpsc<THead>(THead head)
        where THead : SpscQueueSink => new(head);

    /// <summary>Starts a chain with an MPSC queue sink head.</summary>
    public static SinkChain<THead> StartMpsc<THead>(THead head)
        where THead : MpscQueueSink => new(head);
}
```

`src/Relay/Builder/SinkChain.Packet.cs`:

```csharp
using System;

namespace Relay.Builder;

/// <summary>Fluent builder for <see cref="PacketSink"/> fallback chains.</summary>
public sealed class SinkChain<THead> where THead : PacketSink
{
    /// <summary>First sink in the chain. Pass to <see cref="Enqueue"/> callers.</summary>
    public THead Head { get; }

    private PacketSink _tail;

    internal SinkChain(THead head) { Head = head; _tail = head; }

    /// <summary>
    /// Appends <paramref name="sink"/> as fallback. Wires <see cref="SpscQueueSink.Prev"/>
    /// when <paramref name="sink"/> is an <see cref="SpscQueueSink"/> (enables drain-to-prev).
    /// </summary>
    public SinkChain<THead> To(PacketSink sink)
    {
        _tail.Next = sink;
        if (sink is SpscQueueSink spsc) spsc.Prev = _tail;
        _tail = sink;
        return this;
    }

    /// <summary>Inserts a <see cref="ForkSink"/> that delivers to <paramref name="primary"/> and Next.</summary>
    public SinkChain<THead> Fork(PacketSink primary)
    {
        var fork = new ForkSink(primary);
        _tail.Next = fork;
        _tail = fork;
        return this;
    }

    /// <summary>Opens a conditional gate. Close with <see cref="FilterBinding{THead}.To"/>.</summary>
    public FilterBinding<THead> When(PacketPredicate predicate) => new(this, predicate);

    /// <summary>Inserts a <see cref="MultiSink"/> broadcasting to all <paramref name="children"/>.</summary>
    public SinkChain<THead> Multi(params PacketSink[] children)
    {
        var multi = new MultiSink(children);
        _tail.Next = multi;
        _tail = multi;
        return this;
    }

    /// <summary>Returns the head as a bare <see cref="PacketSink"/> reference.</summary>
    public static implicit operator PacketSink(SinkChain<THead> chain) => chain.Head;

    // Called by FilterBinding.To: installs the filter at the current tail and advances
    // the tail to downstream. The filter is terminal for the predicate; downstream
    // extends the chain for any subsequent .To(...).
    internal void AppendFilter(FilterSink filter, PacketSink downstream)
    {
        _tail.Next = filter;
        _tail      = downstream;
    }
}
```

`src/Relay/Builder/FilterBinding.Packet.cs`:

```csharp
namespace Relay.Builder;

/// <summary>Intermediate state after <see cref="SinkChain{THead}.When"/>; closed by <see cref="To"/>.</summary>
public sealed class FilterBinding<THead> where THead : PacketSink
{
    private readonly SinkChain<THead> _chain;
    private readonly PacketPredicate  _predicate;

    internal FilterBinding(SinkChain<THead> chain, PacketPredicate predicate)
    {
        _chain     = chain;
        _predicate = predicate;
    }

    /// <summary>
    /// Creates a <see cref="FilterSink"/> wrapping <paramref name="downstream"/> and appends it
    /// to the current tail. Advances the chain tail to <paramref name="downstream"/> so subsequent
    /// <c>.To(...)</c> calls extend downstream's fallback chain (not overwrite the filter).
    /// </summary>
    public SinkChain<THead> To(PacketSink downstream)
    {
        var filter = new FilterSink(_predicate, downstream);
        _chain.AppendFilter(filter, downstream);
        return _chain;
    }
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Relay.Tests --filter "SinkChainBuilderTests"
```

Expected: 5 passed, 0 failed.

- [ ] **Step 5: Run full suite — Camada 1 complete**

```bash
dotnet test tests/Relay.Tests
```

Expected: all tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/Relay/ tests/Relay.Tests/
git commit -m "feat: add SinkChainBuilder, SinkChain<THead>, FilterBinding for packet chains w/Claude"
```

---

## Task 11: TcpSink

**Files:**
- Create: `src/Relay/Sinks/TcpSink.cs`
- Create: `tests/Relay.Tests/TcpSinkTests.cs`

- [ ] **Step 1: Write failing tests**

`tests/Relay.Tests/TcpSinkTests.cs`:

```csharp
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests;

public sealed class TcpSinkTests : IDisposable
{
    private readonly TcpListener _listener;
    private readonly int         _port;
    private readonly List<byte[]> _received = new();
    private readonly Thread      _serverThread;
    private volatile bool        _serverRunning = true;

    public TcpSinkTests()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        _port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _serverThread = new Thread(ServerLoop) { IsBackground = true };
        _serverThread.Start();
    }

    public void Dispose()
    {
        _serverRunning = false;
        _listener.Stop();
        _serverThread.Join(500);
    }

    [Fact]
    public void Enqueue_PayloadDeliveredWith4ByteBigEndianLengthPrefix()
    {
        using var sink = new TcpSink("127.0.0.1", _port, flushIntervalMs: 50);
        sink.Start();
        byte[] payload = [10, 20, 30, 40];

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 2_000);

        WaitForReceived(count: 1);
        _received.Should().HaveCount(1);
        _received[0].Should().Equal(payload);
    }

    [Fact]
    public void Enqueue_MultiplePayloads_AllDeliveredInOrder()
    {
        using var sink = new TcpSink("127.0.0.1", _port, flushIntervalMs: 50);
        sink.Start();

        for (int i = 0; i < 5; i++)
            sink.Enqueue([(byte)i]);
        sink.Stop(drainTimeoutMs: 2_000);

        WaitForReceived(count: 5);
        _received.Should().HaveCount(5);
        for (int i = 0; i < 5; i++)
            _received[i][0].Should().Be((byte)i);
    }

    [Fact]
    public void IsHealthy_True_AfterSuccessfulConnect()
    {
        using var sink = new TcpSink("127.0.0.1", _port, flushIntervalMs: 100);
        sink.Start();
        Thread.Sleep(200); // let consumer thread connect

        sink.IsHealthy.Should().BeTrue();
        sink.Stop();
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var sink = new TcpSink("127.0.0.1", _port);
        sink.Start();
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }

    private void ServerLoop()
    {
        try
        {
            while (_serverRunning)
            {
                _listener.Server.Poll(50_000, SelectMode.SelectRead);
                if (!_serverRunning) break;
                if (!_listener.Pending()) continue;
                var client = _listener.AcceptTcpClient();
                var stream = client.GetStream();
                var lenBuf = new byte[4];
                try
                {
                    while (true)
                    {
                        int n = stream.Read(lenBuf, 0, 4);
                        if (n == 0) break;
                        int len = (int)BinaryPrimitives.ReadUInt32BigEndian(lenBuf);
                        var payload = new byte[len];
                        int read = 0;
                        while (read < len)
                            read += stream.Read(payload, read, len - read);
                        lock (_received) _received.Add(payload);
                    }
                }
                catch { }
                finally { client.Dispose(); }
            }
        }
        catch { }
    }

    private void WaitForReceived(int count, int timeoutMs = 3_000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            lock (_received)
                if (_received.Count >= count) return;
            Thread.Sleep(10);
        }
    }
}
```

- [ ] **Step 2: Run — expect compile error**

```bash
dotnet test tests/Relay.Tests --filter "TcpSinkTests" 2>&1 | head -10
```

- [ ] **Step 3: Implement TcpSink**

`src/Relay/Sinks/TcpSink.cs`:

```csharp
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary>
/// <see cref="SpscQueueSink"/> that delivers byte payloads over TCP with a 4-byte Big-Endian
/// length prefix. BE framing is wire-compatible with Input2Log TCP, NamedPipe, UnixSocket,
/// and SharedMemory receivers (<c>BinaryPrimitives.ReadInt32BigEndian</c>).
/// </summary>
public sealed class TcpSink : SpscQueueSink
{
    private const int MinBackoffMs = 1_000;
    private const int MaxBackoffMs = 30_000;

    private readonly string _host;
    private readonly int    _port;
    private readonly byte[] _sendBuffer;  // POH pinned

    private Socket? _socket;
    private int     _filled;
    private int     _backoffMs      = MinBackoffMs;
    private long    _nextRetryTicks;

    /// <param name="host">TCP server hostname or IP.</param>
    /// <param name="port">TCP server port.</param>
    /// <param name="sendBufferCapacity">POH send buffer size in bytes. Default 64 KB.</param>
    /// <param name="ringCapacity">SPSC ring size in bytes. Must be power of two. Default 64 KB.</param>
    /// <param name="flushIntervalMs">Max ms between forced flushes. Default 100 ms.</param>
    public TcpSink(
        string host,
        int    port,
        int    sendBufferCapacity = 65_536,
        int    ringCapacity       = 65_536,
        int    flushIntervalMs    = 100)
        : base(ringCapacity, flushIntervalMs, $"tcp-{host}:{port}")
    {
        _host       = host;
        _port       = port;
        _sendBuffer = GC.AllocateArray<byte>(sendBufferCapacity, pinned: true);
        ConnectSocket();
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        // 4B Big-Endian length prefix + payload. BE matches Input2Log TCP/NamedPipe/
        // UnixSocket/SharedMemory receivers (BinaryPrimitives.ReadInt32BigEndian).
        int needed = 4 + payload.Length;
        if (_filled + needed > _sendBuffer.Length)
            FlushBackend();

        BinaryPrimitives.WriteUInt32BigEndian(_sendBuffer.AsSpan(_filled), (uint)payload.Length);
        _filled += 4;
        payload.CopyTo(_sendBuffer.AsSpan(_filled));
        _filled += payload.Length;
    }

    protected override void FlushBackend()
    {
        if (_filled == 0 || _socket is null) return;
        try
        {
            _socket.Send(_sendBuffer.AsSpan(0, _filled));
            _filled    = 0;
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _filled  = 0;
            _healthy = false;
        }
    }

    protected override void TryRecoverBackend()
    {
        if (_healthy) return;
        if (HfClock.NowTicks < _nextRetryTicks) return;
        ConnectSocket();
    }

    protected override void DisposeBackend()
    {
        try { _socket?.Shutdown(SocketShutdown.Both); } catch { }
        _socket?.Dispose();
        _socket = null;
    }

    private void ConnectSocket()
    {
        try
        {
            _socket?.Dispose();
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            _socket.Connect(_host, _port);
            _healthy   = true;
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _socket?.Dispose();
            _socket         = null;
            _healthy        = false;
            _backoffMs      = Math.Min(_backoffMs * 2, MaxBackoffMs);
            _nextRetryTicks = HfClock.NowTicks + (long)_backoffMs * (Stopwatch.Frequency / 1_000);
        }
    }
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Relay.Tests --filter "TcpSinkTests"
```

Expected: 4 passed, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add src/Relay/ tests/Relay.Tests/
git commit -m "feat: add TcpSink with 4B LE length framing and exponential backoff reconnect w/Claude"
```

---

## Task 12: UdpSink

**Files:**
- Create: `src/Relay/Sinks/UdpSink.cs`
- Create: `tests/Relay.Tests/UdpSinkTests.cs`

- [ ] **Step 1: Write failing tests**

`tests/Relay.Tests/UdpSinkTests.cs`:

```csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests;

public sealed class UdpSinkTests : IDisposable
{
    private readonly UdpClient _receiver;
    private readonly int       _port;

    public UdpSinkTests()
    {
        _receiver = new UdpClient(0);
        _port     = ((IPEndPoint)_receiver.Client.LocalEndPoint!).Port;
    }

    public void Dispose() => _receiver.Dispose();

    [Fact]
    public void Enqueue_PayloadDeliveredAsDatagram_NoLengthPrefix()
    {
        using var sink = new UdpSink("127.0.0.1", _port, ringCapacity: 4_096, flushIntervalMs: 50);
        sink.Start();
        byte[] payload = [1, 2, 3, 4, 5];

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 1_000);

        _receiver.Client.Poll(2_000_000, SelectMode.SelectRead).Should().BeTrue();
        var ep = new IPEndPoint(IPAddress.Any, 0);
        byte[] received = _receiver.Receive(ref ep);
        received.Should().Equal(payload);
    }

    [Fact]
    public void Enqueue_PayloadExceedsMaxPayload_SetUnhealthy()
    {
        using var sink = new UdpSink("127.0.0.1", _port, maxPayload: 4,
                                      ringCapacity: 4_096, flushIntervalMs: 50);
        sink.Start();
        Thread.Sleep(100); // let consumer start

        sink.Enqueue(new byte[5]); // exceeds maxPayload=4
        Thread.Sleep(200);

        sink.IsHealthy.Should().BeFalse();
        sink.Stop();
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var sink = new UdpSink("127.0.0.1", _port);
        sink.Start();
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }
}
```

- [ ] **Step 2: Run — expect compile error**

```bash
dotnet test tests/Relay.Tests --filter "UdpSinkTests" 2>&1 | head -10
```

- [ ] **Step 3: Implement UdpSink**

`src/Relay/Sinks/UdpSink.cs`:

```csharp
using System;
using System.Diagnostics;
using System.Net.Sockets;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary><see cref="SpscQueueSink"/> that delivers byte payloads as UDP datagrams.</summary>
public sealed class UdpSink : SpscQueueSink
{
    private const int MinBackoffMs = 1_000;
    private const int MaxBackoffMs = 10_000;

    private readonly string _host;
    private readonly int    _port;
    private readonly int    _maxPayload;

    private Socket? _socket;
    private int     _backoffMs      = MinBackoffMs;
    private long    _nextRetryTicks;

    /// <param name="host">UDP destination hostname or IP.</param>
    /// <param name="port">UDP destination port.</param>
    /// <param name="maxPayload">Max payload bytes; payloads exceeding this mark the sink unhealthy.</param>
    /// <param name="ringCapacity">SPSC ring size in bytes. Must be power of two.</param>
    /// <param name="flushIntervalMs">Max ms between recovery checks.</param>
    public UdpSink(
        string host,
        int    port,
        int    maxPayload      = 65_507,
        int    ringCapacity    = 65_536,
        int    flushIntervalMs = 100)
        : base(ringCapacity, flushIntervalMs, $"udp-{host}:{port}")
    {
        _host       = host;
        _port       = port;
        _maxPayload = maxPayload;
        CreateSocket();
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        if (_socket is null || payload.Length > _maxPayload)
        {
            _healthy = false;
            return;
        }
        try
        {
            _socket.Send(payload);
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _healthy = false;
        }
    }

    // UDP is fire-and-forget per datagram; no buffer to flush.
    protected override void FlushBackend() { }

    protected override void TryRecoverBackend()
    {
        if (_healthy) return;
        if (HfClock.NowTicks < _nextRetryTicks) return;
        try
        {
            _socket?.Dispose();
            CreateSocket();
            _healthy   = true;
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _socket?.Dispose();
            _socket         = null;
            _backoffMs      = Math.Min(_backoffMs * 2, MaxBackoffMs);
            _nextRetryTicks = HfClock.NowTicks + (long)_backoffMs * (Stopwatch.Frequency / 1_000);
        }
    }

    protected override void DisposeBackend()
    {
        _socket?.Dispose();
        _socket = null;
    }

    private void CreateSocket()
    {
        _socket = new Socket(SocketType.Dgram, ProtocolType.Udp) { DontFragment = true };
        _socket.Connect(_host, _port);
    }
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Relay.Tests --filter "UdpSinkTests"
```

Expected: 3 passed, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add src/Relay/ tests/Relay.Tests/
git commit -m "feat: add UdpSink for datagram delivery with maxPayload enforcement w/Claude"
```

---

## Task 13: FileSink

**Files:**
- Create: `src/Relay/Sinks/FileSink.cs`
- Create: `tests/Relay.Tests/FileSinkTests.cs`

- [ ] **Step 1: Write failing tests**

`tests/Relay.Tests/FileSinkTests.cs`:

```csharp
using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests;

public sealed class FileSinkTests : IDisposable
{
    private readonly string _path = Path.GetTempFileName();

    public void Dispose()
    {
        if (File.Exists(_path)) File.Delete(_path);
    }

    [Fact]
    public void Enqueue_PayloadWrittenToFile()
    {
        byte[] payload = [10, 20, 30];
        using var sink = new FileSink(_path, ringCapacity: 4_096, flushIntervalMs: 50);
        sink.Start();

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 1_000);

        File.ReadAllBytes(_path).Should().Equal(payload);
    }

    [Fact]
    public void Start_WithHeader_HeaderWrittenBeforePayloads()
    {
        byte[] header  = [0xCA, 0xFE];
        byte[] payload = [0x01, 0x02];
        File.Delete(_path); // ensure empty file
        using var sink = new FileSink(_path, ringCapacity: 4_096, flushIntervalMs: 50,
                                       header: header);
        sink.Start();

        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 1_000);

        byte[] all = File.ReadAllBytes(_path);
        all[..2].Should().Equal(header);
        all[2..].Should().Equal(payload);
    }

    [Fact]
    public void Start_HeaderNotWritten_WhenFileAlreadyHasContent()
    {
        byte[] existing = [0xAA, 0xBB];
        byte[] header   = [0xFF, 0xFF];
        byte[] payload  = [0x01];
        File.WriteAllBytes(_path, existing);

        using var sink = new FileSink(_path, ringCapacity: 4_096, flushIntervalMs: 50,
                                       header: header);
        sink.Start();
        sink.Enqueue(payload);
        sink.Stop(drainTimeoutMs: 1_000);

        byte[] all = File.ReadAllBytes(_path);
        all[..2].Should().Equal(existing, "header must not overwrite existing content");
        all[2..].Should().Equal(payload);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var sink = new FileSink(_path, ringCapacity: 4_096);
        sink.Start();
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }
}
```

- [ ] **Step 2: Run — expect compile error**

```bash
dotnet test tests/Relay.Tests --filter "FileSinkTests" 2>&1 | head -10
```

- [ ] **Step 3: Implement FileSink**

`src/Relay/Sinks/FileSink.cs`:

```csharp
using System;
using System.Diagnostics;
using System.IO;
using Relay.Internal;

namespace Relay.Sinks;

/// <summary>
/// <see cref="SpscQueueSink"/> that accumulates byte payloads in a POH write buffer and flushes
/// to a <see cref="FileStream"/> on the flush interval. Supports an optional header written once
/// when the file is first created (stream position == 0). No rotation.
/// </summary>
public sealed class FileSink : SpscQueueSink
{
    private const int MinBackoffMs = 1_000;
    private const int MaxBackoffMs = 60_000;

    private readonly string               _path;
    private readonly byte[]               _writeBuffer;   // POH pinned
    private readonly ReadOnlyMemory<byte> _header;

    private FileStream? _stream;
    private int         _filled;
    private int         _backoffMs      = MinBackoffMs;
    private long        _nextRetryTicks;

    /// <param name="path">Destination file path.</param>
    /// <param name="writeBufferCapacity">POH write buffer size in bytes. Default 64 KB.</param>
    /// <param name="ringCapacity">SPSC ring size in bytes. Must be power of two. Default 64 KB.</param>
    /// <param name="flushIntervalMs">Max ms between forced flushes. Default 200 ms.</param>
    /// <param name="header">Optional bytes written once when the file is empty. Caller defines content.</param>
    public FileSink(
        string                path,
        int                   writeBufferCapacity = 65_536,
        int                   ringCapacity        = 65_536,
        int                   flushIntervalMs     = 200,
        ReadOnlyMemory<byte>? header              = null)
        : base(ringCapacity, flushIntervalMs, $"file-{Path.GetFileName(path)}")
    {
        _path        = path;
        _writeBuffer = GC.AllocateArray<byte>(writeBufferCapacity, pinned: true);
        _header      = header ?? ReadOnlyMemory<byte>.Empty;
    }

    protected override void WriteToBackend(ReadOnlySpan<byte> payload)
    {
        if (_stream is null && !TryOpenStream()) return;

        if (_filled + payload.Length > _writeBuffer.Length)
            FlushToStream();

        payload.CopyTo(_writeBuffer.AsSpan(_filled));
        _filled += payload.Length;
    }

    protected override void FlushBackend()
    {
        if (_stream is null) return;
        FlushToStream();
    }

    protected override void TryRecoverBackend()
    {
        if (_healthy) return;
        if (HfClock.NowTicks < _nextRetryTicks) return;
        TryOpenStream();
    }

    protected override void DisposeBackend()
    {
        FlushToStream();
        _stream?.Dispose();
        _stream = null;
    }

    private void FlushToStream()
    {
        if (_stream is null) return;
        try
        {
            if (_filled > 0)
            {
                _stream.Write(_writeBuffer.AsSpan(0, _filled));
                _filled = 0;
            }
            _stream.Flush();
            _backoffMs = MinBackoffMs;
        }
        catch
        {
            _filled = 0;
            _healthy = false;
            _stream?.Dispose();
            _stream = null;
        }
    }

    private bool TryOpenStream()
    {
        try
        {
            _stream = new FileStream(_path, FileMode.Append, FileAccess.Write,
                                     FileShare.Read, bufferSize: 1, useAsync: false);
            if (_header.Length > 0 && _stream.Position == 0)
                _stream.Write(_header.Span);

            _healthy   = true;
            _backoffMs = MinBackoffMs;
            return true;
        }
        catch
        {
            _stream?.Dispose();
            _stream         = null;
            _healthy        = false;
            _backoffMs      = Math.Min(_backoffMs * 2, MaxBackoffMs);
            _nextRetryTicks = HfClock.NowTicks + (long)_backoffMs * (Stopwatch.Frequency / 1_000);
            return false;
        }
    }
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Relay.Tests --filter "FileSinkTests"
```

Expected: 4 passed, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add src/Relay/ tests/Relay.Tests/
git commit -m "feat: add FileSink with optional header, POH write buffer, append-mode recovery w/Claude"
```

---

## Task 14: RamSink

**Files:**
- Create: `src/Relay/Sinks/RamSink.cs`
- Create: `tests/Relay.Tests/RamSinkTests.cs`

- [ ] **Step 1: Write failing tests**

`tests/Relay.Tests/RamSinkTests.cs`:

```csharp
using System;
using System.Collections.Generic;
using FluentAssertions;
using Relay;
using Relay.Sinks;
using Relay.Tests.TestSinks;
using Xunit;

namespace Relay.Tests;

public sealed class RamSinkTests
{
    [Fact]
    public void Accept_PayloadsBuffered_DrainToDeliversInOrder()
    {
        using var sink = new RamSink(capacity: 4_096);
        var target = new CollectingSink();
        byte[] a = [1, 2], b = [3, 4, 5], c = [6];

        sink.Enqueue(a);
        sink.Enqueue(b);
        sink.Enqueue(c);

        sink.DrainTo(target);

        target.Received.Should().HaveCount(3);
        target.Received[0].Should().Equal(a);
        target.Received[1].Should().Equal(b);
        target.Received[2].Should().Equal(c);
    }

    [Fact]
    public void Accept_ReturnsFalse_WhenBufferFull()
    {
        // capacity 64 bytes; each record = 4B header + 4B-aligned payload
        using var sink = new RamSink(capacity: 64);
        byte[] payload = new byte[28]; // 4 + 28 = 32 bytes per record; two fit, third does not

        bool first  = sink.IsHealthy;
        sink.Enqueue(payload);
        bool second = sink.IsHealthy;
        sink.Enqueue(payload);
        bool third  = sink.IsHealthy; // should be false now

        first.Should().BeTrue();
        second.Should().BeTrue();
        third.Should().BeFalse("buffer is full after two 32-byte records in 64-byte capacity");
    }

    [Fact]
    public void DrainTo_UnhealthyTarget_StopsDrain()
    {
        using var sink = new RamSink(capacity: 4_096);
        var target = new CollectingSink();
        target.SetHealthy(false);

        sink.Enqueue([1]);
        sink.Enqueue([2]);

        sink.DrainTo(target);

        target.Received.Should().BeEmpty("drain stops immediately if target is unhealthy");
    }

    [Fact]
    public void DrainTo_CompletelyDrained_IsHealthyBecomesTrue()
    {
        using var sink = new RamSink(capacity: 64);
        byte[] payload = new byte[28];
        sink.Enqueue(payload);
        sink.Enqueue(payload);
        sink.IsHealthy.Should().BeFalse();

        sink.DrainTo(new CollectingSink());

        sink.IsHealthy.Should().BeTrue("buffer reset after full drain");
    }

    [Fact]
    public void Accept_PartialDrain_DoesNotFreeCapacity()
    {
        // Regression: fill-once contract. If DrainTo stops early (target unhealthy), _head
        // advances but _tail stays at capacity. Accept must still return false — writing at
        // _buffer + _tail with _tail ~= _capacity would overflow the buffer.
        using var sink = new RamSink(capacity: 64);
        byte[] payload = new byte[28]; // recordSize = 32B; two fit exactly.

        sink.Enqueue(payload);
        sink.Enqueue(payload);
        sink.IsHealthy.Should().BeFalse("buffer full after two 32B records in 64B capacity");

        // Partial drain: target accepts first, then becomes unhealthy mid-drain.
        var target = new OneShotCollectingSink();
        sink.DrainTo(target);

        target.Received.Should().HaveCount(1, "target stopped accepting after the first record");

        // Third Accept must still return false. With the buggy _tail - _head check, this would
        // pass because _head advanced by 32B — but it would then write at _tail=64, overflowing.
        sink.IsHealthy.Should().BeFalse("partial drain does not free capacity");
    }

    // Collecting sink that flips to unhealthy after accepting exactly one payload.
    private sealed class OneShotCollectingSink : PacketSink
    {
        private readonly List<byte[]> _received = new();
        private bool _healthy = true;

        public IReadOnlyList<byte[]> Received => _received;
        public override bool IsHealthy => _healthy;

        protected override bool Accept(ReadOnlySpan<byte> payload)
        {
            _received.Add(payload.ToArray());
            _healthy = false;
            return true;
        }

        public override void Flush()   { }
        public override void Dispose() { }
    }

    [Fact]
    public void Dispose_ReleasesNativeMemory_NoException()
    {
        var sink = new RamSink(capacity: 4_096);
        sink.Enqueue([1, 2, 3]);
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledTwice_IsIdempotent()
    {
        var sink = new RamSink(capacity: 4_096);
        sink.Dispose();
        var act = () => sink.Dispose();
        act.Should().NotThrow();
    }
}
```

- [ ] **Step 2: Run — expect compile error**

```bash
dotnet test tests/Relay.Tests --filter "RamSinkTests" 2>&1 | head -10
```

- [ ] **Step 3: Implement RamSink**

`src/Relay/Sinks/RamSink.cs`:

```csharp
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Relay.Sinks;

/// <summary>
/// Last-resort <see cref="PacketSink"/> fallback backed by a fixed-size native memory buffer.
/// No consumer thread — <see cref="Accept"/> writes synchronously on the producer thread.
/// Call <see cref="DrainTo"/> from the recovery path once the primary sink recovers.
/// </summary>
/// <remarks>
/// <para><b>Thread contract:</b> SPSC non-concurrent. <see cref="Accept"/> runs on the producer
/// thread; <see cref="DrainTo"/> on the recovery thread. NEVER simultaneously. The caller
/// guarantees producer quiescence before invoking <see cref="DrainTo"/>. No CAS —
/// <c>Volatile.Write</c>/<c>Volatile.Read</c> on <c>_head</c>/<c>_tail</c> suffice.</para>
/// <para><b>Layout:</b> Fill-once non-circular. Records fill <c>_buffer[0.._capacity]</c>
/// linearly. Record = <c>[uint32 length (host order)][payload][padding to 4-byte multiple]</c>.
/// <c>recordSize = 4 + ((payloadLen + 3) &amp; ~3)</c>. Length is host-order because the buffer
/// is process-local (never crosses wire).</para>
/// <para><b>Capacity:</b> Partial drain does NOT free capacity. Only full drain
/// (<c>_head &gt;= _tail</c>) resets the pointers to zero, reopening the buffer.</para>
/// </remarks>
public sealed unsafe class RamSink : PacketSink
{
    private readonly byte* _buffer;
    private readonly int   _capacity;

    private int _head;  // consumer-owned; advanced by DrainTo
    private int _tail;  // producer-owned; advanced by Accept

    private bool _disposed;

    /// <param name="capacity">Buffer size in bytes. Must be a positive power of two.</param>
    public RamSink(int capacity = 4 * 1024 * 1024)
    {
        if (capacity <= 0 || (capacity & (capacity - 1)) != 0)
            throw new ArgumentException("Capacity must be a positive power of two.", nameof(capacity));
        _capacity = capacity;
        _buffer   = (byte*)NativeMemory.AlignedAlloc((nuint)capacity, 64);
    }

    /// <summary>
    /// True when <c>_tail &lt; _capacity</c> — conservative approximation. <see cref="Accept"/>
    /// is authoritative for fit: a large payload may still overflow when <see cref="IsHealthy"/>
    /// is true, in which case <see cref="Accept"/> returns false and the payload falls through to
    /// <see cref="PacketSink.Next"/>.
    /// </summary>
    public override bool IsHealthy => _tail < _capacity;

    /// <summary>
    /// Writes length prefix + payload at absolute position <c>_tail</c>. Returns false when the
    /// record would exceed the buffer. Fill-once: partial drain does NOT free capacity — only
    /// a full <see cref="DrainTo"/> (which resets both pointers to zero) reopens the buffer.
    /// Safe to call from the producer thread only.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(ReadOnlySpan<byte> payload)
    {
        int alignedLen = (payload.Length + 3) & ~3;
        int recordSize = 4 + alignedLen;

        // Absolute-position check (not _tail - _head): after a partial drain, _head may have
        // advanced but _tail stays put. Writing at _buffer + _tail with _tail ~= _capacity
        // would overflow the buffer. Only a full drain reset frees _tail.
        if (_tail + recordSize > _capacity) return false;

        *(uint*)(_buffer + _tail) = (uint)payload.Length;
        fixed (byte* src = payload)
            Unsafe.CopyBlockUnaligned(_buffer + _tail + 4, src, (uint)payload.Length);

        Volatile.Write(ref _tail, _tail + recordSize);
        return true;
    }

    /// <summary>
    /// Replays all buffered payloads to <paramref name="target"/> in order.
    /// Stops if <paramref name="target"/> becomes unhealthy. Resets head and tail when fully drained.
    /// Call from the recovery consumer thread only — never concurrently with <see cref="Accept"/>.
    /// </summary>
    public void DrainTo(PacketSink target)
    {
        while (_head < Volatile.Read(ref _tail))
        {
            if (!target.IsHealthy) return;

            int len        = (int)*(uint*)(_buffer + _head);
            int alignedLen = (len + 3) & ~3;

            target.Enqueue(new ReadOnlySpan<byte>(_buffer + _head + 4, len));
            Volatile.Write(ref _head, _head + 4 + alignedLen);
        }

        // Full drain complete — reset pointers so the buffer can be reused.
        if (_head >= _tail)
        {
            _head = 0;
            Volatile.Write(ref _tail, 0);
        }
    }

    /// <inheritdoc/>
    public override void Flush() { }   // No consumer thread; nothing to flush.

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMemory.AlignedFree(_buffer);
    }
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Relay.Tests --filter "RamSinkTests"
```

Expected: 6 passed, 0 failed.

- [ ] **Step 5: Run full suite**

```bash
dotnet test tests/Relay.Tests
```

Expected: all tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/Relay/ tests/Relay.Tests/
git commit -m "feat: add RamSink native circular buffer with DrainTo for last-resort fallback w/Claude"
```

---

## Task 15: Benchmarks + gate verification + merge

**Files:**
- Create: `benchmarks/Relay.Benchmarks/PacketSinks/TcpSinkBenchmark.cs`
- Create: `benchmarks/Relay.Benchmarks/PacketSinks/FileSinkBenchmark.cs`
- Create: `benchmarks/Relay.Benchmarks/PacketSinks/ChainBenchmark.cs`

- [ ] **Step 1: Create packet sink benchmarks**

`benchmarks/Relay.Benchmarks/PacketSinks/TcpSinkBenchmark.cs`:

```csharp
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Relay.Sinks;

namespace Relay.Benchmarks.PacketSinks;

[MemoryDiagnoser]
public class TcpSinkBenchmark
{
    private TcpListener?             _listener;
    private TcpClient?               _server;
    private CancellationTokenSource? _drainCts;
    private Task?                    _drainTask;
    private TcpSink?                 _sink;
    private byte[]                   _payload = new byte[128];

    [GlobalSetup]
    public void Setup()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        int port = ((IPEndPoint)_listener.LocalEndpoint).Port;

        // Kick off accept BEFORE starting the client sink so Connect() sees a listener backlog.
        var acceptTask = _listener.AcceptTcpClientAsync();

        _sink = new TcpSink("127.0.0.1", port);
        _sink.Start();

        // Block until the server side of the connection is established.
        _server    = acceptTask.GetAwaiter().GetResult();
        _drainCts  = new CancellationTokenSource();
        _drainTask = Task.Run(() => DrainLoop(_server, _drainCts.Token));
    }

    private static void DrainLoop(TcpClient client, CancellationToken token)
    {
        byte[] buf = new byte[4096];
        var stream = client.GetStream();
        try
        {
            while (!token.IsCancellationRequested)
            {
                int n = stream.Read(buf, 0, buf.Length);
                if (n == 0) break;
            }
        }
        catch { /* socket torn down on cleanup — expected */ }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _drainCts?.Cancel();
        _sink?.Stop(1_000);
        _server?.Close();
        _listener?.Stop();
        _listener?.Server.Dispose();
        _drainTask?.Wait(500);
        _drainCts?.Dispose();
    }

    private const int BatchSize = 1024;

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void TcpSink_Enqueue_128B()
    {
        for (int i = 0; i < BatchSize; i++)
            _sink!.Enqueue(_payload);
    }
}
```

`benchmarks/Relay.Benchmarks/PacketSinks/ChainBenchmark.cs`:

```csharp
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Relay.Builder;
using Relay.Sinks;

namespace Relay.Benchmarks.PacketSinks;

[MemoryDiagnoser]
public class ChainBenchmark
{
    private TcpSink?  _tcp;
    private RamSink?  _ram;
    private TcpSink?  _tcp2;
    private TcpSink?  _tcp3;
    private TcpListener? _l1, _l2, _l3;
    private TcpClient?   _s1, _s2, _s3;
    private CancellationTokenSource? _drainCts;
    private Task?        _drainTask1, _drainTask2, _drainTask3;
    private byte[]       _payload = new byte[128];

    [GlobalSetup]
    public void Setup()
    {
        _l1 = MakeListener(); _l2 = MakeListener(); _l3 = MakeListener();

        // Kick off all three accepts before starting client sinks.
        var a1 = _l1.AcceptTcpClientAsync();
        var a2 = _l2.AcceptTcpClientAsync();
        var a3 = _l3.AcceptTcpClientAsync();

        _tcp  = new TcpSink("127.0.0.1", Port(_l1));
        _ram  = new RamSink();
        _tcp2 = new TcpSink("127.0.0.1", Port(_l2));
        _tcp3 = new TcpSink("127.0.0.1", Port(_l3));

        SinkChainBuilder.Start(_tcp).To(_ram);
        _tcp.Start();
        _tcp2.Start();
        _tcp3.Start();

        // Block until all three server-side connections are established.
        _s1 = a1.GetAwaiter().GetResult();
        _s2 = a2.GetAwaiter().GetResult();
        _s3 = a3.GetAwaiter().GetResult();

        _drainCts  = new CancellationTokenSource();
        _drainTask1 = Task.Run(() => DrainLoop(_s1, _drainCts.Token));
        _drainTask2 = Task.Run(() => DrainLoop(_s2, _drainCts.Token));
        _drainTask3 = Task.Run(() => DrainLoop(_s3, _drainCts.Token));
    }

    private static void DrainLoop(TcpClient client, CancellationToken token)
    {
        byte[] buf = new byte[4096];
        var stream = client.GetStream();
        try
        {
            while (!token.IsCancellationRequested)
            {
                int n = stream.Read(buf, 0, buf.Length);
                if (n == 0) break;
            }
        }
        catch { /* socket torn down on cleanup — expected */ }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _drainCts?.Cancel();
        _tcp?.Stop(500); _tcp2?.Stop(500); _tcp3?.Stop(500);
        _s1?.Close(); _s2?.Close(); _s3?.Close();
        _l1?.Stop(); _l1?.Server.Dispose();
        _l2?.Stop(); _l2?.Server.Dispose();
        _l3?.Stop(); _l3?.Server.Dispose();
        _drainTask1?.Wait(500); _drainTask2?.Wait(500); _drainTask3?.Wait(500);
        _drainCts?.Dispose();
    }

    private const int BatchSize = 1024;

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    public void TcpSink_NoPropagation()
    {
        for (int i = 0; i < BatchSize; i++)
            _tcp!.Enqueue(_payload);
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void FanOut_2Sinks()
    {
        for (int i = 0; i < BatchSize; i++)
        {
            _tcp2!.Enqueue(_payload);
            _tcp3!.Enqueue(_payload);
        }
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void SerializeSink_Overhead()
    {
        // MemoryMarshal.AsBytes bridge overhead vs direct packet enqueue
        var bridge = new SerializeSink<long>(_tcp!);
        long val = 42L;
        for (int i = 0; i < BatchSize; i++)
            bridge.Enqueue(in val);
    }

    private static TcpListener MakeListener()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        return l;
    }

    private static int Port(TcpListener l)
        => ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
}
```

- [ ] **Step 2: Run all benchmarks and save results**

```bash
cd benchmarks/Relay.Benchmarks
dotnet run -c Release -- --filter "*PacketSink*" --exporters json \
  --artifacts ../../benchmarks/baselines
```

Rename the output JSON to `benchmarks/baselines/2026-04-24-after-phase1-camada2.json`.

- [ ] **Step 3: Verify gates**

Open both JSON files and compare:

| Metric | Gate | Pass? |
|---|---|---|
| `TcpSink_Enqueue_128B` ns/op | ≤ `TcpSinkTyped_Enqueue` × 1.10 | — |
| `B/op` all packet benchmarks | 0 | — |
| `Gen0` all packet benchmarks | 0 | — |

If any gate fails: investigate before merging. Do NOT proceed with merge until gates pass.

- [ ] **Step 4: Run full test suite — final gate**

```bash
dotnet test tests/Relay.Tests
```

Expected: all tests pass, 0 failures.

- [ ] **Step 5: Commit benchmarks**

```bash
git add benchmarks/
git commit -m "test: add PacketSink BDN benchmarks and record post-phase1 baseline w/Claude"
```

- [ ] **Step 6: Merge to develop**

```bash
git checkout develop
git merge --no-ff feature/260424-packet-sink-phase1 \
  -m "feat: Relay Fase 1 — PacketSink hierarchy, concrete sinks, SinkChainBuilder w/Claude"
git push origin develop
```

---

## Camada summary

| Camada | Tasks | Gate |
|---|---|---|
| 1 — infraestrutura | 1–10 | `dotnet test` 0 failures + IL verification of PropagateAfterAccept |
| 2 — concretos | 11–14 | per-task tests + BDN gate ≤ +10% vs typed baseline |
| Closure | 15 | all tests + benchmark gate + merge to develop |

Fase 2 (Log2 → Relay adapter) may begin after Task 10 completes and Camada 2 benchmarks are green.
