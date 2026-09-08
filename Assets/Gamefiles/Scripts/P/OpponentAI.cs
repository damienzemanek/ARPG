using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            var intentions = eoc.currentIntentUsageCtx.intentionsAmount;
            orderCtx.myEnemyOccupantCtx = eoc;
            orderCtx.myTile.DisplayGridIntentions(intentions);
            
            // For each intention
            for (int intentionsIndex = 0; intentionsIndex < eoc.currentIntentUsageCtx.intentionsAmount; intentionsIndex++)
            {
                var newQueuedAction = GenerateOpponentQueuedAction(orderCtx, GetActionCfg(eoc));
                newQueuedAction.actorTile = orderCtx.myTile;
                eoc.queuedActions.Add(newQueuedAction);
                orderCtx.myTile.DisplayIntentionToBattleTile(intentionsIndex, newQueuedAction.actionCtx.cfg.actionIdentifier);
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
        int amountOfActions = _orderCtx.myEnemyOccupantCtx.queuedActions.Count;
        for (int currentActionIndex = 0; currentActionIndex < amountOfActions; currentActionIndex++)
        {
            var eoc = _orderCtx.myEnemyOccupantCtx;
            var _queuedAction = eoc.queuedActions[currentActionIndex];
            QueuedAction queuedAction = _queuedAction; // prevents closure
            OrderCtx orderCtx = _orderCtx;                           // prevents closure
            
            // Refresh my tile if I moved
            var currentTile = eoc.newTilePosition != null
                ? eoc.newTilePosition
                : orderCtx.myTile;
            eoc.newTilePosition = null;
            orderCtx.myTile = currentTile;
            
            var selectedTarget = SelectATargetUsingQueuedAction(orderCtx, queuedAction, playerTiles);
            var changedTarget = RangeCheck(eoc, queuedAction, grid, orderCtx, ref amountOfActions, ref currentActionIndex); // This can change if the target is not in range
            if(changedTarget != null) selectedTarget = changedTarget;
            
            var actingOccupantCtx = queuedAction.actingOccupantCtx;
            var targetOccupantCtx = (BattlerOccupantCtx)queuedAction.targetTile.occupantCtx; // can be null for empty tile
            
            eoc.PreResolveBeforeActingEffectsOpponent(queuedAction);

            yield return BattleTracker.Instance.C_UseAction(
                actingOccupantCtx,
                targetOccupantCtx, 
                selectedTarget,
                queuedAction,
                false);

            yield return new WaitForSeconds(proto_delayBetweenAttackFinishing + proto_delayBetweenAttacks);
        }
    }
    
    RoleTarget RangeCheck(
        EnemyOccupantCtx eoc,
        QueuedAction queuedAction,
        GridWorld grid,
        OrderCtx orderCtx,
        ref int amountOfActions,
        ref int currentActionIndex)
    {
        var targetTile = queuedAction.targetTile;
        bool targetInRange = grid.IsTargetInRange(queuedAction.actionCtx.targetingCfgInstanced, orderCtx.myTile, targetTile);
        if (targetInRange) return null;
        
        Debug.Log("Not In Range, Queuing Move Action");
        amountOfActions++;
                
                
        var endTile = targetTile;
        var startTile = orderCtx.myTile;
        var newQueuedMoveAction = GenerateOpponentQueuedAction(orderCtx, moveActionCfg);
        eoc.queuedActions.Insert(currentActionIndex, newQueuedMoveAction);
                
        // Target: FIRST tile of the shortest path. 
        targetTile = grid.GetNextMoveTile(startTile, endTile, 
            queuedAction.actionCtx.targetingCfgInstanced,
            true);
        
                
        if (targetTile != null)
        {
            queuedAction.targEmptyTileSlot.emptyTile = targetTile;
            Debug.Log($"[MOVE] Moving from [{orderCtx.myTile.col},{orderCtx.myTile.row}] "
                      + $"to [{targetTile.col},{targetTile.row}]");
            return queuedAction.targEmptyTileSlot;
        }
        
        //Defend
        Debug.Log("Cannot Move, Queuing Defend Action");
        var newDefendActionCtx = GenerateOpponentQueuedAction(orderCtx, armorActionCfg);
        eoc.queuedActions.Insert(currentActionIndex, newDefendActionCtx);
        
        // Target: SELF for defending
        targetTile = orderCtx.myTile;                    
        newDefendActionCtx.targSelfSlot.selfTile = targetTile;
        return newDefendActionCtx.targSelfSlot;
        
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
