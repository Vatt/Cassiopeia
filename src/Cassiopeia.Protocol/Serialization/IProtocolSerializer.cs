namespace Cassiopeia.Protocol.Serialization
{
    
    public interface IProtocolSerializer<T>
    {
        static abstract short GroupId { get; }
        static abstract short Id { get; }
        static abstract bool TryParse(ref ProtocolReader reader, out T message);
        static abstract void Write(ref ProtocolWriter writer, in T message);
    }
}
