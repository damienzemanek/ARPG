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
}
