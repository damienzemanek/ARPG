using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BattlemodeActionConfig", menuName = "ARPG/SO/BattlemodeActionConfig")]
public class BattlemodeActionConfig : ScriptableObject
{
    public enum Role
    {
        None,
        Self,
        Enemy,
        Ally,
        Team,
        EnemyTeam,
    }
    
    

    public string actionName;
    public string description;
    public Sprite icon;
    public int hitCount = 1;
    public int dmgMultiplier = 100;
    public int healMultiplier = 0;
    public int fwdRange = 1;
    public int upRange;
    public int downRange;
    public int aoe = 0;
    public int apCost = 1;
    [FormerlySerializedAs("useTarget")] public Role roleTarget;
    [FormerlySerializedAs("effects")] public List<BattlemodeEffectConfig> effectsToApplyToTarget = new();
    public List<BattlemodeEffectConfig> effectsToApplyToSelf = new();

    public BattlemodeActionCtx GenerateActionCtx(
        BattlemodeActionCtx.Status _status,
        BattlerOccupantCtx actingBatlerCtx,
        BattlerOccupantCtx targetBatlerCtx = null)
    {
        var ctx = new BattlemodeActionCtx
        {
            hitCount = this.hitCount,
            hitCountDelta = 0,
            dmg = actingBatlerCtx.currentDMG,
            heal = targetBatlerCtx?.maxHp ?? 0,
            deltaDmgMultiplier = this.dmgMultiplier,
            deltaHealMultiplier = this.healMultiplier,
            status = _status,
            cfg = this,
        };

        if (actingBatlerCtx is CharacterOccupantCtx)
        {
            ctx.ap = apCost;
            ctx.apDelta = 0;
        }
        
        return ctx;
    }
}

public class BattlemodeActionCtx
{
    public enum Status
    {
        BeingHit,
        Acting,
        TurnEnd,
        BattleEnd,
        ExpeditionEnd,
    }
    
    public Status status;

    public int hitCount;
    public int dmg;
    public int heal;
    public int ap;
    
    public int hitCountDelta;
    public int apDelta;
    public float deltaDmgMultiplier;
    public float deltaHealMultiplier;
    
    public BattlemodeActionConfig cfg;

    public void SetHealViaTargetMaxHealth(BattlerOccupantCtx targetBatlerCtx)
        => heal = targetBatlerCtx?.maxHp ?? 0;
}