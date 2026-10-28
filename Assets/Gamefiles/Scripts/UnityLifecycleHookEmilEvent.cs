using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class UnityLifecycleHookEmilEvent : MonoBehaviour
{
    public bool awake, onenable, ondisable, start, update, fixedupdate;

    [ShowIf("awake")] public EmilEvent OnAwake;
    [ShowIf("onenable")] public EmilEvent OnEnabled;
    [ShowIf("ondisable")] public EmilEvent OnDisabled;
    [ShowIf("start")] public EmilEvent OnStart;
    [ShowIf("update")] public EmilEvent OnUpdate;
    [ShowIf("fixedupdate")] public EmilEvent OnFixedUpdate;

    void Awake()
    {
        if (awake) OnAwake?.Invoke(gameObject);
    }

    void OnEnable()
    {
        if (onenable) OnEnabled?.Invoke(gameObject);
    }
    void OnDisable()
    {
        if (ondisable) OnDisabled?.Invoke(gameObject);
    }

    void Start()
    {
        if (start) OnStart?.Invoke(gameObject);
    }

    void Update()
    {
        if (update) OnUpdate?.Invoke(gameObject);
    }

    void FixedUpdate()
    {
        if (fixedupdate) OnFixedUpdate?.Invoke(gameObject);
    }

}