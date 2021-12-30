// See https://aka.ms/new-console-template for more information

using Cassiopeia.Buffers;
using Cassiopeia.IO;
using Cassiopeia.IO.FileSequence;
using Cassiopeia.Protocol.Messages;
using System.Diagnostics;
using static Cassiopeia.IO.FileSequence.FileSequence;

await Runner.RunSingleDriveE();
//await Runner.RunOnDriveD();
//await Runner.RunOnDriveE();
static class Runner
{
    public static int FileSize = 100 * 1024 * 1024;// * 1024;
    public static Memory<byte> Data = new byte[1025];
    //public static ClientHello Hello = new ClientHello("ConsoleApp", "0.0.1-001", ".NET", "This is for FileSequence", "gamover", "gamover", 42, true);
    public static ClientHello Hello = new ClientHello("他妈的狗屎", "他妈的狗屎", "他妈的狗屎", "👨‍👨‍👧‍👧👨‍👨‍👧‍👧👨‍👨‍👧‍👧", "👨‍👨‍👧‍👧👨‍👨‍👧‍👧👨‍👨‍👧‍👧", "👨‍👨‍👧‍👧👨‍👨‍👧‍👧👨‍👨‍👧‍👧", 42, true);
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
            var iteration = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    iteration += 1;
                    //if (iteration == 426979)
                    //{
                    //    Debugger.Break();
                    //}
                    ClientHello.Write(ref writer, Hello);
                    writer.Commit();
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
            var ros = seq.ReadSequence;
            var reader = new BufferReader(ros);
            var iteration = 0;
            while (!token.IsCancellationRequested)
            {
                iteration += 1;
                reader = new BufferReader(seq.ReadSequence);
                if (ClientHello.TryParse(ref reader, out var hello))
                {
                    seq.Advance(reader.Position);
                    
                    if (hello.Equals(Hello) == false)
                    {
                        throw new Exception("CORRUPTION");
                    }
                }
                else
                {
                    ros = seq.ReadSequence;
                    reader = new BufferReader(ros);
                }                
            }
        });

    }
}
