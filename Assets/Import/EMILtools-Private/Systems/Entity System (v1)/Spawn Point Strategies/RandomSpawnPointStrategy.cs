using System.Collections.Generic;
using EMILtools.Extensions;
using UnityEngine;

public class RandomSpawnPointStrategy : ISpawnPointStrategy
{
    List<Transform> unusedSpawnPoints;
    Transform[] spawnPoints;

    public RandomSpawnPointStrategy(Transform[] _spawnPoints)
    {
        spawnPoints = _spawnPoints;
        unusedSpawnPoints = new List<Transform>(spawnPoints);
    }
    
    public Transform NextSpawnPoint()
    {
        if(spawnPoints.Length == 0) { this.Error("No spawn points set"); return null; }
        
        if (unusedSpawnPoints.IsEmpty()) unusedSpawnPoints.AddRange(spawnPoints);
        return unusedSpawnPoints.RandAndRemove();
    }
}