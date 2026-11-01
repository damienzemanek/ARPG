using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
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
        
        currentlySelectedActionBtn = null;
        inspectingCharacterActions = cfg;
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out _, out var charData);
        var equippedActions = charData.charactersData.Find(c => c.characterConfigName == inspectingCharacterActions.occupantName).equippedActions;
        
        
        actionBtns.ForEach(b => b.ResetActionBtn());
        if(inspectingCharacterActions.AllAvaliableActions.Count > actionBtns.Count) Debug.LogError("Max equippable actions greater than action btns amount");
        for (int i = 0; i < cfg.AllAvaliableActions.Count; i++)
        {
            actionBtns[i].gameObject.SetActive(true);
            actionBtns[i].InjectActionAndSetup(cfg.AllAvaliableActions[i]);
            
            var thisActionsName = inspectingCharacterActions.AllAvaliableActions[i].actionName;
            if(equippedActions.Contains(thisActionsName)) 
                actionBtns[i].EquipAction();
            else 
                actionBtns[i].DequipAction();
;
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
        this.Log("Trying to equip action: " + currentlySelectedActionBtn.actionCfg.name);
        bool successfullyEquipped = inspectingCharacterActions.TryEquipActionOnNextAvaliableSlot(currentlySelectedActionBtn.actionCfg);
        if (!successfullyEquipped) return;
        currentlySelectedActionBtn.EquipAction();
        this.Log("Equipped action: " + currentlySelectedActionBtn.actionCfg.name);
    }

    public void DequipSelected()
    {
        inspectingCharacterActions.DequipAction(currentlySelectedActionBtn.actionCfg);
        currentlySelectedActionBtn.DequipAction();
        this.Log("Dequipped action: " + currentlySelectedActionBtn.actionCfg.name);
    }
}