using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable, InlineProperty]
public struct WaitSeconds
{
    [SerializeField, HideInInspector] float _time;
    WaitForSeconds wait;

    [ShowInInspector] public float time
    {
        get => _time;
        set
        {
            if (Mathf.Approximately(value, _time)) return;
            _time = value;
            wait = new WaitForSeconds(value);
        }
    }
    public WaitForSeconds Delay => wait ??= new WaitForSeconds(_time);
}


[Serializable, InlineProperty]
public struct WaitSecondsRealtime
{
    [SerializeField, HideInInspector] float _time;
    WaitForSecondsRealtime wait;

    [ShowInInspector] public float time
    {
        get => _time;
        set
        {
            if (Mathf.Approximately(value, _time)) return;
            _time = value;
            wait = new WaitForSecondsRealtime(value);
        }
    }
    public WaitForSecondsRealtime Delay => wait ??= new WaitForSecondsRealtime(_time);
}