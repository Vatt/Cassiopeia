using System.IO.MemoryMappedFiles;

namespace Cassiopeia.IO.Mmap;

public class MmapFile : IDisposable
{
    private readonly MemoryMappedFile mmapFile;
    private unsafe MappedMemoryOwner memoryOwner;
    public int Size { get; }
    public string Path { get; }
    public Span<byte> Span => memoryOwner.GetSpan();
    public Memory<byte> Memory => memoryOwner.Memory;
    public MmapFile(string path, int size)
    {
        Size = size;
        Path = path;
        mmapFile = MemoryMappedFile.CreateFromFile(path, FileMode.OpenOrCreate, null, size);
        memoryOwner = CreateMemoryAccessor(0, size);
    }
    public void Flush()
    {
        memoryOwner.Flush();
    }
    public void Dispose()
    {
        memoryOwner.Dispose();
        mmapFile.Dispose();
    }
    internal MappedMemoryOwner CreateMemoryAccessor(long offset = 0, int size = 0, MemoryMappedFileAccess access = MemoryMappedFileAccess.ReadWrite)
    {
        if (offset < 0L)
            throw new ArgumentOutOfRangeException(nameof(offset));
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        return new MappedMemoryOwner(mmapFile.CreateViewAccessor(offset, size, access));
    }
}