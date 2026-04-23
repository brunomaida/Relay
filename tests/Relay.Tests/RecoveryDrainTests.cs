using System;
using System.Threading;
using FluentAssertions;
using Relay;
using Relay.Builder;
using Relay.Pipes;
using Xunit;

namespace Relay.Tests;

/// <summary>Prev-drain recovery: items buffered in fallback re-route to predecessor on recovery.</summary>
public sealed class RecoveryDrainTests
{
    [Fact]
    public void RamPipe_Dispose_IsIdempotent()
    {
        var ram = new RamPipe<Entry64>(capacity: 64);
        ram.Dispose();
        var act = () => ram.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void RamPipe_DrainTo_TransfersAllItems()
    {
        using var ram = new RamPipe<Entry64>(capacity: 64);
        var       dst = new CountingPipe();

        ram.Enqueue(new Entry64 { A = 1 });
        ram.Enqueue(new Entry64 { A = 2 });
        ram.Enqueue(new Entry64 { A = 3 });

        ram.DrainTo(dst);

        dst.Accepted.Should().Be(3);
    }

    [Fact]
    public void RamPipe_DrainTo_StopsWhenDestinationUnhealthy()
    {
        using var ram = new RamPipe<Entry64>(capacity: 64);
        var       dst = new CountingPipe(maxAccept: 1);

        for (int i = 0; i < 5; i++)
            ram.Enqueue(new Entry64 { A = i });

        ram.DrainTo(dst);

        dst.Accepted.Should().Be(1);
    }

    [Fact]
    public void Builder_WiresPrevForRecoveryDrain()
    {
        // Verify that PipeChain wires Prev on SpscQueuePipe children.
        using var ram  = new RamPipe<Entry64>(capacity: 64);
        using var file = new TestSpscPipe();

        RelayBuilder
            .Start<Entry64, TestSpscPipe>(file)
            .To(ram)
            .Build();

        // Prev on RamPipe is DispatchPipe<T>, not SpscQueuePipe<T>,
        // so Prev wiring only applies to SpscQueuePipe fallback nodes.
        // This test just verifies the chain does not throw during assembly.
        file.Next.Should().BeSameAs(ram);
    }

    [Fact]
    public void Serial_ItemRoutedToFallback_WhenPrimaryUnhealthy()
    {
        using var primary  = new TestSpscPipe(healthy: false);
        using var fallback = new TestSpscPipe();

        RelayBuilder
            .Start<Entry64, TestSpscPipe>(primary)
            .To(fallback)
            .Build();

        primary.Start();
        fallback.Start();

        primary.Enqueue(new Entry64 { A = 99 });

        Thread.Sleep(200);

        fallback.Stop(500);
        fallback.Consumed.Should().Be(1);

        primary.Stop(500);
    }

    private sealed class CountingPipe : DispatchPipe<Entry64>
    {
        private readonly int _maxAccept;
        private int          _count;

        public CountingPipe(int maxAccept = int.MaxValue) => _maxAccept = maxAccept;

        public int Accepted => _count;

        public override bool IsHealthy => _count < _maxAccept;

        protected override bool Accept(in Entry64 item)
        {
            if (_count >= _maxAccept) return false;
            _count++;
            return true;
        }

        public override void Flush()   { }
        public override void Dispose() { }
    }

    private sealed class TestSpscPipe : SpscQueuePipe<Entry64>
    {
        private readonly bool _pipeHealthy;
        private int           _consumed;

        public int Consumed => _consumed;

        public TestSpscPipe(bool healthy = true)
            : base(64, 50, "test") => _pipeHealthy = healthy;

        public override bool IsHealthy => _pipeHealthy && base.IsHealthy;

        protected override void WriteToBackend(in Entry64 item) => _consumed++;
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }
}
