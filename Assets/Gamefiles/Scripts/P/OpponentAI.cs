using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class OpponentAI : MonoBehaviour
{
    public class OrderCtx
    {
        public EnemyOccupantCtx myEnemyOccupantCtx;
        public BattleTile tile;
    }

    [ShowInInspector] public List<OrderCtx> battlerAttackOrder = new();
    
    public float proto_delayBetweenAttacks = 0.5f;
    public float proto_delayBetweenAttackers = 1f;
    public float proto_delayBetweenAttackFinishing = 0.15f;
    

    public void QueueActions(List<BattleTile> opponentTiles)
    {
        battlerAttackOrder.Clear();
        
        // Opponent's Attack Order
        foreach (var tile in opponentTiles)
            battlerAttackOrder.Add(new OrderCtx(){ tile = tile});

        // Sort the Order
        battlerAttackOrder.Sort((a, b) =>
        {
            EnemyConfig aConfig = (EnemyConfig)a.tile.occupantCtx.cfg;
            EnemyConfig bConfig = (EnemyConfig)b.tile.occupantCtx.cfg;

            return aConfig.attackOrder.CompareTo(bConfig.attackOrder);
        });
        
        // Queue Actions
        foreach (var orderCtx in battlerAttackOrder)
        {
            if (orderCtx.tile.occupantCtx is not EnemyOccupantCtx eoc) return;

            var intentions = eoc.intentions;
            orderCtx.tile.SetIntentionsAmount(intentions);
            orderCtx.tile.intentionsGrid.gameObject.SetActive(true);

            for (int intentionsIndex = 0; intentionsIndex < eoc.currentIntentions; intentionsIndex++)
            {
                // Choose an ability
                float hpPerc01 = eoc.currentHp / eoc.maxHp;
                var newIntentUsage = eoc.enemyCfg.intentUsageCfg.GetIntent(hpPerc01, eoc.currentIntentUsageCtx);
                var actionIndex = newIntentUsage.intentIndex;
                var action = GetAnEquippedActionCfg(eoc, actionIndex);
                if(action == null)
                {
                    int maxAttempts = eoc.enemyCfg.equippedActions.Length;

                    for (int i = 0; i < maxAttempts; i++)
                    {
                        int index = (actionIndex + i) % maxAttempts;
                        action = eoc.enemyCfg.equippedActions.GetValue(index) as BattlemodeActionConfig;

                        if (action != null)
                            break;
                    }
                }
            
                // Generate Action Context
                var actionCtx = action.GenerateActionCtx(
                    BattlemodeActionCtx.Status.Acting,
                    orderCtx.tile.occupantCtx as BattlerOccupantCtx,
                    null); //this null is a slot only for teamates and healing them
            
                // Immedietly Queue the action
                BattleTracker.QueuedAction newQueuedAction = new BattleTracker.QueuedAction();
                newQueuedAction.Init();
                newQueuedAction.actingOccupantCtx = eoc;
                newQueuedAction.actionCtx = actionCtx;
                newQueuedAction.lookingForTarget = action.roleTarget;
                
                // Display Queued Actions in Tile UI
                orderCtx.tile.SetIntention(intentionsIndex, newQueuedAction.actionCtx.cfg.actionIdentifier);
                
                // Assign to order's queued action
                eoc.queuedActions.Add(newQueuedAction);
                orderCtx.myEnemyOccupantCtx = eoc;
            }
        }
        
    }
    
    public void AttackAll(List<BattleTile> playerTiles, GridWorld grid)
    {
        if(battlerAttackOrder == null || battlerAttackOrder.Count == 0)
            Debug.LogError("Opponent Attack Order is empty");
        else
            StartCoroutine(C_Proto_AttackAll(battlerAttackOrder, playerTiles, grid));
    }
    
    IEnumerator C_Proto_AttackAll(List<OrderCtx> attackOrder, List<BattleTile> playerTiles, GridWorld grid)
    {
        foreach (var orderCtx in attackOrder)
        {
            playerTiles.RemoveAll(playerTile => playerTile.occupantCtx == null); // Remove dead tiles
            StartCoroutine(C_Proto_Attack(orderCtx, playerTiles, grid));
            grid.RefreshAllApsAndIntentions();
            yield return new WaitForSeconds(proto_delayBetweenAttackers);
        }
        grid.UpdateGrid();
    }
    
    public IEnumerator C_Proto_Attack(OrderCtx orderCtx, List<BattleTile> playerTiles, GridWorld grid)
    {
        // Choose a target
        RoleTarget target = null;

        foreach (var queuedAction in orderCtx.myEnemyOccupantCtx.queuedActions)
        {
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
                    queuedAction.targEnemySlot.tile = FindTargetViaAttackPriority(); 
                    queuedAction.targetTile = queuedAction.targEnemySlot.tile;
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
            }

            // Idempotent Attacks
            if (target == null) Debug.LogWarning("No Target Found"); 
            else target.ActUponTarget(queuedAction);

            // Still evaluate effects after attacking or not attacking (esp for DoT effects like bleed)
            CoroutineRunner.Instance.RunMethodDelayed(() =>
            {
                queuedAction.actingOccupantCtx.PostResolveActingEffects(queuedAction.actionCtx);
                grid.UpdateGrid(); // Updates AP and HP through transient stats
                
            }, proto_delayBetweenAttackFinishing);

            yield return new WaitForSeconds(proto_delayBetweenAttackFinishing + proto_delayBetweenAttacks);
        }
        
        
        
        
        BattleTile FindTargetViaAttackPriority()
        {
            Debug.Break();
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
        if (eoc.enemyCfg.equippedActions.Length == 0)
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
                int index = (actionIndex + i) % maxAttempts;
                action = eoc.enemyCfg.equippedActions.GetValue(index) as BattlemodeActionConfig;

                if (action != null) break;
            }
        }

        if (action == null) Debug.LogError($"No equipped actions found for {eoc.enemyCfg.name}.");
        Debug.Log("Action Retrieved from ActionCfg for OpponentAI : " + action.name);
        return action;
    }
    

    BattleTile FindLowestHealthCharacterTile(List<BattleTile> occupiedBattlerTiles)
    {
        if (occupiedBattlerTiles.Count == 0) {
            Debug.LogError("No target tiles to select lowest health"); return null; }
        
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
            if (highestHealth == null)
            {
                highestHealth = compare;
                continue;
            }

            var compareHp = ((BattlerOccupantCtx)compare.occupantCtx).currentHp;
            var highestHp = ((BattlerOccupantCtx)highestHealth.occupantCtx).currentHp;
            if (compareHp > highestHp || (compareHp == highestHp && Random.value < 0.5f))
                highestHealth = compare;
        }
        Debug.Log("Found Highest Health: " + highestHealth.occupantCtx.cfg.name);
        return highestHealth;
    }
    
}
