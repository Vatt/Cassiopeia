using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cassiopeia.IO.FileSequence;

public class AsyncFileSequence
{
    private readonly long _fileSize;
    private readonly string _destFolder;
    private readonly string _nameTemplate;
    private int _nextId = 0;
    public AsyncFileSequence(string destFolder, string nameTemplate, long fileSize)
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
    }
    public void Advance(SequencePosition position)
    {

    }
    struct SequentialWriter : IBufferWriter<byte>
    {
        public void Advance(int count)
        {
            throw new NotImplementedException();
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            throw new NotImplementedException();
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            throw new NotImplementedException();
        }
    }
    class AsyncSegment : ReadOnlySequenceSegment<byte>, IDisposable
    {
        public SafeFileHandle Handle;
        public int Size { get; }
        public int ReaderPosition { get; private set; }
        public int WriterPosition { get; private set; }
        public AsyncSegment(string path, int size,  AsyncSegment? prev)
        {
            Handle = File.OpenHandle(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.SequentialScan | FileOptions.Asynchronous, size);
            
        }

        public void Dispose()
        {
            Handle.Dispose();
        }
    }
}
