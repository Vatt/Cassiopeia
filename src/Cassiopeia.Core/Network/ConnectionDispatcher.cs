using Cassiopeia.Protocol;
using Cassiopeia.Protocol.Messages;
using Cassiopeia.Protocol.Serialization;
using Microsoft.AspNetCore.Connections;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.Versioning;

namespace Cassiopeia.Core.Network;


[RequiresPreviewFeatures]
internal class ConnectionDispatcher
{
    private readonly struct HandshakeResult
    {
        public readonly ServerHello? ServerHello;
        public readonly ClientHello? ClientHello;
        public readonly string? ErrorMessage;
        public HandshakeResult(ServerHello? serverHello, ClientHello? clientHello, string? errorMessage)
        {
            ServerHello = serverHello;
            ClientHello = clientHello;
            ErrorMessage = errorMessage;
        }
        public static HandshakeResult Create(ServerHello serverHello) => new HandshakeResult(serverHello, null, null);
        public static HandshakeResult Create(ClientHello clientHello) => new HandshakeResult(null, clientHello, null);
        public static HandshakeResult Create(string error) => new HandshakeResult(null, null, error);
    }
    private static readonly ServerHello ServerMessage = new ServerHello(0, 1, 4 * 1024);
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);
    public static async Task OnConnectedAsync(ConnectionContext context, ConnectionManager connectionManager, INetworkTrace logger)
    {
        var input = context.Transport.Input;
        var output = context.Transport.Output;
        Connection? connection = default;
        long id;
        var handshake = await HandshakeAsync(input, output).WaitAsync(Timeout).ConfigureAwait(false);
        if (handshake.ErrorMessage != null)
        {
            logger.HandshakeFailed(context.ConnectionId, handshake.ErrorMessage);
            context.Abort();
            input.Complete();
            output.Complete();
            await context.DisposeAsync(); //TODO:
            return;
        }
        if (handshake.ClientHello.HasValue == true)
        {
            id = connectionManager.GetNewConnectionId();
            logger.HandshakeComplete(context.ConnectionId);
            connection = new ClientConnection(handshake.ClientHello.Value, id, context, connectionManager, logger);
        }
        else if (handshake.ServerHello.HasValue == true)
        {
            id = connectionManager.GetNewConnectionId();
            logger.HandshakeComplete(context.ConnectionId);
            connection = new ServiceConnection(handshake.ServerHello.Value, connectionManager.GetNewConnectionId(), context, connectionManager, logger);
        }
        else
        {
            logger.HandshakeFailed(context.ConnectionId, "Unknown handshake result");
            context.Abort();
            input.Complete();
            output.Complete();
            await context.DisposeAsync(); //TODO:
            return;
        }
        id = connectionManager.GetNewConnectionId();
        connection = new ClientConnection(default, id, context, connectionManager, logger);
        connectionManager.AddConnection(id, connection);
        ThreadPool.UnsafeQueueUserWorkItem(connection, preferLocal: false);

    }
    private static async Task<HandshakeResult> HandshakeAsync(PipeReader input, PipeWriter output)
    {
        try
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var lockTask = LockInput(input, cts.Token);
            CassiopeiaProtocol.WriteMessageWithHeader(output, ServerMessage);
            await output.FlushAsync().ConfigureAwait(false);
            cts.Cancel();
            var lockResult = await lockTask.ConfigureAwait(false);
            if (lockResult == false)
            {
                return HandshakeResult.Create("Unknown input");
            }
            var result = await input.ReadAtLeastAsync(6).ConfigureAwait(false);
            if (result.IsCompleted)
            {
                return HandshakeResult.Create("Input complete");
            }
            var sequence = result.Buffer;
            var header = ReadHeader(sequence, out var position);
            sequence = sequence.Slice(position);
            if (header.Size > sequence.Length)
            {
                result = await input.ReadAtLeastAsync(header.Size).ConfigureAwait(false);
                sequence = result.Buffer;
            }
            if (header.GroupId != 1)//connection group
            {
                return HandshakeResult.Create("Bad GroupId");
            }
            switch (header.Id)
            {
                case 1:
                    if (!CassiopeiaProtocol.TryReadMessage<ServerHello>(sequence, out var serverHello, out position))
                    {
                        return HandshakeResult.Create("Cant read ServerHello");
                    }
                    input.AdvanceTo(position);
                    return HandshakeResult.Create(serverHello);
                case 2:
                    if (!CassiopeiaProtocol.TryReadMessage<ClientHello>(sequence, out var clientHello, out position))
                    {
                        return HandshakeResult.Create("Cant read ClientHello");
                    }
                    input.AdvanceTo(position);
                    return HandshakeResult.Create(clientHello);
                default:
                    return HandshakeResult.Create("Unknown header id");
            }
        }
        catch (Exception ex)
        {
            Debugger.Break();
            return HandshakeResult.Create(ex.Message);
        }



    }
    private static async ValueTask<bool> LockInput(PipeReader input, CancellationToken token)
    {
        while (token.IsCancellationRequested == false)
        {
            try
            {
                var readResult = await input.ReadAsync(token);
                input.AdvanceTo(readResult.Buffer.End);
                return false;
            }
            catch (OperationCanceledException ex)
            {
                return true;
            }

        }
        return true;
    }
    private static MessageHeader ReadHeader(ReadOnlySequence<byte> input, out SequencePosition consumed)
    {
        var reader = new ProtocolReader(input);
        reader.TryGetInt16(out var groupId);
        reader.TryGetInt16(out var id);
        reader.TryGetInt32(out var size);
        consumed = reader.Position;
        return new MessageHeader(groupId, id, size);
    }

}
