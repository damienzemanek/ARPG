using System;
using EMILtools.Extensions;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnityEventPlus
{

    #region Privates

    #endregion

    public enum DelayType
    {
        oneVal,
        randomValBetween
    }
    public DelayType delayType;

    [ShowIf(condition: "oneVal")] public float delay = 0;
    [ShowIf(condition: "randomValBetween")] public Vector2 random = Vector2.zero;

    [TabGroup("Prereq")] public Func<bool> canCall;

    public UnityEvent get = new UnityEvent();

    public void InvokeWithDelay(MonoBehaviour mono)
    {
        if (!mono) return;

        if (delay <= 0) { get?.Invoke(); return; }

        if(oneVal)
            mono.DelayedCall(() => get?.Invoke(), delay);
        else if(randomValBetween)
            mono.DelayedCall(() => get?.Invoke(), random.Rand());

    }
    public void InvokeWithCondition(MonoBehaviour mono)
    {
        if (!mono) return;

        if (canCall.Invoke()) get?.Invoke();
    }

    public UnityEventPlus()
    {
        delay = 0;
        get = new UnityEvent();
        canCall = () => true;
    }

    public UnityEventPlus(Func<bool> condition)
    {
        delay = 0;
        get = new UnityEvent();
        canCall = condition;
    }

    public bool oneVal => delayType == DelayType.oneVal;
    public bool randomValBetween => delayType == DelayType.randomValBetween;


    #region Methods

    #endregion

}
