using System.Net;
using Relay.Receivers;

namespace Relay.Builder;

public static partial class RelayBuilder
{
    /// <summary>
    /// Creates a <see cref="UdpReceiver{TState}"/> bound to <paramref name="local"/>.
    /// The receiver is passive — call <see cref="UdpReceiver{TState}.Poll"/> from the coordination loop.
    /// </summary>
    /// <typeparam name="TState">Caller state threaded into <paramref name="callback"/> — avoids closure allocation.</typeparam>
    /// <example>
    /// <code>
    /// // Static lambda = no capture; TState = CoordinationEngine (ref type, no boxing)
    /// var udpRecv = RelayBuilder.From(localEp, engine,
    ///     static (eng, frame) => eng.HandlePacket(frame));
    /// </code>
    /// </example>
    public static UdpReceiver<TState> From<TState>(
        IPEndPoint             local,
        TState                 state,
        PacketCallback<TState> callback,
        PacketSink?            next             = null,
        int                    kernelBufferSize = 1 << 20)
        => new(local, state, callback, next, kernelBufferSize);

    /// <summary>
    /// Creates a <see cref="TcpReceiver{TState}"/> bound to <paramref name="local"/>.
    /// Call <see cref="TcpReceiver{TState}.Accept"/> once from the management thread, then
    /// <see cref="TcpReceiver{TState}.Poll"/> from the coordination loop.
    /// </summary>
    /// <typeparam name="TState">Caller state threaded into <paramref name="callback"/> — avoids closure allocation.</typeparam>
    public static TcpReceiver<TState> FromTcp<TState>(
        IPEndPoint             local,
        TState                 state,
        PacketCallback<TState> callback,
        PacketSink?            next             = null,
        int                    bufferSize       = 65_536,
        int                    kernelBufferSize = 1 << 20)
        => new(local, state, callback, next, bufferSize, kernelBufferSize);
}
