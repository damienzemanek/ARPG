using System;
using System.Runtime.InteropServices;
using ProArchitecture.Data;
using ProArchitecture.Logic;
using ProArchitecture.Predicates;
using Unity.Collections;
using UnityEngine;
using EMILtools.Extensions;
using static ProArchitecture.Logic.LogicBuilder<ProTimers.ProTimer>;


namespace ProTimers
{
    public enum TickMath
    {
        Add,
        Subtract
    }
    
    public struct TriggerCtx
    {
        public float time;
        public float triggerTime;
        
        public TriggerCtx(float _time, float _triggerTime)
        {
            time = _time;
            triggerTime = _triggerTime;
        }
    }
    
    /// <summary>
    /// High Level Description:
    /// - Time condition
    /// - AND arbitrary predicate expression
    /// - THEN arbitrary operation
    /// - WITH arbitrary external data
    ///
    /// Benifits:
    /// - `finishedData` can point direclty to caller owned state, allowing generic operations w/out captruing managed objects
    ///
    /// Usage:
    /// - Start a ProTimer which can hold many TimerEvents.
    /// - ProTimer can add or subtract time, when time reaches a trigger time, it will try to call all TimerEvent operations
    /// - TimerEvent ops can have additional predicates that will be evaluated before the op is called.
    /// - When the ProTimer reaches a trigger time, any of the TimerEvent's `stopMyProTimerWhenTimeTriggeredAndMyPredicatePasses` is TRUE the ProTimer will stop ticking and remove itself from the TimerStack
    /// - When the ProTimer reaches a trigger time, any of the TimerEvent's `stopThisTimerEventWhenTimerFinishes` is TRUE the TimerEvent will be removed from the ProTimer's actively called methods
    ///
    /// Nuance:
    /// - Depending on how you set up your timer, you can have a ProTimer keep ticking and stagger TimerEvents in interesting ways
    ///
    /// Examples:
    /// - ProTimer: (Add) (Time: 0s) (TriggerTime: 5s) 
    ///     + TimerEvent1: Car explodes at TriggerTime (Stops Ticking When Fin)
    ///
    /// - ProTimer: (Add) (Time: 0s) (TriggerTime: 5s) 
    ///     + TimerEvent1: Car explodes at TriggerTime (Stops TimerEvent1)
    ///     + TimerEvent2: Engine catches fire at 10s (Stops the ProTimer)
    ///
    /// - ProTimer: (Add) (Time: 0s) (TriggerTime: 5s) 
    ///     + TimerEvent1: Car explodes at TriggerTime (Stops TimerEvent1)
    ///     + TimerEvent2: Engine catches fire at 10s (Stops the TimerEvent2)
    ///     + TimerEvent3: Fire Spreads by a small amount at 20s (Keeps Ticking) (note: i think this is relative to dt)
    ///     + TimerEvent4: Car Explodes again at 30s (Stops the ProTimer)
    /// 
    /// </summary>
    public struct ProTimer
    {
        public int myTimerStackIndex;
        public Data<TimerEvent, NoMtd> events;  
        public readonly TickMath math;
        
        public ProTimer(TickMath _math, ref Data<TimerEvent, NoMtd> _events)
        {
            math = _math;
            events = _events;
        }
        
        public ProTimer(TickMath _math, int eventsSize)
        {
            math = _math;
            events = new Data<TimerEvent, NoMtd>(eventsSize, Allocator.Persistent, out _);
        }
    }

    public static class ProTimerExtensions
    {
        public static ref ProTimer AddOp(this ref ProTimer timer,
            float startTime,
            float triggerTime,
            RefToStatic<Operation<IntPtr>> _onFinished,
            bool onEventStopTimer = true,
            bool onEventStopEvent = true,
            PredicateExpression? _additionalEventPredicates = null)
        {
             TimerEvent newTimerEvent = TimerEvent.NoData(
                 new TriggerCtx(startTime, triggerTime), 
                 _onFinished,
                 onEventStopTimer, 
                 onEventStopEvent,
                _additionalEventPredicates);
             timer.events.Allocate(ref newTimerEvent, out var id);
             return ref timer;
        }
        
        public static ref ProTimer AddOpWithData<TCtx>(this ref ProTimer timer,
            float startTime, 
            float triggerTime,
            RefToStatic<Operation<IntPtr>> _onFinished,
            ref TCtx data,
            bool onEventStopTimer = true,
            bool onEventStopEvent = true,
            PredicateExpression? _additionalEventPredicates = null)
        {
            TimerEvent newTimerEvent = TimerEvent.WithData(
                new TriggerCtx(startTime, triggerTime), 
                _onFinished: _onFinished,
                IntPtrEX.AsIntPtr(ref data),
                onEventStopTimer, 
                onEventStopEvent,
                _additionalEventPredicates);
            timer.events.Allocate(ref newTimerEvent, out var id);
            return ref timer;
        }
        
        public static ref ProTimer AddAction(this ref ProTimer timer,
            float startTime, 
            float triggerTime,
            Action action,
            bool onEventStopTimer = true,
            PredicateExpression? _additionalEventPredicates = null)
        {
            action.Blit(out var handle);
            RefToStatic<Operation<IntPtr>> _onFinished = OpExtensions.Action;
            TimerEvent newTimerEvent = TimerEvent.WithData(
                new TriggerCtx(startTime, triggerTime), 
                _onFinished: _onFinished,
                handle,
                onEventStopTimer, 
                stopThisTimerEventWhenTimerFinishes: true,
                _additionalEventPredicates);
            timer.events.Allocate(ref newTimerEvent, out var id);
            return ref timer;
        }
        
        public static void Start(this ref ProTimer timer) => TimerStack.StartTimer(TimerStack.AddTimer(ref timer));
        public static void Resume(this ref ProTimer timer) => TimerStack.StartTimer(timer.myTimerStackIndex);
        public static void Stop(this ref ProTimer timer) => TimerStack.StopTimer(timer.myTimerStackIndex);
    }
    public static unsafe class OpExtensions
    {
        public static void ActionExecuter(IntPtr* actionPtr)
        {
            var intPtr = (IntPtr)actionPtr;
            var typed = IntPtrEX.To<BlittableReference<Action>>(intPtr);
            Debug.Log(typed.Target);
            typed.Target.Invoke();
            typed.Free();
        }
        
        public static Operation<IntPtr> Action = new(&ActionExecuter);
    }

    
    public struct TimerEvent
    {
        public ByteBool onEventStopTimer;
        public ByteBool onEventStopEvent;

        public TriggerCtx Ctx;
        public PredicateExpression additionalEventPredicates;
        public RefToStatic<Operation<IntPtr>> onFinishedOp; 
        public IntPtr finishedData;

        public static TimerEvent NoData(
            TriggerCtx ctx,
            RefToStatic<Operation<IntPtr>> _onFinished,
            bool onEventStopTimer = true,
            bool onEventStopEvent = true,
            PredicateExpression? _additionalEventPredicates = null)
        {
            return new TimerEvent()
            {
                Ctx = ctx,
                additionalEventPredicates = _additionalEventPredicates != null 
                    ? (PredicateExpression)_additionalEventPredicates 
                    : new PredicateExpression(),
                onFinishedOp = _onFinished,
                onEventStopEvent = onEventStopEvent,
                onEventStopTimer = onEventStopTimer,
                finishedData = IntPtr.Zero
            };
        }

        public static TimerEvent WithData(
            TriggerCtx ctx, 
            RefToStatic<Operation<IntPtr>> _onFinished, 
            IntPtr dataPtr,
            bool stopMyProTimerWhenTimeTriggeredAndMyPredicatePasses = true,
            bool stopThisTimerEventWhenTimerFinishes = true,
            PredicateExpression? _additionalEventPredicates = null)
        {
            return new TimerEvent()
            {
                Ctx = ctx,
                additionalEventPredicates = _additionalEventPredicates != null 
                    ? (PredicateExpression)_additionalEventPredicates 
                    : new PredicateExpression(),               
                onFinishedOp = _onFinished,
                onEventStopEvent = stopThisTimerEventWhenTimerFinishes,
                onEventStopTimer = stopMyProTimerWhenTimeTriggeredAndMyPredicatePasses,
                finishedData = dataPtr
            };
        }
        
        public bool additionalPredicatePasses
        {
            get
            {
                bool ret = additionalEventPredicates.EvaluateIdempotent(ref Ctx);
                Debug.Log("Predicate Evaluation: " + ret + " time: " + Ctx.time + " triggerTime: " + Ctx.triggerTime + " | time >= triggerTime: " + (Ctx.time >= Ctx.triggerTime) + "");
                return ret;
            }
        }
    }
    
    
    
    public static class TimerStack
    {
        // Timer `Playing` will rely on active state on Data index
        public static Data<ProTimer, NoMtd> timers;
        
        static TimerStack() => Reset();
        public static void Reset()
        {
            if (timers.DataIsActive) timers.Dispose();
            timers = new Data<ProTimer, NoMtd>(10000, Allocator.Persistent, out _);
        }

        public static int AddTimer(ref ProTimer _timer)
        {
            timers.Allocate(ref _timer, out var id);
            _timer.myTimerStackIndex = id;
            timers[id].myTimerStackIndex = id;
            StopTimer(id);
            Debug.Log($"Timer Added: {id}");
            return id;
        }
        
        public static void StartTimer(int id)
        {
            timers.GetWrapper(id).Active.Set(true);
            Debug.Log($"Timer Started: {id}");
        }
        
        public static void StopTimer(int id)
        {
            timers.GetWrapper(id).Active.Set(false);
            Debug.Log($"Timer Stopped: {id}");
        }

        public static void TickActives()
        {
            Batcher.Process(ref timers, TimerStackLogics.TickTimerLogic);
        }
        
        public static void TickActivesDebug(float deltaTime)
        {
            TimerStackLogics.CurrentDeltaTime = deltaTime; 
            Batcher.Process(ref timers, TimerStackLogics.TickTimerLogic);
            Debug.Log($"Ticked: {deltaTime}");
        }
    }
    
    
    // This is a nested operation call
    // TickOperation calls the delegate inside of TimerEvent if the timer event is triggered
    // for each `TimerEvent` in the ProTimer
    public static unsafe class TimerStackLogics
    {
        
        public static bool isTesting = false;
        public static float CurrentDeltaTime; // Temporary storage for the batch process

        public static Operation<ProTimer> tickTimerOperation = new(&TickTimerRun);       
        public static Logic<ProTimer> TickTimerLogic = Static.Add(tickTimerOperation).Build();
        
        static void TickTimerRun(ProTimer* timer)
        {
            float dt = isTesting ? CurrentDeltaTime : Time.deltaTime;
            bool timerStopping = false;
            if (timer->math == TickMath.Subtract) dt = -dt;

            for (int i = 0; i < timer->events.currentSize; i++)
            {
                if (!timer->events.GetWrapper(i).Active) continue;
                
                ref var timerEvent = ref timer->events[i]; 
                timerEvent.Ctx.time += dt;
                
                var timeTriggered = timer->math == TickMath.Add 
                    ? timerEvent.Ctx.time >= timerEvent.Ctx.triggerTime
                    : timerEvent.Ctx.time <= timerEvent.Ctx.triggerTime;
                
                if (!timeTriggered) continue;
                if (!timerEvent.additionalPredicatePasses) continue;
                
                if (timerEvent.onFinishedOp.AsRefStatic.ShouldRun(in timerEvent.finishedData)) 
                    timerEvent.onFinishedOp.AsRefStatic.Run(ref timerEvent.finishedData);
                
                if (timerEvent.onEventStopTimer)
                    timerStopping = true;
                
                if (timerEvent.onEventStopEvent) 
                    timer->events.GetWrapper(i).Active.Set(false);
            }
            
            if (timerStopping) TimerStack.StopTimer(timer->myTimerStackIndex);
        }
    }
    
    
    
    

    
}


