using System.Buffers;
using Cassiopeia.Buffers;
using Cassiopeia.Protocol.Attributes;
using Cassiopeia.Protocol.Serialization;

namespace Cassiopeia.Protocol.Messages;

[CassiopeiaProtocol(0, 1)]
public readonly partial record struct AddUser(string Username, string Password);

