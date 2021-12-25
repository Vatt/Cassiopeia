using Cassiopeia.Core.Broker;
using Microsoft.Extensions.Logging;
using System.Net;

var loggerFactory = LoggerFactory.Create(builder =>
{
#if DEBUG
    builder.SetMinimumLevel(LogLevel.Debug);
#else
    builder.SetMinimumLevel(LogLevel.Information);
#endif
    builder.AddConsole();
});
var options = new BrokerOptions(loggerFactory)
{
    ListenEndpoint = new IPEndPoint(IPAddress.Loopback, 15174),
    MaxMessageSize = 5242880,
};
var brocker = new Broker(options);
await brocker.StartAync();
await brocker.ExecutionTask;