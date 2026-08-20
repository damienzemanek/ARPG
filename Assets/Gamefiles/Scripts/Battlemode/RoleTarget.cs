using System.Collections.Generic;
using UnityEngine;
using static BattleTracker;



public abstract class RoleTarget
{
    public abstract BattlemodeActionConfig.Role role { get; }
    public abstract void ClearTarget();
    public abstract void ActUponTarget(QueuedAction queuedActionCtx, BattleTile fromTile);
    
    /// <summary>
    /// This may seem complex, Order is:
    /// 1. Resolve BeforeActor Effects by itself if there no occupant to adjust health target max
    /// 1.1 Otherwise Resolve BeforeActor Effects with the health target if there is one to go off of
    /// 2. If there is an occupant, act upon it
    /// 2.2 Otherwise we do a movement consideration check to move into an empty tile
    /// 3. Finaly, Resolve AfterActor Effects
    ///
    /// Note: BeforeActor effects happen to the ACTOR (the one doing the action)
    ///       AfterActor effects happen to the ACTOR (the one doing the action)
    /// </summary>
    /// <param name="queuedAction"></param>
    /// <param name="targetTile"></param>
    public void ActUponTargetImpl(QueuedAction queuedAction, BattleTile targetTile, BattleTile fromTile)
    {
        if (targetTile == null) { Debug.LogError("No target selected for self role target."); return; }
        if (queuedAction.actionCtx == null) { Debug.LogError("No action context generated."); return; }

        string targName = targetTile.occupantCtx?.cfg.occupantName ?? "Empty Tile";
        Debug.Log($"{queuedAction.actingOccupantCtx.cfg.occupantName}" +
                  $" is Acting upon target {targName} " +
                  $"at tile: [" + targetTile.col + ", " + targetTile.row + "]" +
                  "with action: " + queuedAction.actionCtx.cfg.name + "");
        
        BattlerOccupantCtx _targetBattlerCtx = null;

        if(targetTile.occupantCtx is not BattlerOccupantCtx targetBattlerCtx)
            queuedAction.actionCtx = ResolveBeforeActorEffects(queuedAction, queuedAction.actionCtx);
        else 
        {
            _targetBattlerCtx = targetBattlerCtx;
            queuedAction.actionCtx.SetHealViaTargetMaxHealth(targetBattlerCtx);
            queuedAction.actionCtx = ResolveBeforeActorEffects(queuedAction, queuedAction.actionCtx);
        }
        
        if(_targetBattlerCtx != null)
            _targetBattlerCtx.ActedUponByAction(queuedAction, queuedAction.actionCtx); // Acting on Target
        else if(queuedAction.lookingForTarget == BattlemodeActionConfig.Role.EmptyTile 
        && targetTile.IsEmptyTile() 
        && queuedAction.actionCtx.cfg.moveToSelectedEmptyTile)
            targetTile.TransferInOccupant(fromTile, BattleTracker.Instance.gridWorld);
    
        ResolveAfterActorEffects(queuedAction, queuedAction.actionCtx);
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
    public BattleTile selfTile;
    public override void ClearTarget() => selfTile = null;
    public override void ActUponTarget(QueuedAction queuedActionCtx, BattleTile fromTile) 
        => ActUponTargetImpl(queuedActionCtx, selfTile, fromTile);
}

public sealed class AllyRoleTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Ally;
    public BattleTile allyTile;
    public override void ClearTarget() => allyTile = null;
    public override void ActUponTarget(QueuedAction queuedActionCtx, BattleTile fromTile) 
        => ActUponTargetImpl(queuedActionCtx, allyTile, fromTile);
}

public sealed class EnemyRoleTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Enemy;
    public BattleTile enemyTile;
    public override void ClearTarget() => enemyTile = null;
    public override void ActUponTarget(QueuedAction queuedActionCtx, BattleTile fromTile) 
        => ActUponTargetImpl(queuedActionCtx, enemyTile, fromTile);
}

// For now this is per target for every target, not for every target, mabye change later
public sealed class AllyTeamTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Team;
    public List<BattleTile> allyTiles = new();
    public override void ClearTarget() => allyTiles.Clear();
    public override void ActUponTarget(QueuedAction queuedActionCtx, BattleTile fromTile)
    {
        foreach (var tile in allyTiles)
            ActUponTargetImpl(queuedActionCtx, tile, fromTile);
    }
}

// For now this is per target for every target, not for every target, mabye change later
public sealed class EnemyTeamTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Team;
    public List<BattleTile> enemyTiles = new();
    public override void ClearTarget() => enemyTiles.Clear();
    public override void ActUponTarget(QueuedAction queuedActionCtx, BattleTile fromTile)
    {
        foreach (var tile in enemyTiles)
            ActUponTargetImpl(queuedActionCtx, tile, fromTile);
    }
}

public sealed class EmptyTileTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.EmptyTile;
    public BattleTile emptyTile;
    public override void ClearTarget() => emptyTile = null;
    public override void ActUponTarget(QueuedAction queuedActionCtx, BattleTile fromTile) 
        => ActUponTargetImpl(queuedActionCtx, emptyTile, fromTile);
}