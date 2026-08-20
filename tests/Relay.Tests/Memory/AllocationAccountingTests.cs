using System;
using System.Reflection;
using FluentAssertions;
using Relay.Buffers;
using Relay.Memory;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.Memory;

[CollectionDefinition("NativeAccounting", DisableParallelization = true)]
public sealed class NativeAccountingCollection
{
}

/// <summary>
/// Verifies <see cref="NativeBuffer"/>'s <c>[ThreadStatic]</c> outstanding-byte counters return to
/// their pre-test baseline after Dispose/finalization.
/// </summary>
/// <remarks>
/// <c>[ThreadStatic]</c> is correct when the constructor call and the assertions run on the same
/// thread (true for every test below). It is provably NOT correct for GC-driven finalization: the
/// CLR always runs finalizers on a dedicated finalizer thread, never the allocating thread
/// (verified locally with a throwaway repro — constructor ran on managed thread 2, the object's
/// finalizer ran on managed thread 1). A real <c>GC.Collect()</c> + <c>GC.WaitForPendingFinalizers()</c>
/// test would therefore decrement a <c>[ThreadStatic]</c> slot the test thread can never observe,
/// making the "returns to 0" assertion fail deterministically — not flaky, always red — regardless
/// of whether the production accounting is correct. <see cref="MemorySinkTyped_Finalizer_ReturnsOutstandingBytes"/>
/// below sidesteps this by invoking <c>Object.Finalize()</c> via reflection directly on the test
/// thread: this runs the exact same compiled finalizer body (the C# destructor lowers to an
/// override of <c>Finalize()</c>) without crossing threads, so the <c>[ThreadStatic]</c> design in
/// the brief did not need to change to <c>Interlocked</c> — see task-1-report.md for the full
/// writeup of this decision.
/// </remarks>
[Collection("NativeAccounting")]
public sealed class AllocationAccountingTests
{
    [Fact]
    public void SpscRingBuffer_Dispose_ReturnsAllOutstandingAlignedBytes()
    {
        long before = NativeBuffer.AlignedBytesOutstanding;

        var ring = new SpscRingBuffer<Payload64>(64);
        NativeBuffer.AlignedBytesOutstanding.Should().Be(before + 64 * 64);

        ring.Dispose();
        NativeBuffer.AlignedBytesOutstanding.Should().Be(before);
    }

    [Fact]
    public void MemorySinkTyped_Dispose_ReturnsAllOutstandingUnalignedBytes()
    {
        long before = NativeBuffer.UnalignedBytesOutstanding;

        var sink = new MemorySink<Payload64>(64);
        NativeBuffer.UnalignedBytesOutstanding.Should().Be(before + 64 * 64);

        sink.Dispose();
        NativeBuffer.UnalignedBytesOutstanding.Should().Be(before);
    }

    [Fact]
    public void MemorySinkTyped_Finalizer_ReturnsOutstandingBytes()
    {
        long before = NativeBuffer.UnalignedBytesOutstanding;

        var sink = new MemorySink<Payload64>(64);
        NativeBuffer.UnalignedBytesOutstanding.Should().Be(before + 64 * 64);

        // Runs the real finalizer body synchronously on this thread — see class remarks for why
        // GC.Collect()/WaitForPendingFinalizers() cannot be used to observe [ThreadStatic] accounting here.
        InvokeFinalizerSynchronously(sink);
        // Prevent the CLR from later running the real finalizer too (would double-free _buffer).
        GC.SuppressFinalize(sink);

        NativeBuffer.UnalignedBytesOutstanding.Should().Be(before);
    }

    private static void InvokeFinalizerSynchronously(object instance)
    {
        MethodInfo finalize = instance.GetType().GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Finalize() not found on {instance.GetType()}.");
        finalize.Invoke(instance, null);
    }
}
