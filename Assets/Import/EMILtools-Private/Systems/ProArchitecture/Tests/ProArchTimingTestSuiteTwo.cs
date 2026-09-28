using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EMILtools.Extensions;
using NUnit.Framework;
using static UnityEngine.Debug;


public readonly unsafe struct OperationTwo<T> where T : unmanaged
{
    readonly delegate*<T*, void> run;
    public delegate*<T*, void> RunExposedDelegatePtr => run;
    
    public OperationTwo(delegate*<T*, void> _run)
    {
        run = _run;
    }


    public OperationTwo(delegate*<T*, void> _run, delegate*<T*, bool> _shouldRun)
    {
        run = _run;
    }
    
    /// <summary>
    /// converts the ref T to a pointer, and calls the function
    /// </summary>
    /// <param name="data"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RunPinned(ref T data)
    {
        fixed (T* ptr = &data) run(ptr);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RunPointer(T* data)
    {
        run(data);
    }
    

}

public static unsafe class OperationTwoExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RunGivenOperationAndDataPtr(
        this in OperationTwo<int> operation,
        int* data)
    {
        operation.RunExposedDelegatePtr(data);
    }
}


public unsafe class ProArchTimingTestSuiteTwo
{
    const int Iterations = 5000000;

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
    public void Test_RunPinnedRun()
    {
        // JIT warmup
        int data = 3;
        Operation.RunPinned(ref data);
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            Operation.RunPinned(ref data);
        }
        sw.Stop();
        Log($"Operation<int> Pinned: {sw.ElapsedMilliseconds}ms, Result: {data}");
    }
    
    [Test]
    public void Test_RunPointerRun()
    {
        // JIT warmup
        int data = 3;
        Operation.RunPointer(&data);
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            Operation.RunPointer(&data);
        }
        sw.Stop();
        Log($"Operation<int> Pointer: {sw.ElapsedMilliseconds}ms, Result: {data}");
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
    public void Test_RunGivenOperationAndDataPtr()
    {
        // JIT warmup
        int data = 3;
        Operation.RunGivenOperationAndDataPtr(&data);
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            Operation.RunGivenOperationAndDataPtr(&data);
        }
        sw.Stop();
        Log($"Operation<int> Given Operation and Data Ptr: {sw.ElapsedMilliseconds}ms, Result: {data}");
    }
}