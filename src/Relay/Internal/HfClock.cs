using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Relay.Internal;

internal static class HfClock
{
    public static long NowTicks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Stopwatch.GetTimestamp();
    }
}
