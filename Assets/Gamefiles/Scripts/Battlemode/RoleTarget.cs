using System.Collections.Generic;
using UnityEngine;
using static BattleTracker;

public abstract class RoleTarget
{
    public abstract BattlemodeActionConfig.Role role { get; }
    public abstract void ClearTarget();
    public abstract void ActUponTarget(QueuedAction queuedActionCtx);
    public void ActUponTargetImpl(QueuedAction queuedAction, BattleTile tile)
    {
        if (tile == null) { Debug.LogError("No target selected for self role target."); return; }
        Debug.Log($"Acting upon target at tile: {tile}");

        if (tile.occupantCtx is not BattlerOccupantCtx targetBattlerCtx) return;
        
        queuedAction.queuedActionCtx.SetHealViaTargetMaxHealth(targetBattlerCtx);

        if (queuedAction.queuedActionCtx == null) { Debug.LogError("No action context generated."); return; }
        
        queuedAction.queuedActionCtx = ResolveBeforeActorEffects(queuedAction, queuedAction.queuedActionCtx);
        
        // Acting on Target
        targetBattlerCtx.ActedUponByAction(queuedAction, queuedAction.queuedActionCtx);
        
        ResolveAfterActorEffects(queuedAction, queuedAction.queuedActionCtx);
    }

    BattlemodeActionCtx ResolveBeforeActorEffects(QueuedAction queuedActionCtx, BattlemodeActionCtx actingActionCtx)
    {
        if (queuedActionCtx.targetTile.occupantCtx is not BattlerOccupantCtx battlerCtx) return actingActionCtx;
        foreach (var effect in battlerCtx.currentEffects)
            if (effect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.BeforeActing)
                actingActionCtx = effect.ResolveEffect(battlerCtx, actingActionCtx);
        foreach (var effect in battlerCtx.specialEffects)
            if (effect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.BeforeActing)
                actingActionCtx = effect.ResolveEffect(battlerCtx, actingActionCtx);
        return actingActionCtx;
    }

    void ResolveAfterActorEffects(QueuedAction queuedActionCtx, BattlemodeActionCtx actingActionCtx)
    {
        if (queuedActionCtx.targetTile.occupantCtx is not BattlerOccupantCtx battlerCtx) return;
        foreach (var effect in battlerCtx.currentEffects)
            if (effect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterActing)
                actingActionCtx = effect.ResolveEffect(battlerCtx, actingActionCtx);
        foreach (var effect in battlerCtx.specialEffects)
            if (effect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterActing)
                actingActionCtx = effect.ResolveEffect(battlerCtx, actingActionCtx);
    }
}

public sealed class SelfRoleTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Self;
    public BattleTile tile;
    public override void ClearTarget() => tile = null;
    public override void ActUponTarget(QueuedAction queuedActionCtx) 
        => ActUponTargetImpl(queuedActionCtx, tile);
}

public sealed class AllyRoleTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Ally;
    public BattleTile tile;
    public override void ClearTarget() => tile = null;
    public override void ActUponTarget(QueuedAction queuedActionCtx)
        => ActUponTargetImpl(queuedActionCtx, tile);
}

public sealed class EnemyRoleTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Enemy;
    public BattleTile tile;
    public override void ClearTarget() => tile = null;
    public override void ActUponTarget(QueuedAction queuedActionCtx) 
        => ActUponTargetImpl(queuedActionCtx, tile);
}

// For now this is per target for every target, not for every target, mabye change later
public sealed class AllyTeamTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Team;
    public List<BattleTile> tiles = new();
    public override void ClearTarget() => tiles.Clear();
    public override void ActUponTarget(QueuedAction queuedActionCtx)
    {
        foreach (var tile in tiles)
            ActUponTargetImpl(queuedActionCtx, tile);
    }
}

// For now this is per target for every target, not for every target, mabye change later
public sealed class EnemyTeamTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Team;
    public List<BattleTile> tiles = new();
    public override void ClearTarget() => tiles.Clear();
    public override void ActUponTarget(QueuedAction queuedActionCtx)
    {
        foreach (var tile in tiles)
            ActUponTargetImpl(queuedActionCtx, tile);
    }
}