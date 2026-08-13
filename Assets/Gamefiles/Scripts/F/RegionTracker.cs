using System;
using System.Collections.Generic;
using DesignPatterns.CreationalPatterns;
using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class RegionTracker : Singleton<RegionTracker>
{
    [Serializable]
    public class SpawnLocation
    {
        [ReadOnly] public string id;
        public Transform point;
    }
    
    [Required] public RegionData regionData;
    public List<SpawnLocation> spawnLocations;
    public EmilEvent<GameObject> OnSpawnPlayer;

    void OnValidate() => ValidateSpawnLocations();

    public void Start()
    {
        ValidateSpawnLocations();
        SpawnPlayer();
        RegionUI.Instance.DisplayRegionUI(regionData.regionName);
    }

    [Button]
    void ValidateSpawnLocations()
    {
        spawnLocations ??= new List<SpawnLocation>();
        for (int i = 0; i < regionData.regionSpawnLocationTags.Count; i++)
        {
            if(spawnLocations.Count <= i) spawnLocations.Add(new SpawnLocation());
            spawnLocations[i].id = regionData.regionSpawnLocationTags[i];
        }
    }

    void SpawnPlayer()
    {
        var spawnID = SessionData.Instance.desiredSpawnLocationID;
        var spawn = spawnLocations.Find(x => x.id == spawnID);
        if(spawn == null) { Debug.LogError($"Could not find spawn location {spawnID} in {regionData.regionName}");
            return; }

        var prefab = PersistentConfigurationDataHolder.Instance.prefabData.playerPrefab;
        var player = Instantiate(prefab, spawn.point.position, spawn.point.rotation)
            .GetComponentInChildren<PlayerInstance>().gameObject;

        PersistentConfigurationDataHolder.Instance.sceneLoadInMethod.Invoke(this);
        OnSpawnPlayer?.Invoke(player);
    }
    
    
}