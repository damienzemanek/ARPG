using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
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

    BattlemodeActionCtx moveActionCtx;
    BattlemodeActionCtx defendActionCtx;
    
    public void QueueOpponentActionsAtTurnStart(List<BattleTile> opponentTiles)
    {
        battlerAttackOrder.Clear();

        moveActionCtx ??= moveActionCfg.GenerateActionCtx(BattlemodeActionCtx.Status.Acting, null, null, 1);
        defendActionCtx ??= armorActionCfg.GenerateActionCtx(BattlemodeActionCtx.Status.Acting, null, null, 1);
        
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
        
        // Queue Actions
        // For each battler
        foreach (var orderCtx in battlerAttackOrder)
        {
            var eoc = orderCtx.myEnemyOccupantCtx;
            var intentions = eoc.currentIntentUsageCtx.intentionsAmount;
            orderCtx.myEnemyOccupantCtx = eoc;
            orderCtx.myTile.SetIntentionsAmount(intentions);
            orderCtx.myTile.intentionsGrid.gameObject.SetActive(true);
            
            // For each intention
            for (int intentionsIndex = 0; intentionsIndex < eoc.currentIntentUsageCtx.intentionsAmount; intentionsIndex++)
            {
                var newQueuedAction = GenerateOpponentQueuedAction(intentionsIndex, orderCtx);
                newQueuedAction.actorTile = orderCtx.myTile;
                eoc.queuedActions.Add(newQueuedAction);
                orderCtx.myTile.DisplayIntentionToBattleTile(intentionsIndex, newQueuedAction.actionCtx.cfg.actionIdentifier);
            }
        }
    }

    public BattleTracker.QueuedAction GenerateOpponentQueuedAction(int intentionsIndex, OrderCtx orderCtx)
    {
        if(orderCtx.myEnemyOccupantCtx == null) { Debug.LogError("No Occupant Context found when queing action"); return null; }
        if (orderCtx.myEnemyOccupantCtx is not EnemyOccupantCtx eoc)
            { Debug.LogError("No Enemy Occupant Context found when queing action"); return null; }
        
        // Generate Action Context
        var actionCfg = GetActionCfg(eoc);
        var actionCtx = GetActionCfg(eoc).GenerateActionCtx(
            BattlemodeActionCtx.Status.Acting,
            orderCtx.myEnemyOccupantCtx,
            null); //this null is a slot only for teamates and healing them
                
        // Create the action for queuing
        BattleTracker.QueuedAction newQueuedAction = new BattleTracker.QueuedAction();
        newQueuedAction.Init();
        newQueuedAction.actingOccupantCtx = eoc;
        newQueuedAction.actionCtx = actionCtx;
        newQueuedAction.lookingForTarget = actionCfg.roleTarget;

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
        bool useSaved = (newIntentUsage.savedIntentIndex != -1);
        var actionIndex = (useSaved) ? (newIntentUsage.savedIntentIndex) : (newIntentUsage.intentIndex);
        if (useSaved)
        {
            eoc.ResetSavedIntention();
            Debug.Log("[Intentions] Used Saved Intention");
        }
                
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
            yield return StartCoroutine(C_Proto_Attack(orderCtx, playerTiles, grid));
            orderCtx.myEnemyOccupantCtx.queuedActions.Clear();
            grid.RefreshAllApsAndIntentions();
            Debug.Log($"{orderCtx.myEnemyOccupantCtx.cfg.occupantName} finished all attacks");
            yield return new WaitForSeconds(proto_delayBetweenAttackers);
        }
        grid.UpdateGrid();
        onAttacksComplete?.Invoke();
    }

    
    public IEnumerator C_Proto_Attack(OrderCtx _orderCtx, List<BattleTile> playerTiles, GridWorld grid)
    {
        foreach (var _queuedAction in _orderCtx.myEnemyOccupantCtx.queuedActions)
        {
            bool usedSaved = _orderCtx.myEnemyOccupantCtx.HasSavedIntention(out int savedIntention);
            OrderCtx orderCtx = _orderCtx;
            RoleTarget targetRole = null;
            BattleTracker.QueuedAction queuedAction = _queuedAction;
            if (usedSaved)
            {
                queuedAction = GenerateOpponentQueuedAction(savedIntention, orderCtx);

                Debug.Log($"[Intentions] Used Saved Intention: {savedIntention} " + queuedAction);
                Debug.Log("[Intentions] Which is " + queuedAction.actionCtx.cfg.actionName);

                targetRole = TryGetTarget(orderCtx, queuedAction, playerTiles, out queuedAction.targetTile);

                orderCtx.myEnemyOccupantCtx.ResetSavedIntention();
            }
            else
                targetRole = TryGetTarget(orderCtx, queuedAction, playerTiles, out queuedAction.targetTile);
            
            // Refresh my tile if i moved
            
            var currentTile = orderCtx.myEnemyOccupantCtx.newTilePosition != null
                ? orderCtx.myEnemyOccupantCtx.newTilePosition
                : orderCtx.myTile;
            orderCtx.myEnemyOccupantCtx.newTilePosition = null;
            orderCtx.myTile = currentTile;
            
            targetRole = RangeCheck(targetRole); // This can change if the target is not in range
            
            orderCtx.myEnemyOccupantCtx.PreResolveBeforeActingEffectsOpponent(queuedAction);

            for (int currentHitCount = 1; currentHitCount <= queuedAction.actionCtx.cfg.hitCount; currentHitCount++)
            {
                // Idempotent Attacks
                if (targetRole == null) Debug.LogWarning("No Target Found");
                else
                {
                    queuedAction.actionCtx.currentHitCount = currentHitCount;
                    targetRole.ActedUponBy(queuedAction, orderCtx.myTile);
                }
            }


            // Still evaluate effects after attacking or not attacking (esp for DoT effects like bleed)
            CoroutineRunner.Instance.RunMethodDelayed(() =>
            {
                queuedAction.actingOccupantCtx.ActorResolveAfterActingEffects(queuedAction.actionCtx);
                grid.UpdateGrid(); // Updates AP and HP through transient stats
                
            }, proto_delayBetweenAttackFinishing);

            yield return new WaitForSeconds(proto_delayBetweenAttackFinishing + proto_delayBetweenAttacks);
            
            RoleTarget RangeCheck(RoleTarget target)
            {
                bool isInRange = false;
                bool actionDecided = false;
                var targetTile = queuedAction.targetTile;

                while (!isInRange && !actionDecided)
                {
                    if (!grid.IsTargetInRange(queuedAction.actionCtx.targetingCfgInstanced, orderCtx.myTile, targetTile))
                    {
                        TryToMoveTo(out var moveTile);
                        if (moveTile != null)
                        {
                            queuedAction.targEmptyTileSlot.emptyTile = targetTile;

                            Debug.Log($"[MOVE] Moving from [{orderCtx.myTile.col},{orderCtx.myTile.row}] "
                                      + $"to [{targetTile.col},{targetTile.row}]");
                            return queuedAction.targEmptyTileSlot;
                        }
                        else
                        {
                            Defend();
                            queuedAction.targSelfSlot.selfTile = targetTile;
                            return queuedAction.targSelfSlot;
                        }
                    }

                    isInRange = true;
                    
                    void TryToMoveTo(out BattleTile moveTile)
                    {
                        Debug.Log("Not In Range, Queuing Move Action");

                        var savedTargetTile = targetTile;

                        queuedAction.Init();
                        queuedAction.actingOccupantCtx = orderCtx.myEnemyOccupantCtx;
                        queuedAction.actionCtx = moveActionCtx;
                        queuedAction.lookingForTarget = BattlemodeActionConfig.Role.EmptyTile;

                        actionDecided = true;
                        orderCtx.myEnemyOccupantCtx.SaveCurrentIntention();

                        // Find the FIRST tile of the shortest path.
                        targetTile = grid.GetNextMoveTile(
                            orderCtx.myTile,
                            savedTargetTile, 
                            queuedAction.actionCtx.targetingCfgInstanced,
                            true);
                        moveTile = targetTile;
                    }
                    
                    void Defend()
                    {
                        Debug.Log("Not In Range, Queuing Defend Action");
                        
                        queuedAction.Init();
                        queuedAction.actingOccupantCtx = orderCtx.myEnemyOccupantCtx;
                        queuedAction.actionCtx = defendActionCtx;
                        queuedAction.lookingForTarget = BattlemodeActionConfig.Role.Self;
                        actionDecided = true;
                        targetTile = orderCtx.myTile;
                    }
                    
                }
                return target;
                
            }
            
            
        }
    }
    
    // GridWorld.InRangeCheckCtx inRangeCheckCtx = new GridWorld.InRangeCheckCtx()
    // {
    //     upRange = queuedAction.actionCtx.cfg.upRange,
    //     fwdRange = queuedAction.actionCtx.cfg.fwdRange,
    //     downRange = queuedAction.actionCtx.cfg.downRange,
    //     myRow = orderCtx.tile.row,
    //     myCol = orderCtx.tile.col,
    // };
    //     
    // grid.GetClosestTargetTile(inRangeCheckCtx, queuedAction.lookingForTarget, orderCtx.tile.occupantCtx.cfg);


    RoleTarget TryGetTarget(OrderCtx orderCtx,
        BattleTracker.QueuedAction queuedAction,
        List<BattleTile> playerTiles, out BattleTile targetTile)
    {
        
        RoleTarget target = null;
        targetTile = null;
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
                var newTargetTile = FindTargetViaAttackPriority();
                queuedAction.targEnemySlot.enemyTile = newTargetTile;
                Debug.Log("Tile to be attacked is :" + queuedAction.targEnemySlot.enemyTile.occupantCtx.cfg.occupantName + " at " +
                          "position " + queuedAction.targEnemySlot.enemyTile.col + " , " +  queuedAction.targEnemySlot.enemyTile.row);
                targetTile = newTargetTile;
                target = queuedAction.targEnemySlot;
                break;
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
                target = null;
                break;
        }

        return target;
        
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
