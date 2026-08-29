using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "BattlerConfig", menuName = "ARPG/SO/BattlerConfig")]
public class BattlerConfig : OccupantCfg
{
    public int currentLevel = 1;
    public int maxHP;
    public int maxArmor;
    public int damage;
    [SerializeField, HideInInspector] int _maxEquippableActions;
    [ShowInInspector] public int maxEquippableActions
    {
        get => _maxEquippableActions;
        set
        {
            if (value < 0) return;
            if (value == _maxEquippableActions) return;
            _maxEquippableActions = value;
            var old = equippedActions;
            equippedActions = new BattlemodeActionConfig[value];
            Array.Copy(old, equippedActions, Mathf.Min(old.Length, value));
        }
    }

    [ReadOnly] public BattlemodeActionConfig[] equippedActions = Array.Empty<BattlemodeActionConfig>();
    public List<BattlemodeActionConfig> AllAvaliableActions = new();
    public List<BattlemodeActionConfig> ForcedActions = new();
    
    public List<BattlemodeEffectConfigInstance> specialEffects = new();
    
    [Button]
    public void EquipAction(int slotIndex, BattlemodeActionConfig action)
    {
        if(slotIndex < 0 || slotIndex >= equippedActions.Length) return;
        equippedActions[slotIndex] = action;
    }
}