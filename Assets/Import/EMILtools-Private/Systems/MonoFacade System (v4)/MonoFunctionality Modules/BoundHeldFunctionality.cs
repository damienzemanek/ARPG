using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Systems
{
    public interface IBindable
    {
        public void Bind();
        public void Unbind();
    }
    
    
    
    /// <summary>
    /// Binds a functionality to a Publisher
    /// - Only to be used with PersistentAction (No Args)
    /// - Use TContext to pass around information
    /// </summary>
    /// <typeparam name="TFacade"></typeparam>
    /// <typeparam name="TViewCtx"></typeparam>
    public abstract class BoundFunctionality<TFacade, TViewCtx> : 
            UnboundFunctionality<TFacade, TViewCtx>, 
            IBindable
        where TFacade : IFacade
        where TViewCtx : class, IContextViewImmutable
    {
        [NonSerialized] readonly Publisher<TViewCtx> publisher; // Lazy (Injected)

        protected BoundFunctionality(Publisher<TViewCtx> publisher, TFacade facade) : base(facade)
        {
            this.publisher = publisher;
            Debug.Log($"Bound {typeof(TFacade).Name} Established Publisher : {publisher.GetType().Name} for functionality");
        }

        /// <summary>
        /// Binds the EXECUTION PIPELINE to the BOUND ACTION
        /// </summary>
        public virtual void Bind()
        {
            publisher.Add(exeSub);
            Debug.Log($"Bound {typeof(TFacade).Name} functionality to publisher");
        }
        public virtual void Unbind() => publisher.Remove(exeSub);
    }



    /// <summary>d
    /// Binds a functionality to a Publisher
    /// - Pipeline Injection ONLY FOR TICKFM EXECUTION, does not work with the setter
    /// - Set Args with SettableTemplate
    /// - Tracks: isActive
    /// </summary>
    /// <typeparam name="TFacade"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="DataSetter"></typeparam>
    public abstract class BoundSetFunctionality<
            TFacade,
            TViewCtx, 
            DataSetter> :
        UnboundFunctionality<TFacade, TViewCtx>,
        IBindable
        where TFacade : IFacade
        where DataSetter : class, IDataSetter, new()
        where TViewCtx : class, IContextViewImmutable
    {
        protected DataSetter SetContext => Settable;
        [NonSerialized] [ShowInInspector] readonly DataSetter Settable;
        protected BoundSetFunctionality(IPublisher publisher, TFacade facade) : base(facade)
        {
            Settable = new DataSetter();
            Settable.Publisher = publisher;
            //Debug.Log("Succesfully cached publisher : " + publisher.GetType().Name + " which is " + publisher.GetType());
            // Debug.Log($"Settable action is : " + Settable.Publisher + $" and template call is : " + Settable.Subscriber + $" for functionality : " + GetType().Name);
        }

        public void Bind()
        {
            Debug.Log("Trying to Bind (" + GetType().Name + ")");
            Settable.Publisher.Add(Settable.Subscriber);
            if(this is ON_SET onSet) Settable.OnSetEvent.Add(onSet.MutateUsingNewSetValues);
            Debug.Log("Bound(" + GetType().Name + ")");
        }

        public void Unbind()
        {
            Settable.Publisher.Remove(Settable.Subscriber);
            if(this is ON_SET onSet) Settable.OnSetEvent.Remove(onSet.MutateUsingNewSetValues);
        }

    }


}