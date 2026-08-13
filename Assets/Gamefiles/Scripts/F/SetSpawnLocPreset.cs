using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SetSpawnLocPreset", menuName = "ARPG/SO/Method/SetSpawnLocPreset")]
public class SetSpawnLocPreset : SO_Method_Preset
{
    [Required] public RegionData goingToRegion;
    [ValueDropdown(nameof(GetSpawnLocations))] public string spawnLocation;
    IEnumerable<string> GetSpawnLocations()
    {
        if (goingToRegion == null) yield break;
        foreach (string tag in goingToRegion.regionSpawnLocationTags) yield return tag;
    }

    public override void Invoke()
    {
        SessionData.Instance.SetDesiredSpawnLocation(spawnLocation);
    }
}