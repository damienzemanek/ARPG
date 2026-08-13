using System;
using EMILtools.Core;
using UnityEngine;

[Serializable]
public abstract class ThresholdCore : ScriptableObject
{
    public abstract void Sort();
    public abstract float GetHighestThreshold();
    public abstract void LogThresholds(ref int index, float currentValue);
    public abstract bool WasThresholdReached(ref int index, float currentValue, out Enum state);
    public abstract int ResyncThresholdIndex(float currentValue, out Enum state);
    public abstract bool GetNearestLastThreshold(float value, out Enum label);
    public abstract Enum GetHighestThresholdState();
    public virtual void SetAllDelegates(IDelegator cb) { }
    public virtual void AddOrReplaceDelegate(Enum label, IDelegator cb) { }

}