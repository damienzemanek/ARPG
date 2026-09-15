using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[InlineEditor]
public abstract class ItemSO : ScriptableObject
{
    [BoxGroup("Settings")] public string itemName;
    [BoxGroup("Settings")] public string description;
    [BoxGroup("Settings")] public Sprite icon;
    public abstract ItemSO ProvideReward(int amount = -1);
}