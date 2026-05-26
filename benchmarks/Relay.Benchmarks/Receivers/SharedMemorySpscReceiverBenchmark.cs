using System;
using System.Runtime.Versioning;
using BenchmarkDotNet.Attributes;
using Relay.Receivers;
using Relay.Sinks;

namespace Relay.Benchmarks.Receivers;

/// <summary>
/// Measures <see cref="SharedMemorySpscReceiver{TState}.Poll"/> hot path:
/// <c>Volatile.Read</c> WriteIndex + ring-modular read + <c>Volatile.Write</c> ReadIndex.
/// Windows-only (named MMF).
/// </summary>
/// <remarks>
/// Three states benchmarked:
/// <list type="bullet">
///   <item><c>Poll_Empty</c> — WriteIdx == _readIndex; measures the early-exit gate.</item>
///   <item><c>Roundtrip_PerFrame</c> — paired Sink.Enqueue then Receiver.Poll for one frame (no-wrap fast path).</item>
///   <item><c>Roundtrip_64B</c> / <c>Roundtrip_1KiB</c> — payload size variants for memcpy cost.</item>
/// </list>
/// </remarks>
[SupportedOSPlatform("windows")]
[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(5)]
public class SharedMemorySpscReceiverBenchmark
{
    private SharedMemorySpscSink                                _sink     = null!;
    private SharedMemorySpscReceiver<SharedMemorySpscReceiverBenchmark> _receiver = null!;
    private byte[]                                              _payload  = null!;
    private string                                              _name     = string.Empty;
    private int                                                 _received;

    /// <summary>Payload size in bytes.</summary>
    [Params(64, 256, 1024)]
    public int PayloadSize;

    [GlobalSetup]
    public void Setup()
    {
        _name     = "Local\\relay-recv-bench-" + Guid.NewGuid().ToString("N");
        _sink     = new SharedMemorySpscSink(_name, totalCapacity: 4 * 1024 * 1024);
        _receiver = new SharedMemorySpscReceiver<SharedMemorySpscReceiverBenchmark>(
            _name,
            this,
            static (s, _) => s._received++,
            maxFrameSize: 4096);
        _payload  = new byte[PayloadSize];
        for (int i = 0; i < PayloadSize; i++) _payload[i] = (byte)i;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _receiver.Dispose();
        _sink.Dispose();
    }

    [Benchmark]
    public bool Poll_Empty() => _receiver.Poll();

    [Benchmark]
    public bool Roundtrip_PerFrame()
    {
        _sink.Enqueue(_payload);
        return _receiver.Poll();
    }
}
