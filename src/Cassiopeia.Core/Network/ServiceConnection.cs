using Cassiopeia.Protocol.Messages;
using Microsoft.AspNetCore.Connections;
using System.Runtime.Versioning;

namespace Cassiopeia.Core.Network;

[RequiresPreviewFeatures]
internal class ServiceConnection : Connection
{
    private ServerHello _serverInfo;
    public ServiceConnection(ServerHello server, long id, BaseConnectionContext context, ConnectionManager connectionManager, INetworkTrace logger) : base(id, context, connectionManager, logger)
    {
        _serverInfo = server;
    }

    public override Task Run()
    {
        throw new NotImplementedException();
    }
}
