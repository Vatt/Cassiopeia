using Microsoft.AspNetCore.Connections.Features;
using System.Net.Sockets;

namespace Cassiopeia.Connections.Transport.Sockets.Internal;

internal sealed partial class SocketConnection : IConnectionSocketFeature
{
    public Socket Socket => _socket;

    private void InitializeFeatures()
    {
        _currentIConnectionSocketFeature = this;
    }
}
