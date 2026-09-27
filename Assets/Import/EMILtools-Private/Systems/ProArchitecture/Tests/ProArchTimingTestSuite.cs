using System;
using System.Diagnostics;
using NUnit.Framework;
using Debug = UnityEngine.Debug;

public unsafe class ProArchTimingTestSuite
{
    const int Iterations = 12000000;

    interface IFoo
    {
        int Bar(int x);
    }
	
    public class Foo : IFoo
    {
        public int Bar(int x)
        {
            return x * 3;
        }
    }

    static int Run(int* intptr)
    {
        ref int refInt = ref *intptr;
        return refInt * 3;
    }
    readonly delegate*<int*, int> run = &Run;

    [Test]
    public void TimingTest()
    {
        // bla
        int x = 3;
        Stopwatch sw;
		
        // direct
        Foo foo = new Foo();
	
        // interface
        IFoo ifoo = foo;
		
        // delegate 1
        Func<int, int> del = ifoo.Bar;
		
        // delegate 2
        Delegate del2 = del;
		
        // Make sure everything's JITted:
        ifoo.Bar(3);
        del(3);
        del2.DynamicInvoke(3);
        int something = 3;
        Run(&something);

        x = 3;
        sw = Stopwatch.StartNew();        
        for (int i = 0; i < Iterations; i++)
        {
            x = (int) del2.DynamicInvoke(3);
        }
        sw.Stop();
        Debug.Log($"Delegate: {sw.ElapsedMilliseconds}ms");		
		
        x = 3;
        sw = Stopwatch.StartNew();        
        for (int i = 0; i < Iterations; i++)
        {
            x = ifoo.Bar(x);
        }
        sw.Stop();
        Debug.Log($"Interface: {sw.ElapsedMilliseconds}ms");		

        x = 3;
        sw = Stopwatch.StartNew();        
        for (int i = 0; i < Iterations; i++)
        {
            x = del(x);
        }
        sw.Stop();
        Debug.Log($"Func<int, int>: {sw.ElapsedMilliseconds}ms");		

        x = 3;
        sw = Stopwatch.StartNew();        
        for (int i = 0; i < Iterations; i++)
        {
            x = foo.Bar(x);
        }
        sw.Stop();
        Debug.Log($"Direct: {sw.ElapsedMilliseconds}ms");		
		
        // Delegate*
        x = 3;
        sw = Stopwatch.StartNew();

        for (int i = 0; i < Iterations; i++)
        {
            x = Run(&x);
        }

        sw.Stop();
        Debug.Log($"Delegate* : {sw.ElapsedMilliseconds}ms");		
    }
}
