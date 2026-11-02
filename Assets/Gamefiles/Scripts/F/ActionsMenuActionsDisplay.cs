using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using static ActionsMenuActionBtn;

public class ActionsMenuActionsDisplay : MonoBehaviour
{
    [ReadOnly] public CharacterConfig inspectingCharacterActions;
    [ReadOnly] public ActionsMenuActionBtn currentlySelectedActionBtn = null;
    [Required] public GameObject go_equipDequipBtn;
    [Required] public TextMeshProUGUI text_buttonLabelEquipDequip;

    public List<ActionsMenuActionBtn> actionBtns = new();
    
    [Button]
    public void GrabAllBtns(Transform parent) => actionBtns.AddRange(parent.GetComponentsInChildren<ActionsMenuActionBtn>());

    void Awake() => go_equipDequipBtn.SetActive(false);

    public void DisplayActionsFor(CharacterConfig cfg)
    {
        if(actionBtns == null || actionBtns.Count <= 0) this.Log("No ActionBtns are saved to actionsBtn list");
        if(cfg.AllAvaliableActions.Count > actionBtns.Count) Debug.LogError("Max equippable actions greater than action btns amount");

        this.Log("Displaying actions for: " + cfg.occupantName + "");
        actionBtns.ForEach(b => b.ResetActionBtn());
        currentlySelectedActionBtn = null;
        inspectingCharacterActions = cfg;
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out _, out var charData);
        SaverService.Instance.GetSaverAndData<Items_SavedDataSO>(out _, out var itemData);
        var currEquippedActions = charData.charactersData.Find(c => c.characterConfigName == inspectingCharacterActions.occupantName).equippedActions;
        this.Log("Current equipped actions: " + string.Join(", ", currEquippedActions));

        for (int i = 0; i < inspectingCharacterActions.equippedActions.Length; i++)
            inspectingCharacterActions.equippedActions[i] = null;
        
        for (int i = 0; i < inspectingCharacterActions.AllAvaliableActions.Count; i++)
        {
            actionBtns[i].gameObject.SetActive(true);
            actionBtns[i].InjectActionAndSetup(inspectingCharacterActions.AllAvaliableActions[i]);
            
            var thisActionsName = inspectingCharacterActions.AllAvaliableActions[i].actionName;
            if (currEquippedActions.Contains(thisActionsName))
            {
                this.Log("Equipping action: " + thisActionsName);
                actionBtns[i].EquipAction();
                TryEquipActionOnBtn(inspectingCharacterActions.AllAvaliableActions[i], actionBtns[i]);
            }
            else
            {
                this.Log("Dequipping action: " + thisActionsName);
                actionBtns[i].DequipAction();
                DequipActionOnBtn(inspectingCharacterActions.AllAvaliableActions[i], actionBtns[i]);
            }
            
            var actionData = itemData.GetItemData(Items_SavedDataSO.ItemType.CharacterAction, thisActionsName);
            if(actionData.amount <= 0)
            {
                this.Log("Locking action: " + thisActionsName + " its value is " + actionData.amount);
                actionBtns[i].LockAction();
            }
            
        }
    }

    
    public void SelectAction(ActionsMenuActionBtn actionBtn)
    {
        if(currentlySelectedActionBtn != null) currentlySelectedActionBtn.ForceDefault();
        currentlySelectedActionBtn = actionBtn;
        go_equipDequipBtn.gameObject.SetActive(true);
        if (currentlySelectedActionBtn.state == ActionBtnMenuUIState.Equipped) text_buttonLabelEquipDequip.text = "Dequip";
        else if(currentlySelectedActionBtn.state == ActionBtnMenuUIState.Default) text_buttonLabelEquipDequip.text = "Equip";
    }

    public void PressedEquipDequip()
    {
        if (currentlySelectedActionBtn.state != ActionBtnMenuUIState.Equipped) TryEquipSelected();
        else DequipSelected();
    }

    public void TryEquipSelected()
    {
        TryEquipActionOnBtn(currentlySelectedActionBtn.actionCfg, currentlySelectedActionBtn);
        
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var saver, out var charData);
        charData.charactersData.Find(c => c.characterConfigName == inspectingCharacterActions.occupantName)
            .equippedActions
            .Add(currentlySelectedActionBtn.actionCfg.actionName);
        
        saver.Save();
    }
    
    public void TryEquipActionOnBtn(BattlemodeActionConfig actionCfg, ActionsMenuActionBtn actionBtn)
    {
        this.Log("Trying to equip action: " + actionCfg.name);
        bool successfullyEquipped = inspectingCharacterActions.TryEquipActionOnNextAvaliableSlot(actionCfg);
        if (!successfullyEquipped) return;
        actionBtn.EquipAction();
        this.Log("Equipped action: " + actionCfg.name);
    }

    public void DequipSelected()
    {
        DequipActionOnBtn(currentlySelectedActionBtn.actionCfg, currentlySelectedActionBtn);
        
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var saver, out var charData);
        charData.charactersData.Find(c => c.characterConfigName == inspectingCharacterActions.occupantName)
            .equippedActions
            .Remove(currentlySelectedActionBtn.actionCfg.actionName);
        
        saver.Save();
    }
    
    public void DequipActionOnBtn(BattlemodeActionConfig actionCfg, ActionsMenuActionBtn actionBtn)
    {
        inspectingCharacterActions.DequipAction(actionCfg);
        actionBtn.DequipAction();
        this.Log("Dequipped action: " + actionCfg.name);
    }
}