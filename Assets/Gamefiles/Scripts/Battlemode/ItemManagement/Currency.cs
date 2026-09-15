using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Currency Storage", menuName = "ARPG/SO/Item", order = 0)]
public class Currency : ItemSO
{
    public override bool canOwnMultiple => true;

    public enum CurrencyType
    {
        Gold,       // Y-Currency: To upgrade characters & buy stuff from shopkeeper
        Crystals    // X-Currency: To pull for new characters & buy rare stuff from shopkeeper
    }
    
    public CurrencyType currencyCurrencyType;
}