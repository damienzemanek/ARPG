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
                UseActionOnTargetTile(tile, queuedPlayerAction.lookingForTarget);
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

    public void UseActionOnTargetTile(BattleTile tile, BattlemodeActionConfig.Role targetRole)
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
    

    public void QueueAction(BattleTile tile, BattlemodeActionCtx actionCtx)
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
        gridWorld.HideUnoccupiedTiles_GetAvaliableTargetTiles(
            battleConfig: currentBattleConfig,
            actionTarget: queuedPlayerAction.lookingForTarget,
            queuedOccupant: queuedPlayerAction.targetTile.occupantCtx.cfg,
            out var avaliableTargetsTiles);
        
        player.ZoomOutToSelectQueuedAction();
        actionsDisplay.HideDisplay();
        GridWorld.InRangeCheckCtx inRangeCheckCtx = new()
        {
            upRange = actionCtx.cfg.upRange,
            fwdRange = actionCtx.cfg.fwdRange,
            downRange = actionCtx.cfg.downRange,
            myRow = tile.row,
            myCol = tile.col,
        };
        gridWorld.ShowInRangeTiles(inRangeCheckCtx, avaliableTargetsTiles);

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
        gridWorld.GetBattlerTiles(out var opponentTiles, out var playerTiles);
        opponentAI.AttackAll(opponentTiles, playerTiles, gridWorld);
    }
    
}
