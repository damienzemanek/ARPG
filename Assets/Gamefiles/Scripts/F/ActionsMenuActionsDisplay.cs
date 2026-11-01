using System;
using System.Collections.Generic;
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

    private void Awake() => go_equipDequipBtn.SetActive(false);

    public void DisplayActionsFor(CharacterConfig cfg)
    {
        if(actionBtns == null || actionBtns.Count <= 0) this.Log("No ActionBtns are saved to actionsBtn list");

        currentlySelectedActionBtn = null;
        inspectingCharacterActions = cfg;
        actionBtns.ForEach(b => b.ResetActionBtn());
        if(cfg.maxEquippableActions > actionBtns.Count) Debug.LogError("Max equippable actions greater than action btns amount");
        for (int i = 0; i < cfg.maxEquippableActions; i++)
        {
            actionBtns[i].gameObject.SetActive(true);
            actionBtns[i].InjectActionAndSetup(cfg.equippedActions[i]);
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
        go_equipDequipBtn.SetActive(true);
        if (currentlySelectedActionBtn.state == ActionBtnMenuUIState.Equipped) EquipSelected();
        else DequipSelected();
    }

    public void EquipSelected() => currentlySelectedActionBtn.EquipAction();
    public void DequipSelected() => currentlySelectedActionBtn.DequipAction();
}