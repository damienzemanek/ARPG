using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class BattlemodeAction : MonoBehaviour
{
    public enum RangeMutateActionDisplay
    {
        None,
        Unusable,
        OutOfRange
    }

    [ShowInInspector] public BattlemodeActionCtx actionCtx;
    [SerializeField] GameObject disp_CannotUseInThisTile;
    [SerializeField] GameObject disp_outOfRange;
    public bool useImage = true;
    [ShowIf("useImage")] public Image img;
    [ReadOnly, ShowInInspector] public InjectableClass<BattlemodeActionsDisplay> actionsDisplay = new();

    public void Hide()
    {
        actionCtx = null;
        img.sprite = null;
        gameObject.SetActive(false);
        disp_CannotUseInThisTile.SetActive(false);
        disp_outOfRange.SetActive(false);
    }

    public void InitAction(BattlemodeActionCtx actionCtx)
    {
        this.actionCtx = actionCtx;
        if(!useImage) return;
        img.sprite = actionCtx.cfg.icon;
        disp_CannotUseInThisTile.SetActive(false);
        disp_outOfRange.SetActive(false);
    }
    
    public void RangeMutateAction(RangeMutateActionDisplay mutation)
    {
        switch (mutation)
        {
            case RangeMutateActionDisplay.OutOfRange: 
                disp_outOfRange.SetActive(true);
                disp_CannotUseInThisTile.SetActive(false);
                break;
            case RangeMutateActionDisplay.Unusable:
                disp_outOfRange.SetActive(false);
                disp_CannotUseInThisTile.SetActive(true);
                break;
        }
    }

    public void HoverAction()
    {
        if (BattleTracker.Instance.currentTurn != BattleTracker.Turn.Player) return;
        if (actionCtx != null)
            actionsDisplay.Value.ShowAction(actionsDisplay.Value.currentlySelectedTile, this);
    }

    public void UseAction()
    {
        if (BattleTracker.Instance.currentTurn != BattleTracker.Turn.Player) return;
        Debug.Log("Using Action: " + actionCtx.cfg.name + "");
        actionsDisplay.Value.QueueAction();
    }
}