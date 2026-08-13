
using UnityEngine;

namespace EMILtools.Systems
{
    public interface IFacade
    {
        public TMonoStructureType API_Structure<TMonoStructureType>() where TMonoStructureType : IMonoStructure;
        public TContextType API_Context<TContextType>() where TContextType : IContextViewImmutable;
        public TBlackboardType API_Blackboard<TBlackboardType>() where TBlackboardType : IBlackboard;
        public TConfigType API_Config<TConfigType>() where TConfigType : IConfig;
        public TFunctionalityType API_Functionality<TFunctionalityType>() where TFunctionalityType : IFunctionality;
        public IFSM FSM { get; }
        public Transform transform { get; }
        public T GetFunctionality<T>() where T : class, IAPI_Module;
        public TActionMap API_Actions<TActionMap>() where TActionMap : class, IActionMap;
        public IActionMap GetActions { get; }
    }

}
