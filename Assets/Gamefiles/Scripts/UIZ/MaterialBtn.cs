using System;
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

    public Action TestForSatisfied;

    public void Hide()
    {
        heldItem = null;
        img.sprite = null;
        gameObject.SetActive(false);
    }

    public void Init(ItemSO item, int requiredAmount, int haveAmount, Action tstForSatisfied)
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
        
        satisfied = false;
        unCommiteBtn.SetActive(false);
        
        TestForSatisfied = tstForSatisfied;
    }

    public void TryCommitOneVoid()
    {
        TryCommitOne();
        TestForSatisfied?.Invoke();
    }

    public void UnCommitOneVoid()
    {
        UnCommitOne();
        TestForSatisfied?.Invoke();
    }
    

    public bool TryCommitOne()
    {
        if (haveAmountTemp <= 0) return false; // ran out of `hasAmount`
        if (commitedAmount >= requiredAmount) // already satisifed
        {
            if(!satisfied) Satisfied();
            return false;
        }
        commitedAmount++;
        haveAmountTemp--;
        txt_commitedSlashRequired.text = string.Format(k_commitedSlashRequiredFormat, commitedAmount, requiredAmount);
        if(commitedAmount >= 1) unCommiteBtn.SetActive(true);
        if (commitedAmount >= requiredAmount) Satisfied();
        return true;
    }

    public bool UnCommitOne()
    {
        if (commitedAmount <= 0)
        {
            unCommiteBtn.SetActive(false);
            return false;
        }
        commitedAmount--;
        haveAmountTemp++;
        satisfied = false;
        if (commitedAmount <= 0) unCommiteBtn.SetActive(false);
        txt_commitedSlashRequired.text = string.Format(k_commitedSlashRequiredFormat, commitedAmount, requiredAmount);
        return true;
    }

    public void AutoCommitAsMuchAsPossible()
    {
        while (TryCommitOne()) { }
        TestForSatisfied?.Invoke();
    }

    public void UnCommitAsMuchAsPossible()
    {
        while (commitedAmount > 0) UnCommitOne();
        TestForSatisfied?.Invoke();
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