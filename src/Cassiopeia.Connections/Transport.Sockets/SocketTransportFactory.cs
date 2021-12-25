using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Cassiopeia.Connections.Transport.Sockets;

/// <summary>
/// A factory for socket based connections.
/// </summary>
public sealed class SocketTransportFactory : IConnectionListenerFactory
{
    private readonly SocketTransportOptions _options;
    private readonly ILoggerFactory _logger;

    public SocketTransportFactory(IOptions<SocketTransportOptions> options, ILoggerFactory loggerFactory)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (loggerFactory == null)
        {
            throw new ArgumentNullException(nameof(loggerFactory));
        }

        _options = options.Value;
        _logger = loggerFactory;
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
    {
        var transport = new SocketConnectionListener(endpoint, _options, _logger);
        transport.Bind();
        return new ValueTask<IConnectionListener>(transport);
    }
}
