using Cassiopeia.Protocol.Messages;
using Cassiopeia.Protocol.Serialization;
using Microsoft.AspNetCore.Connections;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.Versioning;

namespace Cassiopeia.Core.Network;
delegate void WriteDelegate(ref ProtocolWriter writer);
delegate bool TryParseDelegate(ref ProtocolReader reader);

[RequiresPreviewFeatures]
internal class ClientConnection : Connection
{
    private ClientHello clientInfo;
    public ClientConnection(ClientHello client, long id, ConnectionContext context, ConnectionManager connectionManager, INetworkTrace logger) : base(id, context, connectionManager, logger)
    {
        clientInfo = client;
    }

    public override async Task Run()
    {
        var incoming = ProcessIncoming();
        var outgoing = ProcessOutgoing();
        await Task.WhenAll(incoming, outgoing);
    }
    private async Task ProcessIncoming()
    {
        var input = Transport.Input;
        while (true)
        {

        }
    }
    private async Task ProcessOutgoing()
    {
        
    }
}
