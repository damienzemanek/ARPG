using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class BattlemodeAction : MonoBehaviour
{
    [ShowInInspector] public BattlemodeActionCtx actionCtx;
    public bool useImage = true;
    [ShowIf("useImage")] public Image img;
    [ReadOnly, ShowInInspector] public InjectableClass<BattlemodeActionsDisplay> actionsDisplay = new();

    public void Hide()
    {
        actionCtx = null;
        img.sprite = null;
        gameObject.SetActive(false);
    }

    public void InitAction(BattlemodeActionCtx actionCtx)
    {
        this.actionCtx = actionCtx;
        if(!useImage) return;
        img.sprite = actionCtx.cfg.icon;
    }

    public void HoverAction()
    {
        if (BattleTracker.Instance.currentTurn != BattleTracker.Turn.Player) return;
        if (actionCtx != null)
            actionsDisplay.Value.ShowAction(actionsDisplay.Value.currentlySelectedTile, actionCtx);
    }

    public void UseAction()
    {
        if (BattleTracker.Instance.currentTurn != BattleTracker.Turn.Player) return;
        Debug.Log("Using Action: " + actionCtx.cfg.name + "");
        actionsDisplay.Value.QueueAction();
    }
}