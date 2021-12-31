using Cassiopeia.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cassiopeia.IO.FileSequence;

public static class FileSequenceBufferWriterExt
{
    public static void Flush(this ref BufferWriter<FileSequence.SequentialFileWriter> writer)
    {
        writer.Commit();
        writer._output.Flush();
    }
}
