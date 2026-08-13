using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EMILtools;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class CwtEX
    {
        public static void GetValues<T, T2>(this ConditionalWeakTable<T2, HashSet<T>> cwt, List<T2> accessValues,
            out HashSet<T> returnValues) where T2 : class
        {
            returnValues = new HashSet<T>();
            foreach (T2 access in accessValues)
            {
                if (!cwt.TryGetValue(access, out HashSet<T> newReturn)) continue;
                returnValues.UnionWith(newReturn);
            }
        }
    }
}