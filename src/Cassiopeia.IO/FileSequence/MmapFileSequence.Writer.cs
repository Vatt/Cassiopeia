using Cassiopeia.IO.Mmap;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cassiopeia.IO.FileSequence;
public partial class MmapFileSequence
{
    private struct BufferSegment : IDisposable
    {
        private IMemoryOwner<byte> _owner;
        private int _length;
        public BufferSegment(IMemoryOwner<byte> owner, int length)
        {
            _owner = owner;
            _length = length;
        }
        public Memory<byte> WrittenMemory => _owner.Memory.Slice(0, _length);
        public void Dispose()
        {
            _owner.Dispose();
            _owner = null!;
            _length = 0;
        }
    }
    public class BufferedWriter : IBufferWriter<byte>
    {
        private const int MaxPageSize = 4096;
        private enum State
        {
            Initial,
            Writing
        }
        private readonly MmapFileSequence _sequence;
        private readonly MemoryPool<byte> _pool;
        private IMemoryOwner<byte>? _current;
        private int _totalWritten;
        private int _pageBuffered;
        private int _pageRemaining;
        private List<BufferSegment> _segments;
        private State _state;
        public BufferedWriter(MmapFileSequence sequence)
        {
            _sequence = sequence;
            _pool = sequence._pool;
            _pageBuffered = 0;
            _totalWritten = 0;
            _pageRemaining = 0;
            _segments = new();
            _state = State.Initial;

        }
        public void Advance(int count)
        {
            _pageBuffered += count;
            _totalWritten += count;
            _pageRemaining -= count;
            if (_pageBuffered > _current!.Memory.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
        }
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (sizeHint > MaxPageSize)
            {
                ThrowArgumentException(nameof(sizeHint));
            }
            switch (_state)
            {
                case State.Initial:
                    _current = _pool.Rent(sizeHint);
                    _state = State.Writing;
                    _pageRemaining = _current!.Memory.Length;
                    return _current.Memory;
                case State.Writing:
                    Debug.Assert(_current != null);
                    if (_pageRemaining >= sizeHint &&  _pageRemaining > 0 && sizeHint != 0)
                    {
                        return _current.Memory.Slice(_pageBuffered);
                    }
                    _segments.Add(new BufferSegment(_current, _pageBuffered));
                    _pageBuffered = 0;
                    _current = _pool.Rent(MaxPageSize);
                    _pageRemaining = _current.Memory.Length;
                    return _current.Memory;
            }
            Debugger.Break();
            return default;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return GetMemory(sizeHint).Span;
        }
        public void Flush()
        {
            switch (_state)
            {
                case State.Initial:
                    Debug.Assert(_current == null);
                    if (_segments.Count == 0)
                    {
                        return;
                    }
                    _sequence.Flush(_segments, _totalWritten);
                    break;
                case State.Writing:
                    Debug.Assert(_current != null);
                    _segments.Add(new BufferSegment(_current, _pageBuffered));
                    _sequence.Flush(_segments, _totalWritten);
                    _state = State.Initial;
                    _totalWritten = 0;
                    _pageBuffered = 0;
                    _current = null;
                    _segments.Clear();
                    break;
            }
        }
        private void ThrowArgumentException(string arg)
        {
            throw new ArgumentException(arg);
        }
    }
}