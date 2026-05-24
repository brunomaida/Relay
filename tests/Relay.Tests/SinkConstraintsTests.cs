using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests;

/// <summary>Verifies SinkConstraints.AssertCacheLineAligned&lt;T&gt; accepts multiples of 64B and rejects all others.</summary>
public sealed class SinkConstraintsTests
{
    [Fact]
    public void SpscQueueSink_Accepts_192B_Struct()
    {
        Action act = () =>
        {
            using var pipe = new InMemoryPipe192(ringCapacity: 8, flushIntervalMs: 50);
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void SpscQueueSink_Rejects_96B_Struct()
    {
        Action act = () => _ = new InMemoryPipe96(ringCapacity: 8, flushIntervalMs: 50);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*positive multiple of 64B*");
    }

    [Fact]
    public void SpscQueueSink_Rejects_32B_Struct()
    {
        Action act = () => _ = new InMemoryPipe32(ringCapacity: 8, flushIntervalMs: 50);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*positive multiple of 64B*");
    }

    // --- payload structs ---

    [StructLayout(LayoutKind.Sequential, Size = 192)]
    private struct Entry192 { public long A; }

    [StructLayout(LayoutKind.Sequential, Size = 96)]
    private struct Entry96  { public long A; }

    [StructLayout(LayoutKind.Sequential, Size = 32)]
    private struct Entry32  { public long A; }

    [Fact]
    public void MpscQueueSink_Accepts_192B_Struct()
    {
        Action act = () =>
        {
            using var pipe = new MpscInMemoryPipe192(ringCapacity: 8, flushIntervalMs: 50);
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void MpscQueueSink_Rejects_96B_Struct()
    {
        Action act = () => _ = new MpscInMemoryPipe96(ringCapacity: 8, flushIntervalMs: 50);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*positive multiple of 64B*");
    }

    [Fact]
    public void MpscQueueSink_Rejects_32B_Struct()
    {
        Action act = () => _ = new MpscInMemoryPipe32(ringCapacity: 8, flushIntervalMs: 50);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*positive multiple of 64B*");
    }

    // --- minimal SpscQueueSink helpers, one per payload type ---

    private sealed class InMemoryPipe192 : SpscQueueSink<Entry192>
    {
        public InMemoryPipe192(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(in Entry192 item) { }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class InMemoryPipe96 : SpscQueueSink<Entry96>
    {
        public InMemoryPipe96(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(in Entry96 item) { }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class InMemoryPipe32 : SpscQueueSink<Entry32>
    {
        public InMemoryPipe32(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(in Entry32 item) { }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    // --- minimal MpscQueueSink helpers, one per payload type ---

    private sealed class MpscInMemoryPipe192 : MpscQueueSink<Entry192>
    {
        public MpscInMemoryPipe192(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(in Entry192 item) { }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class MpscInMemoryPipe96 : MpscQueueSink<Entry96>
    {
        public MpscInMemoryPipe96(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(in Entry96 item) { }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class MpscInMemoryPipe32 : MpscQueueSink<Entry32>
    {
        public MpscInMemoryPipe32(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(in Entry32 item) { }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }
}
