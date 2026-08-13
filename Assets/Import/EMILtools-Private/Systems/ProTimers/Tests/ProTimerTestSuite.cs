// using System;
// using NUnit.Framework;
// using ProArchitecture.Data;
// using ProArchitecture.Logic;
// using Unity.Collections;
// using UnityEngine;
// using ProTimers;
// using Unity.Collections.LowLevel.Unsafe;
//
// public class ProTimerTestSuite
// {
//     static unsafe class SomeTriggerLogic
//     {
//         public static bool _eventTriggered = false;
//         public static int _triggerCount = 0;
//         public static int _testDataMutated = 0;
//         public static IntPtr dummyPtr = IntPtr.Zero;
//         
//         static void SomeLogicRun(IntPtr* ptr)
//         {
//             _eventTriggered = true;
//             _triggerCount++;
//         }
//         static bool SomeLogicShouldRun(IntPtr* ptr) => true;
//         public static Operation<IntPtr> TriggeredOperation = new(&SomeLogicRun, &SomeLogicShouldRun);
//     
//         static void MutateDataLogicRun(IntPtr* ptr)
//         {
//             Debug.Log("[DEBUG_LOG] Inside MutateDataLogicRun");
//
//             _eventTriggered = true;
//             _triggerCount++;
//             ref int value = ref IntPtrPtrTo<int>.GetRef(ptr);
//             value += 100;
//         }
//         public static Operation<IntPtr> MutateDataOperation = new(&MutateDataLogicRun, &SomeLogicShouldRun);
//     }
//
//     [SetUp]
//     public unsafe void Setup()
//     {
//         TimerStack.Reset();
//         TimerStackLogics.isTesting = true;
//         SomeTriggerLogic._eventTriggered = false;
//         SomeTriggerLogic._triggerCount = 0;
//         fixed (IntPtr* p = &SomeTriggerLogic.dummyPtr)
//             SomeTriggerLogic.dummyPtr = (IntPtr)p;
//     }
//
//     [Test]
//     public void Test1_TimerInitialization()
//     {
//         // 1. Manually allocate memory for events
//         int eventCount = 1;
//         
//         var events = new Data<TimerEvent, NoMtd>(eventCount, Allocator.Persistent, out _);
//         
//         var timerEvent = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 0, triggerTime = 5.0f }, 
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//         );
//         
//         events.Allocate(ref timerEvent); //
//         
//
//         // 2. Initialize Timer with pointer
//         ProTimer timer = new ProTimer(TickMath.Add, ref events);
//
//         // 3. Assert
//         Assert.AreEqual(0, timer.events[0].info.time);
//         Assert.AreEqual(1, timer.events.currentSize);
//         Assert.IsTrue(events.DataIsActive);
//         Assert.IsTrue(timer.events.DataIsActive);
//         Assert.IsTrue(Mathf.Approximately(timer.events[0].info.triggerTime, 5.0f));
//     
//         // 4. Manual Cleanup
//         events.Dispose();
//     }
//
//     [Test]
//     public void Test2_AddAndPlayTimer()
//     {
//         Debug.Log("[DEBUG_LOG] Starting Test_AddAndPlayTimer");
//         int eventCount = 0;
//         var events = new Data<TimerEvent, NoMtd>(eventCount, Allocator.Temp, out _);
//         
//         ProTimer timer = new ProTimer(TickMath.Add, ref events);
//
//         // Test Add
//         int id = TimerStack.AddTimer(ref timer);
//         Debug.Log($"[DEBUG_LOG] Added timer id: {id}");
//         Assert.GreaterOrEqual(id, 0);
//
//         // Test Play (Verify it doesn't crash and sets active state)
//         TimerStack.StartTimer(id);
//         Debug.Log("[DEBUG_LOG] Played timer");
//         
//         events.Dispose();
//     }
//
//     [Test]
//     public void Test3_TickDeltaTimeAndTrigger()
//     {
//         Debug.Log("[DEBUG_LOG] Starting Test_TickDeltaTimeAndTrigger");
//         try {
//             // 1. Setup a timer that triggers at 1.0s
//             int eventCount = 1;
//             var events = new Data<TimerEvent, NoMtd>(eventCount, Allocator.Persistent);
//             Debug.Log("[DEBUG_LOG] Allocated events Data");
//             
//             var timerEvent = TimerEvent.NoData(
//                 new TimerPredicateInfo { time = 0, triggerTime = 1.0f }, 
//                 ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//                 ref SomeTriggerLogic.TriggeredOperation,
//                 ref TimerStackLogics.NoOp,
//                 false
//             );
//             
//             Debug.Log("[DEBUG_LOG] Created TimerEvent");
//             events.Allocate(ref timerEvent);
//             Debug.Log("[DEBUG_LOG] Allocated event in Data");
//
//             ProTimer timer = new ProTimer(TickMath.Add, ref events);
//             int id = TimerStack.AddTimer(ref timer);
//             Debug.Log($"[DEBUG_LOG] Added timer id: {id}");
//             TimerStack.StartTimer(id);
//             Debug.Log("[DEBUG_LOG] Played timer");
//
//             // 2. Simulate Tick with Delta Time (0.5s)
//             TimerStack.TickActivesDebug(0.5f);
//             Debug.Log("[DEBUG_LOG] Ticked 0.5s");
//             Assert.IsFalse(SomeTriggerLogic._eventTriggered, "Event should not trigger at 0.5s");
//
//             // 3. Simulate another Tick (0.6s) -> Total 1.1s
//             TimerStack.TickActivesDebug(0.6f);
//             Debug.Log($"[DEBUG_LOG] Ticked 0.6s. Triggered={SomeTriggerLogic._eventTriggered}");
//             Assert.IsTrue(SomeTriggerLogic._eventTriggered, "Event should trigger at 1.1s");
//
//             events.Dispose();
//             Debug.Log("[DEBUG_LOG] Disposed events");
//         } catch (Exception e) {
//             Debug.LogError($"[DEBUG_LOG] Exception in test: {e}");
//             throw;
//         }
//     }
//     
//     [TearDown]
//     public void TearDown()
//     {
//         TimerStackLogics.isTesting = false;
//         SomeTriggerLogic._triggerCount = 0;
//         SomeTriggerLogic._eventTriggered = false;
//     }
//
//     [Test]
//     public void Test4_MultiEventSequentialTriggering()
//     {
//         // Add a timer with two events: Event A at 1.0s and Event B at 2.0s
//         int eventCount = 2;
//         var events = new Data<TimerEvent, NoMtd>(eventCount, Allocator.Persistent);
//
//         var eventA = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 0, triggerTime = 1.0f }, 
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//             );
//         var eventB = TimerEvent.NoData(   
//             new TimerPredicateInfo { time = 0, triggerTime = 2.0f },
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//             );
//
//         events.Allocate(ref eventA);
//         events.Allocate(ref eventB);
//
//         ProTimer timer = new ProTimer(TickMath.Add, ref events);
//         int id = TimerStack.AddTimer(ref timer);
//         TimerStack.StartTimer(id);
//
//         // Ticking 1.1s triggers A but not B
//         TimerStack.TickActivesDebug(1.1f);
//         Assert.AreEqual(1, SomeTriggerLogic._triggerCount);
//
//         // Ticking another 1.0s (total 2.1s) triggers B
//         TimerStack.TickActivesDebug(1.0f);
//         Assert.AreEqual(2, SomeTriggerLogic._triggerCount);
//
//         events.Dispose();
//     }
//
//     [Test]
//     public void Test5_CountdownLogic()
//     {
//         // Setup 5s countdown
//         var events = new Data<TimerEvent, NoMtd>(1, Allocator.Persistent);
//         
//         var timerEvent = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 5.0f, triggerTime = 0.0f },
//             ref ProTimersPredicates.IsLessThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//         );
//         events.Allocate(ref timerEvent);
//
//         ProTimer timer = new ProTimer(TickMath.Subtract, ref events);
//         int id = TimerStack.AddTimer(ref timer);
//         TimerStack.StartTimer(id);
//
//         TimerStack.TickActivesDebug(4.0f); // Time becomes 1.0
//         Assert.IsFalse(SomeTriggerLogic._eventTriggered, "Event should not trigger yet (time=1.0)");
//
//         TimerStack.TickActivesDebug(2.0f); // Time becomes -1.0
//         Assert.IsTrue(SomeTriggerLogic._eventTriggered, "Event should trigger (time=-1.0)");
//
//         events.Dispose();
//     }
//
//     [Test]
//     public void Test6_PausingAndResuming()
//     {
//         var events = new Data<TimerEvent, NoMtd>(1, Allocator.Persistent);
//         
//         var timerEvent = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 0, triggerTime = 1.0f },
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//         );
//         events.Allocate(ref timerEvent);
//
//         ProTimer timer = new ProTimer(TickMath.Add, ref events);
//         int id = TimerStack.AddTimer(ref timer);
//         
//         TimerStack.StartTimer(id);
//         TimerStack.TickActivesDebug(0.5f);
//         Assert.IsFalse(SomeTriggerLogic._eventTriggered);
//
//         TimerStack.StopTimer(id);
//         TimerStack.TickActivesDebug(1.0f); 
//         Assert.IsFalse(SomeTriggerLogic._eventTriggered, "Timer should not have progressed while stopped");
//         
//         TimerStack.StartTimer(id);
//         TimerStack.TickActivesDebug(0.6f); // Total 1.1
//         Assert.IsTrue(SomeTriggerLogic._eventTriggered, "Timer should have resumed and triggered");
//
//         events.Dispose();
//     }
//
//     [Test]
//     public void Test7_ImmediateTrigger()
//     {
//         var events = new Data<TimerEvent, NoMtd>(1, Allocator.Persistent);
//         
//         var timerEvent = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 0, triggerTime = 0.0f },
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//         );
//         events.Allocate(ref timerEvent);
//
//         ProTimer timer = new ProTimer(TickMath.Add, ref events);
//         int id = TimerStack.AddTimer(ref timer);
//         TimerStack.StartTimer(id);
//
//         TimerStack.TickActivesDebug(0.001f);
//         Assert.IsTrue(SomeTriggerLogic._eventTriggered, "Zero duration timer should trigger immediately");
//
//         events.Dispose();
//     }
//
//     [Test]
//     public void Test8_LargeDeltaTimeOvershoot()
//     {
//         var events = new Data<TimerEvent, NoMtd>(1, Allocator.Persistent);
//         
//         var timerEvent = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 0, triggerTime = 1.0f },
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//         );
//         
//         events.Allocate(ref timerEvent);
//
//         ProTimer timer = new ProTimer(TickMath.Add, ref events);
//         int id = TimerStack.AddTimer(ref timer);
//         TimerStack.StartTimer(id);
//
//         TimerStack.TickActivesDebug(100.0f);
//         Assert.IsTrue(SomeTriggerLogic._eventTriggered, "Large delta time should trigger the event");
//
//         events.Dispose();
//     }
//
//     [Test]
//     public void Test9_DataStabilityAndIDReuse()
//     {
//         for (int i = 0; i < 10; i++)
//         {
//             var events = new Data<TimerEvent, NoMtd>(1, Allocator.Temp);
//             
//             var timerEvent = TimerEvent.NoData(
//                 new TimerPredicateInfo { time = 0, triggerTime = 0.1f },
//                 ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//                 ref SomeTriggerLogic.TriggeredOperation,
//                 ref TimerStackLogics.NoOp,
//                 false
//             );
//             events.Allocate(ref timerEvent);
//             ProTimer timer = new ProTimer(TickMath.Add, ref events);
//             int id = TimerStack.AddTimer(ref timer);
//             Assert.GreaterOrEqual(id, 0);
//             events.Dispose();
//         }
//     }
//
//     [Test]
//     public void Test10_MultipleTimersIndependentTicking()
//     {
//         var events1 = new Data<TimerEvent, NoMtd>(1, Allocator.Persistent);
//         var timerEvent1 = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 0, triggerTime = 1.0f },
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//         );
//         events1.Allocate(ref timerEvent1);
//         ProTimer timer1 = new ProTimer(TickMath.Add, ref events1);
//         int id1 = TimerStack.AddTimer(ref timer1);
//
//         var events2 = new Data<TimerEvent, NoMtd>(1, Allocator.Persistent);
//         var timerEvent2 = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 0, triggerTime = 2.0f },
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//         );
//         events2.Allocate(ref timerEvent2);
//         ProTimer timer2 = new ProTimer(TickMath.Add, ref events2);
//         int id2 = TimerStack.AddTimer(ref timer2);
//
//         TimerStack.StartTimer(id1);
//         TimerStack.StartTimer(id2);
//
//         TimerStack.TickActivesDebug(1.1f);
//         Assert.AreEqual(1, SomeTriggerLogic._triggerCount, "Only first timer should have triggered");
//
//         TimerStack.TickActivesDebug(1.0f);
//         Assert.AreEqual(2, SomeTriggerLogic._triggerCount, "Both timers should have triggered");
//
//         events1.Dispose();
//         events2.Dispose();
//     }
//
//     [Test]
//     public void Test11_TimerWithNoEvents()
//     {
//         var events = new Data<TimerEvent, NoMtd>(0, Allocator.Persistent);
//         ProTimer timer = new ProTimer(TickMath.Add, ref events);
//         int id = TimerStack.AddTimer(ref timer);
//         TimerStack.StartTimer(id);
//
//         Assert.DoesNotThrow(() => TimerStack.TickActivesDebug(1.0f), "Ticking a timer with no events should not throw");
//         
//         events.Dispose();
//     }
//
//     [Test]
//     public void Test12_MixedOneShotAndContinuousEvents()
//     {
//         var events = new Data<TimerEvent, NoMtd>(2, Allocator.Persistent);
//         
//         // One-shot
//         var oneShot = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 0, triggerTime = 1.0f },
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//         );
//         
//         // Continuous
//         var continuous = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 0, triggerTime = 1.0f },
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             true
//         );
//
//         events.Allocate(ref oneShot);
//         events.Allocate(ref continuous);
//
//         ProTimer timer = new ProTimer(TickMath.Add, ref events);
//         int id = TimerStack.AddTimer(ref timer);
//         TimerStack.StartTimer(id);
//
//         TimerStack.TickActivesDebug(1.1f);
//         Assert.AreEqual(2, SomeTriggerLogic._triggerCount, "Both events should trigger first time");
//
//         TimerStack.TickActivesDebug(1.1f);
//         Assert.AreEqual(3, SomeTriggerLogic._triggerCount, "Only continuous event should trigger second time");
//
//         events.Dispose();
//     }
//
//
//     [Test]
//     public unsafe void Test13_TriggeredDataPtrMutation()
//     {
//         var events = new Data<TimerEvent, NoMtd>(1, Allocator.Persistent);
//         int testData = 10;
//         
//         var timerEvent = TimerEvent.WithData(
//             new TimerPredicateInfo { time = 0, triggerTime = 0.5f },
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.MutateDataOperation,
//             ref TimerStackLogics.NoOp,
//             false,
//             (IntPtr)(&testData)
//         );
//         
//         Debug.Log("IntPtr is : " + timerEvent.finishedData);
//         
//         events.Allocate(ref timerEvent);
//         ProTimer timer = new ProTimer(TickMath.Add, ref events);
//         int id = TimerStack.AddTimer(ref timer);
//         TimerStack.StartTimer(id);
//
//         TimerStack.TickActivesDebug(1.0f);
//         Assert.IsTrue(SomeTriggerLogic._eventTriggered, "Callback should have been triggered");
//         Assert.AreEqual(110, testData, "Data should have been mutated by the callback (10 + 100)");
//
//         events.Dispose();
//     }
//
//     [Test]
//     public void Test14_NegativeDeltaTimeHandling()
//     {
//         var events = new Data<TimerEvent, NoMtd>(1, Allocator.Persistent);
//         var timerEvent = TimerEvent.NoData(
//             new TimerPredicateInfo { time = 0, triggerTime = 1.0f },
//             ref ProTimersPredicates.IsGreaterThanOrEqualTo,
//             ref SomeTriggerLogic.TriggeredOperation,
//             ref TimerStackLogics.NoOp,
//             false
//         );
//         events.Allocate(ref timerEvent);
//         ProTimer timer = new ProTimer(TickMath.Add, ref events);
//         int id = TimerStack.AddTimer(ref timer);
//         TimerStack.StartTimer(id);
//
//         TimerStack.TickActivesDebug(1.5f);
//         Assert.IsTrue(SomeTriggerLogic._eventTriggered);
//         SomeTriggerLogic._eventTriggered = false;
//
//         // Reset time back via negative DT
//         // We need to re-enable it manually because keepTicking=false deactivated it
//         timer.events.GetWrapper(0).Active.Set(true);
//         TimerStack.TickActivesDebug(-2.0f); 
//         
//         Assert.Less(timer.events[0].info.time, 0);
//
//         events.Dispose();
//     }
// }