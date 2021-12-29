using System.Buffers;
using Cassiopeia.Buffers;
using Cassiopeia.Protocol.Attributes;
using Cassiopeia.Protocol.Serialization;

namespace Cassiopeia.Protocol.Messages;


// [CassiopeiaProtocol(1, 1)]
public readonly partial record struct ServerHello(short Major, short Minor, int PayloadChunkSize);
//
// [CassiopeiaProtocol(1, 2)]
public readonly partial record struct ClientHello(string Product, string Version, string Platform, string Information, string User, string Password, short Heartbeat, bool UseTls);
//
// [CassiopeiaProtocol(1, 3)]
public readonly partial record struct Ping(long Timestamp);




public readonly partial record struct ServerHello : IProtocolSerializer<ServerHello>
{
    public static short GroupId => 1;
    public static short Id => 1;
    public static bool TryParse(ref Cassiopeia.Buffers.BufferReader reader, out Cassiopeia.Protocol.Messages.ServerHello message)
    {
        message = default;
        if (!reader.TryReadInt16(out var Int16Major))
        {
            return false;
        }

        if (!reader.TryReadInt16(out var Int16Minor))
        {
            return false;
        }

        if (!reader.TryReadInt32(out var Int32PayloadChunkSize))
        {
            return false;
        }

        message = new Cassiopeia.Protocol.Messages.ServerHello(Major: Int16Major, Minor: Int16Minor, PayloadChunkSize: Int32PayloadChunkSize);
        return true;
    }

    public static void Write<TWriter>(ref BufferWriter<TWriter> writer, in ServerHello message) where TWriter : IBufferWriter<byte>
    {
        writer.WriteInt16(message.Major);
        writer.WriteInt16(message.Minor);
        writer.WriteInt32(message.PayloadChunkSize);
    }
}

public readonly partial record struct ClientHello: IProtocolSerializer<ClientHello>
{
    public static short GroupId => 1;
        public static short Id => 2;
        public static bool TryParse(ref Cassiopeia.Buffers.BufferReader reader, out Cassiopeia.Protocol.Messages.ClientHello message)
        {
            message = default;
            if (!reader.TryReadString(out var StringProduct))
            {
                return false;
            }

            if (!reader.TryReadString(out var StringVersion))
            {
                return false;
            }

            if (!reader.TryReadString(out var StringPlatform))
            {
                return false;
            }

            if (!reader.TryReadString(out var StringInformation))
            {
                return false;
            }

            if (!reader.TryReadString(out var StringUser))
            {
                return false;
            }

            if (!reader.TryReadString(out var StringPassword))
            {
                return false;
            }

            if (!reader.TryReadInt16(out var Int16Heartbeat))
            {
                return false;
            }

            if (!reader.TryReadBoolean(out var BooleanUseTls))
            {
                return false;
            }

            message = new Cassiopeia.Protocol.Messages.ClientHello(Product: StringProduct, Version: StringVersion, Platform: StringPlatform, Information: StringInformation, User: StringUser, Password: StringPassword, Heartbeat: Int16Heartbeat, UseTls: BooleanUseTls);
            return true;
        }

        public static void Write<TWriter>(ref BufferWriter<TWriter> writer, in ClientHello message) where TWriter : IBufferWriter<byte>
        {
            writer.WriteString(message.Product);
            writer.WriteString(message.Version);
            writer.WriteString(message.Platform);
            writer.WriteString(message.Information);
            writer.WriteString(message.User);
            writer.WriteString(message.Password);
            writer.WriteInt16(message.Heartbeat);
            writer.WriteBoolean(message.UseTls);
        }
}

public readonly partial record struct Ping: IProtocolSerializer<Ping>
{
    public static short GroupId => 1;
    public static short Id => 3;
    public static bool TryParse(ref Cassiopeia.Buffers.BufferReader reader, out Cassiopeia.Protocol.Messages.Ping message)
    {
        message = default;
        if (!reader.TryReadInt64(out var Int64Timestamp))
        {
            return false;
        }

        message = new Cassiopeia.Protocol.Messages.Ping(Timestamp: Int64Timestamp);
        return true;
    }

    public static void Write<TWriter>(ref BufferWriter<TWriter> writer, in Ping message) where TWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(message.Timestamp);
    }
}