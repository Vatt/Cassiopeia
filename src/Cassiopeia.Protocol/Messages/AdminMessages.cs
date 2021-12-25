using Cassiopeia.Protocol.Attributes;

namespace Cassiopeia.Protocol.Messages;

[CassiopeiaProtocol(0, 1)]
public readonly partial record struct AddUser(string Username, string Password);
