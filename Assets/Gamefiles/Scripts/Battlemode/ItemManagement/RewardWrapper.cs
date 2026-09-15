using System;
using UnityEngine;

[Serializable]
public class RewardWrapper 
{
    public int amount;
    public ItemSO item;
    public ItemSO ProvideReward(int _ = -1)
    {
        if (item is IMultiItem multi) return multi.Clone().ProvideReward(amount); 
        return item.ProvideReward();
    }
}