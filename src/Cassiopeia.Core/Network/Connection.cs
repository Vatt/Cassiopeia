using Cassiopeia.Connections.Transport.Abstractions;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System.IO.Pipelines;

namespace Cassiopeia.Core.Network;

internal abstract class Connection : IThreadPoolWorkItem
{
    public BaseConnectionContext TransportConnection { get; }
    public IDuplexPipe Transport { get; }
    public string ConnectionId => TransportConnection.ConnectionId;
    private INetworkTrace Logger { get; }
    public Task ExecutionTask => _completionTcs.Task;

    private ConnectionManager _connectionManager;

    private readonly CancellationTokenSource _connectionClosingCts = new CancellationTokenSource();
    private readonly TaskCompletionSource _completionTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);


    private readonly long _id;
    private bool _completed;
    public Connection(long id, BaseConnectionContext context, ConnectionManager connectionManager, INetworkTrace logger)
    {
        switch (context)
        {
            case TransportConnection ctx:
                Transport = ctx.Transport;
                break;
            case TransportMultiplexedConnection multiplexed:
                Transport = multiplexed.Application;
                break;
            default:
                context.Abort();
                throw new ArgumentException($"Unknown connection context, connection id {context.ConnectionId}");
        }
        TransportConnection = context;
        _connectionManager = connectionManager;
        Logger = logger;
        _id = id;
        _completed = false;
    }

    public abstract Task Run();

    void IThreadPoolWorkItem.Execute()
    {
        _ = ExecuteAsync();
    }

    internal async Task ExecuteAsync()
    {
        try
        {
            Logger.ConnectionStart(TransportConnection.ConnectionId);
            using (BeginConnectionScope(TransportConnection))
            {
                try
                {
                    await Run();
                }
                catch (Exception ex)
                {
                    Logger.LogError(0, ex, "Unhandled exception while processing {ConnectionId}.", TransportConnection.ConnectionId);
                }
            }
        }
        finally
        {
            _completed = true;

            Logger.ConnectionStop(TransportConnection.ConnectionId);

            // Dispose the transport connection, this needs to happen before removing it from the
            // connection manager so that we only signal completion of this connection after the transport
            // is properly torn down.
            await TransportConnection.DisposeAsync();

            _connectionManager.RemoveConnection(_id);
        }
    }
    public void RequestClose()
    {
        try
        {
            _connectionClosingCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // There's a race where the token could be disposed
            // swallow the exception and no-op
        }
    }

    public void Complete()
    {
        _completionTcs.TrySetResult();

        _connectionClosingCts.Dispose();
    }

    protected IDisposable? BeginConnectionScope(BaseConnectionContext connectionContext)
    {
        if (Logger.IsEnabled(LogLevel.Critical))
        {
            return Logger.BeginScope(new ConnectionLogScope(connectionContext.ConnectionId));
        }

        return null;
    }
}
