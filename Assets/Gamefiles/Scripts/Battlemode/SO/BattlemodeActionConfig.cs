using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BattlemodeActionConfig", menuName = "ARPG/SO/BattlemodeActionConfig")]
public class BattlemodeActionConfig : ScriptableObject
{
    bool isEmptyRoleTarget => roleTarget == Role.EmptyTile;
    bool isIncreasingArmor => armorIncreasePercentage > 0;
    bool isMarkHitting => actionQualifier.HasFlag(ActionQualifier.MarkHit);
    
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

    public enum TargetingPattern
    {
        None,
        Self,
        Cross,
        Box,
    }

    [Flags]
    public enum BodyPart
    {
        None = 0,
        Head = 1 << 0,
        Body = 1 << 1,
        Legs = 1 << 2
    }

    [Serializable]
    public struct TargetingCfg
    {
        public TargetingPattern targetingPatternAdditive = TargetingPattern.None;
        [BoxGroup("Row Targeting")] public BattleTile.RowRank usableInRowRanks = BattleTile.RowRank.None;
        [BoxGroup("Row Targeting")] public BattleTile.RowRank targetRowRanks = BattleTile.RowRank.None;
        [BoxGroup("Row Targeting")] public bool targetCurrentRow = false;
        [BoxGroup("Col Targeting")] public BattleTile.ColRank usableInColRanks = BattleTile.ColRank.None;
        [BoxGroup("Col Targeting")] public BattleTile.ColRank targetColRanks = BattleTile.ColRank.None;
        [BoxGroup("Col Targeting")] public bool targetCurrentCol = false;
        public TargetingCfg() { }
    }

    public enum MovementDirection
    {
        None,
        Right,
        Left,
        Up,
        Down,
    }
    
    [Serializable]
    public struct MovementCfg
    {
        public MovementDirection direction;
        public int amount;
        public MovementCfg() { }
    }

    [Flags]
    public enum ActionQualifier
    {
        None = 0,
        Exhuast = 1 << 0,
        UnExhuastAll = 1 << 1,
        MarkHit = 1 << 2,
        ConsumeMark = 1 << 3,
    }
    
    [BoxGroup("Settings")] public string actionName;
    [BoxGroup("Settings")] public string description;
    [BoxGroup("Settings")] public int apCost = 1;
    [BoxGroup("Settings")] public Role roleTarget;
    [BoxGroup("Settings")] [ShowIf("isEmptyRoleTarget")] public bool moveToSelectedEmptyTile = false;
    [BoxGroup("Settings")] public ActionIdentifier actionIdentifier;
    [BoxGroup("Settings")] public ActionQualifier actionQualifier;
    [BoxGroup("Settings")] [ShowIf("isMarkHitting")] public MarkStrategy markStrategy;
    [BoxGroup("Settings")] public BodyPart targetedBodyPart;
    [BoxGroup("Settings")] public bool useActionAnim = true;
    [BoxGroup("Settings")] public bool useActionDelays = true;
    [BoxGroup("Settings")] public Sprite icon;
    [BoxGroup("Settings")] public int hitCount = 1; 
    [BoxGroup("Settings")] public int maxEocTargetingAttempts = 2; // STRETCH GOAL (I think this already works tho)
    [BoxGroup("Settings")] [InfoBox("This is the percentage of DMG of SELF to be inflicted on TARGET")] public int dmgMultiplier = 100;
    [BoxGroup("Settings")] [InfoBox("This is the percentage of max hp of TARGET to be healed")] public int healPercentage = 0;
    [BoxGroup("Settings")] [InfoBox("This is the percentage of armor to be generated")] public int armorIncreasePercentage = 0;
    [BoxGroup("Settings")] [ShowIf("isIncreasingArmor")] public bool capArmorIncrease;

    [BoxGroup("Cfgs")] public TargetingCfg targetingCfg;
    [BoxGroup("Cfgs")] public MovementCfg movementCfg;
    [BoxGroup("Cfgs")] public MovementCfg targetMovementCfg;

    
    [BoxGroup("Effects")] public List<BattlemodeEffectConfigInstance> effectsToApplyToTarget = new();
    [BoxGroup("Effects")] public List<BattlemodeEffectConfigInstance> effectsToApplyToSelf = new();
    
    public int aoe = 0; // STRETCH GOAL
    

    public BattlemodeActionCtx GenerateActionCtx(
        BattlemodeActionCtx.Status _status,
        BattlerOccupantCtx actingBatlerCtx,
        BattlerOccupantCtx targetBatlerCtx = null,
        int overideApCost = -1)
    {
        var ctx = new BattlemodeActionCtx
        {
            currentHitCount = 1,
            maxHitCount = this.hitCount,
            hitCountDelta = 0,
            dmg = actingBatlerCtx?.currentDMG ?? 0,
            heal = targetBatlerCtx?.maxHp ?? 0,
            armor = actingBatlerCtx?.maxArmor ?? 0,
            deltaDmgMultiplier = this.dmgMultiplier,
            deltaHealMultiplier = this.healPercentage,
            deltaArmorMultiplier = this.armorIncreasePercentage,
            status = _status,
            cfg = this,
            movementCfgInstanced = this.movementCfg,
            isArmorPiercing = false,
            targetMovementCfgInstanced = this.targetMovementCfg,
            targetingCfgInstanced = this.targetingCfg,
            critChanceCalculatedAlready = false,
            brokenBodyPart = BattlemodeActionConfig.BodyPart.None,
            markStrategy = markStrategy,
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

    public int currentHitCount;
    public int maxHitCount;
    public int dmg;
    public int heal;
    public int armor;
    public int ap;
    
    public int hitCountDelta;
    public int apDelta;
    public float deltaDmgMultiplier;
    public float deltaHealMultiplier;
    public float deltaArmorMultiplier;
    public bool hasCritChance;
    public bool critHit;
    public bool critChanceCalculatedAlready;
    public bool isArmorPiercing;
    
    public bool bodyPartAlreadyBrokenThisAction => brokenBodyPart != BattlemodeActionConfig.BodyPart.None;
    public BattlemodeActionConfig.BodyPart brokenBodyPart;

    public BattlemodeActionConfig.TargetingCfg targetingCfgInstanced;
    public BattlemodeActionConfig.MovementCfg movementCfgInstanced;
    public BattlemodeActionConfig.MovementCfg targetMovementCfgInstanced;
    public BattlemodeActionConfig cfg;
    public MarkStrategy markStrategy;

    
    public List<BattlemodeEffectStrategyInstance> additionalEffectsToApplyToActor;
    public List<BattlemodeEffectStrategyInstance> additionalEffectsToApplyToTarget;

    public void SetHealViaTargetMaxHealth(BattlerOccupantCtx targetBatlerCtx)
        => heal = targetBatlerCtx?.maxHp ?? 0;
}