using System;
using System.Runtime.CompilerServices;

namespace Relay;

/// <summary>
/// Synchronous broadcast pipe: delivers each item to all children on the caller thread.
/// <see cref="IsHealthy"/> is true when at least one child is healthy (short-circuit OR).
/// Always returns true from <see cref="Accept"/> — fallback to <see cref="DispatchSink{T}.Next"/>
/// only occurs when all children report unhealthy.
/// </summary>
public sealed class MultiSink<T> : DispatchSink<T> where T : unmanaged
{
    private readonly DispatchSink<T>[] _children;

    public MultiSink(params DispatchSink<T>[] children)
    {
        if (children is null || children.Length == 0)
            throw new ArgumentException("MultiSink requires at least one child.", nameof(children));
        for (int i = 0; i < children.Length; i++)
            if (children[i] is null)
                throw new ArgumentException($"MultiSink child at index {i} is null.", nameof(children));
        _children = children;
    }

    /// <summary>True when at least one child can accept items.</summary>
    public override bool IsHealthy
    {
        get
        {
            foreach (var c in _children)
                if (c.IsHealthy) return true;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in T item)
    {
        foreach (var c in _children) c.Enqueue(in item);
        return true;
    }

    public override void Flush()   { foreach (var c in _children) c.Flush(); }
    public override void Dispose() { foreach (var c in _children) c.Dispose(); }
}

/// <summary>
/// Fixed-arity 2-child broadcast with CRTP generic parameters.
/// When <typeparamref name="TC1"/> and <typeparamref name="TC2"/> are sealed, the JIT
/// devirtualizes and inlines both <see cref="DispatchSink{T}.Enqueue"/> calls — saves
/// ~6 cycles (2 indirect calls) vs the array-based <see cref="MultiSink{T}"/>.
/// </summary>
public sealed class Multi2Sink<T, TC1, TC2> : DispatchSink<T>
    where T   : unmanaged
    where TC1 : DispatchSink<T>
    where TC2 : DispatchSink<T>
{
    private readonly TC1 _c1;
    private readonly TC2 _c2;

    public Multi2Sink(TC1 c1, TC2 c2)
    {
        _c1 = c1 ?? throw new ArgumentNullException(nameof(c1));
        _c2 = c2 ?? throw new ArgumentNullException(nameof(c2));
    }

    public override bool IsHealthy => _c1.IsHealthy || _c2.IsHealthy;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override bool Accept(in T item)
    {
        _c1.Enqueue(in item);
        _c2.Enqueue(in item);
        return true;
    }

    public override void Flush()   { _c1.Flush();   _c2.Flush(); }
    public override void Dispose() { _c1.Dispose(); _c2.Dispose(); }
}
