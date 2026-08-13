using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Systems
{
    public interface IMonoStructure
    {
        public void Init();
        public IBlackboard API_Blackboard { get; }
        public IContextViewImmutable API_ContextData { get; }
    }

    [HideLabel]
    [Serializable]
    public abstract class MonoStructure<TBlackboard, TContextData, TContextViewImmutable> : IMonoStructure
        where TBlackboard : class, IBlackboard
        where TContextData : ContextData, TContextViewImmutable, new()
        where TContextViewImmutable : IContextViewImmutable
    {
        [BoxGroup("Blackboard")] [SerializeField] [HideLabel]
        public TBlackboard Blackboard;
    
        [field: BoxGroup("Context")] [field: NonSerialized] [field: HideLabel] [field: ShowInInspector]
        public ContextProvider<TContextData, TContextViewImmutable> ContextProvider { get; internal set; }


        /// <summary>
        /// Default initialization
        /// </summary>
        public void Init()
        {
            if(Blackboard == null) Debug.LogError($"Blackboard not assigned in {GetType().Name}");
            ContextProvider = new ContextProvider<TContextData, TContextViewImmutable>(Blackboard);
        }
        
        public IBlackboard API_Blackboard => Blackboard;
        public IContextViewImmutable API_ContextData => ContextProvider.View;
    }
}


