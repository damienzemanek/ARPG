using UnityEngine;

[CreateAssetMenu(fileName = "SetSpawnLoc", menuName = "ARPG/SO/Method/SetSpawnLoc")]
public class SetSpawnLoc : SO_MethodVTable
{
    public void SetSpawnLocation(string desiredSpawnLoc) => SessionData.Instance.SetDesiredSpawnLocation(desiredSpawnLoc);
}