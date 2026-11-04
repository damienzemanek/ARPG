using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ActionsMenuActionBtn : MenuUISlot<ActionsMenuActionsDisplay, BattlemodeActionConfig>
{
    protected override void InjectAndSetupImplementation(BattlemodeActionConfig actionCfg)
    {
        img.sprite = actionCfg.icon;
    }

    public override void Default()
    {
        if (dependancy.currentlySelectedActionBtn == this) return;
        ForceDefault();
    }

    public override void Hover()
    {
        if (state == BtnMenuUIState.Locked) return;
        img.color = defaultColor;
    }

    public override void Select()
    {
        if (dependancy.currentlySelectedActionBtn == this) return;
        if (state == BtnMenuUIState.Locked) return;
        bgImg.sprite = selectSprite;
        img.color = defaultColor;
        dependancy.SelectAction(this);
    }
    
}