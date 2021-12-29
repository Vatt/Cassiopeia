using Cassiopeia.Buffers;
using System.Buffers;

namespace Cassiopeia.Protocol.Serialization
{

    public interface IProtocolSerializer<T>
    {
        static abstract short GroupId { get; }
        static abstract short Id { get; }
        static abstract bool TryParse(ref BufferReader reader, out T message);
        static abstract void Write<TWriter>(ref BufferWriter<TWriter> writer, in T message) where TWriter : IBufferWriter<byte>;
    }
}
