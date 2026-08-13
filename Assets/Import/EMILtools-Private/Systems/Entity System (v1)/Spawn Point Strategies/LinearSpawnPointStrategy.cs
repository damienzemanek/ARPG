using UnityEngine;

public class LinearSpawnPointStrategy : ISpawnPointStrategy
{
    int indx = 0;
    
    Transform[] spawnPoints;


    public LinearSpawnPointStrategy(Transform[] _spawnPoints)
    {
        spawnPoints = _spawnPoints;
    }

    public Transform NextSpawnPoint()
    {
        Transform result = spawnPoints[indx];
        indx = (indx + 1) % spawnPoints.Length;
        return result;
    }
    
    
}