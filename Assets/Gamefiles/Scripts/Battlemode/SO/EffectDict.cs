using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;
using UnityEngine;

// Singleton In name only for now
[CreateAssetMenu(fileName = "EffectIconDict", menuName = "ARPG/SO/EffectIconDict")]
public class EffectDict : ScriptableObject
{
    [Serializable]
    public struct EffectData
    {
        public Sprite icon;
        public string name;
        public string description;
    }


    [DrawWithUnity]
    public SerializedDictionary<string, EffectData> data = new SerializedDictionary<string, EffectData>();
    
    [Button]
    public void Initialize()
    {
        IEnumerable<Type> effectTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                typeof(BattlemodeEffectStrategy).IsAssignableFrom(type));

        foreach (Type type in effectTypes)
        {
            if (data.ContainsKey(type.Name)) continue;
            AddEffect(type);
        }
    }

    public void AddEffect(Type type)
    {
        data.Add(type.Name, new EffectData
        {
            icon = null,
            name = type.Name,
            description = "No description"
        });
    }
}