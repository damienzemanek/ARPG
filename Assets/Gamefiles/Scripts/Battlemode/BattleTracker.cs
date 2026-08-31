using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using NUnit.Framework;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using static BattlemodeActionConfig;
using static GridWorld;

public class BattleTracker : DesignPatterns.CreationalPatterns.Singleton<BattleTracker>
{
    bool _hasQueuedAction => queuedPlayerAction.hasQueuedAction;
    
    public enum Turn
    {
        Player,
        Enemy,
        Transitioning
    }

    public FadeSettings usingActionFadeSettings;
    [Required] public GameObject usingActionFadeTarg;
    public float usingActionFadeAlpha = 0.6f;
    public bool isUsingAnAction = false;
    
    [ReadOnly] public BattleTile fromTile;
    [FormerlySerializedAs("gridWorld")] [Required] public GridWorld grid;
    [Required] public BattlemodePlayerInstance player;
    [Required] public BattlemodeActionsDisplay actionsDisplay;
    [Required] public ActionUserViewer actionUserViewer;
    [Required] public TurnDisplay turnDisplay;
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
        public BattleTile actorTile;
        public BattleTile targetTile;
        public Role lookingForTarget;  
        
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
            hasQueuedAction = false;
            actionCtx = null;
            actingOccupantCtx = null;
            actorTile = null;
            lookingForTarget = Role.None;
        }
    }
    
    

    protected override void Awake() { base.Awake(); InitializeBattle(); }

    void InitializeBattle()
    {
        grid.PopulateGrid(currentBattleConfig);
        grid.DesignateTileRanks();
        queuedPlayerAction.Init();
        actionsDisplay.ShowEndTurnBtn(false);
        EndEnemyTurnOrStartBattle(true);
        FadeEX.ResetFade(usingActionFadeSettings, false, usingActionFadeTarg);
    }
    


    public void SelectTile(BattleTile tile, bool backSelect = false)
    {
        Debug.Log("Selecting tile 1");
        if (queuedPlayerAction.hasQueuedAction)
        {
            if (backSelect) // Back Select
            {
                // Reselect Previous Tile (Queued State -> Zoom State)
                grid.UnSelectAll();
                ClearRoleTargets();
                player.ZoomIntoTile(tile, DisplayActionsUI);
                queuedPlayerAction.hasQueuedAction = false;
                queuedPlayerAction.actionCtx = null;
            }
            else if (queuedPlayerAction.actingOccupantCtx.exhuastedActionCfgs.Contains(queuedPlayerAction.actionCtx.cfg))
                Debug.Log("Action has been exhuasted.");
            else // Normal Select Target Tile
            {
                StartCoroutine(C_UseActionOnTargetTile(tile, queuedPlayerAction.lookingForTarget));
            }
            return;
        }
        Debug.Log("Selecting tile 2");
        grid.UnSelectAll();
        fromTile = tile;
        queuedPlayerAction.actingOccupantCtx = null;
        if(fromTile.occupantCtx is BattlerOccupantCtx battlerOccupantCtx)
            queuedPlayerAction.actingOccupantCtx = battlerOccupantCtx;
        HideActionsUI();
        player.ZoomIntoTile(fromTile, DisplayActionsUI);
    }

    public IEnumerator C_UseActionOnTargetTile(BattleTile targetTile, Role targetRole)
    {
        isUsingAnAction = true;
        RoleTarget selectedTarget = null;
        switch (targetRole)
        {
            case Role.Self:
                queuedPlayerAction.targSelfSlot.selfTile = targetTile; 
                selectedTarget = queuedPlayerAction.targSelfSlot;
                break;
            case Role.Ally:
                queuedPlayerAction.targAllySlot.allyTile = targetTile; 
                selectedTarget = queuedPlayerAction.targAllySlot;
                break;
            case Role.Enemy:
                queuedPlayerAction.targEnemySlot.enemyTile = targetTile; 
                selectedTarget = queuedPlayerAction.targEnemySlot;
                break;
            case Role.Team:
                queuedPlayerAction.targAllyTeamSlot.allyTiles.Add(targetTile); 
                selectedTarget = queuedPlayerAction.targAllyTeamSlot;
                break;
            case Role.EnemyTeam:
                queuedPlayerAction.targEnemyTeamSlot.enemyTiles.Add(targetTile); 
                selectedTarget = queuedPlayerAction.targEnemyTeamSlot;
                break;
            case Role.EmptyTile:
                queuedPlayerAction.targEmptyTileSlot.emptyTile = targetTile; 
                selectedTarget = queuedPlayerAction.targEmptyTileSlot;
                break;
        }

        // Should always be the case
        if (queuedPlayerAction.actingOccupantCtx != null && queuedPlayerAction.actingOccupantCtx is CharacterOccupantCtx actingCharacterCtx)
            actingCharacterCtx.currentAP -= actionsDisplay.GetCurrentlySelectedAPCost();
        else 
            Debug.LogWarning("Trying to use AP on non-character occupant.");

        Debug.Log("Actor: " + queuedPlayerAction.actingOccupantCtx.obj.name + ", Target: " + targetTile.occupantCtx.obj.name);
        
        selectedTarget?.ActedUponBy(queuedPlayerAction, fromTile);

        yield return CoroutineRunner.Instance.RunAllMethodsAtOnce(
            FadeEX.C_FadeToAlphaValueOf(usingActionFadeSettings, usingActionFadeTarg, usingActionFadeAlpha),
            actionUserViewer.C_UseActionUserViewer(
                queuedPlayerAction.actingOccupantCtx.obj, 
                targetTile.occupantCtx.obj));

        
        for (int currentHitcount = 2; currentHitcount <= queuedPlayerAction.actionCtx.cfg.hitCount; currentHitcount++)
        {
            // TODO: delay to be dependant on a dict storing moveanims, which itself is stored on the battlerconfig
            queuedPlayerAction.actionCtx.currentHitCount = currentHitcount;

            yield return new WaitForSeconds(0.1f); // delay between attack counts
            selectedTarget?.ActedUponBy(queuedPlayerAction, fromTile);
            
        }
        yield return new WaitForSeconds(0.1f); // finish attacking

        // Post Movements
        HandleMovement(fromTile.row, fromTile.col, queuedPlayerAction.actionCtx.movementCfgInstanced);


        // delay for fade back in
        yield return FadeEX.C_FadeToAlphaValueOf(usingActionFadeSettings, usingActionFadeTarg, 0f);
        
        // delay for status effects to resolve, eventually also for status add anims
        yield return new WaitForSeconds(0.1f);

        queuedPlayerAction.hasQueuedAction = false;
        queuedPlayerAction.actingOccupantCtx.ResolveAfterActingEffects(queuedPlayerAction.actionCtx);
        UnSelectAll();
        grid.UpdateGrid(); // Updates AP and HP through transient stats
        isUsingAnAction = false;
    }
    record struct MovementPathPoint
    {
        public BattleTile tile;
        public OccupantCtx occupantCtx;
    }
    
    void HandleMovement(int myRow, int myCol, MovementCfg move)
    {
        if(move.direction == MovementDirection.None) return;
        if (move.amount <= 0) return;

        var dir = move switch
        {
            _ when move.direction == MovementDirection.Right => TileDirection.Right,
            _ when move.direction == MovementDirection.Left => TileDirection.Left,
            _ when move.direction == MovementDirection.Up => TileDirection.Up,
            _ when move.direction == MovementDirection.Down => TileDirection.Down,
            _ => TileDirection.DiagDownLeft
        };
        

        var originTile = fromTile;
        var origMover = originTile.occupantCtx;
        if (origMover == null) return;

        // Distance 1: normal move or direct swap.
        if (move.amount == 1)
        {
            var destinationTile = grid.GetTileToThe(dir, myRow, myCol);

            if (destinationTile == null) return;
            if (destinationTile.section == BattlefieldSection.Right) return;

            if (destinationTile.occupantCtx != null) destinationTile.SwapOccupants(originTile, grid);
            else destinationTile.TransferInOccupant(originTile, grid);
            return;
        }

        var savePath = new List<MovementPathPoint>();

        // Snapshot the entire path BEFORE modifying any tiles.
        for (int i = 0; i <= move.amount; i++)
        {
            var tile = grid.GetTileToThe(dir, myRow, myCol, i);

            if (tile == null) break;
            if (tile.section == BattlefieldSection.Right) break;
            
            savePath.Add(new MovementPathPoint
            {
                tile = tile,
                occupantCtx = tile.occupantCtx
            });
        }

        var actualAmount = savePath.IndexOf(savePath[^1]);
        

        int a = 1;
        foreach (var p in savePath)
        {
            Debug.Log($"SavePath Tile {a}: [" + p.tile.row + ", " + p.tile.col + "] " + (p.occupantCtx != null ? p.occupantCtx.cfg.name : "Empty"));
            a++;
        }
        
        // Destination is empty: ordinary movement.
        if (savePath[^1].occupantCtx == null)
        {
            savePath[^1].tile.TransferInOccupant(originTile, grid);
            return;
        }

        // Build the final arrangement.
        var newPath = new MovementPathPoint[actualAmount + 1];

        // Mover goes into the destination.
        newPath[^1] = new MovementPathPoint
        {
            tile = savePath[actualAmount].tile,
            occupantCtx = origMover
        };

        // Existing occupants get compressed toward the origin.
        
        int newIndex = actualAmount -1;

        for (int i = actualAmount; i > 0; i--)
        {
            var occupant = savePath[i].occupantCtx;

            if (occupant == null)
                continue;

            newPath[newIndex] = new MovementPathPoint
            {
                tile = savePath[newIndex].tile,
                occupantCtx = occupant
            };

            newIndex--;
        }

        // Clear all affected tiles AFTER the snapshot/final arrangement is built.
        foreach (var point in savePath)
        {
            point.tile.Clear();
            point.tile.occupantCtx = null;
        }

        // Apply final arrangement.
        foreach (var point in newPath)
        {
            if (point.occupantCtx == null)
                continue;

            point.tile.occupantCtx = point.occupantCtx;
            point.occupantCtx.newTilePosition = point.tile;
            point.occupantCtx.obj.transform.position =
                point.tile.transform.position + point.tile.occupantSpawnOffset;

            point.tile.display.SetActive(true);
        }

        grid.UpdateGrid();
    }
        
    public void UnSelectAll()
    {
        Debug.Log("Unselecting all tiles and clearing role targets.");
        fromTile = null;
        queuedPlayerAction.actingOccupantCtx = null;
        grid.UnSelectAll();
        HideActionsUI();
        ClearRoleTargets();
    }
    
    public void QueuePlayerAction(BattleTile tile, BattlemodeActionCtx actionCtx)
    {
        Debug.Log("[BattleTracker] Queueing action");
        if (tile.occupantCtx is CharacterOccupantCtx actingCharacterCtx)
        {
            Debug.Log("[BattleTracker] Checking AP : " + actingCharacterCtx.currentAP + " vs " + (actionCtx.ap + actionCtx.apDelta) + "");
            if (actingCharacterCtx.currentAP < (actionCtx.ap + actionCtx.apDelta)) return;
        }
        
        
        ClearRoleTargets();
        queuedPlayerAction.hasQueuedAction = true;
        queuedPlayerAction.actionCtx = actionCtx;
        queuedPlayerAction.actorTile = tile;
        queuedPlayerAction.lookingForTarget = actionCtx.cfg.roleTarget;
        Debug.Log("Queued action: " + actionCtx.cfg.name + " on tile: [" + tile.row + ", " + tile.col + "]");
        
        foreach(var effect in queuedPlayerAction.actingOccupantCtx.currentEffects)
            effect.ResolveEffectRightBeforeQueued(queuedPlayerAction.actingOccupantCtx, queuedPlayerAction.actionCtx);
        foreach(var spEffect in queuedPlayerAction.actingOccupantCtx.specialEffects)
            spEffect.ResolveEffectRightBeforeQueued(queuedPlayerAction.actingOccupantCtx, queuedPlayerAction.actionCtx);
        
        var inRangeTiles = grid.GetInRangeTiles(actionCtx.targetingCfgInstanced, tile);
        
        if (actionCtx.cfg.moveToSelectedEmptyTile)
            inRangeTiles = grid.ExcludeSection(BattlefieldSection.Right, inRangeTiles);

        Debug.Log(string.Join(", ", inRangeTiles.Select(t => $"[{t.row}, {t.col}]")));

        foreach (var t in inRangeTiles)
            t.InRange();
        
        var targetTiles = grid.GetAvaliableTargetTiles(
            myTarget: queuedPlayerAction.lookingForTarget,
            inRangeTiles,
            myTile: tile);
        
        
        player.ZoomOutToSelectQueuedAction();
        actionsDisplay.HideDisplay();

        foreach (var t in targetTiles)
        {
            t.SetUnselectable(false);
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
        actionsDisplay.ShowDisplay(fromTile);
    }

    public void HideActionsUI()
    {
        actionsDisplay.HideDisplay();
        actionsDisplay.ShowEndTurnBtn(true);
    }

    public void StartPlayerTurn(bool firstTurn = false) 
    {
        Debug.Log("Starting player turn");
        currentTurn = Turn.Player;
        grid.GetBattlerTiles(out var opponentTiles, out var playerTiles);
        actionsDisplay.ShowEndTurnBtn(true);
        
        if(opponentTiles.Count > 0) opponentAI.QueueOpponentActionsAtTurnStart(opponentTiles);
        else
        {
            Debug.Log("All Enemies are dead or out of battle.");
            return;
        }

        if (!firstTurn) //----------------- Regular Turns
        {
            // Resolve end of turn effects for opponents
            foreach (var opponentTile in opponentTiles)
            {
                if (opponentTile.occupantCtx is not EnemyOccupantCtx enemyOccupantCtx) continue;
                enemyOccupantCtx.ResolveAfterTurnEndsEffects();
            }
            
            Debug.Log("Starting to Resolve End of Turn effects for players.");
            // Resolve End of Turn effects for player
            foreach (var playerTile in playerTiles)
            {
                if (playerTile.occupantCtx is not CharacterOccupantCtx characterOccupantCtx) continue;
                Debug.Log("Resolve End of Turn Effect for player occupant:" + characterOccupantCtx.cfg.name);
                characterOccupantCtx.ResolveAfterTurnEndsEffects();
            }
        }
        else  //-------------------------- Start of Battle
        {
            foreach (var opponentTile in opponentTiles)
            {
                if (opponentTile.occupantCtx is not EnemyOccupantCtx enemyOccupantCtx) continue;
                enemyOccupantCtx.ResolveStartOfBattleOpponentEffects(enemyOccupantCtx.queuedActions);
            }
            
            foreach (var playerTile in playerTiles)
            {
                if (playerTile.occupantCtx is not CharacterOccupantCtx characterOccupantCtx) continue;
                characterOccupantCtx.ResolveStartOfBattlePlayerEffects(actionsDisplay.actionSlots);
            }
        }
    }

    public void EndPlayerTurn()
    {
        if (currentTurn == Turn.Transitioning) return;
        actionsDisplay.ShowEndTurnBtn(false);
        turnDisplay.StartTurn(Turn.Enemy, EndPlayerTurnImplementation);
    }

    public void EndEnemyTurnOrStartBattle(bool firstStart)
    {
        if (currentTurn == Turn.Transitioning) return;
        turnDisplay.StartTurn(Turn.Player, () => StartPlayerTurn(firstStart));
    }
    
    void EndPlayerTurnImplementation()
    {
        Debug.Log("Ended player turn, Starting Opponent Turn");
        grid.GetBattlerTiles(out _, out var playerTiles);
        currentTurn = Turn.Enemy;
        actionsDisplay.HideDisplay();
        actionsDisplay.ShowEndTurnBtn(false);
        opponentAI.AttackAll(playerTiles, grid, () => EndEnemyTurnOrStartBattle(false));
    }
    
    
}
