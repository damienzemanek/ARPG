using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.TextCore.Text;
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
        public StatDeltasConfig statDeltas;

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
            rewardData.statDeltas = new StatDeltasConfig() { level = i };
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

    [Serializable]
    public class StatDeltasConfig
    {
        public int level;
        // future: for removal and addition operations for deltas for easier time
        // on differnet build paths with differnt deltas
        public CharacterStatDeltas deltas;
    }

    [Serializable]
    public class CharacterStatDeltas
    {
        public int health;
        public int ap;
        public int damage;
        public int armor;
        public int handSize;

        public void MutateDeltas(CharacterStatDeltas other)
        {
            health += other.health;
            ap += other.ap;
            damage += other.damage;
            armor += other.armor;
            handSize += other.handSize;
        }
    }


    public void GetStats(int curLvl, out CharacterStatDeltas s)
    {
        s = new CharacterStatDeltas();
        s.health = maxHP;
        s.ap = maxAP;
        s.damage = damage;
        s.armor = maxArmor;
        s.handSize = maxHandSize;
        
        for(int i = 1; i < curLvl; i++)
            s.MutateDeltas(levelData[i].statDeltas.deltas);
    }
    
    
    // [Button]
    // [PropertyOrder(-1)]
    // private void ValidateStatDeltas()
    // {
    //     var duplicates = statDeltas
    //         .GroupBy(x => x.level)
    //         .Where(g => g.Count() > 1);
    //
    //     foreach (var group in duplicates)
    //         Debug.LogWarning($"Duplicate stat delta at level {group.Key}", this);
    // }
    
}



