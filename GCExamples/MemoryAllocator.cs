using System.Collections.Generic;

namespace GCExamples
{
    class MemoryAllocator
    {
        private List<byte[]> memoryThingy = new List<byte[]>();

        public void AddMemory(long bytes)
        {
            memoryThingy.Add(new byte[bytes]);
        }

        public void AddSohMemory(long bytes)
        {
            for (int i = 0; i < bytes / 84000; i++)
            {
                AddMemory(840000);
            }
        }
    }
}