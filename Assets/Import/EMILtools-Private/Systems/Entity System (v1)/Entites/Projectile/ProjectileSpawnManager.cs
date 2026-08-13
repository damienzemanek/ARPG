using System;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Timers.TimerUtility;


[Serializable]
public class ProjectileSpawnManager : EntitySpawnManager, ITimerUser
{
    public ProjectileData[] data = new ProjectileData[1];
    public ClassRef<float> fireInterval = 1f;
    EntitySpawner<Projectile> projSpawner;
    [SerializeField] public CountdownTimer fireTimer;
    [SerializeField] bool targetSetDirection = false;
    [SerializeField] [ShowIf("targetSetDirection")] public Vector3 direction;
    [SerializeField] bool targetAPosition = false;
    [SerializeField] [ShowIf("targetAPosition")] public Vector3 targetPosition;

    [NonSerialized] public PersistentAction OnSpawn;

    protected override void InitializationImplementation()
    {
        projSpawner = new EntitySpawner<Projectile>(
            new EntityFactory<Projectile>(data),
            spawnPointStrategy);

        fireTimer = new CountdownTimer(fireInterval);
        this.InitTimers((fireTimer, true));
        
        Debug.Log("Projectile Spawner Initialization Complete");
    }

    /// <summary>
    /// Will automatically go proj.transform.fwd unless specifed
    /// </summary>
    protected override void SpawnImplementation()
    {
        if (fireTimer.isRunning) return;
        fireTimer.StartAndReset();
        Projectile proj = projSpawner.Spawn().Initalize(data[0]);
        Vector3 launchDir = proj.transform.forward;
        if(targetSetDirection) launchDir = proj.transform.TransformDirection(direction.normalized);
        if (targetAPosition)
        {
            launchDir = (targetPosition - proj.transform.position).normalized;
            proj.transform.SetPositionAndRotation(proj.transform.position, Quaternion.LookRotation(launchDir));
        }
        proj.rb.AddForce(launchDir * data[0].forceScalar, data[0].forceMode);
        OnSpawn?.Invoke();
    }
}
