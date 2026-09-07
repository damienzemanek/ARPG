using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class SE_Deadeye : BattlemodeEffectStrategyInstance
{
    public override int priority => 1;
    public override bool isSpecial => true;

    public override BattlemodeActionCtx ResolveEffectPreBeforeActing(
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


// Attacks that consume mark will apply 2 additional marks to the target. 
// Which essentially extends the mark duration by 1 and this turn.
[Serializable]
public sealed class SE_DoubleJeopardy : BattlemodeEffectStrategyInstance
{
    public override int priority => 1;
    public override bool isSpecial => true;
    BattlemodeEffectStrategy_Mark newMarkEffect => CreateInstance<BattlemodeEffectStrategy_Mark>
        (2, 0, 0, BattlemodeEffectConfigInstance.AddOccurance.AfterAction);

    public override BattlemodeActionCtx ResolveEffectAfterActing(
        OccupantCtx occupantCtx, 
        BattlemodeActionCtx actionCtx)
    {
        if(stacksTotal <= 0) return actionCtx;
        Debug.Log("[SP EFFECT] Double Jeopardy: Resolving");
        if (!actionCtx.cfg.actionQualifier.HasFlag(BattlemodeActionConfig.ActionQualifier.ConsumeMark)) return actionCtx;
        Debug.Log("[SP EFFECT] Double Jeopardy: Guard Clause Passed, Applying: Adding 2 Marks to target");
        actionCtx.additionalEffectsToApplyToTarget ??= new List<BattlemodeEffectStrategyInstance>();
        actionCtx.additionalEffectsToApplyToTarget.Add(newMarkEffect);
        RemoveAllStacksOnLastHit(actionCtx);
        return actionCtx;
    }
    
}
