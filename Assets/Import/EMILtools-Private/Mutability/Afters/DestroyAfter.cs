using UnityEngine;

/// <summary>
/// Simple alternative to object pooling
/// For Quick Prototyping
/// </summary>
public class DestroyAfter : MonoBehaviour
{
    public bool destroyOnStart = true;
    public float time;
    void Start()
    {
        if (destroyOnStart) Destroy();
    }
    
    public void Destroy() => Destroy(gameObject, time);
}
