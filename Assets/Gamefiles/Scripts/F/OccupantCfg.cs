using Sirenix.OdinInspector;
using UnityEngine;

public abstract class OccupantCfg : ScriptableObject
{
    [Required] public string occupantName;
    [Required] public Sprite icon;
    [Required] public GameObject prefab;
}