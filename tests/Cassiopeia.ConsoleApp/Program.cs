// See https://aka.ms/new-console-template for more information

using Cassiopeia.Buffers;
using Cassiopeia.IO;
using Cassiopeia.IO.FileSequence;
using static Cassiopeia.IO.FileSequence.FileSequence;

await Runner.RunSingleDriveE();
//await Runner.RunOnDriveD();
//await Runner.RunOnDriveE();
static class Runner
{
    public static int FileSize = 100 * 1024 * 1024;// * 1024;
    public static Memory<byte> Data = new byte[1025];
    public static Task RunSingleDriveE()
    {
        return RunSequence($"E:/Cassiopeia/FileSequence");
    }
    public static Task RunSingleDriveD()
    {
        return RunSequence($"D:/Cassiopeia/FileSequence");
    }
    public static Task RunOnDriveE()
    {
        CancellationTokenSource cts = new();
        return Task.WhenAll(RunSequence($"E:/Cassiopeia/FileSequence0", cts), RunSequence($"E:/Cassiopeia/FileSequence1", cts),
                            RunSequence($"E:/Cassiopeia/FileSequence2", cts), RunSequence($"E:/Cassiopeia/FileSequence3", cts),
                            RunSequence($"E:/Cassiopeia/FileSequence4", cts), RunSequence($"E:/Cassiopeia/FileSequence5", cts));
    }
    public static Task RunOnDriveD()
    {
        CancellationTokenSource cts = new();
        return Task.WhenAll(RunSequence($"D:/Cassiopeia/FileSequence0", cts), RunSequence($"D:/Cassiopeia/FileSequence1", cts),
                            RunSequence($"D:/Cassiopeia/FileSequence2", cts), RunSequence($"D:/Cassiopeia/FileSequence3", cts),
                            RunSequence($"D:/Cassiopeia/FileSequence4", cts), RunSequence($"D:/Cassiopeia/FileSequence5", cts));
    }
    public static Task RunSequence(string path, CancellationTokenSource? source = null)
    {
        FileSequence Sequence = new FileSequence(path, "Chunk", FileSize);
        CancellationTokenSource cts = source ?? new();
        var reader = Reader(Sequence, cts);
        var writer = Writer(Sequence, cts);
        return Task.WhenAll(reader, writer);
    }
    public static Task Writer(FileSequence sequence, CancellationTokenSource source)
    {
        return Task.Run(() =>
        {
            var cts = source;
            var token = cts.Token;
            var seq = sequence;
            var writer = new BufferWriter<SequentialFileWriter>(seq.SequentialWriter);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    writer.WriteBytes(Data.Span);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    cts.Cancel();
                }                
            }
        });
    }

    public static Task Reader(FileSequence sequence, CancellationTokenSource source)
    {
        return Task.Run(() =>
        {
            var cts = source;
            var token = cts.Token;
            var seq = sequence;
            Memory<byte> buffer = new byte[1025];
            var ros = seq.ReadSequence;
            var reader = new BufferReader(ros);
            while (!token.IsCancellationRequested)
            {
                //try
                //{
                //    var ros = seq.ReadSequence;
                //    if (ros.Length < buffer.Length + 4)
                //    {
                //        continue;
                //    }
                //    var reader = new BufferReader(ros);
                //    reader.ReadBytesTo(buffer);
                //    seq.Advance(reader.Position);
                //}
                //catch(Exception ex)
                //{
                //    Console.WriteLine(ex.ToString());
                //    cts.Cancel();
                //}
                
                //if (ros.Length < buffer.Length + 4)
                //{
                //    continue;
                //}
                ros = seq.ReadSequence;
                
                if (ros.Length < buffer.Length + 4)
                {
                    continue;
                }
                reader = new BufferReader(ros);
                reader.ReadBytesTo(buffer);
                seq.Advance(reader.Position);
                
                //if (reader.TryReadBytesTo(buffer))
                //{
                //    seq.Advance(reader.Position);
                //}
                //else
                //{
                //    ros = seq.ReadSequence;
                //    reader = new BufferReader(ros);
                //}
                
            }
        });

    }
}

struct TestData
{
    public string TestStr = "Test";
    public int Size = 1025;
    public Memory<byte> TestBytes = new byte[1025];
    public TestData()
    {
        TestStr = "Test";
        Size = 1025;
        TestBytes = new byte[Size];
        byte val = 0;
        for (var i = 0; i < Size; i++)
        {
            TestBytes.Span[i] = val;
            if (val == byte.MaxValue)
            {
                val = 0;
            }
            else
            {
                val += 1;
            }

        }
    }
}