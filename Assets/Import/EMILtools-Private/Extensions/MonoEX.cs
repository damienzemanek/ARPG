


using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EMILtools.Extensions
{
    public static class MonoEX
    {
        public static void FadeToOpaque(this MonoBehaviour mono, FadeSettings fade, Object fadeTarget, Action onComplete) 
            => mono.StartCoroutine(FadeEX.C_FadeToOpaque(fade, fadeTarget, onComplete));
    }
}
