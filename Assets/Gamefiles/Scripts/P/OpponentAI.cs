using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OpponentAI : MonoBehaviour
{
    
    public List<BattleTile> opponentAttackOrder;
    
    public float proto_delayBetweenAttacks = 0.5f;
    
    public void AttackAll(List<BattleTile> opponentTiles, List<BattleTile> playerTiles, GridWorld grid)
    {
        opponentAttackOrder = opponentTiles;
        
        // Opponent's Attack Order
        opponentAttackOrder.Sort((a, b) =>
        {
            EnemyConfig aConfig = (EnemyConfig)a.occupantCtx.cfg;
            EnemyConfig bConfig = (EnemyConfig)b.occupantCtx.cfg;

            return aConfig.attackOrder.CompareTo(bConfig.attackOrder);
        });
        StartCoroutine(C_Proto_AttackAll(opponentAttackOrder, playerTiles, grid));
    }
    
    IEnumerator C_Proto_AttackAll(List<BattleTile> opponentAttackOrder, List<BattleTile> playerTiles, GridWorld grid)
    {
        foreach (BattleTile actorTile in opponentAttackOrder)
        {
            Attack(actorTile, playerTiles, grid);
            yield return new WaitForSeconds(proto_delayBetweenAttacks);
        }
    }
    
    public void Attack(BattleTile actorTile, List<BattleTile> playerTiles, GridWorld grid)
    {
        
        // Choose an ability
        if (actorTile.occupantCtx is not EnemyOccupantCtx eoc) return;
        float hpPerc01 = eoc.currentHp / eoc.maxHp;
        var newIntentUsage = eoc.enemyCfg.intentUsageCfg.GetIntent(hpPerc01, eoc.currentIntentUsageCtx);
        var actionIndex = newIntentUsage.intentIndex;
        var action = GetAnEquippedCfgAction();
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
        var targetRole = action.roleTarget;
        
        // Generate Action Context
        var actionCtx = action.GenerateActionCtx(
            BattlemodeActionCtx.Status.Acting,
            actorTile.occupantCtx as BattlerOccupantCtx,
            null); //this null is a slot only for teamates and healing them
        
        // Immedietly Queue the action
        BattleTracker.QueuedAction queuedAction = new BattleTracker.QueuedAction();
        queuedAction.Init();
        queuedAction.actingOccupantCtx = eoc;
        queuedAction.queuedActionCtx = actionCtx;
        
        // Choose a target
        RoleTarget target = null;
        
        switch (targetRole)
        {
            case BattlemodeActionConfig.Role.Self:
                queuedAction.targSelf.tile = actorTile; 
                target = queuedAction.targSelf;
                break;
            case BattlemodeActionConfig.Role.Ally:
                queuedAction.targAlly.tile = actorTile; 
                target = queuedAction.targAlly;
                break;
            case BattlemodeActionConfig.Role.Enemy:
                queuedAction.targEnemy.tile = FindTargetViaAttackPriority(); 
                queuedAction.targetTile = queuedAction.targEnemy.tile;
                target = queuedAction.targEnemy;
                break;
            case BattlemodeActionConfig.Role.Team:
                queuedAction.targAllyTeam.tiles.Add(actorTile); 
                target = queuedAction.targAllyTeam;
                break;
            case BattlemodeActionConfig.Role.EnemyTeam:
                queuedAction.targEnemyTeam.tiles.Add(actorTile); 
                target = queuedAction.targEnemyTeam;
                break;
        }
        
        if (target == null) { Debug.LogError("No Target Found"); return; }
        target.ActUponTarget(queuedAction);
        
        CoroutineRunner.Instance.RunMethodDelayed(() =>
        {
            queuedAction.actingOccupantCtx.PostResolveActingEffects(queuedAction.queuedActionCtx);
            grid.UpdateGrid(); // Updates AP and HP through transient stats
        }, 0.1f);
        
        
        BattleTile FindTargetViaAttackPriority()
        {
            var atkPriority = eoc.attackPriority.RandBagPull();
            
            switch (atkPriority)
            {
                case EnemyConfig.AttackPriority.LowHP:
                    return FindLowestHealthCharacterTile(playerTiles);
                default:
                    return playerTiles.First();
                    break;
            }
        }

        BattlemodeActionConfig GetAnEquippedCfgAction()
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
                int maxAttempts = eoc.enemyCfg.equippedActions.Length;

                for (int i = 0; i < maxAttempts; i++)
                {
                    int index = (actionIndex + i) % maxAttempts;
                    action = eoc.enemyCfg.equippedActions.GetValue(index) as BattlemodeActionConfig;

                    if (action != null) break;
                }
            }

            if (action == null) Debug.LogError($"No equipped actions found for {eoc.enemyCfg.name}.");
            return action;
        }
        

        
        // Validate, if invalid restart

        // Execute

        // Setup For next attack
        eoc.currentIntentUsageCtx = newIntentUsage;
    }
    

    BattleTile FindLowestHealthCharacterTile(List<BattleTile> occupiedBattlerTiles)
    {
        if (occupiedBattlerTiles.Count == 0)
        {
            Debug.LogError("No target tiles to select lowest health");
            return null;
        }
        BattleTile lowestHealth = null;
        foreach (BattleTile compare in occupiedBattlerTiles)
        {
            if (compare == null) continue;
            if (lowestHealth == null) { lowestHealth = compare; continue; }
            if(((BattlerOccupantCtx)compare.occupantCtx).currentHp < ((BattlerOccupantCtx)lowestHealth.occupantCtx).currentHp)
                lowestHealth = compare;
        }
        return lowestHealth;
    }
    
}
