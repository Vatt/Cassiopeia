using Cassiopeia.IO.Mmap;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cassiopeia.IO.FileSequence;
public partial class MmapFileSequence
{
    private readonly object _lock = new object();
    private readonly long _fileSize;
    private readonly string _destFolder;
    private readonly string _nameTemplate;
    private int _nextId = 0;
    private FileSegment _readerSegment;
    private FileSegment _writerSegment;
    private MemoryPool<byte> _pool;
    public BufferedWriter SequentialWriter => new BufferedWriter(this);
    public ReadOnlySequence<byte> ReadSequence =>  new ReadOnlySequence<byte>(_readerSegment, _readerSegment.ReaderPosition, _writerSegment, (int)_writerSegment.WriterPosition);
    private ReadOnlySequence<byte> GetSequence()
    {
        lock (_lock)
        {
            return new ReadOnlySequence<byte>(_readerSegment, _readerSegment.ReaderPosition, _writerSegment, (int)_writerSegment.WriterPosition);
        }
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
    public MmapFileSequence(string destFolder, string nameTemplate, long fileSize, MemoryPool<byte> pool)
    {
        _fileSize = fileSize;
        _destFolder = destFolder;
        _nameTemplate = nameTemplate;
        _pool = pool;
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
        //var segment = (FileSegment)position.GetObject()!;
        //var count = position.GetInteger();// - (int)segment.RunningIndex;
        //if (segment.Id == _readerSegment.Id)
        //{
        //    segment.AdvanceReadPosition(count - _readerSegment.ReaderPosition);
        //    return;
        //}
        //do
        //{
        //    var next = (FileSegment)_readerSegment.Next!;
        //    _readerSegment.Dispose();
        //    _readerSegment.UnlinkNext();
        //    File.Delete(_readerSegment.File.Path);
        //    _readerSegment = next;

        //} while (_readerSegment.Id != segment.Id);
        //segment.AdvanceReadPosition(count);
        //if (segment.ReaderPosition == segment.WritableSize)
        //{
        //    Debug.Assert(segment.ReaderPosition == segment.WriterPosition && segment.ReaderPosition == segment.WritableSize && segment.ReaderPosition == segment.WritableSize);
        //    File.Delete(segment.File.Path);
        //}
        lock (_lock)
        {
            var segment = (FileSegment)position.GetObject()!;
            var count = position.GetInteger();// - (int)segment.RunningIndex;
            if (segment.Id == _readerSegment.Id)
            {
                segment.AdvanceReadPosition(count - _readerSegment.ReaderPosition);
                return;
            }
            do
            {
                var next = (FileSegment)_readerSegment.Next!;
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
    }
    private void Flush(List<BufferSegment> segments, int totalWritten)
    {
        if (_writerSegment.WriterPosition + totalWritten <= _writerSegment.WritableSize)
        {
            var offset = _writerSegment.WriterPosition;
            for(int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                var memory = segment.WrittenMemory;
                var len = memory.Length;
                memory.CopyTo(_writerSegment.WritableMemory.Slice(offset, len));
                offset += len;
                segment.Dispose();
            }
            _writerSegment.AdvanceWritePosition(totalWritten);
            return;
        }
        FlushMultiFiles(segments, totalWritten);
    }
    private void FlushMultiFiles(List<BufferSegment> segments, int totalWritten)
    {
        lock (_lock)
        {
            FileSegment iterator = _writerSegment;
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                var memory = segment.WrittenMemory;
                var len = memory.Length;
                do
                {
                    var written = WriteTo(iterator, memory);
                    iterator.AdvanceWritePosition(written);
                    memory = memory.Slice(written);
                    if (memory.IsEmpty == false)
                    {                        
                        iterator = NewFileSegment(iterator);
                    }
                } while (memory.IsEmpty == false);
            }
            _writerSegment = iterator;
        }
        int WriteTo(FileSegment segment, Memory<byte> memory)
        {
            var writableSize = segment.WritableSize;
            var positioon = segment.WriterPosition;
            var len = memory.Length;
            if (positioon + len <= writableSize)
            {
                memory.CopyTo(segment.WritableMemory.Slice(positioon, len));
                return len;
            }
            else
            {
                var writable = writableSize - positioon;
                memory.Slice(0, writable).CopyTo(segment.WritableMemory.Slice(positioon, writable));
                //return len - writable;
                return writable;
            }
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

        public void SHIT()
        {
            FileSegment iterator = this;
            while (iterator.Next != null)
            {
                iterator.RunningIndex = 0;
                FileSegment next = (FileSegment)iterator.Next;
                next.RunningIndex = iterator.WriterPosition - iterator.ReaderPosition;
                iterator = next;
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowArgumentOutOfRangeException(string msg)
        {
            throw new ArgumentOutOfRangeException(msg);
        }
    }
}