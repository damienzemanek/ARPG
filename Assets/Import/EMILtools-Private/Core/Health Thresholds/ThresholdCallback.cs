﻿using System;
using System.Collections.Generic;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;


public interface IThresholdMutator
{
    public void AddOrReplaceDelegate(Enum label, IDelegator cb);
    public void SetAllDelegates(IDelegator cb);
}




[Serializable]
public abstract class Threshold<TEnum, TDelegator> : ThresholdCore,
    IThresholdMutator
    where TDelegator : IDelegator
    where TEnum : Enum
{
    [Serializable]
    public struct Entry
    {
        public TEnum label;
        public float threshold;
    }
    
    [SerializeField] Entry[] entries;
    
    [Button]
    public override void Sort()
    {
        if (entries == null || entries.Length == 0)
            return;

        Array.Sort(entries, (a, b) => b.threshold.CompareTo(a.threshold)); 
    }
    
    public override float GetHighestThreshold()
    {
        float highest = -1;
        foreach (var entry in entries)
        {
            Debug.Log("PPS - state: " + entry.label + " threshold: " + entry.threshold);
            if (entry.threshold > highest) highest = entry.threshold;
        }
        Debug.Log("PPS - highest threshold: " + highest);
        return highest;
    }
    
    public override Enum GetHighestThresholdState() => GetHighestThresholdLabel();

    public TEnum GetHighestThresholdLabel()
    {
        float highest = float.MinValue;
        TEnum label = default;
        foreach (var entry in entries)
        {
            if (entry.threshold > highest)
            {
                highest = entry.threshold;
                label = entry.label;
            }
        }
        return label;
    }
    
    public override bool GetNearestLastThreshold(float value, out Enum label)
    {
        if (entries == null || entries.Length == 0)
        {
            label = default;
            return false;
        }

        Array.Sort(entries, (a, b) => b.threshold.CompareTo(a.threshold));

        int nearest = -1;

        // "Last reached" while moving downward through thresholds.
        // Example thresholds: Alive(10), Dying(3), Dead(0)
        // value 4  -> Alive
        // value 2  -> Dying
        // value 0  -> Dead
        for (int i = 0; i < entries.Length; i++)
        {
            if (value <= entries[i].threshold)
                nearest = i;
            else
                break;
        }

        if (nearest == -1)
        {
            label = default;
            return false;
        }

        label = entries[nearest].label;
        return true;
    }
    
    public override bool WasThresholdReached(ref int index, float value, out Enum label)
    {
        if (entries == null || index < 0 || index >= entries.Length)
        {
            label = default;
            return false;
        }

        ref var entry = ref entries[index];

        if (value <= entry.threshold)
        {
            label = entry.label;

            index++; 
            return true;
        }

        label = default;
        return false;
    }

    public override int ResyncThresholdIndex(float value, out Enum state)
    {
        if (entries == null || entries.Length == 0)
        {
            state = default;
            return 0;
        }

        // Entries are expected to be sorted descending (highest HP first)
        // index 0: Alive (100)
        // index 1: Phase2 (50)
        // index 2: Dying (10)
        // index 3: Dead (0)
        
        // If we are at 60 HP, the NEXT threshold to reach is index 1 (Phase2 at 50)
        // So we should return index 1.
        // The current state is index 0 (Alive).

        int nextIndex = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            if (value <= entries[i].threshold)
            {
                // We have passed this threshold (or we are exactly at it)
                // The NEXT threshold will be the one after this.
                nextIndex = i + 1;
            }
            else
            {
                // We haven't reached this threshold yet.
                // So this is the NEXT threshold.
                nextIndex = i;
                break;
            }
        }

        // Clamp nextIndex to entries.Length
        if (nextIndex > entries.Length) nextIndex = entries.Length;
        
        // State is the last reached threshold.
        if (nextIndex == 0)
        {
            // Haven't even reached the first one? (Shouldn't happen if first is max health)
            state = entries[0].label;
        }
        else
        {
            state = entries[nextIndex - 1].label;
        }
        
        return nextIndex;
    }
    
    
    public override void LogThresholds(ref int index, float currentValue)
    {
        if (entries == null || entries.Length == 0)
        {
            Debug.Log("No thresholds configured.");
            return;
        }

        string log = $"[Threshold Debug] Current HP: {currentValue}\n";
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            string status = (i < index) ? "[REACHED]" : (i == index ? "[NEXT]" : "[PENDING]");
            log += $"{status} {entry.label}: {entry.threshold} (Index: {i})\n";
        }
        Debug.Log(log);
    }

    public override void SetAllDelegates(IDelegator cb)
    {
        if (entries == null) throw new InvalidOperationException("Threshold entries are not initialized.");
        if (!(cb is TDelegator typedCb)) throw new ArgumentException($"Callback must be of type {typeof(TDelegator)}", nameof(cb));

        // Note: This SO-based method is deprecated in favor of LivingEntity instance-based callbacks
        // but kept for interface compatibility if needed for other purposes.
        Debug.LogWarning("SetAllDelegates on Threshold SO is deprecated. Use LivingEntity.SetAllThresholdCallbacks instead.");
    }

    public override void AddOrReplaceDelegate(Enum label, IDelegator cb)
    {
        if (entries == null) throw new InvalidOperationException("Threshold entries are not initialized.");
        if (!(label is TEnum typedLabel)) throw new ArgumentException($"Label must be of type {typeof(TEnum)}", nameof(label));
        if (!(cb is TDelegator typedCb)) throw new ArgumentException($"Callback must be of type {typeof(TDelegator)}", nameof(cb));

        int indx = Array.FindIndex(entries, entry => EqualityComparer<TEnum>.Default.Equals(entry.label, typedLabel));
        if (indx == -1) throw new ArgumentException("Threshold does not contain label", nameof(label));

        Debug.LogWarning("AddOrReplaceDelegate on Threshold SO is deprecated. Use LivingEntity.AddOrReplaceThresholdCallback instead.");
    }
}