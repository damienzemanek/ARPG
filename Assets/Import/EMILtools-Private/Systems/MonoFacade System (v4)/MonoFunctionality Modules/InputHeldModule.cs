// using System;
// using EMILtools.Core;
// using Sirenix.OdinInspector;
// using UnityEngine;
//
// public abstract class InputHeldModule<TPublisherArgs> : MonoFunctionalityModule
// {
//     public InputHeldModule(PersistentAction<TPublisherArgs, bool> action, string name = "Functionality Module", bool useIsActiveGuard = true)
//     {
//         this.action = action;
//         this.useIsActiveGuard = useIsActiveGuard;
//     }
//     
//     bool initialized;
//     bool useIsActiveGuard;
//     [NonSerialized] PersistentAction<TPublisherArgs, bool> action;
//     [ShowInInspector] protected bool isActive;
//     [ShowInInspector] protected ActionGuarderMutable executeGuarder;
//
//
//     public override void Bind() => action.Add(OnSetTemplateCall);
//
//     public override void Unbind()
//     {
//         action.Remove(OnSetTemplateCall);
//         isActive = false;
//     }
//     
//     public override void SetupModule()
//     {
//         if (initialized) return; initialized = true;
//         if (useIsActiveGuard) executeGuarder = new(new ActionGuard(() => !isActive, "Not Active"));
//         else executeGuarder = new ActionGuarderMutable();
//         Awake();
//     }
//
//     protected virtual void Awake() { }
//     public void OnSetTemplateCall(TPublisherArgs args, bool v) { isActive = v; OnSet(args); }
//     protected abstract void OnSet(TPublisherArgs args);
//
//     protected override void ExecuteTemplateCall(float dt) 
//     {
//         if (executeGuarder.TryEarlyExit()) return;
//         Execute(dt);
//     }
//     protected abstract void Execute(float dt);
// }
//
// public abstract class InputHeldModule : MonoFunctionalityModule
// {
//     public InputHeldModule(PersistentAction<bool> action, bool useIsActiveGuard = true)
//     {
//         this.action = action;
//         this.useIsActiveGuard = useIsActiveGuard;
//     }
//     
//     bool initialized;
//     bool useIsActiveGuard;
//     [NonSerialized] PersistentAction<bool> action;
//     [ShowInInspector] protected bool isActive;
//     [ShowInInspector] protected ActionGuarderMutable executeGuarder;
//
//
//     public override void Bind() => action.Add(OnSetTemplateCall);
//
//     public override void Unbind()
//     {
//         action.Remove(OnSetTemplateCall);
//         isActive = false;
//     }
//     
//     public override void SetupModule()
//     {
//         if (initialized) return; initialized = true;
//         if (useIsActiveGuard) executeGuarder = new(new ActionGuard(() => !isActive, "Not Active"));
//         else executeGuarder = new ActionGuarderMutable();
//         Awake();
//     }
//     
//     protected virtual void Awake() { }
//
//     void OnSetTemplateCall(bool v) { isActive = v; OnSet(); }
//     protected virtual void OnSet() { }
//
//     protected override void ExecuteTemplateCall(float dt) 
//     {
//         if (executeGuarder.TryEarlyExit()) return;
//         Execute(dt);
//     }
//     protected abstract void Execute(float dt);
//     
// }