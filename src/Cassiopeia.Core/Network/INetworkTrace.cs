using Microsoft.Extensions.Logging;

namespace Cassiopeia.Core.Network;

public interface INetworkTrace : ILogger
{
    void ConnectionAccepted(string connectionId);

    void ConnectionStart(string connectionId);

    void ConnectionStop(string connectionId);

    void ConnectionPause(string connectionId);

    void ConnectionResume(string connectionId);

    void ConnectionRejected(string connectionId);

    void ConnectionKeepAlive(string connectionId);

    void ConnectionDisconnect(string connectionId);

    void NotAllConnectionsClosedGracefully();

    void NotAllConnectionsAborted();

    void HeartbeatSlow(TimeSpan heartbeatDuration, TimeSpan interval, DateTimeOffset now);

    void HandshakeFailed(string connectionId, string reason);

    void HandshakeComplete(string connectionId);

    void ApplicationAbortedConnection(string connectionId, string traceIdentifier);

    void HPackDecodingError(string connectionId, int streamId, Exception ex);

    void QPackDecodingError(string connectionId, long streamId, Exception ex);

    void QPackEncodingError(string connectionId, long streamId, Exception ex);

}
