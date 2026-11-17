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


    [Serializable]
    public class BattlerSpecificActionConfig
    {
        [Required] public Sprite useActionSprite;
        public bool useSelfVFX = false;
        [ShowIf("useSelfVFX")] public Sprite useActionSpriteSelfVFX;
        public bool projectileBased = false;
        [ShowIf("projectileBased")] public int projectileCount = 1;
        [ShowIf("projectileBased")] public Sprite projectileSprite;
        [Required] public BattlemodeActionConfig cfg;
    }
    
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
    [SerializeField, ShowInInspector] List<BattlerSpecificActionConfig> allAvaliableActions = new();
    public List<BattlerSpecificActionConfig> AllAvaliableActions
    {
        get => allAvaliableActions;
        set
        {
            allAvaliableActions = value;
            allAvaliableActionNames = new List<string>(allAvaliableActions.Count);
            foreach (var action in allAvaliableActions)
                allAvaliableActionNames.Add(action.cfg.name);
        }
    }

    public List<BattlemodeEffectConfigInstance> specialEffects = new();
    [ReadOnly] public List<string> allAvaliableActionNames = new();

    public BattlerSpecificActionConfig GetSpecificActionCfg(BattlemodeActionConfig actionCfg)
    {
         var ret = allAvaliableActions.FirstOrDefault(a => a.cfg == actionCfg);
         return ret;
    }
    
    [Button]
    public void EquipAction(int slotIndex, int availableActionIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedActions.Length) return;
        if (availableActionIndex < 0 || availableActionIndex >= allAvaliableActions.Count) return;
        equippedActions[slotIndex] = allAvaliableActions[availableActionIndex].cfg;
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