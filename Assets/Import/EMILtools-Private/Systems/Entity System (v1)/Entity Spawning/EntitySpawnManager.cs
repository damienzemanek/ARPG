using System;
using UnityEngine;

[Serializable]
public abstract class EntitySpawnManager
{
    [SerializeField] bool initialized = false;
    
    protected enum SpawnPointStrategyType { Linear, Random }
    
    [SerializeField] protected SpawnPointStrategyType spawnPointStrategyType = SpawnPointStrategyType.Linear;
    [SerializeField] protected Transform[] spawnPoints;

    protected ISpawnPointStrategy spawnPointStrategy;

    void Initialize()
    {
        initialized = true;
        spawnPointStrategy = spawnPointStrategyType switch
        {
            SpawnPointStrategyType.Linear => new LinearSpawnPointStrategy(spawnPoints),
            SpawnPointStrategyType.Random => new RandomSpawnPointStrategy(spawnPoints),
            _ => spawnPointStrategy
        };
        
        Debug.Log("Base Initialization Complete");
        InitializationImplementation();
    }

    public void Spawn()
    {
        if (!initialized) Initialize();
        SpawnImplementation();
    }

    protected abstract void InitializationImplementation();
    protected abstract void SpawnImplementation();
}