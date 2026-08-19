using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class BattleTracker : DesignPatterns.CreationalPatterns.Singleton<BattleTracker>
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
        public BattlemodeActionCtx actionCtx;
        public BattlerOccupantCtx actingOccupantCtx;
        public BattleTile targetTile;
        public BattlemodeActionConfig.Role lookingForTarget;  
        
        public SelfRoleTarget targSelfSlot;
        public AllyRoleTarget targAllySlot;
        public EnemyRoleTarget targEnemySlot;
        public AllyTeamTarget targAllyTeamSlot;
        public EnemyTeamTarget targEnemyTeamSlot;
        public EmptyTileTarget targEmptyTileSlot;
        
        public void Init()
        {
            targSelfSlot = new();
            targAllySlot = new();
            targEnemySlot = new();
            targAllyTeamSlot = new();
            targEnemyTeamSlot = new();
            targEmptyTileSlot = new();
        }
    }
    
    

    protected override void Awake() { base.Awake(); InitializeBattle(); }

    void InitializeBattle()
    {
        gridWorld.PopulateGrid(currentBattleConfig);
        queuedPlayerAction.Init();
        StartPlayerTurn();
    }
    

    public void SelectTile(BattleTile tile, bool backSelect = false)
    {
        Debug.Log("Selecting tile 1");
        if (queuedPlayerAction.hasQueuedAction)
        {
            if (backSelect) // Back Select
            {
                // Reselect Previous Tile (Queued State -> Zoom State)
                gridWorld.UnSelectAll();
                ClearRoleTargets();
                player.ZoomIntoTile(tile, DisplayActionsUI);
                queuedPlayerAction.hasQueuedAction = false;
                queuedPlayerAction.actionCtx = null;
            }
            else // Select Target Tile
            {
                UseActionOnTargetTile(tile, queuedPlayerAction.lookingForTarget);
            }
            return;
        }
        Debug.Log("Selecting tile 2");
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
                queuedPlayerAction.targSelfSlot.selfTile = tile; 
                selectedTarget = queuedPlayerAction.targSelfSlot;
                break;
            case BattlemodeActionConfig.Role.Ally:
                queuedPlayerAction.targAllySlot.allyTile = tile; 
                selectedTarget = queuedPlayerAction.targAllySlot;
                break;
            case BattlemodeActionConfig.Role.Enemy:
                queuedPlayerAction.targEnemySlot.enemyTile = tile; 
                selectedTarget = queuedPlayerAction.targEnemySlot;
                break;
            case BattlemodeActionConfig.Role.Team:
                queuedPlayerAction.targAllyTeamSlot.allyTiles.Add(tile); 
                selectedTarget = queuedPlayerAction.targAllyTeamSlot;
                break;
            case BattlemodeActionConfig.Role.EnemyTeam:
                queuedPlayerAction.targEnemyTeamSlot.enemyTiles.Add(tile); 
                selectedTarget = queuedPlayerAction.targEnemyTeamSlot;
                break;
            case BattlemodeActionConfig.Role.EmptyTile:
                queuedPlayerAction.targEmptyTileSlot.emptyTile = tile; 
                selectedTarget = queuedPlayerAction.targEmptyTileSlot;
                break;
        }

        // Should always be the case
        if (queuedPlayerAction.actingOccupantCtx != null && queuedPlayerAction.actingOccupantCtx is CharacterOccupantCtx actingCharacterCtx)
            actingCharacterCtx.currentAP -= actionsDisplay.GetCurrentlySelectedAPCost();
        else 
            Debug.LogWarning("Trying to use AP on non-character occupant.");

        selectedTarget?.ActUponTarget(queuedPlayerAction);
        CoroutineRunner.Instance.RunMethodDelayed(() =>
        {
            queuedPlayerAction.hasQueuedAction = false;
            queuedPlayerAction.actingOccupantCtx.PostResolveActingEffects(queuedPlayerAction.actionCtx);
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
        Debug.Log("[BattleTracker] Queueing action");
        if (tile.occupantCtx is CharacterOccupantCtx actingCharacterCtx)
            if (actingCharacterCtx.currentAP < (actionCtx.ap + actionCtx.apDelta))
                return;
        
        ClearRoleTargets();
        queuedPlayerAction.hasQueuedAction = true;
        queuedPlayerAction.actionCtx = actionCtx;
        queuedPlayerAction.targetTile = tile;
        queuedPlayerAction.lookingForTarget = actionCtx.cfg.roleTarget;
        Debug.Log("Queued action: " + actionCtx.cfg.name + " on tile: [" + tile.row + ", " + tile.col + "]");
        
        // Target Selection
        GridWorld.InRangeCheckCtx inRangeCheckCtx = new()
        {
            upRange = actionCtx.cfg.upRange,
            fwdRange = actionCtx.cfg.fwdRange,
            downRange = actionCtx.cfg.downRange,
            myRow = tile.row,
            myCol = tile.col,
        };

        
        var inRangeTiles = gridWorld.GetInRangeTiles(inRangeCheckCtx, queuedPlayerAction.lookingForTarget);
        inRangeTiles.ForEach(t => t.InRange());
        
        var targetTiles = gridWorld.GetAvaliableTargetTiles(
            battleConfig: currentBattleConfig,
            actionTarget: queuedPlayerAction.lookingForTarget,
            inRangeTiles);
        
        player.ZoomOutToSelectQueuedAction();
        actionsDisplay.HideDisplay();

        foreach (var t in targetTiles)
        {
            t.ResetSpecialTargetConsiderations();
            t.SpecialTargetConsiderations(queuedPlayerAction.lookingForTarget);
        }

    }


    public void ClearRoleTargets()
    {
        queuedPlayerAction.targSelfSlot.ClearTarget();
        queuedPlayerAction.targAllySlot.ClearTarget();
        queuedPlayerAction.targEnemySlot.ClearTarget();
        queuedPlayerAction.targAllyTeamSlot.ClearTarget();
        queuedPlayerAction.targEnemyTeamSlot.ClearTarget();
        queuedPlayerAction.targEmptyTileSlot.ClearTarget();
        Debug.Log("Cleared role targets");
    }

    public void DisplayActionsUI()
    {
        actionsDisplay.ShowDisplay(currentlySelectedTile);
    }
    
    public void HideActionsUI() => actionsDisplay.HideDisplay();

    public void StartPlayerTurn()
    {
        currentTurn = Turn.Player;
        gridWorld.GetBattlerTiles(out var opponentTiles, out _);
        opponentAI.QueueActions(opponentTiles);
    }

    public void EndPlayerTurn()
    {
        currentTurn = Turn.Enemy;
        actionsDisplay.HideDisplay();
        gridWorld.GetBattlerTiles(out _, out var playerTiles);
        opponentAI.AttackAll(playerTiles, gridWorld);
    }
    
}
