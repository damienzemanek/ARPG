using System;
using System.Collections.Generic;
using UnityEngine;

public static class InterfaceEX
{
   
    
    public static IEnumerable<Type> GetInterfacesAssignableTo<TBaseInterface>(Type concreteType)
        where TBaseInterface : class
    {
        var baseType = typeof(TBaseInterface);

        if (!baseType.IsInterface)
            throw new ArgumentException($"{baseType.Name} must be an interface.");

        foreach (var iface in concreteType.GetInterfaces())
        {
            if (baseType.IsAssignableFrom(iface) && iface != baseType)
            {
                yield return iface;
            }
        }
    }
}
