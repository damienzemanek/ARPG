using System;
using UnityEngine;

public class SaverService : Servicer<Type, Saver, SaverService>
{
    public bool Register(Saver saver)
    {
        var type = saver.saveSOType.Type;

        if (type == null)
        {
            Debug.LogError($"{saver.name}: No save type assigned.");
            return false;
        }

        if (services.TryGetValue(type, out var existing))
        {
            if (existing == saver)
                return true;

            Debug.LogWarning(
                $"Saver for {type.Name} already exists: {existing.name}. " +
                $"Rejecting {saver.name}.");

            return false;
        }

        RegisterService(type, saver);
        return true;
    }

    public void Unregister(Saver saver)
    {
        var type = saver.saveSOType.Type;

        if (services.TryGetValue(type, out var existing) &&
            existing == saver)
        {
            UnregisterService(type);
        }
    }

    public Saver GetSaver<T>() where T : SavedDataSO
    {
        return services[typeof(T)];
    }
}