using System;
using Sirenix.OdinInspector;

namespace EMILtools.Systems
{
    public abstract class UnboundFunctionality<TFacade, TViewCtx> : MonoFunctionalityModule<TFacade>, 
        IPipelineInjector<TViewCtx>,
        TickFM<TViewCtx>
        where TFacade : IFacade
        where TViewCtx : class, IContextViewImmutable
    {
        /// <summary>
        /// Execute the functionality's purpose
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public virtual void Execute(TViewCtx ctx) { }

        protected UnboundFunctionality(TFacade facade) : base(facade)
        {
            var injectablePipeline = new InjectablePipeline<TViewCtx> ( injector: this, setupPipelineInOnePass:false);
            exeSub = new SubResolvableCtx<TViewCtx>(ctx => { injectablePipeline.Execute(ctx); return false; });
        }
        public override void SetupModule() => Awake();

        // ---------------------------------- PIPELINE SYSTEM ----------------------------------
        public Pipeline<TViewCtx> InjectPipeline(PipelineBuilder<TViewCtx> builder) => throw new NotImplementedException();
        public Action<TViewCtx> InjectMainStep() => Execute;
        public virtual PipelineBuilder<TViewCtx> InjectSteps(PipelineBuilder<TViewCtx> builder) => builder;
        
        

        // ---------------------------------- PUB / SUB SYSTEM ----------------------------------

        protected readonly SubResolvableCtx<TViewCtx> exeSub;    
        public override ISubscriber Subscriber => exeSub;
        
        
        // ---------------------------------- CONSUME BUFFER SYSTEM ----------------------------------
        
        [ShowInInspector] ConsumeBufferSub<TViewCtx> consumeBuffer;  
        protected void ResetExeConsumeBuffer() => consumeBuffer?.Reset();
        protected void UseExecutionBuffer(Func<bool> bufferPredicate, ClassRef<float> bufferTime, Func<bool> enableHandle = null)
            => consumeBuffer = new ConsumeBufferSub<TViewCtx>(bufferPredicate, exeSub, bufferTime, enableHandle);
    }
}
