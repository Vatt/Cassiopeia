using Cassiopeia.Protocol.Messages;
using Microsoft.AspNetCore.Connections;

namespace Cassiopeia.Core.Network;

internal class ClientConnection : Connection
{
    private ClientHello clientInfo;
    public ClientConnection(ClientHello client, long id, ConnectionContext context, ConnectionManager connectionManager, INetworkTrace logger) : base(id, context, connectionManager, logger)
    {
        clientInfo = client;
    }

    public override Task Run()
    {
        return Task.CompletedTask;
    }
}
