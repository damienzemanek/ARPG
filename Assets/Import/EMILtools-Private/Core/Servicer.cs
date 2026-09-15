using System.Collections.Generic;
using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using UnityEngine;

public abstract class Servicer<TKey, TService, TSelf> : PersistantReplacerSingleton<TSelf>
    where TSelf : Component
{
    protected readonly Dictionary<TKey, TService> services = new();

    public bool TryGetService(TKey key, out TService service)
    {
        return services.TryGetValue(key, out service);
    }

    public void RegisterService(TKey key, TService service)
    {
        services[key] = service;
    }

    public bool UnregisterService(TKey key)
    {
        return services.Remove(key);
    }
}