using System.Buffers;
using Cassiopeia.Buffers;
using Cassiopeia.Protocol.Attributes;
using Cassiopeia.Protocol.Serialization;

namespace Cassiopeia.Protocol.Messages;


[CassiopeiaProtocol(1, 1)]
public readonly partial record struct ServerHello(short Major, short Minor, int PayloadChunkSize);

[CassiopeiaProtocol(1, 2)]
public readonly partial record struct ClientHello(string Product, string Version, string Platform, string Information, string User, string Password, short Heartbeat, bool UseTls);

[CassiopeiaProtocol(1, 3)]
public readonly partial record struct Ping(long Timestamp);