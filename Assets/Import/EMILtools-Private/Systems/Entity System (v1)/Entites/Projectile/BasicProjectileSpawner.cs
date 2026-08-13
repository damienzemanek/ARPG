using UnityEngine;

public class BasicProjectileSpawner : MonoBehaviour
{
    public ProjectileSpawnManager projSpawner;

    void Update()
    {
        projSpawner.Spawn();
    }
}