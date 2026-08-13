using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class UnityLifecycleHook : MonoBehaviour
{
    public bool awake, onenable, ondisable, start, update, fixedupdate;

    [ShowIf("awake")] public UnityEvent OnAwake;
    [ShowIf("onenable")] public UnityEvent OnEnabled;
    [ShowIf("ondisable")] public UnityEvent OnDisabled;
    [ShowIf("start")] public UnityEvent OnStart;
    [ShowIf("update")] public UnityEvent OnUpdate;
    [ShowIf("fixedupdate")] public UnityEvent OnFixedUpdate;

    private void Awake()
    {
        if (awake) OnAwake?.Invoke();
    }

    private void OnEnable()
    {
        if (onenable) OnEnabled?.Invoke();
    }
    private void OnDisable()
    {
        if (ondisable) OnDisabled?.Invoke();
    }

    private void Start()
    {
        if (start) OnStart?.Invoke();
    }

    private void Update()
    {
        if (update) OnUpdate?.Invoke();
    }

    private void FixedUpdate()
    {
        if (fixedupdate) OnFixedUpdate?.Invoke();
    }

}