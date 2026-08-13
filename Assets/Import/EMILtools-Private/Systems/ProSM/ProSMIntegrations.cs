using System;
using System.Runtime.CompilerServices;
using System.Timers;
using ProArchitecture.Data;
using ProArchitecture.Logic;
using ProArchitecture.Predicates;
using ProSM;
using UnityEngine;
using Unity.Collections;

namespace ProSM
{
    
    public static unsafe class Transitioner<TData> where TData : unmanaged
    {
        public static Operation<IntPtr> TransitionOperation = new (&Transition, &AlwaysRuns);
        static bool AlwaysRuns(IntPtr* data) => true;
        static void Transition(IntPtr* data)
        {
            ref Data<Transition, ProTimersProSM_TransitionMtd>.DataWrapper transition = ref IntPtrPtrTo<Data<Transition, ProTimersProSM_TransitionMtd>.DataWrapper>.GetRef(data);
            ref ProSM<TData> fsm = ref IntPtrTo<ProSM<TData>>.GetRef(ref transition.MetaDataVolatile.fsm);
            bool didTransition = fsm.TryPollDurationTransitionsOnLayer(ref fsm.layers[transition.MetaDataVolatile.layer], out int nextState);
            ref TData fetchDataRef = ref IntPtrTo<TData>.GetRef(ref transition.MetaDataVolatile.dataFetchLocationOnComplete);
            if(didTransition) fsm.TransitionOnLayer_CallExitEnter(transition.MetaDataVolatile.layer, nextState, ref fetchDataRef);
            else Debug.LogWarning("No Transition Found when Timer transitioned");
            
            Debug.Log("TRANSITIONING");
        }
    }
    
    // public static unsafe class ProSMxProTimersIntegrationLogic
    // {
    //     
    //     public static Operation<IntPtr> RemoveSelfFromTimerStackOperation = new (&RemoveSelfFromTimerStackRun, &AlwaysRuns);
    //     static void RemoveSelfFromTimerStackRun(IntPtr* data)
    //     {
    //         ref Data<Transition, ProTimersProSM_TransitionMtd>.DataWrapper transitionWrapper = ref IntPtrPtrTo<Data<Transition, ProTimersProSM_TransitionMtd>.DataWrapper>.GetRef(data);
    //         transitionWrapper.DataVolatile.durationMet.Set(true);
    //         TimerStack.StopTimer(transitionWrapper.MetaDataVolatile.timerStackRemovalIndex);
    //     }
    //     static bool AlwaysRuns(IntPtr* data) => true;
    //
    // }
    
    public static partial class ProSMLogic
    {
        static unsafe Predicate AlwaysTruePredicate = new Predicate(&TrueCondition);
        static unsafe bool TrueCondition(void* ptr) => true;

        // public static void AddDirectTimedTransition<TStates, TData>(
        //     this ref ProSM<TData> fsm, 
        //     int layerIndex,
        //     TStates from, 
        //     TStates to,
        //     ref TData dataFetchLocationOnComplete,
        //     float duration)    
        // where TData : unmanaged
        // {
        //     // Valid Enum (Input Validation)
        //     if (typeof(TStates).GetHashCode() != fsm.layers.GetWrapper(layerIndex).MetaDataVolatile.enumTypeId)
        //         throw new ArgumentException($"Enum type '{typeof(TStates).Name}' does not match the type used to initialize layer {layerIndex}.");
        //
        //     // Valid From (Input Validation)
        //     int fromIndex = Unsafe.As<TStates, int>(ref from);
        //     if (fromIndex < 0 || fromIndex >= fsm.layers[layerIndex].states.currentSize)
        //         throw new ArgumentOutOfRangeException(nameof(from), $"State {from} (index {fromIndex}) does not exist in layer {layerIndex}.");
        //
        //     // Valid To (Input Validation)
        //     int toIndex = Unsafe.As<TStates, int>(ref to);
        //     if (toIndex < 0 || toIndex >= fsm.layers[layerIndex].states.currentSize)
        //         throw new ArgumentOutOfRangeException(nameof(to), $"State {to} (index {toIndex}) does not exist in layer {layerIndex}.");
        //
        //     unsafe
        //     {
        //          PredicateExpression AlwaysTrue = new PredicateExpression(1, AlwaysTruePredicate);
        //          var tempTransition = new Transition((short)toIndex, ref AlwaysTrue, true);
        //          ref var transitions = ref fsm.layers[layerIndex].states[fromIndex].transitions;
        //          transitions.Allocate(ref tempTransition, out int transitionIndex);
        //          transitions.GetWrapper(transitionIndex).MetaDataVolatile.dataFetchLocationOnComplete
        //              = (IntPtr)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref dataFetchLocationOnComplete);
        //
        //          ref var storedTransitionWrapper = ref fsm.layers[layerIndex].states[fromIndex].transitions.GetWrapper(transitionIndex);
        //          
        //          var events = new Data<TimerEvent, NoMtd>(1, Allocator.Temp, out _);
        //          var removeSelfFromStackEvent = TimerEvent.WithData(
        //              new TimerPredicateInfo(0, duration),
        //              ref ProTimersPredicates.IsGreaterThanOrEqualTo,
        //              ref ProSMxProTimersIntegrationLogic.RemoveSelfFromTimerStackOperation,
        //              ref Transitioner<TData>.TransitionOperation,
        //              false,
        //              (IntPtr)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref storedTransitionWrapper)
        //          );
        //          
        //          events.Allocate(ref removeSelfFromStackEvent);
        //          var timer = new ProTimer(TickMath.Add, ref events);
        //          int id = TimerStack.AddTimer(ref timer);
        //          storedTransitionWrapper.MetaDataVolatile.timerStackRemovalIndex = id;
        //         
        //          storedTransitionWrapper.MetaDataVolatile.layer = layerIndex;
        //          storedTransitionWrapper.MetaDataVolatile.fsm = (IntPtr)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref fsm);
        //         
        //          TimerStack.StartTimer(id);
        //     }
        // }
    }

}
