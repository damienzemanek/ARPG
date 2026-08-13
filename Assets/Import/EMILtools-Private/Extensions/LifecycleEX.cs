using Sirenix.OdinInspector;
using System;
using EMILtools.Core;
using UnityEngine;

public static class LifecycleEX
{
    [Serializable]
    public class DelayLimitedMethod
    {
        public float delay;
        [ShowInInspector, ReadOnly] float timer;
        [NonSerialized] PersistentAction method;
        public void InjectMethod(PersistentAction mthd) => method = mthd;
        public void RateLimitedUpdateTick()
        {
            timer += Time.deltaTime;
            if (timer < delay) return;
            timer = 0;
            method?.Invoke();
        }
    }

}
