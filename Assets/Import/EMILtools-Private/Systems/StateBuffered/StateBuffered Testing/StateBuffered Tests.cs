using System.Collections;
using NUnit.Framework;
using EMILtools.Systems;
using EMILtools.Timers;
using UnityEngine;
using UnityEngine.TestTools;

public class DelayBufferTests
{
    
    [Test]
    public void Test1_Initializes()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 1f);
        
        Assert.IsNotNull(bufferedBool);
    }
    
    [Test]
    public void Test2_CanSet()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 1f);

        bufferedBool.SetBuffered(true);
        TimerUtility.TickAllFixed(0.9f);
        Assert.AreEqual(false, bufferedBool.Value);
        TimerUtility.TickAllFixed(0.2f);
        Assert.AreEqual(true, bufferedBool.Value);
    }
    
    [Test]
    public void Test3_NoChangeWithoutSet()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 1f);

        TimerUtility.TickAllFixed(2f);

        Assert.AreEqual(false, bufferedBool.Value);
    }
    
    [Test]
    public void Test4_RapidSetsRestartBuffer()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 1f);

        bufferedBool.SetBuffered(true);

        TimerUtility.TickAllFixed(0.5f);

        bufferedBool.SetBuffered(false);

        TimerUtility.TickAllFixed(0.6f);

        // Should still be old value because timer restarted
        Assert.AreEqual(false, bufferedBool.Value);

        TimerUtility.TickAllFixed(0.5f);

        Assert.AreEqual(false, bufferedBool.Value);
    }

    
    [Test]
    public void Test5_MultipleChangesDuringBuffer()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 1f);

        bufferedBool.SetBuffered(true);
        TimerUtility.TickAllFixed(0.3f);

        bufferedBool.SetBuffered(false);
        TimerUtility.TickAllFixed(0.3f);

        bufferedBool.SetBuffered(true);

        TimerUtility.TickAllFixed(1.1f);

        Assert.AreEqual(true, bufferedBool.Value);
    }
    
    [Test]
    public void Test6_ExactBoundary()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 1f);

        bufferedBool.SetBuffered(true);

        TimerUtility.TickAllFixed(1f);

        Assert.AreEqual(true, bufferedBool.Value);
    }
    
    [Test]
    public void Test7_TimerOvershoot()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 1f);

        bufferedBool.SetBuffered(true);

        TimerUtility.TickAllFixed(5f);

        Assert.AreEqual(true, bufferedBool.Value);
    }
    
    [Test]
    public void Test8_SetSameValue()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 1f);

        bufferedBool.SetBuffered(false);

        TimerUtility.TickAllFixed(1.1f);

        Assert.AreEqual(false, bufferedBool.Value);
    }
    
    [Test]
    public void Test9_FlipStateRapidly()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 1f);

        bufferedBool.SetBuffered(true);
        TimerUtility.TickAllFixed(0.2f);

        bufferedBool.SetBuffered(false);
        TimerUtility.TickAllFixed(0.2f);

        bufferedBool.SetBuffered(true);
        TimerUtility.TickAllFixed(0.2f);

        bufferedBool.SetBuffered(false);

        TimerUtility.TickAllFixed(1.2f);

        Assert.AreEqual(false, bufferedBool.Value);
    }
    
    [Test]
    public void Test10_TimerStopsAfterCompletion()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 1f);

        bufferedBool.SetBuffered(true);

        TimerUtility.TickAllFixed(1.1f);

        Assert.AreEqual(true, bufferedBool.Value);

        TimerUtility.TickAllFixed(5f);

        Assert.AreEqual(true, bufferedBool.Value);
    }

    [Test]
    public void Test11_RandomizedStress()
    {
        DelayBuffer<bool> bufferedBool = new DelayBuffer<bool>(false, 0.5f);

        for (int i = 0; i < 100; i++)
        {
            bufferedBool.SetBuffered(Random.value > 0.5f); 
            TimerUtility.TickAllFixed(Random.Range(0f, 0.3f));
        }

        TimerUtility.TickAllFixed(1f);
    }

}
