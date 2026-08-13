using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace EMILtools.Extensions
{
    public static class NavEX
    {

        public static Vector3 ToNearestNavmeshPoint(this Vector3 pos, float range, int areaMask = NavMesh.AllAreas)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(pos, out hit, range, areaMask)) return hit.position;

            Debug.LogWarning("Did not find nav mesh point to teleport to, TPing to original point given");

            return pos;

        }

        public static Vector3 RandomNavMeshPoint(this Transform origin, float range, int areaMask = NavMesh.AllAreas)
        {
            Vector3 randomPos = origin.position + Random.insideUnitSphere * range;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPos, out hit, range, areaMask))
                return ToNearestNavmeshPoint(hit.position, 5);

            return origin.position;
        }

        public static Func<bool> Reached(this NavMeshAgent agent, float timeout, float bufer = 0.1f)
        {
            
            //Initial time is taken ONLY during the initial closure
            float startTime = Time.time;
            
            //This is the only thing being called over and over
            return () =>
            {
                Debug.Log("Attempting to reach");
                if (Time.time - startTime >= timeout) return true;
                if (agent.pathPending) return false;
                if (!agent.hasPath) return false;


                bool isCloseEnough = agent.remainingDistance < agent.stoppingDistance;
                bool isStopped = agent.velocity.sqrMagnitude < 0.02f;

                Debug.Log("close enough : " + isCloseEnough);
                Debug.Log("is stopped : " + isStopped);


                return (isCloseEnough || isStopped);
            };
        }

        public static void Halt(this NavMeshAgent agent, float duration = 0f)
        {
            if (!agent || !agent.isActiveAndEnabled) { agent.EarlyReturn(); return; }

            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            if (duration > 0) _ = DelayUtility.Delay(() =>
            {
                if (agent && agent.isActiveAndEnabled) agent.Continue();
            },
            duration);
            
        }

        public static void Continue(this NavMeshAgent agent)
        {
            if(!agent || !agent.isActiveAndEnabled) return;
            agent.isStopped = false;
        }



        public static void Teleport(Transform tpLoc, GameObject objToTeleport, out bool teleporting)
        {
            teleporting = true; if (!tpLoc || !objToTeleport) return;

            bool foundTpLocOnNavMesh = NavMeshUtility.NearestLocOnNavMesh(tpLoc.position, 5f, out Vector3 tpLocOnNavMesh);
            if (objToTeleport.gameObject.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
            {
                if (foundTpLocOnNavMesh) agent.Warp(tpLocOnNavMesh);
                else
                {
                    agent.enabled = false;
                    objToTeleport.transform.position = tpLoc.position;
                    agent.enabled = true;
                }
            }
            else
                objToTeleport.transform.position = foundTpLocOnNavMesh ? tpLocOnNavMesh : tpLoc.position;

            teleporting = false;
        }

        public static void Teleport(Vector3 tpLoc, GameObject objToTeleport, out bool teleporting)
        {
            teleporting = true; if (!objToTeleport) return;

            bool foundTpLocOnNavMesh = NavMeshUtility.NearestLocOnNavMesh(tpLoc, 5f, out Vector3 tpLocOnNavMesh);
            if (objToTeleport.gameObject.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
            {
                if (foundTpLocOnNavMesh) agent.Warp(tpLocOnNavMesh);
                else
                {
                    agent.enabled = false;
                    objToTeleport.transform.position = tpLoc;
                    agent.enabled = true;
                }
            }
            else
                objToTeleport.transform.position = foundTpLocOnNavMesh ? tpLocOnNavMesh : tpLoc;

            teleporting = false;
        }

        public static IEnumerator C_Disable(this NavMeshAgent agent)
        {
            Vector3 pos = agent.transform.position;
            agent.ResetPath();
            agent.updatePosition = false;
            agent.updateRotation = false;

            yield return null;

            agent.enabled = false;
            agent.transform.position = pos;

        }

    }
}