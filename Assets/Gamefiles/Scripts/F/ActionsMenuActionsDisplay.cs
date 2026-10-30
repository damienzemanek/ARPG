using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using static ActionsMenuActionBtn;

public class ActionsMenuActionsDisplay : MonoBehaviour
{
    public ActionsMenuActionBtn currentlySelectedActionBtn = null;
    [Required] public TextMeshProUGUI text_buttonLabelEquipDequip;
    
    public void SelectAction(ActionsMenuActionBtn actionBtn)
    {
        currentlySelectedActionBtn = actionBtn;
        if (currentlySelectedActionBtn.state == ActionBtnMenuUIState.Equipped) text_buttonLabelEquipDequip.text = "Dequip";
        else if(currentlySelectedActionBtn.state == ActionBtnMenuUIState.Default) text_buttonLabelEquipDequip.text = "Equip";
    }

    public void PressedEquipDequip()
    {
        if (currentlySelectedActionBtn.state == ActionBtnMenuUIState.Equipped) EquipSelected();
        else DequipSelected();
    }

    public void EquipSelected() => currentlySelectedActionBtn.EquipAction();
    public void DequipSelected() => currentlySelectedActionBtn.DequipAction();
}