using System;
using UnityEngine;

public class EmilEventTest : MonoBehaviour
{
    [Header("1. Auto-fill: GameObject only")]
    public EmilEvent test1_GameObject;

    [Header("2. Auto-fill: GameObject and Bool")]
    public EmilEvent<GameObject, bool> test2_GameObject_Bool;

    [Header("3. Auto-fill: GameObject, Bool, and Float")]
    public EmilEvent<GameObject, bool, float> test3_GameObject_Bool_Bool_Float;

    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        GameObject go = other.gameObject;
        
        // 1. Tests auto-supplying GameObject
        test1_GameObject.Invoke(go);
        
        // 2. Tests auto-supplying GameObject and bool
        test2_GameObject_Bool.Invoke(go, true);
        
        // 3. Tests auto-supplying GameObject, bool, and float
        test3_GameObject_Bool_Bool_Float.Invoke(go, true, 0.5f);
    }
}
