using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RegionData", menuName = "ARPG/SO/RegionData")]
public class RegionData : ScriptableObject
{
    public string regionName;
    public List<string> regionSpawnLocationTags;
}