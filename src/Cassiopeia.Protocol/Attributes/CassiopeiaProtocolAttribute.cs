namespace Cassiopeia.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class CassiopeiaProtocolAttribute : Attribute
{
    public CassiopeiaProtocolAttribute(byte GroupId, byte Id)
    {

    }
}
