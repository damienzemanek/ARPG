using System;
using EMILtools.Core;
using UnityEngine;


namespace EMILtools.Systems
{
    
    
    /// <summary>
    /// DataSetters handle what happens when values are set
    /// They solve the problem of needing to do repetitive templating and work generically
    ///
    /// Example of Repetitive Templating that this class solves:
    /// ---------------------------------
    /// void Set(T inputVal)
    /// {
    ///     storedVal = inputVal;
    ///     OnSet();
    /// }
    ///
    /// void OnSet();
    /// ---------------------------------
    /// 
    /// Ensure you set aliases for values they are generic storage only
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public abstract class DataSetter<T1> : 
        IDataSetter
    {
        // Values
        public T1 Get { get; private set; }
        public void ExplicitSet(T1 val) => Get = val;
        
        // Template Call 
        readonly SubResolvableCtx<T1> subscriber_TemplateCall;
        public ISubscriber Subscriber => subscriber_TemplateCall;
        public PersistentAction OnSetEvent { get; set; } = new();
        bool _TemplateCall(T1 val)
        {
            Get = val;
            LocalOnSet(val);
            OnSetEvent.Invoke();
            //Debug.Log(" ( ! ) DataSetter Called, Value: " + val + "");
            return false;
        }
        
        // Ctor
        protected DataSetter() => subscriber_TemplateCall = new SubResolvableCtx<T1>(_TemplateCall);
        
        // Action
        [NonSerialized] Publisher<T1> _publisher;
        public IPublisher Publisher
        {
            get => _publisher;
            set
            {
                if (value is Publisher<T1> typedValue) _publisher = typedValue;
                else throw new ArgumentException($"Value must be of type Publisher<T1> it is currently ({value.GetType()})");
            }
        }

        // Abstract
        protected virtual void LocalOnSet(T1 val) { }
        
    }
    
     
}


