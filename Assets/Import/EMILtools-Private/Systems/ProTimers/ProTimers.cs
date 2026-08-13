// using System;
// using ProArchitecture.Data;
// using ProArchitecture.Logic;
// using ProArchitecture.Predicates;
// using Unity.Collections;
// using UnityEngine;
// using static ProArchitecture.Logic.LogicBuilder<ProTimers.ProTimer>;
//
//
// namespace ProTimers
// {
//     public enum TickMath
//     {
//         Add,
//         Subtract
//     }
//     
//     public struct TimerPredicateInfo
//     {
//         public float time;
//         public float triggerTime;
//         public TimerPredicateInfo(float _time, float _triggerTime) : this()
//         {
//             time = _time;
//             triggerTime = _triggerTime;
//         }
//     }
//     
//     public static unsafe class ProTimersPredicates
//     {
//         public static Predicate IsLessThan;
//         static bool isLessThan(void* ptr) {
//             var info = (TimerPredicateInfo*)ptr;
//             return info->time < info->triggerTime;
//         }
//
//         public static Predicate IsGreaterThan;
//         static bool isGreaterThan(void* ptr)
//         {
//             var info = (TimerPredicateInfo*)ptr;
//             return info->time > info->triggerTime;
//         }
//
//         public static Predicate IsGreaterThanOrEqualTo;
//         static bool isGreaterThanOrEqualTo(void* ptr)
//         {
//             var info = (TimerPredicateInfo*)ptr;
//             return info->time >= info->triggerTime;
//         }
//
//         public static Predicate IsLessThanOrEqualTo;
//         static bool isLessThanOrEqualTo(void* ptr)
//         {
//             var info = (TimerPredicateInfo*)ptr;
//             return info->time <= info->triggerTime;
//         }
//
//         static ProTimersPredicates()
//         {
//             IsLessThan = new Predicate(&isLessThan);
//             IsGreaterThan = new Predicate(&isGreaterThan);
//             IsGreaterThanOrEqualTo = new Predicate(&isGreaterThanOrEqualTo);
//             IsLessThanOrEqualTo = new Predicate(&isLessThanOrEqualTo);
//         }
//         
//     }
//
//     /// <summary>
//     /// DataEvents live on the ProTimer, which lives in the static TimerStack
//     /// Ensure that you do not allocated Persistent for your temporary Data<TimerEvent> pass through
//     /// </summary>
//     public struct ProTimer
//     {
//         public Data<TimerEvent, NoMtd> events;  
//         public readonly TickMath math;
//         
//         public ProTimer(TickMath _math, ref Data<TimerEvent, NoMtd> _events)
//         {
//             math = _math;
//             events = _events;
//         }
//     }
//     
//     
//     
//
//     public unsafe struct TimerEvent
//     {
//         public ByteBool keepTicking;
//
//         public TimerPredicateInfo info;
//         public RefToStatic<Predicate> predicate;
//         public Operation<IntPtr>* removeSelfOperation; 
//         public ref Operation<IntPtr> OnFinishedRemoveSelf => ref *removeSelfOperation;
//         
//         public Operation<IntPtr>* onFinishedOperation; 
//         public ref Operation<IntPtr> OnFinishedOperation => ref *onFinishedOperation;   
//         public IntPtr finishedData;
//
//         public static TimerEvent NoData(TimerPredicateInfo _info, ref Predicate _predicate,
//             ref Operation<IntPtr> _removeSelfFromTimerStack, ref Operation<IntPtr> _onFinished, bool keepTickingAfterEventTriggered)
//         {
//             return new TimerEvent()
//             {
//                 info = _info,
//                 predicate = new RefToStatic<Predicate>(ref _predicate),
//                 removeSelfOperation = (Operation<IntPtr>*)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref _removeSelfFromTimerStack),
//                 onFinishedOperation = (Operation<IntPtr>*)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref _onFinished),
//                 keepTicking = new ByteBool(keepTickingAfterEventTriggered),
//                 finishedData = IntPtr.Zero
//             };
//         }
//
//         public static TimerEvent WithData(TimerPredicateInfo _info, ref Predicate _predicate,
//             ref Operation<IntPtr> _removeSelfFromTimerStack, ref Operation<IntPtr> _onFinished, bool keepTickingAfterEventTriggered, IntPtr dataPtr)
//         {
//             return new TimerEvent()
//             {
//                 info = _info,
//                 predicate = new RefToStatic<Predicate>(ref _predicate),
//                 removeSelfOperation = (Operation<IntPtr>*)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref _removeSelfFromTimerStack),
//                 onFinishedOperation = (Operation<IntPtr>*)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref _onFinished),
//                 keepTicking = new ByteBool(keepTickingAfterEventTriggered),
//                 finishedData = dataPtr
//             };
//         }
//         
//         // Want the callback to do something when it ends
//         // Want the callback to inactive itself on the TimerStack
//         public bool IsTriggered
//         {
//             get
//             {
//                 bool ret = predicate.RefStatic.Evaluate(ref info);
//                 Debug.Log("Predicate Evaluation: " + ret + " time: " + info.time + " triggerTime: " + info.triggerTime + " | time >= triggerTime: " + (info.time >= info.triggerTime) + "");
//                 return ret;
//             }
//         }
//         public void SetFinishedData(ref IntPtr data) => finishedData = data;
//     }
//     
//
//     // Register 
//     // Idles dont poll
//     // Delegate* wrapped struct timer
//     
//     public static class TimerStack
//     {
//         // Timer `Playing` will rely on active state on Data index
//         public static Data<ProTimer, NoMtd> timers;
//         
//         static TimerStack() => Reset();
//         public static void Reset()
//         {
//             if (timers.DataIsActive) timers.Dispose();
//             timers = new Data<ProTimer, NoMtd>(10000, Allocator.Persistent, out _);
//         }
//
//         public static int AddTimer(ref ProTimer _timer)
//         {
//             timers.Allocate(ref _timer, out var id);
//             StopTimer(id);
//             Debug.Log($"Timer Added: {id}");
//             return id;
//         }
//
//         public static void StartTimer(int id)
//         {
//             timers.GetWrapper(id).Active.Set(true);
//             Debug.Log($"Timer Started: {id}");
//         }
//
//         public static void StopTimer(int id)
//         {
//             timers.GetWrapper(id).Active.Set(false);
//             Debug.Log($"Timer Stopped: {id}");
//         }
//
//         public static void TickActives()
//         {
//             Batcher.Process(ref timers, TimerStackLogics.TickTimerLogic);
//         }
//         
//         public static void TickActivesDebug(float deltaTime)
//         {
//             TimerStackLogics.CurrentDeltaTime = deltaTime; 
//             Batcher.Process(ref timers, TimerStackLogics.TickTimerLogic);
//             Debug.Log($"Ticked: {deltaTime}");
//         }
//     }
//
//     // This is a nested operation call
//     // TickOperation calls the delegate inside of TimerEvent if the timer event is triggered
//     // for each `TimerEvent` in the ProTimer
//     public static unsafe class TimerStackLogics
//     {
//         public static Operation<IntPtr> NoOp = new(&NoneOperationRun);
//         static void NoneOperationRun(IntPtr* ptr) {}
//         
//         public static bool isTesting = false;
//         public static float CurrentDeltaTime; // Temporary storage for the batch process
//
//         public static Logic<ProTimer> TickTimerLogic = Static.Add(tickTimerOperation).Build();
//         public static Operation<ProTimer> tickTimerOperation = new(&TickTimerRun);
//         static void TickTimerRun(ProTimer* timer)
//         {
//             float dt = isTesting ? CurrentDeltaTime : Time.deltaTime;
//             if (timer->math == TickMath.Subtract) dt = -dt;
//
//             for (int i = 0; i < timer->events.currentSize; i++)
//             {
//                 if (!timer->events.GetWrapper(i).Active) continue;
//                 
//                 ref var timerEvent = ref timer->events[i]; 
//                 timerEvent.info.time += dt;
//                 
//                 if(!timerEvent.IsTriggered) continue;
//                 if (!timerEvent.OnFinishedRemoveSelf.ShouldRun(in timerEvent.finishedData)) continue;
//                 if (timerEvent.keepTicking == false) timer->events.GetWrapper(i).Active.Set(false);
//                 timerEvent.OnFinishedRemoveSelf.Run(ref timerEvent.finishedData);
//                 timerEvent.OnFinishedOperation.Run(ref timerEvent.finishedData);
//             }
//         }
//         
//     }
//     
//     
//     
//     
//
//     
// }
//
//
