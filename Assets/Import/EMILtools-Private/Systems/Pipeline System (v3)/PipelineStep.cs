using System;
using EMILtools.Core;

namespace EMILtools.Systems
{
    
    /// <summary>
    /// Represents a step in a pipeline,
    /// Defines the type of step, its execution logic, and its associated resolution contexts.
    /// SRP: Storage
    /// </summary>
    /// <typeparam name="TContext">
    /// The type of context used in the pipeline.
    /// Intent: High Throughput, so it's a CLASS
    /// </typeparam>
    public readonly struct PipelineStep<TViewCtx>
        where TViewCtx : IContextViewImmutable
    {
        public class ActionCtxResolvable : IResolvable, IContextInjectible<TViewCtx>
        {
            readonly Action<TViewCtx> action;
            [NonSerialized] TViewCtx cachedCtx;

            public void InjectContext(TViewCtx ctx)
            {
                cachedCtx = ctx;
                //Debug.Log($" + + + Injection At Class Level Detected!");
            }
            public bool consumed => false;
            public void ResetWait() {
                // No op
            }

            public Func<bool> Resolve { get; }
            
            public ActionCtxResolvable(Action<TViewCtx> _action)
            {
                action = _action;
                Resolve = () => { action?.Invoke(cachedCtx); return true; };
            }
        }
        
        // ------ Variables --------
        public readonly NotPredicateCtx<TViewCtx> Condition;
        public readonly ActionCtxResolvable CallbackSlot;
        public readonly Resolves Resolves;
        public readonly StepType StepType;

        
        // ------ Ctors ------
        public PipelineStep(IPredicate condition, 
            Resolves resolves = default)
        {
            Condition = new NotPredicateCtx<TViewCtx>(condition);
            CallbackSlot = null;
            Resolves = resolves;
            StepType = StepType.ShortCircuit;
        }
        public PipelineStep(Action<TViewCtx> mainMethod)
        {
            Condition = null;
            CallbackSlot = new ActionCtxResolvable(mainMethod);
            Resolves = new Resolves(true);
            StepType = StepType.MainMethod;
        }
        
        public PipelineStep(Action<TViewCtx> middlewareMethod, Resolves resolves)
        {
            Condition = null;
            CallbackSlot = new ActionCtxResolvable(middlewareMethod);
            Resolves = resolves;
            StepType = StepType.Middleware;
        }
        
    }


    /// <summary>
    /// Represents the type of a step within a pipeline.
    /// Defines the role and behavior of a step during pipeline execution.
    /// SRP: Classification of step roles.
    /// </summary>
    public enum StepType
    {
        /// <summary>
        /// Allows execution to continue regardless of the return state
        ///
        /// Usage: Add_Middleware(ctx => { DoSomething(ctx); return false; });
        /// </summary>
        Middleware, 
        
        /// <summary>
        /// Short-circuits execution if return state is false (Early Exit)
        /// Typically used with validation steps to ensure that the main logic is only executed when certain conditions are met.
        ///
        /// Usage: Add_ShortCircuit(ctx => isSomethingValid(ctx)); 
        /// </summary>
        ShortCircuit,

        /// <summary>
        /// Represents the primary execution step in a pipeline.
        /// This step defines the main logic to be executed within the process
        /// and is typically the final operation in the pipeline sequence.
        ///
        /// Usage: Add_MainMethod(ctx => { DoSomething(ctx); });
        /// </summary>
        MainMethod
    }

}




