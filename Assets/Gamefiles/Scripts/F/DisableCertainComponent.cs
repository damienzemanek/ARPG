using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "DisableCertainComponent", menuName = "ARPG/SO/Method/DisableCertainComponent")]
public class DisableCertainComponent : SO_Method<GameObject>
{
    [SerializeField, ReadOnly] public TypeSerializedCore componentToDisable;
    
    [Button]
    public void AssignType( [ValueDropdown(nameof(GetComponentTypes))] Type type) => componentToDisable = new TypeSerializedCore(type);

    public override void Invoke(GameObject targ)
    {
        if (componentToDisable == null) return;
        if (!targ.TryGetComponent(componentToDisable.SystemType, out Component component)) return;
        switch (component)
        {
            case Behaviour b: b.enabled = false; break;
            case Renderer r: r.enabled = false; break;
            case Collider c: c.enabled = false; break;
            default: Debug.LogWarning($"{component.GetType().Name} cannot be disabled."); break;
        }
    }

    static IEnumerable<Type> GetComponentTypes()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
            })
            .Where(t =>
                t != null &&
                typeof(Component).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsGenericType);
    }
}


