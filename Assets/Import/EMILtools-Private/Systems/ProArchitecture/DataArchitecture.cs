using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ProArchitecture.Logic;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace ProArchitecture.Data
{

    public static class Batcher
    {
        // Logics Batching
        public static void Process<T, TMetaDeta>(ref Data<T, TMetaDeta> data, Logic<T> logic) 
            where T : unmanaged
            where TMetaDeta : unmanaged
        {
            for(int i = 0; i < data.currentSize; i++)
            {
                ref Data<T, TMetaDeta>.DataWrapper element = ref data.GetWrapper(i);
                if(!element.Active) continue;
                logic.TryRunAllSequentially(ref element.DataVolatile);
            }
        }
        
        // Logic Group Double Batcher Process
        public static void Process<T, TMetaDeta>(ref Data<T, TMetaDeta> data, LogicGroup<T> logicGroup) 
            where T : unmanaged
            where TMetaDeta : unmanaged
        {
            for(int i = 0; i < logicGroup.currentSize; i++)
                if(logicGroup.LogicsData.GetWrapper(i).Active)
                    Batcher.Process(ref data, logicGroup.LogicsData[i].GetRef);
        }
    }

    public static class PtrEX
    {
        public static unsafe T* AsPtr<T>(ref T value) where T : unmanaged =>
            (T*)Unsafe.AsPointer(ref value);
        
        public static unsafe ref T AsRef<T>(T* ptr) where T : unmanaged =>
            ref Unsafe.AsRef<T>(ptr);
        
        public static unsafe ref T VPtrToRef<T>(void* ptr) where T : unmanaged 
            => ref Unsafe.AsRef<T>(ptr);
    }
    
    
    public static unsafe class IntPtrPtrTo<T> where T : unmanaged
    {
        public static ref T GetRef(IntPtr* data)
        {
            IntPtr intptr = *data;
            T* tptr = (T*)intptr;
            ref T t = ref *tptr;
            return ref t;
        }
    }

    public static unsafe class IntPtrTo<T> where T : unmanaged
    {
        public static ref T GetRef(ref IntPtr data)
        {
            T* tptr = (T*)data;
            ref T t = ref *tptr;
            return ref t;
        }
    }
    
    /// <summary>
    /// A memory-aligned, 1-byte boolean for unmanaged and blittable contexts.
    /// Solves the size ambiguity of the standard C# 'bool' (which can be 4 bytes) in structs.
    ///
    /// Features/Configuration:
    /// - Memory Efficiency: Strictly 1 byte, allowing for predictable struct packing.
    /// - Blittable: Safe for use in unmanaged memory, NativeLists, and across P/Invoke boundaries.
    /// - Inspector Friendly: Custom drawer support via ByteBool (shows as a standard toggle).
    ///
    /// Usage:
    /// - Replace 'bool' with 'ByteBool' in any struct destined for Data<T> or UnsafeList.
    /// - Supports implicit conversion to and from standard 'bool'.
    /// - Use .active for property access or .Set() for direct mutation.
    /// </summary>
    [Serializable, InlineProperty]
    public struct ByteBool
    {
        [SerializeField, HideInInspector] byte value;

        [ShowInInspector, HideLabel]
        public bool active
        {
            get => value != 0;
            set => this.value = (byte)(value ? 1 : 0);
        }
        
        public void Set(bool _value) => value = (byte)(_value ? 1 : 0);
        public static implicit operator bool(ByteBool b) => b.active;
        public static implicit operator ByteBool(bool b) => new ByteBool { value = (byte)(b ? 1 : 0) };
        public ByteBool(bool _value) => value = (byte)(_value ? 1 : 0);
    }
    
    
    

    public struct OrTupled<T1, T2> where T1 : unmanaged where T2 : unmanaged
    {
        public static implicit operator T1(in OrTupled<T1, T2> tupled)
        {
            if(tupled.usingFirst) return tupled.Item1;
            Debug.LogError("Tried to get the first item of a OrTupled<T1, T2> when using the second");
            return default;
        }

        public static implicit operator T2(in OrTupled<T1, T2> tupled)
        {
            if(!tupled.usingFirst) return tupled.Item2;
            Debug.LogError("Tried to get the second item of a OrTupled<T1, T2> when using the first");
            return default;
        }
        
        public ByteBool usingFirst;
        public T1 Item1;
        public T2 Item2;
        public OrTupled(T1 item1)
        {
            Item1 = item1;
            Item2 = default;
            usingFirst = new ByteBool();
            usingFirst.Set(true);
        }
        public OrTupled(T2 item2)
        {
            Item1 = default;
            Item2 = item2;
            usingFirst = new ByteBool();
            usingFirst.Set(false);
        }
    }


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

    /// <summary>
    /// A tiny handle for a single unmanaged value allocated on the unmanaged heap.
    /// Provides stable pointer access that survives struct-copying (pass-by-value).
    ///
    /// Features/Configuration:
    /// - Pointer Stability: The data resides at a fixed address; copying the handle does not copy the data.
    /// - Life-cycle Sharing: Multiple handles point to the same ByteBool or state flag.
    /// - Performance: Raw pointer access via .Ref or .Ptr; zero overhead beyond the allocation.
    ///
    /// Usage:
    /// - Initialize: var handle = new MallocHandle<int>(10, Allocator.Persistent);
    /// - Access: int val = handle.Ref; or *handle.Ptr = 20;
    /// - Cleanup: MUST call .Free() when the last owner is finished.
    ///
    /// Validation / Exception Handling:
    /// - Implicit conversion allows the handle to be used as the underlying type in many expressions.
    /// - Requires manual memory management (Malloc/Free).
    /// </summary>
    public unsafe struct MallocHandle<T> where T : unmanaged
    {
        public static implicit operator T(MallocHandle<T> handle) => handle.Ref;
        public static implicit operator MallocHandle<T>(in T value) => new(value);
        
        public ref T Ref => ref *Ptr;
        public T* Ptr;
        Allocator allocation;
        public bool IsCreated => Ptr != null;
        public MallocHandle(T initValue, Allocator allocator = Allocator.Persistent)
        {
            Ptr = (T*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<T>(), 
                UnsafeUtility.AlignOf<T>(),
                allocation = allocator);
            
            UnsafeUtility.WriteArrayElement(Ptr, 0, initValue);
        }
        public void Free() => UnsafeUtility.Free(Ptr, allocation);
    }
    
    public struct NoMtd { }
    
    /// <summary>
    /// High-performance, cache-friendly data store utilizing an Object Pooling Defragmentation Strategy.
    /// Designed for high-frequency mutation and batch processing with zero GC overhead.
    ///
    /// Why this over NativeList<T>?
    /// - Volatile Refs: Accesses elements via 'ref T', avoiding stack-copy cycles during mutation.
    /// - Synchronized Lifecycle: Uses a shared MallocHandle flag to invalidate all copies upon disposal.
    /// - Registry Protected: Automatically registers with DataActiveRegistry for safety-net cleanup.
    /// - MetaData Pattern: Universally available metadata per-element for system-specific tagging.
    /// 
    /// Usage:
    /// - Stable Keys: Use the returned 'int' index as a stable handle for the element's lifetime.
    /// - Ownership: The creating system owns the data and MUST call .Dispose().
    /// - Batching: Optimized for the 'Batcher.Process' loops to iterate over active elements only.
    ///
    /// Validation / Exception Handling:
    /// - Disposal is synchronized: Disposing one copy kills all copies (IsActive becomes false).
    /// - Internal list resizes automatically while maintaining the stability of the lifecycle flag.
    /// - Throws NotImplementedException if initialized via default constructor (use capacity ctor).
    /// </summary>
    public unsafe struct Data<T, TMetaData> 
        where T : unmanaged
        where TMetaData : unmanaged
    {
        /// <summary>
        /// For:
        /// - "Removal" of unsued Indicies from batch processes
        /// - Object pooling
        /// </summary>
        public unsafe struct DataWrapper
        {
            public ByteBool Active;
            T data;
            TMetaData mtd;
            public DataWrapper() => throw new System.NotImplementedException("This struct is only a wrapper for the Data struct, and should not be initialized directly.");
            public DataWrapper(T getData, TMetaData mtd)
            {
                Active = new ByteBool();
                Active.Set(true);
                this.data = getData;
                this.mtd = mtd;
            }

            /// <summary>
            /// During Allocation, if SetCapacity is called, these refs will point to nothing
            /// Don't store these if you are unsure about potential capacity cahnges
            /// </summary>
            public ref T DataVolatile
            {
                get
                {
                    fixed (T* ptr = &data)
                        return ref *ptr;
                }
            }
            
            public ref TMetaData MetaDataVolatile
            {
                get
                {
                    fixed (TMetaData* ptr = &mtd)
                        return ref *ptr;
                }
            }
        }
        
        
        public int currentSize => nextIndex;
        MallocHandle<ByteBool> dataIsActive;
        public bool DataIsActive => dataIsActive.IsCreated && dataIsActive.Ref.active;   
        int nextIndex;
        internal UnsafeList<DataWrapper> data;
        public int Capacity => data.Capacity;
        public int remainingCapacitySpace;
        
        public Data() => throw new System.NotImplementedException("Use Data(int capacity, Allocator allocator) constructor to initialize with a specific capacity and allocator.");

        public Data(int explicitCapacity, Allocator allocator)
        {
            var oldCapacity = explicitCapacity;
            explicitCapacity = Mathf.NextPowerOfTwo(explicitCapacity);
            remainingCapacitySpace = oldCapacity - explicitCapacity;
            nextIndex = 0;
            data = new UnsafeList<DataWrapper>(explicitCapacity, allocator);
            
            dataIsActive = new ByteBool(true);
            DataActiveRegistry.Register(dataIsActive.Ptr);
        }
        
        public Data(int explicitCapacity, Allocator allocator, out int remainingCapacitySpace)
        {
            var oldCapacity = explicitCapacity;
            explicitCapacity = Mathf.NextPowerOfTwo(explicitCapacity);
            this.remainingCapacitySpace = remainingCapacitySpace = oldCapacity - explicitCapacity;
            nextIndex = 0;
            data = new UnsafeList<DataWrapper>(explicitCapacity, allocator);
            
            dataIsActive = new ByteBool(true);
            DataActiveRegistry.Register(dataIsActive.Ptr);
        }

        public Data(Data<T, TMetaData> tempAllocatedEvents)
        {
            nextIndex = tempAllocatedEvents.nextIndex;
            data = tempAllocatedEvents.data;
            dataIsActive = tempAllocatedEvents.dataIsActive;
            remainingCapacitySpace = tempAllocatedEvents.remainingCapacitySpace;
        }

        public Data(ReadOnlySpan<T> span, Allocator allocator)
        {
            int count = span.Length;
            int explicitCapacity = Mathf.NextPowerOfTwo(count);
            this.remainingCapacitySpace = explicitCapacity - count;
            this.nextIndex = count;
            
            // Initialize the list with the correct capacity
            data = new UnsafeList<DataWrapper>(explicitCapacity, allocator);
            data.Resize(count);
            
            // Bulk copy the data into the wrappers
            for (int i = 0; i < count; i++)
            {
                data[i] = new DataWrapper(span[i], default);
            }
            
            // Lifecycle setup
            dataIsActive = new MallocHandle<ByteBool>(new ByteBool(true), allocator);
            DataActiveRegistry.Register(dataIsActive.Ptr);
        }

        /// <summary>
        /// Allocates a new element and returns its stable index.
        /// Capacity is reserved memory space, can be uninitialized memory that points to random stuff
        /// Resize() adjusts Length, which is the number of initialized elements.
        /// </summary>
        /// <param name="_data">Data to store.</param>
        /// <returns>Allocated element index.</returns>
        public int Allocate(ref T _data)
        {
            if (nextIndex >= data.Capacity) data.SetCapacity(math.max(1, data.Capacity * 2));
            
            data.Resize(nextIndex + 1);
            var mtd = default(TMetaData);
            data[nextIndex] = new DataWrapper(_data, mtd);
            var ret = nextIndex++;
            remainingCapacitySpace--;
            return ret;
        }
        public void Allocate(ref T _data, out int allocationId) => allocationId = Allocate(ref _data);
        public void Allocate(T _data, out int allocationId) => allocationId = Allocate(ref _data);
        
        /// <summary>
        /// Used for Object Pooling to reallocate unsued indicies
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newData"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void ReAllocateInactive(int id, ref T newData, ref TMetaData _mtd)
        {
            if(id >= currentSize) throw new System.IndexOutOfRangeException($"Index {id} out of bounds. Data length is {currentSize}");
            if(data[id].Active) throw new System.IndexOutOfRangeException($"Index {id} Trying to reallocate an active element.");
            data[id] = new DataWrapper(newData, _mtd);
        }
        
        public ref T this[int id] => ref Get(id);

        /// <summary>
        /// used by the batcher to get the data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public ref DataWrapper GetWrapper(int id)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if ((uint)id >= (uint)currentSize) throw new System.IndexOutOfRangeException($"Index {id} out of bounds. Data length is {data.Length}");
#endif
            return ref data.ElementAt(id); 
        }

        /// <summary>
        /// Use directly to get the data.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ref T Get(int id)
        {
            ref DataWrapper element = ref GetWrapper(id);
            return ref element.DataVolatile;
        }
        
        
        public void Dispose()
        {
            if (!DataIsActive) return;
            
            dataIsActive.Ref = false;
            if (data.IsCreated) data.Dispose();
            
            DataActiveRegistry.Unregister(dataIsActive.Ptr);
            dataIsActive.Free();
        }
    }
    
    
    // Notes on v3 (11:31 pm, may 21)
    // refactored system with validation, added summary, added public ctor throw, removed length manipultation by ctor
    // allocate is dynamic and resizes when capacity changes
    
    // Development of batcher, initially thought this was good, realized that data.active is not a valid check, need to be isntanced at the UnsafeLIst<T>[] level
    // for(int i = 0; i < data.currentSize; i++)
    // {
    //     ref T element = ref data[i];
    //     if(!data.active) continue;
    //     logic.TryRun(ref element);
    // }
    // so i need to wrap it in a struct
    
    // development of the wrapper
    // We replace the 'ref get' with a pointer-based ref return
    // the getting of the internal data initially wasnt going to work because unity disallows ref returns of struct fields,
    // but this is circumvented by using a pointer and fixed statement to get a ref to the data field.
    // This allows us to have the wrapper struct contain the active flag and the data, while still allowing us to get a ref
    // to the data for direct mutation of the internal data
    //
    // initilaly it looked like
    //public ref T Data => return ref data;
    //
    // final:
    // public ref T Data
    // {
    // get
    // {
    //     fixed (T* ptr = &data)
    //         return ref *ptr;
    // }
    // }
    
}
