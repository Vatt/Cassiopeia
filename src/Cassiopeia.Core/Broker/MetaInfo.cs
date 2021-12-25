using FASTER.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cassiopeia.Core.Broker;

internal class MetaInfo
{
    private readonly FasterKV<string, string> _usersKv;
    public MetaInfo()
    {
    }
}
