using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.Serialization;
using AYellowpaper.SerializedCollections;
using UnityEngine.Serialization;

[Serializable]
public class BattlemodeEffectConfigInstance
{
    public SerializedDictionary<string, EffectDict.EffectData> effectData 
        => PersistentConfigurationDataHolder.Instance.effectDict.data;

    
    
    
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


    public AddOccurance addOccurance;
    
    
    [SerializeReference] public BattlemodeEffectStrategyInstance defaultEffectStrategyValues;
    public string typeKey => defaultEffectStrategyValues.GetType().Name;
    public string effectName { get { var ret = effectData.GetValueOrDefault(typeKey).name; if (String.IsNullOrEmpty(ret)) ret = ""; return ret; } }
    public Sprite icon => effectData.GetValueOrDefault(typeKey).icon;
    public string description => effectData.GetValueOrDefault(typeKey).description;
    public BattlemodeEffectStrategyInstance CreateNewEffectInstance()
        => defaultEffectStrategyValues.Clone(this);
}


[Serializable]
public abstract class BattlemodeEffectStrategyInstance
{
    static int effectValuesLength = Enum.GetValues(typeof(BattlemodeEffectConfigInstance.EffectTime)).Length;
    public int stacksTotal => ctx.stackCtxs.Sum(stackCtx => stackCtx.stacks);

    public bool HasStackCtx(BattlemodeEffectConfigInstance.EffectTime effectTime) => Array.Exists(ctx.stackCtxs, s => s.instanceEffectTime == effectTime);

    public ref StackCtx GetStackCtx(BattlemodeEffectConfigInstance.EffectTime effectTime)
    {
        int index = Array.FindIndex(ctx.stackCtxs, s => s.instanceEffectTime == effectTime);
        return ref ctx.stackCtxs[index];
    }
    
    [Serializable, InlineProperty]
    public struct BattlemodeEffectStrategyCtx
    {
        public bool markedForRemoval = false;
        public bool markForReResolve = false;
        public StackCtx[] stackCtxs;
        public BattlemodeEffectStrategyCtx() { }
    }

    [Serializable]
    public struct StackCtx
    {
        public BattlemodeEffectConfigInstance.EffectTime instanceEffectTime;
        public int stacks;
    }
    
    [NonSerialized] public BattlemodeEffectConfigInstance cfg;
    [SerializeField] public BattlemodeEffectStrategyCtx ctx;
    
    public abstract bool isSpecial { get; }

    
    public BattlemodeEffectStrategyInstance Clone(BattlemodeEffectConfigInstance cfg)
    {
        var clone = (BattlemodeEffectStrategyInstance)MemberwiseClone();
        clone.ctx.stackCtxs = new StackCtx[ctx.stackCtxs.Length];
        clone.cfg = cfg;
        clone.ctx.stackCtxs = new StackCtx[effectValuesLength];
        
        bool cont = (cfg.defaultEffectStrategyValues.ctx.stackCtxs == null ||
                     cfg.defaultEffectStrategyValues.ctx.stackCtxs.Length == 0);
        
        for (int i = 0; i < clone.ctx.stackCtxs.Length; i++)
        {
            clone.ctx.stackCtxs[i] = new StackCtx {instanceEffectTime = (BattlemodeEffectConfigInstance.EffectTime)i};
            clone.ctx.stackCtxs[i].stacks = 0;
            if (cont) continue;
            if(cfg.defaultEffectStrategyValues.HasStackCtx((BattlemodeEffectConfigInstance.EffectTime)i))
                clone.ctx.stackCtxs[i].stacks = cfg.defaultEffectStrategyValues.GetStackCtx((BattlemodeEffectConfigInstance.EffectTime)i).stacks;
        }
        return clone;
    }
    
    // Resolve Occurances
    // ------------------
    // BeforeHitByAction,
    // AfterHitByAction,
    // BeforeActing,
    // AfterActing,
    // AfterTurnEnds,
    // StartOfBattle
    
    // Mutates Directly
    public virtual BattlemodeActionCtx ResolveEffectBeforeHitByAction(
        OccupantCtx occupantCtx,
        BattlemodeActionCtx actionCtx) => actionCtx;
    
    public virtual BattlemodeActionCtx ResolveEffectAfterHitByAction(
        OccupantCtx occupantCtx,
        BattlemodeActionCtx actionCtx) => actionCtx;
    
    public virtual BattlemodeActionCtx ResolveEffectBeforeActing(
        OccupantCtx occupantCtx,
        BattlemodeActionCtx actionCtx) => actionCtx;
    
    public virtual BattlemodeActionCtx ResolveEffectAfterActing(
        OccupantCtx occupantCtx,
        BattlemodeActionCtx actionCtx) => actionCtx;
    
    public virtual void ResolveEffectAfterTurnEnds(
        OccupantCtx occupantCtx) { }
    
    public virtual BattlemodeActionCtx ResolveEffecStartOfBattle(
        OccupantCtx occupantCtx,
        BattlemodeActionCtx actionCtx) => actionCtx;
}


// A strategy is instantiated for every ctx, oh well i will change later probably