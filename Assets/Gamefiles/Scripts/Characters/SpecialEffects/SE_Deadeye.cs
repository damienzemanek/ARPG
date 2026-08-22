using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class SE_Deadeye : BattlemodeEffectStrategy
{
    public override bool isSpecial => true;
    public override BattlemodeEffectConfig.RemovalOccurrence removealOccurance => BattlemodeEffectConfig.RemovalOccurrence.None;
    public override BattlemodeEffectConfig.ResolveOccurance resolveOccurance => BattlemodeEffectConfig.ResolveOccurance.BeforeActing;

    public override BattlemodeActionCtx ResolveEffect(
        OccupantCtx occupantCtx, 
        BattlemodeActionCtx actionCtx,
        int stacks)
    {
        Debug.Log("[SP EFFECT] Deadeye: Resolving");
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply Deadeye effect to non-battler occupant");
            return actionCtx;
        }
        if (actionCtx.cfg.actionName != "Deadeye") return actionCtx;
        if (stacks <= 0) return actionCtx;
        actionCtx.deltaDmgMultiplier += (100 * stacks);
        if(stacks > 1)
            actionCtx.apDelta += stacks - 1;
        Debug.Log("[SP EFFECT] Deadeye: Resolved, AP Delta: " + actionCtx.apDelta + "");

        return actionCtx;
    }
}
