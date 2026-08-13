using System;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class NumEX
    {
        public const float ZeroF = 0f;
        public static readonly Index LastIndex = ^1;
        private const float NinetyF = 90f;

        /// <summary>
        /// Ensure Tolerance is between 0 and 1
        /// </summary>
        /// <param name="set"></param>
        /// <param name="compare"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static float ToleranceSet(float value, float target, float tolerance)
        {
            tolerance = Mathf.Clamp01(tolerance);
            float range = Mathf.Abs(target) * tolerance;
            return Mathf.Abs(value - target) <= range ? target : value;
        }

        /// <summary>
        /// Flips 0 to 1 and 1 to 0
        /// or 0.25 to 0.75
        /// or 0.5 to 0.5
        /// or 0.75 to 0.25
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float Flip01(float val)
        => Mathf.Abs(val - 1f);
        
        
        public static float Scaled(
            this float value,
            Vector2 oldRange,
            Vector2 newRange)
        {
            float currLerpPos = Mathf.InverseLerp(oldRange.x, oldRange.y, value);
            
            float scale = Mathf.Lerp(newRange.x, newRange.y, currLerpPos);
            float newLerpPos = Mathf.Clamp01(currLerpPos * scale);
            
            // Re-Map
            return Mathf.Lerp(newRange.x, newRange.y, newLerpPos);
        }
        
        public static float Scaled(
            this float value,
            (float min, float max) oldRange,
            (float min, float max) newRange)
        {
            float currLerpPos = Mathf.InverseLerp(oldRange.min, oldRange.max, value);
            
            float scale = Mathf.Lerp(newRange.min, newRange.max, currLerpPos);
            float newLerpPos = Mathf.Clamp01(currLerpPos * scale);
            
            // Re-Map
            return Mathf.Lerp(newRange.min, newRange.max, newLerpPos);
        }
        
        
        public static float ProgressWhenApproaching(
            this float value,
            float start,
            float target,
            bool increasing // true = going up, false = going down
        )
        {
            if (Mathf.Approximately(start, target))
                return 1f;

            float t;

            if (increasing)
            {
                // value moves from start → target (ascending)
                t = (value - start) / (target - start);
            }
            else
            {
                // value moves from start → target (descending)
                t = (start - value) / (start - target);
            }

            return Mathf.Clamp01(t);
        }
        
        
    }
}