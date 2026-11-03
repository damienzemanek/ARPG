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
        if (currentlySelectedBtn.state == BtnMenuUIState.Equipped) text_buttonLabelEquipDequip.text = "Dequip";
        else if(currentlySelectedBtn.state == BtnMenuUIState.Default) text_buttonLabelEquipDequip.text = "Equip";
        ShowEquipment(currentlySelectedBtn);
    }

    public void PressedEquipDequip()
    {
        if (currentlySelectedBtn.state != BtnMenuUIState.Equipped)
        {
            if(equippedDisplaySlots.All(s => s != null)) return;
            equippedDisplaySlots.First(e => e == null)
                .EquipOnSlot(currentlySelectedBtn);
            currentlySelectedBtn.Equip();
        }
        else
        {
            currentlySelectedBtn.Dequip();
            equippedDisplaySlots.Find(e => e.currentlyEquippedBtn == currentlySelectedBtn)
                .UnequipFromSlot();
        }
    }

    public void ShowEquipment(EquipmentMenuBtn equipBtn)
    {
        go_infoDisplay.SetActive(true);
        txt_currentlySelectedEquipment.text = equipBtn.cfg.name;
        txt_currentlySelectedEquipmentDesc.text = equipBtn.cfg.description;
    }
}