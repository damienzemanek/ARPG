using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMILtools.Core;

namespace EMILtools.Systems
{
    public interface IPublisher : IDelegatorAbstract<ISubscriber>
    {
        public IReadOnlyList<ISubscriber> API_Snapshot { get; }
        public Task PublishImplementation(ISubscriber sub);
        public abstract void Snapshot();
        public async Task PublishCore()
        {
            Snapshot();
            for (int i = 0; i < API_Snapshot.Count; i++)
            {
                var sub = API_Snapshot[i];
                if (!sub.isActive) continue;
                await PublishImplementation(sub);
            }
        }
    }
    

    public class Publisher :
        IPublisher
    {
        // ISubscriber directly inherits from ISubscriber<TContext> and ISubscriber<T1, T2>

        [NonSerialized] readonly List<ISubscriber> subs = new();
        [NonSerialized] readonly List<ISubscriber> snapshot = new();
        public IReadOnlyList<ISubscriber> API_Snapshot => snapshot;
        // ---------- Typed Generic --------------
        public ISubscriber Add(ISubscriber cb) { subs.Add(cb); return cb; }
        public ISubscriber Remove(ISubscriber cb) { subs.Remove(cb); return cb; }
        public Task Publish() => ((IPublisher)this).PublishCore();
        public Task PublishImplementation(ISubscriber sub) => sub.Execute();
        
        // API
        public void Snapshot()
        {
            snapshot.Clear();
            snapshot.AddRange(subs);
        }
    }

    

    public class Publisher<TContext> :
        IDelegatorAbstract<ISubscriber<TContext>>,
        IPublisher
    {
        // ISubscriber directly inherits from ISubscriber<TContext> and ISubscriber<T1, T2>
        [NonSerialized] readonly List<ISubscriber<TContext>> subs = new();
        [NonSerialized] readonly List<ISubscriber> snapshot = new();
        public IReadOnlyList<ISubscriber> API_Snapshot => snapshot;

        [NonSerialized] TContext cachedContext;

        // ---------- Typed Generic --------------
        public ISubscriber<TContext> Add(ISubscriber<TContext> cb) { subs.Add(cb); return cb; }
        public ISubscriber<TContext> Remove(ISubscriber<TContext> cb) { subs.Remove(cb); return cb; }

        public ISubscriber Add(ISubscriber cb)
        {
            if(cb is ISubscriber<TContext> typedCB) return ((IDelegatorAbstract<ISubscriber<TContext>>)this).Add(typedCB);
            throw SubscriberTypeError(cb, "added");
        }

        public ISubscriber Remove(ISubscriber cb)
        {
            if (cb is ISubscriber<TContext> typed)
                return ((IDelegatorAbstract<ISubscriber<TContext>>)this).Remove(typed);

            throw SubscriberTypeError(cb, "removed");
        }

        public Task PublishImplementation(ISubscriber sub)
        {
            if (sub is ISubscriber<TContext> subTyped)
                return subTyped.Execute(cachedContext);

            throw SubscriberTypeError(sub, "published");
        }
        
        private Exception SubscriberTypeError(object obj, string operation)
        {
            var givenType = FormatType(obj.GetType());
            var expectedType = FormatType(typeof(ISubscriber<TContext>));
            var publisher = FormatType(GetType());

            throw new Exception(
                $"Subscriber type '{givenType}' cannot be {operation}.\n" +
                $"Expected type: '{expectedType}'.\n" +
                $"Publisher: {publisher}."
            );
            
                    
            static string FormatType(Type type)
            {
                if (!type.IsGenericType)
                    return type.Name;

                var name = type.Name[..type.Name.IndexOf('`')];
                var args = string.Join(", ", type.GetGenericArguments().Select(FormatType));
                return $"{name}<{args}>";
            }
        }
        

        public Task Publish(TContext ctx)
        {
            cachedContext = ctx;
            return ((IPublisher)this).PublishCore();
        }
        
        public void Snapshot()
        {
            snapshot.Clear();
            snapshot.AddRange(subs);
        }
    }
    
    
}
