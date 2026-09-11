using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using static BattleTracker;
using static GridWorld;
using Random = UnityEngine.Random;

public class OpponentAI : MonoBehaviour
{
    public class OrderCtx
    {
        public EnemyOccupantCtx myEnemyOccupantCtx;
        public BattleTile myTile;
    }

    [ShowInInspector] public List<OrderCtx> battlerAttackOrder = new();
    
    public float proto_delayBetweenAttacks = 0.5f;
    public float proto_delayBetweenAttackers = 1f;
    public float proto_delayBetweenAttackFinishing = 0.15f;

    [Required] public BattlemodeActionConfig moveActionCfg;
    [Required] public BattlemodeActionConfig armorActionCfg;
    
    public void QueueOpponentActionsAtTurnStart(List<BattleTile> opponentTiles)
    {
        battlerAttackOrder.Clear();
        
        // Opponent's Attack Order
        foreach (var tile in opponentTiles)
            battlerAttackOrder.Add(new OrderCtx(){ myTile = tile, myEnemyOccupantCtx = tile.occupantCtx as EnemyOccupantCtx});

        if (opponentTiles.Count == 0)
        {
            Debug.LogWarning("No Opponent Tiles Found");
            return;
        }
        
        // Sort the Order
        battlerAttackOrder.Sort((a, b) =>
        {
            EnemyConfig aConfig = (EnemyConfig)a.myTile.occupantCtx.cfg;
            EnemyConfig bConfig = (EnemyConfig)b.myTile.occupantCtx.cfg;

            return aConfig.attackOrder.CompareTo(bConfig.attackOrder);
        });
        
        // Queue Actions for each battler
        foreach (var orderCtx in battlerAttackOrder)
        {
            var eoc = orderCtx.myEnemyOccupantCtx;
            var currentMaxIntentions = eoc.currentIntentUsageCtx.intentionsAmount;
            orderCtx.myEnemyOccupantCtx = eoc;
            orderCtx.myTile.DisplayGridIntentions(currentMaxIntentions);
            
            // For each intention
            for (int intentionsIndex = 0; eoc.queuedActions.Count < eoc.currentIntentUsageCtx.intentionsAmount; intentionsIndex++)
            {
                var newQueuedAction = GenerateOpponentQueuedAction(orderCtx, GetActionCfg(eoc));
                eoc.queuedActions.AddLast(newQueuedAction);
                orderCtx.myTile.DisplayIntentionToBattleTile(intentionsIndex, newQueuedAction.actionCtx.cfg.actionIdentifier);
                Debug.Log("[OPP AI] Queued Intention, Intention Queue Now At: " + eoc.queuedActions.Count + " out of currentMaxIntentions: " + currentMaxIntentions);
            }
        }
    }

    public QueuedAction GenerateOpponentQueuedAction(OrderCtx orderCtx, BattlemodeActionConfig actionCfg)
    {
        if(orderCtx.myEnemyOccupantCtx == null) { Debug.LogError("No Occupant Context found when queing action"); return null; }
        if (orderCtx.myEnemyOccupantCtx is not EnemyOccupantCtx eoc)
            { Debug.LogError("No Enemy Occupant Context found when queing action"); return null; }
        
        // Generate Action Context
        var actionCtx = actionCfg.GenerateActionCtx(
            BattlemodeActionCtx.Status.Acting,
            eoc,
            null); //this null is a slot only for teamates and healing them
                
        // Create the action for queuing
        QueuedAction newQueuedAction = new QueuedAction();
        newQueuedAction.Init();
        newQueuedAction.actingOccupantCtx = eoc;
        newQueuedAction.actionCtx = actionCtx;
        newQueuedAction.lookingForTarget = actionCtx.cfg.roleTarget;
        newQueuedAction.actorTile = orderCtx.myTile;

        // Before Queued Resolves
        foreach (var effect in eoc.currentEffects)
            effect.ResolveEffectRightBeforeQueued(eoc, actionCtx);
        foreach(var spEffect in eoc.specialEffects)
            spEffect.ResolveEffectRightBeforeQueued(eoc, actionCtx);
        
        // Assign to order's queued action
        return newQueuedAction;
    }
    
    
    
    public BattlemodeActionConfig GetActionCfg(EnemyOccupantCtx eoc)
    {
        float hpPerc01 = eoc.currentHp / eoc.maxHp;
        var newIntentUsage = eoc.enemyCfg.intentUsageCfg.GetIntent(hpPerc01, eoc.currentIntentUsageCtx);
                
        // Acounting for saved intentions, saves will allways be an actual action
        var actionIndex = newIntentUsage.intentIndex;

        // Get the action via the intention index
        // if null keep looping for a bit
        var actionCfg = GetAnEquippedActionCfg(eoc, actionIndex);
        if(actionCfg == null)
        {
            int maxAttempts = eoc.enemyCfg.equippedActions.Length;
            for (int i = 0; i < maxAttempts; i++)
            {
                int index = (actionIndex + i) % maxAttempts;
                actionCfg = eoc.enemyCfg.equippedActions.GetValue(index) as BattlemodeActionConfig;
                if (actionCfg != null) break;
            }
        }
        return actionCfg;
    }
    
    public void AttackAll(List<BattleTile> playerTiles, GridWorld grid, Action onAttackComplete)
    {
        if(battlerAttackOrder == null || battlerAttackOrder.Count == 0) Debug.LogError("Opponent Attack Order is empty");
        else StartCoroutine(C_Proto_AttackAll(battlerAttackOrder, playerTiles, grid, onAttackComplete));
    }
    
    IEnumerator C_Proto_AttackAll(List<OrderCtx> attackOrder, List<BattleTile> playerTiles, GridWorld grid, Action onAttacksComplete)
    {
        foreach (var orderCtx in attackOrder)
        {
            playerTiles.RemoveAll(playerTile => playerTile.occupantCtx == null); // Remove dead tiles
            
            yield return StartCoroutine(C_OpponentUseAction(orderCtx, playerTiles, grid));
            
            orderCtx.myEnemyOccupantCtx.queuedActions.Clear();
            grid.RefreshAllApsAndIntentions();
            Debug.Log($"{orderCtx.myEnemyOccupantCtx.cfg.occupantName} finished all attacks");
            yield return new WaitForSeconds(proto_delayBetweenAttackers);
        }
        grid.UpdateGrid();
        onAttacksComplete?.Invoke();
    }

    
    public IEnumerator C_OpponentUseAction(OrderCtx _orderCtx, List<BattleTile> playerTiles, GridWorld grid)
    {
        bool usedFreeMove = false;
        int maxIntentions = _orderCtx.myEnemyOccupantCtx.currentIntentUsageCtx.intentionsAmount;
        int intentions = maxIntentions;
        for (; intentions > 0 && _orderCtx.myEnemyOccupantCtx.queuedActions.Count > 0; intentions--)
        {
            Debug.Log($"[OPP AI] Using First Action [{intentions}] out of [{maxIntentions}], sequnce is: ");
            var eoc = _orderCtx.myEnemyOccupantCtx;
            eoc.queuedActions.ToList().PrintList();
            var _queuedAction = eoc.queuedActions.First();
            QueuedAction queuedAction = _queuedAction;               // prevents closure
            OrderCtx orderCtx = _orderCtx;                           // prevents closure
            
            // Refresh my tile if I moved
            var currentTile = eoc.newTilePosition != null
                ? eoc.newTilePosition
                : orderCtx.myTile;
            eoc.newTilePosition = null;
            orderCtx.myTile = currentTile;

            // action attempts max reached
            if (eoc.currentIntentUsageCtx.currentIntentionIndexCurrentAttempt > queuedAction.actionCtx.cfg.maxEocTargetingAttempts)
                { MoveOntoTryingNextAction(); continue; }

            // Usable Range Check
            RoleTarget selectedTarget = null;
            bool usable = grid.IsActionUsable(queuedAction.actionCtx.targetingCfgInstanced, orderCtx.myTile);
            bool altAction = false;
            if (!usable)
            {
                Debug.Log("[OPP AI] Unusable Attack");
                //Move & reqeue
                eoc.currentIntentUsageCtx.currentIntentionIndexCurrentAttempt++;
                var usableTile = grid.GetClosestUsableTile(queuedAction.actionCtx.targetingCfgInstanced, (orderCtx.myTile.col, orderCtx.myTile.row));
                if (usableTile != null) Debug.Log("[OPP AI] Cloeset Usable tile: [" + usableTile.row +  "," + usableTile.col + "]");
                else Debug.Log("[OPP AI] No Cloeset Usable tile found! Queuing Defend");
                selectedTarget = QueueMoveTarget(eoc, usableTile, queuedAction, orderCtx, grid) 
                                 ?? QueueDefendTarget(eoc, orderCtx);
                queuedAction = eoc.queuedActions.First(); // grab new first
                Debug.Log("[OPP AI] Attempting to do alternative action: " + queuedAction.actionCtx.cfg.actionName);
                altAction  = true;
            }
            else
                // Target Selection (Can target empty tiles for movement)
                selectedTarget = SelectATargetUsingQueuedAction(orderCtx, queuedAction, playerTiles);
            
            // Target Range Check
            var isTargetInRange = grid.IsTargetTargettable(queuedAction.actionCtx.targetingCfgInstanced, queuedAction.targetTile);
            if (!isTargetInRange && !altAction)
                { MoveOntoTryingNextAction(); continue; }
            
            var actingOccupantCtx = queuedAction.actingOccupantCtx;
            var targetOccupantCtx = (BattlerOccupantCtx)queuedAction.targetTile.occupantCtx; // can be null for empty tile
            
            eoc.PreResolveBeforeActingEffectsOpponent(queuedAction);

            eoc.queuedActions.RemoveFirst();
            yield return BattleTracker.Instance.C_UseAction(
                actingOccupantCtx,
                targetOccupantCtx, 
                selectedTarget,
                queuedAction,
                false);

            yield return new WaitForSeconds(proto_delayBetweenAttackFinishing + proto_delayBetweenAttacks);

            void MoveOntoTryingNextAction()
            {
                eoc.currentIntentUsageCtx.currentIntentionIndexCurrentAttempt = 1; // resets to 1
                if (!usedFreeMove) { intentions++; usedFreeMove = true; }
                eoc.queuedActions.RemoveFirst();
            }
        }
    }
    

    RoleTarget QueueDefendTarget(EnemyOccupantCtx eoc, OrderCtx orderCtx)
    {
        Debug.Log("Cannot Move, Queuing Defend Action");
        var newDefendActionCtx = GenerateOpponentQueuedAction(orderCtx, armorActionCfg);
        // Target: SELF for defending
        var targetTile = orderCtx.myTile;   
        newDefendActionCtx.targetTile = targetTile;
        newDefendActionCtx.targSelfSlot.selfTile = targetTile;
        eoc.queuedActions.AddFirst(newDefendActionCtx);
        
        return newDefendActionCtx.targSelfSlot;
    }

    RoleTarget QueueMoveTarget(
        EnemyOccupantCtx eoc,
        BattleTile endTile,
        QueuedAction queuedAction,
        OrderCtx orderCtx,
        GridWorld grid)
    {
        var startTile = orderCtx.myTile;
        
        var newQueuedMoveAction = GenerateOpponentQueuedAction(orderCtx, moveActionCfg);
        // Target: FIRST tile of the shortest path. 
        var targetMoveTile = grid.GetNextMoveTile(startTile, endTile, 
            queuedAction.actionCtx.targetingCfgInstanced,
            true);
        if (targetMoveTile == null) return null;
        
        newQueuedMoveAction.targetTile = targetMoveTile;
        newQueuedMoveAction.targEmptyTileSlot.emptyTile = targetMoveTile;
        eoc.queuedActions.AddFirst(newQueuedMoveAction);
        Debug.Log($"[MOVE] Queued Move from [{orderCtx.myTile.col},{orderCtx.myTile.row}] "
                  + $"to [{targetMoveTile.col},{targetMoveTile.row}]");
        return newQueuedMoveAction.targEmptyTileSlot;
    }

    RoleTarget SelectATargetUsingQueuedAction(OrderCtx orderCtx,
        QueuedAction queuedAction,
        List<BattleTile> playerTiles)
    {
        // Target Selection
        switch (queuedAction.lookingForTarget)
        {
            case BattlemodeActionConfig.Role.Self:
                // orderCtx.queuedAction.targSelf.tile = actorTile; 
                // target = queuedAction.targSelf;
                break;
            case BattlemodeActionConfig.Role.Ally:
                // queuedAction.targAlly.tile = actorTile; 
                // target = queuedAction.targAlly;
                break;
            case BattlemodeActionConfig.Role.Enemy:
                queuedAction.targetTile = FindTargetViaAttackPriority();
                queuedAction.targEnemySlot.enemyTile = queuedAction.targetTile;
                Debug.Log("Tile to be attacked is :" + queuedAction.targEnemySlot.enemyTile.occupantCtx.cfg.occupantName + " at " +
                          "position " + queuedAction.targEnemySlot.enemyTile.col + " , " +  queuedAction.targEnemySlot.enemyTile.row);
                return queuedAction.targEnemySlot;
            case BattlemodeActionConfig.Role.Team:
                // queuedAction.targAllyTeam.tiles.Add(actorTile); 
                // target = queuedAction.targAllyTeam;
                break;
            case BattlemodeActionConfig.Role.EnemyTeam:
                // queuedAction.targEnemyTeam.tiles.Add(actorTile); 
                // target = queuedAction.targEnemyTeam;
                break;
            case BattlemodeActionConfig.Role.EmptyTile:
                // happens during range check
                queuedAction.targetTile = null;
                return queuedAction.targEmptyTileSlot;
        }
        
        BattleTile FindTargetViaAttackPriority()
        {
            var atkPriority = orderCtx.myEnemyOccupantCtx.attackPriority.RandBagPull();
            
            switch (atkPriority)
            {
                case EnemyConfig.AttackPriority.LowHP:
                    return FindLowestHealthCharacterTile(playerTiles);
                case EnemyConfig.AttackPriority.HighHP:
                    return FindHighestHealthCharacterTile(playerTiles);
                default:
                    return playerTiles.First();
                    break;
            }
        }
        return null;
    }


    

    

    BattlemodeActionConfig GetAnEquippedActionCfg(EnemyOccupantCtx eoc, int actionIndex)
    {
        BattlemodeActionConfig action = null;
        var equippedActions = eoc.enemyCfg.equippedActions;
        if (equippedActions == null || equippedActions.Length == 0)
        {
            Debug.LogError($"No equipped actions found for {eoc.enemyCfg.name}.");
            return null;
        }
        
        if (actionIndex >= 0 && actionIndex < eoc.enemyCfg.equippedActions.Length)
            action = eoc.enemyCfg.equippedActions[actionIndex];

        if (action == null)
        {
            int maxAttempts = eoc.enemyCfg.equippedActions.Length * 2;

            for (int i = 0; i < maxAttempts; i++)
            {
                int index = (actionIndex + i) % eoc.enemyCfg.equippedActions.Length;
                action = eoc.enemyCfg.equippedActions.GetValue(index) as BattlemodeActionConfig;

                if (action != null) break;
            }
        }

        if (action == null) Debug.LogError($"No equipped actions found for {eoc.enemyCfg.name}.");
        else Debug.Log("Action Retrieved from ActionCfg for OpponentAI : " + action.name);
        return action;
    }
    

    BattleTile FindLowestHealthCharacterTile(List<BattleTile> occupiedBattlerTiles)
    {
        if (occupiedBattlerTiles.Count == 0) { Debug.Log("No target tiles to select lowest health"); return null; }
        
        BattleTile lowestHealth = null;
        foreach (BattleTile compare in occupiedBattlerTiles)
        {
            if (compare == null) continue;
            if (lowestHealth == null) { lowestHealth = compare; continue; }
            if(((BattlerOccupantCtx)compare.occupantCtx).currentHp < ((BattlerOccupantCtx)lowestHealth.occupantCtx).currentHp)
                lowestHealth = compare;
        }
        Debug.Log("Found Lowest Health: " + lowestHealth.occupantCtx.cfg.name);
        return lowestHealth;
    }

    BattleTile FindHighestHealthCharacterTile(List<BattleTile> occupiedBattlerTiles)
    {
        if (occupiedBattlerTiles.Count == 0) {
            Debug.LogError("No Target Tiles to select highest health"); return null; }

        BattleTile highestHealth = null;
        foreach (var compare in occupiedBattlerTiles)
        {
            if (compare == null) continue;
            if (highestHealth == null) { highestHealth = compare; continue; }

            var compareHp = ((BattlerOccupantCtx)compare.occupantCtx).currentHp;
            var highestHp = ((BattlerOccupantCtx)highestHealth.occupantCtx).currentHp;
            if (compareHp > highestHp || (compareHp == highestHp && Random.value < 0.5f))
                highestHealth = compare;
        }
        Debug.Log("Found Highest Health: " + highestHealth.occupantCtx.cfg.name);
        return highestHealth;
    }
    
}
