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
        if (actionCtx.critchanceCalculatedAlready)
        {
            RemoveAllStacksOnLastHit();
            return actionCtx;
        }

        if (stacksTotal == 1)
        {
            if (!actionCtx.critHit)
                actionCtx.critHit = UnityEngine.Random.Range(0, 100) < 50;
        }
        else if (stacksTotal > 1) actionCtx.critHit = true;
        
        RemoveAllStacksOnLastHit();
        
        return actionCtx;
        
        void RemoveAllStacksOnLastHit()
        {
            if(actionCtx.currentHitCount < actionCtx.maxHitCount) return;
            Debug.Log("[Critical Avaliable] Removing stacks");
            GetStackCtx(BattlemodeEffectConfigInstance.EffectTime.Turn).stacks = 0;
            GetStackCtx(BattlemodeEffectConfigInstance.EffectTime.Battle).stacks = 0;
            GetStackCtx(BattlemodeEffectConfigInstance.EffectTime.Expedition).stacks = 0;
        }
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
        if (actionCtx.critchanceCalculatedAlready)
        {
            RemoveAllStacksOnLastHit();
            return actionCtx;
        }
        
        if (stacksTotal == 1)
        {
            if (!actionCtx.critHit)
                actionCtx.critHit = UnityEngine.Random.Range(0, 100) < 50;
        }
        else if (stacksTotal > 1) actionCtx.critHit = true;
        
        RemoveAllStacksOnLastHit();
        
        return actionCtx;

        void RemoveAllStacksOnLastHit()
        {
            if (actionCtx.currentHitCount < actionCtx.maxHitCount) return;
            Debug.Log("[Critically Exposed] Removing stacks");
            GetStackCtx(BattlemodeEffectConfigInstance.EffectTime.Turn).stacks = 0;
            GetStackCtx(BattlemodeEffectConfigInstance.EffectTime.Battle).stacks = 0;
            GetStackCtx(BattlemodeEffectConfigInstance.EffectTime.Expedition).stacks = 0;
        }
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