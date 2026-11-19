using System.Collections.Generic;
using UnityEngine;

public class EE_SerratedGreataxe : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Serrated Greataxe";

    public override int priority => 1;

    public override bool isSpecial => false;

    // If the first Action of the turn is an Attack that costs 3 AP or more, apply 1 stack of Vulnerable to the target

    BattlemodeEffectStrategy_Vulnerable newVulnerableEffect => CreateInstance<BattlemodeEffectStrategy_Vulnerable>
        (1, 0, 0, BattlemodeEffectConfigInstance.AddOccurance.AfterAction);
    bool hasTriggered = false;

    public override BattlemodeActionCtx ResolveEffectAfterActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_SerratedGreataxe effect to non-battler occupant");
            return actionCtx;
        }

        if (actionCtx.dmg > 0 && actionCtx.ap >= 3 && !hasTriggered) 
        {
            actionCtx.additionalEffectsToApplyToTarget ??= new List<BattlemodeEffectStrategyInstance>();
            actionCtx.additionalEffectsToApplyToTarget.Add(newVulnerableEffect);
            hasTriggered = true;
        }
        return actionCtx;
    }

    public override void ResolveEffectAfterTurnEndsImplementation(OccupantCtx occupantCtx, BattleTile tile)
    {
        hasTriggered = false;
    }
}
