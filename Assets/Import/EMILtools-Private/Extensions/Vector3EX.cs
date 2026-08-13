using UnityEngine;

namespace EMILtools.Extensions
{
    public static class Vector3EX
    {
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
            => new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);

        public static Vector3 WithScale(this Vector3 v, Vector3 scale)
            => Vector3.Scale(v, scale);

        public static bool IsAnyGreaterThan(this Vector3 v, float? x = null, float? y = null, float? z = null)
        {
            if (x.HasValue && v.x > x) return true;
            if (y.HasValue && v.y > y) return true;
            if (z.HasValue && v.z > z) return true;
            return false;
        }
        
    }
}