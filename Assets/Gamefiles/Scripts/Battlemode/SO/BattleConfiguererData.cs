using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "BattleConfiguererData", menuName = "ARPG/SO/BattleConfiguererData")]
public class BattleConfiguererData : ScriptableObject
{
    public List<BattleConfig> battleConfigs = new();
    public float enemyCountExpGainScalarAdditive = 1.5f;
    public AnimationCurve levelExpGainCurve = AnimationCurve.Linear(0, 1, 1, 1);
    
    #if UNITY_EDITOR
    [Button]
    public void RetrieveAllCurrentBattleConfigsInAssets()
    {
        battleConfigs.Clear();

        string[] guids = AssetDatabase.FindAssets("t:BattleConfig");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var cfg = AssetDatabase.LoadAssetAtPath<BattleConfig>(path);
            if(cfg != null) battleConfigs.Add(cfg);
        }
        
        // 0 -> Lowest value, (Count - 1) -> Highest value
        battleConfigs.Sort((a, b) => a.amountOfEnemies.CompareTo(b.amountOfEnemies));
        
        Debug.Log("Retrieved " + battleConfigs.Count + " battle configs.");
    }
    #endif
    

}