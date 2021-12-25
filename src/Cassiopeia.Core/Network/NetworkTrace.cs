using Microsoft.Extensions.Logging;

namespace Cassiopeia.Core.Network
{
    public partial class NetworkTrace : INetworkTrace
    {

        protected readonly ILogger _generalLogger;
        protected readonly ILogger _connectionsLogger;
        public NetworkTrace(ILoggerFactory loggerFactory)
        {
            _generalLogger = loggerFactory.CreateLogger("Cassiopeia.Network");
            _connectionsLogger = loggerFactory.CreateLogger("Cassiopeia.Network.Connections");
        }

        [LoggerMessage(39, LogLevel.Debug, @"Connection id ""{ConnectionId}"" accepted.", EventName = "ConnectionAccepted")]
        private static partial void ConnectionAccepted(ILogger logger, string connectionId);

        public virtual void ConnectionAccepted(string connectionId)
        {
            ConnectionAccepted(_connectionsLogger, connectionId);
        }

        [LoggerMessage(1, LogLevel.Debug, @"Connection id ""{ConnectionId}"" started.", EventName = "ConnectionStart")]
        private static partial void ConnectionStart(ILogger logger, string connectionId);

        public virtual void ConnectionStart(string connectionId)
        {
            ConnectionStart(_connectionsLogger, connectionId);
        }

        [LoggerMessage(2, LogLevel.Debug, @"Connection id ""{ConnectionId}"" stopped.", EventName = "ConnectionStop")]
        private static partial void ConnectionStop(ILogger logger, string connectionId);

        public virtual void ConnectionStop(string connectionId)
        {
            ConnectionStop(_connectionsLogger, connectionId);
        }

        [LoggerMessage(4, LogLevel.Debug, @"Connection id ""{ConnectionId}"" paused.", EventName = "ConnectionPause")]
        private static partial void ConnectionPause(ILogger logger, string connectionId);

        public virtual void ConnectionPause(string connectionId)
        {
            ConnectionPause(_connectionsLogger, connectionId);
        }

        [LoggerMessage(5, LogLevel.Debug, @"Connection id ""{ConnectionId}"" resumed.", EventName = "ConnectionResume")]
        private static partial void ConnectionResume(ILogger logger, string connectionId);

        public virtual void ConnectionResume(string connectionId)
        {
            ConnectionResume(_connectionsLogger, connectionId);
        }

        [LoggerMessage(9, LogLevel.Debug, @"Connection id ""{ConnectionId}"" completed keep alive response.", EventName = "ConnectionKeepAlive")]
        private static partial void ConnectionKeepAlive(ILogger logger, string connectionId);

        public virtual void ConnectionKeepAlive(string connectionId)
        {
            ConnectionKeepAlive(_connectionsLogger, connectionId);
        }

        [LoggerMessage(24, LogLevel.Warning, @"Connection id ""{ConnectionId}"" rejected because the maximum number of concurrent connections has been reached.", EventName = "ConnectionRejected")]
        private static partial void ConnectionRejected(ILogger logger, string connectionId);

        public virtual void ConnectionRejected(string connectionId)
        {
            ConnectionRejected(_connectionsLogger, connectionId);
        }

        [LoggerMessage(10, LogLevel.Debug, @"Connection id ""{ConnectionId}"" disconnecting.", EventName = "ConnectionDisconnect")]
        private static partial void ConnectionDisconnect(ILogger logger, string connectionId);

        public virtual void ConnectionDisconnect(string connectionId)
        {
            ConnectionDisconnect(_connectionsLogger, connectionId);
        }

        //13
        //18
        [LoggerMessage(16, LogLevel.Debug, "Some connections failed to close gracefully during server shutdown.", EventName = "NotAllConnectionsClosedGracefully")]
        private static partial void NotAllConnectionsClosedGracefully(ILogger logger);

        public virtual void NotAllConnectionsClosedGracefully()
        {
            NotAllConnectionsClosedGracefully(_connectionsLogger);
        }

        //20 free event 
        [LoggerMessage(21, LogLevel.Debug, "Some connections failed to abort during server shutdown.", EventName = "NotAllConnectionsAborted")]
        private static partial void NotAllConnectionsAborted(ILogger logger);

        public virtual void NotAllConnectionsAborted()
        {
            NotAllConnectionsAborted(_connectionsLogger);
        }

        [LoggerMessage(22, LogLevel.Warning, @"As of ""{now}"", the heartbeat has been running for ""{heartbeatDuration}"" which is longer than ""{interval}"". This could be caused by thread pool starvation.", EventName = "HeartbeatSlow")]
        private static partial void HeartbeatSlow(ILogger logger, DateTimeOffset now, TimeSpan heartbeatDuration, TimeSpan interval);

        public virtual void HeartbeatSlow(TimeSpan heartbeatDuration, TimeSpan interval, DateTimeOffset now)
        {
            // while the heartbeat does loop over connections, this log is usually an indicator of threadpool starvation
            HeartbeatSlow(_generalLogger, now, heartbeatDuration, interval);
        }

        //23 
        //32-33 
        //28

        [LoggerMessage(25, LogLevel.Error, @"Connection id ""{ConnectionId}"" : handshake failed (""{reason}"").", EventName = "HandshakeFailed")]
        private static partial void HandshakeFailed(ILogger logger, string connectionId, string reason);
        public virtual void HandshakeFailed(string connectionId, string reason)
        {
            HandshakeFailed(_connectionsLogger, connectionId, reason);
        }

        [LoggerMessage(26, LogLevel.Information, @"Connection id ""{ConnectionId}"" : handshake complete.", EventName = "HandshakeComplete")]
        private static partial void HandshakeComplete(ILogger logger, string connectionId);
        public virtual void HandshakeComplete(string connectionId)
        {
            HandshakeComplete(_connectionsLogger, connectionId);
        }
        [LoggerMessage(34, LogLevel.Information, @"Connection id ""{ConnectionId}"", Request id ""{TraceIdentifier}"": the application aborted the connection.", EventName = "ApplicationAbortedConnection")]
        private static partial void ApplicationAbortedConnection(ILogger logger, string connectionId, string traceIdentifier);

        public virtual void ApplicationAbortedConnection(string connectionId, string traceIdentifier)
        {
            ApplicationAbortedConnection(_connectionsLogger, connectionId, traceIdentifier);
        }
        //29
        //36
        //48
        //30
        //35
        //31
        //38
        //37
        //49
        //40
        //41-47
        //50-53
        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _generalLogger.Log(logLevel, eventId, state, exception, formatter);

        public virtual bool IsEnabled(LogLevel logLevel) => _generalLogger.IsEnabled(logLevel);

        public virtual IDisposable BeginScope<TState>(TState state) => _generalLogger.BeginScope(state);

        public void HPackDecodingError(string connectionId, int streamId, Exception ex)
        {
            throw new NotImplementedException();
        }

        public void QPackDecodingError(string connectionId, long streamId, Exception ex)
        {
            throw new NotImplementedException();
        }

        public void QPackEncodingError(string connectionId, long streamId, Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
