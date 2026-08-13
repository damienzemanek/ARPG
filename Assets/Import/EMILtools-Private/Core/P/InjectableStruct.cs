using System;

public struct InjectableStruct<T> where T : struct
{
    internal T value;
    public bool IsInjected { get; internal set; }
    public readonly T Value 
        => !IsInjected 
        ? throw new InvalidOperationException($"{typeof(T).Name} has not been injected.") 
        : value;
    
    public static implicit operator T(InjectableStruct<T> injectable) => injectable.Value;
}

public static class InjectableStructExtensions
{
    public static void Inject<T>(this ref InjectableStruct<T> injectable, T instance)
        where T : struct
    {
        if (injectable.IsInjected) throw new InvalidOperationException($"{typeof(T).Name} has already been injected.");
        injectable = new InjectableStruct<T>
        {
            value = instance,
            IsInjected = true
        };
    }
}