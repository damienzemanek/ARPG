using UnityEngine;

public class EE_WardensGauntlet : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Warden's Gauntlet";

    public override int priority => 1;

    public override bool isSpecial => false;

    // If character Max Armor is 5 or more, the first Attack of the battle deals 200% damage.

    bool hasActivated = false;

    public override BattlemodeActionCtx ResolveEffectRightBeforeActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_WardensGauntlet effect to non-battler occupant");
            return actionCtx;
        }

        if (battlerOccupantCtx.maxArmor >= 5 && !hasActivated) 
        {
            if(actionCtx.dmg <= 0) // This prevents the effect from being wasted on a non-damaging attack.
            {
                return actionCtx;
            }
            actionCtx.dmg = actionCtx.dmg * 2;
            hasActivated = true; 
        }
        return actionCtx;
    }
}
