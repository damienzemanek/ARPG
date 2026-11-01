using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BattlerConfig", menuName = "ARPG/SO/BattlerConfig")]
public class BattlerConfig : OccupantCfg
{
    [BoxGroup("Stats")] public int currentLevel = 1;
    [BoxGroup("Stats")] public int maxHP;
    [BoxGroup("Stats")] public int maxArmor;
    [BoxGroup("Stats")] public int damage;
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
    [SerializeField, ShowInInspector] List<BattlemodeActionConfig> allAvaliableActions = new();
    public List<BattlemodeActionConfig> AllAvaliableActions
    {
        get => allAvaliableActions;
        set
        {
            allAvaliableActions = value;
            allAvaliableActionNames = new List<string>(allAvaliableActions.Count);
            foreach (var action in allAvaliableActions)
                allAvaliableActionNames.Add(action.name);
        }
    }

    public List<BattlemodeEffectConfigInstance> specialEffects = new();
    [ReadOnly] public List<string> allAvaliableActionNames = new();
    
    
    [Button]
    public void EquipAction(int slotIndex, BattlemodeActionConfig action)
    {
        if(slotIndex < 0 || slotIndex >= equippedActions.Length) return;
        equippedActions[slotIndex] = action;
    }

    public bool TryEquipActionOnNextAvaliableSlot(BattlemodeActionConfig actionCfg)
    {
        if(equippedActions.Contains(actionCfg)) return false;
        
        for (int i = 0; i < equippedActions.Length; i++)
        {
            if(equippedActions[i] == null)
            {
                equippedActions[i] = actionCfg;
                return true;
            }
        }
        return false;
    }

    public void DequipAction(BattlemodeActionConfig actionCfg)
    {
        for (int i = 0; i < equippedActions.Length; i++)
        {
            if(equippedActions[i] == actionCfg)
            {
                equippedActions[i] = null;
                return;
            }
        }
    }
}