using System;
using System.Reflection;
using FluentAssertions;
using Relay.Buffers;
using Relay.Memory;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.Memory;

/// <summary>
/// Footprint tests: assert the *actual* committed native block size (via <see cref="NativeSizeProbe"/>,
/// which reads the CRT heap directly) matches the intended size for each of the 4 <see cref="NativeBuffer"/>
/// call sites. A recomputation-only assertion (e.g. re-deriving <c>capacity * sizeof(T)</c> and comparing
/// it to itself) would pass even if the call site allocated the wrong amount — these tests read the real
/// allocation, so they would have caught the Wave <c>AllocZeroed(count, elementSize)</c> argument-swap bug.
/// </summary>
public sealed unsafe class NativeAllocationSizeTests
{
    [WindowsOracleFact]
    public void Probe_Calibration_ReportsSaneSizeForKnownAllocation()
    {
        NativeSizeProbe.IsCalibrated.Should().BeTrue(NativeSizeProbe.CalibrationFailureReason);
    }

    private static void* GetPrivatePointer(object instance, string fieldName)
    {
        FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on {instance.GetType()}.");
        object boxed = field.GetValue(instance)
            ?? throw new InvalidOperationException($"Field '{fieldName}' on {instance.GetType()} was null.");
        return Pointer.Unbox(boxed);
    }

    /// <summary>Allocators round up to alignment/page granularity; band tolerates that without being tautological.</summary>
    private static void AssertInBand(nuint actual, nuint expected)
    {
        actual.Should().BeGreaterThanOrEqualTo(expected);
        actual.Should().BeLessThan(expected * 2 + 4096);
    }

    private static void AssertSpscBlockSize<T>(int capacity) where T : unmanaged
    {
        using var ring = new SpscRingBuffer<T>(capacity);
        void* basePtr = GetPrivatePointer(ring, "_basePtr");
        nuint expected = (nuint)(capacity * sizeof(T));
        AssertInBand(NativeSizeProbe.AlignedMsize(basePtr), expected);
    }

    private static void AssertMpscBlockSize<T>(int capacity) where T : unmanaged
    {
        using var ring = new MpscRingBuffer<T>(capacity);
        void* basePtr = GetPrivatePointer(ring, "_basePtr");
        int   stride  = 64 + sizeof(T);
        nuint expected = (nuint)(capacity * stride);
        AssertInBand(NativeSizeProbe.AlignedMsize(basePtr), expected);
    }

    private static void AssertMemorySinkTypedBlockSize<T>(int capacity) where T : unmanaged
    {
        using var sink = new MemorySink<T>(capacity);
        void* buffer = GetPrivatePointer(sink, "_buffer");
        nuint expected = (nuint)(capacity * sizeof(T));
        AssertInBand(NativeSizeProbe.Msize(buffer), expected);
    }

    // --- SpscRingBuffer<T> — aligned family, expected = capacity * sizeof(T) ---

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void SpscRingBuffer_Payload1_BlockSizeMatchesCapacityTimesSizeOfT(int capacity) =>
        AssertSpscBlockSize<Payload1>(capacity);

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void SpscRingBuffer_Payload8_BlockSizeMatchesCapacityTimesSizeOfT(int capacity) =>
        AssertSpscBlockSize<Payload8>(capacity);

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void SpscRingBuffer_Payload64_BlockSizeMatchesCapacityTimesSizeOfT(int capacity) =>
        AssertSpscBlockSize<Payload64>(capacity);

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void SpscRingBuffer_Payload256_BlockSizeMatchesCapacityTimesSizeOfT(int capacity) =>
        AssertSpscBlockSize<Payload256>(capacity);

    // --- MpscRingBuffer<T> — aligned family, expected = capacity * (64 + sizeof(T)) stride ---

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void MpscRingBuffer_Payload1_BlockSizeMatchesCapacityTimesStride(int capacity) =>
        AssertMpscBlockSize<Payload1>(capacity);

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void MpscRingBuffer_Payload8_BlockSizeMatchesCapacityTimesStride(int capacity) =>
        AssertMpscBlockSize<Payload8>(capacity);

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void MpscRingBuffer_Payload64_BlockSizeMatchesCapacityTimesStride(int capacity) =>
        AssertMpscBlockSize<Payload64>(capacity);

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void MpscRingBuffer_Payload256_BlockSizeMatchesCapacityTimesStride(int capacity) =>
        AssertMpscBlockSize<Payload256>(capacity);

    // --- MemorySink<T> — unaligned family, expected = capacity * sizeof(T) ---

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void MemorySinkTyped_Payload1_BlockSizeMatchesCapacityTimesSizeOfT(int capacity) =>
        AssertMemorySinkTypedBlockSize<Payload1>(capacity);

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void MemorySinkTyped_Payload8_BlockSizeMatchesCapacityTimesSizeOfT(int capacity) =>
        AssertMemorySinkTypedBlockSize<Payload8>(capacity);

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void MemorySinkTyped_Payload64_BlockSizeMatchesCapacityTimesSizeOfT(int capacity) =>
        AssertMemorySinkTypedBlockSize<Payload64>(capacity);

    [OracleTheory]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(1024)]
    public void MemorySinkTyped_Payload256_BlockSizeMatchesCapacityTimesSizeOfT(int capacity) =>
        AssertMemorySinkTypedBlockSize<Payload256>(capacity);

    // --- MemorySink (Packet) — aligned family, byte-addressed, expected = capacity ---

    [OracleTheory]
    [InlineData(4)]
    [InlineData(4096)]
    [InlineData(1 << 20)]
    public void MemorySinkPacket_BlockSizeMatchesCapacityBytes(int capacity)
    {
        using var sink = new MemorySink(capacity);
        void* buffer = GetPrivatePointer(sink, "_buffer");
        AssertInBand(NativeSizeProbe.AlignedMsize(buffer), (nuint)capacity);
    }

    // --- Regression coverage for the exact Wave bug shape ---

    [Fact]
    public void NativeBuffer_AllocZeroedAligned_SwappedArguments_Throws()
    {
        // The Wave bug: NativeMemory.AllocZeroed(bufferBytes, (nuint)sizeof(T)) — a 2-arg
        // (elementCount, elementSize) calloc call misused with (bytes, alignment) arguments.
        // NativeBuffer's alignment validation catches the swap: a byte count passed as
        // "alignment" is almost always > 4096 or not a power of two.
        Action act = () => NativeBuffer.AllocZeroedAligned(byteCount: 64, alignment: 4_194_304);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NativeBuffer_AllocZeroedAligned_RejectsNonPowerOfTwoAlignment()
    {
        Action act = () => NativeBuffer.AllocZeroedAligned(byteCount: 256, alignment: 100);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
