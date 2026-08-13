// using System;
// using EMILtools.Core;
// using Sirenix.OdinInspector;
//
// public abstract class UnboundFunctionalityModule : MonoFunctionalityModule
// {
//     bool initialized;
//     bool initGuarder;
//     [ShowInInspector] protected ActionGuarderMutable executeGuarder;
//
//     public UnboundFunctionalityModule(bool initGuarder) => this.initGuarder = initGuarder;
//     
//     public override void SetupModule()
//     {
//         if (initialized) return; initialized = true;
//         if (initGuarder) executeGuarder = new ActionGuarderMutable();
//         Awake();
//     }
//     protected virtual void Awake() { }
//     public void ExecuteTemplateCall(float dt)
//     {
//         if (executeGuarder.TryEarlyExit()) return;
//         Execute();
//     }
//     public void ExecuteTemplateCall()
//     {
//         if (executeGuarder.TryEarlyExit()) return;
//         Execute();
//     }
//     public abstract void Execute();
// }
//
// public abstract class UnboundFunctionalityModuleFacade<TCoreFacade> : UnboundFunctionalityModule
//     where TCoreFacade : class, IFacade
// {
//     [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }
//
//
//     public UnboundFunctionalityModuleFacade(TCoreFacade facade, bool initGuarder) : base(initGuarder) 
//         => this.facade = facade;
//     
// }
