using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BattleTracker;



public abstract class RoleTarget
{
    public abstract BattlemodeActionConfig.Role role { get; }
    public abstract void ClearTarget();
    public abstract void ActedUponBy(QueuedAction queuedActionCtx, BattleTile fromTile);
    
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
    public void ActedUponByImplementation(QueuedAction queuedAction, BattleTile targetTile, BattleTile fromTile)
    {
        if (targetTile == null) { Debug.LogError("No target selected for self role target."); return; }
        if (queuedAction.actionCtx == null) { Debug.LogError("No action context generated."); return; }

        string targName = targetTile.occupantCtx?.cfg.occupantName ?? "Empty Tile";
        Debug.Log($"{queuedAction.actingOccupantCtx.cfg.occupantName}" +
                  $" is Acting upon target {targName} " +
                  $"at tile: [" + targetTile.col + ", " + targetTile.row + "]" +
                  "with action: " + queuedAction.actionCtx.cfg.name + "");
        
        BattlerOccupantCtx _targetBattlerCtx = null;
        
        List<BattlemodeEffectStrategyInstance> effectsToAddToActorBeforeActing = null;
        List<BattlemodeEffectStrategyInstance> specialEffectsToAddToActorBeforeActing = null;
        
        List<BattlemodeEffectStrategyInstance> effectsToAddToActorAfterActing = null;
        List<BattlemodeEffectStrategyInstance> specialEffectsToAddToActorAfterActing = null;

        if (targetTile.occupantCtx is not BattlerOccupantCtx targetBattlerCtx) // Empty Tile 
        {
            ResolveRightBeforeActingEffects(queuedAction, queuedAction.actionCtx, 
                out effectsToAddToActorBeforeActing,
                out specialEffectsToAddToActorBeforeActing);
        }
        else // Tile with Occupant
        {   
            _targetBattlerCtx = targetBattlerCtx;
            queuedAction.actionCtx.SetHealViaTargetMaxHealth(targetBattlerCtx);
            ResolveRightBeforeActingEffects(queuedAction, queuedAction.actionCtx,
                out effectsToAddToActorBeforeActing,
                out specialEffectsToAddToActorBeforeActing);
        }
        
        
        if (effectsToAddToActorBeforeActing != null)
            foreach (var addEffect in effectsToAddToActorBeforeActing)
            {
                if (queuedAction.actingOccupantCtx.currentEffects
                    .Any(e => e.GetType() == addEffect.GetType()))
                {
                    queuedAction.actingOccupantCtx.MutateStacksToAlreadyExistingEffect(addEffect);
                    continue;
                }
                queuedAction.actingOccupantCtx.currentEffects.Add(addEffect);
            }

        if (specialEffectsToAddToActorBeforeActing != null)
            foreach (var addSpEffect in specialEffectsToAddToActorBeforeActing)
            {
                if (queuedAction.actingOccupantCtx.specialEffects
                    .Any(e => e.GetType() == addSpEffect.GetType()))
                {
                    queuedAction.actingOccupantCtx.MutateStacksToAlreadyExistingEffect(addSpEffect);
                    continue;
                }
                queuedAction.actingOccupantCtx.specialEffects.Add(addSpEffect);
            }
        
        // ACTING
        if(_targetBattlerCtx != null)
            _targetBattlerCtx.ActedUponByAction(queuedAction, queuedAction.actionCtx); // Acting on Target
        else if(queuedAction.lookingForTarget == BattlemodeActionConfig.Role.EmptyTile 
        && targetTile.IsEmptyTile() 
        && queuedAction.actionCtx.cfg.moveToSelectedEmptyTile)
            targetTile.TransferInOccupant(fromTile, BattleTracker.Instance.grid);
    
        
        ResolveAfterActorEffects(queuedAction, queuedAction.actionCtx);
        
        // Only removes status effects, not special effects
        queuedAction.actingOccupantCtx.currentEffects.RemoveAll(effect => effect.stacksTotal <= 0);
        
        if(queuedAction.actionCtx.cfg.actionQualifier.HasFlag(BattlemodeActionConfig.ActionQualifier.UnExhuastAll))
            queuedAction.actingOccupantCtx.exhuastedActionCfgs.Clear();
            
        if (queuedAction.actionCtx.cfg.actionQualifier.HasFlag(BattlemodeActionConfig.ActionQualifier.Exhuast))
            queuedAction.actingOccupantCtx.exhuastedActionCfgs.Add(queuedAction.actionCtx.cfg);
        
    }


    
    
    void ResolveRightBeforeActingEffects(QueuedAction queuedActionCtx, BattlemodeActionCtx actingActionCtx, 
        out List<BattlemodeEffectStrategyInstance> effectsToAddToActorRightBeforeActing,
        out List<BattlemodeEffectStrategyInstance> specialEffectsToAddToActorRightBeforeActing)
    {
        effectsToAddToActorRightBeforeActing = null;
        specialEffectsToAddToActorRightBeforeActing = null;
        
        if (queuedActionCtx.targetTile.occupantCtx is not BattlerOccupantCtx) return;
        
        // Resolve Effects that resolve before mutation
        foreach (var effect in queuedActionCtx.actingOccupantCtx.currentEffects)
            effect.ResolveEffectRightBeforeActing(queuedActionCtx.actingOccupantCtx, queuedActionCtx.actionCtx);
                
        foreach (var spEffect in queuedActionCtx.actingOccupantCtx.specialEffects)
            spEffect.ResolveEffectRightBeforeActing(queuedActionCtx.actingOccupantCtx, queuedActionCtx.actionCtx);
        
        effectsToAddToActorRightBeforeActing = new List<BattlemodeEffectStrategyInstance>();
        foreach(var effectCfg in queuedActionCtx.actionCtx.cfg.effectsToApplyToSelf)
            if (effectCfg.addOccurance == BattlemodeEffectConfigInstance.AddOccurance.BeforeAction)
            {
                var effect = effectCfg.CreateNewEffectInstance();
                if (effect.isSpecial)
                {
                    specialEffectsToAddToActorRightBeforeActing ??= new List<BattlemodeEffectStrategyInstance>();
                    specialEffectsToAddToActorRightBeforeActing.Add(effect);
                }
                else
                {
                    effectsToAddToActorRightBeforeActing ??= new List<BattlemodeEffectStrategyInstance>();
                    effectsToAddToActorRightBeforeActing.Add(effectCfg.CreateNewEffectInstance());
                }

            }
    }
    
    

    void ResolveAfterActorEffects(QueuedAction queuedActionCtx, BattlemodeActionCtx actingActionCtx)
    {
        if (queuedActionCtx.targetTile.occupantCtx is not BattlerOccupantCtx battlerCtx) return;
        
        
        foreach (var effect in battlerCtx.currentEffects)
            effect.ResolveEffectAfterActing(battlerCtx, actingActionCtx);
        foreach (var spEffect in battlerCtx.specialEffects)
            spEffect.ResolveEffectAfterActing(battlerCtx, actingActionCtx);
        
        List<BattlemodeEffectStrategyInstance> effectsToAddToActorAfterActing = null;
        List<BattlemodeEffectStrategyInstance> specialEffectsToAddToActorAfterActing = null;
        
        effectsToAddToActorAfterActing = new List<BattlemodeEffectStrategyInstance>();
        foreach(var effectCfg in queuedActionCtx.actionCtx.cfg.effectsToApplyToSelf)
            if (effectCfg.addOccurance == BattlemodeEffectConfigInstance.AddOccurance.AfterAction)
            {
                var effect = effectCfg.CreateNewEffectInstance();
                if (effect.isSpecial)
                {
                    specialEffectsToAddToActorAfterActing ??= new List<BattlemodeEffectStrategyInstance>();
                    specialEffectsToAddToActorAfterActing.Add(effect);
                }
                else
                {
                    effectsToAddToActorAfterActing ??= new List<BattlemodeEffectStrategyInstance>();
                    effectsToAddToActorAfterActing.Add(effectCfg.CreateNewEffectInstance());
                }

            }
        
        
        if (effectsToAddToActorAfterActing != null)
            foreach (var addEffect in effectsToAddToActorAfterActing)
            {
                if (queuedActionCtx.actingOccupantCtx.currentEffects
                    .Any(e => e.GetType() == addEffect.GetType()))
                {
                    queuedActionCtx.actingOccupantCtx.MutateStacksToAlreadyExistingEffect(addEffect);
                    continue;
                }
                queuedActionCtx.actingOccupantCtx.currentEffects.Add(addEffect);
            }

        if (specialEffectsToAddToActorAfterActing != null)
        {
            Debug.Log("[ACTOR] Adding Special Effects: " + specialEffectsToAddToActorAfterActing.Count);
            foreach (var addSpEffect in specialEffectsToAddToActorAfterActing)
            {
                if (queuedActionCtx.actingOccupantCtx.specialEffects
                    .Any(sp => sp.GetType() == addSpEffect.GetType()))
                {
                    Debug.Log("[ACTOR] Special Effect always already exists, mutating...");
                    queuedActionCtx.actingOccupantCtx.MutateStacksToAlreadyExistingEffect(addSpEffect);
                    continue;
                }
                queuedActionCtx.actingOccupantCtx.specialEffects.Add(addSpEffect);
            }
        }
    }
}

public sealed class SelfRoleTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Self;
    public BattleTile selfTile;
    public override void ClearTarget() => selfTile = null;
    public override void ActedUponBy(QueuedAction queuedActionCtx, BattleTile fromTile) 
        => ActedUponByImplementation(queuedActionCtx, selfTile, fromTile);
}

public sealed class AllyRoleTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Ally;
    public BattleTile allyTile;
    public override void ClearTarget() => allyTile = null;
    public override void ActedUponBy(QueuedAction queuedActionCtx, BattleTile fromTile) 
        => ActedUponByImplementation(queuedActionCtx, allyTile, fromTile);
}

public sealed class EnemyRoleTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Enemy;
    public BattleTile enemyTile;
    public override void ClearTarget() => enemyTile = null;
    public override void ActedUponBy(QueuedAction queuedActionCtx, BattleTile fromTile) 
        => ActedUponByImplementation(queuedActionCtx, enemyTile, fromTile);
}

// For now this is per target for every target, not for every target, mabye change later
public sealed class AllyTeamTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Team;
    public List<BattleTile> allyTiles = new();
    public override void ClearTarget() => allyTiles.Clear();
    public override void ActedUponBy(QueuedAction queuedActionCtx, BattleTile fromTile)
    {
        foreach (var tile in allyTiles)
            ActedUponByImplementation(queuedActionCtx, tile, fromTile);
    }
}

// For now this is per target for every target, not for every target, mabye change later
public sealed class EnemyTeamTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.Team;
    public List<BattleTile> enemyTiles = new();
    public override void ClearTarget() => enemyTiles.Clear();
    public override void ActedUponBy(QueuedAction queuedActionCtx, BattleTile fromTile)
    {
        foreach (var tile in enemyTiles)
            ActedUponByImplementation(queuedActionCtx, tile, fromTile);
    }
}

public sealed class EmptyTileTarget : RoleTarget
{
    public override BattlemodeActionConfig.Role role => BattlemodeActionConfig.Role.EmptyTile;
    public BattleTile emptyTile;
    public override void ClearTarget() => emptyTile = null;
    public override void ActedUponBy(QueuedAction queuedActionCtx, BattleTile fromTile) 
        => ActedUponByImplementation(queuedActionCtx, emptyTile, fromTile);
}