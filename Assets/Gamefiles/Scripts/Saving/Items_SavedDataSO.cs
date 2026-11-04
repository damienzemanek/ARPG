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
    
    public enum ItemType
    {
        None,
        CharacterAction,
        Equipment,
        Currency,
    }

    [Serializable]
    public class ItemData
    {
        public string itemName;
        public int amount;
    }

    [ReadOnly, SerializeField] public CharacterConfigurations currentCharacterConfigurations;
    
    public List<ItemData> characterActionsData = new();
    public List<ItemData> equipmentData = new();
    public List<ItemData> currencyData = new();

    
    public ItemData AddItemData(string itemName, ItemType itemType, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemName)) throw new ArgumentException("Item name cannot be null or empty.", nameof(itemName));

        var data = itemType switch
        {
            ItemType.CharacterAction => characterActionsData,
            ItemType.Equipment => equipmentData,
            ItemType.Currency => currencyData,
            _ => throw new ArgumentOutOfRangeException(nameof(itemType))
        };

        var existing = data.FirstOrDefault(c => c.itemName == itemName);

        if (existing != null)
            existing.amount += amount;
        else
        {
            existing = new ItemData { itemName = itemName, amount = amount };
            data.Add(existing);
        }

        return existing;
    }

    public ItemData GetItemData(ItemType itemType, string itemName)
    {
        if(String.IsNullOrEmpty(itemName)) Debug.LogError("No item name provided");
        var searchCache = itemType switch
        {
            ItemType.CharacterAction => characterActionsData,
            ItemType.Equipment => equipmentData,
            ItemType.Currency => currencyData, 
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), $"Unsupported item type: {itemType}")
        };
        var ret = searchCache.FirstOrDefault(c => c.itemName == itemName);
        return ret ?? AddItemData(itemName, itemType, 0);
    }

    public ItemData GetItemData(ItemType itemType, ItemData itemData) => GetItemData(itemType, itemData.itemName);
}