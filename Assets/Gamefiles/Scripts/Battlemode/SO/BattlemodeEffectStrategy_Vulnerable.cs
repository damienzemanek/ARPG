using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public sealed class BattlemodeEffectStrategy_Vulnerable : BattlemodeEffectStrategyInstance
{
    public override int priority => 1;
    public override bool isSpecial => false;

    public override BattlemodeActionCtx ResolveEffectBeforeHitByAction(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        var newDmgVal = Mathf.CeilToInt(actionCtx.dmg * 1.5f); // *1.5, or 50% more dmg taken
        actionCtx.dmg = newDmgVal;
        return actionCtx;
    }
}

[Serializable]
public sealed class BattlemodeEffectStrategy_Swift : BattlemodeEffectStrategyInstance
{
    public override int priority => 1;
    public override bool isSpecial => false;
    [ShowInInspector] SwiftStrategyCtx swiftStrategyCtx;

    public override BattlemodeActionCtx ResolveEffectPreBeforeActing(
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
        return actionCtx;
    }
    
    public struct SwiftStrategyCtx 
    {
        public bool usedThisRound = false;

        public SwiftStrategyCtx() { }
    }

    public override void ResolveEffectAfterTurnEndsImplementation(OccupantCtx occupantCtx)
    {
        Debug.Log("[SP EFFECT] Swift: Turn Ends");
        swiftStrategyCtx.usedThisRound = false;
    }
}

[Serializable]
public sealed class BattlemodeEffectStrategy_CriticalAvaliable : BattlemodeEffectStrategyInstance
{
    public override int priority => 3;
    public override bool isSpecial => false;

    public override BattlemodeActionCtx ResolveEffectRightBeforeActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        // Crits cannot target the self or allys
        switch (actionCtx.cfg.roleTarget) {
            case BattlemodeActionConfig.Role.Self: return actionCtx; 
            case BattlemodeActionConfig.Role.Ally: return actionCtx;
            case BattlemodeActionConfig.Role.Team: return actionCtx; }

        if(stacksTotal == 0) return actionCtx;
        actionCtx.hasCritChance = true;

        if (actionCtx.critchanceCalculatedAlready)
        {
            RemoveAllStacksOnLastHit(actionCtx);
            return actionCtx;
        }

        if (stacksTotal == 1)
        {
            if (!actionCtx.critHit)
                actionCtx.critHit = UnityEngine.Random.Range(0, 100) < 50;
        }
        else if (stacksTotal > 1) actionCtx.critHit = true;
        
        RemoveAllStacksOnLastHit(actionCtx);
        
        return actionCtx;
    }
}

[Serializable]
public sealed class BattlemodeEffectStrategy_CriticallyExposed : BattlemodeEffectStrategyInstance
{
    public override int priority => 2;
    public override bool isSpecial => false;

    public override BattlemodeActionCtx ResolveEffectBeforeHitByAction(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        // Crits cannot target the self or allys
        switch (actionCtx.cfg.roleTarget) {
            case BattlemodeActionConfig.Role.Self: return actionCtx; 
            case BattlemodeActionConfig.Role.Ally: return actionCtx;
            case BattlemodeActionConfig.Role.Team: return actionCtx; }

        if(stacksTotal == 0) return actionCtx;
        actionCtx.hasCritChance = true;
        if (actionCtx.critchanceCalculatedAlready)
        {
            RemoveAllStacksOnLastHit(actionCtx);
            return actionCtx;
        }
        
        if (stacksTotal == 1)
        {
            if (!actionCtx.critHit)
                actionCtx.critHit = UnityEngine.Random.Range(0, 100) < 50;
        }
        else if (stacksTotal > 1) actionCtx.critHit = true;
        
        RemoveAllStacksOnLastHit(actionCtx);
        
        return actionCtx;
        
    }
    
}

[Serializable]
public sealed class BattlemodeEffectStrategy_MoveLocked : BattlemodeEffectStrategyInstance
{
    public override int priority => 1;
    public override bool isSpecial => false;

    public override BattlemodeActionCtx ResolveEffectPreBeforeActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        ref var movementCfgInstanced = ref actionCtx.movementCfgInstanced;
        if (movementCfgInstanced.amount > 0) movementCfgInstanced.amount = 0;
        return actionCtx;
    }

    public override BattlemodeActionCtx ResolveEffectRightBeforeQueued(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        ref var targetingCfgInstanced = ref actionCtx.targetingCfgInstanced;
        if (actionCtx.cfg.actionName == "Move")
            targetingCfgInstanced.targetingPatternAdditive = BattlemodeActionConfig.TargetingPattern.None;
        return actionCtx;
    }
}

[Serializable ]
public sealed class BattlemodeEffectStrategy_BodyCompromised : BattlemodeEffectStrategyInstance
{
    public override int priority => 0; // Last, should happen after critically exposed caluclates, to check if the crit hit
    public override bool isSpecial => false;
    
    public override BattlemodeActionCtx ResolveEffectBeforeHitByAction(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (!actionCtx.hasCritChance) return actionCtx;
        if (actionCtx.bodyPartAlreadyBrokenThisAction || !actionCtx.critHit)
        {
            RemoveAllStacksOnLastHit(actionCtx);
            return actionCtx;
        }
        
        if (UnityEngine.Random.Range(0, 100) < 50)
        {
            actionCtx.brokenBodyPart = actionCtx.cfg.targetedBodyPart;
            Debug.Log("[Body Compromised] BONE BREAK! : " + actionCtx.brokenBodyPart + " is broken");
        }
        RemoveAllStacksOnLastHit(actionCtx);
        return actionCtx;
    }
    
    
}