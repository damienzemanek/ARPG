using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class BattlemodeSpecialEffect : BattlemodeEffectBase
{
    const string k_EffectNameFormat = "Special: {0}";
    const string k_StackCountFormat = "{0} stack(s)";
    
    [ReadOnly] public BattlemodeEffectStrategyInstance effectCtx;
    [Required] public TextMeshProUGUI txt_name;
    [Required] public TextMeshProUGUI txt_description;
    [Required] public TextMeshProUGUI txt_stackTotalNum;
    
    public override void Hide()
    {
        effectCtx = null;
        txt_name.text = "";
        txt_description.text = "";
        gameObject.SetActive(false);
    }
    
    public override void PopulateEffect(BattlemodeEffectStrategyInstance effect)
    {
        this.effectCtx = effect;
        txt_name.text = string.Format(k_EffectNameFormat, effect.cfg.effectName);
        txt_description.text = effect.cfg.description;
        txt_stackTotalNum.text = string.Format(k_StackCountFormat, effect.stacksTotal);
    }
}