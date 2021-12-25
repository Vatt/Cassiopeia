using Cassiopeia.IO.Mmap;
using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cassiopeia.IO;

public class FileSequence
{
    private readonly long _fileSize;
    private readonly string _destFolder;
    private readonly string _nameTemplate;
    private int _nextId = 0;
    private int _readId = 0;
    private MmapFile _writerHead;
    private MmapFile _readerHead;
    private long _headSize;
    private string NewFileName()
    {
        var name = $"{_destFolder}/{_nameTemplate}{_nextId}";
        _nextId += 1;
        return name;
    }
    public FileSequence(string destFolder, string nameTemplate, long fileSize)
    {
        _fileSize = fileSize;
        _destFolder = destFolder;
        _nameTemplate = nameTemplate;
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
        files.Sort((string name1, string name2) =>
        {
            var idx1 = int.Parse(name1.Substring(name1.LastIndexOf(_nameTemplate) + _nameTemplate.Length));
            var idx2 = int.Parse(name2.Substring(name2.LastIndexOf(_nameTemplate) + _nameTemplate.Length));
            if (idx1 > idx2)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        });
        if (files.Count > 0)
        {
            var first = files[0];
            var last = files.Last();
            _nextId = files.Count;
            _readId = int.Parse(first.Substring(first.LastIndexOf(_nameTemplate) + _nameTemplate.Length));
            _writerHead = new MmapFile(last, (int)_fileSize);
            _readerHead = new MmapFile(first, (int)_fileSize);
        }
        else
        {
            var newFileName = NewFileName();
            _writerHead = new MmapFile(newFileName, (int)_fileSize);
            _readerHead = _writerHead;
        }
        _headSize = _writerHead.Memory.Length;
    } 
    public void WriteAsync(Memory<byte> data)
    {
        //var free = _fileSize - _writeHead.Length; 
        var free = _fileSize - _headSize; 
        if (free < data.Length)
        {
            //await _writerHead.WriteAsync(data.Slice(0, (int)free)).ConfigureAwait(false);
            var headMem = _writerHead.Memory.Slice((int)_headSize);
            data.Slice(0, (int)free).CopyTo(headMem);
            //await _writerHead.FlushAsync().ConfigureAwait(false);
            _writerHead.Flush();
            var newHeadName = NewFileName();
            var newHead = new MmapFile(newHeadName, (int)_fileSize);
            _writerHead.Dispose();
            //await _writerHead.DisposeAsync().ConfigureAwait(false);
            var tail = data.Slice((int)free);
            //await newHead.WriteAsync(tail).ConfigureAwait(false);
            tail.CopyTo(newHead.Memory);
            _writerHead = newHead;
            _headSize = tail.Length;
        }
        else
        {
            //await _writerHead.WriteAsync(data).ConfigureAwait(false);
            data.CopyTo(_writerHead.Memory.Slice((int)_headSize));
            _headSize += data.Length;
        }
    }
    //public async ValueTask<int> ReadAsync(Memory<byte> data)
    //{
    //    var first = await _readerHead.ReadAsync(data).ConfigureAwait(false);
    //    if (first < data.Length)
    //    { 
    //        if (_readId == _nextId)
    //        {
    //            return first;
    //        }
    //        File.Delete($"{_destFolder}/{_nameTemplate}{--_readId}");
    //        var newFile = new FileStream($"{_destFolder}/{_nameTemplate}{_readId}", FileMode.Open, FileAccess.Read);
    //        _readId += 1;
    //        var second = await newFile.ReadAsync(data.Slice(first)).ConfigureAwait(false);
    //        _readerHead = newFile;
    //        return first + second;
    //    }
    //    return first;
    //}
}
