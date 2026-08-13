using System;
using Sirenix.OdinInspector;


[InlineProperty]
public sealed class InjectableClass<T> where T : class
{
    [ShowInInspector, ReadOnly, HideLabel] T? value;
    bool reInjectable = false;
    public bool IsInjected { get; private set; }
    public T Value 
        => !IsInjected 
        ? throw new InvalidOperationException($"{typeof(T).Name} has not been injected.") 
        : value!;

    public T Inject(T instance)
    {
        if (IsInjected && !reInjectable) 
            throw new InvalidOperationException($"{typeof(T).Name} has already been injected.");
        value = instance ?? throw new ArgumentNullException(nameof(instance));
        IsInjected = true;
        return instance;
    }

    public static implicit operator T(InjectableClass<T> injectable) => injectable.Value;

    public InjectableClass()
    {
        reInjectable = false;
        IsInjected = false;
        value = null;
    }
}



