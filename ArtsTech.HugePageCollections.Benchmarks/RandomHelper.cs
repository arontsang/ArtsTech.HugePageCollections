using System.Runtime.InteropServices;

namespace ArtsTech.HugePageCollections.Benchmarks;

public static class RandomHelper
{
    public static unsafe Guid NextGuid(this Random random)
    {
        Span<byte> bytes = stackalloc byte[sizeof(Guid)];
        random.NextBytes(bytes);
        return MemoryMarshal.Cast<byte, Guid>(bytes)[0];
    }
}