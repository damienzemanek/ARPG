using System;
using EMILtools.Core;
using Sirenix.OdinInspector;

//
// /// <summary>
// /// Unguarded
// /// No Args
// /// </summary>
// public abstract class InputPressedModuleUG : MonoFunctionalityModule
// {
//     public InputPressedModuleUG(PersistentAction action)
//      => this.action = action;
//     
//     bool initialized;
//     [NonSerialized] PersistentAction action;
//     
//     
//     public override void Bind() => action.Add(OnPress);
//     public override void Unbind() => action.Remove(OnPress);
//     
//     public override void SetupModule()
//     {
//         if (initialized) return; initialized = true;
//         Awake();
//     }
//     
//     protected virtual void Awake() { }
//     protected abstract void OnPress();
//     
// }
//
// /// <summary>
// /// Guarded
// /// No Args
// /// </summary>
// public abstract class InputPressedModule : MonoFunctionalityModule
// {
//     
//     public InputPressedModule(PersistentAction action)
//     {
//         this.action = action;
//         onPressGuarder = new();
//     }
//     
//     bool initialized;
//     [NonSerialized] PersistentAction action;
//     [ShowInInspector] protected ActionGuarderMutable onPressGuarder;
//     
//     
//     public override void Bind() => action.Add(OnPressTemplateCall);
//     public override void Unbind() => action.Remove(OnPressTemplateCall);
//     
//     public override void SetupModule()
//     {
//         if (initialized) return; initialized = true;
//         Awake();
//     }
//     
//     protected virtual void Awake() { }
//     
//     void OnPressTemplateCall()
//     {
//         if (onPressGuarder.TryEarlyExit()) return;
//         OnPress();
//     }
//     protected abstract void OnPress();
// }
//
// /// <summary>
// /// Guarded
// /// 1 Args
// /// </summary>
// /// <typeparam name="T"></typeparam>
// public abstract class InputPressedModule<T> : MonoFunctionalityModule
// {
//     
//     public InputPressedModule(PersistentAction<T> action)
//     {
//         this.action = action;
//         onPressGuarder = new();
//     }
//     
//     bool initialized;
//     [NonSerialized] PersistentAction<T> action;
//     [ShowInInspector] protected ActionGuarderMutable onPressGuarder;
//     
//     
//     public override void Bind() => action.Add(OnPressTemplateCall);
//     public override void Unbind() => action.Remove(OnPressTemplateCall);
//     
//     public override void SetupModule()
//     {
//         if (initialized) return; initialized = true;
//         Awake();
//     }
//     
//     protected virtual void Awake() { }
//     
//     void OnPressTemplateCall(T val)
//     {
//         if (onPressGuarder.TryEarlyExit()) return;
//         OnPress(val);
//     }
//     protected abstract void OnPress(T val);
//     
// }
//
//
