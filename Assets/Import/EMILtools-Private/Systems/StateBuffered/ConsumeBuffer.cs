using System;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Timers.TimerUtility;

namespace EMILtools.Systems
{
    public class ConsumeBuffer<TPublisherArgs> : ITimerUser
    {
        TPublisherArgs cachedArgValues;
        readonly Publisher<TPublisherArgs> publisher;
        readonly Func<bool> bufferPredicate;
        readonly CountdownTimer timer;
        bool isBuffered => timer.isRunning;
    
        public ConsumeBuffer(Func<bool> bufferPredicate, Publisher<TPublisherArgs> publisher, float _bufferTime)
        {
            this.bufferPredicate = bufferPredicate;
            this.publisher = publisher;
            timer = new CountdownTimer(_bufferTime);
            timer.OnTimerTick.Add(TryConsume);
            this.InitTimer(timer, true);
        }

        public virtual void Invoke(TPublisherArgs args)
        {
            cachedArgValues = args;
            timer.StartAndReset();
        }

        protected virtual void TryConsume()
        {
            if (bufferPredicate()) Consume();
        }

        protected virtual void Consume()
        {
            if (!isBuffered) return;
            timer.Stop();
            publisher.Publish(cachedArgValues);
            cachedArgValues = default;
        }

    }
    
    
    /// <summary>
    /// Because the publisher will execute the subscriber's callback via the Resolver
    /// we need to replace the subscribers callback with our Invoke (the buffer invoke)
    ///
    /// and our Consume will house the previous original callback (from the subscriber)
    /// (This is for replacing subscribers on Publishers from Input)
    /// </summary>
    /// <typeparam name="TPublisherArgs"></typeparam>
    /// <typeparam name="TDelegate"></typeparam>
    public class ConsumeBufferSub : ITimerUser
    {
        readonly ISubEditable<Func<bool>> subscriber;
        readonly Func<bool> bufferPredicate;
        readonly CountdownTimer timer;
        bool isBuffered => timer.isRunning;
        
        readonly Func<bool> originalCallback;

        public ConsumeBufferSub(Func<bool> bufferPredicate, ISubEditable<Func<bool>> subscriber, float _bufferTime)
        {
            this.bufferPredicate = bufferPredicate;
            this.subscriber = subscriber;
            timer = new CountdownTimer(_bufferTime);
            timer.OnTimerTick.Add(TryConsume);
            this.InitTimer(timer, true);

            // GET would-be replacement predicate
            Func<bool> replacementExecution = Invoke;

            // STORE old predicate
            originalCallback = subscriber.RetrieveCallback();
            
            // REPLACE old predicate position with new predicate
            subscriber.ReplaceCallback(replacementExecution);
        }

        bool Invoke()
        {
            timer.StartAndReset();
            return false; // False is: does NOT short circuit (what I want for execution resolving)
        }
        
        void TryConsume()
        {
            if (bufferPredicate()) Consume();
        }

        void Consume()
        {
            if (!isBuffered) return;
            timer.Stop();
            subscriber.Execute();
            originalCallback.Invoke();
        }

    }

    /// <summary>
    /// Because the publisher will execute the subscriber's callback via the Resolver
    /// we need to replace the subscribers callback with our Invoke (the buffer invoke)
    ///
    /// and our Consume will house the previous original callback (from the subscriber)
    /// (This is for replacing subscribers on Publishers from Input)
    /// </summary>
    /// <typeparam name="TPublisherArgs"></typeparam>
    /// <typeparam name="TDelegate"></typeparam>
    public class ConsumeBufferSub<TContext> : ITimerUser
    {
        readonly Func<bool> enableHandle = () => true;

        TContext cachedCtx; 
        readonly ISubEditable<Func<TContext, bool>> subscriber;
        readonly Func<bool> bufferPredicate;
        [ShowInInspector] readonly CountdownTimer timer;
        [ShowInInspector] bool isBuffered => timer.isRunning;
        
        readonly Func<TContext, bool> originalCallback;

        public ConsumeBufferSub(
            Func<bool> bufferPredicate, 
            ISubEditable<Func<TContext, bool>> subscriber,
            ClassRef<float> _bufferTime, 
            Func<bool> enableHandle = null)
        {
            this.bufferPredicate = bufferPredicate;
            this.subscriber = subscriber;
            timer = new CountdownTimer(_bufferTime);
            timer.OnTimerTick.Add(TryConsume);
            this.InitTimer(timer, true);

            // GET would-be replacement predicate
            Func<TContext, bool> replacementExecution = Invoke;

            // STORE old predicate
            originalCallback = subscriber.RetrieveCallback();
            
            // REPLACE old predicate position with new predicate
            subscriber.ReplaceCallback(replacementExecution);


            if (enableHandle != null) this.enableHandle = enableHandle;
        }
        
        [Button] public void Reset() => timer.Reset();

        bool Invoke(TContext ctx)
        {
            //Debug.Log("startng input buffer timer");
            cachedCtx = ctx;
            timer.StartNoReset();
            return false; // False is: does NOT short circuit (what I want for execution resolving)
        }
        
        void TryConsume()
        {
            bool willConsume = bufferPredicate() && enableHandle();
            //Debug.Log("Will Consume? (" + willConsume + ") enabled? (" + enableHandle() + ") predicate: (" + bufferPredicate() + ")");
            if (willConsume) Consume();
        }

        void Consume()
        {
            if (!isBuffered) return;
            timer.Stop();
            subscriber.Execute();
            originalCallback.Invoke(cachedCtx);
            cachedCtx = default;
           // Debug.Log("Consumed buffered sub");
        }

    }
}
