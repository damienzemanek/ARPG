using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public interface ISingleInspectingUI
{
    public void StopInspecting();
}

public class ItemHolder : MonoBehaviour, ISingleInspectingUI
{
    [ShowInInspector] public ItemSO heldItem;
    public bool useImage = true;
    [ShowIf("useImage")] public Image img;
    public bool showingInspectDisplay = false;
    public GameObject dispInspectItem;

    public void Hide()
    {
        heldItem = null;
        img.sprite = null;
        gameObject.SetActive(false);
    }

    public void Init(ItemSO item)
    {
        heldItem = item;
        dispInspectItem.gameObject.SetActive(false);
        if (!useImage) return;
        img.sprite = item.icon;
    }

    public void Hover()
    {
        // low priority mabye slight anim
    }
    
    public void StopHover()
    {
        // low priority mabye slight anim
    }

    public void ToggleItemDisplay()
    {
        showingInspectDisplay = !showingInspectDisplay;
        dispInspectItem.gameObject.SetActive(showingInspectDisplay);
        if (showingInspectDisplay)
        {
            if(SessionData.Instance.singleInspectingUI != null)
                SessionData.Instance.singleInspectingUI.StopInspecting();
            SessionData.Instance.singleInspectingUI = this;
        }
        else
        {
            SessionData.Instance.singleInspectingUI = null;
            StopInspecting();
        }
    }

    public void StopInspecting()
    {
        showingInspectDisplay = false;
        dispInspectItem.gameObject.SetActive(false);
    }
}