using System;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Timers.CurveValue;

namespace EMILtools.Timers
{
    public interface ICurveValue
    {
        public void Start(Operation _operation);
        public void DynamicStart(Operation _operation);
        public void DynamicStartAndPause(Operation _operation, float pauseProgress);
        public void StartAndPauseAt(Operation _operation, float pauseProgress);
        public float Value { get; set; }
        public float Evaluate { get; }
    }
    
    [Serializable]
    public class CurveValue : Timer, ICurveValue
    {
        private const float NO_PAUSE = -1f;
        public enum Operation { Increase, Decrease }
        
        public float Value
        {
            get => Time;
            set => Time = value;
        }
        
        [FoldoutGroup("Curve")] [ShowInInspector, ReadOnly] public virtual float Evaluate => curve?.Evaluate(Progress) ?? 0;
        [FoldoutGroup("Curve")] [ReadOnly] public Operation operation;
        [FoldoutGroup("Curve")] public AnimationCurve curve;
        [FoldoutGroup("Curve")] [SerializeField] [Range(0.01f, 5)] public float operationScalar = 1f;
        [HideInInspector] [ReadOnly] public float pauseProgress = NO_PAUSE;

        public CurveValue() : base(new ClassRef<float>(1f))
        {
            operationScalar = 1f;
            curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        }
        
        public CurveValue(float _initialTime, AnimationCurve curve) : base(_initialTime)
            =>  this.curve = curve;
        public CurveValue(ClassRef<float> _initialTime, AnimationCurve curve) : base(_initialTime) 
            =>  this.curve = curve;
        public CurveValue(AnimationCurve curve, float _initialTime, Action[] OnTimerStartCbs = null, Action[] OnTimerTickCbs = null, Action[] OnTimerStopCbs = null) : base(_initialTime, OnTimerStartCbs, OnTimerTickCbs, OnTimerStopCbs)
            => this.curve = curve;

        
        public override void TickImplementation(float deltaTime)
        {
            if (operation == Operation.Increase)
            {
                if (Time < initialTime)
                {
                    Time += deltaTime * operationScalar;
                    if (pauseProgress != NO_PAUSE && Progress > pauseProgress) Pause();
                }
                else Stop();
            }
            else if(operation == Operation.Decrease)
            {
                if (Time > 0)
                {
                    Time -= deltaTime * operationScalar;
                    if(pauseProgress != NO_PAUSE && Progress < pauseProgress) Pause();
                }
                else Stop();
            }
        }

        public override void InitializeTime() => Time = operation == Operation.Increase ? 0f : initialTime;

        public override void StartAndReset() =>
            throw new SystemException("Start() is not intended to be used in CurveValue, uss Start(Operation)");

        public void Start(Operation _operation)
        {
            operation = _operation;
            pauseProgress = NO_PAUSE;
            base.StartAndReset();
        }

        /// <summary>
        /// Start the curve either while its already active, or if its paused and you want to move it around
        /// </summary>
        /// <param name="_operation"></param>
        public void DynamicStart(Operation _operation)
        {
            if (initialTime == null) initialTime = new ClassRef<float>(1f);
            operation = _operation;
            pauseProgress = NO_PAUSE;
            StartNoReset();
        }
        
        /// <summary>
        /// Start the curve either while its already active, or if its paused and you want to move it around
        /// </summary>
        /// <param name="_operation"></param>
        public void DynamicStartAndPause(Operation _operation, float pauseProgress)
        {
            if (initialTime == null) initialTime = new ClassRef<float>(1f);
            operation = _operation;
            this.pauseProgress = pauseProgress;
            StartNoReset();
        }
        
        public void StartAndPauseAt(Operation _operation, float pauseProgress)
        {
            operation = _operation;
            this.pauseProgress = pauseProgress;
            base.StartAndReset();
        }


    }
}