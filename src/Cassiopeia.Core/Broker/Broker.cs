using Cassiopeia.Connections.Transport.Sockets;
using Cassiopeia.Core.Network;
using Cassiopeia.Protocol.Messages;
using Cassiopeia.Protocol.Serialization;
using Microsoft.Extensions.Options;
using System.Runtime.Versioning;
using System.Threading.Channels;

namespace Cassiopeia.Core.Broker;

public class Broker
{
    private readonly TaskCompletionSource _executionTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    private ConnectionListener _listener;
    private Task _listenerTask;
    private readonly BrokerOptions _options;
    public Task ExecutionTask => _listenerTask;//_executionTcs.Task;
    public Broker(BrokerOptions options)
    {
        _options = options;
    }
    
    public async Task StartAync()
    {
        var logger = _options.LoggerFactory;
        var options = new SocketTransportOptions();
        //options.UnsafePreferInlineScheduling = true;
        SocketTransportFactory transportFactory = new SocketTransportFactory(Options.Create(options), logger);
        var transport = await transportFactory.BindAsync(_options.ListenEndpoint);
        _listener = new ConnectionListener(transport, new NetworkTrace(logger));
        _listenerTask = _listener.StartAcceptingConnections();
    }
    public async Task StopAync()
    {
        await _listener.StopAsync(default);
        _executionTcs.TrySetResult();
        await _listenerTask;
    }
}
