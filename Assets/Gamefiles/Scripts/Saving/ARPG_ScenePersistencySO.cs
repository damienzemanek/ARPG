using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using UnityEngine;

[CreateAssetMenu(fileName = "ARPG Scene Persistency Data", menuName = "ARPG/SO/SavedData/Scene Persistency")]
public class ARPG_ScenePersistencySO : SavedDataSO
{
    static readonly Type persistentSceneSaverType = typeof(ARPG_ScenePersistencySO);

    public enum PersistentMutation
    {
        SetActiveFalse,
        SetActiveTrue,
        Destroy,
    }
    
    [Serializable]
    public struct Persistancy
    {
        public PersistentMutation mutation;
        public ulong id;
        public Persistancy(ulong  id, PersistentMutation mutation)
        {
            this.id = id;
            this.mutation = mutation;
        }
    }
    
    static readonly TypeSerialized<Type> _subType = new(typeof(ARPG_ScenePersistencySO));
    public override TypeSerialized<Type> subType => _subType;
    
    public override string pathName => "ARPG_ScenePersistencySO";
    public override SavedDataSO CreateNewData() => CreateInstance<ARPG_ScenePersistencySO>();
    
    public override void MigrateData(int oldVersion) { } //noop for now
    public override void ResetDataOptionalInternal() { } // No op
    
    [SerializeField] public List<Persistancy> persistencies = new();
    
    public void PersistentMutate(
        GameObject obj,
        PersistentMutation mutation)
    {
        Mutate(obj, mutation);
        var handle = obj.Get<PersistentMutationHandle>();
        ulong id = handle.id;
        int index = persistencies.FindIndex(p => p.id == id);
        if (index >= 0) persistencies[index] = new Persistancy(id, mutation);
        else persistencies.Add(new Persistancy(id, mutation));

        if (!SaverService.Instance.TryGetService(persistentSceneSaverType, out var persistencySaver)) {
            Debug.LogError($"No Saver registered for {persistentSceneSaverType.Name}");
            return; }

        persistencySaver.ManuallySave(this);
    }
    
    public static void Mutate(GameObject obj, PersistentMutation mutation)
    {
        switch (mutation)
        {
            case PersistentMutation.SetActiveFalse: obj.SetActive(false); break;
            case PersistentMutation.SetActiveTrue: obj.SetActive(true); break;
            case PersistentMutation.Destroy: Destroy(obj); break;
        }
    }
}