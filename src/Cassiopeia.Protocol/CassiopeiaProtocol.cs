using Cassiopeia.Buffers;
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
        public static void WriteMessageWithHeader<TWriter,TMessage>(TWriter output, in TMessage message) 
            where TWriter : IBufferWriter<byte>
            where TMessage : IProtocolSerializer<TMessage>
        {
            var writer = new BufferWriter<TWriter>(output);
            WriteMessageWithHeader(ref writer, message, commit: true);
        }
        /*
         * Wrap message: GroupId Id Size Message
         * GroupId: byte
         * Id: byte
         * Size: int
         * Message: original message
         */
        public static void WriteMessageWithHeader<TWriter, TMessage>(ref BufferWriter<TWriter> writer, in TMessage message, bool commit = false)
            where TWriter : IBufferWriter<byte>
            where TMessage : IProtocolSerializer<TMessage>
        {
            writer.WriteInt16(TMessage.GroupId);
            writer.WriteInt16(TMessage.Id);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            TMessage.Write(ref writer, message);
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
            var reader = new BufferReader(input);
            if (!T.TryParse(ref reader, out message))
            {
                return false;
            }
            position = reader.Position;
            return true;

        }
    }
}
