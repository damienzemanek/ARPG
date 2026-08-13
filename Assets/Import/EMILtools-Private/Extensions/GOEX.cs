using System.Linq;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class GOEX
    {
        public static GameObject SetActiveThen(this GameObject gameObject, bool val)
        {
            gameObject.SetActive(val);
            return gameObject;
        }

        public static void FillWithChildren(this GameObject[] array, GameObject parent)
        {
            if (parent == null) return;

            var children = parent.transform.Cast<Transform>()
                .Select(t => t.gameObject)
                .Take(array.Length)
                .ToArray();

            for (int i = 0; i < children.Length; i++)
                array[i] = children[i];
        }

        public static GameObject[] Children(this Transform parent)
        {
            var ret = System.Array.Empty<GameObject>();
            if (parent == null || parent.childCount == 0) return ret;
            return parent.Cast<Transform>()
                .Select(t => t.gameObject)
                .ToArray();
        }
        
        public static void DestroyAll(this GameObject[] gameObjects)
        {
            if (gameObjects == null) return;

            foreach (var go in gameObjects)
            {
                if (go == null) continue;

#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                    Object.Destroy(go);
#if UNITY_EDITOR
                else
                    Object.DestroyImmediate(go);
#endif
            }
        }
        
    }

}