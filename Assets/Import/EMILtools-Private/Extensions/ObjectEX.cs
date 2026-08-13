using System;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class ObjectEX
    {
        //doenst work atm
        public static void NullCheck(this object script, object input)
        {
            if (input == null) script.Error($"Null Check FAILED. {input} resulted in NULL value");

        }
        
        //First check (t == null) tests if the object is null
        //Second check (t is UnityEngine.Object obj && !obj) tests if the object is a Unity object and is not null
        public static bool IsUnityNull<T>(this T t) => t == null || (t is UnityEngine.Object obj && !obj);    }
}