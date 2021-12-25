using FASTER.core;

namespace Cassiopeia.Core.Broker;

internal class MetaInfo
{
    private readonly FasterKV<string, string> _usersKv;
    public MetaInfo()
    {
    }
}
