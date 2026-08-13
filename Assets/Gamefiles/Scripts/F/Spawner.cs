using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using Sirenix.OdinInspector;

public class SessionData : PersistantReplacerSingleton<SessionData>
{
    [ReadOnly] public string desiredSpawnLocationID;
    [ReadOnly] public BattleConfig currentBattleConfig;
    
    public void SetDesiredSpawnLocation(string id) => desiredSpawnLocationID = id;
    
    [Button] public void SetCurrentBattleConfig(BattleConfig config) => currentBattleConfig = config;
    
        
    
}
