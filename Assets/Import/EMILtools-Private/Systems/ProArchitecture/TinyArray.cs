using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ProArchitecture.Data
{
    /// <summary>
    /// A minimal, unmanaged heap-allocated array.
    /// Provides raw pointer access with zero managed overhead or safety handles.
    /// </summary>
    public unsafe struct TinyArray<T> : IDisposable where T : unmanaged
    {
        public T* Ptr;
        
        // The fixed size of the allocation
        public readonly int Length;

        public TinyArray(int length, Allocator allocator = Allocator.Persistent)
        {
            if (length <= 0)
            {
                Ptr = null;
                Length = 0;
                return;
            }

            Length = length;
            int size = length * UnsafeUtility.SizeOf<T>();
            
            // Malloc provides the raw memory without any overhead like headers or safety handles
            Ptr = (T*)UnsafeUtility.Malloc(
                size, 
                UnsafeUtility.AlignOf<T>(), 
                allocator);
        }

        /// <summary>
        /// Indexer for clean syntax: array[i] = value;
        /// </summary>
        public ref T this[int index]
        {
            get
            {
                #if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (index < 0 || index >= Length) throw new IndexOutOfRangeException($"Index {index} is out of range [0, {Length})");
                #endif
                return ref Ptr[index];
            }
        }

        public void Dispose()
        {
            if (Ptr == null) return;
            UnsafeUtility.Free(Ptr, Allocator.Persistent);
            Ptr = null;
        }
    }
}
