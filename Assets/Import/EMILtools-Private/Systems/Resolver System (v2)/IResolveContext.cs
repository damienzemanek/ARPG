using System;
using System.Threading.Tasks;
using EMILtools.Timers;
using UnityEngine;
using static EMILtools.Timers.TimerUtility;


namespace EMILtools.Systems
{

    public abstract class Resolvable : IResolvable
    {
        protected readonly bool ShortCircuitIfNotFinished = false; 
        protected readonly bool ContinueResolving = true;
        public readonly bool executeOnce;
        public bool consumed { get; set; }
        protected Resolvable(bool executeOnce = false) => this.executeOnce = executeOnce;
        public virtual void ResetWait() => consumed = false;
        public Func<bool> Resolve { get; protected set; }
    }

    
    /// <summary>
    /// Represents a callback mechanism that can be invoked before pipeline step execution
    /// </summary>
    public class CallbackCtx<TCtx> : Resolvable, IContextInjectible<TCtx>
    {
        readonly Action<TCtx> action;
        [NonSerialized] TCtx cached;
        public void InjectContext(TCtx ctx) => cached = ctx;
        public CallbackCtx(Action<TCtx> action, bool executeOnce = false) : base(executeOnce)
        {
            this.action = action;
            Resolve = () =>
            {
                action?.Invoke(cached);
                return ContinueResolving;
            };
        }
    }
    
    
    /// <summary>
    /// Represents a callback mechanism that can be invoked before pipeline step execution
    /// </summary>
    public class Callback : Resolvable
    {
        readonly Action Action;

        public Callback(Action _action, bool executeOnce = false) : base(executeOnce)
        {
            Action = _action;
            Resolve = ResolveImplementation;
        }

        bool ResolveImplementation()
        {
            Action?.Invoke();
            return ContinueResolving;
        }
    }


    /// <summary>
    /// Short Circuits if time is not finished
    /// Re-accesses the timer to check if it is finished
    /// If its finished it will continue
    /// </summary>
    public class TimedGate : Resolvable, ITimerUser
    {
        // Is not intended to be read as ShortCircuit FALSE, used just for readability in the Resolve()
        readonly bool resetWhenAccessedAndDone;
        readonly CountdownTimer timer;

        bool open => timer.IsFinished();
        
        public TimedGate(float sec, bool resetWhenAccessedAndDone, out Action resetHandle, bool executeOnce = false) : base(executeOnce)
        {
            timer = new CountdownTimer(sec);
            this.InitTimer(timer, isFixed: true); 
            this.resetWhenAccessedAndDone = resetWhenAccessedAndDone;
            resetHandle = () => timer.StartAndReset();
            Resolve = ResolveImplementation;
        }
        
        bool ResolveImplementation()
        {
            if(!timer.isRunning && !timer.IsFinished()) timer.StartAndReset();
            var _open = open;
            if(open && resetWhenAccessedAndDone) timer.Reset();
            return _open ? ContinueResolving : ShortCircuitIfNotFinished;
        }
    }

    /// <summary>
    /// Blocks until complete
    /// </summary>
    public class Wait : Resolvable, ITimerUser, IResolveWaitable
    {
        // --- Privates ----
        readonly CountdownTimer timer;
        TaskCompletionSource<bool> blockingTcs;
        
        // --- API ----
        public bool waiting { get; set; } = false;
        public Task cachedWaitTask { get; set; }

        // --- Ctor ----
        public Wait(float sec, bool executeOnce = false) : base(executeOnce)
        {
            timer = new CountdownTimer(sec);
            this.InitTimer(timer, isFixed: true);
            blockingTcs = new();
            cachedWaitTask = blockingTcs.Task;
            timer.OnTimerStop.Add(TimerStopped);
            Resolve = ResolveImplementation;
        }
        
        void TimerStopped()
        {
            blockingTcs.TrySetResult(true);
            waiting = false;
            Debug.Log("Wait Timer Finished");
            ResetWait();
        }


        public override void ResetWait()
        {
            blockingTcs = new();
            cachedWaitTask = blockingTcs.Task;
            timer.Reset();
        }

        bool ResolveImplementation()
        {
            if (!timer.isRunning && !timer.IsFinished())
            {
                timer.StartAndReset();
                Debug.Log("Started Wait Timer");
            }
            return ContinueResolving;   
        }
        
    }
    
}



