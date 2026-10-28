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
    [Required] [SerializeField] GameObject disp_CannotUseInThisTile;
    [Required] [SerializeField] GameObject disp_outOfRange;
    public bool useImage = true;
    [Required] [ShowIf("useImage")] public Image bgImg;
    [Required] [ShowIf("useImage")] public Image img;
    [Required] public Sprite defaultSprite;
    [Required] public Sprite hoverSprite;
    [ReadOnly, ShowInInspector] public InjectableClass<BattlemodeActionsDisplay> actionsDisplay = new();

    public void Hide()
    {
        actionCtx = null;
        img.sprite = null;
        gameObject.SetActive(false);
        if(disp_CannotUseInThisTile == null || disp_outOfRange == null) return;
        disp_CannotUseInThisTile.SetActive(false);
        disp_outOfRange.SetActive(false);
        DefaultAction();
    }

    public void InitAction(BattlemodeActionCtx actionCtx)
    {
        this.actionCtx = actionCtx;
        if(!useImage) return;
        if(actionCtx != null) img.sprite = actionCtx.cfg.icon;
        if(disp_CannotUseInThisTile == null || disp_outOfRange == null) return;
        disp_CannotUseInThisTile.SetActive(false);
        disp_outOfRange.SetActive(false);
        DefaultAction();
    }
    
    public void RangeMutateAction(RangeMutateActionDisplay mutation)
    {
        if(disp_CannotUseInThisTile == null || disp_outOfRange == null) return;
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
    
    public void DefaultAction() => bgImg.sprite = defaultSprite;
    public void HoverAction()
    {
        if (BattleTracker.Instance.currentTurn != BattleTracker.Turn.Player) return;
        if (actionCtx != null)
        {
            actionsDisplay.Value.ShowAction(actionsDisplay.Value.currentlySelectedTile, this);
            if(bgImg != null)
                if(hoverSprite != null)
                     bgImg.sprite = hoverSprite;
        }
    }

    public void UseAction()
    {
        if (BattleTracker.Instance.currentTurn != BattleTracker.Turn.Player) return;
        Debug.Log("Using Action: " + actionCtx.cfg.name + "");
        actionsDisplay.Value.QueueAction();
    }
}