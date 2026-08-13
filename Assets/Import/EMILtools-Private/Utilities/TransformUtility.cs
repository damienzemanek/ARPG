using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public static class TransformUtility 
{
    public static void Slerp(this Transform myTransform, Vector3 to, float duration, MonoBehaviour mono, Action endHook = null)
    {
        if(mono.isActiveAndEnabled)
            mono?.StartCoroutine(C_Slerp(myTransform, to, duration, endHook));

    }

    public static IEnumerator C_Slerp(this Transform myTransform, Vector3 to, float duration, Action endHook = null)
    {
        Vector3 start = myTransform.position;
        Vector3 end = to;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            myTransform.position = Vector3.Slerp(start, end, t);
            yield return null;
        }

        myTransform.position = end;
        endHook?.Invoke();
    }

    public static void Lerp(this Transform myTransform, Vector3 to, float duration, MonoBehaviour mono, Action endHook = null, bool local = false)
    {
        if(mono.isActiveAndEnabled)
            mono?.StartCoroutine(C_Lerp(myTransform, to, duration, endHook, local));
    }
    public static IEnumerator C_Lerp(this Transform myTransform, Vector3 to, float duration, Action endHook = null, bool local = false)
    {
        Vector3 start = new Vector3();
        if (!local) start = myTransform.position;
        else start = myTransform.localPosition;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            if(!local) myTransform.position = Vector3.Lerp(start, to, t);
            else myTransform.localPosition = Vector3.Lerp(start, to, t);
            yield return null;
        }

        if (local)
            myTransform.localPosition = to;
        else
            myTransform.position = to;

        endHook?.Invoke();
    }

    public static void LerpScale(this Transform myTransform, Vector3 to, float duration, MonoBehaviour mono, Action endHook = null)
    {
        if(mono.isActiveAndEnabled)
            mono?.StartCoroutine(C_LerpScale(myTransform, to, duration, endHook));
    }

    public static IEnumerator C_LerpScale(this Transform myTransform, Vector3 to, float duration, Action endHook = null)
    {
        Vector3 start = myTransform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            myTransform.localScale = Vector3.Lerp(start, to, t);
            yield return null;
        }

        myTransform.localScale = to;
        endHook?.Invoke();
    }

    public static void LerpRot(this Transform myTransform, Quaternion to, float duration, MonoBehaviour mono, Action endHook = null, bool local = false)
    {
        if (mono != null && mono.isActiveAndEnabled)
            mono.StartCoroutine(C_LerpRot(myTransform, to, duration, endHook, local));
    }

    public static IEnumerator C_LerpRot(this Transform myTransform, Quaternion to, float duration, Action endHook = null, bool local = false)
    {
        Quaternion start = local ? myTransform.localRotation : myTransform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            if (local)
                myTransform.localRotation = Quaternion.Lerp(start, to, t);
            else
                myTransform.rotation = Quaternion.Lerp(start, to, t);

            yield return null;
        }

        if (local)
            myTransform.localRotation = to;
        else
            myTransform.rotation = to;

        endHook?.Invoke();
    }
}
