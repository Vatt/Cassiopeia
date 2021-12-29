using Cassiopeia.IO.Mmap;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cassiopeia.IO.FileSequence;
public class FileSequence
{
    private readonly long _fileSize;
    private readonly string _destFolder;
    private readonly string _nameTemplate;
    private int _nextId = 0;
    private FileSegment _readerSegment;
    private FileSegment _writerSegment;
    private Span<byte> _writerHeadSpan => _writerHeadMemory.Span;
    private Memory<byte> _writerHeadMemory => _writerSegment.WritableMemory.Slice(_writerSegment.WriterPosition);
    public SequentialFileWriter SequentialWriter => new SequentialFileWriter(this);
    public ReadOnlySequence<byte> ReadSequence => new ReadOnlySequence<byte>(_readerSegment, _readerSegment.ReaderPosition, _writerSegment, (int)_writerSegment.WriterPosition);
    //public ReadOnlySequence<byte> ReadSequence => new ReadOnlySequence<byte>(_readerSegment,0, _writerSegment, _writerSegment.WriterPosition);
    private FileSegment NewFileSegment(FileSegment? prev)
    {
        var name = $"{_destFolder}/{_nameTemplate}{_nextId}";
        var segment = new FileSegment(_nextId, new MmapFile(name, (int)_fileSize), prev);
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
    private void WriterHeadAdvance(int count)
    {
        _writerSegment.AdvanceWritePosition(count);
        if (_writerSegment.WriterPosition == _fileSize - 8)
        {
            var newHead = NewFileSegment(_writerSegment);
            _writerSegment.Flush();
            //_writerSegment.Dispose();
            _writerSegment = newHead;
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
        private readonly FileSequence sequence;
        public SequentialFileWriter(FileSequence sequence)
        {
            this.sequence = sequence;
        }

        public void Advance(int count)
        {
            sequence.WriterHeadAdvance(count);
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            return sequence._writerHeadMemory;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return sequence._writerHeadSpan;
        }
    }
}