using Cassiopeia.Protocol.Serialization;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Cassiopeia.Protocol
{
    public static class CassiopeiaProtocol
    {
        /*
         * Wrap message: GroupId Id Size Message
         * GroupId: byte
         * Id: byte
         * Size: int
         * Message: original message
         */
        public static void WriteMessageWithHeader<T>(IBufferWriter<byte> output, in T message) where T : IProtocolSerializer<T>
        {
            var writer = new ProtocolWriter(output);
            WriteMessageWithHeader(ref writer, message, commit: true);
        }
        /*
         * Wrap message: GroupId Id Size Message
         * GroupId: byte
         * Id: byte
         * Size: int
         * Message: original message
         */
        public static void WriteMessageWithHeader<T>(ref ProtocolWriter writer, in T message, bool commit = false) where T : IProtocolSerializer<T>
        {
            writer.WriteInt16(T.GroupId);
            writer.WriteInt16(T.Id);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            T.Write(ref writer, message);
            reserved.Write(writer.Written - checkpoint);
            if (commit)
            {
                writer.Commit();
            }
        }
        public static bool TryReadMessage<T>(ReadOnlySequence<byte> input, [MaybeNullWhen(false)] out T message, out SequencePosition position) where T : IProtocolSerializer<T>
        {
            message = default;
            position = default;
            var reader = new ProtocolReader(input);
            if (!T.TryParse(ref reader, out message))
            {
                return false;
            }
            position = reader.Position;
            return true;

        }
    }
}
