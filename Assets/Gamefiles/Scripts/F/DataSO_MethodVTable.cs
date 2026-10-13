using System.Linq;
using EMILtools.Extensions;
using UnityEngine;

[CreateAssetMenu(fileName = "DataSO Method VTable", menuName = "ARPG/SO/MethodVTables/DataSO")]
public class DataSO_MethodVTable : SO_MethodVTable
{
    public void UnlockCharacter(CharacterConfig characterCfg)
    {
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var saver, out var data);
        var savedData = data.charactersData.First(c => c.characterConfigName == characterCfg.occupantName);
        if (savedData.hasCharacter) return;
        savedData.hasCharacter = true;
        savedData.equipped = false;
        savedData.row = -1;
        savedData.col = -1;
        saver.Save();
    }
}