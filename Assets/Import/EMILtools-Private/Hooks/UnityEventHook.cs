using UnityEngine;
using UnityEngine.Events;

public class UnityEventHook : MonoBehaviour
{
    public UnityEvent _event;

    public void Invoke() => _event?.Invoke();
}
