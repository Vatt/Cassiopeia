using Cassiopeia.Buffers;

namespace Cassiopeia.Protocol.Serialization
{

    public interface IProtocolSerializer<T>
    {
        static abstract short GroupId { get; }
        static abstract short Id { get; }
        static abstract bool TryParse(ref BufferReader reader, out T message);
        static abstract void Write(ref BufferWriter writer, in T message);
    }
}
