using System.Runtime.CompilerServices;
using ProArchitecture.Data;


public unsafe struct PtrHandle<T> where T : unmanaged
{
    public T* Ptr;
    public PtrHandle(T* ptr) => Ptr = ptr;
}