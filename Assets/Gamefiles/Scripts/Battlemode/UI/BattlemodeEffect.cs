using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BattlemodeEffectConfig;

public abstract class BattlemodeEffectBase : MonoBehaviour
{
    public abstract void Hide();
    public abstract void PopulateEffect(BattlemodeEffectCtx effectCtx);
}

public class BattlemodeEffect : BattlemodeEffectBase
{
    const string k_StackCountFormat = "{0} stack(s)";

    
    [ReadOnly] public BattlemodeEffectCtx effectCtx;
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
    
    public override void PopulateEffect(BattlemodeEffectCtx effectCtx)
    {
        turnEffect.Setup();
        battleEffect.Setup();
        expeditionEffect.Setup();
        
        this.effectCtx = effectCtx;
        txt_name.text = effectCtx.cfg.effectName;
        txt_description.text = effectCtx.cfg.description;
        img.sprite = effectCtx.cfg.icon;
        switch (effectCtx.cfg.time)
        {
            case EffectTime.Turn: turnEffect.UpdateStackCount(effectCtx.stacks); break;
            case EffectTime.Battle: battleEffect.UpdateStackCount(effectCtx.stacks); break;
            case EffectTime.Expedition: expeditionEffect.UpdateStackCount(effectCtx.stacks); break;
            default: throw new NotImplementedException();
        }
        txt_stackTotalNum.text = string.Format(k_StackCountFormat, effectCtx.stacks);
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