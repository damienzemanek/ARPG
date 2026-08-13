using UnityEngine;

[CreateAssetMenu(fileName = "SetSpawnLoc", menuName = "ARPG/SO/Method/SetSpawnLoc")]
public class SetSpawnLoc : SO_Method<string>
{
    public override void Invoke(string desiredSpawnLoc)
    {
        SessionData.Instance.SetDesiredSpawnLocation(desiredSpawnLoc);
    }
}