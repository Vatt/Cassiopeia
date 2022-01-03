// See https://aka.ms/new-console-template for more information

using Cassiopeia.Buffers;
using Cassiopeia.Buffers.MemoryPool;
using Cassiopeia.IO;
using Cassiopeia.IO.FileSequence;
using Cassiopeia.Protocol.Messages;
using System.Buffers;
using System.Diagnostics;

//await Runner.RunSingleAsyncDriveE();
//await Runner.RunSingleAsyncDriveD();
//await Runner.RunAsyncOnDriveD();
await Runner.RunSingleMmapDriveE();
//await Runner.RunSingleMmapDriveD();
//await Runner.RunMmapOnDriveE();
//await Runner.RunMmapOnDriveD();
static class Runner
{
    public static MemoryPool<byte> MemoryPool = new PinnedBlockMemoryPool();
    public static int FileSize = 100 * 1024 * 1024;
    //public static int FileSize = 1 * 1024 * 1024 * 1024;
    public static Memory<byte> Data = new byte[1025];
    public static ClientHello Hello = new ClientHello("ConsoleApp", "0.0.1-001", ".NET", "This is for FileSequence", "gamover", "gamover", 42, true);
    // public static ClientHello Hello = new ClientHello("他妈的狗屎", "他妈的狗屎", "他妈的狗屎", "👨‍👨‍👧‍👧👨‍👨‍👧‍👧👨‍👨‍👧‍👧", "👨‍👨‍👧‍👧👨‍👨‍👧‍👧👨‍👨‍👧‍👧", "👨‍👨‍👧‍👧👨‍👨‍👧‍👧👨‍👨‍👧‍👧", 42, true);
    public static Task RunSingleAsyncDriveE()
    {
        return RunAsyncSequence($"E:/Cassiopeia/AsyncFileSequence");
    }
    public static Task RunSingleAsyncDriveD()
    {
        return RunAsyncSequence($"D:/Cassiopeia/AsyncFileSequence");
    }
    public static Task RunAsyncOnDriveE()
    {
        CancellationTokenSource cts = new();
        return Task.WhenAll(RunAsyncSequence($"E:/Cassiopeia/AsyncFileSequence0", cts), RunAsyncSequence($"E:/Cassiopeia/AsyncFileSequence1", cts),
                            RunAsyncSequence($"E:/Cassiopeia/AsyncFileSequence2", cts), RunAsyncSequence($"E:/Cassiopeia/AsyncFileSequence3", cts),
                            RunAsyncSequence($"E:/Cassiopeia/AsyncFileSequence4", cts), RunAsyncSequence($"E:/Cassiopeia/AsyncFileSequence5", cts));
    }
    public static Task RunAsyncOnDriveD()
    {
        CancellationTokenSource cts = new();
        return Task.WhenAll(RunAsyncSequence($"D:/Cassiopeia/AsyncFileSequence0", cts), RunAsyncSequence($"D:/Cassiopeia/AsyncFileSequence1", cts),
                            RunAsyncSequence($"D:/Cassiopeia/AsyncFileSequence2", cts), RunAsyncSequence($"D:/Cassiopeia/AsyncFileSequence3", cts),
                            RunAsyncSequence($"D:/Cassiopeia/AsyncFileSequence4", cts), RunAsyncSequence($"D:/Cassiopeia/AsyncFileSequence5", cts));
    }
    public static async Task RunAsyncSequence(string path, CancellationTokenSource? source = null)
    {
        AsyncFileSequence Sequence = await AsyncFileSequence.CreateAsync(MemoryPool, path, "Chunk", FileSize);
        CancellationTokenSource cts = source ?? new();
        //var reader = MmapReader(Sequence, cts);
        var writer = AsyncWriter(Sequence, cts);
        await Task.WhenAll(/*reader,*/ writer);
    }
    public static Task AsyncWriter(AsyncFileSequence sequence, CancellationTokenSource source)
    {
        return Task.Run(async () =>
        {
            var cts = source;
            var token = cts.Token;
            var seq = sequence;
            var bufferWritter = seq.SequentialWriter;
            
            var iteration = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    iteration += 1;
                    //for(var i = 0; i< 10000; i++)
                    //{
                    //    Write(bufferWritter);
                    //}
                    Write(bufferWritter);
                    await bufferWritter.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    cts.Cancel();
                }
            }
        });
        void Write(AsyncFileSequence.AsyncFileWriter output)
        {
            var writer = new BufferWriter<AsyncFileSequence.AsyncFileWriter>(output);
            //ClientHello.Write(ref writer, Hello);
            writer.WriteBytes(Data.Span);
            writer.Commit();
        }
    }
    public static Task RunSingleMmapDriveE()
    {
        return RunMmapSequence($"E:/Cassiopeia/FileSequence");
    }
    public static Task RunSingleMmapDriveD()
    {
        return RunMmapSequence($"D:/Cassiopeia/FileSequence");
    }
    public static Task RunMmapOnDriveE()
    {
        CancellationTokenSource cts = new();
        return Task.WhenAll(RunMmapSequence($"E:/Cassiopeia/FileSequence0", cts), RunMmapSequence($"E:/Cassiopeia/FileSequence1", cts),
                            RunMmapSequence($"E:/Cassiopeia/FileSequence2", cts), RunMmapSequence($"E:/Cassiopeia/FileSequence3", cts),
                            RunMmapSequence($"E:/Cassiopeia/FileSequence4", cts), RunMmapSequence($"E:/Cassiopeia/FileSequence5", cts));
    }
    public static Task RunMmapOnDriveD()
    {
        CancellationTokenSource cts = new();
        return Task.WhenAll(RunMmapSequence($"D:/Cassiopeia/FileSequence0", cts), RunMmapSequence($"D:/Cassiopeia/FileSequence1", cts),
                            RunMmapSequence($"D:/Cassiopeia/FileSequence2", cts), RunMmapSequence($"D:/Cassiopeia/FileSequence3", cts),
                            RunMmapSequence($"D:/Cassiopeia/FileSequence4", cts), RunMmapSequence($"D:/Cassiopeia/FileSequence5", cts));
    }
    public static Task RunMmapSequence(string path, CancellationTokenSource? source = null)
    {
        MmapFileSequence Sequence = new MmapFileSequence(path, "Chunk", FileSize);
        CancellationTokenSource cts = source ?? new();
        var reader = MmapReader(Sequence, cts);
        var writer = MmapWriter(Sequence, cts);
        return Task.WhenAll(reader, writer);
    }
    public static Task MmapWriter(MmapFileSequence sequence, CancellationTokenSource source)
    {
        return Task.Run(() =>
        {
            var cts = source;
            var token = cts.Token;
            var seq = sequence;
            var bufferWritter = seq.SequentialWriter;
            var writer = new BufferWriter<MmapFileSequence.FileWriter>(bufferWritter);
            var iteration = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    iteration += 1;
                    ClientHello.Write(ref writer, Hello);
                    //writer.WriteBytes(Data.Span);
                    writer.Flush();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    cts.Cancel();
                }                
            }
        });
    }

    public static Task MmapReader(MmapFileSequence sequence, CancellationTokenSource source)
    {
        return Task.Run(() =>
        {
            var cts = source;
            var token = cts.Token;
            var seq = sequence;
            var ros = seq.ReadSequence;
            var reader = new BufferReader(ros);
            var allIteration = 0;
            var successIterations = 0;
            var failedIterations = 0;
            while (!token.IsCancellationRequested)
            {
                //try
                //{
                //    allIteration += 1;
                //    //reader = new BufferReader(seq.ReadSequence);
                //    if (ClientHello.TryParse(ref reader, out var hello))
                //    {
                //        successIterations += 1;
                //        seq.Advance(reader.Position);

                //        if (hello.Equals(Hello) == false)
                //        {
                //            throw new Exception("CORRUPTION");
                //        }
                //    }
                //    else
                //    {
                //        failedIterations += 1;
                //        ros = seq.ReadSequence;
                //        reader = new BufferReader(ros);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //    Console.WriteLine(ex.StackTrace);
                //    cts.Cancel();
                //}
                allIteration += 1;
                //reader = new BufferReader(seq.ReadSequence);
                if (ClientHello.TryParse(ref reader, out var hello))
                {
                    successIterations += 1;
                    seq.Advance(reader.Position);

                    if (hello.Equals(Hello) == false)
                    {
                        throw new Exception("CORRUPTION");
                    }
                }
                else
                {
                    failedIterations += 1;
                    ros = seq.ReadSequence;
                    reader = new BufferReader(ros);
                }

            }
        });

    }
}
