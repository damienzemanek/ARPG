using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface ISingleInspectingUI
{
    public void StopInspecting();
}

public class ItemHolder : MonoBehaviour, ISingleInspectingUI
{
    [Required] public GameObject dispInspectItem;
    [Required] public TextMeshProUGUI txt_amountText;
    public bool useImage = true;
    [ShowIf("useImage")] public Image img;
    
    [ShowInInspector, ReadOnly] public ItemSO heldItem;
    [ReadOnly] public int amount = 0;
    [ReadOnly] public bool showingInspectDisplay = false;

    public void Hide()
    {
        heldItem = null;
        img.sprite = null;
        gameObject.SetActive(false);
    }

    public void Init(ItemSO item, int amount)
    {
        heldItem = item;
        dispInspectItem.gameObject.SetActive(false);
        
        // Amount display, TODO: Countup anim
        if (amount > 0)
        {
            txt_amountText.gameObject.SetActive(true);
            txt_amountText.text = amount.ToString();
        }
        else txt_amountText.gameObject.SetActive(false);

        // Not all item holders will have an image (?)
        if (!useImage) return;
        img.sprite = item.squareIcon;
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