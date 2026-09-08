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
        FadeEX.ResetFade(usingActionFadeSettings, false, usingActionFadeTarg, 0);
    }
    


    public void SelectTile(BattleTile tile, bool backSelect = false)
    {
        if (isUsingAnAction) return;
        Debug.Log("Selected Tile: [" + tile.row + ", " + tile.col + "]");
       
        if (queuedPlayerAction.hasQueuedAction)
        {
            if (backSelect) SelectPreviousSelectedTile();
            else TryUseQueuedAction(queuedPlayerAction, tile);
            return;
        }

        SelectANewTile();

        // No Tile is currently selected, Selects a new tile
        void SelectANewTile() 
        {
            grid.UnSelectAll();
            queuedPlayerAction.actingOccupantCtx = null;
            if(tile.occupantCtx is BattlerOccupantCtx battlerOccupantCtx)
                queuedPlayerAction.actingOccupantCtx = battlerOccupantCtx;
            HideActionsUI();
            player.ZoomIntoTile(tile, 
                () => actionsDisplay.ShowDisplay(tile));
        }
        
        // Action is currently queued, Unqueues that action and selects the previously selected tile
        void SelectPreviousSelectedTile()
        {
            // Reselect Previous Tile (Queued State -> Zoom State)
            grid.UnSelectAll();
            ClearRoleTargets();
            player.ZoomIntoTile(queuedPlayerAction.actorTile, 
                () => actionsDisplay.ShowDisplay(queuedPlayerAction.actorTile));
            queuedPlayerAction.hasQueuedAction = false;
            queuedPlayerAction.actionCtx = null;
        }
    }
    
    // Action is currently queued, Uses the queued action on a newly selected target tile
    void TryUseQueuedAction(QueuedAction queuedAction, BattleTile targetTile)
    {
        if (queuedAction.actingOccupantCtx.exhuastedActionCfgs.Contains(queuedPlayerAction.actionCtx.cfg))
            Debug.Log("Action has been exhuasted.");
        else // Normal Select Target Tile
        {
            // actor tile already selected during Queue
            queuedPlayerAction.targetTile = targetTile;
            StartCoroutine(C_PlayerUseActionOnTargetTile(queuedPlayerAction, queuedPlayerAction.lookingForTarget));
        }
    }
    

    // PreReqs
    // - Target
    // - TargetRole
    // - QueuedAction
    // - Doesnt need a range check due to selection restrictions
    public IEnumerator C_PlayerUseActionOnTargetTile(QueuedAction queuedAction, Role targetRole)
    {
        isUsingAnAction = true;
        bool isEmptyTileTargeted = false;
        RoleTarget selectedTarget = null;
        BattleTile targetTile = queuedAction.targetTile;
        
        switch (targetRole)
        {
            case Role.Self:
                queuedAction.targSelfSlot.selfTile = targetTile; 
                selectedTarget = queuedAction.targSelfSlot;
                break;
            case Role.Ally:
                queuedAction.targAllySlot.allyTile = targetTile; 
                selectedTarget = queuedAction.targAllySlot;
                break;
            case Role.Enemy:
                queuedAction.targEnemySlot.enemyTile = targetTile; 
                selectedTarget = queuedAction.targEnemySlot;
                break;
            // No Team Targetting just yet
            // case Role.Team:
            //     queuedAction.targAllyTeamSlot.allyTiles.Add(targetTile); 
            //     selectedTarget = queuedPlayerAction.targAllyTeamSlot;
            //     break;
            // case Role.EnemyTeam:
            //     queuedAction.targEnemyTeamSlot.enemyTiles.Add(targetTile); 
            //     selectedTarget = queuedPlayerAction.targEnemyTeamSlot;
            //     break;
            case Role.EmptyTile:
                queuedAction.targEmptyTileSlot.emptyTile = targetTile; 
                selectedTarget = queuedAction.targEmptyTileSlot;
                isEmptyTileTargeted = true;
                break;
        }
        
        BattlerOccupantCtx actingOccupantCtx = queuedAction.actingOccupantCtx;
        BattlerOccupantCtx targetOccupantCtx = !isEmptyTileTargeted ? queuedAction.targetTile.occupantCtx as BattlerOccupantCtx : null;
        
        grid.HideAllDisplays();

        yield return C_UseAction(actingOccupantCtx, targetOccupantCtx, selectedTarget, queuedAction, true);
        
        grid.ClearAllTiles();

        queuedAction.hasQueuedAction = false;
        
        UnSelectAll();
        isUsingAnAction = false;
    }

    public IEnumerator C_UseAction(
        BattlerOccupantCtx actingOccupantCtx,
        BattlerOccupantCtx targetOccupantCtx,
        RoleTarget selectedTarget,
        QueuedAction queuedAction, 
        bool leftIsActor)
    {
        BattleTile actorTile = queuedAction.actorTile;
        BattleTile targetTile = queuedAction.targetTile;

        if(actingOccupantCtx is CharacterOccupantCtx actingCharacterCtx)
            actingCharacterCtx.currentAP -= actionsDisplay.GetCurrentlySelectedAPCost();
        else if (actingOccupantCtx is EnemyOccupantCtx actingEnemyCtx)
        {
            // enemy intentions subtraction
        }
        

        // First Attack
        string targetName = targetOccupantCtx?.cfg.name ?? "Empty";
        Debug.Log("Actor: " + actingOccupantCtx.obj.name + ", Target: " + targetName);
        selectedTarget?.ActedUponBy(queuedAction, actorTile);
        
        // Pre Movement Calculations
        CalculatedMovement calcMovement = new();
        CalculatedMovement targetCalcMovement = new();
        bool moves = DoesPrepareMovement(actorTile, BattlefieldSection.Right, false, queuedAction.actionCtx.movementCfgInstanced, ref calcMovement);
        bool targetMoves = DoesPrepareMovement(targetTile, BattlefieldSection.Left, true, queuedAction.actionCtx.targetMovementCfgInstanced, ref targetCalcMovement);   

        // Pre Movement Animations Location Destinations
        Vector3 actorFinalPos = actingOccupantCtx.obj.transform.position;
        Vector3 targetFinalPos = targetTile.worldOccupantSpawnPos;
        if (moves) actorFinalPos = calcMovement.newPath[^1].tile.worldOccupantSpawnPos;
        if (targetMoves) targetFinalPos = targetCalcMovement.newPath[^1].tile.worldOccupantSpawnPos;

        // Movement Animations
        if (queuedAction.actionCtx.cfg.useActionAnim)
            yield return CoroutineRunner.Instance.RunAllMethodsAtOnce(
                FadeEX.C_FadeToAlphaValueOf(usingActionFadeSettings, usingActionFadeTarg, usingActionFadeAlpha),
                actionUserViewer.C_UseActionUserViewer(
                    actingOccupantCtx.obj, actorFinalPos,
                    targetOccupantCtx.obj, targetFinalPos,
                    leftIsActor));

        
        // Actual hits (times) hit count
        for (int currentHitcount = 2; currentHitcount <= queuedAction.actionCtx.cfg.hitCount; currentHitcount++)
        {
            // TODO: delay to be dependant on a dict storing moveanims, which itself is stored on the battlerconfig
            queuedAction.actionCtx.currentHitCount = currentHitcount;

            yield return new WaitForSeconds(0.1f); // delay between attack counts
            selectedTarget?.ActedUponBy(queuedAction, actorTile);
        }
        
        // Hit Delay 
        if(queuedAction.actionCtx.cfg.useActionDelays)
            yield return new WaitForSeconds(0.1f); // finish attacking
        

        // Post Movements
        HandleMovement(actorTile.row, actorTile.col, queuedAction.actionCtx.movementCfgInstanced, ref calcMovement);
        HandleMovement(targetTile.row, targetTile.col, queuedAction.actionCtx.targetMovementCfgInstanced, ref targetCalcMovement);
        
        // delay for fade back in
        if(queuedAction.actionCtx.cfg.useActionAnim)
            yield return FadeEX.C_FadeToAlphaValueOf(usingActionFadeSettings, usingActionFadeTarg, 0f);
        
        // delay for status effects to resolve, eventually also for status add anims
        if(queuedAction.actionCtx.cfg.useActionDelays)
            yield return new WaitForSeconds(0.1f);
        
        // Actor After-Acting Resolves
        Debug.Log("Resolving After Acting: Actor");
        
        // Addition Effects
        // Note: Target is included here cause AfterEffect Resolves can still add to the AdditionalEffects
        //       lists of both Target and Actor
        actingOccupantCtx.ActorResolveAfterActingEffects(queuedAction.actionCtx);
        actingOccupantCtx.ActorAddEffectsAfterActing(queuedAction);
        
        actingOccupantCtx.ActorAddAdditionalEffects(queuedAction, queuedAction.actionCtx);
        targetOccupantCtx?.TargetAddAdditionalEffects(queuedAction.actionCtx);
        
        grid.UpdateGrid(); // Updates AP and HP through transient stats
    }
    
    

    
    record struct MovementPathPoint
    {
        public BattleTile tile;
        public OccupantCtx occupantCtx;
    }

    struct CalculatedMovement
    {
        public List<MovementPathPoint> savePath;
        public MovementPathPoint[] newPath;
        public BattleTile originTile;
        public int actualAmount;
        public TileDirection dir;
    }
    
    bool DoesPrepareMovement(BattleTile forTile, BattlefieldSection preventMovementIntoSection, bool preventCenterSection, MovementCfg move, ref CalculatedMovement calcMovement)
    {
        if (move.direction == MovementDirection.None) return false;
        if (move.amount <= 0) return false;

        calcMovement.dir = move switch
        {
            _ when move.direction == MovementDirection.Right => TileDirection.Right,
            _ when move.direction == MovementDirection.Left => TileDirection.Left,
            _ when move.direction == MovementDirection.Up => TileDirection.Up,
            _ when move.direction == MovementDirection.Down => TileDirection.Down,
            _ => TileDirection.DiagDownLeft
        };
        

        calcMovement.originTile = forTile;
        if (calcMovement.originTile.occupantCtx == null) { Debug.LogError("Origin tile occupant is null"); return false; }
 
        // Distance 1: normal move or direct swap.
        if (move.amount == 1)
        {
            var destinationTile = grid.GetTileToThe(calcMovement.dir, forTile.row, forTile.col);
            if (destinationTile == null) return false;
            if (destinationTile.section == preventMovementIntoSection) return false;
            if (preventCenterSection && destinationTile.section == BattlefieldSection.Contested) return false;
            calcMovement.newPath = new[]
            {
                new MovementPathPoint { tile = destinationTile, occupantCtx = calcMovement.originTile.occupantCtx }
            };
            return true;
        }


        calcMovement.savePath = new List<MovementPathPoint>();

        // Snapshot the entire path BEFORE modifying any tiles.
        for (int i = 0; i <= move.amount; i++)
        {
            var tile = grid.GetTileToThe(calcMovement.dir, forTile.row, forTile.col, i);

            if (tile == null) break;
            if (tile.section == preventMovementIntoSection) break;
            if (preventCenterSection && tile.section == BattlefieldSection.Contested) break;
            
            calcMovement.savePath.Add(new MovementPathPoint
            {
                tile = tile,
                occupantCtx = tile.occupantCtx
            });
        }

        calcMovement.actualAmount = calcMovement.savePath.IndexOf(calcMovement.savePath[^1]);
        
        int a = 1;
        foreach (var p in calcMovement.savePath)
        {
            Debug.Log($"SavePath Tile {a}: [" + p.tile.row + ", " + p.tile.col + "] " + (p.occupantCtx != null ? p.occupantCtx.cfg.name : "Empty"));
            a++;
        }
        
        // Build the final arrangement.
        calcMovement.newPath = new MovementPathPoint[calcMovement.actualAmount + 1];
        
        // Mover goes into the destination.
        calcMovement.newPath[^1] = new MovementPathPoint
        {
            tile = calcMovement.savePath[calcMovement.actualAmount].tile,
            occupantCtx = calcMovement.originTile.occupantCtx
        };
        
        return true;
    }
    
    void HandleMovement(int myRow, int myCol, MovementCfg move, ref CalculatedMovement calcMovement)
    {
        if (move.direction == MovementDirection.None) return;
        if (move.amount <= 0) return;
        if (calcMovement.originTile.occupantCtx == null) return;
        
        if (move.amount == 1)
        {
            var destinationTile = calcMovement.newPath[^1].tile;
            if (destinationTile.occupantCtx != null) destinationTile.SwapOccupants(calcMovement.originTile, grid);
            else destinationTile.TransferInOccupant(calcMovement.originTile, grid);
            return;
        }
        
        // Destination is empty: ordinary movement.
        if (calcMovement.savePath[^1].occupantCtx == null)
        {
            calcMovement.savePath[^1].tile.TransferInOccupant(calcMovement.originTile, grid);
            return;
        }
        
        // Existing occupants get compressed toward the origin.
        int newIndex = calcMovement.actualAmount -1;

        for (int i = calcMovement.actualAmount; i > 0; i--)
        {
            var occupant = calcMovement.savePath[i].occupantCtx;

            if (occupant == null)
                continue;

            calcMovement.newPath[newIndex] = new MovementPathPoint
            {
                tile = calcMovement.savePath[newIndex].tile,
                occupantCtx = occupant
            };

            newIndex--;
        }

        // Clear all affected tiles AFTER the snapshot/final arrangement is built.
        foreach (var point in calcMovement.savePath)
        {
            point.tile.Clear();
            point.tile.occupantCtx = null;
        }

        // Apply final arrangement.
        foreach (var point in calcMovement.newPath)
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
