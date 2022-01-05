﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Cassiopeia.IO.FileSequence;

public class AsyncFileSequence
{
    private readonly long _fileSize;
    private readonly string _destFolder;
    private readonly string _nameTemplate;
    private int _nextId = 0;
    private MemoryPool<byte> Pool { get; }
    public AsyncFileWriter SequentialWriter => new(this);
    private SafeFileHandle Handle;
    private FileStream Stream;
    private long Offset = 0;
    private AsyncFileSequence(MemoryPool<byte> pool, string destFolder, string nameTemplate, long fileSize)
    {
        Pool = pool;
        _destFolder = destFolder;
        _nameTemplate = nameTemplate;
        _fileSize = fileSize;
        //Handle = File.OpenHandle(NewName(), FileMode.Create, FileAccess.Write, FileShare.Write,FileOptions.Asynchronous, fileSize);
        //Stream = new FileStream(Handle, FileAccess.Write, 4096, true);
        Stream = new FileStream(NewName(), FileMode.Append, FileAccess.Write, FileShare.Write, 4096, FileOptions.WriteThrough);
    }
    private string NewName()
    {
        var name =  $"{_destFolder}/{_nameTemplate}{_nextId}";
        _nextId += 1;
        return name;
    }
    public static async ValueTask<AsyncFileSequence> CreateAsync(MemoryPool<byte> pool, string destFolder, string nameTemplate, long fileSize)
    {
        if (fileSize <= 32)
        {
            throw new ArgumentException(nameof(fileSize));
        }

        List<string> files = new List<string>();
        if (!Directory.Exists(destFolder))
        {
            Directory.CreateDirectory(destFolder);
        }
        else
        {
            foreach (var file in Directory.GetFiles(destFolder))
            {
                if (file.Contains(nameTemplate))
                {
                    files.Add(file);
                }
            }
        }
        files.Sort((name1, name2) =>
        {
            var idx1 = int.Parse(name1.Substring(name1.LastIndexOf(nameTemplate) + nameTemplate.Length));
            var idx2 = int.Parse(name2.Substring(name2.LastIndexOf(nameTemplate) + nameTemplate.Length));
            if (idx1 > idx2)
            {
                return 1;
            }
            return -1;
        });
        return new AsyncFileSequence(pool, destFolder, nameTemplate, fileSize);
    }
    private async Task FlushAsync(List<AsyncFileWriter.Segment> segmnents)
    {
        for (var i = 0; i < segmnents.Count; i++)
        {
            var segment = segmnents[i];
            var memory = segment.WrittenMemory;
            var memoryLen = memory.Length;
            if (Offset + memoryLen <= _fileSize)
            {

                Offset += memoryLen;
                await Stream.WriteAsync(memory).ConfigureAwait(false);
                segment.Data.Dispose();// FOR TEST!!!!
            }
            else
            {
                var rem = _fileSize - Offset;
                var remMem = memory.Slice(0, (int)rem);
                await Stream.WriteAsync(remMem).ConfigureAwait(false);
                var tail = memory.Slice((int)rem);
                //Handle.Close();
                //Handle.Dispose();
                await Stream.FlushAsync().ConfigureAwait(false);
                await Stream.DisposeAsync().ConfigureAwait(false);
                //Handle = File.OpenHandle(NewName(), FileMode.Create, FileAccess.Write, FileShare.Write, FileOptions.Asynchronous, _fileSize);
                //Stream = new FileStream(Handle, FileAccess.Write, 4096, true);
                Stream = new FileStream(NewName(), FileMode.Append, FileAccess.Write, FileShare.Write);//, 4096, true);
                await Stream.WriteAsync(tail).ConfigureAwait(false);
                Offset = tail.Length;
                segment.Data.Dispose();// FOR TEST!!!!
            }
        }
    }
    private async Task FlushAsync1(List<AsyncFileWriter.Segment> segmnents)
    {
        for(var i = 0; i< segmnents.Count; i++)
        {
            var segment = segmnents[i];
            var memory = segment.WrittenMemory;
            var memoryLen = memory.Length;
            if (Offset + memoryLen <= _fileSize)
            {

                Offset += memoryLen;
                await RandomAccess.WriteAsync(Handle, memory, Offset).ConfigureAwait(false);
                segment.Data.Dispose();// FOR TEST!!!!
            }
            else
            {
                var rem = _fileSize - Offset;
                var remMem = memory.Slice(0, (int)rem);
                await RandomAccess.WriteAsync(Handle, remMem, Offset).ConfigureAwait(false);
                var tail = memory.Slice((int)rem);
                Handle.Close();
                Handle.Dispose();
                Handle = File.OpenHandle(NewName(), FileMode.Create, FileAccess.Write, FileShare.Write, FileOptions.Asynchronous, _fileSize);
                await RandomAccess.WriteAsync(Handle, tail, 0);
                Offset = tail.Length;
                segment.Data.Dispose();// FOR TEST!!!!
            }
        }
    }
    public void Advance(SequencePosition position)
    {

    }
    public class AsyncFileWriter : IBufferWriter<byte>
    {
        internal struct Segment
        {
            public IMemoryOwner<byte> Data;
            public int Length;
            public Memory<byte> WrittenMemory => Data.Memory.Slice(0, Length);
            public Segment(IMemoryOwner<byte> data, int length)
            {
                Data = data;
                Length = length;
            }
        }
        private readonly AsyncFileSequence _sequence;
        private readonly MemoryPool<byte> _pool;
        private List<Segment> _segments;
        private IMemoryOwner<byte>? _current = null;
        private int _currentLen = 0;
        public AsyncFileWriter(AsyncFileSequence sequence)
        {
            _sequence = sequence;
            _pool = sequence.Pool;
            _segments = new();
        }
        public void Advance(int count)
        {
            Debug.Assert(_current != null);
            if (_currentLen + count > 4096)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            _currentLen += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (_current != null && _current.Memory.Slice(_currentLen).Length == 0)
            {
                _segments.Add(new Segment(_current, _currentLen));
                _current = _pool.Rent(4096);
                _currentLen = 0;

            }
            else if (_current == null)
            {
                _current = _pool.Rent(4096);
                _currentLen = 0;
            }
            return _current.Memory.Slice((int)_currentLen);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return GetMemory().Span;
        }
        public async Task FlushAsync()
        {
            Debug.Assert(_current != null);
            _segments.Add(new Segment(_current, _currentLen));
            await _sequence.FlushAsync(_segments);
            _segments.Clear();
            _current = null;
            _currentLen = 0;
        }
    }
    class AsyncSegment : ReadOnlySequenceSegment<byte>
    {
        public AsyncSegment(IMemoryOwner<byte> data, int length, AsyncSegment? prev)
        {
            Memory = data.Memory.Slice(0, length);
        }
        public void LinkNext(AsyncSegment next)
        {
            Next = next;
        }
        public void UnlinkNext()
        {
            Next = null;
        }
    }
}