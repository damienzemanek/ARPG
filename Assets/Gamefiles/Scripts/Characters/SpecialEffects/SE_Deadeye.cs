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
        Debug.Log("B4");
        Debug.Log(actionCtx);
        Debug.Log(actionCtx.cfg);
        Debug.Log(actionCtx.cfg.actionName);
        if (actionCtx.cfg.actionName != "Deadeye") return actionCtx;
        Debug.Log("B5");
        if (stacksTotal <= 0) return actionCtx;
        Debug.Log("B6");
        actionCtx.deltaDmgMultiplier += (100 * stacksTotal);
        if(stacksTotal > 1)
            actionCtx.apDelta += stacksTotal - 1;
        Debug.Log("[SP EFFECT] Deadeye: Resolved, AP Delta: " + actionCtx.apDelta + "");

        return actionCtx;
    }
}
