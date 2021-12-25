using DotNext.IO.MemoryMappedFiles;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cassiopeia.IO.Mmap;

public class MmapFile : IDisposable
{
    private readonly MemoryMappedFile _mmapFile;
    public int Size { get; }
    public Span<byte> Span => MemoryOwner.Bytes;
    public Memory<byte> Memory => MemoryOwner.Memory;
    public unsafe IMappedMemoryOwner MemoryOwner;
    public unsafe MmapFile(string path, int size)
    {
        Size = size;
        _mmapFile = MemoryMappedFile.CreateFromFile(path, FileMode.OpenOrCreate, null, size);
        MemoryOwner = _mmapFile.CreateMemoryAccessor(0, size);
    }
    public void Flush()
    {
        MemoryOwner.Flush();
    }
    public void Dispose()
    {
        MemoryOwner.Dispose();
        _mmapFile.Dispose();
    }
}