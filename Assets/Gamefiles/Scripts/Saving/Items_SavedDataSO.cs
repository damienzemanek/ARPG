using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Items Saved Data", menuName = "ARPG/SO/SavedData/Items")]
public class Items_SavedDataSO : SavedDataSO
{
    static readonly TypeSerialized<Type> _subType = new(typeof(Items_SavedDataSO));
    public override TypeSerialized<Type> subType => _subType;
    
    public override string pathName => "Items_SavedData";
    public override SavedDataSO CreateNewData() => CreateInstance<Items_SavedDataSO>();
    
    public override void MigrateData(int oldVersion)
    {
        //noop for now
    }

    public override void ResetDataOptionalInternal()
    {
        
    }

    [Serializable]
    public class ItemData
    {
        public string itemName;
        public int amount;
    }

    [ReadOnly, SerializeField] public CharacterConfigurations currentCharacterConfigurations;
    public List<ItemData> itemDatas = new();

    public ItemData AddItemDataAndSave(string itemName, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemName)) throw new ArgumentException("Item name cannot be null or empty.", nameof(itemName));

        var existing = itemDatas.FirstOrDefault(c => c.itemName == itemName);
        
        if (existing != null)
            existing.amount += amount;
        else
            itemDatas.Add(existing = new ItemData { itemName = itemName, amount = amount });

        SaverService.Instance.GetSaver<Items_SavedDataSO>().Save();
        return existing;
    }

    public ItemData GetItemData(string itemName)
    {
        if(String.IsNullOrEmpty(itemName)) Debug.LogError("No item name provided");
        var ret = itemDatas.FirstOrDefault(c => c.itemName == itemName);
        return ret ?? AddItemDataAndSave(itemName, 0);
    }

    public ItemData GetItemData(ItemData itemData) => GetItemData(itemData.itemName);
}