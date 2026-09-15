using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "MultiItem", menuName = "ARPG/SO/MultiItem", order = 0)]
public class MultiItem : ItemSO
{
    public int amount;
    public ItemSO item;
    public override ItemSO ProvideReward()
    {
        if (item is IMultiItem multi) return multi.Clone();
        
        Debug.LogError("Tried to Provide a MultiItem that is not marked as IMultiItem");
        return null;

    }
}