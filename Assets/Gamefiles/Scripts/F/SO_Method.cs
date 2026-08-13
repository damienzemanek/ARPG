using UnityEngine;


public abstract class SO_Method<T> : ScriptableObject
{
    public abstract void Invoke(T targ);
}

public abstract class SO_Method_Preset : ScriptableObject
{
    public abstract void Invoke();
}