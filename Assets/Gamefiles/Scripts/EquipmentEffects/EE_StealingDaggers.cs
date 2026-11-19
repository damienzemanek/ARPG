using UnityEngine;

public class EE_StealingDaggers : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Stealing Daggers";

    public override int priority => 1;

    public override bool isSpecial => false;

    // When the character has 0 Armor, their next Attack generates 50% of damage dealt into Armor. Twice per battle.

    int maxTriggered = 2;
    int timeTriggered = 0;

    public override BattlemodeActionCtx ResolveEffectAfterActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_StealingDaggers effect to non-battler occupant");
            return actionCtx;
        }
        
        if (battlerOccupantCtx.currentArmor <= 0 && timeTriggered < maxTriggered) 
        {
            if(actionCtx.dmg <= 0) // This prevents the effect from being wasted on a non-damaging attack.
            {
                return actionCtx;
            }
            battlerOccupantCtx.currentArmor += Mathf.FloorToInt(actionCtx.dmg * 0.5f + 0.5f);
            timeTriggered++;
        }
        return actionCtx;
    }
}
