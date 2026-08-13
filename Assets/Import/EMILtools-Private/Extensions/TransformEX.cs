using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class TransformEX
    {
        public static Transform LookAtPosThenMyTransform(this Transform transform, Vector3 pos)
        {
            transform.LookAt(pos);
            return transform;
        }

        public static Transform WithEuler(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            transform.eulerAngles = new Vector3(
                x ?? transform.eulerAngles.x,
                y ?? transform.eulerAngles.y,
                z ?? transform.eulerAngles.z);
            return transform;
        }

        public static Transform GetClosest(this Transform transform, Transform[] list)
        {
            Transform closest = list[0];
            float closestDist = Vector3.Distance(transform.position, list[0].position);

            foreach (var item in list)
            {
                float dist = Vector3.Distance(transform.position, item.position);
                if (dist > closestDist) continue;

                closest = item;
                closestDist = dist;
            }

            return closest;
        }

        public static Transform GetClosest(this Transform transform, List<Transform> list)
        {
            Transform closest = list[0];
            float closestDist = Vector3.Distance(transform.position, list[0].position);

            foreach (var item in list)
            {
                float dist = Vector3.Distance(transform.position, item.position);
                if (dist > closestDist) continue;

                closest = item;
                closestDist = dist;
            }

            return closest;
        }
    }
}