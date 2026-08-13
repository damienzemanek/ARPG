using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using static InterfaceEX;


namespace EMILtools.Systems
{
    
    public interface IFunctionality
    {
        public Dictionary<Type, IMonoFunctionalityModule> APIs { get; }
        public void InjectFacadeReference(IFacade f);
        public void SetupModules();
        public void UpdateTick();
        public void FixedTick();
        public void LateTick();
        public IFSM API_FSM();
    }
    
    public abstract class Functionalities<TMonoFacade, TViewCtx> : IFunctionality
        where TMonoFacade : class, IFacade
        where TViewCtx : class, IContextViewImmutable
    {
        [field: NonSerialized] protected TMonoFacade facade { get; private set; }
        [ShowInInspector, HideLabel, InlineProperty] StateMachine<TViewCtx> stateMachine;

        
        /// <summary>
        /// Later: Dictionary of Type and List of Modules for multi-service subscription
        /// </summary>
        readonly Dictionary<Type, IMonoFunctionalityModule> API_Modules = new();
        public Dictionary<Type, IMonoFunctionalityModule> APIs => API_Modules;
        [ShowInInspector] readonly List<IMonoFunctionalityModule> modules = new();

        readonly Publisher<TViewCtx> updateTick = new();
        readonly Publisher<TViewCtx> fixedTick = new();
        readonly Publisher<TViewCtx> lateTick = new();


        bool usingFSM = false;
        public IFSM API_FSM() => stateMachine;
        List<FSM_AVALIABLE> states;

        StateMachine<TViewCtx> InitFSM(IState initialState)
        {
            if(states == null) throw new InvalidOperationException("No states were added to the FSM");
           var fsm = stateMachine = new StateMachine<TViewCtx>(facade.API_Context<TViewCtx>(), initialState);
           foreach (var state in states.Where(state => state != initialState))
               fsm.AddNode(state);
           return fsm;
        }
        
        
        public void InjectFacadeReference(IFacade f)
        {
            if(f is TMonoFacade typedFacade) facade = typedFacade;
            else throw new InvalidCastException($"Facade type mismatch. Expected {typeof(TMonoFacade)}, got {f.GetType()}");
        }
        

        public void SetupModules()
        {
            var initialState = AddModulesHere(facade);
            if(initialState == null) Debug.LogWarning("(!) You are using a stateless MonoFacade");
            else
            {
                var fsm = InitFSM(initialState);
                SetupTransitionsForFSM(fsm, facade.API_Context<TViewCtx>());
            }
            
            foreach (var t in modules)  t.SetupModule();
            Debug.Log($"{GetType().Name} Functionality modules successfully setup | API Count: " + API_Modules.Count);
        }
        
        public void Bind()
        {
            Debug.Log("binding in progress...");
            foreach (var t in modules)
                if(t is IBindable bindable) bindable.Bind();
        }

        public void Unbind()
        {
            foreach (var t in modules)
                if(t is IBindable bindable) bindable.Unbind();
        }

        public void UpdateTick()
        {
            updateTick.Publish(facade.API_Context<TViewCtx>()).Forget("UpdateTick");
            if(usingFSM) stateMachine.PollTransitionsAsync().Forget("FSM Poll");
        }
        public void FixedTick() => fixedTick.Publish(facade.API_Context<TViewCtx>()).Forget("FixedTick");
        public void LateTick() => lateTick.Publish(facade.API_Context<TViewCtx>()).Forget("LateTick");

        protected IState AddModule(MonoFunctionalityModule<TMonoFacade> module)
        {
            modules.Add(module);
            Debug.Log("ADDING module " + module.GetType().Name + " new count is " + modules.Count);
            if (module.GetType() != typeof(BoundFunctionality<TMonoFacade,TViewCtx>) && module is not IEntryPointFM) Debug.LogError("Module is not an IEntryPointFM, Please assign an EntryPoint");
            if (typeof(UPDATE<TViewCtx>).IsAssignableFrom(module.GetType())) updateTick.Add(module.Subscriber);
            if (typeof(FIXED_UPDATE<TViewCtx>).IsAssignableFrom(module.GetType())) fixedTick.Add(module.Subscriber);
            if (typeof(LATE_UPDATE<TViewCtx>).IsAssignableFrom(module.GetType())) lateTick.Add(module.Subscriber);
            if (typeof(IAPI_Module).IsAssignableFrom(module.GetType()))
            {
                foreach (var iface in GetInterfacesAssignableTo<IAPI_Module>(module.GetType()))
                {
                    if (!API_Modules.TryAdd(iface, module)) throw new InvalidOperationException($"API interface {iface.Name} already registered.");
                    Debug.Log("Added interface to modules API dictionary, new API add is : " + iface + " new count is " + API_Modules.Count);
                }                
            }

            if (module is FSM_AVALIABLE stateModule)
            {
                if(states == null) states = new();
                usingFSM = true;
                states.Add(stateModule);
            }
            
            return module;
        }
        
        
        protected abstract IState AddModulesHere(TMonoFacade f);
        protected abstract void SetupTransitionsForFSM(StateMachine<TViewCtx> fsm, TViewCtx ctx);

    }
}