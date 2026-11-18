using UnityEngine;

public class EE_CloudwalkerBoots : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Cloudwalker Boots";

    public override int priority => 1;

    public override bool isSpecial => false;

    // Gives additional 0 AP cost movement each turn. Cannot enter contested area.

    public override BattlemodeActionCtx ResolveEffectPreBeforeActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_CloudwalkerBoots effect to non-battler occupant");
            return actionCtx;
        }

        
        return actionCtx;
    }
}
