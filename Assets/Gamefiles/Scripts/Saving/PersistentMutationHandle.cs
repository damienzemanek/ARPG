using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class PersistentMutationHandle : MonoBehaviour
{
    static readonly Type persistentSceneSaverType = typeof(ARPG_ScenePersistencySO);
    
    [ReadOnly] public ulong id;

    private void Start()
    {
        if (!SaverService.Instance.TryGetService(persistentSceneSaverType, out var saver)) {
            Debug.LogError($"No Saver registered for {persistentSceneSaverType.Name}");
            return; }

        var data = saver.currentData as ARPG_ScenePersistencySO;
        var matchExists = data.persistencies.Any(p => p.id == id);
        if (!matchExists) return;
        var match = data.persistencies.Find(p  => p.id == id);
        var mutation = match.mutation;
        ARPG_ScenePersistencySO.Mutate(gameObject, mutation);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(id == 0) GenerateID();
    }
    #endif

    [Button]
    void GenerateID()
    {
        var bytes = System.Guid.NewGuid().ToByteArray();
        id = System.BitConverter.ToUInt64(bytes, 0);
    }
}