using System.Buffers;

namespace Cassiopeia.Buffers.MemoryPool
{
    internal static class PinnedBlockMemoryPoolFactory
    {
        public static MemoryPool<byte> Create()
        {
#if DEBUG
            return new DiagnosticMemoryPool(CreatePinnedBlockMemoryPool());
#else
            return CreatePinnedBlockMemoryPool();
#endif
        }

        public static MemoryPool<byte> CreatePinnedBlockMemoryPool()
        {
            return new PinnedBlockMemoryPool();
        }
    }
}
