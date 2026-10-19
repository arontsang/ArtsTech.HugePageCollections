using System;
// Copied from https://github.com/tolzy88/Collections.Pooled.V2/blob/master/Collections.Pooled/BitHelper.cs
namespace ArtsTech.HugePageCollections.Collections;

internal ref struct BitHelper
{
    private const int IntSize = sizeof(int) * 8;
    private readonly Span<int> _span;

    internal BitHelper(Span<int> span, bool clear)
    {
        if (clear)
        {
            span.Clear();
        }
        _span = span;
    }

    internal void MarkBit(int bitPosition)
    {
        int bitArrayIndex = bitPosition / IntSize;
        if ((uint)bitArrayIndex < (uint)_span.Length)
        {
            _span[bitArrayIndex] |= (1 << (bitPosition % IntSize));
        }
    }

    internal bool IsMarked(int bitPosition)
    {
        int bitArrayIndex = bitPosition / IntSize;
        return
            (uint)bitArrayIndex < (uint)_span.Length &&
            (_span[bitArrayIndex] & (1 << (bitPosition % IntSize))) != 0;
    }

    internal int FindFirstUnmarked(int startPosition = 0)
    {
        int i = startPosition;
        for (int bi = i / IntSize; (uint)bi < (uint)_span.Length; bi = ++i / IntSize)
        {
            if ((_span[bi] & (1 << (i % IntSize))) == 0)
                return i;
        }
        return -1;
    }

    internal int FindFirstMarked(int startPosition = 0)
    {
        int i = startPosition;
        for (int bi = i / IntSize; (uint)bi < (uint)_span.Length; bi = ++i / IntSize)
        {
            if ((_span[bi] & (1 << (i % IntSize))) != 0)
                return i;
        }
        return -1;
    }

    /// <summary>How many ints must be allocated to represent n bits. Returns (n+31)/32, but avoids overflow.</summary>
    internal static int ToIntArrayLength(int n) => n > 0 ? ((n - 1) / IntSize + 1) : 0;
}