using UnityEngine;
using UnityEngine.Events;

namespace EMILtools.Core {
    
    public abstract class EventListener<T> : MonoBehaviour
    {
        [SerializeField] EventChannel<T> channel;
        [SerializeField] UnityEvent<T> unityEvent;
        
        protected void Awake() => channel.Register(this);
        protected void OnDestroy() => channel.Unregister(this);
        
        public void Raise(T val) => unityEvent?.Invoke(val);
    }
    
    public class EventListener : EventListener<Void> {}
}