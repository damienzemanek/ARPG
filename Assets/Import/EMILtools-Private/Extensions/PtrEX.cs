using System;
using System.Runtime.CompilerServices;

namespace EMILtools.Extensions;

public static class PtrEX
{
    public static unsafe T* AsPtr<T>(ref T value) where T : unmanaged =>
        (T*)Unsafe.AsPointer(ref value);
        
    public static unsafe ref T AsRef<T>(T* ptr) where T : unmanaged =>
        ref Unsafe.AsRef<T>(ptr);
        
    public static unsafe ref T VoidPtrAsRef<T>(void* ptr) where T : unmanaged 
        => ref Unsafe.AsRef<T>(ptr);
}

public static class IntPtrEX
{
    public static unsafe IntPtr AsIntPtr(void* ptr) => (IntPtr)ptr;
    public static unsafe IntPtr AsIntPtr<T>(ref T ptr) => (IntPtr)Unsafe.AsPointer(ref ptr);
    public static unsafe ref T To<T>(IntPtr ptr) => ref Unsafe.AsRef<T>(ptr.ToPointer());
}