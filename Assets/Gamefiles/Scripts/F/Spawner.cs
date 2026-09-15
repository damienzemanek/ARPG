using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using Sirenix.OdinInspector;

public class SessionData : PersistantReplacerSingleton<SessionData>
{
    public ItemRewards defaultRewards;
    public ISingleInspectingUI singleInspectingUI = null;
    
    [ReadOnly] public string desiredSpawnLocationID;
    [ReadOnly] public BattleConfig currentBattleConfig;
    [ReadOnly] public ItemRewards currentBattlemodePotentialRewards;
    
    public void SetDesiredSpawnLocation(string id) => desiredSpawnLocationID = id;
    
    [Button] public void SetCurrentBattleConfig(BattleConfig config) => currentBattleConfig = config;
    
        
    
}
