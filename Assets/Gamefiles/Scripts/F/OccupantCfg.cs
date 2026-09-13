using Sirenix.OdinInspector;
using UnityEngine;

public abstract class OccupantCfg : ScriptableObject
{
    [BoxGroup("Settings")] [Required] public string occupantName;
    [BoxGroup("Settings")] [Required] public Sprite icon;
    [BoxGroup("Settings")] [Required] public GameObject prefab;
}