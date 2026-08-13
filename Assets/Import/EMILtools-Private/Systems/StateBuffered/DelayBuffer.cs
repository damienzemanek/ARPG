using System.Collections.Generic;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Timers.TimerUtility;

namespace EMILtools.Systems
{
    public class DelayBuffer<TVal> : ITimerUser
    {
        [ShowInInspector] public TVal Value
        {
            get => isBuffered ? bufferVal : actualVal;
            set
            {
                if (EqualityComparer<TVal>.Default.Equals(actualVal, value)) return;
                actualVal = value;
                StateChanged();
            }
        }

        [ShowInInspector, ReadOnly] TVal actualVal;
        [ShowInInspector, ReadOnly] TVal bufferVal;
    
        bool isBuffered => bufferTime.isRunning;
        readonly CountdownTimer bufferTime;
    
        public DelayBuffer(TVal value, ClassRef<float> _bufferTime)
        {
            bufferTime = new CountdownTimer(_bufferTime);
            bufferTime.OnTimerStop.Add(BufferEndsThenReset);
            this.InitTimer(bufferTime, true);
            actualVal = bufferVal = value;
        }

        void StateChanged()
        {
            bufferTime.StartAndReset();
        }

        void BufferEndsThenReset()
        {
            bufferVal = actualVal;
        }
        
        public void SetNotBuffered(TVal val) => actualVal = bufferVal = val;
        public void SetBuffered(TVal val) => Value = val;
        
        public static implicit operator TVal(DelayBuffer<TVal> buffer) => buffer.Value;
    }

}
