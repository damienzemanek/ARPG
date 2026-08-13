using System;
using EMILtools.Systems;
using Sirenix.OdinInspector;
using UnityEngine;


public interface IActionsAware
{
    public void InjectActions(IActionMap actions);
}

namespace EMILtools.Systems
{
        
    [Serializable]
    public abstract class MonoFacade<
            TFunctionality,  // Systems and functionality
            TConfig,         // Immutable Configuration
            TMonoStructure,  // References + CQRS Context
            TActionMap>      // Internal Action bindings
        
        : MonoBehaviour, IFacade

        where TConfig        : Config        
        where TMonoStructure : class, IMonoStructure, new()
        where TFunctionality : class, IFunctionality, new()
        where TActionMap     : class, IActionMap, new()
    {
        
        bool initialized = false;

        [field:HideLabel] [field: NonSerialized] public TActionMap Actions { get; protected set; }
        
        [field: BoxGroup("Config")]
        [field:SerializeField, Required] public TConfig Config { get; private set; }
        [SerializeField, HideLabel] public TMonoStructure structure;
        [field: BoxGroup("Functionality")]
        [field: ShowInInspector] [field:HideLabel] [field: NonSerialized] public TFunctionality Functionality { get; private set; }

        
        public IFSM FSM => Functionality.API_FSM();
        public T GetFunctionality<T>() where T : class, IAPI_Module
        {
            if (Functionality.APIs.TryGetValue(typeof(T), out var module)) return module as T;
            if(module == null) Debug.LogWarning("Did not find module of type " + typeof(T));
            return null;
        }
        


        protected void InitializeFacade(TConfig injectConfig = null)
        {
            if (initialized) return;
            if (injectConfig != null) Config = injectConfig;
            
            structure.Init();
            Actions = new();
            Functionality = new ();
            
            Debug.Assert(Config != null, $"{name}: Config not assigned");
            Debug.Assert(Functionality != null, $"{name}: Functionality did not initialize");

            Functionality.InjectFacadeReference(this);
            Functionality.SetupModules();  
            
            initialized = true;
        }
        

        protected virtual void Update()
        {
            if (!initialized) return;
            Functionality.UpdateTick();
        }
        
        protected virtual void FixedUpdate()
        {
            if (!initialized) return;
            Functionality.FixedTick();
        }
        
        protected virtual void LateUpdate()
        {
            if (!initialized) return;
            Functionality.LateTick();
        }
        
        
        public TMonoStructureType API_Structure<TMonoStructureType>() where TMonoStructureType : IMonoStructure
        {
            if (structure is TMonoStructureType monoStructure) return monoStructure; throw new InvalidCastException();
        }

        public TContextType API_Context<TContextType>() where TContextType : IContextViewImmutable
        {
            if (structure.API_ContextData is TContextType context) return context; throw new InvalidCastException();
        }

        public TBlackboardType API_Blackboard<TBlackboardType>() where TBlackboardType : IBlackboard
        {
            if(structure.API_Blackboard is TBlackboardType blackboard) return blackboard; throw new InvalidCastException();
        }

        public TConfigType API_Config<TConfigType>() where TConfigType : IConfig
        {
            if(Config is TConfigType config) return config; throw new InvalidCastException();
        }

        public TFunctionalityType API_Functionality<TFunctionalityType>() where TFunctionalityType : IFunctionality
        {
            if(Functionality is TFunctionalityType functionality) return functionality; throw new InvalidCastException();
        }
        
        public TActionMap1 API_Actions<TActionMap1>() where TActionMap1 : class, IActionMap
        {
            if(Actions is TActionMap1 actions) return actions; throw new InvalidCastException();
        }

        public IActionMap GetActions => Actions;
    }
}
