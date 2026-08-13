using UnityEngine;
using System;
using System.Collections;

namespace EMILtools.Extensions
{
    public static class CoroutineEX
    {

        #region Privates

        #endregion

        public static void DelayedCall(this MonoBehaviour mono, Action method, float delay, bool preStopAllCoroutines = false)
        {
            if(!mono || method == null) return;
            if (!mono.enabled) return;
            if (preStopAllCoroutines) mono.StopAllCoroutines();
            mono.StartCoroutine(C_CallAfterDelay(method, delay));
        }


        public static IEnumerator C_CallAfterDelay(Action method, float delay)
        {
            yield return new WaitForSeconds(delay);
            method?.Invoke();
        }

        #region Methods

        #endregion

    }
}
