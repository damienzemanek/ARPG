using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class Vector2EX
    {
        public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
            => new Vector2(x ?? vector.x, y ?? vector.y);

        public static Vector2 WithScale(this Vector2 v, Vector2 scale)
            => Vector2.Scale(v, scale);

        public static bool IsAnyGreaterThan(this Vector2 v, float? x = null, float? y = null)
        {
            if (x.HasValue && v.x > x) return true;
            if (y.HasValue && v.y > y) return true;
            return false;
        }
        
        public static Vector2 GetRandomPointOnImage(this RectTransform rect)
        {
            Vector2 size = rect.rect.size;
            size = size / 2;
            float x = Random.Range(-size.x, size.x);
            float y = Random.Range(-size.y, size.y);
            return rect.TransformPoint(new Vector3(x, y, 0f));
        }
    }

}