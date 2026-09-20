using System;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector;

namespace ProArchitecture.Data;

/// <summary>
/// A handle that allows managed classes to be stored and mutated within unmanaged structs.
/// Bridges the gap between the GC-managed heap and unmanaged pointer logic.
///
/// Features/Configuration:
/// - Pointer-based: Stores a GCHandle as an IntPtr, making the parent struct blittable.
/// - Zero GC Pressure: The handle itself is a value type; only the allocation/free calls impact the GC.
/// - Mutability: Allows updating the target reference without re-allocating the handle.
///
/// Usage:
/// - Allocate: var handle = BlittableReference<MyClass>.Allocate(instance);
/// - Access: MyClass target = handle.Target;
/// - Mutate: handle.Target = newInstance;
/// - Cleanup: MUST call .Free() to release the GCHandle and prevent memory leaks.
///
/// Validation / Exception Handling:
/// - IsAllocated check prevents null pointer access to the GCHandle.
/// - Free() is idempotent; checking against IntPtr.Zero before releasing.
/// </summary>
[Serializable, InlineProperty]
public struct BlittableReference<T> where T : class
{
    public static implicit operator T(BlittableReference<T> handle) => handle.Target;
        
    IntPtr Handle;
    public readonly bool IsAllocated => Handle != IntPtr.Zero;
    public static BlittableReference<T> Allocate(T target) => new BlittableReference<T> { Handle = (IntPtr)GCHandle.Alloc(target) };
    [ShowInInspector, HideLabel]
    public T Target
    {
        get
        {
            if (!IsAllocated) return null;
            return (T)((GCHandle)Handle).Target;
        }
        set
        {
            if (!IsAllocated)
            {
                Handle = (IntPtr)GCHandle.Alloc(value);
                return;
            }

            GCHandle handle = (GCHandle)Handle;
            handle.Target = value;
        }
    }
    public void Free()
    {
        if (Handle == IntPtr.Zero) return;
        ((GCHandle)Handle).Free();
        Handle = IntPtr.Zero;
    }
}