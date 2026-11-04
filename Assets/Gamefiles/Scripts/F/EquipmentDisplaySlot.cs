

using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDisplaySlot : MonoBehaviour
{
    public bool isEquipped => currentlyEquippedBtn != null;
    
    [Required] public EquipmentMenuActionsDisplay dependancy;
    public EquipmentMenuBtn currentlyEquippedBtn = null;
    
    public Color defaultColor;
    public Color darkenedColor;
    
    [Required] public Image bgImg;
    [Required] public Image sprImg;
    [Required] public Sprite defaultSprite;
    [Required] public Sprite selectedSprite;
    [Required] public TextMeshProUGUI txt_name;
    
    
    public void EquipOnSlot(EquipmentMenuBtn btn)
    {
        this.currentlyEquippedBtn = btn;
        sprImg.sprite = btn.cfg.icon;
        sprImg.gameObject.SetActive(true);
        txt_name.text = btn.cfg.name;
    }

    public EquipmentDisplaySlot UnequipFromSlot()
    {
        this.currentlyEquippedBtn = null;
        sprImg.sprite = null;
        sprImg.gameObject.SetActive(false);
        txt_name.text = "";
        return this;
    }

    public void Default()
    {
        if(currentlyEquippedBtn != null) 
            if(dependancy.currentlySelectedBtn == currentlyEquippedBtn) return;
        bgImg.sprite = defaultSprite;
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
        bgImg.sprite = selectedSprite;
    }

    public void SelectFromBtn()
    {
        if(currentlyEquippedBtn == null) return;
        currentlyEquippedBtn.Select();
    }
}