using System;

namespace Relay;

/// <summary>
/// Abstract base for inbound frame receivers.
/// Receivers are passive — the caller's coordination loop drives them by calling <see cref="Poll"/>.
/// Non-blocking: <see cref="Poll"/> returns immediately when no data is available.
/// </summary>
public abstract class PacketReceiver : IDisposable
{
    /// <summary>Optional forward-chain: dispatched frame is also sent to this sink after the callback.</summary>
    public PacketSink? Next { get; init; }

    /// <summary>
    /// Attempts to receive one frame. Non-blocking.
    /// Returns <c>true</c> when a frame was received and the callback invoked; <c>false</c> when no data.
    /// </summary>
    public abstract bool Poll();

    /// <inheritdoc/>
    public abstract void Dispose();
}
