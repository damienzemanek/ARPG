using System;
using System.Collections.Generic;

namespace EMILtools.Systems
{
    /// <summary>
    /// A builder class for constructing a pipeline of steps to be executed using a defined context.
    /// SRP: Initialization
    /// </summary>
    /// <typeparam name="TContext">
    /// The type of the context associated with the pipeline.
    /// TContext must be a structure and implement the IPipelineContext interface.
    /// </typeparam>
    public class PipelineBuilder<TViewCtx>
        where TViewCtx : IContextViewImmutable
    {
        // ------ Variables ---------
        List<PipelineStep<TViewCtx>> steps = new();
        
        // ------ API Methods ---------
        public PipelineBuilder<TViewCtx> Add_ShortCircuit(
            IPredicate @if,
            IResolvable[] before = null, IResolvable[] after = null, IResolvable[] shortCircuited = null)
        {
            var NewResolves = new Resolves(true, before, after, shortCircuited);
            steps.Add(new PipelineStep<TViewCtx>(@if, NewResolves));
            return this;
        }
        public PipelineBuilder<TViewCtx> Add_Middleware(
            Action<TViewCtx> method, 
            IResolvable[] before = null, IResolvable[] after = null)
        {
            var NewResolves = new Resolves(true, before, after);
            steps.Add(new PipelineStep<TViewCtx>(method, NewResolves));
            return this;
        }
        
        
        public Pipeline<TViewCtx> InjectMainMethod(Action<TViewCtx> mainMethod) 
        {
            steps.Add(new PipelineStep<TViewCtx>(mainMethod));
            return new Pipeline<TViewCtx>(steps.ToArray());
        }
    }
}
