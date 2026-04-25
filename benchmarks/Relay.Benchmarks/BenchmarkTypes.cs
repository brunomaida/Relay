using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Relay;

namespace Relay.Benchmarks;

/// <summary>64-byte cache-line-sized payload. Matches Entry64 in Relay.Tests.</summary>
[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct Entry64
{
    /// <summary>First 8-byte payload word.</summary>
    public long A;
    /// <summary>Second 8-byte payload word.</summary>
    public long B;
}

/// <summary>Healthy no-op pipe — zero overhead sink for baseline chains.</summary>
internal sealed class SinkPipe : DispatchSink<Entry64>
{
    public override bool IsHealthy => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in Entry64 item) => true;

    public override void Flush() { }
    public override void Dispose() { }
}

/// <summary>Always-unhealthy pipe — forces every Enqueue to fall through to Next.</summary>
internal sealed class DeadPipe : DispatchSink<Entry64>
{
    public override bool IsHealthy => false;

    // Accept is never called: IsHealthy == false short-circuits Enqueue.
    protected override bool Accept(in Entry64 item) => true;

    public override void Flush() { }
    public override void Dispose() { }
}

/// <summary>Healthy pipe that rejects every item — forces fallback via Accept returning false.</summary>
internal sealed class RejectPipe : DispatchSink<Entry64>
{
    public override bool IsHealthy => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in Entry64 item) => false;

    public override void Flush() { }
    public override void Dispose() { }
}

/// <summary>
/// Healthy pipe with an observable side-effect — prevents JIT dead-code elimination.
/// Accept writes item.A via Volatile.Write, making the call visible to the optimizer.
/// </summary>
internal sealed class CounterPipe : DispatchSink<Entry64>
{
    public long LastValue;

    public override bool IsHealthy => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in Entry64 item)
    {
        Volatile.Write(ref LastValue, item.A);
        return true;
    }

    public override void Flush() { }
    public override void Dispose() { }
}
