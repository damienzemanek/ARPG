using System;
using UnityEngine;

public interface IMultiItem
{
    public abstract ItemSO Clone();
}

[Serializable]
[CreateAssetMenu(fileName = "New Currency Storage", menuName = "ARPG/SO/Currency", order = 0)]
public class Currency : ItemSO, IMultiItem
{
    public ItemSO Clone()
    {
        var clone = CreateInstance<Currency>();
        clone.icon = icon;
        clone.currencyCurrencyType = currencyCurrencyType;
        clone.itemName = itemName;
        clone.description = description;
        return clone;    
    }
    
    public enum CurrencyType
    {
        Gold,       // Y-Currency: To upgrade characters & buy stuff from shopkeeper
        Crystals    // X-Currency: To pull for new characters & buy rare stuff from shopkeeper
    }
    
    public CurrencyType currencyCurrencyType;

    public override ItemSO ProvideReward(int amount)
    {
        SaverService.Instance.GetSaverAndData<ARPG_SavedDataSO>(out var saver, out var data);
        switch (currencyCurrencyType)
        {
            case  CurrencyType.Gold: data.goldAmount += amount ; break;
            case CurrencyType.Crystals: data.crystalAmount += amount; break;
            default: throw new ArgumentOutOfRangeException("ProvideReward()'d on unknown currencyType");
        }
        saver.Save();
        return this;
    }
}