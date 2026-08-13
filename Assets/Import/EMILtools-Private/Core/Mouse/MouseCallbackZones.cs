using System;
using System.Collections.Generic;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;


namespace EMILtools.Core
{

    [Serializable]
    public struct RectLabeled<TEnum>
        where TEnum : struct, Enum
    {
        public TEnum label;
        public Rect rect;
    }

    public static class RectEX<TEnum> where TEnum : struct, Enum
    {
        public static TEnum CheckIfAnyContains(RectLabeled<TEnum>[] zones, Vector2 mousePos)
        {
            foreach (var zone in zones) { if (zone.rect.Contains(mousePos)) return zone.label; }
            //Debug.LogWarning("No zone contains mouse position");
            return default;
        }
    }
    
    
    
    
    [Serializable]
    public class MousePosCallbackZone<TEnum>
        where TEnum : struct, Enum
    {
        readonly RectLabeled<TEnum> zone;
        [ShowInInspector, ReadOnly] bool wasInside;
        [NonSerialized] PersistentAction callback;
    
        public void CheckZone(Vector2 mousePos)
        {
            var inside = zone.rect.Contains(mousePos);
            if (inside && !wasInside) callback.Invoke();
            wasInside = inside;
        }

        public MousePosCallbackZone(RectLabeled<TEnum> zone, PersistentAction callback)
        { 
            this.zone = zone;
            wasInside = false;
            this.callback = callback;
        }
    }

    public static class MouseCallbackZoneExtensions<TEnum>
        where TEnum : struct, Enum
    {

        
        public static void CheckAllZones(MousePosCallbackZone<TEnum>[] zones, Vector2 mousePos) 
        {
            foreach (var zone in zones) zone.CheckZone(mousePos);
        }

        public static MousePosCallbackZone<TEnum>[] RetrieveAndInitZones(
            RectLabeled<TEnum>[] zones,
            params (PersistentAction cb, TEnum label)[] callbacks)
        {
            if (zones.Length != callbacks.Length)
                Debug.LogError("Zones length does not match callback length");

            // Build lookup from callbacks
            var map = new Dictionary<TEnum, PersistentAction>(callbacks.Length);
            for (int i = 0; i < callbacks.Length; i++)
            {
                if (map.ContainsKey(callbacks[i].label)) { Debug.LogError($"Duplicate callback label: {callbacks[i].label}");
                    continue; }

                map.Add(callbacks[i].label, callbacks[i].cb);
            }

            var result = new MousePosCallbackZone<TEnum>[zones.Length];

            // Init Zones
            for (int i = 0; i < zones.Length; i++)
            {
                var label = zones[i].label;

                if (!map.TryGetValue(label, out var cb)) { Debug.LogError($"Missing callback for zone label: {label}");
                    continue; }

                result[i] = new MousePosCallbackZone<TEnum>(zones[i], cb);
            }

            return result;
        }
    }
}
