// using System;
// using EMILtools.Core;
// using Sirenix.OdinInspector;
//
// /// <summary>
// /// For Logic triggered by internal events rather than direct input
// /// ex: Another Module will trigger this one
// ///
// /// No Args
// /// </summary>
// public abstract class BasicFunctionalityModule : MonoFunctionalityModule
// {
//     bool initialized;
//     bool initGuarder;
//     [NonSerialized] PersistentAction action;
//     [ShowInInspector] protected ActionGuarderMutable executeGuarder;
//
//     public BasicFunctionalityModule(PersistentAction action, bool initGuarder)
//     {
//         this.action = action;
//         this.initGuarder = initGuarder;
//     }
//     
//     public override void SetupModule()
//     {
//         if (initialized) return; initialized = true;
//         if(initGuarder) executeGuarder = new ActionGuarderMutable();
//         Awake();
//     }
//     protected virtual void Awake() { }
//     
//     public void Bind() => action.Add(ExecuteTemplateCall);
//     public void Unbind() => action.Remove(ExecuteTemplateCall);
//
//     public void ExecuteTemplateCall()
//     {
//         if (initGuarder && executeGuarder.TryEarlyExit()) return;
//         Execute();
//     }
//     
//     public abstract void Execute();
// }
//
// /// <summary>
// /// For Logic triggered by internal events rather than direct input
// /// ex: Another Module will trigger this one
// ///
// /// 1 Args
// public abstract class BasicFunctionalityModule<T> : MonoFunctionalityModule
// {
//     bool initialized;
//     bool initGuarder;
//     [NonSerialized] PersistentAction<T> action;
//     [ShowInInspector] protected ActionGuarderMutable executeGuarder;
//
//     public BasicFunctionalityModule(PersistentAction<T> action, bool initGuarder)
//     {
//         this.action = action;
//         this.initGuarder = initGuarder;
//     }
//     
//     public override void SetupModule()
//     {
//         if (initialized) return; initialized = true;
//         if(initGuarder)  executeGuarder = new ActionGuarderMutable();
//         Awake();
//     }
//     protected virtual void Awake() { }
//     
//     public override void Bind() => action.Add(ExecuteTemplateCall);
//     public override void Unbind() => action.Remove(ExecuteTemplateCall);
//     
//     public void ExecuteTemplateCall(T val)
//     {
//         if (initGuarder && executeGuarder.TryEarlyExit()) return;
//         Execute(val);
//     }
//
//     public abstract void Execute(T val);
// }
//
