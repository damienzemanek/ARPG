using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Systems
{
    /// <summary>
    /// Override this to add data to the context
    /// </summary>
    /// <typeparam name="TBlackboard"></typeparam>
    public abstract class ContextData : IContextViewImmutable
    {
        internal IBlackboard Blackboard;
        
        public TBlackboard API_Blackboard<TBlackboard>()
            where TBlackboard : IBlackboard
        => (TBlackboard)Blackboard;
        protected ContextData() { }
    }

    /// <summary>
    /// CQRS (Command Query Responsibility Segregation) principle applied at the type level
    /// + Writes go through concrete classes
    /// + Reads go through interfaces
    ///
    /// Intended Design: High Throughput (For Pipeline)
    /// </summary>
    /// <typeparam name="TContextData"></typeparam>
    /// <typeparam name="TContextViewImmutable"></typeparam>
    [HideLabel]
    [InlineProperty]
    public class ContextProvider<TContextData, TContextViewImmutable>
        where TContextData : ContextData, TContextViewImmutable, IContextViewImmutable, new()
        where TContextViewImmutable : IContextViewImmutable
    {
        
        /// <summary>
        /// Data is mutated where it's visible (public is OK)
        /// </summary>
        [ShowInInspector, HideLabel] public TContextData Data;

        /// <summary>
        /// This will be passed around to scripts that depend on it
        /// </summary>
        [HideInInspector] public readonly TContextViewImmutable View;

        public ContextProvider(IBlackboard blackboard)
        {
            Data = new TContextData() { Blackboard = blackboard };
            View = Data;
        }
    }
}





