using System.Buffers;
using System.IO.MemoryMappedFiles;

namespace Cassiopeia.IO.Mmap;

internal class MappedMemoryOwner : MemoryManager<byte>
{
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly int _length;
    private unsafe readonly byte* _ptr;

    internal unsafe MappedMemoryOwner(MemoryMappedViewAccessor accessor)
    {
        if (accessor.Capacity > int.MaxValue)
            throw new ArgumentException("SegmentVeryLarge", nameof(accessor));
        _length = (int)accessor.Capacity;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _ptr);
        _accessor = accessor;
    }

    public long Size => _length;

    public unsafe override Span<byte> GetSpan() => new(_ptr, _length);

    public override Memory<byte> Memory => CreateMemory(_length);

    public unsafe override MemoryHandle Pin(int elementIndex) => new(_ptr + elementIndex);

    public override void Unpin()
    {
    }
    public void Flush() => _accessor.Flush();
    public void Dispose() => Dispose(true);
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            _accessor.Dispose();
        }
    }
}
