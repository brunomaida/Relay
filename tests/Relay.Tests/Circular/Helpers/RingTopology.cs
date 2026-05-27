using System;
using Relay;

namespace Relay.Tests.Circular.Helpers;

internal sealed record RingNodeConfig(
    int  NodeCount,
    int  RingCapacity  = 1024,
    int  FlushMs       = 1,
    bool DecrementHops = true
);

internal sealed record PacketRingConfig(
    int NodeCount,
    int RingCapacity    = 65536,
    int FlushMs         = 1,
    int MaxPayloadBytes = 512
);

internal sealed class SpscRingTopology<T> : IDisposable where T : unmanaged
{
    public readonly SpscRingNode<T>[] Nodes;
    public SpscRingNode<T> Entry => Nodes[0];

    public SpscRingTopology(RingNodeConfig cfg)
    {
        if (cfg.NodeCount < 2)
            throw new ArgumentOutOfRangeException(nameof(cfg), "NodeCount must be >= 2.");

        Nodes = new SpscRingNode<T>[cfg.NodeCount];
        for (int i = 0; i < cfg.NodeCount; i++)
            Nodes[i] = new SpscRingNode<T>(cfg.RingCapacity, cfg.FlushMs, $"n{i}", cfg.DecrementHops);

        for (int i = 0; i < cfg.NodeCount; i++)
            Nodes[i].RingNext = Nodes[(i + 1) % cfg.NodeCount];
    }

    public void Start()
    {
        foreach (SpscRingNode<T> n in Nodes) n.Start();
    }

    public void Stop(int drainMs = 5_000)
    {
        foreach (SpscRingNode<T> n in Nodes) n.Stop(drainMs);
    }

    public long TotalCount()
    {
        long total = 0;
        foreach (SpscRingNode<T> n in Nodes) total += n.Count;
        return total;
    }

    public void Dispose()
    {
        foreach (SpscRingNode<T> n in Nodes) n.Dispose();
    }
}

internal sealed class InfiniteRingTopology<T> : IDisposable where T : unmanaged
{
    public readonly MpscRingNode<T> Entry;
    public readonly SpscRingNode<T>[] Rest;

    public InfiniteRingTopology(RingNodeConfig cfg)
    {
        if (cfg.NodeCount < 2)
            throw new ArgumentOutOfRangeException(nameof(cfg), "NodeCount must be >= 2.");

        Entry = new MpscRingNode<T>(cfg.RingCapacity, cfg.FlushMs, "n0", cfg.DecrementHops);
        Rest  = new SpscRingNode<T>[cfg.NodeCount - 1];

        for (int i = 0; i < Rest.Length; i++)
            Rest[i] = new SpscRingNode<T>(cfg.RingCapacity, cfg.FlushMs, $"n{i + 1}", cfg.DecrementHops);

        Entry.RingNext = Rest[0];
        for (int i = 0; i < Rest.Length - 1; i++)
            Rest[i].RingNext = Rest[i + 1];
        Rest[Rest.Length - 1].RingNext = Entry;
    }

    public void Start()
    {
        Entry.Start();
        foreach (SpscRingNode<T> n in Rest) n.Start();
    }

    public void Stop(int drainMs = 5_000)
    {
        foreach (SpscRingNode<T> n in Rest) n.Stop(drainMs);
        Entry.Stop(drainMs);
    }

    public long TotalCount()
    {
        long total = Entry.Count;
        foreach (SpscRingNode<T> n in Rest) total += n.Count;
        return total;
    }

    public void Dispose()
    {
        foreach (SpscRingNode<T> n in Rest) n.Dispose();
        Entry.Dispose();
    }
}

internal sealed class PacketRingTopology : IDisposable
{
    public readonly PacketRingNode[] Nodes;
    public PacketRingNode Entry => Nodes[0];

    public PacketRingTopology(PacketRingConfig cfg)
    {
        if (cfg.NodeCount < 2)
            throw new ArgumentOutOfRangeException(nameof(cfg), "NodeCount must be >= 2.");

        Nodes = new PacketRingNode[cfg.NodeCount];
        for (int i = 0; i < cfg.NodeCount; i++)
            Nodes[i] = new PacketRingNode(cfg.RingCapacity, cfg.FlushMs, $"pn{i}", cfg.MaxPayloadBytes);

        for (int i = 0; i < cfg.NodeCount; i++)
            Nodes[i].RingNext = Nodes[(i + 1) % cfg.NodeCount];
    }

    public void Start()
    {
        foreach (PacketRingNode n in Nodes) n.Start();
    }

    public void Stop(int drainMs = 5_000)
    {
        foreach (PacketRingNode n in Nodes) n.Stop(drainMs);
    }

    public long TotalCount()
    {
        long total = 0;
        foreach (PacketRingNode n in Nodes) total += n.Count;
        return total;
    }

    public void Dispose()
    {
        foreach (PacketRingNode n in Nodes) n.Dispose();
    }
}

internal sealed class PacketInfiniteRingTopology : IDisposable
{
    public readonly PacketMpscRingNode Entry;
    public readonly PacketRingNode[] Rest;

    public PacketInfiniteRingTopology(PacketRingConfig cfg)
    {
        if (cfg.NodeCount < 2)
            throw new ArgumentOutOfRangeException(nameof(cfg), "NodeCount must be >= 2.");

        Entry = new PacketMpscRingNode(cfg.RingCapacity, cfg.FlushMs, "pn0", cfg.MaxPayloadBytes);
        Rest  = new PacketRingNode[cfg.NodeCount - 1];

        for (int i = 0; i < Rest.Length; i++)
            Rest[i] = new PacketRingNode(cfg.RingCapacity, cfg.FlushMs, $"pn{i + 1}", cfg.MaxPayloadBytes);

        Entry.RingNext = Rest[0];
        for (int i = 0; i < Rest.Length - 1; i++)
            Rest[i].RingNext = Rest[i + 1];
        Rest[Rest.Length - 1].RingNext = Entry;
    }

    public void Start()
    {
        Entry.Start();
        foreach (PacketRingNode n in Rest) n.Start();
    }

    public void Stop(int drainMs = 5_000)
    {
        foreach (PacketRingNode n in Rest) n.Stop(drainMs);
        Entry.Stop(drainMs);
    }

    public long TotalCount()
    {
        long total = Entry.Count;
        foreach (PacketRingNode n in Rest) total += n.Count;
        return total;
    }

    public void Dispose()
    {
        foreach (PacketRingNode n in Rest) n.Dispose();
        Entry.Dispose();
    }
}
