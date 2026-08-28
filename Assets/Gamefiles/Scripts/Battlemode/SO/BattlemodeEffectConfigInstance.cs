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
        int indx = Array.FindIndex(ctx.stackCtxs, s => s.instanceEffectTime == effectTime);
        return ref ctx.stackCtxs[indx];
    }
    
    [Serializable, InlineProperty]
    public struct BattlemodeEffectStrategyCtx
    {
        public bool markedForRemoval = false;
        public StackCtx[] stackCtxs;
        public BattlemodeEffectStrategyCtx() { }
    }

    [Serializable]
    public struct StackCtx
    {
        public BattlemodeEffectConfigInstance.EffectTime instanceEffectTime;
        public int stacks;
        public bool setStacksDirectly;
    }
    
    [NonSerialized] public BattlemodeEffectConfigInstance cfg;
    [SerializeField] public BattlemodeEffectStrategyCtx ctx;
    
    public abstract bool isSpecial { get; }

    
    public BattlemodeEffectStrategyInstance Clone(BattlemodeEffectConfigInstance cfg)
    {
        var clone = (BattlemodeEffectStrategyInstance)MemberwiseClone();

        clone.cfg = cfg;
        clone.ctx.stackCtxs = new StackCtx[effectValuesLength];

        bool cont = cfg.defaultEffectStrategyValues.ctx.stackCtxs == null ||
                    cfg.defaultEffectStrategyValues.ctx.stackCtxs.Length == 0;

        for (int i = 0; i < clone.ctx.stackCtxs.Length; i++)
        {
            var effectTime = (BattlemodeEffectConfigInstance.EffectTime)i;
            clone.ctx.stackCtxs[i] = new StackCtx
            {
                instanceEffectTime = effectTime,
                stacks = 0,
                setStacksDirectly = false
            };

            if (cont) continue;
            if (cfg.defaultEffectStrategyValues.HasStackCtx(effectTime))
            {
                var defaultStackCtx = cfg.defaultEffectStrategyValues.GetStackCtx(effectTime);

                clone.ctx.stackCtxs[i].stacks = defaultStackCtx.stacks;
                clone.ctx.stackCtxs[i].setStacksDirectly = defaultStackCtx.setStacksDirectly;
            }
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