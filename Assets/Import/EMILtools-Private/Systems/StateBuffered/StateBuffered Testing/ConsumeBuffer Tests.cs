using System.Collections;
using EMILtools.Systems;
using EMILtools.Timers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ConsumeBufferTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void Test1_Initializes()
    {
        var pub = new Publisher<int>();

        var buffer = new ConsumeBuffer<int>(() => false, pub, 1f);

        Assert.IsNotNull(buffer);
    }

    [Test]
    public void Test2_ConsumesWhenPredicateTrue()
    {
        bool condition = false;
        bool consumed = false;

        var pub = new Publisher<int>();
        var sub = new SubResolvableCtx<int>(Consume);
        pub.Add(sub);

        var buffer = new ConsumeBuffer<int>(() => condition, pub, 1f);

        buffer.Invoke(5);

        TimerUtility.TickAllFixed(0.5f);

        condition = true;

        TimerUtility.TickAllFixed(0.1f);

        Assert.IsTrue(consumed);

        bool Consume(int v) => consumed = true;
    }
    
    [Test]
    public void Test3_DoesNotConsumeWhenPredicateFalse()
    {
        bool consumed = false;

        var pub = new Publisher<int>();
        var sub = new SubResolvableCtx<int>(Consume);
        pub.Add(sub);

        var buffer = new ConsumeBuffer<int>(() => false, pub, 1f);

        buffer.Invoke(10);

        TimerUtility.TickAllFixed(2f);

        Assert.IsFalse(consumed);

        bool Consume(int v) => consumed = true;
    }
    
    [Test]
    public void Test4_PassesCorrectArgument()
    {
        bool condition = true;
        int received = 0;

        var pub = new Publisher<int>();
        var sub = new SubResolvableCtx<int>(Consume);
        pub.Add(sub);

        var buffer = new ConsumeBuffer<int>(() => condition, pub, 1f);

        buffer.Invoke(42);

        TimerUtility.TickAllFixed(0.1f);

        Assert.AreEqual(42, received);

        bool Consume(int v)
        {
            received = v;
            return true;
        }
    }
    
    [Test]
    public void Test5_ConsumesOnlyOnce()
    {
        bool condition = true;
        int count = 0;

        var pub = new Publisher<int>();
        var sub = new SubResolvableCtx<int>(Consume);
        pub.Add(sub);

        var buffer = new ConsumeBuffer<int>(() => condition, pub, 1f);

        buffer.Invoke(5);

        TimerUtility.TickAllFixed(0.5f);
        TimerUtility.TickAllFixed(0.5f);
        TimerUtility.TickAllFixed(0.5f);

        Assert.AreEqual(1, count);

        bool Consume(int v)
        {
            count++;
            return true;
        }
    }
    
    [Test]
    public void Test6_LatestInvokeOverrides()
    {
        bool condition = false;
        int received = 0;

        var pub = new Publisher<int>();
        var sub = new SubResolvableCtx<int>(Consume);
        pub.Add(sub);

        var buffer = new ConsumeBuffer<int>(() => condition, pub, 1f);

        buffer.Invoke(1);
        buffer.Invoke(2);

        condition = true;

        TimerUtility.TickAllFixed(0.1f);

        Assert.AreEqual(2, received);

        bool Consume(int v)
        {
            received = v;
            return true;
        }
    }
    
    [Test]
    public void Test7_BufferExpiresBeforePredicate()
    {
        bool condition = false;
        bool consumed = false;

        var pub = new Publisher<int>();
        var sub = new SubResolvableCtx<int>(Consume);
        pub.Add(sub);

        var buffer = new ConsumeBuffer<int>(() => condition, pub, 0.5f);

        buffer.Invoke(5);

        TimerUtility.TickAllFixed(1f);

        condition = true;

        TimerUtility.TickAllFixed(0.2f);

        Assert.IsFalse(consumed);

        bool Consume(int v) => consumed = true;
    }
    
    [Test]
    public void Test8_BufferReusable()
    {
        bool condition = true;
        int count = 0;

        var pub = new Publisher<int>();
        var sub = new SubResolvableCtx<int>(Consume);
        pub.Add(sub);

        var buffer = new ConsumeBuffer<int>(() => condition, pub, 1f);

        buffer.Invoke(1);
        TimerUtility.TickAllFixed(0.1f);

        buffer.Invoke(2);
        TimerUtility.TickAllFixed(0.1f);

        Assert.AreEqual(2, count);

        bool Consume(int v)
        {
            count++;
            return true;
        }
    }
    
    [Test]
    public void Test9_PredicateBecomesTrueDuringBuffer()
    {
        bool condition = false;
        bool consumed = false;

        var pub = new Publisher<int>();
        var sub = new SubResolvableCtx<int>(Consume);
        pub.Add(sub);

        var buffer = new ConsumeBuffer<int>(() => condition, pub, 1f);

        buffer.Invoke(3);

        TimerUtility.TickAllFixed(0.4f);

        condition = true;

        TimerUtility.TickAllFixed(0.2f);

        Assert.IsTrue(consumed);

        bool Consume(int v) => consumed = true;
    }

}
