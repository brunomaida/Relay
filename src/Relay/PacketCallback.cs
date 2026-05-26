using System;

namespace Relay;

/// <summary>
/// Zero-alloc callback delegate for received frames.
/// <para>
/// <c>Action&lt;ReadOnlySpan&lt;byte&gt;&gt;</c> is illegal in C# — spans cannot be generic type arguments
/// on standard delegates. This custom delegate captures <typeparamref name="TState"/> as a typed
/// parameter instead of a captured closure, eliminating allocation and enabling static-lambda dispatch.
/// </para>
/// </summary>
/// <typeparam name="TState">Caller-owned state passed through on each invocation. Avoids closure allocation.</typeparam>
/// <param name="state">Caller state (e.g., a <c>CoordinationEngine</c> reference).</param>
/// <param name="frame">Received frame bytes. Valid for the duration of the call only — do not store.</param>
public delegate void PacketCallback<TState>(TState state, ReadOnlySpan<byte> frame);
