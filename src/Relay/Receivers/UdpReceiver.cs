using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Relay.Receivers;

/// <summary>
/// Hot-path UDP receiver. Non-blocking <see cref="Poll"/> via <c>Socket.Poll(0, SelectRead)</c>.
/// <para>
/// Buffer strategy: <c>stackalloc byte[1432]</c> per <see cref="Poll"/> call — MTU-safe (1500 B
/// Ethernet - 20 B IP - 8 B UDP = 1472; 1432 adds 40 B margin for tunnels/VXLAN). Stack allocation
/// is cache-hot, zero GC, and automatically reclaimed after each Poll — matches fTL DispatchReceiver.
/// </para>
/// <para>Caller owns the coordination loop and thread assignment.</para>
/// </summary>
/// <typeparam name="TState">
/// Caller state threaded into <paramref name="callback"/> on each frame — avoids closure allocation.
/// Use a static lambda: <c>static (eng, frame) => eng.HandlePacket(frame)</c>.
/// </typeparam>
public sealed class UdpReceiver<TState> : PacketReceiver
{
    private readonly Socket                 _socket;
    private readonly TState                 _state;
    private readonly PacketCallback<TState> _callback;

    /// <param name="local">Local bind endpoint; port 0 = OS-assigned ephemeral port.</param>
    /// <param name="state">Caller state passed to <paramref name="callback"/> on each frame.</param>
    /// <param name="callback">Invoked synchronously for each received frame. Must not store the span.</param>
    /// <param name="next">Optional: dispatched frame also forwarded to this sink.</param>
    /// <param name="kernelBufferSize">Kernel RX buffer size in bytes (default 1 MB).</param>
    public UdpReceiver(
        IPEndPoint             local,
        TState                 state,
        PacketCallback<TState> callback,
        PacketSink?            next             = null,
        int                    kernelBufferSize = 1 << 20)
    {
        _state    = state;
        _callback = callback;
        Next      = next;

        _socket = new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        _socket.Blocking          = false;
        _socket.ReceiveBufferSize = kernelBufferSize;
        _socket.Bind(local);
    }

    /// <summary>Actual local endpoint after OS bind — advertise to senders.</summary>
    public IPEndPoint LocalEndPoint => (IPEndPoint)_socket.LocalEndPoint!;

    /// <summary>
    /// Non-blocking poll. Receives at most one datagram per call.
    /// Returns <c>true</c> when a frame was received and the callback invoked.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Poll()
    {
        if (!_socket.Poll(0, SelectMode.SelectRead)) return false;

        Span<byte> frame = stackalloc byte[1432]; // MTU-safe, cache-hot, zero GC
        int n = _socket.Receive(frame, SocketFlags.None);
        frame = frame[..n];

        _callback(_state, frame);
        Next?.Enqueue(frame);
        return true;
    }

    /// <inheritdoc/>
    public override void Dispose() => _socket.Dispose();
}
