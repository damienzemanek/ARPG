using EMILtools.Extensions;
using UnityEngine;

[CreateAssetMenu(fileName = "Battle MethodVTable", menuName = "ARPG/SO/MethodVTables/Battle")]
public class BattleSO_MethodVTable : SO_MethodVTable
{
    public int loaderIndex;
    public string startBattlemodeEncounterAnimName;
    public string camZoomAnimName;
    
    public void LoadBattle(GameObject playerObj, GameObject triggerObj, BattleConfig battleConfig, ItemRewards rewards)
    {
        if (!playerObj.Has(out PlayerInstance playerInstance)) return;
        var fade = PlayerScreenFade.Instance.fadeTarg;
        SessionData.Instance.currentBattleConfig = battleConfig;
        SessionData.Instance.currentBattlemodePotentialRewards = rewards;
        SaverService.Instance.GetSaverAndData<ARPG_SavedDataSO>(out var saver, out var data);
        data.currentCharacterWorldLocation = new Pose(playerObj.transform);
        saver.Save();
        
        playerInstance.ToggleInputReading(false);
        playerInstance.faderAnimator.PlayOnEnd(startBattlemodeEncounterAnimName, () => 
        {
            triggerObj.gameObject.SetActive(false);
            playerInstance.cameraSystemAnimator.PlayOnEnd(camZoomAnimName, () =>
                { Loader.Instance.LoadSceneAdditiveUnloadCurrent(loaderIndex); });
        });
    }
    
}

