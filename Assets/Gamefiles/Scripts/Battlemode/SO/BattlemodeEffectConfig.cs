using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.Serialization;
using AYellowpaper.SerializedCollections;
using UnityEngine.Serialization;

[Serializable]
public class BattlemodeEffectConfig
{
    public SerializedDictionary<string, EffectDict.EffectData> effectData 
        => PersistentConfigurationDataHolder.Instance.effectDict.data;

    
    [Flags]
    public enum RemovalOccurrence
    {
        None            = 0,
        OnHit           = 1 << 0,
        OnPostAttack    = 1 << 1,
        OnTurnEnd       = 1 << 2,
        OnBattleEnd     = 1 << 3,
        OnExpeditionEnd = 1 << 4,
    }
    
    
    public enum EffectTime
    {
        Turn,
        Battle,
        Expedition,
        Permanent,
    }
    
    public enum AddOccurance
    {
        None,
        BeforeAction,
        AfterAction,
    }
    
    public enum ResolveOccurance
    {
        BeforeHitByAction,
        AfterHitByAction,
        BeforeActing,
        AfterActing,
        AfterTurnEnds,
        StartOfBattle
    }

    public AddOccurance addOccurance;
    public EffectTime time;
    public int stacks;
    
    [SerializeField]
    public TypeSerialized<BattlemodeEffectStrategy> effectStrategy;
    public string typeKey => effectStrategy.Type.Name;
    public string effectName
    {
        get
        {
            var ret = effectData.GetValueOrDefault(typeKey).name;
            Debug.Log("Found effect: " + ret + ".");
            if (String.IsNullOrEmpty(ret)) ret = "";
            Debug.Log($"effectName: {ret}");
            return ret;
        }
    }
    
    public Sprite icon => effectData.GetValueOrDefault(typeKey).icon;
    public string description => effectData.GetValueOrDefault(typeKey).description;

    public BattlemodeEffectCtx GenerateEffectCtx()
    {
        BattlemodeEffectCtx effect = new BattlemodeEffectCtx();
        effect.effectStrategy = Activator.CreateInstance(effectStrategy.Type) as BattlemodeEffectStrategy;
        effect.cfg = this;
        effect.instanceEffectTime = time;
        effect.stacks = stacks;

        return effect;
    }
}

[Serializable]
public class BattlemodeEffectCtx
{
    public bool markedForRemoval = false;
    [SerializeReference] public BattlemodeEffectStrategy effectStrategy;
    [FormerlySerializedAs("effectCfg")] public BattlemodeEffectConfig cfg;
    [FormerlySerializedAs("effectTime")] public BattlemodeEffectConfig.EffectTime instanceEffectTime;
    public int stacks;

    // Mutates Directly
    public BattlemodeActionCtx ResolveEffect(
        OccupantCtx occupantCtx,
        BattlemodeActionCtx actionCtx)
    {
        var newActionCtx = effectStrategy.ResolveEffect(
            occupantCtx,
            actionCtx,
            stacks);

        if (effectStrategy.removealOccurance == BattlemodeEffectConfig.RemovalOccurrence.None
            || instanceEffectTime == BattlemodeEffectConfig.EffectTime.Permanent)
            return newActionCtx;

        switch (actionCtx.status)
        {
            case BattlemodeActionCtx.Status.BeingHit:
                if (effectStrategy.removealOccurance.HasFlag(BattlemodeEffectConfig.RemovalOccurrence.OnHit))
                    stacks--; break;

            case BattlemodeActionCtx.Status.Acting:
                if (effectStrategy.removealOccurance.HasFlag(BattlemodeEffectConfig.RemovalOccurrence.OnPostAttack))
                    stacks--; break;

            case BattlemodeActionCtx.Status.TurnEnd:
                if (effectStrategy.removealOccurance.HasFlag(BattlemodeEffectConfig.RemovalOccurrence.OnTurnEnd))
                    stacks--; break;

            case BattlemodeActionCtx.Status.BattleEnd:
                if (effectStrategy.removealOccurance.HasFlag(BattlemodeEffectConfig.RemovalOccurrence.OnBattleEnd))
                    stacks--; break;

            case BattlemodeActionCtx.Status.ExpeditionEnd:
                if (effectStrategy.removealOccurance.HasFlag(BattlemodeEffectConfig.RemovalOccurrence.OnExpeditionEnd))
                    stacks--; break;
        }

        if (stacks <= 0) markedForRemoval = true;
        return newActionCtx;
    }
    
}

[Serializable]
public abstract class BattlemodeEffectStrategy
{
    public abstract bool isSpecial { get; }
    public abstract BattlemodeEffectConfig.RemovalOccurrence removealOccurance { get; }
    public abstract BattlemodeEffectConfig.ResolveOccurance resolveOccurance { get; }

    
    // Mutates Directly
    public abstract BattlemodeActionCtx ResolveEffect(
        OccupantCtx occupantCtx,
        BattlemodeActionCtx actionCtx,
        int stacks);
}

// A strategy is instantiated for every ctx, oh well i will change later probably
[Serializable]
public sealed class BattlemodeEffectStrategy_Vulnerable : BattlemodeEffectStrategy
{
    public override bool isSpecial => false;
    public override BattlemodeEffectConfig.RemovalOccurrence removealOccurance => BattlemodeEffectConfig.RemovalOccurrence.OnTurnEnd;
    public override BattlemodeEffectConfig.ResolveOccurance resolveOccurance => BattlemodeEffectConfig.ResolveOccurance.BeforeHitByAction;

    public override BattlemodeActionCtx ResolveEffect(
        OccupantCtx occupantCtx,
        BattlemodeActionCtx actionCtx,
        int stacks)
    {
        actionCtx.dmg *= 2;
        return actionCtx;
    }
}