using System;
using EMILtools.Systems;

namespace EMILtools.Core
{
    public sealed class NotPredicate : IPredicate
    {
        private readonly IPredicate _inner;
        public NotPredicate(IPredicate inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            Resolve = () => !_inner.Resolve();
        }

        public bool Evaluate() => !_inner.Evaluate();
        public bool consumed => false;
        public void ResetWait() {
            // No op
        }

        public Func<bool> Resolve { get; private set;}
    }

    public sealed class NotPredicateCtx<TCtx> : IPredicate, IContextInjectible<TCtx>
    {
        private readonly IPredicate _inner;

        public void InjectContext(TCtx ctx)
        {
            if (_inner is IContextInjectible<TCtx> injectible) injectible.InjectContext(ctx);
        }

        public NotPredicateCtx(IPredicate inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            Resolve = () => !_inner.Resolve();
        }

        public bool Evaluate() => !_inner.Evaluate();
        public bool consumed => false;
        public void ResetWait() {
            // No op
        }
        
        public Func<bool> Resolve { get; private set;}
    }




    public sealed class FuncPredicate : IPredicate
    {
        readonly Func<bool> func;
        public bool Evaluate() => func();
        public bool Evaluate<TContext>(TContext ctx) => Evaluate();

        public FuncPredicate(Func<bool> _func)
        {
            func = _func;
            Resolve = () => func();
        }
        public bool consumed => false;
        public void ResetWait() {
           // No op
        }
        public Func<bool> Resolve { get; private set; }
        
    }

    public sealed class FuncCtxPredicate<TCtx> : IPredicate, IContextInjectible<TCtx>
    {
        readonly Func<TCtx, bool> func;
        [NonSerialized] TCtx cachedCtx;
        public void InjectContext(TCtx ctx) => cachedCtx = ctx;

        public FuncCtxPredicate(Func<TCtx, bool> _func)
        {
            func = _func;
            Resolve = () => func(cachedCtx);
        }
        public bool Evaluate() => throw new InvalidOperationException("Cannot Evaluate a Predicate without a Context");
        public bool consumed => false;
        public void ResetWait()
        {
            // No op
        }

        public Func<bool> Resolve { get; private set; }
    }

    public sealed class FuncCtxLazyPredicate<TCtxRef> : IPredicate
        where TCtxRef : class
    {
        [NonSerialized] TCtxRef ctxRef;
        readonly Func<TCtxRef, bool> func;
        public FuncCtxLazyPredicate(Func<TCtxRef, bool> _func, TCtxRef _ctxRef)
        {
            func = _func;
            ctxRef = _ctxRef;
            Resolve = () => func(ctxRef);
        }
        public void ReplaceCtxRef(TCtxRef newCtx) => ctxRef = newCtx;
        public bool Evaluate() => func(ctxRef);
        public bool consumed => false;
        public void ResetWait() {
            // No op
        }

        public Func<bool> Resolve { get; private set; }
    }


}



