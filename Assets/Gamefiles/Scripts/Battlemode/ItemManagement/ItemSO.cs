using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[InlineEditor]
public class ItemSO : ScriptableObject
{ 
    public Items_SavedDataSO.ItemData itemData;
    [BoxGroup("Settings")] public string description;
    [BoxGroup("Settings")] public Sprite icon;
    
    public virtual ItemSO Clone()
    {
        var clone = CreateInstance<ItemSO>();
        clone.itemData = new Items_SavedDataSO.ItemData();
        clone.itemData.itemName = itemData.itemName;
        clone.itemData.amount = itemData.amount;
        
        clone.icon = icon;
        clone.description = description;
        
        return clone;    
    }
    
    public ItemSO ProvideItem(int amount = -1)
    {
        var clone = Clone();
        if(amount > 0) clone.itemData.amount = amount;
        return clone;
    }
}