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
    
    public override void PopulateEffect(BattlemodeEffectStrategyInstance effect)
    {
        img.sprite = effect.cfg.icon;
        txt_stacksNum.text = effect.stacksTotal.ToString();
    }
}