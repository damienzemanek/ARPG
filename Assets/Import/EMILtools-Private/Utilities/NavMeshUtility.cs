using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtility
{
    public static bool NearestLocOnNavMesh(Vector3 targ, float searchDist, out Vector3 result)
    {
        if(NavMesh.SamplePosition(targ, out NavMeshHit hit, searchDist, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = targ;
        return false;
    }
}
