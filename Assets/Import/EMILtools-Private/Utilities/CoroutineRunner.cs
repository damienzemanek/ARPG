using System;
using System.Collections;
using System.Collections.Generic;
using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using UnityEngine;
    
[DefaultExecutionOrder(-99)]
public class CoroutineRunner : PersistantReplacerSingleton<CoroutineRunner>
{
    public void RunMethodDelayed(Action action, float delay) 
        => StartCoroutine(RunMethodDelayedCoroutine(action, delay));

    IEnumerator RunMethodDelayedCoroutine(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
    
    public IEnumerator RunAllMethodsAtOnce(params IEnumerator[] methods)
    {
        int remaining = methods.Length;

        if (remaining == 0) yield break;

        foreach (IEnumerator method in methods)
            StartCoroutine(RunAndTrackCompletion(method, () => remaining--));
        
        yield return new WaitUntil(() => remaining <= 0);
    }

    IEnumerator RunAndTrackCompletion(IEnumerator method, Action onComplete)
    {
        yield return StartCoroutine(method);
        onComplete?.Invoke();
    }
}
