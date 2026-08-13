using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "BattlerConfig", menuName = "ARPG/SO/BattlerConfig")]
public class BattlerConfig : BattlePositionOccupantConfig
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
            equippedActions = new BattlemodeActionConfig[value];
        }
    }

    [ReadOnly] public BattlemodeActionConfig[] equippedActions = Array.Empty<BattlemodeActionConfig>();
    public List<BattlemodeActionConfig> AllAvaliableActions = new();
    public List<BattlemodeActionConfig> ForcedActions = new();
    
    public List<BattlemodeEffectConfig> specialEffects = new();
    
    [Button]
    public void EquipAction(int slotIndex, BattlemodeActionConfig action)
    {
        if(slotIndex < 0 || slotIndex >= equippedActions.Length) return;
        equippedActions[slotIndex] = action;
    }
}