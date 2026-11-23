using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

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

    [ListDrawerSettings(ShowIndexLabels = true), InlineProperty]
    public List<LevelRewardData> levelRewards;
    
    void OnValidate() => AdjustExperienceToLevelUpAccordingToCurve();
    
    [Serializable]
    public class LevelRewardData
    {
        public int experienceToLevelUp;
        public List<ItemSO> Rewards;
    }

    [Button]
    public void InitRewards()
    {
        levelRewards = new List<LevelRewardData>();
        for (int i = 0; i < 61; i++) levelRewards.Add(new LevelRewardData());
        AdjustExperienceToLevelUpAccordingToCurve();
    }

    public void AdjustExperienceToLevelUpAccordingToCurve()
    {
        if(levelRewards == null) InitRewards();
        else if(levelRewards.Count <= 1) { Debug.LogError("Level rewards count must be greater than 1 to adjust experience curve"); return; }
        else {
            
            for (int i = 0; i < levelRewards.Count; i++)
            {
                float percOfMaxLevel = (float)i / (levelRewards.Count - 1);
                levelRewards[i].experienceToLevelUp = Mathf.RoundToInt(cfg.experienceIncreaseCurve.Evaluate(percOfMaxLevel) * cfg.experienceScalar);
            }
            
            
        }
    }
}



