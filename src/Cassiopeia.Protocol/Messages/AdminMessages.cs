using System.Buffers;
using Cassiopeia.Buffers;
using Cassiopeia.Protocol.Attributes;
using Cassiopeia.Protocol.Serialization;

namespace Cassiopeia.Protocol.Messages;

// [CassiopeiaProtocol(0, 1)]
public readonly partial record struct AddUser(string Username, string Password);


public readonly partial record struct AddUser : IProtocolSerializer<AddUser>
{
    public static short GroupId => 0;
    public static short Id => 1;
    public static bool TryParse(ref Cassiopeia.Buffers.BufferReader reader, out Cassiopeia.Protocol.Messages.AddUser message)
    {
        message = default;
        if (!reader.TryReadString(out var StringUsername))
        {
            return false;
        }

        if (!reader.TryReadString(out var StringPassword))
        {
            return false;
        }

        message = new Cassiopeia.Protocol.Messages.AddUser(Username: StringUsername, Password: StringPassword);
        return true;
    }

    public static void Write<TWriter>(ref BufferWriter<TWriter> writer, in AddUser message) where TWriter : IBufferWriter<byte>
    {
        writer.WriteString(message.Username);
        writer.WriteString(message.Password);
    }
}
