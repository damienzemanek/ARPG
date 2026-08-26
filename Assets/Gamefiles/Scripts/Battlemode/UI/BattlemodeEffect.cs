using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BattlemodeEffectConfigInstance;

public abstract class BattlemodeEffectBase : MonoBehaviour
{
    public abstract void Hide();
    public abstract void PopulateEffect(BattlemodeEffectStrategyInstance effect);
}

public class BattlemodeEffect : BattlemodeEffectBase
{
    const string k_StackCountFormat = "{0} stack(s)";

    
    [ReadOnly] public BattlemodeEffectStrategyInstance effectCtx;
    [Required] public TextMeshProUGUI txt_name;
    [Required] public TextMeshProUGUI txt_description;
    [Required] public Image img;
    
    [Required] public TextMeshProUGUI txt_stackTotalNum;
    public EffectTurn turnEffect = new(EffectTime.Turn);
    public EffectTurn battleEffect = new(EffectTime.Battle);
    public EffectTurn expeditionEffect = new(EffectTime.Expedition);
    

    public override void Hide()
    {
        effectCtx = null;
        img.sprite = null;
        txt_name.text = "";
        txt_description.text = "";
        gameObject.SetActive(false);
    }
    
    public override void PopulateEffect(BattlemodeEffectStrategyInstance effect)
    {
        turnEffect.Setup();
        battleEffect.Setup();
        expeditionEffect.Setup();
        
        this.effectCtx = effect;
        txt_name.text = effect.cfg.effectName;
        txt_description.text = effect.cfg.description;
        img.sprite = effect.cfg.icon;
        int turnStacks = effect.GetStackCtx(EffectTime.Turn).stacks;
        int battleStacks = effect.GetStackCtx(EffectTime.Battle).stacks;
        int expeditionStacks = effect.GetStackCtx(EffectTime.Expedition).stacks;
        turnEffect.UpdateStackCount(turnStacks);
        battleEffect.UpdateStackCount(battleStacks);
        expeditionEffect.UpdateStackCount(expeditionStacks);
        txt_stackTotalNum.text = string.Format(k_StackCountFormat, effect.stacksTotal);
    }

    [Serializable]
    public class EffectTurn
    {
        [ReadOnly] public EffectTime time;
        [Required] public TextMeshProUGUI txt_stackCount;
        public void Setup() => txt_stackCount.text = string.Format(k_StackCountFormat, 0);
        public void UpdateStackCount(int stacks) => txt_stackCount.text = string.Format(k_StackCountFormat, stacks);
        public EffectTurn(EffectTime time) => this.time = time;
    }
}