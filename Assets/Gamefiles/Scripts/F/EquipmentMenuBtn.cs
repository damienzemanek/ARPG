

using System;
using TMPro;

public class EquipmentMenuBtn : MenuUISlot<EquipmentMenuActionsDisplay, EquipmentConfig>
{
    public TextMeshProUGUI txt_name;
    protected override void InjectAndSetupImplementation(EquipmentConfig equipCfg)
    {
        img.sprite = equipCfg.icon;
    }

    void OnEnable()
    {
        Default();
        if(cfg != null) txt_name.text = cfg.name;
    }

    public override void Default()
    {
        if (dependancy.currentlySelectedBtn == this) return;
        ForceDefault();
        bgImg.color = darkened;
        txt_name.color = darkened;
    }

    public override void Hover()
    {
        if (state == BtnMenuUIState.Locked) return;
        img.color = defaultColor;
        bgImg.color = defaultColor;
        txt_name.color = defaultColor;
    }

    public override void Select()
    {
        if (dependancy.currentlySelectedBtn == this) return;
        if (state == BtnMenuUIState.Locked) return;
        bgImg.sprite = selectSprite;
        img.color = defaultColor;
        bgImg.color = defaultColor;
        txt_name.color = defaultColor;
        dependancy.SelectAction(this);
    }
    
}