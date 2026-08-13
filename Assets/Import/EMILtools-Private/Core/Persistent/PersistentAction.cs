using System;
using System.Linq;
using UnityEngine;

namespace EMILtools.Core
{
    

    public sealed class PersistentAction<T1, T2> : IPersistentAction<Action<T1,T2>, PersistentAction<T1, T2>>
    {
        public PersistentAction() { }
        public PersistentAction(Action<T1, T2> _del) => _action = _del;
        
        // No IInvokeWithEnum for multiple parameters as it wouldn't match the signature
        
        // ------------ IPersistentDelegate ------------
        public void API_Add(Delegate cb) => Add((Action<T1, T2>)cb);
        public void API_Remove(Delegate cb) => Remove((Action<T1, T2>)cb);
        
        // ----------- Concrete CRTP -----------
        public PersistentAction<T1, T2> Add(Action<T1, T2> cb) { _action += cb; return this; }
        public PersistentAction<T1, T2> Remove(Action<T1, T2> cb) { _action -= cb; return this; }
        
        // ----------- Interface Constrained Non CRTP -----------
        void IPersistentDelegate<Action<T1, T2>>.Add(Action<T1, T2> cb) => Add(cb);
        void IPersistentDelegate<Action<T1, T2>>.Remove(Action<T1, T2> cb) => Remove(cb);
        
        // ----------- Generic -----------
        public Delegate Add(Delegate cb) => _action += (Action<T1, T2>)cb;
        public Delegate Remove(Delegate cb) => _action -= (Action<T1, T2>)cb;
        
        // ----------- Core -----------
        [NonSerialized] Action<T1, T2> _action = delegate { };
        public void Invoke(T1 val1, T2 val2) => _action.Invoke(val1, val2);
        public int Count => _action.GetInvocationList().Length;
        public void PrintInvokeListNames()
        {
            var names = _action.GetInvocationList()
                .Select(d => d.Method.Name);
            Debug.Log($"PersistentAction has ({Count}) Subs, SUBS: [ {string.Join(" ], [ ", names)} ]");
        }
        
    }
    
    public sealed class PersistentAction<T> : IPersistentAction<Action<T>, PersistentAction<T>>, IInvokeWithEnum
    {
        public PersistentAction() { }
        public PersistentAction(Action<T> _del) => _action = _del;
        
        // ------------ IInvokeWithEnum ------------
        void IInvokeWithEnum.Invoke(Enum value) => Invoke((T)(object)value);

        // ------------ IPersistentDelegate ------------
        public void API_Add(Delegate cb) => Add((Action<T>)cb);
        public void API_Remove(Delegate cb) => Remove((Action<T>)cb);

        //------------  Concrete CRTP ------------ 
        public PersistentAction<T> Add(Action<T> cb) { _action += cb; return this; }
        public PersistentAction<T> Remove(Action<T> cb) { _action -= cb; return this; }
        
        //------------ Non CRTP ------------ 
        void IPersistentDelegate<Action<T>>.Add(Action<T> cb) => Add(cb);
        void IPersistentDelegate<Action<T>>.Remove(Action<T> cb) => Remove(cb);
        
        // ------------ Generic ------------ 
        public Delegate Add(Delegate cb) => _action += (Action<T>)cb;
        public Delegate Remove(Delegate cb) => _action -= (Action<T>)cb;
        
        // ------------ Core ------------ 
        [NonSerialized] Action<T> _action = delegate { };
        public void Invoke(T value) => _action?.Invoke(value);
        public int Count => _action.GetInvocationList().Length;
        public void PrintInvokeListNames()
        {
            var names = _action.GetInvocationList()
                .Select(d => d.Method.Name);
            Debug.Log($"PersistentAction has ({Count}) Subs, SUBS: [ {string.Join(" ], [ ", names)} ]");
        }
    }

    /// <summary>
    /// Non-generic version for simple triggers
    /// </summary>
    public sealed class PersistentAction : IPersistentAction<Action, PersistentAction>, IInvokeWithEnum
    {
        public PersistentAction() { }
        public PersistentAction(Action _del) => _action = _del;
        
        // ------------ IInvokeWithEnum ------------
        void IInvokeWithEnum.Invoke(Enum value) => Invoke();

        // ------------ API: IPersistentDelegate ------------
        public void API_Add(Delegate cb) => Add((Action)cb);
        public void API_Remove(Delegate cb) => Remove((Action)cb);
        
        // ------------ Generic ------------
        // Allows generic systems to store and cast the action w/out needing to know the type
        public Delegate Add(Delegate cb) => _action += (Action)cb;
        public Delegate Remove(Delegate cb) => _action -= (Action)cb;
        
        // ------------ CRTP ------------
        // Allows for returning of the PersistentAction
        // - Fluent Chaining
        public PersistentAction Add(Action cb) { _action += cb; return this; }
        public PersistentAction Remove(Action cb) { _action -= cb; return this; }
        
        // ------------ Non CRTP ------------
        // Allows generic systems to store and cast the action w/out needing to know the type
        // Purpose: External systems that need to register and don't care about the resulting delegate state
        void IPersistentDelegate<Action>.Add(Action cb) => Add(cb);
        void IPersistentDelegate<Action>.Remove(Action cb) => Remove(cb);

        // ------------ Core ------------ 
        [NonSerialized] Action _action = delegate { };
        public void Clear() => _action = delegate { };
        public void Invoke() => _action.Invoke();
        public int Count => _action.GetInvocationList().Length;
        public void PrintInvokeListNames()
        {
            var names = _action.GetInvocationList()
                .Select(d => d.Method.Name);
            Debug.Log($"PersistentAction has ({Count}) Subs, SUBS: [ {string.Join(" ], [ ", names)} ]");
        }        
        public void Add(Action[] cbs)
            { foreach (var cb in cbs) Add(cb); }
        
        public void Remove(Action[] cbs)
            { foreach (var cb in cbs) Remove(cb); }
    }
}