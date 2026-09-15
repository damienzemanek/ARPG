using EMILtools.Extensions;
using UnityEngine;

[CreateAssetMenu(fileName = "Battle MethodVTable", menuName = "ARPG/SO/MethodVTables/Battle")]
public class BattleSO_MethodVTable : SO_MethodVTable
{
    public int loaderIndex;
    public string startBattlemodeEncounterAnimName;
    public string camZoomAnimName;
    
    public void LoadBattle(GameObject playerObj, BattleConfig battleConfig)
    {
        if (!playerObj.Has(out PlayerInstance playerInstance)) return;
        var fade = PlayerScreenFade.Instance.fadeTarg;
        SessionData.Instance.currentBattleConfig = battleConfig;

        playerInstance.ToggleInputReading(false);
        playerInstance.faderAnimator.PlayOnEnd(startBattlemodeEncounterAnimName, () => 
        {
            playerInstance.cameraSystemAnimator.PlayOnEnd(camZoomAnimName, () =>
                { Loader.Instance.LoadSceneAdditiveDisableCurrent(loaderIndex); });
        });
    }
    
}

