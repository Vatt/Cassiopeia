namespace Cassiopeia.Protocol.Messages;

public readonly record struct MessageHeader(short GroupId, short Id, int Size);
