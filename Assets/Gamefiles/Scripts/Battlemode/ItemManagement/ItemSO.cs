using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[InlineEditor]
public abstract class ItemSO : ScriptableObject
{
    [BoxGroup("State")] [ShowIf("canOwnMultiple")] public int amountOwned = 0;
    [BoxGroup("Settings")] public bool canOwnMultiple => this is IMultiItem;
    [BoxGroup("Settings")] public string itemName;
    [BoxGroup("Settings")] public string description;
    [BoxGroup("Settings")] public Sprite icon;
    public abstract ItemSO ProvideReward();
}