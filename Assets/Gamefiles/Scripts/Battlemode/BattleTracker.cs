using System.Collections.Generic;
using DesignPatterns.CreationalPatterns;
using Sirenix.OdinInspector;
using UnityEngine;

public class BattleTracker : Singleton<BattleTracker>
{
    bool _hasQueuedAction => queuedPlayerAction.hasQueuedAction;
    
    public enum Turn
    {
        Player,
        Enemy
    }
    
    [ReadOnly] public BattleTile currentlySelectedTile;
    [Required] public GridWorld gridWorld;
    [Required] public BattlemodePlayerInstance player;
    [Required] public BattlemodeActionsDisplay actionsDisplay;
    [Required] public OpponentAI opponentAI;
    [ReadOnly] public Turn currentTurn;

    [ReadOnly, ShowIf("_hasQueuedAction"), ShowInInspector]
    public QueuedAction queuedPlayerAction = new();
    [ReadOnly, ShowInInspector] BattleConfig currentBattleConfig 
        => SessionData.HasInstance && SessionData.Instance.currentBattleConfig != null 
        ? SessionData.Instance.currentBattleConfig 
        : null;
    
    public class QueuedAction
    {
        public bool hasQueuedAction;
        public BattlemodeActionCtx queuedActionCtx;
        public BattlerOccupantCtx actingOccupantCtx;
        public BattleTile targetTile;
        public BattlemodeActionConfig.Role lookingForTarget;  
        
        public SelfRoleTarget targSelf;
        public AllyRoleTarget targAlly;
        public EnemyRoleTarget targEnemy;
        public AllyTeamTarget targAllyTeam;
        public EnemyTeamTarget targEnemyTeam;
        
        public void Init()
        {
            targSelf = new();
            targAlly = new();
            targEnemy = new();
            targAllyTeam = new();
            targEnemyTeam = new();
        }
    }
    
    

    protected override void Awake() { base.Awake(); InitializeBattle(); }

    void InitializeBattle()
    {
        gridWorld.PopulateGrid(currentBattleConfig);
        currentTurn = Turn.Player;
        queuedPlayerAction.Init();
    }

    public void SelectTile(BattleTile tile, bool backSelect = false)
    {
        if (queuedPlayerAction.hasQueuedAction)
        {
            if (backSelect) // Back Select
            {
                // Reselect Previous Tile (Queued State -> Zoom State)
                gridWorld.UnSelectAll();
                ClearRoleTargets();
                player.ZoomIntoTile(tile, DisplayActionsUI);
                queuedPlayerAction.hasQueuedAction = false;
                queuedPlayerAction.queuedActionCtx = null;
            }
            else // Select Target Tile
            {
                SelectTargetTile(tile, queuedPlayerAction.lookingForTarget);
            }
            return;
        }
        gridWorld.UnSelectAll();
        currentlySelectedTile = tile;
        queuedPlayerAction.actingOccupantCtx = null;
        if(currentlySelectedTile.occupantCtx is BattlerOccupantCtx battlerOccupantCtx)
            queuedPlayerAction.actingOccupantCtx = battlerOccupantCtx;
        HideActionsUI();
        player.ZoomIntoTile(currentlySelectedTile, DisplayActionsUI);
    }

    public void SelectTargetTile(BattleTile tile, BattlemodeActionConfig.Role targetRole)
    {
        RoleTarget selectedTarget = null;
        switch (targetRole)
        {
            case BattlemodeActionConfig.Role.Self:
                queuedPlayerAction.targSelf.tile = tile; 
                selectedTarget = queuedPlayerAction.targSelf;
                break;
            case BattlemodeActionConfig.Role.Ally:
                queuedPlayerAction.targAlly.tile = tile; 
                selectedTarget = queuedPlayerAction.targAlly;
                break;
            case BattlemodeActionConfig.Role.Enemy:
                queuedPlayerAction.targEnemy.tile = tile; 
                selectedTarget = queuedPlayerAction.targEnemy;
                break;
            case BattlemodeActionConfig.Role.Team:
                queuedPlayerAction.targAllyTeam.tiles.Add(tile); 
                selectedTarget = queuedPlayerAction.targAllyTeam;
                break;
            case BattlemodeActionConfig.Role.EnemyTeam:
                queuedPlayerAction.targEnemyTeam.tiles.Add(tile); 
                selectedTarget = queuedPlayerAction.targEnemyTeam;
                break;
        }

        // Should always be the case
        if (queuedPlayerAction.actingOccupantCtx is CharacterOccupantCtx actingCharacterCtx)
            actingCharacterCtx.currentAP -= actionsDisplay.GetCurrentlySelectedAPCost();
        else 
            Debug.LogWarning("Trying to use AP on non-character occupant.");

        selectedTarget?.ActUponTarget(queuedPlayerAction);
        CoroutineRunner.Instance.RunMethodDelayed(() =>
        {
            queuedPlayerAction.hasQueuedAction = false;
            queuedPlayerAction.actingOccupantCtx.PostResolveActingEffects(queuedPlayerAction.queuedActionCtx);
            UnSelectAll();
            gridWorld.UpdateGrid(); // Updates AP and HP through transient stats
        }, 0.1f);
    }
    
    public void UnSelectAll()
    {
        Debug.Log("Unselecting all tiles and clearing role targets.");
        currentlySelectedTile = null;
        queuedPlayerAction.actingOccupantCtx = null;
        gridWorld.UnSelectAll();
        HideActionsUI();
        ClearRoleTargets();
    }
    

    public void UseAndQueueAction(BattleTile tile, BattlemodeActionCtx actionCtx)
    {
        if (tile.occupantCtx is CharacterOccupantCtx actingCharacterCtx)
            if (actingCharacterCtx.currentAP < (actionCtx.ap + actionCtx.apDelta))
                return;
        
        ClearRoleTargets();
        queuedPlayerAction.hasQueuedAction = true;
        queuedPlayerAction.queuedActionCtx = actionCtx;
        queuedPlayerAction.targetTile = tile;
        queuedPlayerAction.lookingForTarget = actionCtx.cfg.roleTarget;
        
        // Target Selection
        gridWorld.HideNotContaining(
            battleConfig: currentBattleConfig,
            actionTarget: queuedPlayerAction.lookingForTarget,
            queuedOccupant: queuedPlayerAction.targetTile.occupantCtx.cfg);
        
        player.ZoomOutToSelectQueuedAction();
        actionsDisplay.HideDisplay();
    }


    public void ClearRoleTargets()
    {
        queuedPlayerAction.targSelf.ClearTarget();
        queuedPlayerAction.targAlly.ClearTarget();
        queuedPlayerAction.targEnemy.ClearTarget();
        queuedPlayerAction.targAllyTeam.ClearTarget();
        queuedPlayerAction.targEnemyTeam.ClearTarget();
    }

    public void DisplayActionsUI()
    {
        actionsDisplay.ShowDisplay(currentlySelectedTile);
    }
    
    public void HideActionsUI() => actionsDisplay.HideDisplay();

    public void EndPlayerTurn()
    {
        currentTurn = Turn.Enemy;
        actionsDisplay.HideDisplay();
        gridWorld.GetTiles(out var opponentTiles, out var playerTiles);
        opponentAI.AttackAll(opponentTiles, playerTiles, gridWorld);
    }


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
    
}
