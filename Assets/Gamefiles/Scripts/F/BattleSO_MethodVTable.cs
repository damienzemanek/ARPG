using EMILtools.Extensions;
using UnityEngine;

[CreateAssetMenu(fileName = "Battle MethodVTable", menuName = "ARPG/SO/MethodVTables/Battle")]
public class BattleSO_MethodVTable : SO_MethodVTable
{
    public int loaderIndex;

    public void LoadBattle(BattleConfig battleConfig)
    {
        var fade = PlayerScreenFade.Instance.fadeTarg;
        Loader.Instance.LoadSceneAdditiveDisableCurrent(2);
        SessionData.Instance.currentBattleConfig = battleConfig;
    }
    
}

