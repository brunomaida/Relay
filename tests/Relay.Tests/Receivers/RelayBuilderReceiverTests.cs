using System;
using System.Net;
using FluentAssertions;
using Relay;
using Relay.Builder;
using Relay.Receivers;
using Xunit;

namespace Relay.Tests.Receivers;

public sealed class RelayBuilderReceiverTests
{
    [Fact]
    public void RelayBuilder_From_ReturnsUdpReceiver()
    {
        using var recv = RelayBuilder.From(
            new IPEndPoint(IPAddress.Loopback, 0),
            state: 0,
            callback: static (_, _) => { });

        recv.Should().BeOfType<UdpReceiver<int>>();
        recv.LocalEndPoint.Port.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RelayBuilder_FromTcp_ReturnsTcpReceiver()
    {
        using var recv = RelayBuilder.FromTcp(
            new IPEndPoint(IPAddress.Loopback, 0),
            state: 0,
            callback: static (_, _) => { });

        recv.Should().BeOfType<TcpReceiver<int>>();
        recv.LocalEndPoint.Port.Should().BeGreaterThan(0);
    }
}
