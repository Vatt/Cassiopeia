using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Cassiopeia.Core.Network;

public class ConnectionListener
{
    private readonly IConnectionListener _listener;
    private readonly INetworkTrace _log;
    private readonly ConnectionManager _connectionManager;
    private readonly TaskCompletionSource _acceptLoopTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public ConnectionListener(IConnectionListener listener, INetworkTrace log)
    {
        _listener = listener;
        _log = log;
        _connectionManager = new ConnectionManager(log);
    }

    public Task StartAcceptingConnections()
    {
        // REVIEW: Multiple accept loops in parallel?
        ThreadPool.UnsafeQueueUserWorkItem(StartAcceptiongConnectionsCore, _listener, preferLocal: false);
        return _acceptLoopTcs.Task;
    }
    public async Task StopAsync(CancellationToken token)
    {
        await _listener.UnbindAsync(token).ConfigureAwait(false);
        await _acceptLoopTcs.Task.ConfigureAwait(false);
        if (!await _connectionManager.CloseAllConnectionsAsync(token).ConfigureAwait(false))
        {
            _log.NotAllConnectionsClosedGracefully();
            if (!await _connectionManager.AbortAllConnectionsAsync().ConfigureAwait(false))
            {
                _log.NotAllConnectionsAborted();
            }
        }
        await _listener.DisposeAsync().ConfigureAwait(false);
    }
    private void StartAcceptiongConnectionsCore(IConnectionListener listener)
    {
        _ = AcceptConnectionsAsync(listener);

        async Task AcceptConnectionsAsync(IConnectionListener listener)
        {
            try
            {
                while (true)
                {
                    var connection = await listener.AcceptAsync().ConfigureAwait(false);

                    if (connection == null)
                    {
                        // We're done listening
                        break;
                    }
                    _log.ConnectionAccepted(connection.ConnectionId);
                    _ = ConnectionDispatcher.OnConnectedAsync(connection, _connectionManager, _log);
                    //var id = _connectionManager.GetNewConnectionId();
                    //var newConnection = new ClientConnection(default, id, connection, _connectionManager, _log);
                    //_connectionManager.AddConnection(id, newConnection);
                    //ThreadPool.UnsafeQueueUserWorkItem(newConnection, preferLocal: false);
                }
            }
            catch (Exception ex)
            {
                // REVIEW: If the accept loop ends should this trigger a server shutdown? It will manifest as a hang
                _log.LogCritical(0, ex, "The connection listener failed to accept any new connections.");
            }
            finally
            {
                _acceptLoopTcs.TrySetResult();
            }
        }
    }
}
