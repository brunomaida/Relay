namespace Relay.Builder;

/// <summary>Static entry points for building <see cref="PacketSink"/> fallback chains.</summary>
public static class SinkChainBuilder
{
    /// <summary>Starts a chain with any <see cref="PacketSink"/> head.</summary>
    public static SinkChain<THead> Start<THead>(THead head)
        where THead : PacketSink => new(head);

    /// <summary>Starts a chain with an SPSC queue sink head.</summary>
    public static SinkChain<THead> StartSpsc<THead>(THead head)
        where THead : SpscQueueSink => new(head);

    /// <summary>Starts a chain with an MPSC queue sink head.</summary>
    public static SinkChain<THead> StartMpsc<THead>(THead head)
        where THead : MpscQueueSink => new(head);
}
