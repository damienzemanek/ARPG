using UnityEngine;

namespace EMILtools.Extensions
{
    public static class QuaternionEX
    {
        public static Quaternion WithEuler(this Quaternion quaternion, float? x = null, float? y = null, float? z = null)
        {
            var euler = quaternion.eulerAngles;
            return Quaternion.Euler(x ?? euler.x, y ?? euler.y, z ?? euler.z);
        }

        public static Transform SetWorldEuler(this Transform t, float? x = null, float? y = null, float? z = null)
        {
            var euler = t.transform.eulerAngles;
            t.rotation = Quaternion.Euler(x ?? euler.x, y ?? euler.y, z ?? euler.z);
            return t;
        }

        public static Transform SetLocalEuler(this Transform t, float? x = null, float? y = null, float? z = null)
        {
            var euler = t.transform.eulerAngles;
            t.localRotation = Quaternion.Euler(x ?? euler.x, y ?? euler.y, z ?? euler.z);
            return t;
        }
    }
}