// See https://aka.ms/new-console-template for more information

using System;
using Cassiopeia.Connections.Transport.Sockets.Client;
using Cassiopeia.IO.Mmap;
using Cassiopeia.Protocol;
using Cassiopeia.Protocol.Messages;
using Cassiopeia.Protocol.Serialization;
using FASTER.core;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;
using System.Buffers;
using System.Net;
using Cassiopeia.IO;

const int fileSize = 1 * 1024 * 1024 * 1024;
Memory<byte> data = new byte[1025];
for (int i = 0; i < data.Length; i++)
{
    if (i == 0)
    {
        data.Span[0] = 0;
    }
    else
    {
        data.Span[i] = (byte)((byte)i % (byte)255);
    }
}
var seq = new FileSequence($"{Environment.CurrentDirectory}/FileSequence", "CassiopeiaChunk", fileSize);
for (int i = 0; i < 10240000; i++)
{
    seq.WriteAsync(data);
}
//MmapFile file = new MmapFile($"{Environment.CurrentDirectory}/FileSequence/CassiopeiaChunk", fileSize);
//var span = file.Span;
//for(var i = 0; i < fileSize; i++)
//{
//    span[i] = 255;
//}
//file.Dispose();
struct TestData
{
    public string TestStr = "Test";
    public int Size = 1025;
    public Memory<byte> TestBytes = new byte[1025];
    public TestData()
    {
        TestStr = "Test";
        Size = 1025;
        TestBytes = new byte[Size];
        byte val = 0;
        for (var i = 0; i < Size; i++)
        {
            TestBytes.Span[i] = val;
            if (val == byte.MaxValue)
            {
                val = 0;
            }
            else
            {
                val += 1;
            }

        }
    }
}