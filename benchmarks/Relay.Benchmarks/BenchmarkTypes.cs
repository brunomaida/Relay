using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
internal sealed class SinkPipe : DispatchPipe<Entry64>
{
    public override bool IsHealthy => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in Entry64 item) => true;

    public override void Flush() { }
    public override void Dispose() { }
}

/// <summary>Always-unhealthy pipe — forces every Enqueue to fall through to Next.</summary>
internal sealed class DeadPipe : DispatchPipe<Entry64>
{
    public override bool IsHealthy => false;

    // Accept is never called: IsHealthy == false short-circuits Enqueue.
    protected override bool Accept(in Entry64 item) => true;

    public override void Flush() { }
    public override void Dispose() { }
}

/// <summary>Healthy pipe that rejects every item — forces fallback via Accept returning false.</summary>
internal sealed class RejectPipe : DispatchPipe<Entry64>
{
    public override bool IsHealthy => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in Entry64 item) => false;

    public override void Flush() { }
    public override void Dispose() { }
}
