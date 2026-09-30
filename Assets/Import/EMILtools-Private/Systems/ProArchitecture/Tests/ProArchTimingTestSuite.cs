using System;
using System.Diagnostics;
using NUnit.Framework;
using ProArchitecture.Data;
using ProArchitecture.Logic;
using Debug = UnityEngine.Debug;

public unsafe class ProArchTimingTestSuite
{
    const int Iterations = 5000000;

    interface IFoo
    {
        void Bar(int x);
    }
	
    public class Foo : IFoo
    {
        public void Bar(int x)
        {
            var res = x * 3;
        }
    }

    static void Run(int* intptr)
    {
        var res = *intptr * 3;
    }
    readonly delegate*<int*, void> run = &Run;
    
    public static Operation<int> RunOp = new(&Run);
    
    static int _staticRefVar = 3;
    static RefToStatic<int> staticRefVar = new(ref _staticRefVar);

    public static Logic<int> RunLogic = RunOp;


    [Test]
    public void TimingTest()
    {
        // bla
        Stopwatch sw;
		
        // direct
        Foo foo = new Foo();
	
        // interface
        IFoo ifoo = foo;
		
        // delegate 1
        Action<int> del = ifoo.Bar;
		
        // delegate 2
        Delegate del2 = del;
		
        // Make sure everything's JITted:
        ifoo.Bar(3);
        del(3);
        del2.DynamicInvoke(3);
        int something = 3;
        Run(&something);
        RunOp.Run(ref something);
        RunLogic.TryRunAllSequentially(ref something);

        sw = Stopwatch.StartNew();        
        for (int i = 0; i < Iterations; i++)
        {
            del2.DynamicInvoke(3);
        }
        sw.Stop();
        Debug.Log($"Delegate: {sw.ElapsedMilliseconds}ms");		
        
        sw = Stopwatch.StartNew();        
        for (int i = 0; i < Iterations; i++)
        {
            ifoo.Bar(3);
        }
        sw.Stop();
        Debug.Log($"Interface: {sw.ElapsedMilliseconds}ms");		

        sw = Stopwatch.StartNew();        
        for (int i = 0; i < Iterations; i++)
        {
            del(3);
        }
        sw.Stop();
        Debug.Log($"Action<int>: {sw.ElapsedMilliseconds}ms");		

        sw = Stopwatch.StartNew();        
        for (int i = 0; i < Iterations; i++)
        {
            foo.Bar(3);
        }
        sw.Stop();
        Debug.Log($"Direct: {sw.ElapsedMilliseconds}ms");		
		
        // Delegate*
        sw = Stopwatch.StartNew();
        int somevar = 3;
        for (int i = 0; i < Iterations; i++)
        { 
            Run(&somevar);
        }
        sw.Stop();
        Debug.Log($"Delegate * : {sw.ElapsedMilliseconds}ms");		
        
        
        sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            RunOp.Run(ref somevar);
        }
        sw.Stop();
        Debug.Log($"Operation (w/ ref param) : {sw.ElapsedMilliseconds}ms");

        
        int refVar = 3;
        Ref<int> someRef = new(ref refVar);
        sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            RunOp.Run(someRef);
        }
        sw.Stop();
        Debug.Log($"Operation (w/ Ref<T> param) : {sw.ElapsedMilliseconds}ms");	

        
        sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            RunOp.Run(staticRefVar);
        }
        sw.Stop();
        Debug.Log($"Operation (w/ RefToStatic<T> param) : {sw.ElapsedMilliseconds}ms");

        
        sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            RunLogic.RunAllRegarlessOfShouldRun(ref refVar);
        }
        sw.Stop();
        Debug.Log($"Logic (w/ Ref<T> param) : {sw.ElapsedMilliseconds}ms");
        
        sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            RunLogic.RunAllRegarlessOfShouldRun(staticRefVar);
        }
        sw.Stop();
        Debug.Log($"Logic (w/ RefToStatic<T> param) : {sw.ElapsedMilliseconds}ms");

    }
}