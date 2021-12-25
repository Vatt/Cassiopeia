using System.Buffers;
using System.IO.MemoryMappedFiles;

namespace Cassiopeia.IO.Mmap;

internal class MappedMemoryOwner : MemoryManager<byte>
{
    private readonly MemoryMappedViewAccessor accessor;
    private readonly int length;
    private unsafe readonly byte* ptr;

    internal unsafe MappedMemoryOwner(MemoryMappedViewAccessor accessor)
    {
        if (accessor.Capacity > int.MaxValue)
            throw new ArgumentException("SegmentVeryLarge", nameof(accessor));
        length = (int)accessor.Capacity;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        this.accessor = accessor;
    }

    public long Size => length;

    public unsafe override Span<byte> GetSpan() => new(ptr, length);

    public override Memory<byte> Memory => CreateMemory(length);

    public unsafe override MemoryHandle Pin(int elementIndex) => new(ptr + elementIndex);

    public override void Unpin()
    {
    }
    public void Flush() => accessor.Flush();
    public void Dispose() => Dispose(true);
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            accessor.Dispose();
        }
    }
}
