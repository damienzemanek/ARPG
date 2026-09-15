using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class ItemSO : ScriptableObject
{
    [BoxGroup("State")] [ShowIf("canOwnMultiple")] public int amountOwned = 0;
    [BoxGroup("Settings")] public abstract bool canOwnMultiple { get; }
    [BoxGroup("Settings")] public string itemName;
    [BoxGroup("Settings")] public string description;
}