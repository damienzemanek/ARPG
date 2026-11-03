

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDisplaySlot : MonoBehaviour
{
    [Required] public EquipmentMenuActionsDisplay dependancy;
    public EquipmentMenuBtn currentlyEquippedBtn = null;
    
    public Color defaultColor;
    public Color darkenedColor;
    
    [Required] public Image bgImg;
    [Required] public Image sprImg;
    [Required] public Sprite defaultSprite;
    
    public void EquipOnSlot(EquipmentMenuBtn btn)
    {
        this.currentlyEquippedBtn = btn;
        sprImg.sprite = btn.cfg.icon;
        sprImg.gameObject.SetActive(true);
    }

    public void UnequipFromSlot()
    {
        this.currentlyEquippedBtn = null;
        sprImg.sprite = null;
        sprImg.gameObject.SetActive(false);
    }

    public void Default()
    {
        if(currentlyEquippedBtn != null) 
            if(dependancy.currentlySelectedBtn == currentlyEquippedBtn) return;
        bgImg.color = darkenedColor;
        sprImg.color = darkenedColor;
        if(currentlyEquippedBtn == null) UnequipFromSlot();
    }

    public void Hover()
    {
        bgImg.color = defaultColor;
        sprImg.color = defaultColor;
    }

    public void SelectSlot()
    {
        bgImg.color = defaultColor;
        sprImg.color = defaultColor;
        dependancy.SelectAction(currentlyEquippedBtn);
    }
}