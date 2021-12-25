using Microsoft.AspNetCore.Connections;
using System.Collections.Concurrent;

namespace Cassiopeia.Core.Network
{
    internal class ConnectionManager
    {
        private long _connectionId;
        private readonly ConcurrentDictionary<long, Connection> _connections;

        public ConnectionManager(INetworkTrace log)
        {
            _connectionId = 0;
            _connections = new();
        }
        public long GetNewConnectionId()
        {
            return Interlocked.Increment(ref _connectionId);
        }
        public void AddConnection(long id, Connection connection)
        {
            if (!_connections.TryAdd(id, connection))
            {
                throw new ArgumentException("Unable to add connection.", nameof(id));
            }
        }
        public void RemoveConnection(long id)
        {
            if (!_connections.TryRemove(id, out var connection))
            {
                throw new ArgumentException("Unable to remove connection.", nameof(id));
            }

            connection.Complete();
        }
        public async Task<bool> CloseAllConnectionsAsync(CancellationToken token)
        {
            List<Task> closeTasks = new();
            foreach (var kvp in _connections)
            {
                var connection = kvp.Value;
                connection.RequestClose();
                closeTasks.Add(connection.ExecutionTask);
            }
            var allClosedTask = Task.WhenAll(closeTasks);
            return await Task.WhenAny(allClosedTask, CancellationTokenAsTask(token)).ConfigureAwait(false) == allClosedTask;
        }
        public async Task<bool> AbortAllConnectionsAsync()
        {
            var abortTasks = new List<Task>();

            foreach (var kvp in _connections)
            {
                var connection = kvp.Value;
                connection.TransportConnection.Abort(new ConnectionAbortedException("The connection was aborted because the server is shutting down and request processing didn't complete within the time specified by ShutdownTimeout."));
                abortTasks.Add(connection.ExecutionTask);
            }

            var allAbortedTask = Task.WhenAll(abortTasks.ToArray());
            return await Task.WhenAny(allAbortedTask, Task.Delay(1000)).ConfigureAwait(false) == allAbortedTask;
        }
        private static Task CancellationTokenAsTask(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            token.Register(() => tcs.SetResult());
            return tcs.Task;
        }
    }
}
