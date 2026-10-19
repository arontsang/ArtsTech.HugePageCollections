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

    public void Dispose()
    {
            
    }

    public Memory<T> Memory => _manager.Memory;
}

[SupportedOSPlatform("linux")]
file static unsafe class LinuxHugePageMemoryInterop
{
    [DllImport("libc", SetLastError = true)]
    public static extern byte* mmap(void* addr, nuint length, Prots prot, Flags flags, int fd, nint offset);
    [DllImport("libc", SetLastError = true)]
    public static extern int munmap(void* addr, nuint length);
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
        _pointer = LinuxHugePageMemoryInterop.mmap(null, size, Prots.READ | Prots.WRITE, Flags.ANONYMOUS | Flags.HUGE | Flags.PRIVATE, -1, 0);
        if (_pointer == null) throw new OutOfMemoryException($"Linux HugePage allocation failed. Error: {Marshal.GetLastWin32Error()}");
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
        return new MemoryHandle(_pointer + elementIndex);
    }

    public override void Unpin()
    {
        
    }
    
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (_pointer != null)
            {
                LinuxHugePageMemoryInterop.munmap(_pointer, (nuint) _length);
            }
            _isDisposed = true;
        }
    }

    private int Power2Size(int size)
    {
        return 1 << BitOperations.Log2((uint)size - 1) + 1;
    }
}