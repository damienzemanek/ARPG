using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EMILtools.Extensions.NumEX;
using static EMILtools.Extensions.EnumerateEX;

namespace EMILtools.Extensions
{
    public class EnumEX<T> where T : Enum
    {
        public static T Rand()
        {
            var include = Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToArray();

            if (include.Length <= 0) return default;
            return EnumerateEX.Rand(include);
        }

        public static T Rand(params T[] exclude)
        {
            var enumVals = Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToArray();

            var include = enumVals.Where(e => !exclude.Contains(e))
                .ToArray();
                
            if (include.Length <= 0) return default;

            return EnumerateEX.Rand(include);
        }
        public static int Size() => Enum.GetValues(typeof(T)).Length;

    }
}
