using System;
using System.Collections.Generic;

public abstract class EventSystem<T> 
{
    
}

public class EnumEventSystem<TEnum> : EventSystem<Action<TEnum>>
{
    static HashSet<TEnum> eventEnumTypes = new();
    static event Action<TEnum> OnEvent;
    public static void Raise(TEnum @enum) => OnEvent?.Invoke(@enum);
    public static void Add(Action<TEnum> cb, TEnum type)
    {
        if (eventEnumTypes.Contains(type)) return;
        else
        {
            eventEnumTypes.Add(type);
            OnEvent += cb;
        }
    }
    
    public static void Remove(Action<TEnum> cb, TEnum type)
    {
        if (!eventEnumTypes.Contains(type)) return;
        else
        {
            eventEnumTypes.Remove(type);
            OnEvent -= cb;
        }
    }
}

