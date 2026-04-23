using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Relay;
using Xunit;

namespace Relay.Tests;

/// <summary>Verifies PipeConstraints.AssertCacheLineAligned&lt;T&gt; accepts multiples of 64B and rejects all others.</summary>
public sealed class PipeConstraintsTests
{
    [Fact]
    public void SpscQueuePipe_Accepts_192B_Struct()
    {
        Action act = () =>
        {
            using var pipe = new InMemoryPipe192(ringCapacity: 8, flushIntervalMs: 50);
        };
        act.Should().NotThrow();
    }

#if DEBUG
    [Fact]
    public void SpscQueuePipe_Rejects_96B_Struct_In_Debug()
    {
        Action act = () => _ = new InMemoryPipe96(ringCapacity: 8, flushIntervalMs: 50);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*positive multiple of 64B*");
    }

    [Fact]
    public void SpscQueuePipe_Rejects_32B_Struct_In_Debug()
    {
        Action act = () => _ = new InMemoryPipe32(ringCapacity: 8, flushIntervalMs: 50);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*positive multiple of 64B*");
    }
#endif

    // --- payload structs ---

    [StructLayout(LayoutKind.Sequential, Size = 192)]
    private struct Entry192 { public long A; }

    [StructLayout(LayoutKind.Sequential, Size = 96)]
    private struct Entry96  { public long A; }

    [StructLayout(LayoutKind.Sequential, Size = 32)]
    private struct Entry32  { public long A; }

    // --- minimal SpscQueuePipe helpers, one per payload type ---

    private sealed class InMemoryPipe192 : SpscQueuePipe<Entry192>
    {
        public InMemoryPipe192(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(in Entry192 item) { }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class InMemoryPipe96 : SpscQueuePipe<Entry96>
    {
        public InMemoryPipe96(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(in Entry96 item) { }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }

    private sealed class InMemoryPipe32 : SpscQueuePipe<Entry32>
    {
        public InMemoryPipe32(int ringCapacity, int flushIntervalMs)
            : base(ringCapacity, flushIntervalMs, "test") { }

        protected override void WriteToBackend(in Entry32 item) { }
        protected override void FlushBackend()      { }
        protected override void TryRecoverBackend() { }
        protected override void DisposeBackend()    { }
    }
}
