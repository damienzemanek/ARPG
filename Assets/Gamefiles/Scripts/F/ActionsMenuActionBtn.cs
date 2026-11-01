using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ActionsMenuActionBtn : MonoBehaviour
{
    public enum ActionBtnMenuUIState
    {
        Default,
        Locked,
        Equipped
    }

    public Color darkened;
    public Color defaultColor;

    public ActionBtnMenuUIState state;
    [ShowInInspector] public BattlemodeActionConfig actionCfg;
    [Required] public Image bgImg;
    [Required] public Image img;
    [Required] public Sprite defaultSprite;
    [Required] public Sprite hoverSprite;
    [Required] public ActionsMenuActionsDisplay actionsDisplay;
    [Required] public GameObject go_lock;
    [Required] public GameObject go_check;

    public void InjectActionAndSetup(BattlemodeActionConfig actionCfg)
    {
        this.actionCfg = actionCfg;
        img.sprite = actionCfg.icon;
        ForceDefault();
    }

    public void LockAction()
    {
        state = ActionBtnMenuUIState.Locked;
        img.color = darkened;
        bgImg.sprite = defaultSprite;
        go_check.SetActive(false);
        go_lock.SetActive(true);
    }

    public void ResetActionBtn()
    {
        img.sprite = null;
        actionCfg = null;
        gameObject.SetActive(false);
    }

    public void ForceDefault()
    {
        bgImg.sprite = defaultSprite;
        img.color = darkened;
    }
    
    public void Default()
    {
        if (actionsDisplay.currentlySelectedActionBtn == this) return;
        ForceDefault();
    }

    public void Hover()
    {
        if (state == ActionBtnMenuUIState.Locked) return;
        img.color = defaultColor;
    }

    public void Select()
    {
        if (actionsDisplay.currentlySelectedActionBtn == this) return;
        if (state == ActionBtnMenuUIState.Locked) return;
        bgImg.sprite = hoverSprite;
        img.color = defaultColor;
        actionsDisplay.SelectAction(this);
    }

    public void EquipAction()
    {
        state = ActionBtnMenuUIState.Equipped;
        go_check.SetActive(true);
        go_lock.SetActive(false);
    }

    public void DequipAction()
    {
        state = ActionBtnMenuUIState.Default;
        go_check.SetActive(false);
        go_lock.SetActive(false);
    }

}