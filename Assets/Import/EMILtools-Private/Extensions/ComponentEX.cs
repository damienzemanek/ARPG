using System;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;


namespace EMILtools.Extensions
{
    public static class ComponentEX
    {
        public static T Get<T>(this Object obj) where T : Component
        {
            string thisType = typeof(T).Name;

            switch (obj)
            {
                case Component comp: return TryOnComponent(comp);
                case GameObject go: return TryOnGameObject(go);
                default: return (T)CannotTryGet(obj);
            }

            T TryOnComponent(Component comp)
            {
                if (comp.TryGetComponent<T>(out T found)) return found;
                obj.Error($"Failed to TryGet<{thisType}> on {found}");
                return null;
            }

            T TryOnGameObject(GameObject go)
            {
                if (go.TryGetComponent<T>(out T found)) return found;
                go.Error($"Failed to TryGet<{thisType}> on {found}");
                return null;
            }

            object CannotTryGet(object obj)
            {
                obj.Error($"Failed to TryGet<{thisType}> on {obj}");
                return null;
            }
        }

        public static T OptionalGet<T>(this Object obj) where T : Component
        {
            string thisType = typeof(T).Name;

            switch (obj)
            {
                case Component comp: return TryOnComponent(comp);
                case GameObject go: return TryOnGameObject(go);
                default: return (T)CannotTryGet(obj);
            }

            T TryOnComponent(Component comp)
            {
                if (comp.TryGetComponent<T>(out T found)) return found;
                return null;
            }

            T TryOnGameObject(GameObject go)
            {
                if (go.TryGetComponent<T>(out T found)) return found;
                return null;
            }

            object CannotTryGet(object obj)
            {
                return null;
            }
        }

        public static TComponent TryGetOrAdd<TComponent>(this Object obj) where TComponent : Component
        {
            if (obj is GameObject go)
                return go.TryGetComponent<TComponent>(out TComponent c)
                    ? c : go.AddComponent<TComponent>();

            if(obj is Component comp)
                return comp.TryGetComponent<TComponent>(out TComponent c2)
                    ? c2 : comp.gameObject.AddComponent<TComponent>();

            return null;
        }

        public static TComponent TryGetOrAdd<TComponent>(this Object obj, out TComponent output) where TComponent : Component
        {
            output = null;
            if (obj is GameObject go)
                return output = go.TryGetComponent<TComponent>(out TComponent c) 
                    ? c : go.AddComponent<TComponent>();

            if (obj is Component comp)
                return output = comp.TryGetComponent<TComponent>(out TComponent c2)
                    ? c2 : comp.gameObject.AddComponent<TComponent>();
            return null;
        }

        public static bool Has<TComponent>(this Object obj, out TComponent result) where TComponent : Component
        {
            result = null;

            if (obj is GameObject go)
                if (go.TryGetComponent(out result)) return true;
            
            if(obj is Component comp)
                if (comp.TryGetComponent(out result)) return true;

            return false;
        }

        public static bool Has<TComponent>(this Object obj) where TComponent : Component
        {
            if (obj is GameObject go)
                if (go.GetComponent<TComponent>()) return true;

            if (obj is Component comp)
                if (comp.GetComponent<TComponent>()) return true;

            return false;
        }

        public static void Ensure(this Object obj, MonoBehaviour mono)
        {
            if (obj == null) mono.Error("Did not assign reference, or is null");
        }

    }

}