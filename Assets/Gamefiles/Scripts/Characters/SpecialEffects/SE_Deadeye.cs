using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class SE_Deadeye : BattlemodeEffectStrategyInstance
{
    public override bool isSpecial => true;

    public override BattlemodeActionCtx ResolveEffectBeforeActing(
        OccupantCtx occupantCtx, 
        BattlemodeActionCtx actionCtx)
    {
        Debug.Log("[SP EFFECT] Deadeye: Resolving");
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply Deadeye effect to non-battler occupant");
            return actionCtx;
        }
        if (actionCtx.cfg.actionName != "Deadeye") return actionCtx;
        if (stacksTotal <= 0) return actionCtx;
        actionCtx.deltaDmgMultiplier += (100 * stacksTotal);
        if(stacksTotal > 1)
            actionCtx.apDelta += stacksTotal - 1;
        Debug.Log("[SP EFFECT] Deadeye: Resolved, AP Delta: " + actionCtx.apDelta + "");

        return actionCtx;
    }
}
