using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ARPG Saved Data", menuName = "ARPG/SO/SavedData/ARPG Game")]
public class ARPG_SavedDataSO : SavedDataSO
{
    static readonly TypeSerialized<Type> _subType = new(typeof(ARPG_SavedDataSO));
    public override TypeSerialized<Type> subType => _subType;
    
    public override string pathName => "ARPG_SavedData";
    public override SavedDataSO CreateNewData() => CreateInstance<ARPG_SavedDataSO>();
    
    public override void MigrateData(int oldVersion)
    {
        //noop for now
    }

    public override void ResetDataOptionalInternal() { } // No op

    public bool returningPlayer;
}