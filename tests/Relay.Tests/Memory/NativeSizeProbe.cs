using System;
using System.Runtime.InteropServices;
using Relay.Memory;
using Xunit;

namespace Relay.Tests.Memory;

/// <summary>
/// Oracle for the actual committed size of a native allocation, via ucrtbase.dll's
/// <c>_msize</c>/<c>_aligned_msize</c>. Reads what the CRT allocator actually reserved — not a
/// recomputation of the caller's intended size — so a test built on this oracle can catch a
/// call-site regression that would compute an identical "intended size" to the bug shape
/// (e.g. <c>count * elementSize</c> either way).
/// </summary>
internal static unsafe class NativeSizeProbe
{
    private static readonly delegate* unmanaged[Cdecl]<void*, nuint>        _msize;
    private static readonly delegate* unmanaged[Cdecl]<void*, nuint, nuint, nuint> _alignedMsize;

    /// <summary>True once load + the 1&nbsp;MiB known-allocation calibration both succeeded.</summary>
    internal static readonly bool IsCalibrated;

    /// <summary>Explains why <see cref="IsCalibrated"/> is false. Empty when calibrated.</summary>
    internal static readonly string CalibrationFailureReason = string.Empty;

    static NativeSizeProbe()
    {
        try
        {
            nint lib = NativeLibrary.Load("ucrtbase.dll");
            _msize        = (delegate* unmanaged[Cdecl]<void*, nuint>)NativeLibrary.GetExport(lib, "_msize");
            _alignedMsize = (delegate* unmanaged[Cdecl]<void*, nuint, nuint, nuint>)NativeLibrary.GetExport(lib, "_aligned_msize");
        }
        catch (Exception ex)
        {
            CalibrationFailureReason = $"Failed to load ucrtbase.dll _msize/_aligned_msize exports: {ex.Message}";
            return;
        }

        const nuint oneMiB = 1024 * 1024;
        void* probe = NativeBuffer.AllocZeroed(oneMiB);
        try
        {
            nuint reported = _msize(probe);
            if (reported < oneMiB || reported >= oneMiB * 2)
            {
                CalibrationFailureReason =
                    $"_msize reported {reported} bytes for a known {oneMiB}-byte allocation (expected [{oneMiB}, {oneMiB * 2}))";
                return;
            }
        }
        finally
        {
            NativeBuffer.Free(probe, oneMiB);
        }

        IsCalibrated = true;
    }

    /// <summary>Actual committed size of an unaligned allocation, per the CRT heap.</summary>
    internal static nuint Msize(void* ptr) => _msize(ptr);

    /// <summary>Actual committed size of an aligned allocation, per the CRT heap.</summary>
    internal static nuint AlignedMsize(void* ptr) => _alignedMsize(ptr, 0, 0);
}

/// <summary>
/// <see cref="FactAttribute"/> that dynamically skips when <see cref="NativeSizeProbe.IsCalibrated"/>
/// is false, rather than letting every footprint assertion downstream silently pass against a
/// broken oracle. <see cref="FactAttribute.Skip"/> is evaluated by xUnit at discovery time, after
/// this constructor runs, so setting it here from a runtime check is honored normally.
/// </summary>
internal sealed class OracleFactAttribute : FactAttribute
{
    public OracleFactAttribute()
    {
        if (!NativeSizeProbe.IsCalibrated)
            Skip = NativeSizeProbe.CalibrationFailureReason;
    }
}

/// <summary>Theory counterpart of <see cref="OracleFactAttribute"/>.</summary>
internal sealed class OracleTheoryAttribute : TheoryAttribute
{
    public OracleTheoryAttribute()
    {
        if (!NativeSizeProbe.IsCalibrated)
            Skip = NativeSizeProbe.CalibrationFailureReason;
    }
}
