using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;





/// <summary>
/// Singular Saver for a SavedDataSO subtype.
/// </summary>
[DefaultExecutionOrder(-500)]
public class Saver : ConditionalPersistentReplacerSingleton<Saver>
{
    public List<Saver> GetSaver(Type saveSOType)
    {
        var list = FindObjectsByType<Saver>()
            .Where(s => {
                Debug.Log("Compare: " + s.saveSOType.Type + " | " + saveSOType + " |");
                var ret = s.saveSOType.Type == saveSOType;
                Debug.Log($"[GetSaver] Type: {saveSOType.Name} | Found: {ret}");
                return ret; })
            .ToList();

        Debug.Log($"[GetSaver] Type: {saveSOType.Name} | Found {list.Count}: " 
                  + string.Join(", ", list.Select(s => s.name)));

        return list;
    }

    public Saver GetFirstSaver(Type saveSOType)
    {
        var ret = GetSaver(saveSOType).First();
        Debug.Log($"[GetFirstSaver] Type: {saveSOType.Name} | Returning: {ret.name}");
        return ret;
    }

public TypeSerialized<SavedDataSO> saveSOType;

    [Tooltip("Optional default save data used when no save exists.")]
    public SavedDataSO defaultData;

    [ReadOnly, SerializeField] public SavedDataSO currentData;

    [Button]
    public void ManuallySave(SavedDataSO data)
    {
        currentData = data;
        Save();
    }

    [ShowInInspector, ReadOnly]
    public string CopyablePath
    {
        get
        {
            if (saveSOType.Type == null) return "No Save Type Selected";

            var tempData = currentData ?? CreateDataInstance();
            if (tempData == null) return "Invalid Save Type";

            return Path.Combine(
                Application.persistentDataPath,
                tempData.pathName,
                "savefile.json")
                .Replace("\\", "/");
        }
    }

    [Button("Open Save Location")]
    void OpenSaveLocation()
    {
#if UNITY_EDITOR
        string path = SavePath.Replace("\\", "/");

        if (File.Exists(path))
            UnityEditor.EditorUtility.RevealInFinder(path);
        else
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                UnityEditor.EditorUtility.RevealInFinder(directory);
        }
#endif
    }

    protected override bool ShouldReplace(Saver existingInstance)
    {
        if(existingInstance == null) Debug.LogError("No existing instance");
        if (existingInstance.currentData == null)
        {
            Debug.Log("No ExistingInstance current data");
            return false;
        }
        if (currentData == null)
        {
            Debug.Log("No current data");
            return false;
        }
        if(existingInstance.currentData.subType.Type == null) Debug.LogError("No existing instance subType");
        if(currentData.subType.Type == null) Debug.LogError("No current data subType");
        bool same = existingInstance.currentData.subType.Type == currentData.subType.Type;
        return same;
    }

    protected override void Awake()
    {
        base.Awake();
        if (HasInstance && Instance != this) return;
        Load();
    }

    SavedDataSO CreateEmptyData()
    {
        if (saveSOType.Type == null) return null;
        return (SavedDataSO)ScriptableObject.CreateInstance(saveSOType.Type);
    }

    SavedDataSO CreateDataInstance()
    {
        // Clone the default asset so we never modify the asset itself.
        if (defaultData != null) return Instantiate(defaultData);

        return CreateEmptyData();
    }

    string SavePath
    {
        get
        {
            if (currentData == null) currentData = CreateDataInstance();
            if (currentData == null) return string.Empty;
            return Path.Combine(
                Application.persistentDataPath,
                currentData.pathName,
                "savefile.json");
        }
    }

    void EnsureSaveDirectory() => Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);

    [Button]
    public void ResetData()
    {
        EnsureSaveDirectory();

        if (File.Exists(SavePath)) File.Delete(SavePath);
        currentData = defaultData == null ? CreateDataInstance() : Instantiate(defaultData);

        if (currentData == null)
        {
            Debug.LogError("No data to reset.");
            return;
        }

        Save();
    }

    public void Save()
    {
        currentData ??= CreateDataInstance();

        if (currentData == null)
        {
            Debug.LogError("No data to save.");
            return;
        }

        currentData.schemaInfo.hash = SaveSchemaUtility.GetSchemaHash(currentData.GetType());

        EnsureSaveDirectory();

        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(SavePath, json);

        this.Log($"Saved data at [{SavePath}]");
    }

    public void Load()
    {
        currentData = CreateDataInstance();

        if (currentData == null)
        {
            Debug.LogError("Cannot load: No save type selected.");
            return;
        }

        EnsureSaveDirectory();

        if (!File.Exists(SavePath))
        {
            Save();
            return;
        }

        string json = File.ReadAllText(SavePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            Save();
            return;
        }

        string currentSchema = SaveSchemaUtility.GetSchemaHash(currentData.GetType());

        JsonUtility.FromJsonOverwrite(json, currentData);

        if (currentData.schemaInfo.hash != currentSchema)
        {
            int oldVersion = currentData.schemaInfo.version;

            Debug.Log($"Save schema changed. Migrating {oldVersion} -> current.");

            currentData.MigrateData(oldVersion);

            currentData.schemaInfo.version++;
            currentData.schemaInfo.hash = currentSchema;

            Save();
        }

        this.Log($"Loaded data at [{SavePath}]");
    }

    void OnApplicationQuit() => Save();
}