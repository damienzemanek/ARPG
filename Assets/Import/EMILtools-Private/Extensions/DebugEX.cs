using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EMILtools.Extensions
{
    public static class DebugEX
    {
        const string NullColor = "#AAAAAA";
        const string ScriptColor = "#00FF00";   // Green
        const string GameObjectColor = "#FFA500"; // Orange

        static string Colorize(string text, string colorHex) => $"<color={colorHex}>{text}</color>";
        static string Bold(string text) => $"<b>{text}</b>";

        public static void PrintList<T>(this List<T> list) where T : class
        {
            if (list == null) { list.Log("List is <null>"); return; }

            string contents;

            // If the list items are Unity objects, print their .name
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                contents = string.Join(", ", list
                    .Cast<UnityEngine.Object>()
                    .Select(o => o != null ? o.name : "<null>"));
            }
            else
            {
                // Regular object list -> print normally
                contents = string.Join(", ", list);
            }

            string count = $"[{list.Count} VALUES]";
            list.Log($"{contents} {count}");
        }

        public static void PrintArray<T>(this T[] array)
        {
            if(array == null) { array.Log("Array is null"); return; }

            if (typeof(Object).IsAssignableFrom(typeof(T))){

            }
        }

        /// <summary>
        /// Prints members of a class. Use syntax (x => x.value) to select the member wanted
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="list"></param>
        /// <param name="selector"></param>
        public static void PrintList<T, TOut>(this List<T> list, Func<T, TOut> selector) where T : class
        {
            if (list == null)
            {
                list.Log("List is <null>");
                return;
            }

            var values = list.Select(selector).ToList();

            string contents = string.Join(", ", values);
            string count = $"[{values.Count} VALUES]";

            list.Log($"{contents} {count}");
        }




        public static void Log(this object obj, string msg = "")
        {
            if (obj == null)
            {
                Debug.Log($"{Colorize("[<null>]", NullColor)}: {msg}");
                return;
            }

            if (obj is Object unityObj)
                Debug.Log($"{Colorize($"[SCRIPT: {Bold(unityObj.GetType().Name)}]", ScriptColor)} " +
                          $"{Colorize($"[G.O.: {Bold(unityObj.name)}]", GameObjectColor)}: {msg}");
            else
                Debug.Log($"{Colorize($"[SCRIPT: {Bold(obj.GetType().Name)}]", ScriptColor)}: {msg}");
        }

        public static void Warn(this object obj, string msg = "")
        {
            if (obj == null)
            {
                Debug.LogWarning($"{Colorize("[<null>]", NullColor)}: {msg}");
                return;
            }

            if (obj is Object unityObj)
                Debug.LogWarning($"{Colorize($"[SCRIPT: {Bold(unityObj.GetType().Name)}]", ScriptColor)} " +
                                 $"{Colorize($"[G.O.: {Bold(unityObj.name)}]", GameObjectColor)}: {msg}");
            else
                Debug.LogWarning($"{Colorize($"[SCRIPT: {Bold(obj.GetType().Name)}]", ScriptColor)}: {msg}");
        }

        public static void EarlyReturn(this object obj)
        {
            obj.Warn("EARLY RETURN");
        }
        public static void Error(this object obj, string msg = "")
        {
            if (obj == null)
            {
                Debug.LogError($"{Colorize("[<null>]", NullColor)}: {msg}");
                return;
            }

            if (obj is Object unityObj)
                Debug.LogError($"{Colorize($"[SCRIPT: {Bold(unityObj.GetType().Name)}]", ScriptColor)} " +
                               $"{Colorize($"[G.O.: {Bold(unityObj.name)}]", GameObjectColor)}: {msg}");
            else
                Debug.LogError($"{Colorize($"[SCRIPT: {Bold(obj.GetType().Name)}]", ScriptColor)}: {msg}");
        }

        public static void SetCheck(this object obj)
        {
            if (obj == null)
                throw new System.ArgumentNullException($"{Colorize("[<null>]", NullColor)}: not set in inspector � please set");

            if (obj is Object unityObj)
                Debug.LogError($"{Colorize($"[SCRIPT: {Bold(unityObj.GetType().Name)}]", ScriptColor)} " +
                               $"{Colorize($"[G.O.: {Bold(unityObj.name)}]", GameObjectColor)}: field not assigned in inspector");
            else
                Debug.LogError($"{Colorize($"[SCRIPT: {Bold(obj.GetType().Name)}]", ScriptColor)}: field not assigned in inspector");
        }
    }
}