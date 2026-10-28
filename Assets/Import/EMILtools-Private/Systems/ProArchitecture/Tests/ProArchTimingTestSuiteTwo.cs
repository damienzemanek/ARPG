using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EMILtools.Extensions;
using NUnit.Framework;
using ProArchitecture.Data;
using static UnityEngine.Debug;
using System.Runtime.InteropServices;


public readonly unsafe struct LogicTwo<T> 
    where T : unmanaged
{
    public static LogicTwo<T> EmptyStatic = new LogicTwo<T>(null, 0, true);
    public bool HasOperations => Count != 0;
    
    public readonly ByteBool isStatic;
    public readonly OperationTwo<T>* ops;
    public readonly ushort Count;
    
    // Multi-Operation constructor used by the Builder
    public LogicTwo(OperationTwo<T>* _ops, ushort opCount, bool isStatic)
    {
        ops = _ops;
        Count = opCount;
        this.isStatic = isStatic;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RunAll(T* dataPtr)
    {
        if (ops == null) return;
        for (int i = 0; i < Count; i++)
        {
            ops[i].run(dataPtr);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TryRunAllSequentially(ref T data)
    {
        if (ops == null) return;
        fixed (T* dataPtr = &data)
        {
            for (int i = 0; i < Count; i++)
            {
                var shouldRunPtr = ops[i].shouldRun;
                if (shouldRunPtr == null || shouldRunPtr(dataPtr))
                    ops[i].run(dataPtr);
            }
        }
    }
    
}


public readonly unsafe struct OperationTwo<T> where T : unmanaged
{
    public readonly delegate*<T*, void> run;
    public readonly delegate*<T*, bool> shouldRun;
    
    public OperationTwo(delegate*<T*, void> _run, delegate*<T*, bool> _shouldRun = null)
    {
        run = _run;
        shouldRun = _shouldRun;
    }
}

public class ComplexClass
{
    public const int DataSize = 16384; // 64 KB (16,384 * 4 bytes) exceeding L1 cache size
    public readonly float[] Values = new float[DataSize];
    public float Result;
}

public unsafe struct ComplexStruct
{
    public const int DataSize = 16384; // 64 KB (16,384 * 4 bytes) exceeding L1 cache size
    public fixed float Values[DataSize];
    public float Result;
}

public static unsafe class OperationTwoExtensions
{
    public static bool AlwaysShouldRun<T>(T* data) => true;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnceIndirectedDelegatePtrCall(
        this in OperationTwo<ComplexStruct> operation,
        ComplexStruct* data)
    {
        operation.run(data);
    }
}

public unsafe class ProArchTimingTestSuiteTwo
{
    const int Iterations = 10000;
    const int ArraySize = 64;
    const int ArrayIterations = 1000;
    
    public static void Run(ComplexStruct* ptr)
    {
        float sum = 0f;
        for (int i = 0; i < ComplexStruct.DataSize; i++)
        {
            sum += ptr->Values[i] * 1.0001f + 0.5f;
        }
        ptr->Result = sum;
    }
    
    public static void Increment(ComplexStruct* data)
    {
        data->Result += 1f;
    }

    public static OperationTwo<ComplexStruct> Operation = new(&Run);
    public static OperationTwo<ComplexStruct> IncrementOperation = new(&Increment);

    public static LogicTwo<ComplexStruct> Logic;

    private static IntPtr LogicOpsMemory;
    private static OperationTwo<ComplexStruct>* LogicOps;

    public static void SetupLogic()
    {
        int size = sizeof(OperationTwo<ComplexStruct>) * 2;

        LogicOpsMemory = Marshal.AllocHGlobal(size);
        LogicOps = (OperationTwo<ComplexStruct>*)LogicOpsMemory;

        LogicOps[0] = Operation;
        LogicOps[1] = IncrementOperation;

        Logic = new LogicTwo<ComplexStruct>(
            LogicOps,
            2,
            true);
    }

    public void RunAction(ComplexClass obj)
    {
        float sum = 0f;
        var values = obj.Values;
        for (int i = 0; i < ComplexClass.DataSize; i++)
        {
            sum += values[i] * 1.0001f + 0.5f;
        }
        obj.Result = sum;
    }
    
    public void IncrementAction(ComplexClass obj)
    {
        obj.Result += 1f;
    }

    public Action<ComplexClass> action;
    
    [SetUp]
    public void SetUp()
    {
        action = RunAction;
    }
    
    [TearDown]
    public void TearDown()
    {
        if (LogicOpsMemory != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(LogicOpsMemory);
            LogicOpsMemory = IntPtr.Zero;
            LogicOps = null;
        }
    }

    [Test]
    public void Test_RunAction()
    {
        var localAction = action;
        var data = new ComplexClass();

        // JIT warmup
        localAction(data);
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            localAction(data);
        }
        sw.Stop();
        Log($"Action<ComplexClass>: {sw.ElapsedMilliseconds}ms, Result: {data.Result}");
    }
    
    [Test]
    public void Test_RunExposedDelegatePtrRun()
    {
        ComplexStruct data = default;
        ComplexStruct* dataPtr = &data;
        var op = Operation;
        
        // JIT warmup
        op.run(dataPtr);
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            op.run(dataPtr);
        }
        sw.Stop();
        Log($"Operation<ComplexStruct> Delegate Ptr (Direct field call): {sw.ElapsedMilliseconds}ms, Result: {data.Result}");
    }
    
    [Test]
    public void Test_RunOnceIndirectedDelegatePtrCall()
    {
        ComplexStruct data = default;
        ComplexStruct* dataPtr = &data;
        var op = Operation;
        
        // JIT warmup
        op.OnceIndirectedDelegatePtrCall(dataPtr);
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            op.OnceIndirectedDelegatePtrCall(dataPtr);
        }
        sw.Stop();
        Log($"Operation<ComplexStruct> Given Operation and Data Ptr: {sw.ElapsedMilliseconds}ms, Result: {data.Result}");
    }
    
    
    [Test]
    public void Test_ListOfActions()
    {
        var data = new ComplexClass();
        var actions = new List<Action<ComplexClass>>
        {
            RunAction,
            IncrementAction,
        };

        // JIT warmup
        // Warmup
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i](data);
        }
        var sw = Stopwatch.StartNew();

        for (int iteration = 0; iteration < Iterations; iteration++)
        {
            for (int i = 0; i < actions.Count; i++) actions[i](data);
        }

        sw.Stop();

        Log($"List<Action<ComplexClass>>: " + $"{sw.ElapsedMilliseconds}ms, Result: {data.Result}");
    }
    
    [Test]
    public void Test_LogicTwo()
    {
        SetupLogic();
        ComplexStruct data = default;
        var dataPtr = (ComplexStruct*)(Unsafe.AsPointer(ref data));
        Logic.RunAll(dataPtr);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            Logic.RunAll(dataPtr);
        }
        sw.Stop();
        Log($"LogicTwo<ComplexStruct> Given Logic and Data Ptr: {sw.ElapsedMilliseconds}ms, Result: {data.Result}");
    }
    [Test]
    public void Test_LogicTwo_IncrementOnly()
    {
        SetupLogic();

        // Replace the two operations with Increment operations.
        LogicOps[0] = IncrementOperation;
        LogicOps[1] = IncrementOperation;

        var logic = Logic;

        ComplexStruct data = default;
        var dataPtr = (ComplexStruct*)(Unsafe.AsPointer(ref data));

        // Warmup
        Logic.RunAll(dataPtr);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < DispatchIterations; i++)
        {
            Logic.RunAll(dataPtr);
        }

        sw.Stop();

        Log(
            $"LogicTwo Increment x2: " +
            $"{sw.ElapsedMilliseconds}ms, Result: {data.Result}");
    }
    const int DispatchIterations = 10_000_000;


    [Test]
    public void Test_ListOfActions_IncrementOnly()
    {
        var data = new ComplexClass();

        var actions = new List<Action<ComplexClass>>
        {
            IncrementAction,
            IncrementAction
        };

        // Warmup
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i](data);
        }

        var sw = Stopwatch.StartNew();

        for (int iteration = 0; iteration < DispatchIterations; iteration++)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i](data);
            }
        }

        sw.Stop();

        Log(
            $"List<Action> Increment x2: " +
            $"{sw.ElapsedMilliseconds}ms, Result: {data.Result}");
    }


}