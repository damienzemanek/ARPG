using System;
using NUnit.Framework;
using ProArchitecture.Data;
using ProArchitecture.Logic;
using Unity.Collections;
using UnityEngine;
using ProTimers;
using UnityEngine.TestTools;

public class ProTimersUserAPITestSuite
{
    
    static unsafe class SomeTriggerLogic
    {
        public static bool _eventTriggered = false;
        public static int _triggerCount = 0;
        public static IntPtr dummyPtr = IntPtr.Zero;
        
        static void SomeLogicRun(IntPtr* ptr)
        {
            _eventTriggered = true;
            _triggerCount++;
        }
        public static Operation<IntPtr> TriggeredOperation = new(&SomeLogicRun);
    
        static void MutateDataLogicRun(IntPtr* ptr)
        {
            Debug.Log("[DEBUG_LOG] Inside MutateDataLogicRun");

            _eventTriggered = true;
            _triggerCount++;
            ref int value = ref IntPtrPtrTo<int>.GetRef(ptr);
            value += 100;
        }
        public static Operation<IntPtr> MutateDataOperation = new(&MutateDataLogicRun);
    }
    [SetUp]
    public unsafe void Setup()
    {
        TimerStack.Reset();
        TimerStackLogics.isTesting = true;
        SomeTriggerLogic._eventTriggered = false;
        SomeTriggerLogic._triggerCount = 0;
        fixed (IntPtr* p = &SomeTriggerLogic.dummyPtr)
            SomeTriggerLogic.dummyPtr = (IntPtr)p;
    }

    [Test]
    public void Test_StandardStopwatchTimer()
    {
        int myData = 1;
        ProTimer timer = new ProTimer(TickMath.Add, 2);
        timer.AddOp(0, 1, SomeTriggerLogic.TriggeredOperation)
            .AddOpWithData(0, 1, SomeTriggerLogic.MutateDataOperation, ref myData)
            .Start();
        TimerStack.TickActivesDebug(1);
        Assert.IsTrue(SomeTriggerLogic._eventTriggered);
        Assert.AreEqual(2, SomeTriggerLogic._triggerCount);
        Assert.AreEqual(101, myData);
    }
    
    [Test]
    public void Test_StandardCountdownTimer()
    {
        int myData = 1;
        ProTimer timer = new ProTimer(TickMath.Subtract, 2);
        timer.AddOp(1, 0, SomeTriggerLogic.TriggeredOperation)
            .AddOpWithData(1, 0, SomeTriggerLogic.MutateDataOperation, ref myData)
            .Start();
        TimerStack.TickActivesDebug(1);
        Assert.IsTrue(SomeTriggerLogic._eventTriggered);
        Assert.AreEqual(2, SomeTriggerLogic._triggerCount);
        Assert.AreEqual(101, myData);
    }
    
    string actionLogMsg = "Hello World!";
    void SomeMethod() => Debug.Log(actionLogMsg);

    [Test]
    public void Test_ActionOp()
    {
        ProTimer timer = new ProTimer(TickMath.Add, 1);
        timer.AddAction(0, 1, SomeMethod)
            .Start();
        TimerStack.TickActivesDebug(1);
        LogAssert.Expect(LogType.Log, actionLogMsg);
    }
    
}
