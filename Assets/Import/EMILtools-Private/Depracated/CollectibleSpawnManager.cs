// using UnityEngine;
// using EMILtools.Timers;
// using EMILtools.Signals;
// using static EMILtools.Timers.TimerUtility;

// public class CollectibleSpawnManager : EntitySpawnManager, ITimerUser
// {
//     [SerializeField] CollectibleData[] collectibleData;
//     [SerializeField] float spawnInterval;
//
//     EntitySpawner<Collectible> spawner;
//     
//     public CountdownTimer spawnTimer;
//
//     int counter = 0;
//
//     protected override void InitializationImplementation()
//     {
//
//         spawner = new EntitySpawner<Collectible>(
//             new EntityFactory<Collectible>(collectibleData),
//             spawnPointStrategy);
//
//         spawnTimer = new CountdownTimer(spawnInterval);
//         
//         this.InitTimers((spawnTimer, false))
//             .Sub(spawnTimer.OnTimerStop, HandleTimerStop);    }
//     
//     void OnDestroy() => this.ShutdownTimers();
//     
//     void Start() => spawnTimer.Start();
//     protected override void SpawnImplementation() => spawner.Spawn();
//
//     void HandleTimerStop()
//     {
//         if(counter++ >= spawnPoints.Length) { spawnTimer.Stop(); return;}
//         Spawn();
//         spawnTimer.Start();
//     }
//     
//     
// }