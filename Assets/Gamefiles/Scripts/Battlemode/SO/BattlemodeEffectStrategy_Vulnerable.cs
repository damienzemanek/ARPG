using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public sealed class BattlemodeEffectStrategy_Vulnerable : BattlemodeEffectStrategyInstance
{
    public override bool isSpecial => false;

    public override BattlemodeActionCtx ResolveEffectBeforeHitByAction(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        actionCtx.dmg *= 2;
        return actionCtx;
    }


}

[Serializable]
public sealed class BattlemodeEffectStrategy_Swift : BattlemodeEffectStrategyInstance
{
    public override bool isSpecial => false;
    [ShowInInspector] SwiftStrategyCtx swiftStrategyCtx;

    public override BattlemodeActionCtx ResolveEffectBeforeActing(
        OccupantCtx occupantCtx,
        BattlemodeActionCtx actionCtx)
    {
        Debug.Log("[SP EFFECT] Swift: Resolving");
        if(swiftStrategyCtx.usedThisRound) return actionCtx;
        
        actionCtx.ap -= 1;
        if(actionCtx.ap < 0) 
            actionCtx.ap = 0;
        
        return actionCtx;
    }

    public override BattlemodeActionCtx ResolveEffectAfterActing(
        OccupantCtx occupantCtx, 
        BattlemodeActionCtx actionCtx)
    {
        swiftStrategyCtx.usedThisRound = true;
        ctx.markForReResolve = true;
        return actionCtx;
    }
    
    public struct SwiftStrategyCtx 
    {
        public bool usedThisRound = false;

        public SwiftStrategyCtx() { }
    }

    public override void ResolveEffectAfterTurnEnds(OccupantCtx occupantCtx)
    {
        Debug.Log("[SP EFFECT] Swift: Turn Ends");
        swiftStrategyCtx.usedThisRound = false;
    }
}