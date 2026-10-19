using System;
using System.Buffers;

namespace ArtsTech.HugePageCollections;

public static partial class HugePageMemoryPool<T> where T : unmanaged
{
    static HugePageMemoryPool()
    {
        if (OperatingSystem.IsLinux())
            MemoryPool = new LinuxHugePageMemoryAllocator();
        throw new NotImplementedException();
    }
    
    public static MemoryPool<T> MemoryPool { get; }
}