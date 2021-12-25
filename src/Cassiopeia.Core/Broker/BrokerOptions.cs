using Microsoft.Extensions.Logging;
using System.Net;

namespace Cassiopeia.Core.Broker;

public class BrokerOptions
{
    public EndPoint ListenEndpoint { get; set; } = new IPEndPoint(IPAddress.Loopback, 15174);
    public int MaxMessageSize { get; set; } = 5242880; //5 MB
    public ILoggerFactory LoggerFactory { get; }
    public BrokerOptions(ILoggerFactory factory)
    {
        LoggerFactory = factory;
    }
}
