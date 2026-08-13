using System;

namespace EMILtools.Systems
{
    public interface IPipelineInjector<TViewCtx>
        where TViewCtx : IContextViewImmutable
    {
        public Pipeline<TViewCtx> InjectPipeline(PipelineBuilder<TViewCtx> builder);

        public PipelineBuilder<TViewCtx> InjectSteps(PipelineBuilder<TViewCtx> builder);
        public Action<TViewCtx> InjectMainStep();
    }
}