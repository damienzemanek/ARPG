

using System;

public class EquipmentMenuBtn : MenuUISlot<EquipmentMenuActionsDisplay, EquipmentConfig>
{
    protected override void InjectAndSetupImplementation(EquipmentConfig equipCfg)
    {
        img.sprite = equipCfg.icon;
    }

    void OnEnable()
    {
        Default();
    }

    public override void Default()
    {
        if (dependancy.currentlySelectedBtn == this) return;
        ForceDefault();
    }

    public override void Hover()
    {
        if (state == BtnMenuUIState.Locked) return;
        img.color = defaultColor;
    }

    public override void Select()
    {
        if (dependancy.currentlySelectedBtn == this) return;
        if (state == BtnMenuUIState.Locked) return;
        bgImg.sprite = selectSprite;
        img.color = defaultColor;
        dependancy.SelectAction(this);
    }
    
}