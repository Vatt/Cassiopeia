using Cassiopeia.IO.Mmap;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cassiopeia.IO.FileSequence;
public class FileSequence
{
    private readonly object _lock = new object();
    private readonly long _fileSize;
    private readonly string _destFolder;
    private readonly string _nameTemplate;
    private int _nextId = 0;
    private FileSegment _readerSegment;
    private FileSegment _writerSegment;
    public SequentialFileWriter SequentialWriter => new SequentialFileWriter(this);
    public ReadOnlySequence<byte> ReadSequence => BuildSequence();
    private ReadOnlySequence<byte> BuildSequence()
    {
        //lock (_lock)
        //{
        //    return new ReadOnlySequence<byte>(_readerSegment, _readerSegment.ReaderPosition, _writerSegment, (int)_writerSegment.WriterPosition);
        //}
        return new ReadOnlySequence<byte>(_readerSegment, _readerSegment.ReaderPosition, _writerSegment, (int)_writerSegment.WriterPosition);

    }
    private FileSegment NewFileSegment(FileSegment? prev)
    {
        var name = $"{_destFolder}/{_nameTemplate}{_nextId}";
        var segment = new FileSegment(_nextId, new MmapFile(name, (int)_fileSize), prev);
        _nextId += 1;
        return segment;
    }
    private FileSegment NewFileSegment(long prevRunningIndex, int prevWriterPosition, int prevReaderPosition)
    {
        var name = $"{_destFolder}/{_nameTemplate}{_nextId}";
        var segment = new FileSegment(_nextId, new MmapFile(name, (int)_fileSize), prevRunningIndex, prevWriterPosition, prevReaderPosition);
        _nextId += 1;
        return segment;
    }
    public FileSequence(string destFolder, string nameTemplate, long fileSize)
    {
        _fileSize = fileSize;
        _destFolder = destFolder;
        _nameTemplate = nameTemplate;
        if (fileSize <= 32)
        {
            throw new ArgumentException(nameof(fileSize));
        }
        
        List<string> files = new List<string>();
        if (!Directory.Exists(_destFolder))
        {
            Directory.CreateDirectory(_destFolder);
        }
        else
        {
            foreach (var file in Directory.GetFiles(_destFolder))
            {
                if (file.Contains(_nameTemplate))
                {
                    files.Add(file);
                }
            }
        }
        files.Sort((name1, name2) =>
        {
            var idx1 = int.Parse(name1.Substring(name1.LastIndexOf(_nameTemplate) + _nameTemplate.Length));
            var idx2 = int.Parse(name2.Substring(name2.LastIndexOf(_nameTemplate) + _nameTemplate.Length));
            if (idx1 > idx2)
            {
                return 1;
            }
            return -1;
        });
        if (files.Count > 0)
        {
            var first = files[0];
            _readerSegment = new FileSegment(0, new MmapFile(first, (int) fileSize), null);
            _writerSegment = _readerSegment;
            for (var i = 1; i < files.Count; i++)
            {
                _writerSegment = new FileSegment(i, new MmapFile(files[i], (int) fileSize), _writerSegment);
                if (_readerSegment!.ReaderPosition == _readerSegment.WriterPosition)
                {
                    var next = (FileSegment)_readerSegment.Next!;
                    if(_readerSegment.WriterPosition == _readerSegment.WritableSize && _readerSegment.ReaderPosition == _readerSegment.WriterPosition)
                    {
                        _readerSegment.Dispose();
                        _readerSegment.UnlinkNext();
                        File.Delete(files[i]);
                        _readerSegment = next;
                    }
                }
            }
            _nextId = int.Parse(files.Last().Substring(files.Last().LastIndexOf(_nameTemplate) + _nameTemplate.Length)) + 1;
        }
        else
        {
            _writerSegment = NewFileSegment(null);
            _readerSegment = _writerSegment;
        }

    }
    public void Advance(SequencePosition position)
    {
            var segment = (FileSegment)position.GetObject()!;
            var count = position.GetInteger();// - (int)segment.RunningIndex;
            if (segment.Id == _readerSegment.Id)
            {
                segment.AdvanceReadPosition(count - _readerSegment.ReaderPosition);
                return;
            }
            var remaining = count;
            do
            {
                var next = (FileSegment)_readerSegment.Next!;
                var rem = _readerSegment.WriterPosition - _readerSegment.ReaderPosition;
                //_readerSegment.AdvanceReadPosition(rem);
                _readerSegment.Dispose();
                _readerSegment.UnlinkNext();
                File.Delete(_readerSegment.File.Path);
                _readerSegment = next;

            } while (_readerSegment.Id != segment.Id);
            segment.AdvanceReadPosition(count);
            if (segment.ReaderPosition == segment.WritableSize)
            {
                Debug.Assert(segment.ReaderPosition == segment.WriterPosition && segment.ReaderPosition == segment.WritableSize && segment.ReaderPosition == segment.WritableSize);
                File.Delete(segment.File.Path);
            }
    }

    private class FileSegment : ReadOnlySequenceSegment<byte>, IDisposable
    {
        
        public int WritableSize { get; }
        public long Id { get; }
        public MmapFile File { get; }
        public Memory<byte> ReadPositionMemory { get; }
        public Memory<byte> WritePositionMemory { get; }
        public Memory<byte> WritableMemory { get; }
        public int ReaderPosition { get; private set; }
        public int WriterPosition { get; private set; }
        public FileSegment(long id, MmapFile file)
        {
            Id = id;
            File = file;
            Memory = file.Memory.Slice(sizeof(int) * 2);
            WritableMemory = file.Memory.Slice(sizeof(int) * 2);
            ReadPositionMemory = file.Memory.Slice(0, sizeof(int));
            WritePositionMemory = file.Memory.Slice(sizeof(int), sizeof(int));
            WritableSize = file.Size - sizeof(int) * 2;
            WriterPosition = BinaryPrimitives.ReadInt32LittleEndian(WritePositionMemory.Span);
            ReaderPosition = BinaryPrimitives.ReadInt32LittleEndian(ReadPositionMemory.Span);
            Next = null;
        }
        public FileSegment(long id, MmapFile file, FileSegment? prev)
            :this(id, file)
        {
            if (prev != null)
            {
                RunningIndex = prev.RunningIndex + prev.WriterPosition - prev.ReaderPosition;
                prev.Next = this;
            }
            else
            {
                RunningIndex = 0;
            }
            
        }
        internal FileSegment(long id, MmapFile file, long prevRunningIndex, int prevWriterPosition, int prevReaderPosition)
             : this(id, file)
        {
            RunningIndex = prevRunningIndex + prevWriterPosition - prevReaderPosition;

        }
        public void AdvanceReadPosition(int count)
        {
            ReaderPosition += count;
            if (ReaderPosition > WriterPosition)
            {
                ThrowArgumentOutOfRangeException(nameof(count));
            }
            BinaryPrimitives.WriteInt32LittleEndian(ReadPositionMemory.Span, ReaderPosition);
        }
        public void AdvanceWritePosition(int count)
        {
            WriterPosition += count;
            RunningIndex += count;
            if (WriterPosition > File.Size - sizeof(int) * 2)
            {
                ThrowArgumentOutOfRangeException(nameof(count));
            }
            BinaryPrimitives.WriteInt32LittleEndian(WritePositionMemory.Span, WriterPosition);
        }
        public void Flush()
        {
            File.Flush();
        }
        public void LinkNext(FileSegment next)
        {
            Next = next;
        }
        public void UnlinkNext()
        {
            Next = null;
        }
        public void Dispose()
        {
            File.Flush();
            File.Dispose();
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowArgumentOutOfRangeException(string msg)
        {
            throw new ArgumentOutOfRangeException(msg);
        }
    }
    public struct SequentialFileWriter : IBufferWriter<byte>
    {
        private enum State
        {
            Current,
            Head,
            Tail,
        }
        private readonly FileSequence sequence;
        private FileSegment _current;
        private FileSegment? head;
        private FileSegment? tail;
        private int? _buffered;
        private State _state;
        public SequentialFileWriter(FileSequence sequence)
        {
            this.sequence = sequence;
            _current = sequence._writerSegment;
            _state = State.Current; 
            head = tail = null;
            _buffered = null;
        }
        public void Advance(int count)
        {
            switch (_state)
            {
                case State.Current:
                    AdvanceCurrent(count);
                    break;
                case State.Head:
                    AdvanceHead(count);
                    break;
                case State.Tail:
                    AdvanceTail(count);
                    break;
            }
        }
        private void AdvanceCurrent(int count)
        {
            Debug.Assert(_buffered.HasValue == false);
            var newSize = _current.WriterPosition + count;
            if (newSize < _current.WritableSize)
            {
                _current.AdvanceWritePosition(count);
                return;
            }
            else
            {
                Debug.Assert(newSize == _current.WritableSize);
                _buffered = count;
                head = sequence.NewFileSegment(_current.RunningIndex + count, _current.WritableSize, _current.ReaderPosition);
                _state = State.Head;    
            }
        }
        private void AdvanceHead(int count)
        {
            Debug.Assert(tail == null && head != null);
            Debug.Assert(_buffered.HasValue != false);
            var newSize = head.WriterPosition + count;
            if (head != null && newSize < head.WritableSize)
            {
                head.AdvanceWritePosition(count);
            }
            else
            {
                Debug.Assert(head != null && newSize == _current.WritableSize);
                head.AdvanceWritePosition(count);
                head.Flush();
                tail = sequence.NewFileSegment(head);
                _state= State.Tail;
            }
        }
        private void AdvanceTail(int count)
        {
            Debug.Assert(tail != null);
            var newSize = tail.WriterPosition + count;
            if (newSize < tail.WritableSize)
            {
                tail.AdvanceWritePosition(count);
            }
            else
            {
                Debug.Assert(newSize == tail.WritableSize);
                tail.AdvanceWritePosition(count);
                tail.Flush();
                tail = sequence.NewFileSegment(tail);
            }
        }
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (_buffered.HasValue == false)
            {
                return _current.WritableMemory.Slice(_current.WriterPosition);
            }
            if (tail == null)
            {
                Debug.Assert(head != null);
                return head.WritableMemory.Slice(head.WriterPosition);
            }
            Debug.Assert(tail != null);
            return tail.WritableMemory.Slice(tail.WriterPosition);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return GetMemory().Span;
        }
        public void Flush()
        {
            if (_state == State.Current)
            {
                return;
            }
            Debug.Assert(head != null && _buffered.HasValue);
            FileSegment newWriterSegment = head;
            var iterator = (FileSegment)newWriterSegment.Next!;
            newWriterSegment = head;
            while (iterator != null)
            {
                newWriterSegment = iterator;
                iterator = (FileSegment)iterator.Next!;
            }
            //lock (sequence._lock)
            //{
            //    _current.AdvanceWritePosition(_buffered.Value);
            //    _current.LinkNext(head);
            //    _current.Flush();
            //    _current = head;
            //    sequence._writerSegment = newWriterSegment;
            //}
            _current.AdvanceWritePosition(_buffered.Value);
            _current.LinkNext(head);
            _current.Flush();
            _current = head;
            sequence._writerSegment = newWriterSegment;

            _buffered = null;
            head = tail = null;
            _state = State.Current;
        }
    }
}