using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "ARPG/SO/EnemyConfig")]
public class EnemyConfig : BattlerConfig
{
    [FormerlySerializedAs("actionIntentionsCount")] public int intentions = 1;
    
    [FormerlySerializedAs("visableActionCount")] 
    [FormerlySerializedAs("VisableActionCount")] 
    public int defaultPredicted = 1;
}