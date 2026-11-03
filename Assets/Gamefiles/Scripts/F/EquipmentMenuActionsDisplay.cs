using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EquipmentMenuActionsDisplay : MonoBehaviour
{
    public EquipmentMenuBtn currentlySelectedBtn = null;
    
    public GameObject go_equipDequipBtn;
    public TextMeshProUGUI text_buttonLabelEquipDequip;
    
    
    public List<EquipmentDisplaySlot> equippedDisplaySlots = new();

    public GameObject go_infoDisplay;
    public TextMeshProUGUI txt_currentlySelectedEquipment;
    public TextMeshProUGUI txt_currentlySelectedEquipmentDesc;

    void OnEnable() => Refresh();

    public void Refresh()
    {
        go_equipDequipBtn.SetActive(false);
        go_infoDisplay.SetActive(false);
        currentlySelectedBtn = null;
        equippedDisplaySlots.ForEach(s => s.Default());
    }

    public void SelectAction(EquipmentMenuBtn btn)
    {
        if(currentlySelectedBtn != null) currentlySelectedBtn.ForceDefault();
        currentlySelectedBtn = btn;
        go_equipDequipBtn.gameObject.SetActive(true);
        go_infoDisplay.SetActive(true);
        equippedDisplaySlots.ForEach(s => s.Default());
        if (currentlySelectedBtn.state == BtnMenuUIState.Equipped)
        {
            text_buttonLabelEquipDequip.text = "Dequip";
            equippedDisplaySlots
                .Find(e => e.currentlyEquippedBtn == currentlySelectedBtn)
                .SelectSlot();
        }
        else if (currentlySelectedBtn.state == BtnMenuUIState.Default)
        {
            text_buttonLabelEquipDequip.text = "Equip";
            equippedDisplaySlots.ForEach(s => s.Default());
        }
        ShowEquipment(currentlySelectedBtn);
    }

    public void PressedEquipDequip()
    {
        Debug.Log("Pressed Equip Dequip");
        if (currentlySelectedBtn.state != BtnMenuUIState.Equipped)
        {
            if(equippedDisplaySlots.All(s => s.isEquipped)) return;
            
            Debug.Log("Equipping : " + currentlySelectedBtn.cfg.name + "");
            var slot = equippedDisplaySlots.First(e => !e.isEquipped);
            slot.EquipOnSlot(currentlySelectedBtn);
            slot.SelectSlot();
            currentlySelectedBtn.Equip();
        }
        else
        {
            Debug.Log("Dequipping : " + currentlySelectedBtn.cfg.name + "");
            currentlySelectedBtn.Dequip();
            equippedDisplaySlots.Find(e => e.currentlyEquippedBtn == currentlySelectedBtn)
                .UnequipFromSlot()
                .Default();
        }
    }

    public void ShowEquipment(EquipmentMenuBtn equipBtn)
    {
        go_infoDisplay.SetActive(true);
        txt_currentlySelectedEquipment.text = equipBtn.cfg.name;
        txt_currentlySelectedEquipmentDesc.text = equipBtn.cfg.description;
    }
}