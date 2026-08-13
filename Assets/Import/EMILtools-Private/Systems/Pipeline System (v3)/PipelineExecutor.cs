using System.Threading.Tasks;

namespace EMILtools.Systems
{
    
    /// <summary>
    /// Frame-Agnostic Async Pipeline Executor
    /// SRP: Execution
    /// </summary>
    public static class PipelineExecutor<TContext>
        where TContext : IContextViewImmutable
    {
        public static async Task<bool> Execute(Pipeline<TContext> pipeline, TContext ctx)
        {
            for (int i = 0; i < pipeline.Size; i++)
            {
                var step = pipeline[i];
                var isShortCircuit = step.StepType == StepType.ShortCircuit;
                IResolvable resolve;
                
                if (step.Condition != null) resolve = step.Condition;
                else resolve = step.CallbackSlot;
                
               // Debug.Log($"[Pipeline Executor] Step {i}: {step.StepType}, Size: {pipeline.Size}");
                if(!await Resolver<TContext>.ResolveContainer(step.Resolves, resolve, isShortCircuit, ctx)) return false;
            }
            return true;
        }
        
        

        /// <summary>
        /// Main API for executing pipelines,
        /// FLUENT API-lite, call pipelines like your "Trying to" "Do something"
        /// ---------------------------------
        /// Usage:
        /// var jump = PipelineBuilder...
        /// TryTo(jump, jumpContext);
        /// ---------------------------------
        /// SRP: API
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="ctx"></param>
        /// <typeparam name="TContext"></typeparam>
        /// <returns></returns>
        public static Task TryTo(Pipeline<TContext> pipeline, in TContext ctx)
            => Execute(pipeline, ctx);

        
        
    }
}
