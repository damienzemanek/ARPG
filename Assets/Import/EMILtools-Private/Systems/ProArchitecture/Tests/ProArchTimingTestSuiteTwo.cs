using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EMILtools.Extensions;
using NUnit.Framework;
using ProArchitecture.Data;
using static UnityEngine.Debug;


public readonly unsafe struct OperationTwo<T> where T : unmanaged
{
    // Property access same speed as field access
    readonly delegate*<T*, void> run;
    internal delegate*<T*, void> RunExposedDelegatePtr => run;
    readonly delegate*<T*, bool> shouldRun;
    internal delegate*<T*, bool> shouldRunExposedDelegatePtr => shouldRun;  
    
    public OperationTwo(delegate*<T*, void> _run, delegate*<T*, bool> _shouldRun = null)
    {
        run = _run;
        if(_shouldRun == null) _shouldRun = &OperationTwoExtensions.AlwaysShouldRun<T>;
        else shouldRun = _shouldRun;
    }
}


public readonly unsafe struct LogicTwo<T> where T : unmanaged
{
    public bool hasOperations => count > 0;
    
    public readonly OperationTwo<T>** ops;
    public readonly int count;

    public int Count => count;
    
    // Multi-Operation constructor used by the Builder
    public LogicTwo(OperationTwo<T>** _ops, int opCount)
    {
        ops = _ops;
        count = opCount;
    }

    /// <summary>
    /// Use if you want to call all operations sequentially
    /// </summary>
    /// <param name="data"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TryRunAllSequentially(ref T data)
    {
        fixed (T* dataPtr = &data)
        {
            if (ops == null) return;
            for (int i = 0; i < count; i++)
            {
                // Double De-Reference
                var op = ops[i];
                if (op->shouldRunExposedDelegatePtr(dataPtr))
                    op->RunExposedDelegatePtr(dataPtr);
            }
        }
    }
}


public static unsafe class OperationTwoExtensions
{
    public static bool AlwaysShouldRun<T>(T* data) => true;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnceIndirectedDelegatePtrCall(
        this in OperationTwo<int> operation,
        int* data)
    {
        operation.RunExposedDelegatePtr(data);
    }
}


public unsafe class ProArchTimingTestSuiteTwo
{
    const int Iterations = 5000000;

    public void LocalRun(int* intptr)
    {
        *intptr += 1;   
    }
    
    public static void Run(int* intptr)
    {
        *intptr += 1;   
    }
    public static OperationTwo<int> Operation = new (&Run);

    static int ActionResult = 3;
    void RunAction(int x)
    {
        ActionResult += x;
    }
    public Action<int> action;
    
    [SetUp]
    public void SetUp()
    {
        action = RunAction;
    }

    [Test]
    public void Test_RunAction()
    {
        // JIT warmup
        action(1);
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            action(1);
        }
        sw.Stop();
        Log($"Action<int>: {sw.ElapsedMilliseconds}ms, Result: {ActionResult}");
    }
    
    
    [Test]
    public void Test_RunExposedDelegatePtrRun()
    {
        // JIT warmup
        int data = 3;
        Operation.RunExposedDelegatePtr(&data);
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            Operation.RunExposedDelegatePtr(&data);
        }
        sw.Stop();
        Log($"Operation<int> Exposed Delegate Ptr: {sw.ElapsedMilliseconds}ms, Result: {data}");
    }
    
    [Test]
    public void Test_RunOnceIndirectedDelegatePtrCall()
    {
        // JIT warmup
        int data = 3;
        Operation.OnceIndirectedDelegatePtrCall(&data);
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            Operation.OnceIndirectedDelegatePtrCall(&data);
        }
        sw.Stop();
        Log($"Operation<int> Given Operation and Data Ptr: {sw.ElapsedMilliseconds}ms, Result: {data}");
    }
}