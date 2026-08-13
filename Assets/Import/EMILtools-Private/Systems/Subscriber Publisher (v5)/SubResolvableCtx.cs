using System;
using System.Threading.Tasks;
using EMILtools.Core;


namespace EMILtools.Systems
{
    public abstract class ResolveSubscriberBase : ISubscriber
    {
        protected readonly Resolves ResolveContainer;
        
        // Mutations
        public bool canShortCircuit { get; set; }
        public bool isActive { get; set; }
        public abstract Task Execute();
        
        protected ResolveSubscriberBase( bool isActive = true, bool canShortCircuit = false)
        {
            this.canShortCircuit = canShortCircuit;
            this.isActive = isActive;
            ResolveContainer = new Resolves(true);
        }

        protected ResolveSubscriberBase( Resolves resolveContainer, bool isActive = true, bool canShortCircuit = false)
        {
            ResolveContainer = resolveContainer;
            this.canShortCircuit = canShortCircuit;
            this.isActive = isActive;

            if (ResolveContainer.afterExecution == null 
                && ResolveContainer.beforeExecution == null
                && ResolveContainer.failedExecution == null)
                ResolveContainer = new Resolves(true);
        }
    }

    public interface ISubEditable<TDelegate>
        where TDelegate : Delegate
    {
        void ReplaceCallback(TDelegate newCb);
        TDelegate RetrieveCallback();
        public Task Execute();
    }

    /// <summary>
    /// No context
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    /// <typeparam name="TResolver"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public sealed class SubResolvable : ResolveSubscriberBase, ISubscriber, ISubEditable<Func<bool>>
    {
        Func<bool> Callback;
        IPredicate Cb;

        public SubResolvable(Func<bool> callback, bool isActive = true, bool canShortCircuit = false) : base(isActive,
            canShortCircuit)
        {
            Callback = callback;
            Cb = new FuncPredicate(callback);
        }

        public SubResolvable(Func<bool> callback, Resolves resolveContainer, bool isActive = true,
            bool canShortCircuit = false) : base(resolveContainer, isActive, canShortCircuit)
        {
            Callback = callback;
            Cb = new FuncPredicate(callback);
        }
        public override Task Execute()
        {
            if (!isActive) return Task.CompletedTask;
            return Resolver<object>.ResolveContainer(ResolveContainer, Cb, canShortCircuit, null);
        }

        void ISubEditable<Func<bool>>.ReplaceCallback(Func<bool> newCb)
        {
            Callback = newCb;
            Cb = new FuncPredicate(newCb);
        }
        Func<bool> ISubEditable<Func<bool>>.RetrieveCallback() => Callback;
    }
    
    /// <summary>
    /// With Context
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    /// <typeparam name="TResolver"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public sealed class SubResolvableCtx<TContext> : 
        ResolveSubscriberBase, 
        ISubscriber<TContext>,
        ISubEditable<Func<TContext, bool>>
    {
        Func<TContext, bool> Callback;
        IPredicate Cb;

        public SubResolvableCtx(Func<TContext, bool> callback, bool isActive = true, bool canShortCircuit = false) :
            base(isActive, canShortCircuit)
        {
            Callback = callback;
            Cb = new FuncCtxPredicate<TContext>(callback);
        }

        public SubResolvableCtx(Func<TContext, bool> callback, Resolves resolveContainer, bool isActive = true,
            bool canShortCircuit = false) : base(resolveContainer, isActive, canShortCircuit)
        {
            Callback = callback;
            Cb = new FuncCtxPredicate<TContext>(callback);
        }

        public TContext cachedCtx { get; set; }

        public Task Execute(TContext ctx)
        {
            if (!isActive) return Task.CompletedTask;
            cachedCtx = ctx;
            return Execute();
        }
        
        public override Task Execute()
            => Resolver<TContext>.ResolveContainer(ResolveContainer, Cb, canShortCircuit, cachedCtx);


        void ISubEditable<Func<TContext, bool>>.ReplaceCallback(Func<TContext, bool> newCb)
        {
            Callback = newCb;
            Cb = new FuncCtxPredicate<TContext>(newCb);
        }
        Func<TContext, bool> ISubEditable<Func<TContext, bool>>.RetrieveCallback() => Callback;


    }
    
}
