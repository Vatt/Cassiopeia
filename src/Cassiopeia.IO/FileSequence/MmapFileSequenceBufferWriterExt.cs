using Cassiopeia.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cassiopeia.IO.FileSequence;

public static class MmapFileSequenceBufferWriterExt
{
    public static void Flush(this ref BufferWriter<MmapFileSequence.FileWriter> writer)
    {
        writer.Commit();
        writer._output.Flush();
    }
}
