using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterConfigurations", menuName = "ARPG/SO/CharacterConfigurations")]
public class CharacterConfigurations : ScriptableObject
{
    [DrawWithUnity] 
    public SerializedDictionary<string, CharacterConfig> characterConfigs = new();

    public int maxLevel = 60;
    public float goldScalar = 500f;
    public float crystalScalar = 25f;

    public AnimationCurve goldMaterialIncreaseCurve = AnimationCurve.Linear(0, 1, 1, 1);
    public AnimationCurve crystalMaterialIncreaseCurve = AnimationCurve.Linear(0, 1, 1, 1);

    [Required] public ItemSO coinItemSO;
    [Required] public ItemSO crystalItemSO;

    // 0 -> 10 (0 is will be unused, so 1 -> 10)
    public List<MaterialRequirement> materialRequirementsPer10Levels = new(11);

    [Serializable]
    public class MaterialRequirement
    {
        public List<ItemSO> items;
    }
    
    [Button]
    public void InitRewardsOnAllCharacters()
    {
        foreach (var characterConfig in characterConfigs.Values)
            characterConfig.InitRewards();
    }
    

}