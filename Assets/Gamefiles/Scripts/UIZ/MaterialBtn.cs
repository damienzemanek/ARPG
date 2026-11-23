using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialBtn : MonoBehaviour
{
    const string k_commitedSlashRequiredFormat = "{0}/{1}";
    const string k_haveAmountFormat = "x{0}";
    
    [Required] public TextMeshProUGUI txt_haveAmount;
    [Required] public TextMeshProUGUI txt_commitedSlashRequired;
    [Required] public Image img;
    [Required] public GameObject unCommiteBtn;

    [ReadOnly] public bool satisfied = false;
    [ShowInInspector, ReadOnly] public ItemSO heldItem;
    [ReadOnly] public int requiredAmount = 0;
    [ReadOnly] public int commitedAmount = 0;
    [ReadOnly] public int haveAmountTemp = 0;

    public void Hide()
    {
        heldItem = null;
        img.sprite = null;
        gameObject.SetActive(false);
    }

    public void Init(ItemSO item, int requiredAmount, int haveAmount)
    {
        heldItem = item;
        this.requiredAmount = requiredAmount;
        commitedAmount = 0;
        haveAmountTemp = haveAmount;
        
        // Amount display, TODO: Countup anim
        txt_haveAmount.text = string.Format(k_haveAmountFormat, haveAmount);
        txt_commitedSlashRequired.text = string.Format(k_commitedSlashRequiredFormat, commitedAmount, requiredAmount);

        // Not all item holders will have an image (?)
        img.sprite = item.squareIcon;
    }

    public void TryCommitOne()
    {
        if (haveAmountTemp <= 0) return;
        if (commitedAmount >= requiredAmount)
        {
            if(!satisfied) Satisfied();
            return;
        }
        commitedAmount++;
        haveAmountTemp--;
        txt_commitedSlashRequired.text = string.Format(k_commitedSlashRequiredFormat, commitedAmount, requiredAmount);
        if(commitedAmount >= 1) unCommiteBtn.SetActive(true);
    }

    public void UnCommitOne()
    {
        if (commitedAmount <= 0)
        {
            unCommiteBtn.SetActive(false);
            return;
        }
        commitedAmount--;
        haveAmountTemp++;
        txt_commitedSlashRequired.text = string.Format(k_commitedSlashRequiredFormat, commitedAmount, requiredAmount);
    }

    public void AutoCommitAsMuchAsPossible()
    {
        for(int i = 0; i < haveAmountTemp; i++) TryCommitOne();
    }

    public void UnCommitAsMuchAsPossible()
    {
        for(int i = 0; i < commitedAmount; i++) UnCommitOne();
    }

    public void Satisfied()
    {
        satisfied = true;
    }

    public void Hover()
    {
        
    }
    
    public void StopHover()
    {
        
    }

    
}