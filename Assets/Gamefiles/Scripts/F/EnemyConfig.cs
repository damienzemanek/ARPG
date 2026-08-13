using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "ARPG/SO/EnemyConfig")]
public class EnemyConfig : BattlerConfig
{
    [FormerlySerializedAs("ActionIntentionsCount")] public int actionIntentionsCount = 1;
    [FormerlySerializedAs("VisableActionCount")] public int visableActionCount = 1;
}