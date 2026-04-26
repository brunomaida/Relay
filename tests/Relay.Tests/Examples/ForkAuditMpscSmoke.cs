using System;
using System.Threading;
using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests.Examples;

/// <summary>
/// End-to-end smoke example: 4 producer threads fan in through a <see cref="ForkSink{T}"/> that
/// routes every item to both an audit sink (side channel) and a main <see cref="MpscQueueSink{T}"/>
/// (primary delivery). Demonstrates that the typed MPSC + fork pattern is safe under multi-producer
/// load and that the audit and main paths see identical item counts.
/// </summary>
/// <remarks>
/// <para>Topology:</para>
/// <code>
///       4 producer threads
///              │
///              ▼
///   ┌─────────────────────────┐
///   │  ForkSink(audit=counter)│   PropagateAfterAccept = true
///   └─────────────────────────┘
///       │                  │
///       │ local accept     │ always propagate
///       ▼                  ▼
///   AuditCounter       MpscMainPipe (MPSC consumer drain)
///   (Interlocked)      (counts via WriteToBackend)
/// </code>
/// <para>
/// Audit counter uses <see cref="Interlocked.Increment(ref long)"/> because <see cref="DispatchSink{T}.Accept"/>
/// is called from N producer threads concurrently. Main pipe's counter is incremented on the
/// single consumer thread and uses a plain write.
/// </para>
/// </remarks>
public sealed class ForkAuditMpscSmoke
{
    [Fact]
    public void FourProducers_ForkWithAudit_BothPathsReceiveAllItems()
    {
        const int Producers   = 4;
        const int PerProducer = 50_000;
        const int Total       = Producers * PerProducer;

        using var main    = new MainCounterMpscPipe(ringCapacity: 1 << 20, flushIntervalMs: 25);
        var       audit   = new AuditCounterPipe();
        using var fork    = new ForkSink<Entry64>(audit);
        fork.Next         = main;

        main.Start();

        var barrier = new ManualResetEventSlim(false);
        var threads = new Thread[Producers];
        for (int p = 0; p < Producers; p++)
        {
            int pid = p;
            threads[p] = new Thread(() =>
            {
                var item = new Entry64 { A = pid };
                barrier.Wait();
                for (int i = 0; i < PerProducer; i++)
                    fork.Enqueue(in item);
            })
            { IsBackground = true };
            threads[p].Start();
        }

        barrier.Set();
        foreach (var t in threads) t.Join();

        // Let consumer thread drain the MPSC ring.
        main.Stop(drainTimeoutMs: 10_000);

        audit.Accepted.Should().Be(Total, "audit sink runs synchronously on every producer");
        main.Consumed.Should().Be(Total,  "main pipe consumer must drain every enqueued item");
    }

    /// <summary>Thread-safe synchronous counter used as a fork audit sink.</summary>
    private sealed class AuditCounterPipe : DispatchSink<Entry64>
    {
        private long _count;
        public long Accepted => Volatile.Read(ref _count);

        public override bool IsHealthy => true;

        protected override bool Accept(in Entry64 item)
        {
            Interlocked.Increment(ref _count);
            return true;
        }

        public override void Flush()   { }
        public override void Dispose() { }
    }

    /// <summary>MPSC-backed counter; increment runs on the single consumer thread.</summary>
    private sealed class MainCounterMpscPipe : MpscQueueSink<Entry64>
    {
        private long _consumed;
        public long Consumed => Volatile.Read(ref _consumed);

        public MainCounterMpscPipe(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "main") { }

        protected override void WriteToBackend(in Entry64 item) => _consumed++;
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }
}
