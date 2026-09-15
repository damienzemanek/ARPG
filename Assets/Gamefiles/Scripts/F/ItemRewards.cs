using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "ARPG/SO/ItemRewards", fileName = "Item Rewards")]
public class ItemRewards : ScriptableObject
{
    [SerializeField] public List<ItemSO> rewards;
}