using System;
using UnityEngine;

public abstract class SavedDataSO: ScriptableObject
{
    public abstract TypeSerialized<Type> subType { get; }
    public abstract string pathName { get; }
    public abstract SavedDataSO CreateNewData();
    public SavedDataSO ResetData()
    {
        var dat = CreateNewData();
        ResetDataOptionalInternal();
        return dat;
    }
    public abstract void MigrateData(int oldVersion);
    public SaveSchemaUtility.SaveSchemaInfo schemaInfo;
    public abstract void ResetDataOptionalInternal();
}

