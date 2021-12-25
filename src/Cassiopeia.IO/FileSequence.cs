using Cassiopeia.IO.Mmap;

namespace Cassiopeia.IO;

public partial class FileSequence
{
    private readonly long fileSize;
    private readonly string destFolder;
    private readonly string nameTemplate;
    private int nextId = 0;
    private MmapFile writerHead;
    private int writerHeadPosition;
    private int readerHeadPosition;
    private Span<byte> writerHeadSpan => writerHead.Span.Slice(writerHeadPosition);
    private Memory<byte> writerHeadMemory => writerHead.Memory.Slice(writerHeadPosition);
    public SequentialFileWriter SequentialWriter => new SequentialFileWriter(this);
    private string NewFileName()
    {
        var name = $"{destFolder}/{nameTemplate}{nextId}";
        nextId += 1;
        return name;
    }
    public FileSequence(string destFolder, string nameTemplate, long fileSize)
    {
        this.fileSize = fileSize;
        this.destFolder = destFolder;
        this.nameTemplate = nameTemplate;
        List<string> files = new List<string>();
        if (!Directory.Exists(this.destFolder))
        {
            Directory.CreateDirectory(this.destFolder);
        }
        else
        {
            foreach (var file in Directory.GetFiles(this.destFolder))
            {
                if (file.Contains(this.nameTemplate))
                {
                    files.Add(file);
                }
            }
        }
        files.Sort((string name1, string name2) =>
        {
            var idx1 = int.Parse(name1.Substring(name1.LastIndexOf(this.nameTemplate) + this.nameTemplate.Length));
            var idx2 = int.Parse(name2.Substring(name2.LastIndexOf(this.nameTemplate) + this.nameTemplate.Length));
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
            nextId = files.Count;
            writerHead = new MmapFile(last, (int)this.fileSize);
            writerHeadPosition = writerHead.Memory.Length;//error
        }
        else
        {
            var newFileName = NewFileName();
            writerHead = new MmapFile(newFileName, (int)this.fileSize);
            writerHeadPosition = 0;
        }

    }
    private void WriterHeadAdvance(int count)
    {
        var position = writerHeadPosition;
        var newPosition = position + count;
        if (newPosition > fileSize)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        if (newPosition == fileSize)
        {
            var newHeadName = NewFileName();
            var newHead = new MmapFile(newHeadName, (int)fileSize);
            writerHead.Flush();
            writerHead.Dispose(); // ???
            writerHead = newHead;
            writerHeadPosition = 0;
        }
        else
        {
            writerHeadPosition += count;
        }


    }
    //private void Write(Memory<byte> data)
    //{
    //    var free = fileSize - writerHeadPosition;
    //    if (free < data.Length)
    //    {
    //        var headMem = writerHead.Memory.Slice((int)writerHeadPosition);
    //        data.Slice(0, (int)free).CopyTo(headMem);
    //        writerHead.Flush();
    //        var newHeadName = NewFileName();
    //        var newHead = new MmapFile(newHeadName, (int)fileSize);
    //        writerHead.Dispose();
    //        var tail = data.Slice((int)free);
    //        tail.CopyTo(newHead.Memory);
    //        writerHead = newHead;
    //        writerHeadPosition = tail.Length;
    //    }
    //    else
    //    {
    //        data.CopyTo(writerHead.Memory.Slice((int)writerHeadPosition));
    //        writerHeadPosition += data.Length;
    //    }
    //}
}