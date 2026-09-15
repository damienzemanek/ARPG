using System.Collections.Generic;
using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using Sirenix.OdinInspector;
using UnityEngine;

public class SessionData : PersistantReplacerSingleton<SessionData>
{
    public ItemRewards defaultRewards;
    public ISingleInspectingUI singleInspectingUI = null;

    [ReadOnly] public PlayerEvents.SpawnEvent currentSpawnEvent;
    [ReadOnly] public string desiredSpawnLocationID;
    [ReadOnly] public BattleConfig currentBattleConfig;
    [ReadOnly] public ItemRewards currentBattlemodePotentialRewards;
    [ReadOnly] public List<GameObject> inactives = new();
    
    public void SetDesiredSpawnLocation(string id) => desiredSpawnLocationID = id;
    
    [Button] public void SetCurrentBattleConfig(BattleConfig config) => currentBattleConfig = config;
}
