using System.Linq;
using UnityEngine;

public class EE_BrandbearerCharm : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Brandbearer Charm";

    public override int priority => 1;

    public override bool isSpecial => false;

    // If character is Marked, attacks deal +1 damage

    public override BattlemodeActionCtx ResolveEffectRightBeforeActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_BrandbearerCharm effect to non-battler occupant");
            return actionCtx;
        }

        if(battlerOccupantCtx.currentEffects.Any(effect => effect.effectName == "Mark"))
        {
            actionCtx.dmg = actionCtx.dmg + 1;
        }
        return actionCtx;
    }
}
