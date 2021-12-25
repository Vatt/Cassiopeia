using System.Buffers;

namespace Cassiopeia.IO;

public partial class FileSequence
{
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
            return sequence.writerHeadMemory;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return sequence.writerHeadSpan;
        }
    }
}