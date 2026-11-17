using UnityEngine;

public class EE_SerratedGreataxe : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Serrated Greataxe";

    public override int priority => 1;

    public override bool isSpecial => false;

    // If the first Action of the turn is an Attack that costs 3 AP or more, apply 1 stack of Vulnerable to the target

    bool hasTriggered = false;

    public override BattlemodeActionCtx ResolveEffectAfterActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_SerratedGreataxe effect to non-battler occupant");
            return actionCtx;
        }

        if (actionCtx.ap >= 3 && !hasTriggered)
        {
            // +1 vulnerable to target
        }
        return actionCtx;
    }

    // cant override end turn
    // reset hasTriggered
}
