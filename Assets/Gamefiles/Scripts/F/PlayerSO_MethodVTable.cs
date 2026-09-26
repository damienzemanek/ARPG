using System;
using EMILtools.Extensions;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO Method VTable", menuName = "ARPG/SO/MethodVTables/PlayerSO")]
public class PlayerSO_MethodVTable : SO_MethodVTable
{
    public void DisableEnableRotComposer(GameObject playerGO)
    {
        if (playerGO.Has(out PlayerInstance P)) P.DisableEnableRotComposer();
    }

    public void ToggleFog(GameObject playerGO, bool enable, float density)
    {
        if (playerGO.Has(out PlayerInstance P)) P.ToggleFog(enable, density);
    }

    public void ToggleRain(GameObject playerGO, bool enable)
    {
        if (playerGO.Has(out PlayerInstance P)) P.ToggleRain(enable);
    }

    public void CallPlayerEvent(GameObject playerGO, PlayerEvents.PlayerEvent playerEvent)
    {
        if (playerGO.Has(out PlayerInstance P)) P.CallPlayerEvent(playerEvent);
    }
    
}
