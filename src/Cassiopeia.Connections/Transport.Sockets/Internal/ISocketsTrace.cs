using Microsoft.Extensions.Logging;

namespace Cassiopeia.Connections.Transport.Sockets.Internal;

internal interface ISocketsTrace : ILogger
{
    void ConnectionReadFin(SocketConnection connection);

    void ConnectionWriteFin(SocketConnection connection, string reason);

    void ConnectionError(SocketConnection connection, Exception ex);

    void ConnectionReset(string connectionId);
    void ConnectionReset(SocketConnection connection);

    void ConnectionPause(SocketConnection connection);

    void ConnectionResume(SocketConnection connection);
}
