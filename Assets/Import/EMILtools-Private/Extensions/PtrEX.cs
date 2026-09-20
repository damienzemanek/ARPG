using System.Runtime.CompilerServices;

namespace EMILtools.Extensions;

public static class PtrEX
{
    public static unsafe T* AsPtr<T>(ref T value) where T : unmanaged =>
        (T*)Unsafe.AsPointer(ref value);
        
    public static unsafe ref T AsRef<T>(T* ptr) where T : unmanaged =>
        ref Unsafe.AsRef<T>(ptr);
        
    public static unsafe ref T VPtrToRef<T>(void* ptr) where T : unmanaged 
        => ref Unsafe.AsRef<T>(ptr);
}