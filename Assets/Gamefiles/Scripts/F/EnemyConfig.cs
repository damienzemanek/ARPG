using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "ARPG/SO/EnemyConfig")]
public class EnemyConfig : BattlerConfig
{
    public enum AttackOrder
    {
        First,
        Second,
        Third,
        Fourth,
        Last
    }

    public enum AttackPriority
    {
        LowHP,
        HighHP,
        Closest,
        Farthest,
    }

    public AttackOrder attackOrder;
    public int primaryAttackPriorityWeight = 2;
    public int secondaryAttackPriorityWeight = 1;
    public AttackPriority PrimaryAttackPriority;
    public AttackPriority SecondaryAttackPriority;
    [FormerlySerializedAs("intentUsage")] public IntentUsage intentUsageCfg;

    [FormerlySerializedAs("actionIntentionsCount")]
    public int intentions = 1;

    [FormerlySerializedAs("visableActionCount")]
    [FormerlySerializedAs("VisableActionCount")]
    public int defaultPredicted = 1;

    void OnValidate()
    {
        if (intentUsageCfg == null || equippedActions == null) return;
        int availableActions = equippedActions.Length;
        if (intentUsageCfg.phases == null) return;

        for (int i = 0; i < intentUsageCfg.phases.Length; i++)
        {
            IntentPhase phase = intentUsageCfg.phases[i];

            if (phase == null) continue;

            if (!phase.ValidateActionOrder(availableActions))
                Debug.LogError($"Invalid Intent Phase {i} on EnemyConfig '{name}'.", this);
        }
    }
}

[Serializable]
public class IntentUsage
{
    
    // creating in new eoc, eoc created in tile init, which is at start
    [Serializable]
    public class IntentUsageCtx
    {
        public int currentIntentionIndexCurrentAttempt;
        public int intentionsAmount;
        public int intentIndex;
        public int phaseIndex;

        public IntentUsageCtx(int intentions)
        {
            intentionsAmount = intentions;
            intentIndex = 0;
            phaseIndex = 0;
            currentIntentionIndexCurrentAttempt = 1;
        }
    }


    [MinValue(1)]
    int _phaseCount = 1;

    [MinValue(1)]
    public int phaseCount
    {
        get => _phaseCount;
        set
        {
            if (value < 1) return;
            _phaseCount = value;
            UpdatePhases();
        }
    }

    [ListDrawerSettings(Expanded = true)]
    public IntentPhase[] phases = new IntentPhase[1];

    void UpdatePhases()
    {
        var oldPhases = phases;
        phases = new IntentPhase[phaseCount];
        if (oldPhases != null)
            Array.Copy(oldPhases, phases, Mathf.Min(oldPhases.Length, phaseCount));
    }


    public IntentUsageCtx GetAndProgressIntent(float healthPercentage01, IntentUsageCtx currentIntentUsageCtx)
    {
        if (phases == null || phases.Length == 0) {
            Debug.LogError("No phases found in IntentUsage.");
            return currentIntentUsageCtx; }
        

        healthPercentage01 = Mathf.Clamp01(healthPercentage01);
        int phaseIndex = 0;

        // Find the active phase.
        for (int i = 0; i < phases.Length; i++)
        {
            if (phases[i] == null || !(healthPercentage01 <= phases[i].hpThreshold)) continue;
            phaseIndex = i;
            break;
        }

        // If the phase changed, restart its intent traversal.
        if (phaseIndex != currentIntentUsageCtx.phaseIndex)
        {
            currentIntentUsageCtx.phaseIndex = phaseIndex;
            currentIntentUsageCtx.intentIndex = 0;
        }
        else // if no phase change, increment the intent index.
        // else increment the intent index.
        // note: saved intent indexes do not save across phases.
        {
            Debug.Log("[EnemyConfig] Incrementing IntentUsageCtx's Intent Index from " +
                      "" + currentIntentUsageCtx.intentIndex + " to " 
                      + (currentIntentUsageCtx.intentIndex + 1).ToString());
            currentIntentUsageCtx.intentIndex++;
            if(currentIntentUsageCtx.intentIndex > currentIntentUsageCtx.intentionsAmount)
                currentIntentUsageCtx.intentIndex = 0;
        }
        
        return currentIntentUsageCtx;
    }
}

[Serializable]
public class IntentPhase
{
    [Range(0f, 1f)]
    public float hpThreshold;

    public bool linearDefaultTraversal = true;

    [Tooltip("Treat the order as starting with action 0.")]
    [HideIf("linearDefaultTraversal")]
    public bool leadingZero; // Displaces, doe not replace

    [Tooltip("Each decimal digit represents an action index. Example: 2314 = 2 -> 3 -> 1 -> 4.")]
    [HideIf("linearDefaultTraversal")]
    public ulong actionOrder;

    public int GetActionIndex(int position, int maxAmountOfTotalActionsAvaliable)
    {
        if (position < 0) return -1;
        if (maxAmountOfTotalActionsAvaliable <= 0 ||
            maxAmountOfTotalActionsAvaliable > 9)
            return -1;
        // Default traversal is simply 0 -> 1 -> 2 -> ... -> max - 1 -> 0...
        if (linearDefaultTraversal) return position % maxAmountOfTotalActionsAvaliable;

        ulong order = actionOrder;

        int digitCount = 0;
        ulong temp = order;

        while (temp > 0)
        {
            digitCount++;
            temp /= 10;
        }

        if (leadingZero) digitCount++;
        if (digitCount == 0) return -1;

        // Loop back to the beginning of the custom order.
        position %= digitCount;

        // The leading zero occupies position 0.
        if (leadingZero && position == 0) return 0;

        int digitsFromRight = digitCount - position - 1;
        while (digitsFromRight-- > 0)
            order /= 10;

        return (int)(order % 10);
    }

    public bool ValidateActionOrder(int maxAmountOfTotalActionsAvaliable)
    {
        if (maxAmountOfTotalActionsAvaliable <= 0)
        {
            Debug.LogError("IntentPhase cannot have an action order when no actions are available.");
            return false;
        }

        if (maxAmountOfTotalActionsAvaliable > 9)
        {
            Debug.LogError($"IntentPhase supports a maximum of 9 actions, but " +
                           $"{maxAmountOfTotalActionsAvaliable} are available.");
            return false;
        }

        // Linear traversal automatically adapts to the number of actions.
        if (linearDefaultTraversal) return true;

        // A leading zero is always valid because action 0 exists
        // whenever at least one action is available.
        ulong order = actionOrder;

        if (order == 0 && !leadingZero)
        {
            Debug.LogError("IntentPhase has a custom traversal enabled, but no action order was provided.");
            return false;
        }

        while (order > 0)
        {
            int digit = (int)(order % 10);

            if (digit >= maxAmountOfTotalActionsAvaliable)
            {
                Debug.LogError(
                    $"IntentPhase action order '{actionOrder}' contains " +
                    $"action index {digit}, but only " +
                    $"{maxAmountOfTotalActionsAvaliable} actions are available. " +
                    $"Valid action indices are 0-{maxAmountOfTotalActionsAvaliable - 1}."
                );

                return false;
            }

            order /= 10;
        }

        return true;
    }
}