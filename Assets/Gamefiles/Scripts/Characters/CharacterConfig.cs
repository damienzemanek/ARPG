using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using static CharacterConfig.LevelData;

[CreateAssetMenu(fileName = "Character", menuName = "ARPG/SO/Character")]
public class CharacterConfig : BattlerConfig
{
    CharacterConfigurations cfg => PersistentConfigurationDataHolder.Instance.characterConfigs;
    
    [BoxGroup("Stats")] public int maxAP = 5;
    [BoxGroup("Stats")] public int maxHandSize = 6;
    [BoxGroup("Settings")] [Required] public Sprite portraitArtSelected;
    [BoxGroup("Settings")] [Required] public Sprite portraitArtUnselected;
    [BoxGroup("Settings")] public Sprite portraitArtSquareSelected;
    [BoxGroup("Settings")] public Sprite portraitArtSquareUnselected;
    [BoxGroup("Settings")] [Required] public Sprite portraitArtLocked;
    [BoxGroup("Settings")] [Required] public Sprite mainArt;
    public string specialEffectName; // Mabye later characters can have multiple sp effects
    public string desc_tactics;
    public string desc_about;
    public List<BattlemodeActionConfig> startingKitActions = new();
    public List<EquipmentConfig> equippedEquipments;
    // i would have to make the char info display sp effect multi scalable tho

    [ListDrawerSettings(ShowIndexLabels = true), InlineProperty] [Required("Ensure you press `InitRewards` to Initialize")]
    public List<LevelData> levelData;
    
    [Serializable]
    public class LevelData
    {
        [ListDrawerSettings(DefaultExpandedState = true)] 
        public List<RewardConfigData> Rewards = new();
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<MaterialConfigData> MaterialRequirements = new();

        [Serializable]
        public class MaterialConfigData
        {
            public int amount;
            [HideLabel] public ItemSO item;
        }
    }

    [Button]
    public void InitRewards()
    {
        levelData = new List<LevelData>();
        
        for (int i = 0; i < cfg.maxLevel + 1; i++)
        {
            var rewardData = new LevelData();
            levelData.Add(rewardData);
            if (i == 0) continue; // index buffered offset
            var indexMod10 = (i - 1) % 10;
            float percOfMaxLevel = (float)i / cfg.maxLevel;

            foreach (var material in cfg.materialRequirementsPer10Levels[indexMod10].items)
            {
                var newMaterialRequirement = new LevelData.MaterialConfigData();
                newMaterialRequirement.item = material;
                rewardData.MaterialRequirements.Add(newMaterialRequirement);
                
                bool isGold = newMaterialRequirement.item.itemData.itemName == cfg.coinItemSO.itemData.itemName;
                if (isGold) newMaterialRequirement.amount = Mathf.RoundToInt(cfg.goldMaterialIncreaseCurve.Evaluate(percOfMaxLevel) * cfg.goldScalar);

                bool isCrystal = newMaterialRequirement.item.itemData.itemName == cfg.crystalItemSO.itemData.itemName;
                if (isCrystal) newMaterialRequirement.amount = Mathf.RoundToInt(cfg.crystalMaterialIncreaseCurve.Evaluate(percOfMaxLevel) * cfg.crystalScalar);
            }
            
            rewardData.Rewards = new List<RewardConfigData>();
        }
    }
}



