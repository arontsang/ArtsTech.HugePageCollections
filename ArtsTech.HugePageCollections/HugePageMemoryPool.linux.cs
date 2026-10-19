using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ArtsTech.HugePageCollections;

partial class HugePageMemoryPool<T>
{
    [SupportedOSPlatform("linux")]
    private class LinuxHugePageMemoryAllocator : MemoryPool<T>
    {
        protected override void Dispose(bool disposing)
        {
        
        }

        public override IMemoryOwner<T> Rent(int minBufferSize = -1)
        {
            return new LinuxHugePageOwnedMemory<T>(minBufferSize);
        }

        public override int MaxBufferSize => Int32.MaxValue;

        
    }
}
[SupportedOSPlatform("linux")]
file class LinuxHugePageOwnedMemory<T>(int length) : IMemoryOwner<T>
    where T : unmanaged
{
    private readonly LinuxHugePageMemoryManager<T> _manager = new(length);
    private bool _isDisposed;

    public void Dispose()
    {
        if (!_isDisposed)
        {
            IDisposable disposal = _manager;
            disposal.Dispose();
            _isDisposed = true;
        }
    }

    public Memory<T> Memory => _isDisposed
        ? throw new ObjectDisposedException(nameof(LinuxHugePageOwnedMemory<T>))
        : _manager.Memory;
}

[SupportedOSPlatform("linux")]
file static unsafe class LinuxHugePageMemoryInterop
{
    [DllImport("libc", SetLastError = true)]
    public static extern byte* mmap(void* addr, nuint length, Prots prot, Flags flags, int fd, nint offset);
    [DllImport("libc", SetLastError = true)]
    public static extern int munmap(byte* addr, nuint length);
    
    [DllImport("libc", SetLastError = true)]
    public static extern int madvise(byte* addr, UIntPtr length, Advice advice);

}

[Flags]
file enum Prots : uint
{
    READ = 0x01,
    WRITE = 0x02,
    
}

[Flags]
file enum Flags : uint
{
    PRIVATE = 0x02,
    ANONYMOUS = 0x20,
    HUGE = 0x40000
}

file enum Advice : uint
{
    MADV_HUGEPAGE = 14
}

[SupportedOSPlatform("linux")]
file unsafe class LinuxHugePageMemoryManager<T> : MemoryManager<T> where T : unmanaged
{
    private readonly byte* _pointer;
    private readonly int _length;
    private readonly int _requestedLength;
    private bool _isDisposed;

    public LinuxHugePageMemoryManager(int length)
    {
        _requestedLength = length;
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));
        
        _length = Power2Size(length * sizeof(T));
        var size = (nuint) _length;
        _pointer = LinuxHugePageMemoryInterop.mmap(null, size, Prots.READ | Prots.WRITE, Flags.ANONYMOUS | Flags.PRIVATE, -1, 0);
        if (_pointer == (byte*)new IntPtr(-1))
        {
            throw new OutOfMemoryException($"Linux huge-page allocation failed. errno: {Marshal.GetLastPInvokeError()}");
        }

        if (0 != LinuxHugePageMemoryInterop.madvise(_pointer, size, Advice.MADV_HUGEPAGE))
            Console.WriteLine($"Warning: madvise failed with error code: {Marshal.GetLastWin32Error()}");
        for (ulong i = 0; i < size; i += 4096)
        {
            _pointer[i] = 1; // Touch each page boundary to trigger the 2MB allocation
        }
    }
    
    

    public override Span<T> GetSpan()
    {
        if (_isDisposed) 
            throw new ObjectDisposedException(nameof(LinuxHugePageMemoryManager<T>));
        return new Span<T>(_pointer, _requestedLength);
    }

    public override MemoryHandle Pin(int elementIndex = 0)
    {
        if (_isDisposed) 
            throw new ObjectDisposedException(nameof(LinuxHugePageMemoryManager<T>));
        if (elementIndex < 0 || elementIndex >= _requestedLength) 
            throw new ArgumentOutOfRangeException(nameof(elementIndex));
        return new MemoryHandle(_pointer + elementIndex * sizeof(T), default, this);
    }

    public override void Unpin()
    {
        
    }
    
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            LinuxHugePageMemoryInterop.munmap(_pointer, (nuint) _length);
            _isDisposed = true;
        }
    }

    private int Power2Size(int size)
    {
        return 1 << BitOperations.Log2((uint)size - 1) + 1;
    }
}
