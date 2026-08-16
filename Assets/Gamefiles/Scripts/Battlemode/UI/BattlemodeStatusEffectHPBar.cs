using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

public class BattlemodeStatusEffectHPBar : BattlemodeEffectBase
{
    [Required] public Image img;
    [Required] public TextMeshProUGUI txt_stacksNum;
    
    public override void Hide()
    {
        img.sprite = null;
        txt_stacksNum.text = "";
        gameObject.SetActive(false);
    }
    
    public override void PopulateEffect(BattlemodeEffectCtx effectCtx)
    {
        img.sprite = effectCtx.cfg.icon;
        txt_stacksNum.text = effectCtx.stacks.ToString();
    }
}