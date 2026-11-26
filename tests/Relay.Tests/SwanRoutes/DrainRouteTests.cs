using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using FluentAssertions;
using Relay.Builder;
using Relay.Sinks;
using Xunit;

namespace Relay.Tests.SwanRoutes;

/// <summary>
/// Flow tests for the SWAN hot routes of Relay that enter at <see cref="DispatchSink{T}.Enqueue"/>, a <c>MemorySink.DrainTo</c>
/// or <see cref="SpscQueueSink{T}.EnqueueBatch"/> and exit through <see cref="FileSink"/>'s direct write (SWAN S5.5d): each
/// builds the chain its route's hops name, calls the first code hop in its body and asserts the file's bytes in its body.
/// The <see cref="FileSink"/> write buffer holds 1 byte, so every record bypasses batching and only
/// <c>FileSink.WriteToBackend</c>'s own <c>FileStream.Write</c> (<c>FileSink.cs:63</c>) lands bytes; with no deadline flush the
/// bytes are checked before <c>Stop</c>.
/// </summary>
[Trait("Category", "Integration")]
public sealed class DrainRouteTests : IDisposable
{
    private const int TimeoutMs    = 5_000;
    private const int NoDeadlineMs = 60_000; // no deadline flush runs within a test

    [StructLayout(LayoutKind.Sequential, Size = 64)]
    private struct Record64 { public long Value; }

    private static readonly Record64[] Records = [new() { Value = 0x0102_0304_0506_0708 }, new() { Value = 0x1112_1314_1516_1718 }];

    private readonly string _dir = Directory.CreateTempSubdirectory("relay-swan-").FullName;
    private string Path0 => Path.Combine(_dir, "drain.bin");

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    /// <summary>
    /// Route <c>scan.in.relay.dispatchsink-1.enqueue.-0-.out.system.io.filestream.write.0da4d73d</c>:
    /// <c>DispatchSink.Enqueue &gt; SerializeSink.Accept &gt; PacketSink.Enqueue &gt; SpscQueueSink.Accept &gt; ConsumeLoop &gt; FileSink.WriteToBackend</c>.
    /// </summary>
    [Fact]
    public void Enqueue_SerializeSinkIntoFileSink_WriteToBackendWritesRecord()
    {
        using var file = new FileSink(Path0, writeBufferCapacity: 1, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        var serialize  = new SerializeSink<Record64>(file);
        file.Start();

        serialize.Enqueue(in Records[0]);
        byte[] expected = Bytes(Records.AsSpan(0, 1));
        SpinWait.SpinUntil(() => Holds(Path0, expected), TimeoutMs).Should().BeTrue("WriteToBackend writes the oversized record directly");
        file.Stop(drainTimeoutMs: 1_000);

        SpinWait.SpinUntil(() => Holds(Path0, expected), TimeoutMs).Should().BeTrue("the bytes are unchanged after Stop");
        ReadShared(Path0).Should().Equal(expected);
    }

    /// <summary>
    /// Route <c>scan.in.relay.sinks.memorysink-1.drainto.relay.dispatchsink--0-.out.system.io.filestream.write.c53223d5</c>:
    /// <c>MemorySink&lt;T&gt;.DrainTo &gt; DispatchSink.Enqueue &gt; SerializeSink.Accept &gt; PacketSink.Enqueue &gt; SpscQueueSink.Accept &gt;
    /// ConsumeLoop &gt; FileSink.WriteToBackend</c>.
    /// </summary>
    [Fact]
    public void DrainTo_TypedMemorySinkIntoSerializeSink_WriteToBackendWritesRecords()
    {
        using var memory = new MemorySink<Record64>(capacity: 16);
        using var file   = new FileSink(Path0, writeBufferCapacity: 1, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        var serialize    = new SerializeSink<Record64>(file);
        memory.Enqueue(in Records[0]);
        memory.Enqueue(in Records[1]);
        file.Start();

        memory.DrainTo(serialize);
        byte[] expected = Bytes(Records);
        SpinWait.SpinUntil(() => Holds(Path0, expected), TimeoutMs).Should().BeTrue("WriteToBackend writes each drained record directly");
        file.Stop(drainTimeoutMs: 1_000);

        SpinWait.SpinUntil(() => Holds(Path0, expected), TimeoutMs).Should().BeTrue("the bytes are unchanged after Stop");
        ReadShared(Path0).Should().Equal(expected);
    }

    /// <summary>
    /// Route <c>scan.in.relay.sinks.memorysink.drainto.relay.packetsink.out.system.io.filestream.write.fe0f149a</c>:
    /// <c>MemorySink.DrainTo &gt; PacketSink.Enqueue &gt; SpscQueueSink.Accept &gt; ConsumeLoop &gt; FileSink.WriteToBackend</c>.
    /// </summary>
    [Fact]
    public void DrainTo_PacketMemorySinkIntoFileSink_WriteToBackendWritesPayloads()
    {
        byte[] first  = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
        byte[] second = [0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18];
        using var memory = new MemorySink(capacity: 1_024);
        using var file   = new FileSink(Path0, writeBufferCapacity: 1, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        memory.Enqueue(first);
        memory.Enqueue(second);
        file.Start();

        memory.DrainTo(file);
        byte[] expected = [.. first, .. second];
        SpinWait.SpinUntil(() => Holds(Path0, expected), TimeoutMs).Should().BeTrue("WriteToBackend writes each drained payload directly");
        file.Stop(drainTimeoutMs: 1_000);

        SpinWait.SpinUntil(() => Holds(Path0, expected), TimeoutMs).Should().BeTrue("the bytes are unchanged after Stop");
        ReadShared(Path0).Should().Equal(expected);
    }

    /// <summary>
    /// Route <c>scan.in.relay.spscqueuesink-1.enqueuebatch.system.readonlyspan--0-.out.system.io.filestream.write.56b91a52</c>:
    /// <c>SpscQueueSink&lt;T&gt;.EnqueueBatch &gt; DispatchSink.Enqueue &gt; SerializeSink.Accept &gt; PacketSink.Enqueue &gt;
    /// SpscQueueSink.Accept &gt; ConsumeLoop &gt; FileSink.WriteToBackend</c>. The head's constructor connect is refused (port bound,
    /// not listening), so the unhealthy head forwards the whole batch to its <c>Next</c>; the head is never started.
    /// </summary>
    [Fact]
    public void EnqueueBatch_UnhealthyHeadFallsBackToSerializeSink_WriteToBackendWritesRecords()
    {
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        using var head = new TcpSink<Record64>("127.0.0.1", ((IPEndPoint)listener.LocalEndPoint!).Port, ringCapacity: 16);
        head.IsHealthy.Should().BeFalse("the constructor's connect is refused while the port does not listen");
        using var file = new FileSink(Path0, writeBufferCapacity: 1, ringCapacity: 4_096, flushIntervalMs: NoDeadlineMs);
        RelayBuilder.StartSpsc<Record64, TcpSink<Record64>>(head).To(new SerializeSink<Record64>(file));
        file.Start();

        head.EnqueueBatch(Records);
        byte[] expected = Bytes(Records);
        SpinWait.SpinUntil(() => Holds(Path0, expected), TimeoutMs).Should().BeTrue("WriteToBackend writes each forwarded record directly");
        file.Stop(drainTimeoutMs: 1_000);

        SpinWait.SpinUntil(() => Holds(Path0, expected), TimeoutMs).Should().BeTrue("the bytes are unchanged after Stop");
        ReadShared(Path0).Should().Equal(expected);
    }

    private static byte[] Bytes(ReadOnlySpan<Record64> records) => MemoryMarshal.AsBytes(records).ToArray();

    // Stop ignores its Join result: the final flush can land after Stop returns.
    private static bool Holds(string path, byte[] expected) => ReadShared(path).AsSpan().SequenceEqual(expected);

    // The sink holds the file open for write: read with FileShare.ReadWrite.
    private static byte[] ReadShared(string path)
    {
        if (!File.Exists(path)) return [];
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var bytes = new byte[stream.Length];
        stream.ReadExactly(bytes);
        return bytes;
    }
}
