using Unity.VisualScripting;
using UnityEngine;

public class EE_BlessedRosary : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Blessed Rosary";

    public override int priority => 1;

    public override bool isSpecial => false;

    // 50% chance of preventing death and come back with 1 HP. Twice per battle.

    int maxTriggerCount = 2;
    int triggerCount = 0;

    public override BattlemodeActionCtx ResolveEffectAfterHitByAction(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_BlessedRosary effect to non-battler occupant");
            return actionCtx;
        }

        if (battlerOccupantCtx.currentHp <= 0 && triggerCount < maxTriggerCount)
        {
            if (Random.value < 0.5f)
            {
                battlerOccupantCtx.currentHp = 1;
                triggerCount++;
            }
        }
        return actionCtx;
    }
}
