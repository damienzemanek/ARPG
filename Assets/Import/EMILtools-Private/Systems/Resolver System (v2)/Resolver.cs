using System;
using System.Threading.Tasks;
using EMILtools.Core;
using EMILtools.Systems;
using UnityEngine;

namespace EMILtools.Systems
{
    public static class Resolver<TContext>
    {
        static bool _debug = false;
        static void Log(string msg)
        {
            if (_debug) Debug.Log($"[ResolverSystem] {msg}");
        }
        public static async Task<bool> ResolveContainer(
            Resolves resolves,
            IResolvable resolveable,
            bool canShortCircuit,
            TContext ctx)
        {
            Log("=== ResolveContainer START ===");

            Log("(Attempting to resolve BEFORE)");
            if (!await ResolveArray(resolves.beforeExecution, canShortCircuit, ctx))
            {
                Log("beforeExecution FAILED (short-circuit)");
                return false;
            }
            Log(" + Complete!");

            Log($"(Attempting to Inject MAIN: {resolveable.GetType().Name})");
            if (resolveable is IContextInjectible<TContext> injectible)
            {
                injectible.InjectContext(ctx);
                Log($" + + Injection Successful!");
            }
            Log($" + Complete!");
            Log($"(Attempting to resolve MAIN)");
            bool mainResultContinues = resolveable.Resolve();
            Log($" + Complete! Main Result: {mainResultContinues}");

            if (!mainResultContinues && canShortCircuit)
            {
                Log("Short-circuit triggered by MAIN resolve");

                await ResolveArray(resolves.failedExecution, true, ctx);

                Log("=== ResolveContainer END (FAILED via short-circuit) ===");
                return false;
            }

            Log("(Attempting to resolve AFTER)");
            if (!await ResolveArray(resolves.afterExecution, canShortCircuit, ctx))
            {
                Log("afterExecution FAILED (short-circuit)");
                return false;
            }
            Log(" + Complete!");

            if (resolves.resetWhenAllResolved) resolves.ResetAllOnceResolves();

            Log("=== ResolveContainer END (SUCCESS) ===");
            return true;
        }
        
        static async Task<bool> ResolveArray(
            IResolvable[] resolvables,
            bool isShortCircuit,
            TContext ctx)
        {
            if (resolvables == null || resolvables.Length == 0)
            {
                Log("ResolveArray skipped (null or empty)");
                return true;
            }

            Log($"ResolveArray START (Count: {resolvables.Length}, ShortCircuit: {isShortCircuit})");

            for (int i = 0; i < resolvables.Length; i++)
            {
                var resolve = resolvables[i];

                Log($"Resolving [{i}] -> {resolve.GetType().Name}");

                if (resolve.consumed)
                {
                    Log($"Skipped (consumed) [{i}]");
                    continue;
                }
                
                if(resolve is IContextInjectible<TContext> injectible) 
                    injectible.InjectContext(ctx);
                bool result = resolve.Resolve();

                Log($"Result [{i}] -> {result}");

                if (!result && isShortCircuit)
                {
                    Log($"Short-circuit EXIT at index {i}");
                    return false;
                }

                if (resolve is IResolveWaitable waitable)
                {
                    if (!waitable.waiting)
                    {
                        Log($"Wait START [{i}] -> {resolve.GetType().Name}");

                        waitable.waiting = true;
                        await waitable.WaitUntilResolved();

                        Log($"Wait END [{i}] -> {resolve.GetType().Name}");
                    }
                    else
                    {
                        Log($"Skipped wait (already waiting) [{i}]");
                    }
                }
            }

            Log("ResolveArray END");
            return true;
        }
        
    }
    
}