using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BattlemodeActionConfig", menuName = "ARPG/SO/BattlemodeActionConfig")]
public class BattlemodeActionConfig : ScriptableObject
{
    bool isEmptyRoleTarget => roleTarget == Role.EmptyTile;
    
    public enum Role
    {
        None,
        Self, // GOAL
        Enemy,
        Ally, // GOAL
        Team, // GOAL
        EnemyTeam, // GOAL
        EmptyTile 
    }

    public enum ActionIdentifier
    {
        None,
        Attack,
        AttackDebuff,
        AttackBuff,
        Debuff,
        Buff,
        Move,
    }
    

    public string actionName;
    public string description;
    public Sprite icon;
    public int hitCount = 1; // STRETCH GOAL (I think this already works tho)
    public int dmgMultiplier = 100;
    public int healMultiplier = 0;
    [BoxGroup("Ranges")] [InfoBox("FWD: 0 always unincluded, start at 1")]
    [BoxGroup("Ranges")] public Vector2 fwdRange = new Vector2(1, 1);
    [BoxGroup("Ranges")] [InfoBox("UP & DOWN: 0 is directly up and down, it is included")]
    [BoxGroup("Ranges")] public Vector2 upRange = Vector2.zero;
    [BoxGroup("Ranges")] public Vector2 downRange = Vector2.zero;
    public int aoe = 0; // STRETCH GOAL
    public int apCost = 1;
    public ActionIdentifier actionIdentifier;
    [ShowIf("isEmptyRoleTarget")] public bool moveToSelectedEmptyTile = false;
    [FormerlySerializedAs("useTarget")] public Role roleTarget;
    [FormerlySerializedAs("effects")] public List<BattlemodeEffectConfig> effectsToApplyToTarget = new();
    public List<BattlemodeEffectConfig> effectsToApplyToSelf = new();

    public BattlemodeActionCtx GenerateActionCtx(
        BattlemodeActionCtx.Status _status,
        BattlerOccupantCtx actingBatlerCtx,
        BattlerOccupantCtx targetBatlerCtx = null,
        int overideApCost = -1)
    {
        var ctx = new BattlemodeActionCtx
        {
            hitCount = this.hitCount,
            hitCountDelta = 0,
            dmg = actingBatlerCtx?.currentDMG ?? 0,
            heal = targetBatlerCtx?.maxHp ?? 0,
            deltaDmgMultiplier = this.dmgMultiplier,
            deltaHealMultiplier = this.healMultiplier,
            status = _status,
            cfg = this,
        };

        if (actingBatlerCtx is CharacterOccupantCtx)
            ctx.ap = apCost;
        
        if(overideApCost != -1)
            ctx.ap = overideApCost;
        
        ctx.apDelta = 0;

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